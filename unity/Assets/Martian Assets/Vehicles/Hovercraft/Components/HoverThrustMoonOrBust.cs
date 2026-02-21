using System;
using UnityEngine;

[Serializable]
public class HoverThrustMoonOrBust : MonoBehaviour
{
	public Vehicle vehicle;
	public ParticleRenderer particleRenderer;

	public void Start()
    {
		vehicle = (Vehicle)gameObject.transform.root.gameObject.GetComponentInChildren(typeof(Vehicle));
		particleRenderer = (ParticleRenderer)gameObject.GetComponent("ParticleRenderer");
	}

	public void FixedUpdate()
    {
		float x = (float)((vehicle.input.y == 0f) ? 2 : 5) * vehicle.input.x;
		Vector3 localVelocity = particleEmitter.localVelocity;
		localVelocity.x = x;
		particleEmitter.localVelocity = localVelocity;
		
		float z = Mathf.Min(-10f * vehicle.input.y, -0.5f);
		localVelocity = particleEmitter.localVelocity;
		localVelocity.z = z;
		particleEmitter.localVelocity = localVelocity;
		
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
