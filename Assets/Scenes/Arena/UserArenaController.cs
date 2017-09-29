using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UserArenaController : MonoBehaviour {

	[SerializeField] GameObject _AIPrefab;
	[SerializeField] GameObject _PlayerPrefab;
	[SerializeField] GameObject _WeaponPrefab;
	[SerializeField] GameObject _ClawPrefab;
	[SerializeField] GameObject optionsMenuButton;
	[SerializeField] GameObject returnToControlButton;
	[SerializeField] Transform clonePrefabSpawn;
	bool inGame = false;
	string mainMenuName = "Main Menu";
	private GameObject weaponClone;
	private GameObject clawClone;
	private Weapon myClaw;
	private Weapon myWeapon;

	[Header("Spawn In Stats")]
	[SerializeField] float health = 100f;




	private void InitiateAllParameters(Molat molat, MolatAIController molatAIController)
	{
		if(molat)
		{

		}
		
		if(molatAIController)
		{

		}
	}



	public delegate void OnOptionSelected(ButtonSelection buttonSelection); // declare new delegate type
	public event OnOptionSelected OptionSelectedDel; // instantiate an observer set

	// Use this for initialization
	void Start ()
	{
		optionsMenuButton.SetActive(false);
		returnToControlButton.SetActive(false);
		clawClone = Instantiate(_ClawPrefab, clonePrefabSpawn);
		myClaw = clawClone.GetComponent<Weapon>();
		myClaw.instigator = gameObject;

		weaponClone = Instantiate(_WeaponPrefab, clonePrefabSpawn);
		myWeapon = weaponClone.GetComponent<Weapon>();
		myWeapon.instigator = gameObject;

	}

	// Update is called once per frame
	void Update () {
		
	}

	public void LoseArenaControl()
	{

	}

	//Called from elsewhere
	public void SpawnAICharacter(Vector3 spawnPoint)
	{
		GameObject PrefabCopy = Instantiate(_AIPrefab, spawnPoint, Quaternion.LookRotation(Vector3.zero,Vector3.up));
		InitiateAllParameters(PrefabCopy.GetComponent<Molat>(), PrefabCopy.GetComponent<MolatAIController>());
	}

	public void DestroyAICharacter(GameObject molatTarget)
	{
		Destroy(molatTarget);
	}

	public void SpawnMyselfIn(Vector3 spawnPoint)
	{
		GameObject PrefabCopy = Instantiate(_PlayerPrefab, spawnPoint, Quaternion.LookRotation(Vector3.zero, Vector3.up));
		InitiateAllParameters(PrefabCopy.GetComponent<Molat>(), null);
	}

	public void TakeControlOfUnit(GameObject molatTarget)
	{
		MolatAIController aiComponent = molatTarget.GetComponent<MolatAIController>();
		Destroy(aiComponent);
		molatTarget.AddComponent<MolatPlayerController>();
		//TODO Swap Cameras, swap canvases
	}

	//Called from buttons on press
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
	}

	public void RegainArenaControl()
	{

	}

	public void QuitToMainMenu()
	{
		SceneManager.LoadScene(mainMenuName, LoadSceneMode.Single);
	}

	public void QuitToDesktop()
	{
		Application.Quit();
	}


	public void OnOptionClick(ButtonSelection buttonSelection)
	{
		if(OptionSelectedDel != null)
		{
			OptionSelectedDel(buttonSelection);
		}
	}
}

public enum ButtonSelection {Spawn, Control, SpawnMe, Destroy}
