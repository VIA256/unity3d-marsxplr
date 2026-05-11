using UnityEngine;
using System.Collections;
using UnityEditor;

public class ClearPrefs : MonoBehaviour {
	
	[MenuItem("Assets/Clear PlayerPrefs")]
	static void clear () {
		PlayerPrefs.DeleteAll();
		Debug.Log ("PlayerPrefs have been cleared");
	}
}
