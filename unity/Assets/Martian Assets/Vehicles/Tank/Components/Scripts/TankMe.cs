using System;
using UnityEngine;

[Serializable]
public class TankMe : MonoBehaviour
{
	public Vehicle vehicle;
	public bool bellyup = false;

	public void FixedUpdate()
	{
        if (vehicle.myRigidbody.isKinematic) return; //We are materializing, don't try to manipulate physics
		
        if (vehicle.brakes && vehicle.myRigidbody.velocity.magnitude < 3f)
		{
			vehicle.myRigidbody.velocity = Vector3.zero;
			vehicle.myRigidbody.angularVelocity = Vector3.zero;
			if (vehicle.input.y != 0f)
			{
				vehicle.myRigidbody.drag = 5f;
			}
			else
			{
				vehicle.myRigidbody.drag = 10000f;
			}
		}
		else
		{
			vehicle.myRigidbody.angularDrag = 2f;
			vehicle.myRigidbody.drag = 0.01f;
		}

		if (bellyup && Vector3.Angle(transform.up, Vector3.up) < 35f)
		{
			bellyup = false;
		}
	}

	public void OnCollisionStay(Collision collision)
	{
        if (vehicle.zorbBall) return;
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.otherCollider.gameObject.layer != 0) continue;
            if (
                bellyup == false &&
                vehicle.myRigidbody.velocity.sqrMagnitude < 20 &&
                vehicle.myRigidbody.angularVelocity.sqrMagnitude < 5 &&
                Vector3.Angle(transform.up, contact.normal) > 130)
            {
                bellyup = true;
            }

            if (bellyup)
            {
                vehicle.myRigidbody.AddForce(Vector3.up * 5000f);
                vehicle.myRigidbody.AddTorque(
                    Vector3.Cross(transform.up, Vector3.up) * 100000f,
                    ForceMode.Acceleration);
            }
        }
	}

	public void OnSetSpecialInput() {}
}
