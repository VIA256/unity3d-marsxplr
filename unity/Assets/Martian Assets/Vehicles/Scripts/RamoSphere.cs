using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class RamoSphere : MonoBehaviour
{
	public Transform shield;
	private float offset;
	public Color tagColor;
	public Color ramColor;
	public bool ram = false;
	public Vehicle vehicle;

	public void Start()
	{
		shield.renderer.material.color = tagColor;
		vehicle = (Vehicle)transform.root.gameObject.GetComponent(typeof(Vehicle));
	}

	public void Update()
	{
		offset += Time.deltaTime * 0.1f;
		if (offset > 1f)
		{
			offset -= Mathf.Floor(offset);
		}

		shield.renderer.material.SetFloat("_Offset", offset);

		Color srmcol = shield.renderer.material.color;
        srmcol.a = Mathf.Lerp(
            shield.renderer.material.color.a,
            (ram) ? ramColor.a : tagColor.a,
            Time.deltaTime * 3f);
		shield.renderer.material.color = srmcol;

		shield.rotation = Quaternion.identity;
	}

    public IEnumerator colorSet(bool r)
    {
        yield return new WaitForFixedUpdate(); //bizarre, but necessary...
        if (r) shield.renderer.material.color = ramColor;
        else shield.renderer.material.color = tagColor;

        Color srmcol = shield.renderer.material.color;
        srmcol.a = 0f;
        shield.renderer.material.color = srmcol;

        ram = r;
    }

	public void OnCollisionEnter(Collision collision)
	{
		float i = collision.relativeVelocity.magnitude *
            Mathf.Abs(Vector3.Dot(
                collision.contacts[0].normal,
                collision.relativeVelocity.normalized));
		if (i > 3f)
		{
			Color srmcol = shield.renderer.material.color;
            srmcol.a = ((ram) ? ramColor.a : tagColor.a) + i * 0.1f;
			shield.renderer.material.color = srmcol;
		}
	}

	public void OnTriggerStay(Collider other)
	{
        if (other.gameObject.layer == 14) return;
		if (
            other.name == "ORB(Clone)" &&
            shield.renderer.material.color.a < 3f)
		{
			Color srmcol = shield.renderer.material.color;
			srmcol.a = 3f;
			shield.renderer.material.color = srmcol;
		}
        if (!vehicle.networkView.isMine) return;
		if ((bool)other.attachedRigidbody)
		{
			vehicle.OnRam(other.attachedRigidbody.gameObject);
		}
	}

	public void OnTriggerEnter(Collider other)
	{
        if (other.gameObject.layer == 14) return;
        if (other.name == "ORB(Clone)")
        {
            Color srmcol = shield.renderer.material.color;
            srmcol.a = 10f;
            shield.renderer.material.color = srmcol;
        }
	}

	public void OnLaserHit()
	{
        Color srmcol = shield.renderer.material.color;
        srmcol.a = 5f;
        shield.renderer.material.color = srmcol;
	}
}
