using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class JumpPoint : MonoBehaviour
{
    public WhirldObject whirldObject;
	public GameObject smokeObject;
    private float time = 1;
    private int randMin = 0;
    private int randMax = 0;
    private int velocity = 50;
    private float lastBlast;

    public IEnumerator Start()
    {
        if (!(bool)whirldObject || whirldObject.parameters == null)
        {
            yield break;
        }
        if (whirldObject.parameters["JumpTime"] != null)
        {
            time = float.Parse((String)whirldObject.parameters["JumpTime"]);
        }
        if (whirldObject.parameters["JumpRandMin"] != null)
        {
            randMin = (int)float.Parse((String)whirldObject.parameters["JumpRandMin"]);
        }
        if (whirldObject.parameters["JumpRandMax"] != null)
        {
            randMax = (int)float.Parse((String)whirldObject.parameters["JumpRandMax"]);
        }
        if (whirldObject.parameters["JumpVelocity"] != null)
        {
            velocity = (int)float.Parse((String)whirldObject.parameters["JumpVelocity"]);
        }

		yield return new WaitForSeconds(15);

		if(!(bool)smokeObject) yield break;
		ParticleEmitter pE = (ParticleEmitter)smokeObject.GetComponent(typeof(ParticleEmitter));
		if(!pE) yield break;
		pE.emit = true;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 14) return;
        lastBlast = Time.time + (float)time;
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 14) return;
        if (Time.time - 0.1f < lastBlast) return;
        lastBlast = Time.time;
        if (randMin != 0 && randMax != 0)
        {
            other.attachedRigidbody.AddForce(
                transform.up * UnityEngine.Random.Range(randMin, randMax),
                ForceMode.VelocityChange);
        }
        else
        {
            other.attachedRigidbody.AddForce(
                transform.up * velocity,
                ForceMode.VelocityChange);
        }
        /*other.attachedRigidbody.AddExplosionForce(
            UnityEngine.Random.Range(randMin, randMax),
            transform.position,
            0f,
            2f,
            ForceMode.VelocityChange);*/
    }
}

