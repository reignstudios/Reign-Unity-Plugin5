using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MessageBoxManager : MonoBehaviour
{
	public Button OkButton, CancelButton;

	public delegate void CallbackMethod(bool isOK);
	private static CallbackMethod callback;

	void Start()
	{
		OkButton.onClick.AddListener(okClick);
		CancelButton.onClick.AddListener(cancelClick);
	}

	private void cancelClick()
	{
		if (callback != null) callback(false);
		gameObject.SetActive(false);
	}

	private void okClick()
	{
		if (callback != null) callback(true);
		gameObject.SetActive(false);
	}

	public static void Show(MessageBoxManager messageBox, CallbackMethod callback)
	{
		MessageBoxManager.callback = callback;
		messageBox.gameObject.SetActive(true);
	}
}
