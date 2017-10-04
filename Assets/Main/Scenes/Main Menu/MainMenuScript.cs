using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour {
	string arenaSceneName = "Arena";
	string singlePlayerSceneName = "Single Player Demo";

	public void ChangeSceneToArena()
	{
		SceneManager.LoadScene(arenaSceneName, LoadSceneMode.Single);
	}

	public void ChangeSceneToSinglePlayer()
	{
		SceneManager.LoadScene(singlePlayerSceneName, LoadSceneMode.Single);
	}

	public void QuitGame()
	{
		Application.Quit();
	}
}
