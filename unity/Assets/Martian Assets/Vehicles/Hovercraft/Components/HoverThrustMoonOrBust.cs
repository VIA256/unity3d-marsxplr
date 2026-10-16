using System;
using UnityEngine;

[Serializable]
public class HoverThrustMoonOrBust : MonoBehaviour
{
	public Vehicle vehicle;
	public ParticleSystemRenderer particleRenderer;

	public void Start()
    {
		vehicle = (Vehicle)gameObject.transform.root.gameObject.GetComponentInChildren(typeof(Vehicle));
		particleRenderer = (ParticleSystemRenderer)gameObject.GetComponent("ParticleSystemRenderer");
	}

	public void FixedUpdate()
    {
		ParticleSystem pS = GetComponent<ParticleSystem>();
		
		ParticleSystem.VelocityOverLifetimeModule localVelocity = pS.velocityOverLifetime;
		localVelocity.x = (float)((vehicle.input.y == 0f) ? 2 : 5) * vehicle.input.x;
		localVelocity.z = Mathf.Min(-10f * vehicle.input.y, -0.5f);
		
		if (pS.velocityOverLifetime.z.constant >= -1f)
        {
			particleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
		}
		else
        {
			particleRenderer.renderMode = ParticleSystemRenderMode.Stretch;
		}
	}
}
