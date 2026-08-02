using System;
using UnityEngine;

[Serializable]
public class WhirldLOD : MonoBehaviour
{
	public GameObject[] lodObjs;
	public int lodLevMax = 0;

	[HideInInspector]
	public int level = 0;
	private int lastLevel = -1;
	private float lodCheck;

	public void Start()
	{
		lodCheck = UnityEngine.Random.Range(30, 60) / 10;
		InvokeRepeating("SetLOD", 0f, lodCheck);
	}

	public void SetLOD()
	{
		
        //Level determined by distance to cam
		if (lodLevMax == 0)
		{
			level = (World.lodDist > 0 ?
                (int)(Mathf.Lerp(
                    0,
                    lodObjs.Length - 1,
                    Vector3.Distance(
                        transform.position,
                        Camera.main.transform.position) / World.lodDist)) :
                lodObjs.Length - 1);
		}

        //Level determined directly by game quality level
		else
		{
			level = (((int)QualitySettings.GetQualityLevel() >= lodLevMax) ? 0 : 1);
		}

        if (lastLevel != level)
        {
            BroadcastMessage(
                "OnLOD",
                level,
                SendMessageOptions.DontRequireReceiver);
            for (int i = 0; i < lodObjs.Length; i++)
            {
                bool desired = (i == level);
                if (lodObjs[i].activeInHierarchy != desired)
                {
                    lodObjs[i].SetActive(desired);
                }
            }
            lastLevel = level;
        }
	}
}
