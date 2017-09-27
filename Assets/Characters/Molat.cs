using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Molat : MonoBehaviour, IDamageable
{

	[Header("Stats")]
	[SerializeField] float maxHealth = 100f;
	[SerializeField] float power = 0.2f;
	[SerializeField] int level = 1;


	[Header("Movement")]
	[SerializeField] float speed = 6f;
	[SerializeField] float straifSpeed = 5f;
	[SerializeField] float sprintSpeed = 8f;
	[Range(0f, 180f)] [SerializeField] float fullSpeedMaxDegrees = 80f;
	[SerializeField] float maxVelocityChange = 10.0f;
	[SerializeField] float climbSpeed = 8f;

	[Header("Stamina")]
	[SerializeField] float maxStamina = 50f;
	[SerializeField] float stamRechargePS = 10f;
	[SerializeField] float attackCost = -20f;
	[SerializeField] float tailJumpCost = -10f;
	[SerializeField] float sprintCostPS = -5f;
	[SerializeField] float climbCostPS = -5f;
	[SerializeField] float jumpCost = -10f;

	[Header("Damage and Bleed")]
	[SerializeField] float clawDamage = 20f;
	[SerializeField] float bleedBluntPercent = 0f;
	[SerializeField] float bleedSpikedPercent = 0.2f;
	[SerializeField] float bleedSharpPercent = 0.5f;
	[SerializeField] float attackLength = 1.2f;
	[SerializeField] float healingRatioPS = 1.2f;
	[SerializeField] float percentDamgeCauseKnockback = 0.4f;
	[SerializeField] float forceThreshold = 30;


	[Header("Abilities")]
	[SerializeField] bool canJump = true;
	[SerializeField] bool canAttack = true;
	[SerializeField] bool canSprint = true;
	[SerializeField] bool canClimb = false;

	[Header("Jumping")]
	
	[SerializeField] float tailJumpHeight = 3.0f;
	[SerializeField] float tailJumpPower = 1000.0f;
	[SerializeField] float tailJumpDuration = 0.4f;

	[SerializeField] float jumpHeight = 13.0f;
	[SerializeField] float jumpPower = 2.0f;
	[SerializeField] LayerMask mask;
	[SerializeField] float downCastRange = 1.2f;

	[Header("Other")]
	[SerializeField] float gravity = 18;
	[SerializeField] float animDampTime = 2;
	[SerializeField] float damageWithMaceThrow = 9.0f;
	[SerializeField]private GameObject throwingWeapon;
	[SerializeField] GameObject projectileSocket;
	[SerializeField] GameObject weaponObject;


	private Weapon myWeapon;
	private float currentSpeed;
	private bool isJumping;
	private bool isSprinting;
	private bool isClimbing;
	private bool isSliding;
	private float currentHealth;
	private float currentStamina;
	private float drag;
	private bool lastTailJumpToggle = false;
	private bool lastJumpToggle = false;
	private float slideTimer;
	private Vector3 slideDirection;
	private float slideSpeed;
	private float slideDuration;

	private float bleed = 0f;
	private bool bleeding = false;
	private bool healing = false;

	private bool weaponEquiped = false;
	private bool changingWeapon = false;
	private bool isDead = false;
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

	public delegate void ActionExpressedDelegate(Action action, GameObject instigator);
	public event ActionExpressedDelegate ActionExpressedDel;
	public delegate void IGotHitDelegate(float damage, float percentOfHealth, GameObject attacker, GameObject victim);
	public event IGotHitDelegate IGotHitDel;

	[HideInInspector] public GameObject mostRecentAttacker;


	Animator m_Animator;
	Rigidbody m_RigidBody;
	private CapsuleCollider m_Capsule;
	private bool previouslyGrounded;
	private Vector3 groundContactNormal;
	float turnAmount;
	float torwardAmount;
	private bool isBlocking;
	private float swingTimer = 0f;


	void Start()
	{
		m_Animator = GetComponent<Animator>();
		m_RigidBody = GetComponent<Rigidbody>();
		m_Capsule = GetComponent<CapsuleCollider>();
		GameObject weaponClone = Instantiate(weaponObject, weapon.transform.position, weapon.transform.rotation);
		weaponClone.transform.parent = weapon;
		myWeapon = weaponClone.GetComponent<Weapon>();
		m_RigidBody.freezeRotation = true;
		m_RigidBody.useGravity = false;
		drag = m_RigidBody.drag;

		currentStamina = maxStamina;
		currentHealth = maxHealth;
		currentSpeed = speed;
		targetDirection = Vector3.zero;
		lookAtDirection = Vector3.zero;
		isJumping = false;
		isSprinting = false;
		isClimbing = false;
		isBlocking = false;
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

			//if (targetDirection != Vector3.zero)
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

				m_Animator.SetFloat("speed", velocityanim, animDampTime, 0.2f);
				m_Animator.SetFloat("movementAngle", AngleOfLookDirection(), animDampTime, 0.2f);
			}
			if(targetDirection == Vector3.zero)
			{
				m_Animator.SetFloat("speed", 0, animDampTime * 2, 0.2f);
				m_Animator.SetFloat("movementAngle", 0);
			}
		}
		//Gravity
		m_RigidBody.AddForce(0, -gravity, 0);


	}



	private void Update()
	{
		if (!isDead)
		{
			HandleDeath();
			HandleStamina();
			lookAtDirection.Normalize();

			if (isSliding)
			{
				if (slideTimer > 0)
				{
					m_RigidBody.velocity = Vector3.Lerp(Vector3.zero, slideSpeed * slideDirection, slideTimer);
					slideTimer -= Time.deltaTime / slideDuration;
				}
				else
				{
					isSliding = false;
				}

			}

			if (!isClimbing)
			{
				if (lookAtDirection != Vector3.zero)
				{
					Quaternion lookrotation2 = Quaternion.LookRotation(lookAtDirection, Vector3.up);
					lookrotation2.x = 0;
					lookrotation2.z = 0;
					transform.rotation = lookrotation2;
				}
			}
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
		if(currentStamina != maxStamina)
		{
			staminaOnTick(stamRechargePS);
		}
		if (isSprinting)
		{
			if(!staminaOnTick(sprintCostPS))
			{
				isSprinting = false;
			}
		}
		if (isClimbing)
		{
			if (!staminaOnTick(climbCostPS))
			{
				isClimbing = false;
			}
		}
	}
	bool staminaOnTick(float staminaUseOrGain)
	{
		float stamUseOrGain = staminaUseOrGain * Time.deltaTime;
		if ((currentStamina + stamUseOrGain) < 0)
		{
			//out of stamina, play sound?
			return false;
		}
		if ((currentStamina + stamUseOrGain) > maxStamina)
		{
			//staminaCost full
			currentStamina = maxStamina;
			return false;
		}
		currentStamina = Mathf.Clamp(currentStamina + stamUseOrGain, 0f, maxStamina);
		return true;
	}
	bool useAbility(float staminaCost)
	{
		if ((currentStamina + staminaCost) < 0 || isDead)
		{
			//out of stamina, play sound?
			return false;
		}
		else
		{
			currentStamina = Mathf.Clamp(currentStamina + staminaCost, 0f, maxStamina);
			return true;
		}
	}

	void HandleDeath()
	{
		if(currentHealth <=0)
		{
			isDead = true;
			ExpressAction(Action.Die);
			m_Animator.SetBool("isDead", true);
		}
	}

	public bool IsDead { get { return isDead; } }

	public float staminaAsPercentage
	{
		get
		{
			return currentStamina / maxStamina;
		}
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
			if (!isDead && canAttack && !isClimbing && weaponEquiped)
			{
				AnimatorStateInfo currentState = m_Animator.GetCurrentAnimatorStateInfo(2);
				if (currentState.length == 1 && useAbility(attackCost))
				{
					ExpressAction(Action.Attack);
					myWeapon.currentDamage = myWeapon.baseDamage * power;
					myWeapon.currentForce = myWeapon.force * power;
					myWeapon.isActivated = true;
					m_Animator.SetFloat("attackType", 0);
					StartCoroutine(CoTickAnimation("attack"));
					return true;
				}
			}
		}
		return false;
	}

	//Called per frame from controller
	public bool TailSprint(bool toggle, Vector3 direction)
	{
		
		if (toggle && canSprint && freeToMove())
		{
			//When out of stamina, update() forces us to stop sprinting.
			//if we are not sprinting, we need enough for a jump to start again
			if (!isSprinting && lastTailJumpToggle == false && useAbility(tailJumpCost))
			{
				//We only come in here once.  This is an action.
				isSprinting = true;
				ExpressAction(Action.tailJump);

				StartCoroutine(CoTickAnimation("tailJump"));
				//tailJump
				myaudiosource.clip = jumpclip2;
				myaudiosource.loop = false;
				myaudiosource.pitch = 1;
				myaudiosource.Play();
				direction.y = 0f;
				StartSliding(direction, tailJumpPower, tailJumpDuration);
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

	private IEnumerator CoTickAnimation(string animName)
	{
		m_Animator.SetBool(animName, true);
		yield return new WaitForSeconds(0.1f);
		m_Animator.SetBool(animName, false);
	}

	//called per frame from controller when pressed
	public bool Jump(bool toggle, Vector3 direction)
	{
		if(toggle)
		{
			if (!isDead && canJump && (grounded || isClimbing) && !isJumping && !lastJumpToggle && useAbility(jumpCost))
			{
				ExpressAction(Action.Jump);
				//We only come in here once.  This is an action.
				isJumping = true;

				myaudiosource.clip = jumpclip1;
				myaudiosource.loop = false;
				myaudiosource.pitch = 1;
				myaudiosource.Play();
				m_RigidBody.AddForce(new Vector3(direction.x * jumpPower, CalculateJumpVerticalSpeed(jumpHeight), direction.z * jumpPower));
				StartCoroutine(CoTickAnimation("jump"));
				StartCoroutine(checkIfLanded());
				lastJumpToggle = true;
				return true;
			}
		}
		lastJumpToggle = toggle;
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
		if(!isDead && grounded && !isClimbing && !isJumping)
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
		lastJumpToggle = false;

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
		m_Animator.SetBool("grounded", grounded);
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

	public GameObject ThrowingWeapon { get { return throwingWeapon; } }

	public void ToggleEquipWeapon()
	{
		if (!isDead && !changingWeapon)
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
				ExpressAction(Action.EquipWeapon);
				m_Animator.SetBool("grabweapon", true);
				yield return new WaitForSeconds(1);
				weaponEquiped = true;
			}
			else
			{
				ExpressAction(Action.UnequipWeapon);
				m_Animator.SetBool("grabweapon", false);
				yield return new WaitForSeconds(0.7f);
				weaponEquiped = false;
			}

			//after 1 seconds
			canAttack = true;
			changingWeapon = false;
	}


	private void StartSliding(Vector3 Direction, float speed, float duration)
	{
		slideDirection = Direction;
		slideSpeed = speed;
		slideDuration = duration;
		slideTimer = 1f;
		isSliding = true;
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
		if(!isDead && !isClimbing)
		{
			GameObject projectileMace = Instantiate(ThrowingWeapon, projectileSocket.transform.position, Quaternion.identity);
			FlyingMace maceComponent = projectileMace.GetComponent<FlyingMace>();
			maceComponent.currentDamage = damageWithMaceThrow;
			projectileMace.GetComponent<Rigidbody>().velocity = lookAtDirection * maceComponent.projectileSpeed;
		}

	}


	public void TakeDamage(float damage, float force, Vector3 direction, EDamageType damageType, GameObject instigator)
	{
		if(!IsDead)
		{
			if (IGotHitDel != null)
			{
				IGotHitDel(damage, maxHealth / damage, instigator, gameObject);
			}
			mostRecentAttacker = instigator;
			if (currentHealth * percentDamgeCauseKnockback < damage)
			{
				//heel over

			}
			if (force > forceThreshold)
			{
				//direction.y = 0f;
				StartSliding(direction, force, 0.2f);
			}
			currentHealth = Mathf.Clamp(currentHealth - damage, 0f, maxHealth);
			switch (damageType)
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
			HandleDeath();
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

	private void ExpressAction(Action action)
	{
		if(ActionExpressedDel != null)
		{
			ActionExpressedDel(action, gameObject);
		}
	}


}

public enum Action {Attack,Die,Jump,tailJump,Parry,ThrowWeapon,EquipWeapon,UnequipWeapon};
