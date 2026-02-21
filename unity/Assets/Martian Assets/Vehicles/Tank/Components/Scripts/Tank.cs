using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Tank : MonoBehaviour
{
	public Vehicle vehicle;
	public GameObject tracks;
	public GameObject tracksSimple;
	public int tracksPerSide = 3;
	public float trackSpacing = 2.5f;
	public GameObject superTracks;
	public GameObject simpleTracks;

	public void InitVehicle(Vehicle veh)
	{
		vehicle = veh;

		List<Material> materialAccents = new List<Material>();

        //Build Tracks
		GameObject track;
		superTracks = new GameObject("Tracks");
		superTracks.transform.parent = transform;
		for (int i = 0; i < tracksPerSide; i++)
		{
            //Left Side
            track = (GameObject)Instantiate(
                tracks,
                transform.TransformPoint(new Vector3(
                    -2,
                    0,
                    -(((tracksPerSide - 1) * trackSpacing) / 2) +
                        i * trackSpacing)),
                transform.rotation);
            materialAccents.Add(
                ((MeshRenderer)track.transform
                .Find("Detailed/Track")
                .GetComponent(typeof(MeshRenderer)))
                .material);
            materialAccents.Add(
                ((MeshRenderer)track.transform
                .Find("Detailed/Tread")
                .GetComponent(typeof(MeshRenderer)))
                .material);
            materialAccents.Add(
                ((MeshRenderer)track.transform
                .Find("Simple")
                .GetComponent(typeof(MeshRenderer)))
                .material);
            track.transform.parent = superTracks.transform;
            //Right Side
            track = (GameObject)Instantiate(
                tracks,
                transform.TransformPoint(new Vector3(
                    2,
                    0,
                    -(((tracksPerSide - 1) * trackSpacing) / 2) +
                        i * trackSpacing)),
                transform.rotation);
            materialAccents.Add(
                ((MeshRenderer)track.transform
                .Find("Detailed/Track")
                .GetComponent(typeof(MeshRenderer)))
                .material);
            materialAccents.Add(
                ((MeshRenderer)track.transform
                .Find("Detailed/Tread")
                .GetComponent(typeof(MeshRenderer)))
                .material);
            materialAccents.Add(
                ((MeshRenderer)track.transform
                .Find("Simple")
                .GetComponent(typeof(MeshRenderer)))
                .material);
			track.transform.parent = superTracks.transform;
            ((TankTrack)track.GetComponent(typeof(TankTrack))).rightSide = true;
		}

		if (!vehicle.networkView.isMine)
		{
			simpleTracks = new GameObject();
			simpleTracks.transform.parent = transform;
			for (int i = 0; i < tracksPerSide; i++)
			{
                //Left Side
                track = (GameObject)Instantiate(
                    tracks,
                    transform.TransformPoint(new Vector3(
                        -2,
                        0.2f,
                        -(((tracksPerSide - 1) * trackSpacing) / 2) +
                            i * trackSpacing)),
                    transform.rotation);
                materialAccents.Add(
                    ((MeshRenderer)track.transform
                    .Find("Detailed/Track")
                    .GetComponent(typeof(MeshRenderer)))
                    .material);
                materialAccents.Add(
                    ((MeshRenderer)track.transform
                    .Find("Detailed/Tread")
                    .GetComponent(typeof(MeshRenderer)))
                    .material);
                materialAccents.Add(
                    ((MeshRenderer)track.transform
                    .Find("Simple")
                    .GetComponent(typeof(MeshRenderer)))
                    .material);
                track.transform.parent = simpleTracks.transform;

                //Right Side
                track = (GameObject)Instantiate(
                    tracks,
                    transform.TransformPoint(new Vector3(
                        2,
                        0.2f,
                        -(((tracksPerSide - 1) * trackSpacing) / 2) +
                            i * trackSpacing)),
                    transform.rotation);
                materialAccents.Add(
                    ((MeshRenderer)track.transform
                    .Find("Detailed/Track")
                    .GetComponent(typeof(MeshRenderer)))
                    .material);
                materialAccents.Add(
                    ((MeshRenderer)track.transform
                    .Find("Detailed/Tread")
                    .GetComponent(typeof(MeshRenderer)))
                    .material);
                materialAccents.Add(
                    ((MeshRenderer)track.transform
                    .Find("Simple")
                    .GetComponent(typeof(MeshRenderer)))
                    .material);
                track.transform.parent = simpleTracks.transform;
			}
		}
		else
		{
			TankMe tankMe = (TankMe)this.gameObject.AddComponent(typeof(TankMe));
			tankMe.vehicle = vehicle;
		}

        vehicle.materialAccent = (Material[])materialAccents.ToArray();
		
        if ((bool)World.baseTF)
        {   //DRAGONHERE: why we need to do this I don't know, but if we don't, we will hover in mid air on local client instances
			transform.position = World.baseTF.position;
		}
        //DRAGONHERE: VERY STRANGE UNITY BUG that sets localposition to -3 if a tank is already present in world...
		transform.localPosition = Vector3.zero;
	}

	public void Update()
	{
        if (!vehicle) return;

		Vector3 centerOfMass = vehicle.myRigidbody.centerOfMass;
        centerOfMass.y = Game.Settings.tankCG;
		vehicle.myRigidbody.centerOfMass = centerOfMass;

		if (!vehicle.networkView.isMine && (bool)vehicle.vehicleNet)
		{
            //Enable advanced physics
			if (vehicle.vehicleNet.simulatePhysics && simpleTracks.active)
			{
                vehicle.myRigidbody.useGravity = true; //Gravity is simulated on the authoratative client instance that owns this tank - it just makes the networked instances jittery
				simpleTracks.SetActiveRecursively(false);
				superTracks.SetActiveRecursively(true);
			}
            //Disable advanced physics
			else if (!vehicle.vehicleNet.simulatePhysics && superTracks.active)
			{
                vehicle.myRigidbody.useGravity = true; //Gravity is simulated on the authoratative client instance that owns this tank - it just makes the networked instances jittery
				simpleTracks.SetActiveRecursively(true);
				superTracks.SetActiveRecursively(false);
			}
		}
	}

	public void OnLOD(int level)
	{
		if ((bool)superTracks)
		{
			foreach(Transform track in superTracks.transform)
			{
                ((MeshRenderer)track
                    .Find("Detailed/Tread")
                    .gameObject.GetComponent(typeof(MeshRenderer))
                    ).enabled = (level == 0);
                ((MeshRenderer)track
                    .Find("Detailed/Track")
                    .gameObject.GetComponent(typeof(MeshRenderer))
                    ).enabled = (level == 0);
                ((MeshRenderer)track
                    .Find("Simple")
                    .gameObject.GetComponent(typeof(MeshRenderer))
                    ).enabled = (level != 0);
			}
		}
		if ((bool)simpleTracks)
		{
            foreach (Transform track in simpleTracks.transform)
            {
                ((MeshRenderer)track
                    .Find("Detailed/Tread")
                    .gameObject.GetComponent(typeof(MeshRenderer))
                    ).enabled = (level == 0);
                ((MeshRenderer)track
                    .Find("Detailed/Track")
                    .gameObject.GetComponent(typeof(MeshRenderer))
                    ).enabled = (level == 0);
                ((MeshRenderer)track
                    .Find("Simple")
                    .gameObject.GetComponent(typeof(MeshRenderer))
                    ).enabled = (level != 0);
            }
		}
	}
}
