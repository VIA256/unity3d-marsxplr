using System;
using UnityEngine;

[Serializable]
public class VehicleLocal : MonoBehaviour
{
	public Vehicle vehicle;
	private Vector3 p;
	private Quaternion r;
	private int m = 0;
	public float syncPosTimer = 0.00f;
	public float syncInpTimer = 0.00f;
	public Vector4 inputS;
	private Vector3 prevPos = Vector3.zero;

	public void Start()
	{
		vehicle.GetComponent<NetworkView>().observed = this;
		vehicle.netCode = "";
		vehicle.isResponding = true;
	}

	public void Update()
	{
		if (vehicle.networkMode == 2 && Time.time > syncPosTimer)
		{
			syncPosTimer = Time.time + 1f / Network.sendRate;
			GetComponent<NetworkView>().RPC(
                "sP",
                RPCMode.Others,
                vehicle.myRigidbody.position,
                vehicle.myRigidbody.rotation);
		}

		if (Time.time > syncInpTimer && vehicle.input != inputS)
		{
			syncInpTimer = Time.time + 1f;
			inputS = vehicle.input;
			GetComponent<NetworkView>().RPC(
                "s4",
                RPCMode.Others,
                Mathf.RoundToInt(inputS.x * 10f),
                Mathf.RoundToInt(inputS.y * 10f),
                Mathf.RoundToInt(inputS.z * 10f),
                Mathf.RoundToInt(inputS.w * 10f));
		}
	}

	public void FixedUpdate()
	{
		vehicle.velocity = vehicle.myRigidbody.velocity;

        //Terrain Penetration
		RaycastHit hit = default(RaycastHit);
		RaycastHit[] hits = default(RaycastHit[]);
		if (prevPos != Vector3.zero)
		{
			hits = Physics.RaycastAll(
                prevPos,
                (vehicle.transform.position - prevPos).normalized,
                (vehicle.transform.position - prevPos).magnitude,
                vehicle.terrainMask);
		}
		if (hits != null && hits.Length > 0)
		{
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].transform.root != transform.root)
                {
                    hit = hits[i];
                    break;
                }
            }
		}
		if (hit.point != Vector3.zero)
		{
			if ((prevPos - transform.position).sqrMagnitude < 500f)
			{
				vehicle.transform.position = hit.point + hit.normal * 0.1f;
			}
			else
			{
                prevPos = vehicle.transform.position;   //We were teleporting
			}
		}
		else
		{
			prevPos = vehicle.transform.position;
		}
	}

	public void OnTriggerStay(Collider other)
	{
        if (other.gameObject.layer == 14) return;
        if ((bool)other.attachedRigidbody)
        {
            vehicle.OnRam(other.attachedRigidbody.gameObject);
        }
	}

	public void OnCollisionStay(Collision collision)
	{
		vehicle.vehObj.BroadcastMessage(
            "OnCollisionStay",
            collision,
            SendMessageOptions.DontRequireReceiver);
        if (collision.collider.transform.root == transform.root) return;
		if (
            (bool)collision.transform.parent &&
            (bool)collision.transform.parent.gameObject.GetComponent<Rigidbody>())
		{
			vehicle.OnRam(collision.transform.parent.gameObject);
		}
		else if ((bool)collision.rigidbody)
		{
			vehicle.OnRam(collision.gameObject);
		}
	}

	public void OnSerializeNetworkView(BitStream stream)
	{
		if (GetComponent<NetworkView>().stateSynchronization == NetworkStateSynchronization.Off)
		{
			Debug.Log("sNv NvL: " + gameObject.name);
			return;
		}

		p = vehicle.myRigidbody.position;
		r = vehicle.myRigidbody.rotation;
		stream.Serialize(ref p);
		stream.Serialize(ref r);
		m = 0;
		stream.Serialize(ref m);
	}
}
