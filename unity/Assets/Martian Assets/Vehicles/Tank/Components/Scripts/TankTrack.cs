using System;
using UnityEngine;

[Serializable]
public class TankTrack : MonoBehaviour
{
	private float motorMinSpeed = 100.0f;
	private float motorMaxAccel = 0.00f;
	private float motorAccelTime = 2.50f;
	private float motorPower = 0.00f;
	private float motorSpeed = 0.00f;
	private float motorSpeedNew = 0.00f;
	private int sideSlipDragForce = 150;
	private int linearDragForce = 50;
	
    // /*UNUSED*/ private ContactPoint hit;
	private Transform myTransform;
	// /*UNUSED*/ private Texture myTexture;
	public Renderer TreadTex;
	public Tank vehicle;
	public bool rightSide = false;
	public ConfigurableJoint joint;
	public float offset;
	public Vector3 strtPos;

	public void Start()
	{
		myTransform = transform;
		vehicle = (Tank)transform.parent
            .transform.parent
            .gameObject.GetComponent<Tank>();
		joint.connectedBody = vehicle.vehicle.GetComponent<Rigidbody>();
		strtPos = transform.localPosition;
	}

	public void OnEnable()
	{
        if (!(bool)joint.connectedBody) return;
		GetComponent<Rigidbody>().isKinematic = true;
		transform.localRotation = Quaternion.identity;
		transform.localPosition = strtPos;
		if (joint.anchor != Vector3.zero)
		{
			joint.anchor = Vector3.zero;
		}
		GetComponent<Rigidbody>().isKinematic = false;
		GetComponent<Rigidbody>().velocity = Vector3.zero;
	}

	public void OnCollisionStay(Collision collision)
	{
		if (
            vehicle.vehicle.zorbBall ||
            collision.collider.transform.root == transform.root)
		{
			return;
		}
		if (
            (bool)collision.transform.root &&
            (bool)collision.transform.root.gameObject.GetComponent<Rigidbody>())
		{
            vehicle.vehicle.OnRam(collision.transform.root.gameObject); //we hit a tank track or something
		}
		else if ((bool)collision.rigidbody)
		{
            vehicle.vehicle.OnRam(collision.gameObject);
		}
        else foreach (ContactPoint hit in collision.contacts)
        {
            if (
                hit.otherCollider.gameObject.layer == 0 ||
                hit.otherCollider.gameObject.layer == 11)
            {
                gotDirt(hit, collision);
                return;
            }
        }
	}

	public void gotDirt(ContactPoint hit, Collision collision)
	{
		if (transform.InverseTransformPoint(hit.point).y > 0f)
		{
            //Don't let the upper tip collide with terrain - we get launched for some reason
			return;
		}

		Vector3 locVel = vehicle.vehicle.myRigidbody.GetPointVelocity(hit.point);
		if (locVel.magnitude > 1000f)
		{
			Debug.Log("Crazy Track LocVel: " + locVel);
			locVel = locVel.normalized * 1000f;
		}
		locVel = myTransform.InverseTransformDirection(locVel);

		offset += Time.deltaTime * locVel.z * -1.2f;
		if (offset > 1f)
		{
			offset -= 1f;
		}
		if (offset < 0f)
		{
			offset += 1f;
		}
		TreadTex.material.SetTextureOffset(
            "_MainTex",
            new Vector2(0f, offset));
		TreadTex.material.SetTextureOffset(
            "_BumpMap",
            new Vector2(0f, offset));

		if (vehicle.vehicle.input.y > 0f)   //Forward
		{
			if (motorSpeed < 0f)
			{
				motorSpeed *= -1f;
			}
			if (motorSpeed < motorMinSpeed)
			{
				motorSpeed = motorMinSpeed;
			}
			motorSpeedNew = Game.Settings.tankPower *
                (Mathf.Max(
                    0.1f,
                    Game.Settings.tankSpeed - locVel.z) /
                    Game.Settings.tankSpeed);
			if (motorSpeedNew > motorSpeed)
			{
				motorSpeed = Mathf.SmoothDamp(
                    motorSpeed,
                    motorSpeedNew,
                    ref motorMaxAccel,
                    motorAccelTime);
			}
			else
			{
				motorSpeed = motorSpeedNew;
			}
			motorPower = motorSpeed;

			if (vehicle.vehicle.input.x != 0f)
			{
				if (rightSide && vehicle.vehicle.input.x > 0f)
				{
					motorPower = 0f;
				}
				else if (!rightSide && vehicle.vehicle.input.x < 0f)
				{
					motorPower = 0f;
				}
			}
		}
		else if (vehicle.vehicle.input.y < 0f)  //Reverse
		{
			if (motorSpeed > 0f)
			{
				motorSpeed *= -1f;
			}
			if (motorSpeed > -motorMinSpeed)
			{
				motorSpeed = -motorMinSpeed;
			}
			motorSpeedNew = -Game.Settings.tankPower *
                (Mathf.Max(0.1f,
                    Game.Settings.tankSpeed + locVel.z) /
                    Game.Settings.tankSpeed);
			if (motorSpeedNew < motorSpeed)
			{
				motorSpeed = Mathf.SmoothDamp(
                    motorSpeed,
                    motorSpeedNew,
                    ref motorMaxAccel,
                    motorAccelTime);
			}
			else
			{
				motorSpeed = motorSpeedNew;
			}
			motorPower = motorSpeed;

			if (vehicle.vehicle.input.x != 0f)
			{
				if (rightSide && vehicle.vehicle.input.x > 0f)
				{
					motorPower = 0f;
				}
				else if (!rightSide && vehicle.vehicle.input.x < 0f)
				{
					motorPower = 0f;
				}
			}
		}
		else                            //Spin
        {
            if (vehicle.vehicle.input.x != 0f)
		    {
			    motorPower = Game.Settings.tankPower * vehicle.vehicle.input.x * (0.5f * -1f) * (float)(rightSide ? 1 : (-1));
		    }
		    else
		    {
			    motorPower = 0f;
		    }
        }

		Vector3 force = new Vector3(
            locVel.x * (float)(-sideSlipDragForce),     //Side
            10f,                                        //Vertical
            ((motorPower < -0.1f) || (motorPower > 0.1f)) ?
                motorPower :
                (locVel.z * (float)(-linearDragForce)));
		GetComponent<Rigidbody>().AddForceAtPosition(
            Quaternion.LookRotation(Vector3.Cross(
                transform.right,
                hit.normal)) *
                force,
            new Vector3(transform.position.x, hit.point.y, hit.point.z) +
                transform.TransformDirection(
                    Vector3.up * Game.Settings.tankGrip));
	}
}
