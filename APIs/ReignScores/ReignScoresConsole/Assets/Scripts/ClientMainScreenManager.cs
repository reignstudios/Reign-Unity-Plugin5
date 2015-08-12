using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ClientMainScreenManager : MonoBehaviour
{
	public GameObject ManageGamesCanvas, CreateGameCanvas;
	public Button ManagerGamesButton, CreateGameButton;

	void Start()
	{
		ManagerGamesButton.onClick.AddListener(manageGames_Clicked);
		CreateGameButton.onClick.AddListener(createGame_Clicked);
	}

	private void manageGames_Clicked()
	{
		ManageGamesCanvas.SetActive(true);
		this.gameObject.SetActive(false);
		ManageGamesManager.Refresh();
	}

	private void createGame_Clicked()
	{
		CreateGameCanvas.SetActive(true);
		this.gameObject.SetActive(false);
	}
}
