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

	[SerializeField] Text toolTipText;


	[SerializeField] Transform clonePrefabSpawn;
	bool inGame = false;
	string mainMenuName = "Main Menu";
	private GameObject weaponClone;
	private GameObject clawClone;
	private GameObject shieldClone;
	private Weapon myClaw;
	private Weapon myWeapon;
	private Weapon myShield;

	HealingStationController healingStationController;

	private GameObject meSpawnedIn;

	//if you change here, ahve to change in ui canvas.  (Should have conntected them and justt get values in code.  oops)
	private float health = 100f;
	private float power = 1;
	private float speed = 6;
	private float courage = 4;
	private float skill = 2;

	private bool canAttack = true;
	private bool canBlock = true;

	private Vector3 lastCamPosition;
	private Quaternion lastCamRotation;

	bool hasPressedButton = false;
	bool hasSpawnedUnit = false;

	private UIButtonSelection currentSelection = UIButtonSelection.None;

	MouseAndRaycast mouseAndRaycast;
	private bool hasWeapon = true;
	private bool hasShield = true;
	private int teamNum = 0;

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
		if (Input.GetKeyDown("1"))
		{
			SpawnAIPressed();
		}
		if (Input.GetKeyDown("2"))
		{
			SpawnMacePressed();
		}
		if (Input.GetKeyDown("3"))
		{
			SpawnShieldPressed();
		}
		if (Input.GetKeyDown("4"))
		{
			SpawnHealingPressed();
		}
		if (Input.GetKeyDown("5"))
		{
			SpawnSelfPressed();
		}
		if (Input.GetKeyDown("6"))
		{
			DestroyObjectPressed();
		}
	}

	public void SwapPlayerMode(bool swapToInGame)
	{
		GetComponent<AudioListener>().enabled = !swapToInGame;
		mouseAndRaycast.SwapPlayerMode(swapToInGame);
		mouseAndRaycast.inGame = swapToInGame;
		InGameUI.SetActive(swapToInGame);
		ControlUI.SetActive(!swapToInGame);
		inGame = swapToInGame;
		if(swapToInGame)
		{
			this.tag = "Untagged";
			lastCamPosition = transform.position;
			lastCamRotation = transform.rotation;
		}
		else
		{
			this.tag = "MainCamera";
			transform.position = lastCamPosition;
			transform.rotation = lastCamRotation;
			Destroy(meSpawnedIn);
		}

	}

	private void ClickedOnMap(RaycastHit? raycastHit)
	{
		if (hasPressedButton && !hasSpawnedUnit)
		{
			hasSpawnedUnit = true;
			toolTipText.enabled = false;
		}
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
			if (hasWeapon)
			{
				molat.weaponObject = weaponClone;
			}
			else
			{
				molat.weaponObject = null;
			}

			if (hasShield)
			{
				molat.shieldObject = shieldClone;
			}
			else
			{
				molat.shieldObject = null;
			}
			

			molat.maxHealth = health;
			molat.power = power;
			molat.speed = speed;

			molat.canAttack = canAttack;
			molat.canBlock = canBlock;

			//only set colors on AI
			molat.SetTeamNumber(teamNum);

		}

		if (molatAIController)
		{
			molatAIController.healingStationController = healingStationController;
			molatAIController.courage = courage;
			molatAIController.skill = skill;
		}
	}

	public void SpawnHealingStation(Vector3 spawnPoint)
	{
		healingStationController.SpawnHealingStation(new Vector3(spawnPoint.x,0.03f,spawnPoint.z));
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
		HealingStation station = currentObject.GetComponent<HealingStation>();
		if (station)
		{
			healingStationController.DeleteStation(station);
		}
		else if (currentObject.GetComponent<Molat>() || currentObject.GetComponent<Item>())
		{
			Destroy(currentObject);
		}
	}

	public void SpawnMyselfIn(Vector3 spawnPoint)
	{
		meSpawnedIn = Instantiate(_PlayerPrefab, spawnPoint, Quaternion.LookRotation(Vector3.right, Vector3.up));
		meSpawnedIn.tag = "Player";
		Molat playerMolat = meSpawnedIn.GetComponent<Molat>();
		InitiateAllParameters(playerMolat, null);
		SwapPlayerMode(true);
		InGameUI.GetComponentInChildren<PlayerHealthBar>().player = playerMolat;
		InGameUI.GetComponentInChildren<PlayerCDBar>().player = playerMolat;
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
		mouseAndRaycast.lookingForObject = false;
		mouseAndRaycast.lookingForItem = false;

		OnOptionClick(UIButtonSelection.Spawn);
	}
	public void SpawnMacePressed()
	{
		mouseAndRaycast.lookingForObject = false;
		mouseAndRaycast.lookingForItem = false;
		OnOptionClick(UIButtonSelection.SpawnMace);
	}
	public void SpawnShieldPressed()
	{
		mouseAndRaycast.lookingForObject = false;
		mouseAndRaycast.lookingForItem = false;
		OnOptionClick(UIButtonSelection.SpawnShield);
	}
	public void SpawnHealingPressed()
	{
		mouseAndRaycast.lookingForObject = false;
		mouseAndRaycast.lookingForItem = false;
		OnOptionClick(UIButtonSelection.SpawnHealing);
	}
	public void SpawnSelfPressed()
	{
		mouseAndRaycast.lookingForObject = false;
		mouseAndRaycast.lookingForItem = false;
		OnOptionClick(UIButtonSelection.SpawnMe);
	}
	public void DestroyObjectPressed()
	{
		mouseAndRaycast.lookingForObject = true;
		mouseAndRaycast.lookingForItem = true;
		OnOptionClick(UIButtonSelection.Destroy);
	}

	public void OnOptionClick(UIButtonSelection buttonSelection)
	{
		mouseAndRaycast.SelectedButton(buttonSelection);
		if (!hasPressedButton)
		{
			hasPressedButton = true;
			toolTipText.text = "Now click the terrain to spawn";
		}
		currentSelection = buttonSelection;
	}
	//INPUT FIELD SETTERS
	public void SetTeam(Int32 teamNum)
	{
		this.teamNum = teamNum;
	}
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
	public void SetCourage(float courage)
	{
		this.courage = courage;
	}
	public void SetSkill(float skill)
	{
		this.skill = skill;
	}

	public void SetCanAttack(bool canAttack)
	{
		print(canAttack);
		this.canAttack = canAttack;
	}
	public void SetCanBlock(bool canBlock)
	{
		this.canBlock = canBlock;
	}

	public void HasWeapon(bool hasWeapon)
	{
		this.hasWeapon = hasWeapon;
	}
	public void HasShield(bool hasShield)
	{
		this.hasShield = hasShield;
	}
	public void SetWeaponDamage(string weaponDamage)
	{
		myWeapon.baseDamage = float.Parse(weaponDamage);
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
