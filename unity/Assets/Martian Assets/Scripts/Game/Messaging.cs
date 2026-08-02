using System;
using System.Collections;
using UnityEngine;

[Serializable]
public enum chatOrigins
{
    Local,
    Remote,
    Server
}

[Serializable]
public class ChatEntry
{
	public string text = "";
	public chatOrigins origin;
}

[Serializable]
public class Messaging : MonoBehaviour
{
	public bool chatting;
	public int showChat = -1;
	[HideInInspector]
	public Vector2 scrollPosition;
	[HideInInspector]
	public ArrayList entries;
	private string inputField = "";
	// /*UNUSED*/ private bool display = true;
	private Rect windowRect;

	public Messaging()
	{
		entries = new ArrayList();
	}

	public void OnGUI()
	{
		if (Game.Settings.simplified)
		{
			return;
		}
		GUI.skin = Game.Skin;

		Color guicol = GUI.color;
		guicol.a = Game.GUIAlpha;
		GUI.color = guicol;

        if (Game.Controller.loadingWorld) return;

		if (showChat == 1 || showChat < 0)
		{
			windowRect = new Rect(
                10f,
                40f,
                Mathf.Min(Mathf.Max(170, Screen.width / 4), 250),
                (!Game.Settings.minimapAllowed || !Game.Settings.useMinimap) ?
                    ((float)(Screen.height - 55)) :
                    ((float)Screen.height * 0.75f - 60f));
			GUI.Window(11, windowRect, ChatWindow, "Messaging Console");
		}
		else if (GUI.Button(new Rect(
            10f,
            40f,
            Mathf.Min(Mathf.Max(170, Screen.width / 4), 250),
            25f), "Messaging Console"))
		{
			showChat = 1;
			GUI.FocusControl("Chat input field");
		}
	}

	public void ChatWindow(int id)
	{
		GUIStyle closeButtonStyle = GUI.skin.GetStyle("close_button");
		if (GUI.Button(
            new Rect(
                closeButtonStyle.padding.left,
                closeButtonStyle.padding.top,
                closeButtonStyle.normal.background.width,
                closeButtonStyle.normal.background.height),
            string.Empty,
            "close_button"))
		{
			showChat = 0;
		}

		scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        foreach (ChatEntry entry in entries)
        {
            GUILayout.BeginHorizontal();
            switch (entry.origin)
            { 
                case chatOrigins.Remote:
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(entry.text, "chatRemote");
                    break;
                case chatOrigins.Local:
                    GUILayout.Label(entry.text, "chatLocal");
                    GUILayout.FlexibleSpace();
                    break;
                default:
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(entry.text, "chatServer");
                    GUILayout.FlexibleSpace();
                    break;
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(3f);
        }
		GUILayout.EndScrollView();

		GUILayout.FlexibleSpace();

		if (Event.current.type == EventType.keyDown &&
            Event.current.character == '\n' &&
            inputField.Length > 0)
		{
			if (inputField == "x" ||
                inputField == "/x" ||
                inputField == "/X")
			{
				if (Game.Settings.zorbSpeed != 0f)
				{
					Game.Player.GetComponent<NetworkView>().RPC(
                        "sZ",
                        RPCMode.All,
                        !Game.PlayerVeh.zorbBall);
					Game.Controller.msg(
                        "XORB " + (Game.PlayerVeh.zorbBall ?
                            "Activated" :
                            "Deactivated"),
                        (int)chatOrigins.Server);
				}
				else
				{
					Game.Controller.msg("XORBs Unavailable", (int)chatOrigins.Server);
				}
			}
			else if (
                inputField == "r" ||
                inputField == "/r" ||
                inputField == "/R")
			{
				Game.Settings.resetTime = Time.time;
				Game.Player.GetComponent<Rigidbody>().isKinematic = true;
				broadcast(Game.Player.name + " Resetting in 10 seconds...");
			}
			else
			{
				inputField = Game.LanguageFilter(inputField);
				Game.Controller.msg(inputField, (int)chatOrigins.Local);
				Game.Controller.GetComponent<NetworkView>().RPC(
                    "msg",
                    RPCMode.Others, inputField + " - " + GameData.userName,
                    (int)chatOrigins.Remote);

			}
			inputField = "";
			chatting = false;
			GUI.UnfocusWindow();
		}

		GUI.SetNextControlName("Chat input field");
		inputField = GUILayout.TextField(inputField, 300);

		if (chatting && inputField == "")
		{
			GUILayout.Label("(Press \"Tab\" to Cancel)");
		}
		else if (chatting)
		{
			GUILayout.Label("(Press \"Enter\" to Send)");
		}
		else
		{
			GUILayout.Label("(Press \"Tab\" to Message)");
		}

		if (chatting)
		{
			GUI.FocusControl("Chat input field");
			GUI.FocusWindow(id);
		}

		if (showChat < 0 && showChat > -5)
		{
			showChat--;
		}
        else if (showChat < 0 || (
            (Input.GetKeyDown(KeyCode.Tab) ||
                Input.GetKeyDown(KeyCode.Mouse0)) &&
            Time.time > Game.Controller.kpTime))
        {
            Game.Controller.kpTime = Time.time + Game.Controller.kpDur;
            if (
                (chatting == false && !Input.GetKeyDown(KeyCode.Mouse0)) ||
                (windowRect.Contains(Input.mousePosition) &&
                    Input.GetKeyDown(KeyCode.Mouse0)))
            {
                if (showChat == 1) chatting = true;
                showChat = 1;
            }
            else
            {
                chatting = false;
                if (Input.GetKeyDown(KeyCode.Tab)) GUI.UnfocusWindow();
            }
        }
	}

	public void broadcast(string str)
	{
		Game.Controller.GetComponent<NetworkView>().RPC(
            "msg",
            RPCMode.All,
            str,
            (int)chatOrigins.Server);
	}
}
