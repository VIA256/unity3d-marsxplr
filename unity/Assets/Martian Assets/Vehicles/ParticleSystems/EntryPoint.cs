using System;
using System.Collections;
using UnityEngine;

public class EntryPoint : MonoBehaviour
{
	public IEnumerator Start()
	{
		yield return new WaitForSeconds(15);
		
		ParticleEmitter pe = (ParticleEmitter)GetComponent(typeof(ParticleEmitter));
		pe.emit = true;
	}
}

/* old .js



function Start () {
	yield new WaitForSeconds(15);
	
	var pe : ParticleEmitter = GetComponent(ParticleEmitter);
	pe.emit = true; 
}



*/