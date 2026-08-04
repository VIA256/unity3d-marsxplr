using System;
using UnityEngine;

[Serializable]
public class Init : MonoBehaviour
{
	public GUIText txt;

	public void Update()
    {
		float i = Application.GetStreamProgressForLevel(Application.loadedLevel + 1);
		if (i == 1f)
        {
			Application.LoadLevel(Application.loadedLevel + 1);
		}
		else
        {
			txt.text = Mathf.RoundToInt(i * 100f) + "%";
		}
	}
}
