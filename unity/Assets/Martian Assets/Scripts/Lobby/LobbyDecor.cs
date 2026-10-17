using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class LobbyDecor : MonoBehaviour
{
	public Lobby Lobby;
	public RawImage bg;

	public int logoOffset;

	public void Awake()
	{
		if (Time.time > 6f)
		{
			Color c = GetComponent<RawImage>().color;
			c.a = 0;
			GetComponent<RawImage>().color = c;
		}
		else if ((bool)bg)
		{
			bg.rectTransform.anchoredPosition = new Vector2(0f, 0f);
			bg.rectTransform.sizeDelta = new Vector2((float)Screen.width, (float)Screen.height);
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
				Color gtc = GetComponent<RawImage>().color;
                gtc.a = Mathf.Lerp(
                    0f,
                    1f,
                    Time.time * (1f / 2.5f));
				GetComponent<RawImage>().color = gtc;
				GetComponent<RawImage>().rectTransform.anchoredPosition = new Vector2(
					(float)(Screen.width / 2) - width / 2f,
                    (float)(Screen.height / 2) - width / 4f);
				GetComponent<RawImage>().rectTransform.sizeDelta = new Vector2(
					width,
					width / 2f);
			}
			if (Time.time > 4.25f)
			{
				GetComponent<RawImage>().rectTransform.anchoredPosition = new Vector2(
					(float)(Screen.width / 2) - width / 2f,
                    easeOutExpo(
                        Time.time - 4.25f,
                        (float)(Screen.height / 2) - width / 4f,
                        (float)Screen.height - width / 2f, 0.75f));
				GetComponent<RawImage>().rectTransform.sizeDelta = new Vector2(
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
			Color gtc = GetComponent<RawImage>().color;
            gtc.a = Mathf.Lerp(
                GetComponent<RawImage>().color.a,
                Lobby.GUIAlpha,
                Time.deltaTime * 4f);
			GetComponent<RawImage>().color = gtc;	
			GetComponent<RawImage>().rectTransform.anchoredPosition = new Vector2(
				(float)(Screen.width / 2) - width / 2f,
                (float)Screen.height - width / 2f);
			GetComponent<RawImage>().rectTransform.sizeDelta = new Vector2(
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
