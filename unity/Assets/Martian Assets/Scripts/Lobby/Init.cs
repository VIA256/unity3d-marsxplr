using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class Init : MonoBehaviour
{
	public void Start()
    {
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
	}
}
