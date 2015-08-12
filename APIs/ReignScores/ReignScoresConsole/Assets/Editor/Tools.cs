using UnityEngine;
using UnityEditor;
using System;

public static class Tools
{
	[MenuItem("Tools/Generate Guid")]
	static void generateGuid()
	{
		Debug.Log("New ID: " + Guid.NewGuid());
	}
}
