using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Molat : MonoBehaviour, IDamageable
{

	[Header("Stats")]
	[SerializeField] float maxHealth = 100f;
	[SerializeField] float maxStamina = 100f;
	[SerializeField] float stamRechargePS = 20f;
	[SerializeField] float Power = 0.2f;

	[Header("Movement")]
	[SerializeField] float speed = 6f;
	[SerializeField] float straifSpeed = 5f;
	[SerializeField] float sprintSpeed = 8f;
	[Range(0f, 180f)] [SerializeField] float fullSpeedMaxDegrees = 80f;
	[SerializeField] float maxVelocityChange = 10.0f;
	[SerializeField] float climbSpeed = 8f;

	[Header("damage and bleed")]
	[SerializeField] float clawDamage = 10f;
	[SerializeField] float bleedBluntPercent = 0f;
	[SerializeField] float bleedSpikedPercent = 0.2f;
	[SerializeField] float bleedSharpPercent = 0.5f;
	[SerializeField] float attackLength = 1.2f;

	[Header("Abilities")]
	[SerializeField] bool canJump = true;
	[SerializeField] bool canAttack = true;
	[SerializeField] bool canSprint = true;
	[SerializeField] bool canClimb = false;

	[Header("Jumping")]
	[SerializeField] float sprintCostPS = 5f;
	[SerializeField] float climbCostPS = 5f;
	[SerializeField] float tailJumpCost = 5f;
	[SerializeField] float tailJumpHeight = 3.0f;
	[SerializeField] float tailJumpPower = 1000.0f;
	[SerializeField] float jumpCost = 10f;
	[SerializeField] float jumpHeight = 13.0f;
	[SerializeField] float jumpPower = 2.0f;
	[SerializeField] LayerMask mask;
	[SerializeField] float downCastRange = 1.2f;

	[Header("Other")]
	[SerializeField] float gravity = 18;
	[SerializeField] float animDampTime = 2;
	[SerializeField] float runAnimationRatio = 0.3f;
	[SerializeField] float damageWithMaceThrow = 9.0f;
	public GameObject throwingWeapon;
	[SerializeField] GameObject projectileSocket;
	[SerializeField] Weapon myWeapon;


	private float currentStamRechargePS;
	private float currentSpeed;
	private bool currentCanSprint;
	private bool isJumping;
	private bool isSprinting;
	private bool isClimbing;
	private float currentHealth;
	private float currentStamina;
	private bool canAttackCurrent;
	private float drag;
	private bool lastTailJumpToggle = false;
	private bool lastJumpToggle = false;
	private float bleed = 0f;

	private bool weaponEquiped = false;
	private bool changingWeapon = false;
	//[SerializeField] Transform target;


	[Header("Body")]
	[SerializeField] AudioSource myaudiosource;
	[SerializeField] Transform chest;
	[SerializeField] Transform shield;
	[SerializeField] Transform weapon;
	[SerializeField] Transform lefthandpos;
	[SerializeField] Transform righthandpos;
	[SerializeField] Transform chestposshield;
	[SerializeField] Transform chestposweapon;
	[SerializeField] AudioClip jumpclip1;
	[SerializeField] AudioClip jumpclip2;
	[SerializeField] AudioClip equip1sound;
	[SerializeField] AudioClip equip2sound;
	[SerializeField] AudioClip holster1sound;
	[SerializeField] AudioClip holster2sound;

	[HideInInspector] public Vector3 targetDirection;
	[HideInInspector] public Vector3 lookAtDirection;

	private bool grounded;



	Animator animator;
	Rigidbody m_RigidBody;
	private CapsuleCollider m_Capsule;
	private bool previouslyGrounded;
	private Vector3 groundContactNormal;
	float turnAmount;
	float torwardAmount;
	private bool isBlocking;
	private float swingTimer = 0f;
	GameObject mostRecentAttacker;

	void Start()
	{
		animator = GetComponent<Animator>();
		m_RigidBody = GetComponent<Rigidbody>();
		m_Capsule = GetComponent<CapsuleCollider>();
		m_RigidBody.freezeRotation = true;
		m_RigidBody.useGravity = false;
		drag = m_RigidBody.drag;

		currentStamina = maxStamina;
		currentHealth = maxHealth;
		currentSpeed = speed;
		currentStamRechargePS = stamRechargePS;
		targetDirection = Vector3.zero;
		lookAtDirection = Vector3.zero;
		isJumping = false;
		isSprinting = false;
		isClimbing = false;
		isBlocking = false;
		currentCanSprint = true;
		canAttackCurrent = canAttack;
		grounded = true;

		myWeapon.instigator = gameObject;
		mostRecentAttacker = gameObject;
	}


	void FixedUpdate()
	{
		lookAtDirection.Normalize();
		targetDirection.Normalize();
		TestGround(); //set grounded to correct state
		if (freeToMove())
		{

			if (targetDirection != Vector3.zero)
			{


				if (Mathf.Abs(AngleOfLookDirection()) > fullSpeedMaxDegrees)
				{
					currentSpeed = straifSpeed;
				}
				else if (isSprinting)
				{
					currentSpeed = sprintSpeed;
				}
				else
				{
					currentSpeed = speed;
				}

				Vector3 targetVelocity = targetDirection;
				targetVelocity *= currentSpeed;

				Vector3 velocityChange = (targetVelocity - m_RigidBody.velocity);
				velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
				velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
				velocityChange.y = 0;
				m_RigidBody.AddForce(velocityChange, ForceMode.VelocityChange);

				float z = m_RigidBody.velocity.z;
				float x = m_RigidBody.velocity.x;
				Vector3 currentmagnitude = new Vector3(x, 0, z);
				float velocityanim = Mathf.Clamp01(currentmagnitude.magnitude) * 2; //Take out times 2 when walking is added, as well as edit anim speeds (straife is too fast for walk)+

				animator.SetFloat("speed", velocityanim, animDampTime, 0.2f);
				animator.SetFloat("movementAngle", AngleOfLookDirection(), animDampTime, 0.2f);
			}
			else
			{
				animator.SetFloat("speed", 0, animDampTime * 2, 0.2f);
				animator.SetFloat("movementAngle", 0);
			}
		}
		//Gravity
		m_RigidBody.AddForce(0, -gravity, 0);


	}


	private void Update()
	{
		HandleStamina();
		lookAtDirection.Normalize();

		if (!isClimbing)
		{
			if (lookAtDirection != Vector3.zero)
			{
				Quaternion lookrotation2 = Quaternion.LookRotation(lookAtDirection, Vector3.up);
				lookrotation2.x = 0;
				lookrotation2.z = 0;
				transform.rotation = lookrotation2;
			}				
				//if angle <30 && > -30 animation speed = 1
				//abs angle < 45    45 = 2   0 = 1    speed = absAngle/45 + 1
			
			//Vector3 localmagnitude = transform.InverseTransformDirection(currentmagnitude);
			//old code for rotation.
			//if(weaponEquiped)
			//Vector3 localTarget = transform.InverseTransformPoint(target.position);
			//float addfloat = (Mathf.Atan2(localTarget.x, localTarget.z));
			//Vector3 relativePos = target.transform.position - transform.position;
			//Quaternion lookrotation = Quaternion.LookRotation(relativePos, Vector3.up);
			//lookrotation.x = 0;
			//lookrotation.z = 0;
			//animator.SetFloat("hor", (localmagnitude.x) + (addfloat * 2), dampTime, 0.8f);
			//animator.SetFloat("ver", (localmagnitude.z), dampTime, 0.8f);
			//transform.rotation = Quaternion.Lerp(transform.rotation, lookrotation, Time.deltaTime * rotateSpeed);
		}
	}

	float AngleOfLookDirection()
	{
		Vector2 A = new Vector2(targetDirection.x, targetDirection.z);
		Vector2 B = new Vector2(lookAtDirection.x, lookAtDirection.z);
		return Vector2.SignedAngle(A,B);
	}

	void HandleStamina()
	{

	}

	public float healthAsPercentage {
		get
		{
			return currentHealth / maxHealth;
		}
	}


	public bool Attack(bool toogle)
	{

		if (toogle)
		{
			if (canAttack && canAttackCurrent && !isClimbing)
			{
				canAttackCurrent = false;
				AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
				currentState = animator.GetCurrentAnimatorStateInfo(2);
				if (currentState.length == 1)
				{
					myWeapon.currentDamage = myWeapon.baseDamage * Power;
					myWeapon.currentForce = myWeapon.force * Power;
					myWeapon.isActivated = true;
					animator.SetFloat("attackType", 0);
					animator.SetBool("attack", true);
					return true;
				}
			}
		}
		canAttackCurrent = true;
		animator.SetBool("attack", false);
		return false;
	}

	//Called per frame from controller
	public bool TailSprint(bool toggle, Vector3 direction)
	{
		
		if (toggle && canSprint && freeToMove())
		{
			//When out of stamina, update() forces us to stop sprinting.
			//if we are not sprinting, we need enough for a jump to start again
			if (!isSprinting && currentStamina > tailJumpCost && lastTailJumpToggle == false)
			{
				//We only come in here once.  This is an action.
				isSprinting = true;

				//tailJump
				myaudiosource.clip = jumpclip2;
				myaudiosource.loop = false;
				myaudiosource.pitch = 1;
				myaudiosource.Play();
				m_RigidBody.AddForce(direction.x * tailJumpPower, CalculateJumpVerticalSpeed(tailJumpHeight), direction.z * tailJumpPower);
			}
			else if (isSprinting && currentStamina > sprintCostPS && AngleOfLookDirection() < fullSpeedMaxDegrees)
			{
				isSprinting = true;
			}
			else
			{
				isSprinting = false;
			}
		}
		else
		{
			isSprinting = false;
		}
		lastTailJumpToggle = toggle;
		return isSprinting;
	}

	//called per frame from controller when pressed
	public bool Jump(bool toggle, Vector3 direction)
	{
		if(toggle)
		{
			if (canJump && (grounded || isClimbing) && !isJumping && currentStamina > jumpCost && !lastJumpToggle)
			{
				//We only come in here once.  This is an action.
				isJumping = true;

				myaudiosource.clip = jumpclip1;
				myaudiosource.loop = false;
				myaudiosource.pitch = 1;
				myaudiosource.Play();
				m_RigidBody.AddForce(new Vector3(direction.x * jumpPower, CalculateJumpVerticalSpeed(jumpHeight), direction.z * jumpPower));
				animator.SetBool("jump", true);
				StartCoroutine(checkIfLanded());
				lastJumpToggle = toggle;
				return true;
			}
		}
		lastJumpToggle = toggle;
		animator.SetBool("jump", false);
		return false;

	}

	private IEnumerator checkIfLanded()
	{
		yield return new WaitForSeconds(1f);
		if(grounded && isJumping)
		{
			Landed();
		}
	}


		/// <summary>
		/// if(grounded && !isClimbing && !isJumping)
		/// </summary>
		bool freeToMove()
	{
		if(grounded && !isClimbing && !isJumping)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	void Landed()
	{
		isJumping = false;
	}

	void TestGround()
	{
		previouslyGrounded = grounded;
		RaycastHit hit;
#if UNITY_EDITOR
		// helper to visualise the ground check ray in the scene view
		Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * downCastRange));
#endif
		//Physics.SphereCast(transform.position, m_Capsule.radius * (1.0f), Vector3.down, out hit,((m_Capsule.height / 2f) - m_Capsule.radius) + downCastRange, Physics.AllLayers, QueryTriggerInteraction.Ignore)
		//if ()
		if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hit, downCastRange, mask))
		{
			grounded = true;
			m_RigidBody.drag = drag;
			groundContactNormal = hit.normal;

		}
		else
		{
			grounded = false;
			m_RigidBody.drag = 0;
			groundContactNormal = Vector3.up;
		}

		if(!previouslyGrounded && grounded)
		{
			Landed();
		}
		animator.SetBool("grounded", grounded);
	}

	float CalculateJumpVerticalSpeed(float height)
	{
		// From the jump height and gravity we deduce the upwards speed 
		// for the character to reach at the apex.
		return (2 * height * gravity);
	}
	void grabshield()
	{
		//shield.parent = lefthandpos;
		//shield.position = lefthandpos.position;
		//shield.rotation = lefthandpos.rotation;
		//weaponEquiped = true;
		//myaudiosource.clip = equip2sound;
		//myaudiosource.loop = false;
		//myaudiosource.pitch = 0.9f + 0.2f * Random.value;
		//myaudiosource.Play();
	}
	void grabweapon()
	{
		weapon.parent = righthandpos;
		weapon.position = righthandpos.position;
		weapon.rotation = righthandpos.rotation;
		myaudiosource.clip = equip1sound;
		myaudiosource.loop = false;
		myaudiosource.pitch = 0.9f + 0.2f * Random.value;
		myaudiosource.Play();


	}
	void holstershield()
	{
		//shield.parent = chestposshield;
		//shield.position = chestposshield.position;
		//shield.rotation = chestposshield.rotation;
		//myaudiosource.clip = holster1sound;
		//myaudiosource.loop = false;
		//myaudiosource.pitch = 0.9f + 0.2f * Random.value;
		//myaudiosource.Play();

	}
	void holsterweapon()
	{
		weaponEquiped = false;
		weapon.parent = chestposweapon;
		weapon.position = chestposweapon.position;
		weapon.rotation = chestposweapon.rotation;
		myaudiosource.clip = holster2sound;
		myaudiosource.loop = false;
		myaudiosource.pitch = 0.9f + 0.2f * Random.value;
		myaudiosource.Play();
	}

	public bool isWeaponEquiped { get { return weaponEquiped; } }
	public void ToggleEquipWeapon()
	{
		if (!changingWeapon)
		{
			StartCoroutine(CoToggleEquipWeapon());
		}
	}
	private IEnumerator CoToggleEquipWeapon()
	{

			changingWeapon = true;
			canAttack = false;

			if (!weaponEquiped)
			{
				animator.SetBool("grabweapon", true);
				yield return new WaitForSeconds(1);
				weaponEquiped = true;
			}
			else
			{

				animator.SetBool("grabweapon", false);
				yield return new WaitForSeconds(0.7f);
				weaponEquiped = false;
			}

			//after 1 seconds
			canAttack = true;
			changingWeapon = false;
	}

	//TODO add block animation(which should be push)
	public void Block(bool click, bool release)
	{
		if(click && !isBlocking)
		{
			isBlocking = true;
			//animate
		}
		else if (release && isBlocking)
		{
			isBlocking = false;
		}
	}

	//TODO change to throw weapon, and make it toss whatever weapon type im holding
	public void ThrowMace()
	{
		GameObject projectileMace = Instantiate(throwingWeapon, projectileSocket.transform.position, Quaternion.identity);
		FlyingMace maceComponent = projectileMace.GetComponent<FlyingMace>();
		maceComponent.currentDamage = damageWithMaceThrow;
		projectileMace.GetComponent<Rigidbody>().velocity = lookAtDirection * maceComponent.projectileSpeed;
	}

	public void TakeDamage(float damage, float force, EDamageType damageType, GameObject instigator)
	{
		currentHealth = Mathf.Clamp(currentHealth - damage, 0f, maxHealth);
		switch(damageType)
		{
			case EDamageType.blunt:
				bleed = damage * bleedBluntPercent;
				break;
			case EDamageType.spiked:
				bleed = damage * bleedSpikedPercent;
				break;
			case EDamageType.sharp:
				bleed = damage * bleedSharpPercent;
				break;
		}
	}

	public void ActivateWeapon()
	{
		myWeapon.Activate();
	}
	public void DeactivateWeapon()
	{
		myWeapon.Deactivate();
	}
}
