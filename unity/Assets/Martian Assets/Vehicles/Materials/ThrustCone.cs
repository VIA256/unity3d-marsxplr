using System;
using UnityEngine;

[Serializable]
public class ThrustCone : MonoBehaviour
{
	public Vehicle vehicle;
	public Material mat;

	public float magSteer = 15f;
	public float magThrottle = 1f;
	public float minThrottle = 0.2f;

	public void Start()
	{
		vehicle = (Vehicle)gameObject
            .transform.root
            .gameObject.GetComponentInChildren(typeof(Vehicle));
	}

	public void Update()
	{
		Vector2 mto = mat.mainTextureOffset;
        mto.y = mat.mainTextureOffset.y - Time.deltaTime * 0.8f;
		mat.mainTextureOffset = mto;

		if (mat.mainTextureOffset.y < -0.5f)
		{
			Vector2 mto2 = mat.mainTextureOffset;
            mto2.y = mat.mainTextureOffset.y + 0.1f;
			mat.mainTextureOffset = mto2;
		}

		if (magSteer > 0f)
		{
			Vector3 lea = transform.localEulerAngles;
            lea.y = vehicle.input.x * -magSteer;
			transform.localEulerAngles = lea;
		}
		if (magThrottle > 0f)
		{
			Vector3 localScale = transform.localScale;
            localScale.y = Mathf.Max(
                minThrottle,
                (vehicle.inputThrottle ?
                    vehicle.input.z :
                    vehicle.input.y) *
                    magThrottle);
			transform.localScale = localScale;
		}
		else
		{
			Vector3 localScale = transform.localScale;
            localScale.y = minThrottle;
			transform.localScale = localScale;
		}
	}
}
