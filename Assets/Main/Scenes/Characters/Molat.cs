﻿using System;
using System.Collections;
using UnityEngine;

public class Molat : MonoBehaviour, IDamageable
{

	[Header("Stats")]
	public float maxHealth = 100f;
	public float power = 0.2f;


	[Header("Movement")]
	public float speed = 6f;
	[SerializeField] float straifSpeed = 5f;
	[SerializeField] float sprintSpeed = 8f;
	[Range(0f, 180f)] [SerializeField] float fullSpeedMaxDegrees = 80f;
	[SerializeField] float maxVelocityChange = 10.0f;

	[Header("Stamina")]
	public float maxStamina = 50f;
	[SerializeField] float stamRechargePS = 10f;
	[SerializeField] float tailJumpCost = -10f;
	[SerializeField] float sprintCostPS = -5f;
	[SerializeField] float jumpCost = -10f;


	[Header("Abilities")]
	public bool canJump = true;
	public bool canAttack = true;
	public bool canSprint = true;
	public bool canBlock = true;

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
	[SerializeField] float forceThreshold = 30;
	[SerializeField] float animDampTime = 2;
	[SerializeField] float clawAttackLength = 0.6f;
	[SerializeField] float maceAttackLength = 1f;
	[SerializeField] float maxPickupDistance = 1.2f;

	[SerializeField] float damageWithMaceThrow = 9.0f;
	[SerializeField] private GameObject throwingWeapon;
	[SerializeField] GameObject projectileSocket;
	public bool dontGoBelowZero = true;

	public GameObject weaponObject;
	private Weapon myWeapon;

	public GameObject clawObject;
	private Claw myClaw;

	public GameObject shieldObject;
	private Shield myShield;

	private float currentSpeed;
	private bool isJumping;
	private bool isSprinting;
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

	private bool healing = false;
	private bool isDead = false;

	private bool weaponEquiped = false;
	private bool changingWeapon = false;
	private MolatSounds molatSounds;
	private float belowGroundY = -2f;


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
	public delegate void DestroyedDelegate(Molat destroyedMolat);
	public event DestroyedDelegate DestroyedDel;
	public delegate void LookAtDirectionDelegate(Vector3 lookAtDirection);
	public event LookAtDirectionDelegate LookAtDirectionDel;
	public delegate void PickUpOrDropItem(Item item, bool pickedUp);
	public event PickUpOrDropItem PickUpOrDropDel;

	[HideInInspector] public GameObject mostRecentAttacker;


	Animator m_Animator;
	Rigidbody m_RigidBody;
	CapsuleCollider m_Capsule;
	[HideInInspector] public PlayerItemFinder playerItemFinder;

	private bool previouslyGrounded;
	private Vector3 groundContactNormal;
	float turnAmount;
	float torwardAmount;
	private bool isBlocking;
	bool animatorShouldBlock = false;
	private float swingTimer = 0f;
	private bool weaponActive = false;
	[HideInInspector] public bool isHealing = false;

	//Getters
	public Item MyWeapon { get { return myWeapon; } }
	public Item MyClaw { get { return myClaw; } }
	public Item MyShield { get { return myShield; } }
	public float MaxPickupDistance { get { return maxPickupDistance; } }


	void OnDestroy()
	{
		if(DestroyedDel != null)
		{
			DestroyedDel(this);
		}
	}


	void Start()
	{
		m_Animator = GetComponent<Animator>();
		m_RigidBody = GetComponent<Rigidbody>();
		m_Capsule = GetComponent<CapsuleCollider>();
		molatSounds = GetComponent<MolatSounds>();
		playerItemFinder = GetComponentInChildren<PlayerItemFinder>();


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
		isBlocking = false;
		grounded = true;

		SpawnAllItems();
		

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
			HandleIsBlockingCheck();
			HandleFallThroughFloor();

			//Anounce look at direction
			if (LookAtDirectionDel != null)
			{
				LookAtDirectionDel(lookAtDirection);
			}

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
			if (lookAtDirection != Vector3.zero)
			{
				Quaternion lookrotation2 = Quaternion.LookRotation(lookAtDirection, Vector3.up);
				lookrotation2.x = 0;
				lookrotation2.z = 0;
				transform.rotation = lookrotation2;
			}
		}
	}

	private void HandleFallThroughFloor()
	{
		if(dontGoBelowZero)
		{
			if (transform.position.y <= belowGroundY)
			{
				transform.position = new Vector3(transform.position.x, 1, transform.position.x);
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
			StaminaOnTick(stamRechargePS);
		}
		if (isSprinting)
		{
			if(!StaminaOnTick(sprintCostPS))
			{
				isSprinting = false;
			}
		}
	}
	bool StaminaOnTick(float staminaUseOrGain)
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
	bool UseAbility(float staminaCost)
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

	bool HandleDeath()
	{
		if(currentHealth <=0)
		{
			isDead = true;
			StopAllCoroutines();
			ExpressAction(Action.Die);
			m_Animator.SetBool("isDead", true);
			DropAllItems();
			DeactivateWeapon();
			DeactivateShield();
		}
		return isDead;
	}

	private void DropAllItems()
	{
		myWeapon = (Weapon) DropItem(myWeapon);
		myShield = (Shield) DropItem(myShield);
	}

	public bool IsDead { get { return isDead; } }

	public float StaminaAsPercentage
	{
		get
		{
			return currentStamina / maxStamina;
		}
	}

	public float HealthAsPercentage {
		get
		{
			return currentHealth / maxHealth;
		}
	}

	public bool Heal(float healAmout)
	{
		if(currentHealth >= maxHealth)
		{
			return false;
		}
		else
		{
			currentHealth = Mathf.Clamp(currentHealth + healAmout, 0, maxHealth);
			return true;
		}


	}

	public void SpawnAllItems()
	{
		SpawnWeapon();
		SpawnClaw();
		SpawnShield();

	}
	private void SpawnWeapon()
	{
		if (weaponObject)
		{
			myWeapon = (Weapon)SpawnItem(weaponObject, weapon, myWeapon);
		}
	}
	private void SpawnClaw()
	{
		if (!clawObject)
		{
			Debug.LogError("clawObject must have a claw prefab");
		}
		myClaw = (Claw)SpawnItem(clawObject, righthandpos, myClaw);
	}
	private void SpawnShield()
	{
		if(shieldObject)
		{
			myShield = (Shield)SpawnItem(shieldObject, shield, myShield);
		}
	}


	private Item SpawnItem(GameObject origional, Transform socketTransform, Item item)
	{
		if (item)
		{
			item.ItemDestroyDel -= ItemBroke;
			Destroy(item.gameObject);
		}

		GameObject clone = Instantiate(origional, socketTransform.position, socketTransform.rotation, socketTransform);
		Item newItem = clone.GetComponent<Item>();
		if (!newItem)
		{
			print(origional + "has no item");
		}
		else
		{
			SetUpItemToggle(newItem, true);
		}
		return newItem;
	}

	private void SetUpItemToggle(Item itemToSetUp, bool setUp)
	{
		if(setUp)
		{
			itemToSetUp.instigator = gameObject;
			itemToSetUp.ItemDestroyDel += ItemBroke;
			itemToSetUp.molatSounds = molatSounds;
			itemToSetUp.StartUsing(this);
		}
		else
		{
			itemToSetUp.instigator = null;
			itemToSetUp.ItemDestroyDel -= ItemBroke;
			itemToSetUp.molatSounds = null;
			itemToSetUp.Deactivate();
			itemToSetUp.StopUsing(this);
		}

	}

	public void GrabNearestItem()
	{
		Item nearestItem = playerItemFinder.GetClosestItem(maxPickupDistance);
		if(nearestItem)
		{
			TryToPickUpItem(nearestItem);
		}
	}

	public void TryToPickUpItem(Item newItem)
	{
		if (newItem && newItem.CanBePickedUp)
		{
			if (newItem is Weapon)
			{
				if(myWeapon)
				{
					DropItem(myWeapon);
				}
				myWeapon = (Weapon)PickUpItem(newItem, weapon);
				return;
			}
			else if (newItem is Shield)
			{
				if(myShield)
				{
					DropItem(myShield);
				}
				myShield = (Shield)PickUpItem(newItem, shield);
				return;
			}
		}
	}

	private Item PickUpItem(Item newItem, Transform socketTransform )
	{
		molatSounds.AmazingSoundEffect();
		molatSounds.PickUpItemSoundEffect();
		m_Animator.SetTrigger("pickUpItem");
		newItem.transform.SetPositionAndRotation(socketTransform.position, socketTransform.rotation);
		newItem.transform.SetParent(socketTransform);
		newItem.ItemPickedUp();
		SetUpItemToggle(newItem, true);
		if (PickUpOrDropDel != null)
		{
			PickUpOrDropDel(newItem, true);
		}
		return (newItem);
	}

	private Item DropItem(Item item)
	{
		if (item)
		{
			item.gameObject.transform.parent = null;
			item.ItemDestroyDel -= ItemBroke;
			item.ItemDropped();
			if (PickUpOrDropDel != null)
			{
				PickUpOrDropDel(item, false);
			}

		}
		return null;
	}




	private void ItemBroke(Item brokenitem)
	{
		brokenitem.StopUsing(this);
	}


	public void Attack()
	{
		if (!isDead && canAttack && !changingWeapon)
		{
			Weapon currentWeapon = myClaw;
			bool usingClaw = true;
			if(!UseClaw())
			{
				currentWeapon = myWeapon;
				usingClaw = false;
			}
			if (!weaponActive && UseAbility(currentWeapon.staminaCost))
			{
				//WaitForAttack
				weaponActive = true;
				StartCoroutine(WaitForAttack(usingClaw));
				ExpressAction(Action.Attack);
				currentWeapon.currentDamage = currentWeapon.baseDamage * power;
				currentWeapon.currentForce = currentWeapon.force * power;
				m_Animator.SetBool("usingClaw", usingClaw);
				m_Animator.SetFloat("attackType", 0);
				m_Animator.SetTrigger("attack");
			}
		}
	}

	private IEnumerator WaitForAttack(bool usingClaw)
	{
		if(usingClaw)
		{
			yield return new WaitForSeconds(clawAttackLength);
		}
		else
		{
			yield return new WaitForSeconds(maceAttackLength);
		}
		weaponActive = false;
	}

	//Called per frame from controller
	public bool TailSprint(bool toggle, Vector3 direction)
	{
		
		if (toggle && canSprint && freeToMove())
		{
			//When out of stamina, update() forces us to stop sprinting.
			//if we are not sprinting, we need enough for a jump to start again
			if (!isSprinting && lastTailJumpToggle == false && UseAbility(tailJumpCost))
			{
				//We only come in here once.  This is an action.
				isSprinting = true;
				ExpressAction(Action.tailJump);

				m_Animator.SetTrigger("tailJump");
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

	void HandleIsBlockingCheck()
	{
		if(animatorShouldBlock)
		{
			if (!m_Animator.GetCurrentAnimatorStateInfo(2).IsTag("blocking"))
			{
				Block(false);

				animatorShouldBlock = false;
				//turn animator off

				//turn shield off
				DeactivateShield();
			}
		}
	}

	//TODO add block animation(which should be push)
	public void Block(bool toggle)
	{
		if (canBlock && toggle && !isBlocking && IsWeaponEquiped && myShield)
		{
			isBlocking = true;
			m_Animator.SetBool("isBlocking", isBlocking);
		}
		else if(!toggle)
		{
			isBlocking = false;
			m_Animator.SetBool("isBlocking", isBlocking);

		}
	}

	//called every animation cycle by asnimation event
	public void ActivateShield()
	{
		if(myShield)
		{
			animatorShouldBlock = true;
			myShield.Activate();
		}
	}
	//Called on check animation tick
	public void DeactivateShield()
	{
		if (myShield)
		{
			myShield.Deactivate();
		}

	}


	//called per frame from controller when pressed
	public bool Jump(bool toggle, Vector3 direction)
	{
		if(toggle)
		{
			if (!isDead && canJump && (grounded) && !isJumping && !lastJumpToggle && UseAbility(jumpCost))
			{
				ExpressAction(Action.Jump);
				//We only come in here once.  This is an action.
				isJumping = true;

				myaudiosource.clip = jumpclip1;
				myaudiosource.loop = false;
				myaudiosource.pitch = 1;
				myaudiosource.Play();
				m_RigidBody.AddForce(new Vector3(direction.x * jumpPower, CalculateJumpVerticalSpeed(jumpHeight), direction.z * jumpPower));
				m_Animator.SetTrigger("jump");
				StartCoroutine(CheckIfLanded());
				lastJumpToggle = true;
				return true;
			}
		}
		lastJumpToggle = toggle;
		return false;

	}

	private IEnumerator CheckIfLanded()
	{
		yield return new WaitForSeconds(1f);
		if(grounded && isJumping)
		{
			Landed();
		}
	}

	bool freeToMove()
	{
		if(!isDead && grounded && !isJumping)
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
		if (shield)
		{
			shield.parent = lefthandpos;
			shield.position = lefthandpos.position;
			shield.rotation = lefthandpos.rotation;
			weaponEquiped = true;
			myaudiosource.clip = equip2sound;
			myaudiosource.loop = false;
			myaudiosource.pitch = 0.9f + 0.2f * UnityEngine.Random.value;
			myaudiosource.Play();
		}
	}
	void grabweapon()
	{
		if (weapon)
		{
			weapon.parent = righthandpos;
			weapon.position = righthandpos.position;
			weapon.rotation = righthandpos.rotation;
			myaudiosource.clip = equip1sound;
			myaudiosource.loop = false;
			myaudiosource.pitch = 0.9f + 0.2f * UnityEngine.Random.value;
			myaudiosource.Play();
		}
	}
	void holstershield()
	{
		if(shield)
		{
			shield.parent = chestposshield;
			shield.position = chestposshield.position;
			shield.rotation = chestposshield.rotation;
			myaudiosource.clip = holster1sound;
			myaudiosource.loop = false;
			myaudiosource.pitch = 0.9f + 0.2f * UnityEngine.Random.value;
			myaudiosource.Play();
		}

	}
	void holsterweapon()
	{
		if (weapon)
		{
			weaponEquiped = false;
			weapon.parent = chestposweapon;
			weapon.position = chestposweapon.position;
			weapon.rotation = chestposweapon.rotation;
			myaudiosource.clip = holster2sound;
			myaudiosource.loop = false;
			myaudiosource.pitch = 0.9f + 0.2f * UnityEngine.Random.value;
			myaudiosource.Play();
		}
	}

	public bool IsWeaponEquiped { get { return weaponEquiped; } }

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


	//TODO change to throw weapon, and make it toss whatever weapon type im holding
	public void ThrowMace()
	{
		if(!isDead)
		{
			GameObject projectileMace = Instantiate(ThrowingWeapon, projectileSocket.transform.position, Quaternion.identity);
			FlyingMace maceComponent = projectileMace.GetComponent<FlyingMace>();
			maceComponent.currentDamage = damageWithMaceThrow;
			projectileMace.GetComponent<Rigidbody>().velocity = lookAtDirection * maceComponent.projectileSpeed;
		}

	}


	public void TakeDamage(float damage, float force, Vector3 direction, GameObject instigator)
	{
		if(!IsDead)
		{
			if (IGotHitDel != null)
			{
				IGotHitDel(damage, maxHealth / damage, instigator, gameObject);
			}
			mostRecentAttacker = instigator;
			if (force > forceThreshold)
			{
				StartSliding(direction, force, 0.2f);
			}
			currentHealth = Mathf.Clamp(currentHealth - damage, 0f, maxHealth);
			if(HandleDeath())
			{
				molatSounds.DiedSoundEffect();
			}
			else if(force > forceThreshold)
			{
				m_Animator.SetTrigger("gotHit");
				molatSounds.GotHitSoundEffect();
			}
		}
	}

	public void ActivateWeapon()
	{
		if(UseClaw())
		{
			myClaw.Activate();
		}
		else
		{
			myWeapon.Activate();
		}
	}
	public void DeactivateWeapon()
	{
		if (UseClaw())
		{
			myClaw.Deactivate();
		}
		else
		{
			myWeapon.Deactivate();
		}
	}

	private bool UseClaw()
	{
		if (myWeapon == null || !IsWeaponEquiped)
			return true;
		return false;
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
