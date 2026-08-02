using System;
using UnityEngine;

[Serializable]
public class State : System.Object
{
    public Vector3 p;
    public Quaternion r;
    public float t = 0.0f;
    public float m = 0.0f;
    public float n = 0.0f;

    public State(Vector3 p, Quaternion r, float t, float m, float n)
    {
        this.p = p;
        this.r = r;
        this.t = t;
        this.m = m;
        this.n = n;
    }
}

[Serializable]
public class VehicleNet : MonoBehaviour
{
	public Vehicle vehicle;
	public bool simulatePhysics = true;
	public bool updatePosition = true;
	public float physInterp = 0.1f;
	public float ping;
	public float jitter;
	public float calcPing = 0.00f;
	public float rpcPing = 0.00f;
	public float lastPing = -1.00f;
	public int pingCheck;
	public bool wePinged = false;
	public float autoInterp = 0.00f;
	private int m;
	private Vector3 p;
	private Quaternion r;
	public State[] states;

	public VehicleNet()
	{
		pingCheck = UnityEngine.Random.Range(15, 20);
		states = new State[15];
	}

	public void Start()
	{
		vehicle.networkView.observed = this;
	}

	public void Update()
	{
		if (vehicle.networkMode == 2 && Time.time > lastPing + (float)pingCheck)
		{
			if (lastPing < 0f)
			{
				lastPing = Time.time;
			}
			else
			{
				networkView.RPC("sT", RPCMode.All, 0f);
			}
		}

		if (
            !updatePosition ||
            states[14] == null ||
            states[14].t == 0f ||
            !vehicle.myRigidbody ||
            !Game.Player ||
            !Game.Player.rigidbody)
		{
			return;
		}

		if (Game.Settings.networkPhysics == 0)
		{
			physInterp = 0.1f;
		}
		else if (Game.Settings.networkPhysics == 1)
		{
			physInterp = 0.2f;
		}

		if (Game.Settings.networkPhysics == 2)
		{
			simulatePhysics = false;
		}
		else
		{
			simulatePhysics = Vector3.Distance(
                vehicle.myRigidbody.position,
                Game.Player.rigidbody.position) < 40f;
		}

		vehicle.myRigidbody.isKinematic = !simulatePhysics;
		vehicle.myRigidbody.interpolation = RigidbodyInterpolation.None;
		
        //Interpolation
        float interpolationTime;
		if (Game.Settings.networkInterpolation > 0f)
		{
			interpolationTime = (float)(Network.time - (double)Game.Settings.networkInterpolation);
		}
		else
		{
			autoInterp = Mathf.Max(
                0.1f,
                Mathf.Lerp(
                    autoInterp,
                    ping * 1.5f + jitter * 8f,
                    Time.deltaTime * 0.15f));
			interpolationTime = (float)(Network.time - (double)autoInterp);
		}

        if (states[0].t > interpolationTime)                                        // Target playback time should be present in the buffer
        {                                                                           // Go through buffer and find correct state to play back
            for (int i = 1; i < states.Length - 1; i++)
            {
                if (
                    states[i] != null &&
                    states[i].t <= interpolationTime ||
                    i == states.Length - 3)
                {
                    State rhs = states[i - 1];                                      // The state one slot newer than the best playback state
                    State lhs = states[i];                                          // The best playback state (closest to .1 seconds old)
                    float l = rhs.t - lhs.t;                                        // Use the time between the two slots to determine if interpolation is necessary
                    float t = 0f;                                                   // As the time difference gets closer to 100 ms, t gets closer to 1 - in which case rhs is used
                    if (l > 0.0001f)
                    {
                        t = (interpolationTime - lhs.t) / l;                        // if t=0 => lhs is used directly
                    }
                    vehicle.velocity = Vector3.Lerp(
                        vehicle.velocity,
                        ((rhs.p - states[i + 2].p) / (rhs.t - states[i + 2].t)),
                        Time.deltaTime * 0.3f);
                    if (simulatePhysics)
                    {
                        vehicle.myRigidbody.MovePosition(Vector3.Lerp(
                            vehicle.transform.position,
                            Vector3.Lerp(lhs.p, rhs.p, t),
                            physInterp));
                        vehicle.myRigidbody.MoveRotation(Quaternion.Slerp(
                            vehicle.transform.rotation,
                            Quaternion.Slerp(lhs.r, rhs.r, t),
                            physInterp));
                        vehicle.myRigidbody.velocity = vehicle.velocity;
                    }
                    else
                    {
                        vehicle.myRigidbody.position = Vector3.Lerp(lhs.p, rhs.p, t);
                        vehicle.myRigidbody.rotation = Quaternion.Slerp(lhs.r, rhs.r, t);
                    }
                    vehicle.isResponding = true;
                    vehicle.netCode = "";
                    return;
                }
            }
        }

        //Extrapolations
        else
        {
            float extrapolationLength = (interpolationTime - states[0].t);
            vehicle.velocity = Vector3.Lerp(
                vehicle.velocity,
                ((states[0].p - states[2].p) / (states[0].t - states[2].t)),
                Time.deltaTime * 0.3f);
            if (extrapolationLength < 1f)
            {
                if (!simulatePhysics)
                {
                    vehicle.myRigidbody.position = states[0].p +
                        (vehicle.velocity * extrapolationLength);
                    vehicle.myRigidbody.rotation = states[0].r;
                }
                vehicle.isResponding = true;
                if (extrapolationLength < 0.3f) vehicle.netCode = ">";
                else vehicle.netCode = " (Delayed)";
            }
            else
            {
                vehicle.netCode = " (Not Responding)";
                vehicle.isResponding = false;
            }
        }
	}

	public void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
        //We are the server, and have to keep track of relaying messages between connected clients
        if (stream.isWriting)
        {
            if (networkView.stateSynchronization == NetworkStateSynchronization.Off)
            {
                Debug.Log("sNv NvS: " + gameObject.name);
                return;
            }

            if (states[0] == null) return;
            p = states[0].p;
            r = states[0].r;
            //m is the number of milliseconds that transpire between the packet's original send time and the time it is resent from the server to all the other clients
            m = (int)((Network.time - (double)states[0].t) * 1000.0);
            stream.Serialize(ref p);
            stream.Serialize(ref r);
            stream.Serialize(ref m);
        }

        //New packet recieved - add it to the states array for interpolation!
        else
        {
            if (networkView.stateSynchronization == NetworkStateSynchronization.Off)
            {
                Debug.Log("sNv NvN: " + gameObject.name);
                return;
            }

            stream.Serialize(ref p);
            stream.Serialize(ref r);
            stream.Serialize(ref m);

            State state = new State(
                p,
                r,
                (float)(info.timestamp - (double)((m <= 0) ?
                    0f :
                    ((float)m / 1000f))),
                m,
                (float)(Network.time - info.timestamp));
            
            if (states[0] != null && state.t == states[0].t)
            {
                state.t += 0.01f; //Bizarre - dragonhere
            }

            if (states[0] == null || state.t > states[0].t)
            {
                float png = (float)(Network.time - (double)state.t);
                jitter = Mathf.Lerp(
                    jitter,
                    Mathf.Abs(ping - png),
                    1f / Network.sendRate);
                ping = Mathf.Lerp(ping, png, 1f / Network.sendRate);
                for (int k = states.Length - 1; k > 0; k--)
                {
                    states[k] = states[k - 1];
                }
                states[0] = state;
            }
        }
	}
}
