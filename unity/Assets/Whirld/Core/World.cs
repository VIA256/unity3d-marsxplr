using System;
using UnityEngine;

[Serializable]
public class World : MonoBehaviour
{
	public static int lodDist;
	public static Transform baseTF;
	public static Terrain[] terrains;
	public static Transform sea;
	public static Color seaFogColor;
	public static Color seaGlowColor;
}
