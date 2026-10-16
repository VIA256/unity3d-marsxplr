using System;
using UnityEngine;

[Serializable]
public class JetThruster : MonoBehaviour
{
	public Vehicle vehicle;
	public ParticleSystemRenderer particleRenderer;

	public void Start()
	{
		vehicle = (Vehicle)gameObject.transform.root.gameObject.GetComponentInChildren(typeof(Vehicle));
		if (!vehicle)
		{
            Destroy(this);
		}
		particleRenderer = (ParticleSystemRenderer)gameObject.GetComponent("ParticleSystemRenderer");
	}

	public void FixedUpdate()
	{
       	ParticleSystem.VelocityOverLifetimeModule locvel = GetComponent<ParticleSystem>().velocityOverLifetime;
        locvel.x = vehicle.input.x;
        locvel.y = vehicle.input.y;
        locvel.z = Mathf.Min(-10.0f * vehicle.input.z, -0.5f);
		
        if (locvel.z.constant >= -1f)
		{
			particleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
		}
		else
		{
			particleRenderer.renderMode = ParticleSystemRenderMode.Stretch;
		}
	}
}
