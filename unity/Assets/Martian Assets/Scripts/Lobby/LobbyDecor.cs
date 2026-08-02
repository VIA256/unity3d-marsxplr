using System;
using UnityEngine;

[Serializable]
public class LobbyDecor : MonoBehaviour
{
	public Lobby Lobby;
	public GUITexture bg;

	public int logoOffset;

	public void Awake()
	{
		if (Time.time > 6f)
		{
			Color c = GetComponent<GUITexture>().color;
			c.a = 0;
			GetComponent<GUITexture>().color = c;
		}
		else if ((bool)bg)
		{
			bg.pixelInset = new Rect(
                0f,
                0f,
                Screen.width,
                Screen.height);
		}
	}

	public void Update()
	{
		float width = (float)Screen.height * 0.84f;
        if (width > 600f)
        {
            width -= (width - 600f) * 0.5f;
        }
		if (width > (float)(Screen.width - 30))
		{
			width = Screen.width - 30;
		}
		if (
            Time.time > 1.5f &&
            !Camera.main.GetComponent<AudioSource>().isPlaying &&
            PlayerPrefs.GetInt("useMusic", 1) != 0)
		{
			Camera.main.GetComponent<AudioSource>().Play();
		}

		if (Time.time < 5f)
		{
			Color bgc = bg.color;
            bgc.a = 1f - Time.time / 4f;
			bg.color = bgc;
			if (Time.time < 2.5f)
			{
				Color gtc = GetComponent<GUITexture>().color;
                gtc.a = Mathf.Lerp(
                    0f,
                    0.6f,
                    Time.time * (1f / 2.5f));
				GetComponent<GUITexture>().color = gtc;
				GetComponent<GUITexture>().pixelInset = new Rect(
                    (float)(Screen.width / 2) - width / 2f,
                    (float)(Screen.height / 2) - width / 4f,
                    width,
                    width / 2f);
			}
			if (Time.time > 4.25f)
			{
				GetComponent<GUITexture>().pixelInset = new Rect(
                    (float)(Screen.width / 2) - width / 2f,
                    easeOutExpo(
                        Time.time - 4.25f,
                        (float)(Screen.height / 2) - width / 4f,
                        (float)Screen.height - width / 2f, 0.75f),
                    width,
                    width / 2f);
			}
		}
		else if (Time.time > 6f)
		{
			if ((bool)bg)
			{
				Destroy(bg);
			}
			Color gtc = GetComponent<GUITexture>().color;
            gtc.a = Mathf.Lerp(
                GetComponent<GUITexture>().color.a,
                Lobby.GUIAlpha - 0.4f,
                Time.deltaTime * 4f);
			GetComponent<GUITexture>().color = gtc;
			GetComponent<GUITexture>().pixelInset = new Rect(
                (float)(Screen.width / 2) - width / 2f,
                (float)Screen.height - width / 2f,
                width,
                width / 2f);
		}

		logoOffset = (int)(width / 2f);
	}

	public float easeOutExpo(float t, float b, float c, float d)
	{
        c = c - b;
        return c * (-Mathf.Pow(2.0f, -10.0f * t/d) + 1.0f) + b;
	}

	public float easeInOutExpo(float t, float b, float c, float d)
	{
        c = c - b;
        if (t < d / 2.0f) return 2.0f*c*t*t/(d*d) + b;
        float ts = t - d/2.0f;
        return -2.0f*c*ts*ts/(d*d) + 2.0f*c*ts/d + c/2.0f + b;
	}
}
