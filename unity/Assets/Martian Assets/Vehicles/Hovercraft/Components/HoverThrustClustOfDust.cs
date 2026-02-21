using System;
using UnityEngine;

[Serializable]
public class HoverThrustClustOfDust : MonoBehaviour
{
	public LayerMask thrustMask;

	public HoverThrustClustOfDust()
    {
		thrustMask = -1;
	}

	public void FixedUpdate()
    {
		if (
			transform.position.y < Game.Settings.lavaAlt + 15f ||
			Physics.Raycast(transform.position, Vector3.down, 15f, thrustMask)
		)
        {
			particleEmitter.emit = true;
		}
		else
        {
			particleEmitter.emit = false;
		}
	}
}
