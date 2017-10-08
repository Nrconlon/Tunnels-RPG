using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

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

	[Header("Cooldown")]
	[SerializeField] float tailJumpCD = 1f;
	[SerializeField] float gotBlockedCD = 1f;
	[SerializeField] float attackCDWep = 1f;
	[SerializeField] float attackCDClaw = 0.5f;
	[SerializeField] float PowerSwingCD = 2f;
	[SerializeField] float gotHitHardCD = 0.5f;



	bool tailJumpCooldown = false;


	[Header("Abilities")]
	public bool canJump = true;
	public bool canAttack = true;
	public bool canSprint = true;
	public bool canBlock = true;

	[Header("Jumping")]
	
	[SerializeField] float tailJumpPower = 40f;
	[SerializeField] float tailJumpDuration = 0.4f;

	[SerializeField] float jumpHeight = 2.0f;
	[SerializeField] float jumpPower = 2.0f;
	[SerializeField] LayerMask mask;
	[SerializeField] float downCastRange = 1.2f;

	[Header("Other")]
	[SerializeField] float gravity = 18;
	[SerializeField] float forceThreshold = 30;
	[SerializeField] float animDampTime = 2;
	[SerializeField] float maxPickupDistance = 1.2f;
	[SerializeField] float weaponThrowSpeed = 17f;
	[SerializeField] float powerSwingDamageMultiplier = 1.5f;
	[SerializeField] float powerSwingForceMultiplier = 3f;

	[SerializeField] private GameObject throwingWeapon;
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
	private float currentCooldown = 0;
	private float drag;
	private bool lastJumpToggle = false;
	private float slideTimer;
	private Vector3 slideDirection;
	private float slideSpeed;
	private float slideDuration;

	private bool isDead = false;

	private bool weaponEquiped = false;
	private bool changingWeapon = false;
	private MolatSounds molatSounds;
	private float belowGroundY = -2f;
	bool JumpRdyForPwrAttack = false;
	//

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

	public delegate void ActionExpressedDelegate(Action action, Molat instigator);
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
	[HideInInspector] public PlayerItemFinder playerItemFinder;

	private bool previouslyGrounded;
	float turnAmount;
	float torwardAmount;
	private bool isBlocking;
	bool animatorShouldBlock = false;
	[HideInInspector] public bool isHealing = false;

	//Getters
	public Item MyWeapon { get { return myWeapon; } }
	public Item MyClaw { get { return myClaw; } }
	public Item MyShield { get { return myShield; } }
	public float MaxPickupDistance { get { return maxPickupDistance; } }
	public MolatSounds MolatSounds { get { return molatSounds; } }


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
		molatSounds = GetComponent<MolatSounds>();
		playerItemFinder = GetComponentInChildren<PlayerItemFinder>();


		m_RigidBody.freezeRotation = true;
		m_RigidBody.useGravity = false;
		drag = m_RigidBody.drag;

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
		if(!isDead)
		{
			lookAtDirection.Normalize();
			targetDirection.Normalize();
			TestGround(); //set grounded to correct state
			if (freeToMove())
			{

				if (isSliding)
				{
					if (slideTimer > 0)
					{
						m_RigidBody.velocity = Vector3.Lerp(Vector3.zero, slideSpeed * slideDirection, slideTimer);
						slideTimer -= 0.02f / slideDuration;
					}
					else
					{
						isSliding = false;
					}
				}
				else
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
				if (targetDirection == Vector3.zero)
				{
					m_Animator.SetFloat("speed", 0, animDampTime * 2, 0.2f);
					m_Animator.SetFloat("movementAngle", 0);
				}
			}
			//Gravity
			m_RigidBody.AddForce(0, -gravity, 0);
		}
		else
		{
			m_RigidBody.velocity = Vector3.zero;
		}
	}



	private void Update()
	{
		if (!isDead)
		{
			HandleDeath();
			HandleCooldown();
			lookAtDirection.Normalize();
			HandleIsBlockingCheck();
			HandleFallThroughFloor();

			//Anounce look at direction
			if (LookAtDirectionDel != null)
			{
				LookAtDirectionDel(lookAtDirection);
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
				transform.position = new Vector3(transform.position.x, 1, transform.position.z);
			}
		}
	}

	float AngleOfLookDirection()
	{
		Vector2 A = new Vector2(targetDirection.x, targetDirection.z);
		Vector2 B = new Vector2(lookAtDirection.x, lookAtDirection.z);
		return Vector2.SignedAngle(A,B);
	}


	void HandleCooldown()
	{
		if(currentCooldown > 0)
		{
			currentCooldown = Mathf.Clamp(currentCooldown - Time.deltaTime, 0, 100);
		}
	}

	bool AddToCooldown(float cooldownCost)
	{
		if(currentCooldown > 0)
		{
			return false;
		}
		else
		{
			currentCooldown += cooldownCost;
			return true;
		}

	}
	void AddToCooldownForced(float cooldownCost)
	{
		currentCooldown += cooldownCost;
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

	public float CurrentCooldown
	{
		get
		{
			return currentCooldown;
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

	public void GotBlocked()
	{
		AddToCooldownForced(gotBlockedCD);
		ExpressAction(Action.GotBlocked);
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
			float cooldownUse = attackCDClaw;
			if(!UseClaw())
			{
				currentWeapon = myWeapon;
				usingClaw = false;
				cooldownUse = attackCDWep;
			}
			if (AddToCooldown(cooldownUse))
			{
				//Power attack?
				if(JumpRdyForPwrAttack)
				{
					ExpressAction(Action.PowerAttack);
					currentWeapon.currentDamage = currentWeapon.baseDamage * power * powerSwingDamageMultiplier;
					currentWeapon.currentForce = currentWeapon.force * power * powerSwingForceMultiplier;
					AddToCooldownForced(PowerSwingCD);
					molatSounds.PowerSwingSoundEffect();
				}
				else
				{
					ExpressAction(Action.Attack);
					currentWeapon.currentDamage = currentWeapon.baseDamage * power;
					currentWeapon.currentForce = currentWeapon.force * power;
				}
				//WaitForAttack
				m_Animator.SetBool("usingClaw", usingClaw);
				m_Animator.SetFloat("attackType", 0);
				m_Animator.SetTrigger("attack");
			}
		}
	}
	private IEnumerator TailJumpCooldown()
	{

		yield return new WaitForSeconds(tailJumpCD);
		tailJumpCooldown = false;
	}

	public void StopTailSprint()
	{
		TailSprint(false, Vector3.zero);
	}
	//Called per frame from controller
	public bool TailSprint(bool toggle, Vector3 direction)
	{
		//TODO Remove
		direction = direction.normalized;
		if (toggle && canSprint && freeToMove())
		{
			//When out of stamina, update() forces us to stop sprinting.
			//if we are not sprinting, we need enough for a jump to start again
			if (!isSprinting && !tailJumpCooldown && direction != Vector3.zero)
			{
				tailJumpCooldown = true;
				StartCoroutine(TailJumpCooldown());
				//We only come in here once.  This is an action.
				isSprinting = true;
				ExpressAction(Action.tailJump);

				m_Animator.SetTrigger("tailJump");
				//tailJump
				myaudiosource.clip = jumpclip2;
				myaudiosource.loop = false;
				myaudiosource.pitch = 1;
				myaudiosource.Play();
				direction = direction.normalized;
				direction.y = 0f;
				StartSliding(direction, tailJumpPower, tailJumpDuration);
			}
			else if (isSprinting && AngleOfLookDirection() < fullSpeedMaxDegrees)
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
		return isSprinting;
	}



	void HandleIsBlockingCheck()
	{
		if(!myShield)
		{
			Block(false);

			animatorShouldBlock = false;
			DeactivateShield();
		}
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
		JumpRdyForPwrAttack = toggle;
		if (toggle)
		{
			if (!isDead && canJump && (grounded) && !isJumping && !lastJumpToggle)
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
		if(!isDead && grounded)
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

		}
		else
		{
			grounded = false;
			m_RigidBody.drag = 0;
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
			if (myShield)
			{
				myaudiosource.clip = equip2sound;
				myaudiosource.loop = false;
				myaudiosource.pitch = 0.9f + 0.2f * UnityEngine.Random.value;
				myaudiosource.Play();
			}
		}
	}
	void grabweapon()
	{
		if (weapon)
		{
			weapon.parent = righthandpos;
			weapon.position = righthandpos.position;
			weapon.rotation = righthandpos.rotation;
			if (myWeapon)
			{
				myaudiosource.clip = equip1sound;
				myaudiosource.loop = false;
				myaudiosource.pitch = 0.9f + 0.2f * UnityEngine.Random.value;
				myaudiosource.Play();
			}
		}
	}
	void holstershield()
	{
		if(shield)
		{
			shield.parent = chestposshield;
			shield.position = chestposshield.position;
			shield.rotation = chestposshield.rotation;
			if (myShield)
			{
				myaudiosource.clip = holster1sound;
				myaudiosource.loop = false;
				myaudiosource.pitch = 0.9f + 0.2f * UnityEngine.Random.value;
				myaudiosource.Play();
			}
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
			if(myWeapon)
			{
				myaudiosource.clip = holster2sound;
				myaudiosource.loop = false;
				myaudiosource.pitch = 0.9f + 0.2f * UnityEngine.Random.value;
				myaudiosource.Play();
			}
		}
	}

	public bool IsWeaponEquiped { get { return weaponEquiped; } }


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
	public void ThrowWeapon()
	{
		if(!isDead && weaponEquiped && myWeapon)
		{
			ExpressAction(Action.ThrowWeapon);
			Weapon throwingWeapon = myWeapon;
			myWeapon = (Weapon) DropItem(myWeapon);
			throwingWeapon.GetComponent<Rigidbody>().velocity = new Vector3(lookAtDirection.x, lookAtDirection.y + 0.2f, lookAtDirection.z) * weaponThrowSpeed;
			throwingWeapon.BeingThrown(throwingWeapon.throwingDamage * power);
			m_Animator.SetBool("usingClaw", false);
			m_Animator.SetFloat("attackType", 1);
			m_Animator.SetTrigger("attack");
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
				AddToCooldownForced(gotHitHardCD);
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
			ActionExpressedDel(action, this);
		}
	}
}

public enum Action {Attack,Die,Jump,tailJump,Parry,ThrowWeapon,EquipWeapon,UnequipWeapon, GotBlocked,PowerAttack};
