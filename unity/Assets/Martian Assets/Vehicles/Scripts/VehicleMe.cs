using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class VehicleMe : MonoBehaviour
{
	public Vehicle vehicle;
	private float rocketFireTime = 0.00f;

	public void Update()
	{
        //Input Assignments
		if (
            Input.GetButtonDown("Jump") &&
            (!Game.Messaging || !Game.Messaging.chatting) &&
            Time.time > Game.Controller.kpTime)
		{
			GetComponent<NetworkView>().RPC(
                "sI",
                RPCMode.All,
                vehicle.specialInput ? false : true);
			Game.Controller.kpTime = Time.time + Game.Controller.kpDur;
		}
		if (Input.GetButton("Fire3") && !vehicle.brakes)
		{
			GetComponent<NetworkView>().RPC("sB", RPCMode.All, true);
		}
		else if (!Input.GetButton("Fire3") && vehicle.brakes)
		{
			GetComponent<NetworkView>().RPC("sB", RPCMode.All, false);
		}
		vehicle.input.x = Input.GetAxis("Horizontal");
		vehicle.input.y = Input.GetAxis("Vertical");
		vehicle.input.z = Input.GetAxis("Throttle");
		vehicle.input.w = Input.GetAxis("Yaw");
		if (vehicle.inputThrottle)
		{
			vehicle.input.z = (vehicle.input.z + 1f) * 0.5f;
		}
		else
		{
			if (vehicle.input.x > 0.1f * -1f && vehicle.input.x < 0.1f)
			{
				vehicle.input.x = vehicle.input.w;
			}
			if (vehicle.input.y > 0.1f * -1f && vehicle.input.y < 0.1f)
			{
				vehicle.input.y = vehicle.input.z;
			}
		}

        //Weapons Locking
		GameObject laserLock;
		if (Game.Settings.laserLocking)
		{
			RaycastHit hit = default(RaycastHit);
			if (
                Physics.Raycast(
                    transform.position + transform.forward * ((
                        Game.Settings.laserLock[vehicle.vehId] +
                        (float)vehicle.camOffset *
                        0.1f) * 15f),
                    transform.forward,
                    out hit,
                    Mathf.Infinity,
                    1 << 14) &&
                (vehicle.isIt != 0 ||
                    ((Vehicle)hit.transform
                        .gameObject.GetComponent(typeof(Vehicle))
                        ).isIt != 0))
			{
				laserLock = hit.transform.gameObject;
				vehicle.laserAimer.SetActive(false);
				vehicle.laserAimerLocked.SetActive(true);
			}
			else
			{
				laserLock = null;
				vehicle.laserAimer.SetActive(true);
				vehicle.laserAimerLocked.SetActive(false);
			}
		}
		else
		{
            vehicle.laserAimer.SetActive (false);
			laserLock = null;
		}

        //Firing
		if (
            (bool)vehicle.ridePos &&
            Game.Settings.lasersAllowed &&
            rocketFireTime < Time.time &&
            Game.Settings.firepower[vehicle.vehId] > 0 &&
		    (laserLock || 							        //Autofiring With Weapons Lock
			    (Input.GetButton("Fire1") &&
                    !Input.GetMouseButton(0)) ||	        //Firing with Joystick
			    (Input.GetButton("Fire1") &&
                    (Input.GetButton("Fire2") ||
                    Input.GetButton("Snipe") ||
                    Game.Settings.camMode == 0)) ||		    //Firing with mouse while inside vehicle
			    (Input.GetButton("Fire1") &&
                    ((Input.mousePosition.x > Screen.width * .25 &&
                        Input.mousePosition.x < Screen.width - 200)))	//Firing with mouse while outside vehicle
		    ))
		{
			bool snipe;
			if ((bool)laserLock)
			{
                snipe = (Game.Settings.firepower[vehicle.vehId] > 2 ||
                    (Game.Settings.firepower[vehicle.vehId] > 1 &&
                        (laserLock.GetComponent<Rigidbody>().velocity.sqrMagnitude > 500 ||
                            Vector3.Distance(
                                transform.position,
                                laserLock.transform.position) > 500)));
                GetComponent<NetworkView>().RPC(
                    (snipe ? "fSl" : "fRl"),
                    RPCMode.All,
                    GetComponent<NetworkView>().viewID,
                    Network.time.ToString(),
                    vehicle.ridePos.position + vehicle.transform.up * -0.1f,
                    laserLock.GetComponent<NetworkView>().viewID);
			}
			else
			{
                Quaternion rang;
                if (
                    Game.Settings.camMode == 0 ||
                    Input.GetButton("Fire2") ||
                    Input.GetButton("Snipe"))
                {
                    rang = Camera.main.transform.rotation;
                }
                else rang = vehicle.ridePos.rotation;
			    snipe = (
                    (Input.GetButton("Snipe") &&
                        Game.Settings.firepower[vehicle.vehId] > 1) ||
                    Game.Settings.firepower[vehicle.vehId] > 2);
			    GetComponent<NetworkView>().RPC(
                    (snipe ? "fS" : "fR"),
                    RPCMode.All,
                    GetComponent<NetworkView>().viewID,
                    Network.time.ToString(),
                    vehicle.ridePos.position + vehicle.transform.up * -0.1f,
                    rang.eulerAngles);
			}
            rocketFireTime = Time.time + ((snipe ?
                (Game.Settings.firepower[vehicle.vehId] > 2f ?
                    0.5f :
                    2f) :
                0.25f));
		}

        //Bounds Checking
		if (vehicle.myRigidbody.position.y < -300f)
		{
			vehicle.myRigidbody.velocity = vehicle.myRigidbody.velocity.normalized;
			vehicle.myRigidbody.isKinematic = true;
			vehicle.transform.position = World.baseTF.position;
			vehicle.myRigidbody.isKinematic = false;
			if ((bool)Game.Messaging)
			{
				Game.Messaging.broadcast(name + " fell off the planet");
			}
		}
		if (vehicle.myRigidbody.velocity.magnitude > 500f)
		{
			vehicle.myRigidbody.velocity = vehicle.myRigidbody.velocity * 0.5f;
		}
	}

	public void FixedUpdate()
	{
		if (
            (bool)vehicle.ramoSphere &&
            vehicle.zorbBall &&
            (vehicle.input.y != 0f || vehicle.input.x != 0f))
		{
			GetComponent<Rigidbody>().AddForce(
                Vector3.Scale(
                    new Vector3(1f, 0f, 1f),
                    Camera.main.transform.TransformDirection(new Vector3(
                        vehicle.input.x * Mathf.Max(
                            0f,
                            Game.Settings.zorbSpeed + Game.Settings.zorbAgility),
                        0f,
                        vehicle.input.y * Game.Settings.zorbSpeed))),
                ForceMode.Acceleration);
			GetComponent<Rigidbody>().AddTorque(
                Camera.main.transform.TransformDirection(new Vector3(
                    vehicle.input.y,
                    0f,
                    vehicle.input.x * -1f)) * Game.Settings.zorbSpeed,
                ForceMode.Acceleration);
		}
	}

	public void OnPrefsUpdated()
	{
		if (Game.Settings.renderLevel > 4)
		{
			Light lt = (Light)gameObject.GetComponentInChildren(typeof(Light));
			if ((bool)lt) lt.enabled = true;
		}
		if (Game.Settings.renderLevel > 3)
		{
            foreach (TrailRenderer tr in gameObject.GetComponentsInChildren(typeof(TrailRenderer)))
            {
                tr.enabled = true;
            }
		}
	}
}
