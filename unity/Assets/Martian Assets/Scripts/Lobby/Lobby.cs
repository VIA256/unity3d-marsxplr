using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public class Lobby : MonoBehaviour
{
	[NonSerialized]
	public int port = 2500;

	private int GuiAnimate = -1;
	[NonSerialized]
	public float GUIAlpha = 0.0f;
	[NonSerialized]
	public float GUIHide = 0.0f;
	private string gameName = "marsxplr";
	// /*UNUSED*/ private float timeoutHostList = 0.00f;
    private float lastHostListRequest = -1000.0f;
	private float lastHostListRefresh = -1000.0f;
	private float hostListRefreshTimeout = 5.0f;

	private ConnectionTesterStatus natCapable = ConnectionTesterStatus.Undetermined;
	// /*UNUSED*/ private bool NATHosts = false;
	private bool probingPublicIP = false;
	//private bool doneTesting = false;
	private float timer = 0.0f;
	private bool filterNATHosts = false;
	private bool forceNAT = false;
	// /*UNUSED*/ private bool hideTest = false;
	private string masterServerMessage = "";
	private int masterServerConFailures = 0;
	private string testMessage = "";
	private string[] netConIP;
	private int netConPort;
	private int netConAttempts = 0;
	private bool useMasterServer = true;
	private bool disableMasterServer = false;
	private bool doNetworking = true;
	[NonSerialized]
	public string userName = "";
	[NonSerialized]
	public string userCode = "";
	private bool userIsRegistered = false;
	private string remoteIP;
	private string userNameTemp = "";
	private string userPassword = "";
	private bool userRemembered = false;
	private string userAuthenticating = "";
	[NonSerialized]
	public Rect windowRect;
	[NonSerialized]
	public string temp = "";
	private Vector2 scrollPosition;
	private string outdated = "";
	// /*UNUSED*/ private bool hostRegistered = false;
	// /*UNUSED*/ private string serverLevel = "";
	private bool showSettings = false;
	private bool autoHostListRefresh = true;
	public bool useAlternateServer = false;
	[NonSerialized]
	public string[] messages;
	private string listServerIP = "";
	private int listServerPort;
	// /*UNUSED*/ private string backupServerIP;
	// /*UNUSED*/ private int backupServerPort = 23456;
	private float buttonHeight;
	private float buttonHeightTarget;
	private bool mouseInServerList;
	private string serverDetails = "";
	private float serverDetailsBoxAlpha;
	public GUISkin Skin;
	public GUIStyle serverDetailsBox;
	public LobbyDecor lobbyDecor;
	public int contentWidth = 0;

	private bool hostDedicated = false;
	private string[] dedicatedIP;
	private int dedicatedPort;
	private bool dedicatedNAT;
	// /*UNUSED*/ private int dedicatedHostAttempts;

	public void Awake()
	{
		QualitySettings.SetQualityLevel(5);
		Screen.lockCursor = false;
		Application.runInBackground = false;
	}

    public IEnumerator Start()
    {
        userPassword = PlayerPrefs.GetString("userPassword", "");
        userCode = PlayerPrefs.GetString("userCode", "");
        userRemembered = (PlayerPrefs.GetInt("userRemembered", 0) == 1 ? true : false);
        userIsRegistered = (PlayerPrefs.GetInt("userRegistered", 0) == 1 ? true : false);
        GameData.masterBlacklist = "";

        if (GameData.userName != "") //They just left a server
        {
            userName = GameData.userName;
            if (userRemembered) userNameTemp = userName;
        }
        else if (userPassword != "")    //They are trying to play authenticated - don't let them in without validating their password
        {
            userNameTemp = PlayerPrefs.GetString("userName", "");
            StartCoroutine_Auto(authenticateUser()); //Attempt Auto Login
        }
        else
        {                               //They are a guest, just let them use whatever username they have.
            userNameTemp = PlayerPrefs.GetString("userName", "");
            userName = userNameTemp + (userNameTemp != "" ? "–" : "");
        }

        List<String> msgs = new List<String>();
        List<GameWorldDesc> wlds = new List<GameWorldDesc>();
        WWW www = new WWW("http://gitea.moe/VIA256/new-marsxplr-web-elements/raw/branch/main/upd3");
        yield return www;
        if (www.error == null)
        {
            String[] data = www.text.Split("\n"[0]);
            foreach (String dat in data)
            {
                if (dat == "") continue;

                int pos = dat.IndexOf("="[0]);
                if (pos == 0 || pos == -1) continue;
                List<String> tmp = new List<String>(new String[]{
                    dat.Substring(0, pos),
                    dat.Substring(pos + 1)});
                String[] val = tmp.ToArray();

                if (
                    val[0] == "sv" &&
                    SemVer.Parse(val[1]) > GameData.gameVersion)
                {
                    outdated = val[1];
                }
                else if (val[0] == "d")
                {
                    hostDedicated = (val[1] == "1" || val[1] == "true");
                }
                else if (val[0] == "m") msgs.Add(val[1]);
                else if (val[0] == "w")
                {
                    String nme = "";
                    String url = "";
                    bool featured = false;
                    String[] wrld = val[1].Split(";"[0]);
                    foreach (String str in wrld)
                    {
                        if (str == "") continue;
                        if (str == "featured")
                        {
                            featured = true;
                            continue;
                        }
                        String[] vals = str.Split(":"[0]);
                        if (vals[0] == "nme") nme = vals[1];
                        else if (vals[0] == "url") url = str.Substring(4);
                    }
                    wlds.Add(new GameWorldDesc(nme, url, featured));
                }
                else if (
                    (val[0] == "s" && !useAlternateServer) ||
                    (val[0] == "s2" && useAlternateServer))
                {
                    String[] ipStr = val[1].Split(":"[0]);
                    listServerIP = ipStr[0];
                    listServerPort = int.Parse(ipStr[1]);
                    MasterServer.ipAddress = listServerIP;
                    MasterServer.port = listServerPort;
                }
                else if (
                    (val[0] == "f" && !useAlternateServer) ||
                    (val[0] == "f2" && useAlternateServer))
                {
                    String[] ipStr = val[1].Split(":"[0]);
                    Network.natFacilitatorIP = ipStr[0];
                    Network.natFacilitatorPort = int.Parse(ipStr[1]);
                }
                else if (
                    (val[0] == "t" && !useAlternateServer) ||
                    (val[0] == "t2" && useAlternateServer))
                {
                    String[] ipStr = val[1].Split(":"[0]);
                    Network.connectionTesterIP = ipStr[0];
                    Network.connectionTesterPort = int.Parse(ipStr[1]);
                }
                else if (val[0] == "b")
                {
                    GameData.masterBlacklist +=
                        (GameData.masterBlacklist != "" ? "\n" : "") +
                        val[1];
                }
                else if (val[0] == "n")
                {
                    GameData.networkMode = int.Parse(val[1]);
                }
            }
        }
        else
        {
            GameData.errorMessage = "Alert: Update server is unreachable.\nIf this computer is online, the update server may be down.\n\nYou need to be connected to the internet to play Mars Explorer.\n\nPlease check MarsXPLR.com for news & updates!";
        }

        GameData.gameWorlds = wlds.ToArray();
        messages = msgs.ToArray();

        MasterServer.RequestHostList(gameName);
    }
    
	public void OnFailedToConnectToMasterServer(NetworkConnectionError info)
	{
        if (
            info == NetworkConnectionError.CreateSocketOrThreadFailure &&
            masterServerMessage != "")
        {
            return;
        }
        MasterServer.ClearHostList();
        masterServerMessage = " Master server connection failure: " + info;
        masterServerConFailures += 1;
	}

	public void OnFailedToConnect(NetworkConnectionError info)
	{
        if (Network.isClient) return; //We already connected to a different server

        GameData.errorMessage = (
            "Game could not be connected to.\nPlease try joining a different game!\n\n\n" +
            (info == NetworkConnectionError.ConnectionFailed ?
                "You may be blocked by a network firewall.\n(See \"How Do I configure My Router\" on the FAQ @ MarsXPLR.com)\n" :
                (info == NetworkConnectionError.NATTargetNotConnected ?
                    "\nThe person hosting the game you were connecting to\nmay have stopped playing Mars Explorer." :
                    info.ToString()
                )
            )
        );

        if (netConAttempts < 2)
        {
            Network.Connect(netConIP, netConPort);
            netConAttempts++;
            GameData.errorMessage +=
                "\n\n...Reattempting Connection - Attempt # " +
                netConAttempts +
                "...";
        }
        else
        {
            GameData.errorMessage += "\n\nReconnection Attempt Failed.";
        }
	}

	public void OnConnectedToServer()
	{
		GameData.errorMessage = "";
        Network.isMessageQueueRunning = false; //We don't want to recieve LoadLevel commands before we are ready... This will be enabled as soon as we get to the "Game" level
		GameData.userName = userName;
		GameData.userCode = userCode;
		StartCoroutine_Auto(LoadGame());
	}

	public void OnGUI()
	{
		GUI.skin = Skin;

        Color guicolor = GUI.color;
        guicolor.a = GUIAlpha;
        GUI.color = guicolor;
        
		if (QualitySettings.GetQualityLevel() < 3)
		{
			QualitySettings.SetQualityLevel(3);
		}

		if (Time.time <= 4.25f)
		{
            return;
		}
        //Fade GUI Out
		if (GuiAnimate == 1)
		{
			if (GUIAlpha <= 0.0f)
			{
				GUIAlpha = 0.0f;
				GuiAnimate = 0;
			}
			else
			{
				GUIAlpha -= Time.deltaTime * 0.35f;
			}
			if (GUIHide > 1.0f)
			{
				GUIHide = 1.0f;
			}
			else
			{
				GUIHide += Time.deltaTime * 0.5f;
			}
		}
        //Fade GUI In
		else if (GuiAnimate == -1)
		{
			GUIAlpha = 0.0f;
			GuiAnimate = -2;
		}
		else if (GuiAnimate == -2)
		{
			if (GUIAlpha >= 1.0f)
			{
				GUIAlpha = 1.0f;
				GuiAnimate = 0;
			}
			else
			{
				GUIAlpha += Time.deltaTime * 0.2f;
			}
		}

		float width = (float)Screen.height * 1.2f;
		if (width > 800.0f)
		{
			width -= (width - 800.0f) * 0.5f;
		}
		if (width > 1200f)
		{
			width = 1200f;
		}
		if (width > (float)(Screen.width - 30))
		{
			width = Screen.width - 30;
		}
		if (width < 600f)
		{
			width = 600f;
		}
		contentWidth = (int)width;

		if (GameData.errorMessage != "")
		{
			GUILayout.Window(
                1,
                new Rect(
                    (float)Screen.width * 0.5f - width / 2f + 50f,
                    lobbyDecor.logoOffset - 20,
                    width - 100f,
                    300f),
                errorWindow,
                "",
                "windowAlert"); //Notice
			GUI.BringWindowToFront(1);
		}
		if (outdated == "")
		{
			GUILayout.Window(
                0,
                new Rect(
                    (float)Screen.width * 0.5f - width / 2f - 50f,
                    lobbyDecor.logoOffset - 70,
                    width + 100f,
                    (float)(doNetworking && userName != "" ?
                        Screen.height - lobbyDecor.logoOffset + 10 :
                        320) + 100
                    ),
                MakeWindow,
                "",
                "windowChromeless");
		}
		else
		{
			GUILayout.Window(
                0,
                new Rect(
                    (float)Screen.width * 0.5f - width / 2f,
                    lobbyDecor.logoOffset - 20,
                    width,
                    150f
                ),
                makeWindowUpdate,
                "",
                "windowChromeless");
		}
		GUI.FocusWindow(0);

        if (serverDetails != "" &&
            GuiAnimate != 1 &&
            GUIAlpha != 0f &&
            GameData.errorMessage == "")
        {
            if (serverDetailsBoxAlpha < 0.99f)
            {
                serverDetailsBoxAlpha += Time.deltaTime * 0.5f;
            }
            else
            {
                serverDetailsBoxAlpha = 1f;
            }
        }
        else
        {
            if (serverDetailsBoxAlpha > 0.01f)
            {
                serverDetailsBoxAlpha -= Time.deltaTime * 0.5f;
            }
            else serverDetailsBoxAlpha = 0.0f;
        }
		if (serverDetailsBoxAlpha > 0.01f)
		{
            Color colorTemp = GUI.color;
            colorTemp.a = serverDetailsBoxAlpha;
            GUI.color = colorTemp;
            
			GUIStyle serverDetailsBoxStyle = GUI.skin.GetStyle("serverDetailsBox");
			GUILayout.Window(
                2,
                new Rect(
                    Input.mousePosition.x - serverDetailsBoxStyle.fixedWidth - 1f,
                    (float)Screen.height - Input.mousePosition.y - serverDetailsBoxStyle.fixedHeight - 4f,
                    serverDetailsBoxStyle.fixedWidth,
                    serverDetailsBoxStyle.fixedHeight),
                serverDetailsWindow,
                "",
                "serverDetailsBox");
			GUI.BringWindowToFront(2);
		}
	}

	public void serverDetailsWindow(int id)
	{
		GUI.contentColor = GUI.skin.GetStyle("serverDetailsBox").normal.textColor;
		GUILayout.Label(serverDetails);
	}

    public IEnumerator TestConnection(bool force)
    {
        disableMasterServer = false;
        probingPublicIP = false;
        if (timer == 0.0f) timer = Time.time + 15;
        testMessage = "testing";
        // /*UNUSED*/ doneTesting = false;
        doNetworking = false;

        natCapable = Network.TestConnection(force);
        while (natCapable == ConnectionTesterStatus.Undetermined)
        {
            natCapable = Network.TestConnection();
            yield return new WaitForSeconds(0.5f);
        }

        switch (natCapable)
        { 
            case ConnectionTesterStatus.Error:
                GameData.errorMessage = "Your computer does not appear to be online...\nNetworking was defaulted to Local Area Network mode.\n\n(You can play with friends over a LAN, but not with people all around the world over the internet)";
                testMessage = "Computer is Offline.";
                // /*UNUSED*/ doneTesting = true;
                doNetworking = true;
                useMasterServer = false;
                break;
            case ConnectionTesterStatus.LimitedNATPunchthroughSymmetric:
                GameData.errorMessage = "You appear to have a private IP and no NAT punchthrough capability...\n\nHit \"Dismiss\" click \"Show Settings\", then click \"Retest Internet Connection\".\nIf you get this message again, read \"How Do I configure My Router\" on the FAQ @ MarsXPLR.com";
                testMessage = "Private IP with no NAT punchthrough - Local network, non internet games only.";
                //filterNATHosts = true;
                Game.useNAT = true;
                // /*UNUSED*/ doneTesting = true;
                doNetworking = true;
                useMasterServer = false;
                break;
            case ConnectionTesterStatus.LimitedNATPunchthroughPortRestricted:
                if (probingPublicIP)
                {
                    testMessage = "Non-connectable public IP address (port " + port + " blocked), NAT punchthrough can circumvent the firewall.";
                }
                else
                {
                    testMessage = "NAT punchthrough enabled.";
                }
                // NAT functionality is enabled in case a server is started,
                // clients should enable this based on if the host requires it
                Game.useNAT = true;
                // /*UNUSED*/ doneTesting = true;
                doNetworking = true;
                useMasterServer = true;
                break;
            case ConnectionTesterStatus.PublicIPIsConnectable:
                testMessage = "Directly connectable public IP address.";
                Game.useNAT = false;
                // /*UNUSED*/ doneTesting = true;
                doNetworking = true;
                useMasterServer = true;
                break;
            // This case is a bit special as we now need to check if we can 
            // cicrumvent the blocking by using NAT punchthrough
            case ConnectionTesterStatus.PublicIPPortBlocked:
                testMessage = "Non-connectible public IP address with NAT punchthrough disabled, running a server is impossible.\n\n(Please setup port forwarding for port # " + port + " in your router)";
                Game.useNAT = false;
                // If no NAT punchthrough test has been performed on this public IP, force a test
                if (!probingPublicIP)
                {
                    Debug.Log("Testing if firewall can be circumvented");
                    natCapable = Network.TestConnectionNAT();
                    probingPublicIP = true;
                    timer = Time.time + 10;
                }
                // NAT punchthrough test was performed but we still get blocked
                else if (Time.time > timer)
                {
                    probingPublicIP = false;    // reset
                    Game.useNAT = true;
                    // /*UNUSED*/ doneTesting = true;
                    doNetworking = true;
                    useMasterServer = false;
                }
                break;
            case ConnectionTesterStatus.PublicIPNoServerStarted:
                testMessage = "Public IP address but server not initialized, it must be started to check server accessibility. Restart connection test when ready.";
                doNetworking = true;
                // /*UNUSED*/ doneTesting = true;
                useMasterServer = true;
                break;
            default:
                testMessage = "Error in test routine, got " + natCapable;
                doNetworking = false;
                break;
        }
    }

	public void MakeWindow(int id)
	{
        //Test User's Network
		if (!doNetworking)
		{
			GUILayout.FlexibleSpace();
			if (testMessage == "testing")
			{
				if (Time.time > timer)
				{
					GUILayout.Label("Network status could not be determined.");
				}
				else
				{
					GUILayout.Label(
                        "Determining your network's configuration... " +
                        (timer - Time.time));
				}
				GUILayout.Space(10f);
			}
			else
			{
				GUILayout.Label(
                    "Unfortunatley, your computer's network settings will not allow Mars Explorer to network. Specific error was: " +
                    testMessage);
			}
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.BeginVertical(GUILayout.Width(300f));
			if (GUILayout.Button("Restart Connection Test"))
			{
				timer = 0f;
				StartCoroutine_Auto(TestConnection(true));
			}
			if (GUILayout.Button("Force Enable Networking (May Not Work)"))
			{
				Game.useNAT = true;
				// /*UNUSED*/ doneTesting = true;
				doNetworking = true;
				useMasterServer = true;
				testMessage = "Network testing failed, networking force enabled";
			}
			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.FlexibleSpace();
		}
        //Get user's name
		else if (userName == "")
		{
			GUILayout.Space(50f);
			GUILayout.Label("Welcome, Space Cadet!\nPlease enter a pseudonym for others to know you by:");

			GUILayout.Space(10f);

			GUILayout.BeginHorizontal();
			GUILayout.Space((float)contentWidth / 3.5f);
			GUILayout.BeginScrollView(Vector2.zero);

			GUILayout.BeginHorizontal();
			GUILayout.Label("Your Name:", GUILayout.Width(100f));
			GUI.SetNextControlName("Name");
			userNameTemp = Regex.Replace(
                GUILayout.TextField(userNameTemp, 25),
                "[^A-Za-z0-9-_.&<>]",
                "");
			GUILayout.EndHorizontal();

			if (userIsRegistered)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Your Password:", GUILayout.Width(100f));
				userPassword = GUILayout.PasswordField(userPassword, "*"[0]);
				GUILayout.EndHorizontal();
			}

			GUILayout.BeginHorizontal();
			GUILayout.Label("", GUILayout.Width(100f));
			userIsRegistered = GUILayout.Toggle(userIsRegistered, "I am registered");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("", GUILayout.Width(100f));
			userRemembered = GUILayout.Toggle(userRemembered, "Remember Me");
			GUILayout.EndHorizontal();

			GUILayout.EndScrollView();
			GUILayout.Space((float)contentWidth / 3.5f);
			GUILayout.EndHorizontal();

			GUILayout.Space(10f);

			GUILayout.BeginHorizontal();
			GUILayout.Space(contentWidth / 4);
			if (userNameTemp != "")
			{
                //Registered User
				if (userIsRegistered)
				{
					if (userPassword != "")
					{
						buttonHeightTarget = 40f;
                        if (
                            GUILayout.Button(
                                (userAuthenticating != "" ?
                                    (userAuthenticating == "true" ?
                                        "...Authenticating..." :
                                        "Authentication Failed: " +
                                            userAuthenticating +
                                            "\n>> Retry <<"
                                    ) :
                                    ">> Authenticate and Explore Mars! <<"
                                ),
                                GUILayout.Height(buttonHeight)
                            ) ||
                            Input.GetKeyDown("return") ||
                            Input.GetKeyDown("enter"))
						{
							StartCoroutine_Auto(authenticateUser());
						}
					}
					else
					{
						buttonHeightTarget = 0f;
						if (buttonHeight > 4f)
						{
                            GUILayout.Button(
                                (userAuthenticating != "" ?
                                    (userAuthenticating == "true" ?
                                        "...Authenticating..." :
                                        ("Authentication Failed: " +
                                            userAuthenticating +
                                            "\n>> Retry <<"
                                        )
                                    ) :
                                    ">> Authenticate and Explore Mars! <<"
                                ),
                                GUILayout.Height(buttonHeight)
                            );
						}
					}
				}
                //Guest User
				else
				{
					buttonHeightTarget = 40f;
					if (
                        GUILayout.Button(
                            ">> I Am Ready to Explore Mars! <<",
                            GUILayout.Height(buttonHeight)) ||
                        Input.GetKeyDown("return") ||
                        Input.GetKeyDown("enter"))
					{
						userName = Game.LanguageFilter(userNameTemp);
						if (
                            userName.IndexOf("ADMIN") != -1 ||
                            userName.IndexOf("admin") != -1 ||
                            userName.IndexOf("Admin") != -1 ||
                            userName.IndexOf("aubrey") != -1 ||
                            userName.IndexOf("Aubrey") != -1 ||
                            userName.IndexOf("AUBREY") != -1 ||
                            userName.IndexOf("abrey") != -1 ||
                            userName.IndexOf("Abrey") != -1 ||
                            userName.IndexOf("aubry") != -1 ||
                            userName.IndexOf("Aubry") != -1 ||
                            userName.IndexOf("aubery") != -1 ||
                            userName.IndexOf("Aubery") != -1)
						{
							userName += " 2";
						}
						if (userCode == "{Code}")
						{
							userCode = "";
						}
						if (userRemembered)
						{
							PlayerPrefs.SetString("userName", userName);
							PlayerPrefs.SetString("userPassword", "");
							PlayerPrefs.SetInt("userRemembered", 1);
						}
						else
						{
							PlayerPrefs.SetString("userName", "");
							PlayerPrefs.SetString("userPassword", "");
							PlayerPrefs.SetInt("userRemembered", 0);
							userNameTemp = "";
							userPassword = "";
						}
						PlayerPrefs.SetInt("userRegistered", 0);

						//userName += "–";

						lastHostListRefresh = -1f;
						lastHostListRequest = Time.time;
					}
				}
			}
			else
			{
				buttonHeightTarget = 0f;
				if (buttonHeight > 4f)
				{
                    GUILayout.Button(
                        (userIsRegistered ?
                            (userAuthenticating != "" ?
                                (userAuthenticating == "true" ?
                                    "...Authenticating..." :
                                    ("Authentication Failed: " +
                                        userAuthenticating +
                                        "\n>> Retry <<"
                                    )
                                ) :
                                ">> Authenticate and Explore Mars! <<"
                            ) :
                            ">> I Am Ready to Explore Mars! <<"
                        ),
                        GUILayout.Height(buttonHeight)
                    );
				}
			}
			GUILayout.Space(contentWidth / 4);
			GUILayout.EndHorizontal();

			GUILayout.Space(10f);

			if (userIsRegistered)
			{
				if (userPassword == "")
				{
					GUILayout.Label(
                        "(Make sure the credentials you enter match your MarsXPLR.com login)");
				}
			}
			else
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(contentWidth / 3);
				if (GUILayout.Button("Want to own your name?\n>> Register an account! <<"))
				{
                    OpenURL("http://web.archive.org/web/20190525045905/http://marsxplr.com/user/login");
				}
				GUILayout.Space(contentWidth / 3);
				GUILayout.EndHorizontal();
			}

			GUILayout.FlexibleSpace();

			if (GUIUtility.keyboardControl == 0)
			{
				GUI.FocusControl("Name");
			}

            //Hide Button
			if (buttonHeightTarget == 0f)
			{
				buttonHeight -= buttonHeight * Time.deltaTime * 6f;
			}
            //Show Button
			else
			{
				buttonHeight += (buttonHeightTarget - buttonHeight) * Time.deltaTime * 6f;
			}
		}

        //Show Server List
		else
		{
			GUILayout.Space(5f);
			GUILayout.BeginHorizontal();

			if (GUILayout.Button(
                "<< Change Name: " + userName,
                GUILayout.Width(238f),
                GUILayout.Height(30f)))
			{
				userName = "";
			}

    		GUILayout.FlexibleSpace();
		    
            // Start a new server

			if (Network.peerType == NetworkPeerType.Disconnected)
			{
				if (GUILayout.Button(
                    "Host Game >>",
                    GUILayout.Width(238f),
                    GUILayout.Height(30f)))
				{
					Network.Disconnect();
					if (hostDedicated && dedicatedIP.Length > 0)
					{
						Game.useNAT = (dedicatedNAT || forceNAT);
						temp = Network.Connect(dedicatedIP, dedicatedPort) + "";
						netConIP = dedicatedIP;
						netConPort = dedicatedPort;
						netConAttempts = 1;
						// /*UNUSED*/ dedicatedHostAttempts = 1;
						if (temp != "NoError")
						{
							GameData.errorMessage =
                                "Dedicated server initialization failed: " +
                                temp +
                                "\n\nTo resolve this issue, please uncheck the\n\"Utilize dedicated servers for hosting games\"\n option in the Settings below.";
						}
						else
						{
							GameData.errorMessage =
                                "...Initializing connection to dedicated server...\n(" +
                                netConIP[0] +
                                ":" +
                                netConPort +
                                (Game.useNAT ? " NAT" : "") +
                                ")\n\n\n\nIf this fails, please uncheck the\n\"Utilize dedicated servers for hosting games\"\n option in the Settings below.";
						}
					}
					else
					{
						Game.useNAT = !Network.HavePublicAddress();
						port = 2500;
						NetworkConnectionError info;
						while (true)
						{
							info = Network.InitializeServer(9, port, Game.useNAT);
                            if (port < 2600 && info != NetworkConnectionError.NoError)
                            {
                                port++;
                            }
                            else
                            {
                                break;
                            }
						}
						if (info != NetworkConnectionError.NoError)
						{
							GameData.errorMessage = "\nCould not start server: " + info;
						}
						else
						{
							GameData.userName = userName;
							StartCoroutine_Auto(LoadGame());
						}
					}
				}
			}
			else
			{
				GUILayout.Button(
                    "Hosting Game...",
                    GUILayout.Width(238f),
                    GUILayout.Height(30f));
			}

			GUILayout.EndHorizontal();
			GUILayout.Space(5f);

			scrollPosition = GUILayout.BeginScrollView(scrollPosition);

			if (Event.current.type != EventType.Layout)
			{
				serverDetails = "";
			}

            int activePlayers = 0;
            int activePlayersVisible = 0;
            int availableServers = 0;
			if (useMasterServer && !disableMasterServer)
			{
				dedicatedIP = new String[0];

                HostData[] data = MasterServer.PollHostList();

                String[] serverData;
                SemVer gameVersion;
                float serverVersion;

                //Precull Data
                String[] vals;
				if (data.Length > 0)
				{
					List<HostData> dataCulled = new List<HostData>();
                    foreach (HostData element in data)
                    {
                        activePlayers += element.connectedPlayers;

                        if (filterNATHosts && element.useNat) continue;

                        serverData = element.comment.Split(";"[0]);
                        gameVersion = new SemVer(0);
                        serverVersion = 0.0f;
                        foreach (String dat in serverData)
                        {
                            if (dat == "") continue;
                            vals = dat.Split("="[0]);
                            if (vals[0] == "v") gameVersion = SemVer.Parse(vals[1]);
                            if (vals[0] == "d") serverVersion = float.Parse(vals[1]);
                        }

                        if (
                            serverVersion == GameData.serverVersion &&
                            element.connectedPlayers == 0)
                        {
                            availableServers++;
                            dedicatedIP = element.ip;
                            dedicatedPort = element.port;
                            dedicatedNAT = element.useNat;
                            continue;
                        }
                        else if (GameData.gameVersion != gameVersion) continue;

                        //Display this Game
                        dataCulled.Add(element);
                    }
                    data = dataCulled.ToArray();
				}

                //Display Data
                if (data.Length > 0)
                {
                    System.Array.Sort(data, sortHostArray);

                    int i = 0;
                    foreach (HostData element in data)
                    { 
                        masterServerConFailures = 0;
                        masterServerMessage = "";
                        serverData = element.comment.Split(";"[0]);
                        gameVersion = new SemVer(0);
                        serverVersion = 0.0f;
                        String serverWorld = "";
                        String serverPlayers = "";
                        // /*UNUSED*/ String serverWorldURL = "";
                        String bannedIPs = "";
                        String serverLag = "";
                        bool isLocked = false;
                        foreach (String dat in serverData)
                        {
                            if (dat == "") continue;
                            vals = dat.Split("="[0]);
                            if (vals[0] == "v") gameVersion = SemVer.Parse(vals[1]);
                            if (vals[0] == "d") serverVersion = float.Parse(vals[1]);
                            else if (vals[0] == "w") serverWorld = vals[1];
                            else if (vals[0] == "p") serverPlayers = vals[1];
                            //else if (vals[0] == "u") serverWorldURL = vals[1];
                            else if (vals[0] == "b") bannedIPs = vals[1];
                            else if (vals[0] == "s") serverLag = vals[1];
                            else if (vals[0] == "l") isLocked = true;
                        }

                        activePlayersVisible += element.connectedPlayers;

                        GUILayout.BeginHorizontal();
                        GUILayout.Label(
                            element.connectedPlayers.ToString(),
                            GUILayout.Width(40));
                        GUILayout.Label(element.gameName);
                        GUILayout.Label(serverWorld);
                        if (serverVersion != 0.0f)
                        {
                            GUILayout.Label("»", GUILayout.Width(15));
                        }
                        else
                        {
                            GUILayout.Label(" ", GUILayout.Width(15));
                        }

                        if (element.connectedPlayers == element.playerLimit)
                        {
                            GUILayout.Label("Game Full", GUILayout.Width(150));
                        }
                        else
                        {
                            bool weAreBanned = false;
                            if (bannedIPs != "")
                            {
                                String[] banned = bannedIPs.Split(","[0]);
                                foreach (String dat in banned)
                                {
                                    if (dat == Network.player.ipAddress)
                                    {
                                        weAreBanned = true;
                                        break;
                                    }
                                }
                            }
                            if (
                                GUILayout.Button(
                                    (weAreBanned ?
                                        "(You Are Banned)" :
                                        (isLocked ? "(Password Protected)" : "Join Game")),
                                    GUILayout.Width(170)))
                            {
                                Game.useNAT = (element.useNat || forceNAT);
                                temp = Network.Connect(element.ip, element.port) + "";
                                netConIP = element.ip;
                                netConPort = element.port;
                                netConAttempts = 1;
                                if (temp != "NoError")
                                {
                                    GameData.errorMessage = "Could not join game: " + temp + "";
                                }
                                else
                                {
                                    GameData.errorMessage = "...Connecting to Game...\n(" +
                                        element.ip[0] +
                                        ":" +
                                        element.port +
                                        ((element.useNat || forceNAT) ? " NAT" : "") +
                                        ")\n\n\n\nPlay safe! Don't share personal information online,\nand don't trust anyone who asks you for it.";
                                }
                            }
                        }
                        GUILayout.EndHorizontal();
                        if (
                            Event.current.type != EventType.Layout &&
                            mouseInServerList &&
                            GUILayoutUtility
                                .GetLastRect()
                                .Contains(Event.current.mousePosition))
                        {
                            serverDetails =
                                (element.connectedPlayers > 0 ? element.connectedPlayers.ToString() : "") +
                                (!serverPlayers.Equals("") ? " Players: " + serverPlayers.Replace(",", ", ") : "") +
                                (serverLag != "" ? "\nAvg Ping: " + serverLag + " ms" : "") +
                                "\n" +
                                element.ip[0] +
                                ":" +
                                element.port +
                                (element.useNat ? " NAT" : "") +
                                (serverVersion != 0.0 ? " (» Dedicated Host Server)" : "");
                        }
                        i++;
                    }
                }
				if (activePlayersVisible == 0)
				{
					if (Time.time < lastHostListRefresh + hostListRefreshTimeout)
					{
						GUILayout.Label("\n\n\nLoading Server List...\n" +
                            (hostListRefreshTimeout + lastHostListRefresh - Time.time));
					}
					else
					{
						GUILayout.Label("\n\n\nNo active games could be found.\nPress \"Host Game >>\" above to start your own!\n");
                        GUILayout.Label((userCode != "" ?
                            "You are viewing only games with the secret code \"" +
                                userCode +
                                "\".\n(Press \"<< Change Name\" above to edit this code)" :
                            "You are running Mars Explorer version " +
                                GameData.gameVersion +
                                ",\nand will only see games hosted by others using this version."));
						GUILayout.Label((!autoHostListRefresh) ?
                            "\n(Press the \"Refresh List\" button in the Networking Settings panel to check for new games)" :
                            "\n(This list refreshes automatically)");
					}
				}

				if (useAlternateServer)
				{
					GUILayout.Label("\n(You are using the backup list server, and will only see games of others doing the same)");
				}
				if (filterNATHosts)
				{
					GUILayout.Label("\n(All games requiring Network Address Translation have been hidden)");
				}

			}
			else
			{
                GUILayout.Label("\n\n" +
                    (disableMasterServer ?
                        "Your computer" :
                        "Even if your computer") +
                        " isn't connected to the internet - don't worry!\nYou can still play Mars Explorer on your own, or with friends on your local network.\n\nIf a friend is already hosting a game, enter their IP address here:\n");
				if (remoteIP == null)
				{
					getRemoteIP();
				}
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.BeginVertical(GUILayout.Width(300f));
				GUILayout.BeginHorizontal();
				remoteIP = GUILayout.TextField(remoteIP);
				GUILayout.Space(5f);
                port = int.Parse(GUILayout.TextField(port + "", GUILayout.Width(60)));
				GUILayout.EndHorizontal();
				GUILayout.Space(5f);
				if (GUILayout.Button("Connect to Game Server"))
				{
                    GameData.errorMessage = "...Connecting to Game...\n(" +
                        remoteIP +
                        ":" +
                        port +
                        (Game.useNAT ? " NAT" : "") +
                        ")\n\n\n\nPlay safe! Don't share personal information online,\nand don't trust anyone who asks you for it.";
					Network.Connect(remoteIP, port);
					List<String> remIP = new List<String>();
					remIP.Add(remoteIP);
                    netConIP = remIP.ToArray();
					netConPort = port;
					netConAttempts = 1;
					PlayerPrefs.SetString("remoteIP", remoteIP);
				}
				GUILayout.Space(5f);
				Game.useNAT = GUILayout.Toggle(Game.useNAT, " Enable NAT (generally unneeded)");
				GUILayout.EndVertical();
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}

			if (activePlayers > 0)
			{
				GUILayout.Space(30f);
				GUILayout.Label(activePlayers +
                    " players online - " +
                    activePlayersVisible +
                    " players in this version - " +
                    availableServers +
                    " available dedicated servers");
			}

			GUILayout.EndScrollView();
			if (Event.current.type != EventType.layout)
			{
				mouseInServerList = GUILayoutUtility
                    .GetLastRect()
                    .Contains(Event.current.mousePosition);
			}

			GUILayout.FlexibleSpace();
			GUILayout.Space(5f);

			if (showSettings)
			{
				GUILayout.BeginHorizontal();
				if (!disableMasterServer)
				{
					if (useMasterServer)
					{
						if (GUILayout.Button("Switch to Direct Connect"))
						{
							useMasterServer = false;
							Game.useNAT = false;
						}
					}
					else if (GUILayout.Button("Switch to Server List"))
					{
						useMasterServer = true;
					}
				}
				if (useMasterServer)
				{
					GUILayout.Space(5f);
					if (GUILayout.Button("Refresh Games"))
					{
						MasterServer.ClearHostList();
						MasterServer.RequestHostList(gameName);
						lastHostListRefresh = -1f;
						lastHostListRequest = Time.time;
					}
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					hostDedicated = GUILayout.Toggle(
                        hostDedicated,
                        "Utilize dedicated servers for hosting games");
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
                    GUILayout.Label(testMessage +
                        masterServerMessage +
                        (masterServerConFailures > 0 ?
                            " (" + masterServerConFailures + " failures)" :
                            "") +
                        (useMasterServer ?
                            " (Master Server @ " +
                                MasterServer.ipAddress +
                                ":" +
                                MasterServer.port +
                                ")" :
                            "") +
                        (autoHostListRefresh && useMasterServer ?
                            " (Autorefresh in " +
                                Mathf.Ceil(lastHostListRequest + hostListRefreshTimeout - Time.time) +
                                ")" :
                            ""));
					if (useMasterServer)
					{
						forceNAT = GUILayout.Toggle(forceNAT, "Force NAT");
					}
				}
				else
				{
					GUILayout.EndHorizontal();
					GUILayout.Space(3f);
					GUILayout.BeginHorizontal();
				}
				GUILayout.EndHorizontal();
			}

			GUILayout.BeginHorizontal();

			if (Application.platform == RuntimePlatform.WindowsPlayer ||
                Application.platform == RuntimePlatform.OSXPlayer)
			{
				if (GUILayout.Button("<< Exit Game", GUILayout.Height(30f)))
				{
					Application.Quit();
				}
				GUILayout.Space(5f);
			}

			if (showSettings)
			{
				if (GUILayout.Button("Hide Settings", GUILayout.Height(30f)))
				{
					showSettings = false;
				}
			}
			else if (GUILayout.Button("Show Settings", GUILayout.Height(30f)))
			{
				showSettings = true;
			}

			if (messages != null && messages.Length > 0)
			{
                foreach (String msg in messages)
                {
                    String[] val = msg.Split(","[0]);
                    GUILayout.Space(5f);
                    if (GUILayout.Button(
                        val[0],
                        GUILayout.Height(30)))
                    {
                        OpenURL(val[1]);
                    }
                }
			}
			GUILayout.EndHorizontal();
		}

		if (
            autoHostListRefresh &&
            (Time.time > lastHostListRequest + hostListRefreshTimeout ||
                lastHostListRefresh < 0f) &&
            useMasterServer)
		{
			MasterServer.RequestHostList(gameName);
			if (lastHostListRefresh <= 0f)
			{
				lastHostListRefresh = Time.time;
			}
			lastHostListRequest = Time.time;
		}
	}

    public IEnumerator LoadGame()
    { 
        GuiAnimate = 1;
        yield return new WaitForSeconds(0.75f);
        Application.LoadLevel(2);
    }

    public IEnumerator authenticateUser()
    {
        userAuthenticating = "true";
        WWW www = new WWW(
            "http://marsxplr.com/user/authenticate.atis-u-" +
            Regex.Replace(
                WWW.EscapeURL(userNameTemp),
                "-",
                "%2d").Replace(".", "%2e") +
            "-p-" +
            Regex.Replace(
                WWW.EscapeURL(userPassword),
                "-",
                "%2d").Replace(".", "%2e"));
        yield return www;
        if (www == null || www.text == "")
        {
            userAuthenticating = "Authentication server is unreachable";
        }
        else if (www.text == "-1")
        {
            userAuthenticating = "";
        }
        else if (www.text == "-2")
        {
            userAuthenticating = "Username not found";
        }
        else if (www.text == "-3")
        {
            userAuthenticating = "Incorrect password";
        }
        else if (www.text == "-4")
        {
            userAuthenticating = "Too many login attempts";
        }
        else {
            String[] data = www.text.Split(":"[0]);
            if (
                String.Equals(
                    data[0],
                    userNameTemp,
                    StringComparison.CurrentCultureIgnoreCase)
                )
            {
                userName = "(" + userNameTemp + ")+";
            }
            else
            {
                userName = "" + data[0] + " (" + userNameTemp + ")+";
            }
            if (userCode == "{Code}")
            {
                userCode = "";
            }

            if (userRemembered)
            {
                PlayerPrefs.SetString("userName", userNameTemp);
                PlayerPrefs.SetString("userPassword", userPassword);
                PlayerPrefs.SetInt("userRemembered", 1);
                PlayerPrefs.SetInt("userRegistered", 1);
            }
            else
            {
                PlayerPrefs.SetString("userName", "");
                PlayerPrefs.SetString("userPassword", "");
                PlayerPrefs.SetInt("userRemembered", 0);
                PlayerPrefs.SetInt("userRegistered", 0);
                userNameTemp = "";
                userPassword = "";
            }
        }
    }

	public int sortHostArray(HostData a, HostData b)
	{
		return (a.connectedPlayers > b.connectedPlayers) ?
            (-1) :
            ((a.connectedPlayers < b.connectedPlayers) ?
                1 :
                0);
	}

	public void errorWindow(int id)
	{
		scrollPosition = GUILayout.BeginScrollView(scrollPosition);
		GUILayout.Label(GameData.errorMessage);
		GUILayout.EndScrollView();
		if (GUILayout.Button("(Dismiss)")
            || Input.GetKeyDown("return") ||
            Input.GetKeyDown("enter") ||
            Input.GetKeyDown(KeyCode.Mouse0))
		{
			GameData.errorMessage = "";
			if (!Network.isServer && Network.peerType != NetworkPeerType.Disconnected)
			{
				Network.Disconnect();
			}
		}
	}

	public void makeWindowUpdate(int id)
	{
		GUILayout.Space(40f);
		GUILayout.Label("A new Mars Explorer version is now available on the Discord Server:");
		GUILayout.Space(10f);
		if (
            GUILayout.Button(">> Go to the Gitea Releases page to Download MarsXPLR version " +
                outdated +
                "! <<",
            GUILayout.Height(40f)))
		{
            OpenURL("https://gitea.moe/VIA256/marsxplr-decomp/releases");
		}
		GUILayout.Space(30f);
		GUILayout.BeginHorizontal();
		GUILayout.Space(100f);
		if (GUILayout.Button("Ignore warning, Play Anyway"))
		{
			outdated = "";
		}
		GUILayout.Space(100f);
		GUILayout.EndHorizontal();
		GUILayout.Space(40f);
	}

	public void getRemoteIP()
	{
		remoteIP = PlayerPrefs.GetString("remoteIP", "127.0.0.1");
	}

    public static void OpenURL(String url)
    { 
        //Exit Fullscreen
        if (Screen.fullScreen)
        {
            if (
                Application.platform == RuntimePlatform.WindowsWebPlayer ||
                Application.platform == RuntimePlatform.OSXWebPlayer ||
                Application.platform == RuntimePlatform.OSXDashboardPlayer)
            {
                Screen.fullScreen = false;
            }
            else
            {
                Resolution resolution = Screen.resolutions[Screen.resolutions.Length - 2];
                Screen.SetResolution(resolution.width, resolution.height, false);
            }
        }

        //Open URL
        if (Application.platform == RuntimePlatform.OSXWebPlayer ||
            Application.platform == RuntimePlatform.WindowsWebPlayer)
        {
            Application.ExternalEval("var confirmPopup = window.open('" +
                url +
                "', '_blank', 'width=' + screen.availWidth + ', height=' + screen.availHeight + ',toolbar=yes,location=yes,directories=yes,status=yes,menubar=yes,scrollbars=yes,copyhistory=no,resizable=yes'); if(!confirmPopup) { if(!confirm('I\\'m Sorry: Your browser blocked the window I attempted to open for you. Please instruct your browser to allow popups and click the link again, or hit \"Cancel\" - and I\\'ll redirect you from Mars Explorer to your intended destination automatically.')) { window.location = '" +
                url +
                "'; } }");
        }
        else Application.OpenURL(url);
    }

    public String htmlDecode(String str)
    {
        return Regex.Replace(str, "<[^>]*?>", "")
            .Replace("&#34;", "\"")
            .Replace("&#39;", "'")
            .Replace("&amp;", "&");
    }

	public static string sha1sum(String strToEncrypt)
	{
		UTF8Encoding encoding = new UTF8Encoding();
        byte[] bytes = encoding.GetBytes(strToEncrypt);
		
        // encrypt bytes
        SHA1CryptoServiceProvider md5 = new SHA1CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);
		
        // Convert the encrypted bytes back to a string (base 16)
        string hashString = "";

		for (int i = 0; i < hashBytes.Length; i++)
		{
			hashString += Convert.ToString(
                hashBytes[i], 16)
                .PadLeft(2, "0"[0]);
		}
		
        return hashString.PadLeft(32, "0"[0]);
	}
}
