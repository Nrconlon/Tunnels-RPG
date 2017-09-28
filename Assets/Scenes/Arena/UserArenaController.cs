using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UserArenaController : MonoBehaviour {

	[SerializeField] GameObject AIPrefab;
	[SerializeField] GameObject optionsMenuButton;
	[SerializeField] GameObject returnToControlButton;
	bool inGame = false;
	string mainMenuName = "Main Menu";


	public delegate void OnOptionSelected(ButtonSelection buttonSelection); // declare new delegate type
	public event OnOptionSelected OptionSelectedDel; // instantiate an observer set

	// Use this for initialization
	void Start ()
	{
		optionsMenuButton.SetActive(false);
		returnToControlButton.SetActive(false);
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
		Instantiate(AIPrefab, spawnPoint, Quaternion.LookRotation(Vector3.zero,Vector3.up));
	}

	public void DestroyAICharacter(Vector3 molatTarget)
	{
		
	}

	public void SpawnMyselfIn(Vector3 spawnPoint)
	{

	}

	public void TakeControlOfUnit(GameObject molatTarget)
	{

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
