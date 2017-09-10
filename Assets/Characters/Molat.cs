using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Molat : MonoBehaviour, IDamageable
{


	[SerializeField] float maxStamina = 100f;
	[SerializeField] float maxHealth = 100f;
	[SerializeField] bool canJump = true;
	[SerializeField] bool canAttack = true;
	[SerializeField] bool canSprint = true;
	[SerializeField] bool canClimb = false;
	[SerializeField] float speed = 6f;
	[SerializeField] float sprintSpeed = 8f;
	//[SerializeField] float climbSpeed = 8f;

	[SerializeField] float stamRechargePS = 20f;
	[SerializeField] float sprintCostPS = 5f;
	//[SerializeField] float climbCostPS = 5f;
	[SerializeField] float tailJumpCost = 5f;
	[SerializeField] float tailJumpHeight = 1.0f;
	[SerializeField] float tailJumpPower = 3.0f;

	[SerializeField] float gravity = 25;
	[SerializeField] float jumpCost = 10f;
	[SerializeField] float jumpHeight = 5.0f;
	[SerializeField] float jumpPower = 2.0f;

	[SerializeField] float maxVelocityChange = 10.0f;
	[SerializeField] float dampTime = 2;
	[SerializeField] float runAnimationRatio = 0.3f;
	[SerializeField] LayerMask mask;
	[SerializeField] float downCastRange = 1.2f;


	private float currentStamRechargePS;
	private float currentSpeed;
	private bool isJumping;
	private bool isSprinting;
	private bool isClimbing;
	private float currentHealth;
	private float currentStamina;
	private bool canAttackCurrent;
	private float drag;

	private bool weaponEquiped = false;
	private bool changingWeapon = false;



	[SerializeField] AudioSource myaudiosource;
	//[SerializeField] Transform target;
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

	[HideInInspector]  public Vector3 targetDirection;

	private bool grounded;



	Animator animator;
	Rigidbody m_RigidBody;
	private CapsuleCollider m_Capsule;
	private bool previouslyGrounded;
	private Vector3 groundContactNormal;

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
		isJumping = false;
		isSprinting = false;
		isClimbing = false;
		canAttackCurrent = canAttack;
		grounded = true;
	}


	void FixedUpdate()
	{
		TestGround(); //set grounded to correct state
		if (freeToMove())
		{

			if (targetDirection != Vector3.zero)
			{
				Vector3 velocity = m_RigidBody.velocity;
				float z = m_RigidBody.velocity.z;
				float x = m_RigidBody.velocity.x;
				Vector3 currentmagnitude = new Vector3(x, 0, z);
				Vector3 localmagnitude = transform.InverseTransformDirection(currentmagnitude);

				float velocityanim = Mathf.Clamp01(currentmagnitude.magnitude);
				velocityanim *= (currentSpeed * runAnimationRatio);

				Vector3 targetVelocity = targetDirection;
				targetVelocity *= currentSpeed;


				Vector3 velocityChange = (targetVelocity - velocity);
				velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
				velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
				velocityChange.y = 0;
				m_RigidBody.AddForce(velocityChange, ForceMode.VelocityChange);
				animator.SetFloat("speed", velocityanim, dampTime, 0.2f);
			}
			else
			{
				animator.SetFloat("speed", 0, dampTime*2, 0.2f);
			}

		}
		else
		{
			animator.SetFloat("speed", 0, dampTime * 2, 0.2f);
		}
		//Gravity
		m_RigidBody.AddForce(0, -gravity, 0);


	}

	private void Update()
	{
		RotateView();
		HandleStamina();

		if (!isClimbing)
		{
			if (targetDirection != Vector3.zero)
			{
				Quaternion lookrotation2 = Quaternion.LookRotation(targetDirection, Vector3.up);
				lookrotation2.x = 0;
				lookrotation2.z = 0;
				transform.rotation = lookrotation2;
			}
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

	void RotateView()
	{
		//avoids the mouse looking if the game is effectively paused
		if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

		// get the rotation before it's changed
		float oldYRotation = transform.eulerAngles.y;

		//mouseLook.LookRotation(transform, cam.transform);

		if (!isClimbing)
		{
			// Rotate the rigidbody velocity to match the new direction that the character is looking
			Quaternion velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
			m_RigidBody.velocity = velRotation * m_RigidBody.velocity;
		}
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


	public bool Attack()
	{
		if(canAttack && canAttackCurrent && !isClimbing)
		{
			if(weaponEquiped)
			{
				AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(2);
				if (currentState.length == 0)
				{
					int attackrandom = Random.Range(0, 4);
					animator.SetFloat("random", attackrandom);
					animator.SetBool("attack", true);
					return true;

				}
			}
			
		}
		animator.SetBool("attack", false);
		return false;
	}

	//Called per frame from controller
	public bool TailSprint(bool toggle, Vector3 direction)
	{
		//Called on Tick
		if(canSprint && freeToMove())
		{
			if (toggle)
			{
				//When out of stamina, update() forces us to stop sprinting.
				//if we are not sprinting, we need enough for a jump to start again
				if (!isSprinting && currentStamina > tailJumpCost)
				{
					//We only come in here once.  This is an action.
					isSprinting = true;

					//tailJump
					myaudiosource.clip = jumpclip2;
					myaudiosource.loop = false;
					myaudiosource.pitch = 1;
					myaudiosource.Play();
					m_RigidBody.AddForce(direction.x * tailJumpPower, CalculateJumpVerticalSpeed(tailJumpHeight), direction.z * tailJumpPower);


					currentSpeed = sprintSpeed;

				}
				else if(isSprinting && currentStamina > sprintCostPS)
				{
					currentSpeed = sprintSpeed;
					isSprinting = true;
				}
				else
				{
					currentSpeed = speed;
					isSprinting = false;
				}
			}
			else
			{
				isSprinting = false;
				currentSpeed = speed;
			}
		}
		return isSprinting;
	}

	//called per frame from controller when pressed
	public bool Jump(bool toggle, Vector3 direction)
	{
		if(toggle)
		{
			if (canJump && (grounded || isClimbing) && !isJumping && currentStamina > jumpCost)
			{
				//We only come in here once.  This is an action.
				isJumping = true;

				myaudiosource.clip = jumpclip1;
				myaudiosource.loop = false;
				myaudiosource.pitch = 1;
				myaudiosource.Play();
				print(CalculateJumpVerticalSpeed(jumpHeight));
				m_RigidBody.AddForce(new Vector3(direction.x * jumpPower, CalculateJumpVerticalSpeed(jumpHeight), direction.z * jumpPower));
				animator.SetBool("jump", true);
				return true;
			}
		}
		animator.SetBool("jump", false);
		return false;

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
		print("landed");
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

		if(!previouslyGrounded && grounded && isJumping)
		{
			isJumping = false;
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
		shield.parent = lefthandpos;
		shield.position = lefthandpos.position;
		shield.rotation = lefthandpos.rotation;
		weaponEquiped = true;
		myaudiosource.clip = equip2sound;
		myaudiosource.loop = false;
		myaudiosource.pitch = 0.9f + 0.2f * Random.value;
		myaudiosource.Play();
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
		shield.parent = chestposshield;
		shield.position = chestposshield.position;
		shield.rotation = chestposshield.rotation;
		myaudiosource.clip = holster1sound;
		myaudiosource.loop = false;
		myaudiosource.pitch = 0.9f + 0.2f * Random.value;
		myaudiosource.Play();

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
	public IEnumerator ToggleEquipWeapon()
	{
		if(!changingWeapon)
		{
			changingWeapon = true;
			canAttack = false;

			if (!weaponEquiped)
			{
				animator.SetBool("grabweapon", true);
				yield return new WaitForSeconds(2);
				weaponEquiped = true;
			}
			else
			{

				animator.SetBool("grabweapon", false);
				yield return new WaitForSeconds(2);
				weaponEquiped = false;
			}

			//after 2 seconds
			canAttack = true;
			changingWeapon = false;
		}

	}

	public void TakeDamage(float damage)
	{
		currentHealth = Mathf.Clamp(currentHealth - damage, 0f, maxHealth);
	}
}
