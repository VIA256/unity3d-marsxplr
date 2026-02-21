using System;
using UnityEngine;

[Serializable]
public class GameData : MonoBehaviour
{
	//public static float gameVersion = 4.0f;
	public static SemVer gameVersion = new SemVer("4.0.3");
	public static float serverVersion = 0.2f;
	public static string gameName = "marsxplr";
	public static string userName = "";
	public static string userCode = "";
	public static string errorMessage = "";
	public static string masterBlacklist = "";
	public static GameWorldDesc[] gameWorlds;
	public static int networkMode = 0;
}
