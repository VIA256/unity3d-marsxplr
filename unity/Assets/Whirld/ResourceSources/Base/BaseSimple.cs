using System;
using UnityEngine;

[Serializable]
public class BaseSimple : MonoBehaviour
{
	private Material mat;
	public bool upMode;

	public void Start()
    {
        mat = GetComponent<MeshRenderer>().material;
		Vector3 mts = mat.mainTextureScale;
		mts.x = 1;
		mts.y = 0.1f;
		mat.mainTextureScale = mts;
	}

	public void Update()
    {
		transform.localScale = Vector3.one * Mathf.Max(
			0.5f,
			Mathf.Min(
				10,
				Vector3.Distance(
                    transform.position,
                    Camera.main.transform.position) / 10f
			)
		);

		Vector3 lea = transform.localEulerAngles;
		lea.y += Time.deltaTime * 10;
		if(lea.y > 360)
        {
			lea.y -= 360;
		}
        transform.localEulerAngles = lea;

		Vector3 mto = mat.mainTextureOffset;
		mto.x += Time.deltaTime * 0.5f;
		if(mto.x > 1)
        {
			mto.x--;
		}

        if (upMode)
        {
            mto.y += Time.deltaTime * 0.1f;
            if (mto.y > 0.6f) upMode = false;
        }
        else
        {
            mto.y -= Time.deltaTime * 0.1f;
            if (mto.y < 0.4f) upMode = true;
        }
		mat.mainTextureOffset = mto;
	}
}
