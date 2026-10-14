using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class LobbyStarfield : MonoBehaviour
{
	public Lobby Lobby;

	public void Update()
	{
        int loadedLevel = SceneManager.GetActiveScene().buildIndex;
		if (loadedLevel == 1 && Lobby.GUIHide > 0f)
		{
            Color col =
                ((ParticleSystemRenderer)gameObject.GetComponent(typeof(ParticleSystemRenderer)))
                .material
                .GetColor("_TintColor");
			col.a = 0.5f - Lobby.GUIHide / 2f;
            ((ParticleSystemRenderer)gameObject.GetComponent(typeof(ParticleSystemRenderer))).material.SetColor("_TintColor", col);
		}
		else if (loadedLevel > 1)
		{
			ParticleSystem.VelocityOverLifetimeModule vol = GetComponent<ParticleSystem>().velocityOverLifetime;
			vol.y = Mathf.Min(Time.timeSinceLevelLoad, 5f) * -1f;
			if (Time.timeSinceLevelLoad > 7f || QualitySettings.GetQualityLevel() < 3)
			{
				GetComponent<ParticleSystem>().Stop();
			}
		}
	}
}
