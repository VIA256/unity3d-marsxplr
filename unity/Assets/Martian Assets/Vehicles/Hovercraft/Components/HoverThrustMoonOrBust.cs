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
		Vector3 localVelocity = GetComponent<ParticleEmitter>().localVelocity;
		localVelocity.x = x;
		GetComponent<ParticleEmitter>().localVelocity = localVelocity;
		
		float z = Mathf.Min(-10f * vehicle.input.y, -0.5f);
		localVelocity = GetComponent<ParticleEmitter>().localVelocity;
		localVelocity.z = z;
		GetComponent<ParticleEmitter>().localVelocity = localVelocity;
		
		if (GetComponent<ParticleEmitter>().localVelocity.z >= -1f)
        {
			particleRenderer.particleRenderMode = ParticleRenderMode.Billboard;
		}
		else
        {
			particleRenderer.particleRenderMode = ParticleRenderMode.Stretch;
		}
	}
}
