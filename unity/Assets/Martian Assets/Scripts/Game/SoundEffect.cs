using System;
using UnityEngine;

[Serializable]
public class SoundEffect : MonoBehaviour
{
	public void Awake()
	{
		if (!Game.Settings.useSfx)
		{
            Destroy(gameObject.GetComponent(typeof(AudioSource)));
		}
	}
}
