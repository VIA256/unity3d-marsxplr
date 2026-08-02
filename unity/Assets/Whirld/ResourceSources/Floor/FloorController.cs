using System;
using UnityEngine;

[Serializable]
public class FloorController : MonoBehaviour
{
	public WhirldObject whirldObject;
	public GameObject floorObject;

	public void OnSceneGenerated()
    {
		if (
			!whirldObject ||
			whirldObject.parameters["Texture"] == null ||
			whirldObject.parameters["Texture"] as Texture == null ||
			!floorObject
		)
        {
			return;
		}

        floorObject.renderer.material.mainTexture = (Texture)(whirldObject.parameters["Texture"]);
	}
}