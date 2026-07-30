using System;
using UnityEngine;
using System.Collections;

[Serializable]
public class WhirldObject : MonoBehaviour {

	public WhirldData[] data;
	public Hashtable parameters;

	public void Awake()
	{
		Activate();
	}

	public void OnSceneGenerated()
	{
		Activate();
	}

    //Futile, as HashTables aren't serialized inside AssetBUndles anyway ;(
    /*public void OnSaveScene()
    {
        Activate();
        data = null;
    }*/

	public void Activate()
	{
		if (parameters != null || data.Length <= 0)
		{
			return;
		}

		parameters = new Hashtable();
        foreach (WhirldData dat in data)
        {
            if (dat.o) parameters.Add(dat.n, dat.o);
            else parameters.Add(dat.n, dat.v);
        }
	}
}