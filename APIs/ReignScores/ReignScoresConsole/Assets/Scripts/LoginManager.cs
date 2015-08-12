using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class LoginManager : MonoBehaviour
{
	public static Guid ClientID;

	public GameObject CreateAccountCanvas, ClientMainScreenCanvas;
	public Button CreateAccountButton, LoginButton;
	public InputField UsernameField, PasswordField;

	void Start()
	{
		CreateAccountButton.onClick.AddListener(createAccount_Clicked);
		LoginButton.onClick.AddListener(login_Clicked);
	}

	private void createAccount_Clicked()
	{
		CreateAccountCanvas.SetActive(true);
		this.gameObject.SetActive(false);
	}

	private void login_Clicked()
	{
		// validate input fields
		if (UsernameField.text.Length <= 5)
		{
			Debug.LogError("Username must be at least 6 characters long!");
			return;
		}

		if (PasswordField.text.Length <= 5)
		{
			Debug.LogError("Password must be at least 6 characters long!");
			return;
		}

		// invoke web method
		ServicesHelper.InvokeServiceMethod(ServiceTypes.Clients, "Login", loginCallback, "username="+UsernameField.text, "password="+PasswordField.text);
	}

	private void loginCallback(bool succeeded, XML.WebResponse response)
	{
		if (succeeded)
		{
			Debug.Log("Logged In with ClientID: " + response.ClientID);
			ClientID = new Guid(response.ClientID);
			ClientMainScreenCanvas.SetActive(true);
			this.gameObject.SetActive(false);
		}
		else
		{
			Debug.LogError("Failed to login");
		}
	}
}
