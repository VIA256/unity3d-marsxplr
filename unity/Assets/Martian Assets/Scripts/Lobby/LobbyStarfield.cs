using System;
using UnityEngine;

[Serializable]
public class LobbyStarfield : MonoBehaviour
{
	public Lobby Lobby;

	public void Update()
	{
		if (Application.loadedLevel == 1 && Lobby.GUIHide > 0f)
		{
            Color col =
                ((ParticleRenderer)gameObject.GetComponent(typeof(ParticleRenderer)))
                .material
                .GetColor("_TintColor");
			col.a = 0.5f - Lobby.GUIHide / 2f;
            ((ParticleRenderer)gameObject.GetComponent(typeof(ParticleRenderer))).material.SetColor("_TintColor", col);
		}
		else if (Application.loadedLevel > 1)
		{
			Vector3 pewv = GetComponent<ParticleEmitter>().worldVelocity;
			pewv.y = Mathf.Min(Time.timeSinceLevelLoad, 5f) * -1f;
			GetComponent<ParticleEmitter>().worldVelocity = pewv;
			if (Time.timeSinceLevelLoad > 7f || QualitySettings.GetQualityLevel() < 3)
			{
				GetComponent<ParticleEmitter>().emit = false;
			}
		}
	}
}
