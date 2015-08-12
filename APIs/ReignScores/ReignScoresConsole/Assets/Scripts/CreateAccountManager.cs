using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CreateAccountManager : MonoBehaviour
{
	public GameObject LoginCanvas;
	public Button CancelButton, CreateButton;
	public InputField UsernameField, EmailField, PasswordField, Password2Field;

	void Start()
	{
		CancelButton.onClick.AddListener(cancel_Clicked);
		CreateButton.onClick.AddListener(create_Clicked);
	}

	private void cancel_Clicked()
	{
		LoginCanvas.SetActive(true);
		this.gameObject.SetActive(false);
	}

	private void create_Clicked()
	{
		// validate input fields
		if (UsernameField.text.Length < 6)
		{
			Debug.LogError("Username must be at least 6 characters long!");
			return;
		}

		if (PasswordField.text.Length < 6)
		{
			Debug.LogError("Password must be at least 6 characters long!");
			return;
		}

		if (PasswordField.text.Length != Password2Field.text.Length)
		{
			Debug.LogError("Password lenghts do not match!");
			return;
		}

		if (PasswordField.text != Password2Field.text)
		{
			Debug.LogError("Passwords do not match!");
			return;
		}

		// invoke web method
		ServicesHelper.InvokeServiceMethod(ServiceTypes.Managers, "CreateClient", createCallback, "username="+UsernameField.text, "email="+EmailField.text, "password="+PasswordField.text);
	}

	private void createCallback(bool succeeded, XML.WebResponse response)
	{
		if (succeeded)
		{
			Debug.Log("Client created!");
			LoginCanvas.SetActive(true);
			this.gameObject.SetActive(false);
		}
		else
		{
			Debug.LogError("Failed to create Client");
		}
	}
}
