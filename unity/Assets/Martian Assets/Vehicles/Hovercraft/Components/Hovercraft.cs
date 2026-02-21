using System;
using UnityEngine;

[Serializable]
public class Hovercraft : MonoBehaviour
{
	public LayerMask thrustMask;
	public Vehicle vehicle;
	// /*unused*/ private RaycastHit hitDown;
	private float thrustLast;
	private float hoverHeight;

	public Hovercraft()
    {
		thrustMask = -1;
	}

	public void InitVehicle(Vehicle veh)
    {
		vehicle = veh;
	}

	public void FixedUpdate()
    {
		if (vehicle.myRigidbody.isKinematic)
        {
			//We are materializing, don't try to manipulate physics
			return;
		}

		Rigidbody mrb = vehicle.myRigidbody;
		Vector3 cm = mrb.centerOfMass;
		cm.x = 0.0f;
		cm.z = 0.0f;
		cm.y = -0.3f;
		mrb.centerOfMass = cm;
		mrb.inertiaTensor = new Vector3(15.0f, 15.0f, 15.0f);
		mrb.mass = 50;
		vehicle.myRigidbody = mrb;
		
		Vector3 locvel = vehicle.myRigidbody.transform.InverseTransformDirection(
			vehicle.myRigidbody.velocity
		);
		
		if (!vehicle.brakes)
        {
			vehicle.myRigidbody.drag = 0f;
			vehicle.myRigidbody.angularDrag = 4f;
			vehicle.myRigidbody.AddRelativeForce(new Vector3(
				locvel.x * -10f,
				0f,
				((vehicle.input.y > 0f) ?
					(vehicle.input.y * Game.Settings.hoverThrust) :
					0
				)
			));
		}
		else
        {
			if (vehicle.myRigidbody.velocity.magnitude > 5f)
            {
				vehicle.myRigidbody.drag = 1.5f;
			}
			else
            {
				vehicle.myRigidbody.drag = 100f;
			}
			vehicle.myRigidbody.angularDrag = 20f;
		}
		
		RaycastHit hit = default(RaycastHit);
		if (
			Physics.Raycast(
				transform.position,
				Vector3.up * -1f,
				out hit,
				30f,
				thrustMask
			) ||
			transform.position.y < Game.Settings.lavaAlt + 30f
		)
        {
			if (
				hit.distance == 0f ||
				hit.distance > Mathf.Max(
					0f,
					transform.position.y - Game.Settings.lavaAlt
				) ||
				transform.position.y < Game.Settings.lavaAlt
			)
            {
				hit.distance = Mathf.Max(
					0f,
					transform.position.y - Game.Settings.lavaAlt
				);
				hit.normal = Vector3.up;
			}
			
			hoverHeight = (
				(Physics.Raycast(
					transform.position,
					Vector3.up,
					5f,
					thrustMask
				) ?
					//Duck!
					((hoverHeight > 5f) ?
						(hoverHeight - Time.deltaTime * 3f) :
						5f
					) :
					//Cruise
					((hoverHeight < Game.Settings.hoverHeight) ?
						(hoverHeight + Time.deltaTime * 3f) :
						Game.Settings.hoverHeight
					)
				)
			);
			
			if (hit.distance < hoverHeight)
            {
				vehicle.myRigidbody.AddForce( //Hover force
					transform.up * (hoverHeight - hit.distance) * Game.Settings.hoverHover
				);
				if (thrustLast > hit.distance)
                {
					vehicle.myRigidbody.AddForce( //AntiCollision force
						hit.normal * Mathf.Min(
							(thrustLast - hit.distance) * Game.Settings.hoverRepel,
							10f
						),
						ForceMode.VelocityChange
					);
				}
			}
			vehicle.myRigidbody.AddTorque(
				Vector3.Cross(transform.up, hit.normal) *
				Vector3.Angle(transform.up, hit.normal) *
				0.2f *
				(40f - Mathf.Min(40f, hit.distance))
			);
			thrustLast = hit.distance;
		}
		else
        {
			vehicle.myRigidbody.angularDrag = 0.5f;
		}

		//Steering
		vehicle.myRigidbody.AddRelativeTorque(new Vector3(
			vehicle.input.y * 30f,
			vehicle.input.x * 100f,
			((vehicle.input.x > 0f) ? 
				(
					(
						(
							(transform.eulerAngles.z > 180f) ?
								(transform.eulerAngles.z - 360f) :
								transform.eulerAngles.z
						) < 40f
					) ? vehicle.input.x : 0f
				) :
				(
					(
						(
							(transform.eulerAngles.z > 180f) ?
								(transform.eulerAngles.z - 360f) :
								transform.eulerAngles.z
						) > -40f
					) ? vehicle.input.x : 0f
				)
			) * -200f
		));
	}
}
