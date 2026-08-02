using System;
using UnityEngine;

[Serializable]
public class Jet : MonoBehaviour
{
	public Vehicle vehicle;
	public GameObject[] landingGear;
	public float landingGearScale;
	public Transform[] hoverThrusters;
	public ParticleRenderer mainThrusterParticles;
	public ThrustCone mainThrusterCone;
	public Transform[] mainThruster;
	public LayerMask thrustMask;
	public MeshCollider bodyCollider;
	public WhirldLOD lod;

	public int hoverThrustFactor = 1;
	public float hoverSteerFactor = 0.1f;
	public float hoverAngDrag = 0.1f;
	public float hoverLevelForceFactor = 0.2f;
	public int flightThrustFactor = 5;
	public float flightAngDrag = 0.1f;

	public float atmosDensity;
	public Vector3 locvel;
	public float speed;
	public float pitch;
	public float roll;
	public float angleOfAttack;
	public float stallFactor;

	private float grav;
	private float mass;
	private Vector3 inertiaTensor;
	private Quaternion inertiaTensorRotation;

	public float lavaFloat = 0.1f;
	public RaycastHit hit;

	public void InitVehicle(Vehicle veh)
	{
		vehicle = veh;
		vehicle.specialInput = true; //Start out in landing mode

		mass = vehicle.myRigidbody.mass;
		grav = -Physics.gravity.y * mass;
		inertiaTensor = vehicle.myRigidbody.inertiaTensor;
		inertiaTensorRotation = vehicle.myRigidbody.inertiaTensorRotation;
	}

	public void Update()
	{
        if (!vehicle)
        {
            return;
        }

        //Thruster Particles
        Vector3 mainThrustPELocVel = mainThrusterParticles.gameObject.GetComponent<ParticleEmitter>().localVelocity;
		if (vehicle.specialInput)
		{
            foreach (Transform thruster in hoverThrusters)
            {
                thruster.gameObject.GetComponent<ParticleEmitter>().emit = true;

                Vector3 pelocvel = thruster.gameObject.GetComponent<ParticleEmitter>().localVelocity;
                pelocvel.y = -vehicle.input.z;
                thruster.gameObject.GetComponent<ParticleEmitter>().localVelocity = pelocvel;

                thruster.gameObject.GetComponent<ParticleEmitter>().minSize =
                    thruster.gameObject.GetComponent<ParticleEmitter>().maxSize =
                    Mathf.Max(0.1f, vehicle.input.z * 0.3f);
            }
			if (lod.level == 0)
			{
                mainThrustPELocVel.x = vehicle.input.x;
                mainThrustPELocVel.y = -vehicle.input.y;
			}
			else
			{
				mainThrusterCone.magThrottle = 0f;
			}
		}
		else
		{
			if (vehicle.brakes)
			{
				vehicle.input.z = 0f;
			}

			if (lod.level == 0)
			{
                mainThrustPELocVel.x = mainThrustPELocVel.y = 0.0f;
            }
			else
			{
				mainThrusterCone.magThrottle = 4f;
			}

            foreach (Transform thruster in hoverThrusters)
            {
                thruster.gameObject.GetComponent<ParticleEmitter>().emit = false;
            }
		}
        mainThrustPELocVel.z = Mathf.Min(
            -10.0f * (vehicle.specialInput ?
                0.1f :
                vehicle.input.z),
            -0.5f);
        if (mainThrustPELocVel.z >= -1)
        {
            mainThrusterParticles.particleRenderMode = ParticleRenderMode.Billboard;
        }
        else mainThrusterParticles.particleRenderMode = ParticleRenderMode.Stretch;
        mainThrusterParticles.gameObject.GetComponent<ParticleEmitter>().localVelocity = mainThrustPELocVel;

        //Camera
		vehicle.camSmooth = !vehicle.specialInput;
	}

	public void FixedUpdate()
	{
		if (!vehicle)
		{
            //We are materializing, don't try to manipulate physics
			return;
		}

        //Landing Gear
		if (vehicle.specialInput)
		{
            if (!landingGear[lod.level].activeInHierarchy)
            {
                landingGear[lod.level].SetActive(true);
            }
            if (landingGearScale < 1)
            {
                landingGearScale += Time.deltaTime;
            }
		}
		else
		{
            if (landingGear[lod.level].activeInHierarchy &&
                landingGearScale > 0)
            {
                landingGearScale -= Time.deltaTime;
            }
            else if (landingGear[lod.level].activeInHierarchy)
            {
                landingGear[lod.level].SetActive(false);
            }
		}
        if (landingGear[lod.level].activeInHierarchy)
        {
            Vector3 locscl = landingGear[lod.level].transform.localScale;
            locscl.y =
                locscl.x =
                landingGearScale;
            landingGear[lod.level].transform.localScale = locscl;
        }

		if (vehicle.myRigidbody.isKinematic)
		{
            //We are materializing, don't try to manipulate physics
			return;
		}

		vehicle.myRigidbody.centerOfMass = Vector3.zero;
		vehicle.myRigidbody.inertiaTensor = inertiaTensor * 0.75f;
		vehicle.myRigidbody.inertiaTensorRotation = inertiaTensorRotation;

        //Hovering
		if (vehicle.specialInput)
		{
            //Force of hover thrusters in atmosphere
			vehicle.myRigidbody.AddForce(
                transform.up *
                vehicle.input.z *
                hoverThrustFactor *
                grav);
			
            //Autoleveling
            vehicle.myRigidbody.AddTorque(
                Vector3.Cross(transform.up, Vector3.up) *
                Vector3.Angle(transform.up, Vector3.up) *
                hoverLevelForceFactor *
                mass);
			
            //Steering
            vehicle.myRigidbody.AddRelativeTorque(new Vector3(
                vehicle.input.y * mass * hoverSteerFactor,
                Mathf.Clamp(vehicle.input.x + vehicle.input.w, -1f, 1f) * mass * hoverSteerFactor,
                vehicle.input.x * -1f * mass * hoverSteerFactor));
			
            vehicle.myRigidbody.drag =
                Game.Settings.jetHDrag *
                vehicle.myRigidbody.velocity.magnitude *
                (float)(vehicle.brakes ? 7 : 1);
			vehicle.myRigidbody.angularDrag =
                hoverAngDrag *
                (float)(vehicle.brakes ? 5 : 1);
            mainThruster[lod.level].localEulerAngles = Vector3.zero;
		}

        //Flying
		else
		{
			if (vehicle.brakes)
			{
				vehicle.input.z = 0f;
			}

            //Pertinent Flight Info
			if (vehicle.myRigidbody.transform.position.y < 5000f)
			{
				atmosDensity = Mathf.Lerp(
                    1.225f,
                    0.18756f,
                    vehicle.myRigidbody.transform.position.y / 5000f);
			}
			else
			{
				atmosDensity = Mathf.Lerp(
                    0.18756f,
                    0.017102f,
                    vehicle.myRigidbody.transform.position.y / 10000f);
			}
			speed = vehicle.myRigidbody.velocity.magnitude;
            pitch = (transform.eulerAngles.x > 180 ?
                transform.eulerAngles.x - 360 :
                transform.eulerAngles.x);
			roll = ((transform.eulerAngles.z > 180f) ?
                (transform.eulerAngles.z - 360f) :
                transform.eulerAngles.z);
			locvel = vehicle
                .myRigidbody
                .transform
                .InverseTransformDirection(vehicle.myRigidbody.velocity);
			angleOfAttack = locvel.normalized.y;
			if (speed < (float)Game.Settings.jetStall)
			{
				stallFactor = Mathf.Lerp(
                    1f,
                    0f,
                    (speed - (float)Game.Settings.jetStall * 0.8f) /
                        (float)Game.Settings.jetStall * 10f);
			}
			else
			{
				stallFactor = Mathf.Max(
                    0f,
                    Mathf.Min(
                        Mathf.Abs(angleOfAttack) - 0.65f,
                        0.1f
                    )
                ) * 10f;
			}

            //Thruster
            mainThruster[lod.level].localEulerAngles = new Vector3(
                -vehicle.input.y * Mathf.Lerp(
                    20,
                    5,
                    speed / (Game.Settings.jetStall * 5)),
                -vehicle.input.x +
                    (vehicle.input.w == 0 ?
                        Mathf.Clamp(-locvel.x * 1, -10, 10) :
                        Mathf.Clamp(-locvel.x * 0.5f, -10, 10)) +
                    -vehicle.input.w * 15,
                0);
            vehicle.myRigidbody.AddForceAtPosition(
                mainThruster[lod.level].forward *
                    vehicle.input.z *
                    flightThrustFactor *
                    grav *
                    0.99f,
                mainThruster[0].position);
			
            //Control Surfaces
            vehicle.myRigidbody.AddRelativeTorque(
                new Vector3(
                    vehicle.input.y *
                        mass *
                        (float)Game.Settings.jetSteer *
                        0.2f,
                    0f,
                    vehicle.input.x *
                        -1f *
                        mass *
                        (float)Game.Settings.jetSteer *
                        0.75f
                    ) *
                Mathf.Lerp(
                    0f,
                    1f,
                    speed / (float)Game.Settings.jetStall * 0.7f
                ) *
                atmosDensity *
                ((locvel.z > 0f) ? 1 : -1));
			vehicle.myRigidbody.angularDrag = flightAngDrag;
			
            //Lift
            if (stallFactor < 1f)
			{
                //http://www.aerospaceweb.org/question/aerodynamics/q0015b.shtml
				int wingArea = 15;
				float liftCoefficient = ((angleOfAttack > 0f) ?
                    (Mathf.Min(0.3f, angleOfAttack) * -1f) :
                    Mathf.Max(0.3f * -1f, angleOfAttack * -1f));
				float lift = Game.Settings.jetLift *
                    atmosDensity *
                    locvel.z *
                    locvel.z *
                    (float)wingArea *
                    liftCoefficient;
				vehicle.myRigidbody.AddRelativeForce(
                    Vector3.up *
                    Mathf.Lerp(lift, 0f, stallFactor));
			}

            //Drag
			if (stallFactor >= 0.5f)
			{
				vehicle.myRigidbody.drag =
                    speed *
                    Mathf.Lerp(
                        Game.Settings.jetDrag,
                        Game.Settings.jetDrag * 5f,
                        Vector3.Angle(
                            vehicle.myRigidbody.velocity,
                            vehicle.myRigidbody.transform.forward
                        ) / 90f
                    ) *
                    atmosDensity;
			}
			else
			{
				vehicle.myRigidbody.drag = 0f;
				vehicle.myRigidbody.AddRelativeForce(
                    new Vector3(
                        locvel.x * (Game.Settings.jetDrag * -1f) * 3f,
                        locvel.y * (Game.Settings.jetDrag * -1f) * 3f,
                        locvel.z * (Game.Settings.jetDrag * -1f)
                    ) *
                        atmosDensity * (vehicle.brakes ? 5 : 1),
                    ForceMode.VelocityChange);
			}
		}

        //Floating
        if (
            transform.position.y < Game.Settings.lavaAlt + 20 ||
            Physics.Raycast(
                transform.position + (Vector3.up * 200),
                Vector3.down,
                out hit,
                220,
                1 << 4))
		{
            Vector3 up = Vector3.up * 200f;
            Vector3 dn = -Vector3.up;
            RaycastHit lavaHit;
            for (int i = 0; i < bodyCollider.sharedMesh.vertices.Length; i++)
            {
                Vector3 pt = bodyCollider.sharedMesh.vertices[i];

                float hitDistance;
                pt = transform.TransformPoint(pt);
                if (pt.y < Game.Settings.lavaAlt)
                {
                    hitDistance = -(pt.y - Game.Settings.lavaAlt);
                }
                else if (
                    hit.distance != 0.0f &&
                    hit.collider.Raycast(new Ray(pt + up, dn), out lavaHit, 200f))
                {
                    hitDistance = (200 - lavaHit.distance);
                }
                else continue;
                Vector3 ptVel = vehicle.myRigidbody.GetPointVelocity(pt);
                vehicle.myRigidbody.AddForceAtPosition(
                    (Vector3.up *
                        lavaFloat *
                        Mathf.Min(6, 3 + hitDistance) *
                        Mathf.Lerp(
                            1.3f,
                            5,
                            new Vector2(ptVel.x, ptVel.z).magnitude / 20) +
                        ptVel *
                        -Game.Settings.jetDrag * 70
                    ) / bodyCollider.sharedMesh.vertexCount,
                    pt,
                    ForceMode.VelocityChange);

                bodyCollider.sharedMesh.vertices[i] = pt;
                i++;
            }
		}
	}
}
