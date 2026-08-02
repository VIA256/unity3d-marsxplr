using System;
using UnityEngine;

[Serializable]
public class JetThruster : MonoBehaviour
{
	public Vehicle vehicle;
	public ParticleRenderer particleRenderer;

	public void Start()
	{
		vehicle = (Vehicle)gameObject.transform.root.gameObject.GetComponentInChildren(typeof(Vehicle));
		if (!vehicle)
		{
            Destroy(this);
		}
		particleRenderer = (ParticleRenderer)gameObject.GetComponent("ParticleRenderer");
	}

	public void FixedUpdate()
	{
        Vector3 locvel = particleEmitter.localVelocity;
        locvel.x = vehicle.input.x;
        locvel.y = vehicle.input.y;
        locvel.z = Mathf.Min(-10.0f * vehicle.input.z, -0.5f);
        particleEmitter.localVelocity = locvel;
		
        if (particleEmitter.localVelocity.z >= -1f)
		{
			particleRenderer.particleRenderMode = ParticleRenderMode.Billboard;
		}
		else
		{
			particleRenderer.particleRenderMode = ParticleRenderMode.Stretch;
		}
	}
}
