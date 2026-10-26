using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class Init : MonoBehaviour
{
	public void Start()
    {
		QualitySettings.SetQualityLevel(6);//Fantastic
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
	}
}
