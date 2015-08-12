using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ManageGamesManager : MonoBehaviour
{
	public Toggle[] GameItems;
	private XML.WebResponse_Game[] gameList;

	public GameObject ClientMainScreenCanvas, CreateGameCanvas;
	public Button BackButton, CreateGameButton, RefreshButton, DeleteGameButton;
	public MessageBoxManager MessageBox;

	public static ManageGamesManager Singleton;
	void Start()
	{
		Singleton = this;

		BackButton.onClick.AddListener(back_Clicked);
		CreateGameButton.onClick.AddListener(createGame_Clicked);
		RefreshButton.onClick.AddListener(refresh_Clicked);
		DeleteGameButton.onClick.AddListener(deleteGame_Clicked);

		refresh_Clicked();
	}

	private void back_Clicked()
	{
		ClientMainScreenCanvas.SetActive(true);
		this.gameObject.SetActive(false);
	}

	private void createGame_Clicked()
	{
		CreateGameCanvas.SetActive(true);
		this.gameObject.SetActive(false);
	}

	public static void Refresh()
	{
		if (Singleton != null) Singleton.refresh_Clicked();
	}

	bool refreshing;
	private void refresh_Clicked()
	{
		if (refreshing) return;
		refreshing = true;

		// clear list
		foreach (var item in GameItems)
		{
			item.gameObject.SetActive(false);
			item.isOn = false;
		}

		// get new list
		ServicesHelper.InvokeServiceMethod(ServiceTypes.Clients, "GetGameList", getGameListCallback, "client_id="+LoginManager.ClientID);
	}

	private void getGameListCallback(bool succeeded, XML.WebResponse response)
	{
		if (succeeded)
		{
			if (response.Games == null)
			{
				Debug.LogError("Failed to refresh games: Null games object");
				return;
			}

			Debug.Log("Recieved games list of count: " + response.Games.Count);
			gameList = response.Games.ToArray();
			for (int i = 0; i != gameList.Length; ++i)
			{
				if (i >= GameItems.Length) break;

				var item = GameItems[i];
				var label = item.transform.GetChild(1).GetComponent<Text>();
				item.gameObject.SetActive(true);
				if (label != null)
				{
					label.text = gameList[i].Name;
				}
				else
				{
					Debug.LogError("Game List item label is null: " + i);
					label.text = "ERROR: ???";
				}
			}

			Debug.Log("Games Refreshed!");
		}
		else
		{
			Debug.LogError("Failed to refresh games");
		}

		refreshing = false;
	}

	private void deleteGame_Clicked()
	{
		MessageBoxManager.Show(MessageBox, messageBoxCallback);
	}

	private void messageBoxCallback(bool isOK)
	{
		if (!isOK) return;

		int selectedGame = -1;
		for (int i = 0; i != GameItems.Length; ++i)
		{
			if (GameItems[i].isOn)
			{
				selectedGame = i;
				break;
			}
		}

		if (selectedGame == -1)
		{
			Debug.LogError("No game is selected!");
			return;
		}

		ServicesHelper.InvokeServiceMethod(ServiceTypes.Clients, "DeleteGame", deleteGameCallback, "game_id="+gameList[selectedGame].ID);
	}

	private void deleteGameCallback(bool succeeded, XML.WebResponse response)
	{
		if (succeeded)
		{
			Debug.Log("Game deleted!");
			refresh_Clicked();
		}
		else
		{
			Debug.LogError("Failed to delete Game");
		}
	}
}
