using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UserArenaController : MonoBehaviour {

	[SerializeField] GameObject _AIPrefab;
	[SerializeField] GameObject _PlayerPrefab;
	[SerializeField] GameObject _WeaponPrefab;
	[SerializeField] GameObject _ClawPrefab;
	[SerializeField] GameObject _ShieldPrefab;

	[SerializeField] GameObject optionsMenuButton;
	[SerializeField] GameObject returnToControlButton;

	[SerializeField] GameObject InGameUI;
	[SerializeField] GameObject ControlUI;

	[SerializeField] Transform clonePrefabSpawn;
	bool inGame = false;
	string mainMenuName = "Main Menu";
	private GameObject AIClone;
	private GameObject playerClone;
	private GameObject weaponClone;
	private GameObject clawClone;
	private GameObject shieldClone;
	private Molat myMolat;
	private MolatAIController myMolatAIController;
	private Weapon myClaw;
	private Weapon myWeapon;
	private Weapon myShield;

	HealingStationController healingStationController;

	private GameObject meSpawnedIn;

	//if you change here, ahve to change in ui canvas.  (Should have conntected them and justt get values in code.  oops)
	private float health = 100f;
	private float power = 1;
	private float speed = 6;
	private float stamina = 50;

	private bool canAttack = true;
	private bool canJump = true;
	private bool canSprint = true;
	private bool canBlock = true;

	private Vector3 lastCamPosition;
	private Quaternion lastCamRotation;

	private UIButtonSelection currentSelection = UIButtonSelection.None;

	MouseAndRaycast mouseAndRaycast;

	// Use this for initialization
	void Start ()
	{
		optionsMenuButton.SetActive(false);
		returnToControlButton.SetActive(false);
		InGameUI.SetActive(false);
		mouseAndRaycast = GetComponent<MouseAndRaycast>();
		mouseAndRaycast.ClickedOnMapDel += ClickedOnMap;

		healingStationController = GetComponent<HealingStationController>();

		clawClone = Instantiate(_ClawPrefab, clonePrefabSpawn);
		myClaw = clawClone.GetComponent<Weapon>();

		if (_WeaponPrefab)
		{
			weaponClone = Instantiate(_WeaponPrefab, clonePrefabSpawn);
			myWeapon = weaponClone.GetComponent<Weapon>();
		}

		if(_ShieldPrefab)
		{
			shieldClone = Instantiate(_ShieldPrefab, clonePrefabSpawn);
			myShield = shieldClone.GetComponent<Weapon>();
		}
	}

	// Update is called once per frame
	void Update () {

		if (Input.GetKeyDown("escape") || Input.GetKeyDown("p"))
		{
			Cursor.lockState = CursorLockMode.None;
			OpenMenu();
		}
	}

	public void SwapPlayerMode(bool swapToInGame)
	{
		GetComponent<AudioListener>().enabled = !swapToInGame;
		mouseAndRaycast.inGame = swapToInGame;
		InGameUI.SetActive(swapToInGame);
		ControlUI.SetActive(!swapToInGame);
		inGame = swapToInGame;
		if(swapToInGame)
		{
			lastCamPosition = transform.position;
			lastCamRotation = transform.rotation;
		}
		else
		{
			transform.position = lastCamPosition;
			transform.rotation = lastCamRotation;
			Destroy(meSpawnedIn);
		}

	}

	private void ClickedOnMap(RaycastHit? raycastHit)
	{
		switch (currentSelection)
		{
			case UIButtonSelection.Spawn:
				SpawnAICharacter(raycastHit.Value.point + Vector3.up);
				break;
			case UIButtonSelection.Destroy:
				DestroyObject(raycastHit.Value.collider.gameObject);
				break;
			case UIButtonSelection.SpawnMe:
				SpawnMyselfIn(raycastHit.Value.point + Vector3.up);
				break;
			case UIButtonSelection.SpawnMace:
				SpawnMace(raycastHit.Value.point + Vector3.up);
				break;
			case UIButtonSelection.SpawnShield:
				SpawnShield(raycastHit.Value.point + Vector3.up);
				break;
			case UIButtonSelection.SpawnHealing:
				SpawnHealingStation(raycastHit.Value.point);
				break;
		}
		
	}

	private void InitiateAllParameters(Molat molat, MolatAIController molatAIController)
	{
		if (molat)
		{
			molat.clawObject = clawClone;
			molat.weaponObject = weaponClone;
			molat.shieldObject = shieldClone;

			molat.maxHealth = health;
			molat.power = power;
			molat.speed = speed;
			molat.maxStamina = stamina;

			molat.canAttack = canAttack;
			molat.canBlock = canBlock;
			molat.canSprint = canSprint;
			molat.canJump = canJump;
	}

		if (molatAIController)
		{
			molatAIController.healingStationController = healingStationController;
		}
	}

	public void SpawnHealingStation(Vector3 spawnPoint)
	{
		healingStationController.SpawnHealingStation(spawnPoint);
	}

	public void SpawnAICharacter(Vector3 spawnPoint)
	{
		GameObject PrefabCopy = Instantiate(_AIPrefab, spawnPoint, Quaternion.LookRotation(Vector3.right,Vector3.up));
		InitiateAllParameters(PrefabCopy.GetComponent<Molat>(), PrefabCopy.GetComponent<MolatAIController>());
	}

	public void SpawnShield(Vector3 spawnPoint)
	{
		GameObject PrefabCopy = Instantiate(shieldClone, spawnPoint, UnityEngine.Random.rotation);
		PrefabCopy.GetComponent<Item>().ItemDropped();
	}

	public void SpawnMace(Vector3 spawnPoint)
	{
		GameObject PrefabCopy = Instantiate(weaponClone, spawnPoint, UnityEngine.Random.rotation);
		PrefabCopy.GetComponent<Item>().ItemDropped();
	}

	public void DestroyObject(GameObject currentObject)
	{
		Destroy(currentObject);
	}

	public void SpawnMyselfIn(Vector3 spawnPoint)
	{
		meSpawnedIn = Instantiate(_PlayerPrefab, spawnPoint, Quaternion.LookRotation(Vector3.right, Vector3.up));
		InitiateAllParameters(meSpawnedIn.GetComponent<Molat>(), null);
		SwapPlayerMode(true);
	}

	public void TakeControlOfUnit(GameObject molatTarget)
	{
		meSpawnedIn = molatTarget;
		//MolatAIController aiComponent = molatTarget.GetComponent<MolatAIController>();
		//Destroy(aiComponent);
		//molatTarget.AddComponent<MolatPlayerController>();
		////TODO Swap Cameras, swap canvases
	}

	//OPTIONS MENU BUTTONS

	public void OpenMenu()
	{

		optionsMenuButton.SetActive(true);
		if(inGame)
		{
			returnToControlButton.SetActive(true);
		}
	}

	public void CloseMenu()
	{
		optionsMenuButton.SetActive(false);
		returnToControlButton.SetActive(false);
		if(inGame)
		{
			Cursor.lockState = CursorLockMode.Locked;
		}
	}

	public void RegainArenaControl()
	{
		SwapPlayerMode(false);
		CloseMenu();
	}

	public void QuitToMainMenu()
	{
		SceneManager.LoadScene(mainMenuName, LoadSceneMode.Single);
	}

	public void QuitToDesktop()
	{
		Application.Quit();
	}
	//END OPTIONS MENU BUTTONS

	//CONTROL BUTTONS
	public void SpawnAIPressed()
	{
		mouseAndRaycast.lookingForCharacter = false;
		mouseAndRaycast.lookingForItem = false;

		OnOptionClick(UIButtonSelection.Spawn);
	}
	public void SpawnMacePressed()
	{
		mouseAndRaycast.lookingForCharacter = false;
		mouseAndRaycast.lookingForItem = false;
		OnOptionClick(UIButtonSelection.SpawnMace);
	}
	public void SpawnShieldPressed()
	{
		mouseAndRaycast.lookingForCharacter = false;
		mouseAndRaycast.lookingForItem = false;
		OnOptionClick(UIButtonSelection.SpawnShield);
	}
	public void SpawnHealingPressed()
	{
		mouseAndRaycast.lookingForCharacter = false;
		mouseAndRaycast.lookingForItem = false;
		OnOptionClick(UIButtonSelection.SpawnHealing);
	}
	public void SpawnSelfPressed()
	{
		mouseAndRaycast.lookingForCharacter = false;
		mouseAndRaycast.lookingForItem = false;
		OnOptionClick(UIButtonSelection.SpawnMe);
	}
	public void DestroyObjectPressed()
	{
		mouseAndRaycast.lookingForCharacter = true;
		mouseAndRaycast.lookingForItem = true;
		OnOptionClick(UIButtonSelection.Destroy);
	}

	public void OnOptionClick(UIButtonSelection buttonSelection)
	{
		currentSelection = buttonSelection;
	}
	//INPUT FIELD SETTERS
	public void SetHealth(string health)
	{
		this.health = float.Parse(health);
	}
	public void SetPower(string power)
	{
		this.power = float.Parse(power);
	}
	public void SetSpeed(string speed)
	{
		this.speed = float.Parse(speed);
	}
	public void SetStamina(string stamina)
	{
		this.stamina = float.Parse(stamina);
	}

	public void SetCanAttack(bool canAttack)
	{
		print(canAttack);
		this.canAttack = canAttack;
	}
	public void SetCanJump(bool canJump)
	{
		this.canJump = canJump;
	}
	public void SetCanSprint(bool canSprint)
	{
		this.canSprint = canSprint;
	}
	public void SetCanBlock(bool canBlock)
	{
		this.canBlock = canBlock;
	}

	public void SetClawDamage(string clawDamage)
	{
		myClaw.baseDamage = float.Parse(clawDamage);
	}
	public void SetClawForce(string clawForce)
	{
		myClaw.force = float.Parse(clawForce);
	}

	public void SetWeaponDamage(string weaponDamage)
	{
		myWeapon.baseDamage = float.Parse(weaponDamage);
	}
	public void SetWeaponForce(string weaponForce)
	{
		myWeapon.force = float.Parse(weaponForce);
	}
	public void SetWeaponHitsBeforeBreak(string weaponHitsBeforeBreak)
	{
		myWeapon.durability = float.Parse(weaponHitsBeforeBreak);
	}

	public void ShieldDamageBeforeBreak(string durability)
	{
		myShield.durability = float.Parse(durability);
	}

	//END INPUT FIELD SETTERS
}

public enum UIButtonSelection {Spawn, SpawnMace, SpawnShield, SpawnHealing, SpawnMe, Destroy, None}
