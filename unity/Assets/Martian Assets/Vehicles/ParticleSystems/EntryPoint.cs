using System;
using System.Collections;
using UnityEngine;

public class EntryPoint : MonoBehaviour
{
	public IEnumerator Start()
	{
		yield return new WaitForSeconds(15);
		
		ParticleSystem ps = (ParticleSystem)GetComponent(typeof(ParticleSystem));
		ps.Play();
	}
}