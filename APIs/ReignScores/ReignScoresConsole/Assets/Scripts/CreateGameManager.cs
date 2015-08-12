using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CreateGameManager : MonoBehaviour
{
	public GameObject ManageGamesCanvas, ClientMainScreenCanvas;
	public Button CancelButton, CreateButton;
	public InputField NameField;

	void Start()
	{
		CancelButton.onClick.AddListener(cancel_Clicked);
		CreateButton.onClick.AddListener(createGame_Clicked);
	}

	private void cancel_Clicked()
	{
		ClientMainScreenCanvas.SetActive(true);
		this.gameObject.SetActive(false);
	}

	private void createGame_Clicked()
	{
		// validate input fields
		if (NameField.text.Length < 4)
		{
			Debug.LogError("Name must be at least 4 characters long!");
			return;
		}

		// invoke web method
		ServicesHelper.InvokeServiceMethod(ServiceTypes.Clients, "CreateGame", createCallback, "client_id="+LoginManager.ClientID, "name="+NameField.text);
	}

	private void createCallback(bool succeeded, XML.WebResponse response)
	{
		if (succeeded)
		{
			Debug.Log("Game created!");
			ManageGamesCanvas.SetActive(true);
			this.gameObject.SetActive(false);
			ManageGamesManager.Refresh();
		}
		else
		{
			Debug.LogError("Failed to create Game");
		}
	}
}
