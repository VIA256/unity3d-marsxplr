using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Buggy : MonoBehaviour
{
	//Object Linkages
	public MeshFilter wing0;
	public MeshFilter wing1;
	public Transform floatPoints;
	public GameObject wheel;
	public GameObject axel;
	public Transform leftTrail;
	public Transform rightTrail;
	public GameObject WheelmarksPrefab;
	public Collider buggyCollider;
	public WhirldLOD lod;
	public Transform[] wheels;
	public Transform[] wheelGraphics;
	public Transform[] axels;
	public Vector3 wheelPos;
	public Vector3 axelPos;

	//Internal Registers
	private Vector3[] baseVertices;
	// /*unused*/ private Vector3[] baseNormals;
	private Mesh wingMesh;
	private float wingState;
	// /*unused*/ private int wingFlaps;
	private bool wingOpen;
	private bool isInverted;
	private Vehicle vehicle;
	private Transform[] bouyancyPoints;
	private float suspensionRange;
	private float friction;
	// /*UNUSED*/ private float[] realComp;
	private float[] hitDistance;
	private float[] hitCompress;
	private float[] hitFriction;
	private Vector3[] hitVelocity;
	private Vector3[] wheelPositn;
	private Vector3[] hitForce;
	private Skidmarks wheelMarks;
	private int[] wheelMarkIndex;
	private bool isDynamic;
	private float frictionTotal;
	private float brakePower;

	//Drivetrain Data
	private float motorTorque;
	private float motorSpeed;
	// /*UNUSED*/ private float motorSpd;
	private float motorInputSmoothed;
	private float wheelRadius;
	private float wheelCircumference;
	private int motorMass;
	private int motorDrag;
	private int maxAcceleration;
	private int motorAccel;
	// /*unused*/ private float motorSpeedNew;
	
	public Buggy()
    {
		wingState = 0f;
		//wingFlaps = 0;
		isInverted = false;
		//realComp = new float[4];
		hitDistance = new float[4];
		hitCompress = new float[4];
		hitFriction = new float[4];
		hitVelocity = new Vector3[4];
		wheelPositn = new Vector3[4];
		hitForce = new Vector3[4];
		wheelMarkIndex = new int[4];
		isDynamic = false;
		motorSpeed = 0f;
		//motorSpd = 0f;
		motorInputSmoothed = 0f;
		wheelRadius = 0.3f;
		wheelCircumference = wheelRadius * (float)Math.PI * 2f;
		motorMass = 1;
		motorDrag = 1;
		maxAcceleration = 60;
		motorAccel = 60;
		//motorSpeedNew = 0f;
	}

	public void InitVehicle(Vehicle veh)
    {
		vehicle = veh;

		List<Material> materialAccents = new List<Material>();

		//Init Wing Tinting
		materialAccents.Add(wing0.renderer.material);
		materialAccents.Add(wing1.renderer.material);

		//Instantiate Wheels
		int i;
		for (i = 0; i < 4; i++)
        {
			wheelPositn[i] = new Vector3(
				wheelPos.x * (float)((i % 2 != 0) ? 1 : (-1)),
				wheelPos.y,
				wheelPos.z * (float)((i < 2) ? 1 : (-1))
			);
			GameObject go = (GameObject)UnityEngine.Object.Instantiate(
				wheel,
				this.transform.TransformPoint(wheelPositn[i]),
				this.transform.rotation
			);
			wheels[i] = go.transform;
			wheelGraphics[i] = wheels[i].Find("Graphic").transform;
			if (i == 1 || i == 3)
            {
				Vector3 localEulerAngles = wheels[i].Find("Graphic/Simple").transform.localEulerAngles;
				localEulerAngles.y = 0;
				wheels[i].Find("Graphic/Simple").transform.localEulerAngles = localEulerAngles;
			}
			MeshRenderer mR = (MeshRenderer)wheelGraphics[i].Find("Detailed/Beadlock").GetComponent(typeof(MeshRenderer));
			materialAccents.Add(mR.material);
			wheels[i].parent = this.transform;
			go = (GameObject)UnityEngine.Object.Instantiate(
				axel,
				this.transform.TransformPoint(new Vector3(
					axelPos.x * (float)((i % 2 != 0) ? 1 : (-1)),
					axelPos.y,
					axelPos.z * (float)((i < 2) ? 1 : (-1))
				)),
				this.transform.rotation
			);
			axels[i] = go.transform;
			axels[i].parent = this.transform;
		}

		//Instantiate Skidmarks
		if (vehicle.isPlayer)
        {
			GameObject go = (GameObject)UnityEngine.Object.Instantiate(
				WheelmarksPrefab,
				Vector3.zero,
				Quaternion.identity
			);
			go.layer = 11;
			wheelMarks = (Skidmarks)go.GetComponentInChildren(typeof(Skidmarks));
		}
		else 
        {
			leftTrail.gameObject.active = false;
			rightTrail.gameObject.active = false;
		}

		//Initialize Bouyancy Points
		i = 0;
		bouyancyPoints = new Transform[floatPoints.childCount];
        foreach (Transform pt in floatPoints)
        {
            bouyancyPoints[i] = pt;
            i++;
        }

		vehicle.materialAccent = (Material[])materialAccents.ToArray();
	}

	public void Update()
    {
		//Wings
		if (wingState != 0f)
        {
			wingState += Time.deltaTime * 2f;
			if (wingState >= 1f)
            {
				wingOpen = true;
				if (wingState > 2f)
                {
					wingState = 0f;
				}
			}
			//Closing Wings
			else if (wingState > 0f)
            {
				wingOpen = false;

				Vector3 localPosition = leftTrail.localPosition;
				localPosition.x = 0;
				leftTrail.localPosition = localPosition;

				localPosition = rightTrail.localPosition;
				localPosition.x = 0;
				rightTrail.localPosition = localPosition;

				wingState = 0f;
			}
		}
		wingMesh = ((lod.level != 0) ? wing1 : wing0).mesh;
		((lod.level != 0) ? wing1 : wing0).gameObject.active = wingOpen;
		if (wingOpen)
        {
			if (baseVertices == null)
            {
				baseVertices = wingMesh.vertices;
			}
			Vector3[] vertices = new Vector3[baseVertices.Length];
			for (int i = 0; i < vertices.Length; i++)
            {
				Vector3 pos = baseVertices[i];
				if (wingState >= -1 && wingState < 0 || wingState >= 1 && wingState < 2)
                {
					if (wingState > 0f)
                    {
						pos.y *= wingState - 1f;
						pos.x *= wingState - 1f;

						Vector3 localPosition = leftTrail.localPosition;
						localPosition.x = (wingState - 1f) * (3.5f * -1f);
						leftTrail.localPosition = localPosition;

						localPosition = rightTrail.localPosition;
						localPosition.x = (wingState - 1f) * 3.5f;
						rightTrail.localPosition = localPosition;
					}
					else
                    {
						pos.y *= wingState;
						pos.x *= wingState;

						Vector3 localPosition = leftTrail.localPosition;
						localPosition.x = wingState * 3.5f;
						leftTrail.localPosition = localPosition;

						localPosition = rightTrail.localPosition;
						localPosition.x = wingState * (3.5f * -1f);
						rightTrail.localPosition = localPosition;
					}
				}
				else
                {
					float t = pos.z * (vehicle.input.x * 0.14f);
					if (pos.z > 0.2) t = 0;
					else t *= (Mathf.Abs(pos.x) / 10);
					t += pos.x * (motorInputSmoothed * 0.04f);
					float st = Mathf.Sin(t);
					float ct = Mathf.Cos(t);
					pos.x = pos.y * st + pos.x * ct;
					pos.y = pos.y * ct - pos.x * st;
				}
				vertices[i] = pos;
			}
			if (!(wingState >= -1 && wingState < 0 || wingState >= 1 && wingState < 2))
            {
				Vector3 localPosition = leftTrail.localPosition;
				localPosition.x = -3.5f;
				leftTrail.localPosition = localPosition;

				localPosition = rightTrail.localPosition;
				localPosition.x = 3.5f;
				rightTrail.localPosition = localPosition;
			}
			wingMesh.vertices = vertices;
		}
		else
        {
			Vector3 localPosition = rightTrail.localPosition;
			localPosition.x = 0;
			rightTrail.localPosition = localPosition;

			localPosition = leftTrail.localPosition;
			localPosition.x = 0;
			leftTrail.localPosition = localPosition;
		}

		//Wheels
		for (int i = 0; i < 4; i++)
        {
			Vector3 pos = new Vector3(
				wheelPos.x * ((i % 2 != 0) ? 1 : -1),
				wheelPos.y - (hitDistance[i] == -1 ? suspensionRange : (hitDistance[i] - wheelRadius)),
				wheelPos.z * (i < 2 ? 1 : -1)
			);
			wheels[i].transform.position = this.transform.TransformPoint(pos);
			wheelGraphics[i].transform.Rotate(
				360f * (motorSpeed / wheelCircumference) * Time.deltaTime * 0.5f,
				0,
				0
			);
			if (axels[i].gameObject.active)
            {
				axels[i].LookAt(wheels[i].position);
			}
		}
	}

	public void FixedUpdate()
    {
		bool wheelsAreTouchingGround = false;
		if (Game.Settings.buggySmartSuspension)
        {
			//Auto Gear Retract
			if (
				(Game.Settings.buggyNewPhysics || wingOpen) &&
				!Physics.Raycast(
					this.transform.position,
					Vector3.up * -1f, 5f,
					vehicle.terrainMask
				)
			)
            {
				if (suspensionRange > 0.01f)
                {
					suspensionRange -= Time.deltaTime * 0.5f;
				}
				else
                {
					suspensionRange = 0f;
				}
			}
			else
            {
				suspensionRange = Mathf.Lerp(
					suspensionRange,
					(wingOpen ? 0.5f : Mathf.Lerp(
						0.4f,
						0.2f,
						Mathf.Min(
							1f,
							((Game.Settings.buggyNewPhysics ?
								vehicle.myRigidbody.velocity.magnitude :
								Mathf.Abs(motorSpeed)) / Game.Settings.buggySpeed
							)
						)
					)),
					Time.deltaTime * 3f
				);
			}
		}
		else
        {
			suspensionRange = 0.4f;
		}
		Vector3 centerOfMass = vehicle.myRigidbody.centerOfMass;
		centerOfMass.z = 0.2f;
		vehicle.myRigidbody.centerOfMass = centerOfMass;

		centerOfMass = vehicle.myRigidbody.centerOfMass;
		centerOfMass.y = Game.Settings.buggyCG * suspensionRange * 0.5f;
		vehicle.myRigidbody.centerOfMass = centerOfMass;

		vehicle.myRigidbody.mass = 30f;

		RaycastHit hit = default(RaycastHit);
		checked
        {
			if (vehicle.myRigidbody.isKinematic)
            {
				for (int i = 0; i < 4; i++)
                {
					if (
						Physics.Raycast(
							transform.TransformPoint(wheelPositn[i]),
							transform.up * -1,
							out hit,
							suspensionRange + wheelRadius, vehicle.terrainMask
						)
					)
                    {
						motorSpeed = hitVelocity[i].z;
						hitDistance[i] = hit.distance;
					}
					else hitDistance[i] = -1;
				}
				return;
			}
			if (wingOpen)
            {
				motorInputSmoothed = Mathf.Lerp(
					vehicle.input.y,
					motorInputSmoothed + (vehicle.brakes ? -1 : 0),
					0.8f
				);
				int stallSpeed = 16;
				Vector3 locVel = transform.InverseTransformDirection(vehicle.myRigidbody.velocity);
				float roll = ((!(transform.eulerAngles.z > 180f)) ?
					transform.eulerAngles.z :
					(transform.eulerAngles.z - 360f)
				);
				float pitch = ((!(transform.eulerAngles.x > 180f)) ?
					transform.eulerAngles.x :
					(transform.eulerAngles.x - 360f)
				);
				if (locVel.sqrMagnitude > (float)stallSpeed)
                {
					vehicle.myRigidbody.drag = vehicle.myRigidbody.velocity.magnitude / Game.Settings.buggyFlightDrag * 0.3f;
					
					//Airbrakes
					if (vehicle.brakes)
                    {
						if (brakePower < 1f)
                        {
							brakePower += Time.deltaTime * 0.15f;
						}
						float multiplier = -brakePower * 2f;
						vehicle.myRigidbody.AddRelativeForce(
							locVel.x * multiplier * 5f,
							locVel.y * multiplier * 100f,
							locVel.z * 150f * multiplier
						);
						vehicle.myRigidbody.AddRelativeTorque(new Vector3(
							(pitch + (vehicle.input.y * -100f)) * -2f,
							vehicle.input.x * 280f,
							roll * -1f
						));
					}

					//Standard Flight
					else
                    {
						brakePower = 0f;
						float angDelta = Vector3.Angle(
							vehicle.myRigidbody.velocity,
							transform.TransformDirection(Vector3.forward)
						);
						if (angDelta > 10f && Game.Settings.buggyFlightSlip)
                        {
							vehicle.myRigidbody.velocity = vehicle.myRigidbody.transform.TransformDirection(
								locVel.x * 0.95f,
								locVel.y * 0.95f,
								locVel.z + ((Mathf.Abs(locVel.x) + Mathf.Abs(locVel.y)) * 0.1f * (angDelta / 360))
							);
						}
						else
                        {
							vehicle.myRigidbody.velocity = vehicle.myRigidbody.transform.TransformDirection(
								0,
								0,
								locVel.magnitude + (Time.deltaTime * 50 * (Game.Settings.buggyFlightLooPower ?
									Mathf.Abs(motorInputSmoothed) / 10 :
									(motorInputSmoothed < 0.999f && motorInputSmoothed > -0.999f ?
										Mathf.Abs(motorInputSmoothed) / 10 :
										0
									)
								))
							);
						}
						vehicle.myRigidbody.AddRelativeTorque(new Vector3(
							motorInputSmoothed * 100 * Game.Settings.buggyFlightAgility,
							0,
							vehicle.input.x * -100 * Game.Settings.buggyFlightAgility
						));
					}

					//Slideslip - "Dihedral"
					if (
						vehicle.input.x == 0 &&
						(transform.eulerAngles.z < 90 ||
							transform.eulerAngles.z > 270
						)
					)
                    {
						vehicle.myRigidbody.AddRelativeTorque(
							((transform.eulerAngles.x < 10 || transform.eulerAngles.x > 350) ?
								pitch - 0.95f :
								0
							) * -0,
							roll * -0.6f,
							((transform.eulerAngles.z < 20 || transform.eulerAngles.z > 340) ?
								roll * -0.5f :
								0
							)
						);
					}
					else if (vehicle.input.x == 0){
						vehicle.myRigidbody.AddRelativeTorque(
							0,
							(transform.eulerAngles.z - 180) * 0.4f, 0
						);
					}

					//Lava "Thermals"
					if (
						transform.position.y < Game.Settings.lavaAlt + 10 ||
						Physics.Raycast(transform.position, Vector3.down, out hit, 10, 1 << 4)
					)
                    {
						vehicle.myRigidbody.AddForce(Vector3.up * (10 - hit.distance) * 40);
					}
					vehicle.myRigidbody.angularDrag = 5f;
				}
				else
                {
					//Stalling
					vehicle.myRigidbody.angularDrag = 1f;
					vehicle.myRigidbody.drag = vehicle.myRigidbody.velocity.magnitude / Game.Settings.buggyFlightDrag * 9f;
					vehicle.myRigidbody.AddRelativeTorque(new Vector3(
						vehicle.input.y + 0.5f * 100f,
						0f,
						vehicle.input.x * -30f
					));
				}
			}
			else if (vehicle.brakes && vehicle.myRigidbody.velocity.magnitude < 1.5f)
            {
				if (vehicle.input.y != 0f)
                {
					vehicle.myRigidbody.drag = 2f;
				}
				else
                {
					vehicle.myRigidbody.drag = 50f;
				}
				vehicle.myRigidbody.angularDrag = 1f;
			}
			else if (vehicle.brakes && vehicle.myRigidbody.velocity.magnitude < 10f)
            {
				if (vehicle.input.y != 0f)
                {
					vehicle.myRigidbody.drag = 2f;
				}
				else
                {
					vehicle.myRigidbody.drag = 10f;
				}
			}
			else
            {
				vehicle.myRigidbody.angularDrag = 0.2f;
				vehicle.myRigidbody.drag = 0.01f;
			}

			//Steering
			float steeringAngle = Mathf.Lerp(
				40f,
				30f,
				vehicle.myRigidbody.velocity.magnitude / Game.Settings.buggySpeed
			);
			wheels[0].localRotation = (wheels[1].localRotation = Quaternion.LookRotation(new Vector3(
				vehicle.input.x * (steeringAngle / 90f),
				0f,
				1f + -1f * Mathf.Abs(vehicle.input.x * (steeringAngle / 90f))
			)));
			steeringAngle = Mathf.Lerp(
				20f,
				0f,
				vehicle.myRigidbody.velocity.magnitude / Game.Settings.buggySpeed
			);
			wheels[2].localRotation = (wheels[3].localRotation = Quaternion.LookRotation(new Vector3(
				vehicle.input.x * -1f * (steeringAngle / 90f),
				0f,
				1f + -1f * Mathf.Abs(vehicle.input.x * (steeringAngle / 90f))
			)));

			//Experimental Motor Physics
			if (Game.Settings.buggyNewPhysics)
            {
				motorTorque = -vehicle.input.y * Mathf.Lerp(
					Game.Settings.buggyPower * 3f,
					0f,
					hitVelocity[0].z / Game.Settings.buggySpeed
				);

				//Apply Wheel Force
				frictionTotal = 0f;
				for (int i = 0; i < 4; i++)
                {
					if (
						Physics.Raycast(
							transform.TransformPoint(wheelPositn[i]),
							transform.up * -1,
							out hit,
							suspensionRange + wheelRadius,
							vehicle.terrainMask
						)
					)
                    {
						//Static Friction
						if (motorTorque == 0 || motorTorque < (hitFriction[i] * hitForce[i].z))
                        {
							motorSpeed = hitVelocity[i].z;
						}
						//Dynamic Friction
						else
                        {
							motorSpeed = Mathf.Lerp(
								Game.Settings.buggySpeed,
								0,
								(motorTorque - (hitFriction[i] * hitForce[i].z)) / motorTorque
							);
							//motorSpeed += -motorSpeed * motorDrag / motorTorque * Time.fixedDeltaTime;
						}
						//motorSpd = (frictionTotal - Game.Settings.buggyPower * 3) / (Game.Settings.buggyPower * 3 / Game.Settings.buggySpeed);

						wheelsAreTouchingGround = true;
						isDynamic = (
							(motorTorque > hitFriction[i]) ||
							(Mathf.Abs(hitVelocity[i].x) > Mathf.Abs(hitVelocity[i].z) * 0.3f)
						);
						hitDistance[i] = hit.distance;
						hitCompress[i] = -((hit.distance) / (suspensionRange + wheelRadius)) + 1;
						hitVelocity[i] = wheels[i].InverseTransformDirection(vehicle.myRigidbody.GetPointVelocity(transform.TransformPoint(wheelPositn[i])));
						if (isDynamic)
                        {
							hitFriction[i] = Game.Settings.buggyTr * 60;
							//Debug.DrawRay(transform.TransformPoint(wheelPositn[i]),transform.up * 5, Color.red);
							//getSpringForce(comp, vel.y) *				//Spring Compression position, normalized (0-1)
							//Mathf.Lerp(1, 1, Mathf.Min(comp * 4, 1))	//Static tire friction coeffecient, as function of downforce*/
						}
						else
                        {
							hitFriction[i] = Game.Settings.buggyTr * 150 * Mathf.Lerp(
								1.5f,
								0.5f,
								Mathf.Min(hitCompress[i] * 3, 1)
							);
						}

						Vector3 dir = new Vector3(
							hitVelocity[i].x,
							0,
							(
								Game.Settings.buggyAWD == true ||
								i > 1 ? (hitVelocity[i].z - motorSpeed) : 0
							)
						);
						if (dir.magnitude > 1) dir = dir.normalized;
						hitForce[i] = dir;
						//Debug.DrawRay(transform.TransformPoint(wheelPositn[i]),transform.right * dir.x, Color.blue);
						//Debug.DrawRay(transform.TransformPoint(wheelPositn[i]),transform.forward * dir.z, Color.blue);
						Vector3 force = wheels[i].TransformDirection(dir * -hitFriction[i]);
						//Debug.DrawRay(hit.point,force / 50);
						vehicle.myRigidbody.AddForceAtPosition(force, hit.point);
						if (wheelMarks)
                        {
							//Do Tire Tracks
							wheelMarkIndex[i] = wheelMarks.AddSkidMark(
								hit.point,
								hit.normal,
								(isDynamic ? 1 : Mathf.Min(
									0.5f,
									force.magnitude * 0.0025f
								)),
								wheelMarkIndex[i]
							);
						}
						frictionTotal += hitFriction[i];
					}
					else
                    {
						hitDistance[i] = -1;
						wheelMarkIndex[i] = -1;
					}
				}
			}

			//Modified Yoggy physics
			else
            {
				//Motor
				motorTorque = Mathf.Max(
					1f,
					Mathf.Lerp(
						Game.Settings.buggyPower * 5f,
						0f,
						motorSpeed / (Game.Settings.buggySpeed * 10f)
					) * Mathf.Abs((wingOpen ? 0 : vehicle.input.y))
				);
				motorAccel = (int)Mathf.Lerp(
					maxAcceleration,
					0f,
					motorSpeed / (Game.Settings.buggySpeed * 10f)
				);
				motorSpeed += vehicle.input.y * (float)motorAccel / (float)motorMass * Time.fixedDeltaTime;
				motorSpeed += -motorSpeed * (vehicle.brakes ? 50 : motorDrag) / motorTorque * Time.fixedDeltaTime;

				//Wheel / Terrain Collisions
				for (int i = 0; i < 4; i++)
                {
					if (Physics.Raycast(
						transform.TransformPoint(wheelPositn[i]),
						transform.up * -1f,
						out hit,
						suspensionRange + wheelRadius, vehicle.terrainMask
					))
                    {
						wheelsAreTouchingGround = true;
						hitCompress[i] = -((hit.distance) / (suspensionRange + wheelRadius)) + 1;
						hitVelocity[i] = wheels[i].InverseTransformDirection(
							vehicle.myRigidbody.GetPointVelocity(transform.TransformPoint(wheelPositn[i]))
						);
						if (hit.rigidbody)
                        {
							vehicle.myRigidbody.AddForceAtPosition(
								(hitVelocity[i] - wheels[i].InverseTransformDirection(
									hit.rigidbody.GetPointVelocity(hit.point)
								)) / 4,
								hit.point,
								ForceMode.VelocityChange
							);
							//vehicle.transform.position += (hit.rigidbody.GetPointVelocity(hit.point) * Time.fixedDeltaTime) / 4;
							//hitVelocity[i] = hit.rigidbody.GetPointVelocity(hit.point);
							//hitVelocity[i] = hit.rigidbody.GetPointVelocity(hit.point);
						}
						friction = Game.Settings.buggyTr * 9 * Mathf.Lerp(0.5f, 1, hitCompress[i]) * Mathf.Max(1, (20 - hitVelocity[i].magnitude) / 4);
						vehicle.myRigidbody.AddForceAtPosition(wheels[i].TransformDirection(Vector3.Min(new Vector3(
							-hitVelocity[i].x * friction,                   //Sideslip
							0,
							-(hitVelocity[i].z - motorSpeed) * friction     //Motor
						), new Vector3(1000, 1000, 1000))), hit.point);
						motorSpeed += ((hitVelocity[i].z - motorSpeed) * friction * Time.fixedDeltaTime) / motorTorque;
						if (wheelMarks)
                        {
							//Do Tire Tracks
							wheelMarkIndex[i] = wheelMarks.AddSkidMark(
								hit.point,
								hit.normal,
								(Mathf.Abs(hitVelocity[i].x) > Mathf.Abs(hitVelocity[i].z) * 0.3f ?
									Mathf.Abs(vehicle.input.y) * 0.5f + 0.25f :
									Mathf.Min(0.5f, friction * 0.05f)
								),
								wheelMarkIndex[i]
							);
						}
					}
					else
                    {
						hit.distance = -1f;
						wheelMarkIndex[i] = -1;
					}
					hitDistance[i] = hit.distance;
				}
			}

			//Suspension
			for (int i = 0; i < 4; i++)
            {
				if (hitDistance[i] == -1) continue;
				vehicle.myRigidbody.AddForceAtPosition(
					transform.up * (-hitVelocity[i].y * Game.Settings.buggySh * 1 * (wingOpen ? 3 : 1) + hitCompress[i] * (20 * vehicle.myRigidbody.mass) * (wingOpen && i < 2 ? 8 : 1)),
					transform.TransformPoint(wheelPositn[i])
				);
			}

			//Floating
			if (
				(transform.position.y < Game.Settings.lavaAlt + 0.1f && transform.position.y - Game.Settings.lavaAlt > -3f) ||
				Physics.Raycast(transform.position + Vector3.up * 3f, Vector3.down, out hit, 3.1f, 1 << 4)
			)
            {
				//Vars
				if (wingOpen && hit.distance < 2f)
                {
					vehicle.myRigidbody.AddForce(Vector3.up * 400f);
				}
				float roll = (transform.eulerAngles.z > 180 ?
					transform.eulerAngles.z - 360 :
					transform.eulerAngles.z
				);
				float pitch = (transform.eulerAngles.x > 180 ?
					transform.eulerAngles.x - 360 :
					transform.eulerAngles.x
				);
				vehicle.myRigidbody.angularDrag = 2f;

				//Flowing Lava
				float waterAngle = default(float);
				Vector3 waterAxis = default(Vector3);
				if (hit.distance != 0f && (bool)hit.transform)
                {
					hit.transform.rotation.ToAngleAxis(out waterAngle, out waterAxis);
					if (waterAngle != 0f)
                    {
						vehicle.myRigidbody.AddForce(hit.transform.rotation.eulerAngles * 0.8f);
					}
				}

				//BouyancyPoints
				int i = 0;
				Transform[] m = bouyancyPoints;
				for (int length = m.Length; i < length; i++)
                {
					if (
						m[i].position.y < Game.Settings.lavaAlt ||
						Physics.Raycast(
							m[i].position + (Vector3.up * 3f),
							Vector3.down,
							out hit,
							3f,
							1 << 4
						)
					)
                    {
						float bouyancyY = (hit.distance != 0f ?
							hit.distance - 5 :
							m[i].position.y - 2 - Game.Settings.lavaAlt
						);
						if (bouyancyY < -1.8f)
                        {
							bouyancyY = -1.8f;
						}
						vehicle.myRigidbody.AddForceAtPosition((new Vector3(
							0f,
							-bouyancyY * (100f + vehicle.myRigidbody.GetPointVelocity(m[i].position).magnitude * (float)((!(vehicle.myRigidbody.GetPointVelocity(m[i].position).magnitude > 15f)) ? 15 : 100)),
							0f
						) + vehicle.myRigidbody.GetPointVelocity(m[i].position) * -200f) / bouyancyPoints.Length, m[i].position);
					}
				}
				if (vehicle.input.y >= 0f)
                {
					vehicle.myRigidbody.AddRelativeTorque(new Vector3(
						vehicle.input.y * -1f * 500f * ((70f - Mathf.Min(70f, Mathf.Max(1f, pitch * -1f))) / 70f),
						vehicle.input.y * vehicle.input.x * 300f,
						roll * -3f + vehicle.input.y * vehicle.input.x * -50f
					));
				}
				if (!wingOpen && hit.distance < 3f)
                {
					vehicle.myRigidbody.AddRelativeForce(
						Vector3.forward * vehicle.input.y * 1200f
					);
				}
			}

			//Diving
			else if (
				transform.position.y < Game.Settings.lavaAlt ||
				Physics.Raycast(
					transform.position + Vector3.up * 200f,
					Vector3.down, 200f,
					1 << 4
				)
			)
            {
				vehicle.myRigidbody.AddForce(vehicle.myRigidbody.velocity * -8f + Vector3.up * (wingOpen ? 400 : 200));
				vehicle.myRigidbody.angularDrag = 2f;
			}

			//Collision Friction
			if (
				wingOpen ||
				wheelsAreTouchingGround ||
				Physics.Raycast(
					transform.position,
					transform.up * -1f,
					3f,
					vehicle.terrainMask
				)
			)
            {
				buggyCollider.material.frictionCombine = PhysicMaterialCombine.Minimum;
			}
			else
            {
				buggyCollider.material.frictionCombine = PhysicMaterialCombine.Maximum;
			}
		}
	}

	//Self righting
	public void OnCollisionStay(Collision collision)
    {
		if (vehicle.zorbBall)
        {
			return;
		}
		int i = 0;
		ContactPoint[] contact = collision.contacts;
		for (int length = contact.Length; i < length; i = checked(i + 1))
        {
			if (
				isInverted &&
				Vector3.Angle(transform.up, contact[i].normal) < 50f
			)
            {
				isInverted = false;
			}
			else if (
				!isInverted &&
				vehicle.myRigidbody.angularVelocity.sqrMagnitude < 5f &&
				!wingOpen
				&& Vector3.Angle(transform.up, contact[i].normal) > 120f
			)
            {
				isInverted = true;
			}
			if (isInverted)
            {
				vehicle.myRigidbody.AddTorque(
					Vector3.Cross(
						transform.up,
						Vector3.up
					) * Vector3.Angle(transform.up, Vector3.up) * 3f
				);
			}
		}
	}

    public IEnumerator OnSetSpecialInput()
    {
        while (!vehicle) yield return null;

        vehicle.camSmooth = vehicle.specialInput;
        if (vehicle.specialInput)
        {
            wingState = 1;
            //wingFlaps = 0;
        }
        else
        {
            wingState = -1;
            //wingFlaps = 0;
        }
    }

	public void OnDisable()
    {
		if ((bool)wheelMarks)
        {
			UnityEngine.Object.Destroy(wheelMarks.gameObject);
		}
	}

	public void OnLOD(int level)
    {
		for (int i = 0; i < 4; i++)
        {
			wheelGraphics[i].Find("Detailed").gameObject.SetActiveRecursively(level == 0);
			wheelGraphics[i].Find("Simple").gameObject.active = level != 0;
			axels[i].gameObject.SetActiveRecursively(level == 0);
		}
	}
}