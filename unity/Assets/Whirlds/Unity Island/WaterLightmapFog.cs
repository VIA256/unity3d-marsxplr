using System;
using UnityEngine;

[Serializable]
public class WaterLightmapFog : MonoBehaviour
{
	public float fogDensity = 0.00f;
	public Color fogColor;
	public Color baseColor;
	public float baseMultBlurPixels = 0.00f;
	public float blurOverDrive = 0.00f;
	public float depthAmbient = 1.50f;
	
    public Vector3 terrainSize;
	public Collider terrainCollider;
	public Texture2D texture;

	[ContextMenu("Apply Fog")]
	public void ApplyFog()
	{
        Texture2D bColorTex = new Texture2D(texture.width, texture.height);
        float x = 0.00f;
        float y = 0.00f;
        while (x < texture.width)
        {
            y = 0.00f;
            while (y < texture.height)
            {
                Vector3 vect = new Vector3(
                    (float)(x / texture.width) * terrainSize.x,
                    400.00f,
                    (float)(y / texture.height) * terrainSize.y);
                RaycastHit hit;
                if (terrainCollider.Raycast(
                    new Ray(vect, Vector3.up * -500f),
                    out hit,
                    500f))
                {
                    float depth = 35.35f - hit.point.y;
                    if (x == 256) print(vect);
                    if (depth > 0f)
                    {
                        Color lightCol = texture.GetPixel((int)x, (int)y);
                        Color curCol = Color.Lerp(
                            lightCol,
                            Color.gray,
                            depthAmbient * depth * fogDensity);
                        Vector3 fog = new Vector3(
                            Mathf.Pow(fogColor.r, depth * fogDensity),
                            Mathf.Pow(fogColor.g, depth * fogDensity),
                            Mathf.Pow(fogColor.b, depth * fogDensity));
                        texture.SetPixel((int)x, (int)y, new Color(
                            curCol.r * fog.x * lightCol.a,
                            curCol.g * fog.y * lightCol.a,
                            curCol.b * fog.z * lightCol.a,
                            curCol.a));
                        bColorTex.SetPixel((int)x, (int)y, new Color(
                            baseColor.r,
                            baseColor.g,
                            baseColor.b,
                            1f));
                    }
                    else
                    {
                        bColorTex.SetPixel((int)x, (int)y, Color.white);
                    }
                }
                y++;
            }
            x++;
        }

        x = 0.00f;
        while (x < texture.width)
        {
            y = 0.00f;
            while (y < texture.height)
            {
                Color curCol = texture.GetPixel((int)x, (int)y);

                float lerp;
                float pix;
                if (baseMultBlurPixels > 0)
                {
                    lerp = (1.00f / (4.00f * baseMultBlurPixels)) * (1f + blurOverDrive);
                    pix = baseMultBlurPixels;
                }
                else
                {
                    lerp = 1.00f;
                    pix = baseMultBlurPixels;
                }

                Color temp = bColorTex.GetPixel(
                    (int)Mathf.Clamp(x, 0, texture.width - 1),
                    (int)Mathf.Clamp(y, 0, texture.width - 1));
                curCol = Color.Lerp(
                    curCol,
                    new Color(
                        curCol.r * temp.r,
                        curCol.g * temp.g,
                        curCol.b * temp.b,
                        curCol.a),
                    lerp);
                while (pix > 0)
                {
                    temp = bColorTex.GetPixel(
                        (int)Mathf.Clamp(x + pix, 0, texture.width - 1),
                        (int)Mathf.Clamp(y, 0, texture.width - 1));
                    curCol = Color.Lerp(
                    curCol,
                    new Color(
                        curCol.r * temp.r,
                        curCol.g * temp.g,
                        curCol.b * temp.b,
                        curCol.a),
                    lerp);

                    temp = bColorTex.GetPixel(
                        (int)Mathf.Clamp(x - pix, 0, texture.width - 1),
                        (int)Mathf.Clamp(y, 0, texture.width - 1));
                    curCol = Color.Lerp(
                    curCol,
                    new Color(
                        curCol.r * temp.r,
                        curCol.g * temp.g,
                        curCol.b * temp.b,
                        curCol.a),
                    lerp);

                    temp = bColorTex.GetPixel(
                        (int)Mathf.Clamp(x, 0, texture.width - 1),
                        (int)Mathf.Clamp(y + pix, 0, texture.width - 1));
                    curCol = Color.Lerp(
                    curCol,
                    new Color(
                        curCol.r * temp.r,
                        curCol.g * temp.g,
                        curCol.b * temp.b,
                        curCol.a),
                    lerp);

                    temp = bColorTex.GetPixel(
                        (int)Mathf.Clamp(x, 0, texture.width - 1),
                        (int)Mathf.Clamp(y - pix, 0, texture.width - 1));
                    curCol = Color.Lerp(
                    curCol,
                    new Color(
                        curCol.r * temp.r,
                        curCol.g * temp.g,
                        curCol.b * temp.b,
                        curCol.a),
                    lerp);
                    pix--;
                }
                texture.SetPixel((int)x, (int)y, curCol);

                y++;
            }
            x++;
        }
        texture.Apply();
        DestroyImmediate(bColorTex);
	}
}
