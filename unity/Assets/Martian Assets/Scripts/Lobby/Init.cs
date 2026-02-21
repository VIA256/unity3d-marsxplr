using System;
using UnityEngine;

[Serializable]
public class Init : MonoBehaviour
{
	public GUIText txt;

	//----------------------
	//	temporary fix for texture/font rendering issues on super high res moniters
	//  resolution is capped to 1920x1080
	public void Start()
	{
		int wnew = -1;
		int hnew = -1;
		foreach(Resolution r in Screen.resolutions)
		{
			if(
				r.width > 1920 || r.width < wnew ||
				r.height > 1080 || r.height < hnew)
			{
				continue;
			}
			wnew = r.width;
			hnew = r.height;
		}
		Screen.SetResolution(wnew, hnew, Screen.fullScreen);
	}
	//----------------------

	public void Update()
    {
		float i = Application.GetStreamProgressForLevel(Application.loadedLevel + 1);
		if (i == 1f)
        {
			Application.LoadLevel(Application.loadedLevel + 1);
		}
		else
        {
			txt.text = Mathf.RoundToInt(i * 100f) + "%";
		}
	}
}
