using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class Init : MonoBehaviour
{
	public GUIText txt;

	public void Update()
    {
        int loadedLevel = SceneManager.GetActiveScene().buildIndex;
		float i = Application.GetStreamProgressForLevel(loadedLevel + 1);
		if (i == 1f)
        {
			SceneManager.LoadScene(loadedLevel + 1);
		}
		else
        {
			txt.text = Mathf.RoundToInt(i * 100f) + "%";
		}
	}
}
