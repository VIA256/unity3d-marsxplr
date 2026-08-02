using System;
using UnityEngine;

[Serializable]
public class VehicleTrigger : MonoBehaviour
{
	private Vehicle vehicle;

	public void Start()
	{
		vehicle = (Vehicle)transform.root.GetComponent(typeof(Vehicle));
	}

	public void OnTriggerStay(Collider other)
	{
		if (
            other.gameObject.layer != 14 &&
            vehicle.GetComponent<NetworkView>().isMine &&
            (bool)other.attachedRigidbody)
		{
			vehicle.OnRam(other.attachedRigidbody.gameObject);
		}
	}
}
