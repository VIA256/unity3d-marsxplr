using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class Rocket : MonoBehaviour
{
	public float lag;
	public Transform explosion;
	public Vehicle launchVehicle;
	public Vehicle targetVehicle;
	public int speedX;
	public string laserID;
	private Vector3 strtPos;
	private int stage = -1;
	public LayerMask mask = -1;
	public LayerMask maskOpt = -1;

    public IEnumerator Start()
    { 
        //Init
        name = "lsr#" + laserID;
        float velocity = Game.Settings.laserSpeed * speedX + Mathf.Max(
            0,
            launchVehicle.transform.InverseTransformDirection(
                launchVehicle.velocity).z *
                (targetVehicle ?
                    1 :
                    Vector3.Dot(
                        transform.forward,
                        launchVehicle.transform.forward)));

        //AutoTargeting
        if (targetVehicle)
        {
            Vector3 bgn = transform.position;
            Vector3 pos;
            Vector3 vel;
            Vector3 accel;
            if (targetVehicle.GetComponent<NetworkView>().isMine) //Local vehicle, high precision
            {
                Vector3 refvel = targetVehicle.gameObject.GetComponent<Rigidbody>().velocity;
                float reftme = Time.time;
                yield return new WaitForSeconds(Time.fixedDeltaTime * 0.5f); //Allow target to move so we can analyze it's acceleration
                pos = targetVehicle.transform.position;
                vel = targetVehicle.gameObject.GetComponent<Rigidbody>().velocity;
                accel = (vel - refvel) / (Time.time - reftme); ////Accelleration over 1 second
            }
            else //Remote vehicle, just make it look good
            {
                Vector3 refpos = targetVehicle.transform.position;
                float reftme = Time.time;
                yield return new WaitForSeconds(Time.fixedDeltaTime * 0.5f); //Allow target to move so we can analyze it's velocity
                Vector3 refvel = (targetVehicle.transform.position - refpos) / (Time.time - reftme); //velocity over 1 second
                refpos = targetVehicle.transform.position;
                reftme = Time.time;
                yield return new WaitForSeconds(Time.fixedDeltaTime * 0.5f); //Allow target to move so we can analyze it's acceleration
                pos = targetVehicle.transform.position;
                vel = (targetVehicle.transform.position - refpos) / (Time.time - reftme); //velocity over 1 second
                accel = (vel - refvel) / (Time.time - reftme); //Accelleration over 1 second
            }

            float dur = 0f;
            while (true)
            {
                pos += vel * Time.fixedDeltaTime;
                vel += accel * Time.fixedDeltaTime;
                dur += Time.fixedDeltaTime;
                if (
                    dur > 10 ||
                    Vector3.Distance(bgn, pos) < dur * velocity)
                {
                    break;
                }
                transform.LookAt(pos);
            }
        }

        //Lag Compensation
        //Unnecessary when autotargeting, as hits are calculated on target client
        else if (lag > 0f)
        {
            GetComponent<Rigidbody>().position += GetComponent<Rigidbody>().transform.TransformDirection(
                0f,
                0f,
                velocity * lag); //Position extrapolation for non authoratative firing instances
        }

        //Begin Run
        GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().transform.TransformDirection(
            0f,
            0f,
            velocity);
        strtPos = GetComponent<Rigidbody>().position;
        stage = 0;

        //Cleanup
        yield return new WaitForSeconds(Mathf.Lerp(
            13f,
            5f,
            Game.Settings.laserSpeed * speedX * 0.003f));
        Destroy(gameObject);
    }

	public void FixedUpdate()
	{
		if (stage != 0 || !launchVehicle || !Game.Settings)
		{
			return;
		}
		if (Game.Settings.laserGrav != 0f)
		{
			Vector3 rigvel = GetComponent<Rigidbody>().velocity;
			rigvel.y -= Game.Settings.laserGrav *
                (float)speedX *
                Time.deltaTime *
                20f;
			GetComponent<Rigidbody>().velocity = rigvel;
		}
        Vector3 vector = transform.position - strtPos;
		RaycastHit[] hits = Physics.RaycastAll(
            strtPos,
            vector,
            vector.magnitude,
            (Game.Settings.lasersOptHit) ? maskOpt : mask);
		for (int i = 0; i < hits.Length; i++)
		{
            RaycastHit collision = hits[i];

            //Launch vehicle is immune to hits
            if (collision.collider.transform.root.gameObject == launchVehicle.gameObject)
            {
                continue;
            }

            //Non-authoratative game instances DO NOT detect vehicle laser hits. Hits are detected on authoratative instance, and then broadcast to other clients with lH RPC
            if (
                ((targetVehicle && !targetVehicle.GetComponent<NetworkView>().isMine) ||
                    (!targetVehicle && !launchVehicle.GetComponent<NetworkView>().isMine)) &&
                collision.rigidbody)
            {
                continue;
            }

            //We are the instance if this laser on the firer's computer, & we tagged another vehicle
            if (
                ((targetVehicle && targetVehicle.GetComponent<NetworkView>().isMine) ||
                    (!targetVehicle && launchVehicle.GetComponent<NetworkView>().isMine)) &&
                collision.rigidbody)
            {
                Vehicle veh = (Vehicle)collision.transform.root.gameObject.GetComponent(typeof(Vehicle));
                if (veh && veh.isResponding)
                {
                    //Determine where we hit them
                    veh.gameObject.GetComponent<NetworkView>().RPC(
                        "lH",
                        RPCMode.Others,
                        laserID,
                        collision.transform.InverseTransformDirection(collision.point));
                    //We are quarry, and made a tag
                    if (
                        launchVehicle.isIt == 1 &&
                        (Time.time - veh.lastTag) > 3f)
                    {
                        launchVehicle.gameObject.GetComponent<NetworkView>().RPC(
                            "iS",
                            RPCMode.All,
                            veh.gameObject.name);
                        veh.lastTag = Time.time;
                    }
                    //They were quarry, and we tagged them
                    else if (
                        veh.isIt == 1 &&
                        (Time.time - launchVehicle.lastTag) > 3 &&
                        (Time.time - veh.lastTag) > 3)
                    {
                        launchVehicle.gameObject.GetComponent<NetworkView>().RPC(
                            "sQ",
                            RPCMode.All,
                            2);
                    }
                    //They weren't quarry, and we weren't supposed to shoot them
                    else if (
                        veh.isIt == 0 &&
                        launchVehicle.isIt == 0)
                    {
                        launchVehicle.gameObject.GetComponent<NetworkView>().RPC(
                            "dS",
                            RPCMode.All,
                            veh.gameObject.name);
                    }
                }
            }

            if ((bool)collision.rigidbody || Game.Settings.laserRico == 0f)
            {
                laserHit(
                    collision.transform.root.gameObject,
                    collision.point,
                    collision.normal);
            }
            else
            {
                GetComponent<Rigidbody>().position = collision.point;
                GetComponent<Rigidbody>().velocity = Game.Settings.laserRico *
                    Vector3.Lerp(
                        Vector3.Scale(
                            GetComponent<Rigidbody>().velocity,
                            collision.normal),
                        Vector3.Reflect(
                            GetComponent<Rigidbody>().velocity,
                            collision.normal),
                        Game.Settings.laserRico);
            }
        }

		strtPos = GetComponent<Rigidbody>().position;
	}

	public void laserHit(GameObject go, Vector3 pos, Vector3 norm)
	{
		stage = 1;

		GetComponent<Rigidbody>().position = pos;
		GetComponent<Rigidbody>().velocity = Vector3.zero;
		go.BroadcastMessage(
            "OnLaserHit",
            launchVehicle.GetComponent<NetworkView>().isMine,
            SendMessageOptions.DontRequireReceiver);
		Collider[] colliders = Physics.OverlapSphere(pos, 10f);
        foreach (Collider hit in colliders)
        {
            if (hit.attachedRigidbody)
            {
                hit.attachedRigidbody.AddExplosionForce(
                    350f + speedX * 300f,
                    pos,
                    1f,
                    2f);
            }
        }
        Instantiate(
            explosion,
            pos,
            Quaternion.FromToRotation(Vector3.up, norm)); //transform.rotation
	}
}
