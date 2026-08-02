using System;
using UnityEngine;

[Serializable]
public enum SeaModes
{
	unset = 99,
	Tropic = 0,
	Lava = 1
}

[Serializable]
public class SeaModeData : System.Object
{
	public string name;
	public Color color;
	public Color glowColor;
	public float waves;
	public float reflection;
	public float refraction;
}

[Serializable]
[ExecuteInEditMode]
public class SeaData : MonoBehaviour
{
	public WhirldObject whirldObject;
	public GameObject seaObject;
	public GameObject seaObjectSimple;
	public GameObject seaObjectSimBot;

	public SeaModes SeaMode = SeaModes.Tropic;
	private SeaModes setMode = SeaModes.unset;
	public SeaModeData[] seaModeData;

	public void Start()
	{
        if (
            whirldObject == null ||
            whirldObject.parameters == null ||
            seaObject == null ||
            whirldObject.parameters["Mode"] == null)
        {
            return;
        }
        SeaMode = (SeaModes)Enum.Parse(
            typeof(SeaModes),
            whirldObject.parameters["Mode"].ToString(),
            true);
	}

	public void Update()
	{
		if (SeaMode != setMode)
		{
			SetSeaMode();
		}
	}

	public void SetSeaMode()
	{
        setMode = SeaMode;
        seaObject.GetComponent<Renderer>().sharedMaterial.SetColor(
            "_RefrColor",
            seaModeData[(int)SeaMode].color);
        seaObject.GetComponent<Renderer>().sharedMaterial.SetFloat(
            "_WaveScale",
            seaModeData[(int)SeaMode].waves);
        seaObject.GetComponent<Renderer>().sharedMaterial.SetFloat(
            "_ReflDistort",
            seaModeData[(int)SeaMode].reflection);
        seaObject.GetComponent<Renderer>().sharedMaterial.SetFloat(
            "_RefrDistort",
            seaModeData[(int)SeaMode].refraction);
        seaObjectSimple.GetComponent<Renderer>().sharedMaterial.SetColor(
            "_Color",
            seaModeData[(int)SeaMode].color);
        seaObjectSimBot.GetComponent<Renderer>().sharedMaterial.SetColor(
            "_Color",
            seaModeData[(int)SeaMode].glowColor);

        Color soscol = seaObjectSimple.GetComponent<Renderer>().sharedMaterial.color;
		soscol.a = 0.67f;//0.85f;
        seaObjectSimple.GetComponent<Renderer>().sharedMaterial.color = soscol;

        Color sosbcol = seaObjectSimBot.GetComponent<Renderer>().sharedMaterial.color;
		sosbcol.a = 0.67f;//0.85f;
        seaObjectSimBot.GetComponent<Renderer>().sharedMaterial.color = sosbcol;

        World.seaFogColor = seaModeData[(int)SeaMode].color;
        World.seaGlowColor = seaModeData[(int)SeaMode].glowColor;
	}
}
