using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class VehicleData : MonoBehaviour
{
	public int camOffset = 2;
	public Transform ridePos;
	public int mass;
	public float drag;
	public float angularDrag;
	public string shortName;
	public bool inputThrottle;

	public MeshRenderer[] materialMain;
	public MeshRenderer[] materialAccent;

	public void InitVehicle(Vehicle veh)
	{
		veh.camOffset = camOffset;
		veh.ridePos = ridePos;
		veh.shortName = shortName;
		veh.myRigidbody.mass = mass;
		veh.myRigidbody.drag = drag;
		veh.myRigidbody.angularDrag = angularDrag;
		if (materialMain.Length > 0)
		{
            List<Material> mats = new List<Material>();
            foreach (MeshRenderer mat in materialMain)
            {
                mats.Add(mat.material);
            }
            veh.materialMain = mats.ToArray();
		}
		if (materialAccent.Length > 0)
		{
            List<Material> mats = new List<Material>();
            foreach (MeshRenderer mat in materialAccent)
            {
                mats.Add(mat.material);
                veh.materialAccent = mats.ToArray();
            }
		}
		veh.inputThrottle = inputThrottle;
		Destroy(this);
	}
}
