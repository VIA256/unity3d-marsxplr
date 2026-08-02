using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public class GUIPanel
{
    public string name;
    public bool active = true;
    public bool open;
    public bool important;
    public int minHeight = 300;
    public int maxHeight;
    public float curHeight;
    public float desHeight;
    public Vector2 scrollPos;
    public float openTime;
}

[Serializable]
public class unauthPlayer
{
    public NetworkPlayer p;
    public string n;
    public float t;
    public unauthPlayer(NetworkPlayer p, string n, float t)
    {
        this.p = p;
        this.n = n;
        this.t = t;
    }
}

[Serializable]
public class Game : MonoBehaviour
{
	public GUISkin GameSkin;
	public static GUISkin Skin;
	public GUIStyle hudTextStyle;
	public GUIPanel[] GUIPanels;
	private float closePanel;
	public GameObject[] GameVehicles;
	public GameWorldDesc[] GameWorlds;
	private WhirldIn whirldIn = new WhirldIn();
	[NonSerialized]
	public bool WorldIsCustom = false;
	[NonSerialized]
	public GameWorldDesc WorldDesc = new GameWorldDesc();
	public GameObject WorldEntryEffect;
	public GameObject objectVehicleObj;
	public static GameObject objectVehicle;
	public GameObject objectMarkerObj;
	public static GameObject objectMarker;
	public GameObject objectMarkerQuarryObj;
	public static GameObject objectMarkerQuarry;
	public GameObject objectMarkerMeObj;
	public static GameObject objectMarkerMe;
	public GameObject objectRocketObj;
	public static GameObject objectRocket;
	public GameObject objectRocketSnipeObj;
	public static GameObject objectRocketSnipe;

	public bool isHost = false;
	public bool isRegistered = false;

	public Texture2D cursor;
	public Texture2D cursorLook;
	public Vector2 cursorOffset;
	[NonSerialized]
	public float kpDur = 0.1f;
	[NonSerialized]
	public float kpTime;

	private int hostPanelTab = 0;
	// /*UNUSED*/ private int windowVehicleHeight = 0;
	private Vector2 scrollPosition;
	private bool killServer = false;
	private int netKillMode = 0;
	private int GameVehicleID;
	[NonSerialized]
	public float quarryDist;
	[NonSerialized]
	public int botsInGame = 0;
	public static float GUIAlpha = 0f;
	private int GuiAnimate = -1;
	public static GameObject Player;
	public static Vehicle PlayerVeh;
	public static Vehicle QuarryVeh;
	public static CameraVehicle CameraVehicle;
	public static Messaging Messaging;
	public static Settings Settings;
	public static Game Controller;
    //[NonSerialized]
	public bool worldLoaded = false;
    //[NonSerialized]
	public bool loadingWorld = true;
	[NonSerialized]
	public float worldLoadTime;
	[NonSerialized]
	public bool hostRegistered = false;
	[NonSerialized]
	public string serverName = "";
	[NonSerialized]
	public string serverPassword = "";
	private float authTime;
	private float authUpdateTime = 2f;
	[NonSerialized]
	public bool serverHidden = false;
	[NonSerialized]
	public Hashtable authenticatedPlayers = new Hashtable();
	//private ArrayList authenticatingPlayers = new ArrayList();
	[NonSerialized]
	public static Hashtable Players;
	public List<unauthPlayer> unauthPlayers = new List<unauthPlayer>();
	
    public Color vehicleIsItColor;
	public Color vehicleIsItAccent;

    //FPS Counter
    private float fpsTime;  // FPS accumulated over the interval
    private int fpsFrames;  // Frames drawn over the interval
	private int heavyTickRate = 2;
	[HideInInspector]
	public float fps;
	
	[NonSerialized]
	public static bool useNAT = false;

	[HideInInspector]
	public void Start()
    {
		//if i started in the game scene in the editor, the master server stuff must be set or it defaults to some unknown thing
		if(!Lobby.scenePlayed)
		{
			MasterServer.ipAddress = "omc.band";
			MasterServer.port = 25010;
		}
		
		//Network.minimumAllocatableViewIDs = 3;
		if (Network.peerType == NetworkPeerType.Disconnected)
        {
			//We are running in the editor, and have started the game in world load mode
			Network.InitializeServer(9, 2500, !Network.HavePublicAddress());
		}
		if (Network.isServer)
        {
			isHost = true;
		}
		
		objectVehicle = objectVehicleObj;
		objectMarker = objectMarkerObj;
		objectMarkerQuarry = objectMarkerQuarryObj;
		objectMarkerMe = objectMarkerMeObj;
		objectRocket = objectRocketObj;
		objectRocketSnipe = objectRocketSnipeObj;
		Skin = GameSkin;
		CameraVehicle = (CameraVehicle)Camera.main.GetComponent<CameraVehicle>();
		Settings = (Settings)gameObject.GetComponent<Settings>();
		Messaging = (Messaging)gameObject.GetComponent<Messaging>();
		Controller = (Game)gameObject.GetComponent<Game>();
		if (GameData.userName == "") GameData.userName = "MarsRacer";
		Settings.bannedIPs = GameData.masterBlacklist;
		Settings.networkMode = GameData.networkMode;
		Players = new Hashtable();
		
		botsInGame = 0;
		worldLoaded = false;
		
		if (GameData.gameWorlds == null) GameData.gameWorlds = GameWorlds;
		//whirldIn = new WhirldIn();
		//whirldIn.worldTerrainTextures = worldTerrainTextures;
		StartCoroutine_Auto(Init());
	}

	public void OnDisable()
    {
		//Unload whatever AssetBundles we were using
		if (whirldIn != null) whirldIn.Cleanup();
	}

	public IEnumerator Init()
    {
        //Wait for Server to send us players via RPC - Probably totally unnecessary
        yield return null;
        //We are now ready to recieve RPC calls!
        Network.isMessageQueueRunning = true;

        //Allow World to Load
        while (worldLoaded == false) yield return null;
        Settings.fogColor = RenderSettings.fogColor;
        GuiAnimate = 1;
        while (GuiAnimate != 0) yield return null;
        yield return new WaitForSeconds(0.25f);
        GuiAnimate = -1;
        loadingWorld = false;
        worldLoadTime = Time.time;
        GameObject worldOBJ = GameObject.Find(whirldIn.worldName);
        Transform[] objs = worldOBJ.GetComponentsInChildren<Transform>();
        if (
            (whirldIn.worldParams["ccc"] != null && (int)whirldIn.worldParams["ccc"] != 1) &&
            objs.Length > 15)
        {
            worldOBJ.AddComponent<CombineChildren>();
        }
        //Give the combine children script a chance to create the optimized world meshes before we start assigning layers to everything...
        yield return null;
        
        //Ensure standard world components are present
        if (GameObject.Find("Base"))
        {
            World.baseTF = GameObject.Find("Base").transform;
        }
        else
        {
            World.baseTF = ((GameObject)GameObject.Instantiate(
                Resources.Load("Base"),
                Vector3.up * 5,
                Quaternion.identity))
                .transform;
        }

        World.terrains = (Terrain[])FindObjectsOfType(typeof(Terrain));
        
        if (
            whirldIn.worldParams["env"] != null &&
            (String)whirldIn.worldParams["env"] == "space")
        {
            //usedSkybox = RenderSettings.skybox = SkyboxSpace;
            //RenderSettings.fogColor = new Color(0.07f, 0.02f, 0.095f, 1f);
        }
        else
        {
            if (GameObject.Find("Sea"))
            {
                World.sea = GameObject.Find("Sea").transform;
                Settings.lavaAlt = World.sea.position.y;
            }
            /*else
            {
                Settings.lavaAlt = -10f;
                World.sea = ((GameObject)GameObject.Instantiate(
                    Resources.Load("Sea"),
                    new Vector3(0f, Settings.lavaAlt, 0f),
                    Quaternion.identity))
                    .transform;
            }*/
            if (
                !GameObject.Find("Floor") &&
                !FindObjectOfType(typeof(Terrain)) &&
                !World.sea &&
                whirldIn.worldParams["nofloor"] == null &&
                (String)whirldIn.worldParams["floor"] != "0")
            {
                GameObject.Instantiate(
                    Resources.Load("Floor"),
                    Vector3.zero,
                    Quaternion.identity);
            }
        }

        bool thereisalight = false;
        foreach (Light light in (Light[])FindObjectsOfType(typeof(Light)))
        {
            if (light.gameObject.name != "VehicleLight")
            {
                thereisalight = true;
            }
        }
        //Make sure there is a light in the scene
        if (!thereisalight) GameObject.Instantiate(Resources.Load("Light"));

        //Add Player!
        mE(World.baseTF.transform.position);
        yield return new WaitForSeconds(1f); //Give server settings a chance to load
        int veh = 0;
        if (Settings.buggyAllowed) veh = 0;
        else if (Settings.tankAllowed) veh = 2;
        else if (Settings.hoverAllowed) veh = 1;
        else if (Settings.jetAllowed) veh = 3;
        GameVehicleID = veh;
        String temp = GameData.userName;
        int i = 1;
        while (
            (i == 1 && GameObject.Find(temp) != null) ||
            (GameObject.Find(temp + " " + i) != null))
        {
            i++;
        }
        GetComponent<NetworkView>().RPC(
            "iV",
            RPCMode.All,
            Network.AllocateViewID(),
            Settings.networkMode,
            GameVehicleID,
            (i != 1 ? temp + " " + i : temp),
            0,
            (isHost ? 1 : 0),
            0,
            0,
            false);

        if (isHost)
        {
            if (!Network.isServer)
            {
                sSHS(); //Send dedicated server my default settings
            }
            
            if (Network.isServer)
            {
                Game.Messaging.broadcast(
                    "You are hosting a server!\n" +
                    (serverPassword != "" ?
                        "Password: " + serverPassword + "\n" :
                        "") +
                    "IP: " +
                    Network.player.ipAddress +
                    "\n" +
                    (Network.player.externalIP != "" ?
                        "External: " + Network.player.externalIP :
                        "") +
                    "\nPort: " +
                    Network.player.port);
                yield return new WaitForSeconds(1f);
                StartCoroutine_Auto(registerHostSet());
            }
            else
            {
                Game.Messaging.broadcast(
                    "You are hosting a game on a dedicated server!\n" +
                    (serverPassword != "" ?
                        "Password: " + serverPassword + "\n" :
                        "") +
                    "IP: " +
                    Network.connections[0].ipAddress +
                    "\n" +
                    (Network.connections[0].externalIP != "" ?
                        "External: " + Network.connections[0].externalIP :
                        "") +
                    "\nPort: " +
                    Network.connections[0].port);
            }

            if (whirldIn.worldParams["message"] != null)
            {
                Settings.serverWelcome = (String)whirldIn.worldParams["message"];
                GetComponent<NetworkView>().RPC(
                    "msg",
                    RPCMode.All,
                    Settings.serverWelcome,
                    (int)chatOrigins.Remote);
            }
            if (whirldIn.worldParams["marsxplr"] != null)
            {
                GetComponent<NetworkView>().RPC(
                    "sSS",
                    RPCMode.All,
                    whirldIn.worldParams["marsxplr"]);
            }
        }
        else
        {
            Game.Messaging.broadcast(
                (i != 1 ? temp + " " + i : temp) +
                " has joined!");
        }

        //Init Game Prefs
        Settings.serverDefault = Settings.packServerPrefs();
        Settings.serverString = Settings.serverDefault;
        Settings.updatePrefs();

        //Randomize new player colors if they are not custom
        yield return null;
        Settings.colorCustom = (PlayerPrefs.GetInt("vehColCustom") == 1);
        if (!Settings.colorCustom) StartCoroutine_Auto(Settings.ramdomizeVehicleColor());
	}

	public void Update()
    {
		Application.runInBackground = true;
		
		if (
			Settings.resetTime > 0f &&
			10f - (Time.time - Settings.resetTime) < 1f)
        {
			if ((bool)PlayerVeh.ramoSphere) Destroy(PlayerVeh.ramoSphere);
			Player.transform.position = World.baseTF.position;
			Player.transform.rotation = World.baseTF.rotation;
			Player.GetComponent<Rigidbody>().isKinematic = false;
			Player.GetComponent<Rigidbody>().velocity = Vector3.zero;
			Settings.resetTime = -10f;
			Settings.updatePrefs(); //Rebuild a new ramosphere
		}
		else if (Settings.resetTime < -1f) Settings.resetTime += Time.deltaTime;
		
		if (
			Settings.serverUpdateTime != 0f &&
			Settings.serverUpdateTime < Time.time)
        {
			Settings.serverString = Settings.packServerPrefs();
			GetComponent<NetworkView>().RPC("sSS", RPCMode.Others, Settings.serverString);
			if (isHost && !Network.isServer)
            {
				//Send dedicated server my default settings
				sSHS();
			}
			Settings.serverUpdateTime = 0f;
		}
		
		if (
			Settings.colorUpdateTime != 0f &&
			Settings.colorUpdateTime < Time.time)
        {
			Settings.colorUpdateTime = 0f;
			Settings.saveVehicleColor();
		}

		//Fade GUI Out
		if (GuiAnimate == 1)
        {
			if (GUIAlpha <= 0f)
            {
				GUIAlpha = 0f;
				GuiAnimate = 0;
			}
			else GUIAlpha -= Time.deltaTime * 0.35f;
		}
		//Fade GUI In
		else if (GuiAnimate == -1)
        {
			GUIAlpha = 0f;
			GuiAnimate = -2;
		}
		else if (GuiAnimate == -2)
        {
			if (GUIAlpha >= 1)
            {
				GUIAlpha = 1f;
				GuiAnimate = 0;
			}
			else GUIAlpha += Time.deltaTime * 0.35f;
		}

		//Let server know that we are knocking at the door
		if (
			Network.peerType != NetworkPeerType.Disconnected &&
			!isHost &&
			WorldDesc.url == "" &&
			Time.time - authUpdateTime > 1f &&
			Time.timeSinceLevelLoad > 3f)
        {
			GetComponent<NetworkView>().RPC(
				"lMI",
				RPCMode.Others,
				Network.player,
				GameData.userName);
			authUpdateTime = Time.time;
		}
		
		if (!worldLoaded)
        {
			if(CameraVehicle.mb.blurAmount < 1f)
			{
				CameraVehicle.mb.blurAmount = CameraVehicle.mb.blurAmount + Time.deltaTime;
			}
		}

		//Handle toggling fullscreen
		if (
			Input.GetKeyDown(KeyCode.Alpha0) &&
			!Messaging.chatting &&
			Time.time > kpTime)
        {
			kpTime = Time.time + kpDur;
			Settings.toggleFullscreen();
		}

        fpsFrames++;
        if (Time.time > fpsTime)
        {
			fps = fpsFrames / heavyTickRate;
			fpsTime = Time.time + (float)heavyTickRate;
			fpsFrames = 0;

            //Prune Unauth Players list
			if (isHost)
            {
				for (int i = 0; i < unauthPlayers.Count; i++)
                {
                    if (Time.time - unauthPlayers[i].t > 2f)
                    {
                        unauthPlayers.RemoveAt(i);
                    }
				}
			}

            //Prune Players list
            foreach (DictionaryEntry plrE in Players)
            {
                if (plrE.Value == null)
                {
                    Debug.Log(plrE.Key + " null - removed from players list");
                    Players.Remove(plrE.Key);
                    break; //Don't want to continue iterating through players, as the hashtable is now out of sync.
                }
            }

            //Prune Ghosts
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player"))
            {
                bool found = false;
                foreach (DictionaryEntry plrE in Players)
                {
                    if ((String)plrE.Key == go.name)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found) //This vehicle is deserted
                {
                    Destroy(go);
                    Debug.Log("Abandoned vehicle destroyed: " + go.name);
                    break;
                }
            }
		}
	}

	public void OnGUI()
    {
		//Cursor Locking
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            GUI.depth = -999;
            GUI.Label(
                new Rect(
                    Screen.width / 2 - cursorOffset.x,
                    Screen.height / 2 - cursorOffset.y,
                    cursor.width,
                    cursor.height),
                (Game.Settings.lasersAllowed &&
                    Game.Settings.firepower[PlayerVeh.vehId] > 0 ?
                    cursor :
                    cursorLook));
        }

        //Basic Setup
        GUI.skin = Skin;

        Color guicol = GUI.color;
        guicol.a = GUIAlpha;
        GUI.color = guicol;

        GUI.depth = 1;

        //World Setup
        if (loadingWorld)
        {
            GUI.Window(
                3,
                new Rect(
                    Screen.width / 2 - 300,
                    Screen.height / 2 - 250,
                    600,
                    500),
                WindowServerSetup,
                "",
                "windowChromeless");
            return;
        }

        //Exit Game
        float hght = (Game.Settings.simplified ? 40 : 25);
        float wdth = (Game.Settings.simplified ?
            40 :
            Mathf.Min(Mathf.Max(
                170f,
                Screen.width / 4), 250));
        if (isHost)
        {
            if (worldLoaded)
            {
                if (killServer == false)
                {
                    if (GUI.Button(
                        new Rect(10, 10, wdth, hght),
                        (Game.Settings.simplified ?
                            "<<" :
                            "<< Stop Hosting Game")))
                    {
                        if (
                            !Network.isServer ||
                            Network.connections.Length < 1)
                        {
                            netKillMode = 1;
                            Network.Disconnect();
                            unregisterHost();
                        }
                        else
                        {
                            killServer = true;
                            //Game.Settings.simplified = true;
                        }
                    }
                }
                else
                {
                    if (GUI.Button(
                        new Rect(10, 10, wdth, hght),
                        (Game.Settings.simplified ?
                            "<<" :
                            "<< Confirm Stop")))
                    {
                        netKillMode = 1;
                        Network.Disconnect();
                        unregisterHost();
                    }
                    if (GUI.Button(
                        new Rect(
                            Screen.width / 2 - 125,
                            Screen.height / 2 - 100,
                            250,
                            200),
                        "Notice:\n\nYou are hosting this game.\nIf you stop hosting, all the players\nin this game will be disconnected.\n\nIf you really want to, press \"Confirm Stop\".\n Otherwise, click this button to cancel\nthe stop and continue hosting!"))
                    {
                        killServer = false;
                        Game.Settings.simplified = false;
                    }
                }
            }
        }
        else
        {
            if (GUI.Button(
                new Rect(10, 10, wdth, hght),
                (Game.Settings.simplified ?
                    "<<" :
                    "<  Exit Game")))
            {
                netKillMode = 1;
                Network.Disconnect();
            }
        }

        //Hide GUI Button
        if (GUI.Button(
            new Rect(
                Screen.width - 50,
                Screen.height - 50,
                40,
                40),
            (!Settings.simplified ? ">>" : "<<")))
        {
            Settings.simplified = !Settings.simplified;
        }
        if (Settings.enteredfullscreen && GUI.Button(
            new Rect(
                Screen.width / 2 - 125,
                Screen.height / 2 - 100,
                250,
                200),
            "Welcome to fullscreen mode!\n\nIf you hear a chime noise while holding\nkeyboard buttons; press \"Esc\",\nthen click your mouse, then press \"0\".\n\n{Click this box to play}"))
        {
            Settings.enteredfullscreen = false;
        }

        //Player related stuff beyond this point
        if (!Player || Settings.simplified) return;

        //Hud text
        if (PlayerVeh.vehId == 3)
        {
            GUI.Button(
                new Rect(
                    (Screen.width / 2) - 200,
                    Screen.height - 63,
                    400,
                    20),
                "{Hold Q for no throttle & E for full throttle}",
                hudTextStyle);
        }
        if (Messaging.chatting)
        {
            GUI.Button(
                new Rect(
                    (Screen.width / 2) - 200,
                    Screen.height - 50,
                    400,
                    20),
                "{Keyboard Shortcuts Locked ~ Press \"Tab\" to unlock}",
                hudTextStyle);
        }
        else if (Cursor.lockState == CursorLockMode.Locked)
        {
            GUI.Button(
                new Rect(
                    (Screen.width / 2) - 200,
                    Screen.height - 50,
                    400,
                    20),
                "{Cursor Locked ~ " +
                    (Input.GetButton("Fire2") ?
                        "Release \"Alt\" to unlock" :
                        (Input.GetButton("Snipe") ?
                            "Release \"Shift\" to unlock" :
                            "Press \"2\" to unlock")) +
                    "}",
                hudTextStyle);
        }
        else if (Settings.camMode == 3 || Settings.camMode == 4)
        {
            GUI.Button(
                new Rect(
                    (Screen.width / 2) - 200,
                    Screen.height - 50,
                    400,
                    20),
                "{Use the UIOJKL keys to adjust camera position}",
                hudTextStyle);
        }

        if (Event.current.type != EventType.Layout)
        { 
            
            //Vehicle Switching
            if (!GUIPanels[4].active)
            {
                if (
                    Vector3.Distance(
                        Player.transform.position,
                        World.baseTF.transform.position) < 20 &&
                    Settings.resetTime > -1)
                {
                    GUIPanels[4].openTime = Time.time;
                    GUIPanels[4].open = true;
                    GUIPanels[4].active = true;
                }
            }
            else
            {
                if (
                    Vector3.Distance(
                        Player.transform.position,
                        World.baseTF.transform.position) >= 20 &&
                    Settings.resetTime > -1)
                {
                    GUIPanels[4].active = false;
                }
            }

            //Settings Advisor
            if (!GUIPanels[5].active)
            {
                if (
                    Settings.showHints &&
                    (fps < 20 && Settings.renderLevel > 1 ||
                        fps > 55 && Settings.renderLevel < 5))
                {
                    GUIPanels[5].openTime = Time.time;
                    GUIPanels[5].open = true;
                    GUIPanels[5].active = true;
                }
            }
            else
            {
                if (
                    !Settings.showHints ||
                    !((fps < 20 && Settings.renderLevel > 1) ||
                        (fps > 55 && Settings.renderLevel < 5)) &&
                    Time.time > GUIPanels[5].openTime + 3)
                {
                    GUIPanels[5].active = false;
                }
            }

        }

        //Build SidePanels
        int liquidPanels = 0;
        int solidPanels = 0;
        int buttonHeight = 25;
        int panelSpacing = 5;
        int areaHeight = Screen.height - 60;
        int areaOffset = panelSpacing * 2;
        int areaOffsetDes = areaOffset;

        //Calc SidePanel Heights
        for (int i = 0; i < GUIPanels.Length; i++)
        {
            if (!GUIPanels[i].active) continue;
            if (GUIPanels[i].open)
            {
                liquidPanels++;
            }
            else
            {
                solidPanels++;
            }
        }

        //Area to fill
        for (int i = 0; i < GUIPanels.Length; i++)
        { 
            if(
                !GUIPanels[i].active &&
                GUIPanels[i].curHeight <= buttonHeight)
            {
                continue;
            }
            if (GUIPanels[i].open && GUIPanels[i].active)
            {
                GUIPanels[i].desHeight =
                    (areaHeight - ((solidPanels + liquidPanels) *
                        panelSpacing +
                        solidPanels *
                        buttonHeight)) / liquidPanels;
                if (
                    GUIPanels[i].maxHeight > 0 &&
                    GUIPanels[i].desHeight > GUIPanels[i].maxHeight)
                {
                    GUIPanels[i].desHeight = GUIPanels[i].maxHeight;
                }
                else if (
                    GUIPanels[i].minHeight > 0 &&
                    GUIPanels[i].desHeight < GUIPanels[i].minHeight)
                {
                    GUIPanels[i].desHeight = GUIPanels[i].minHeight;
                }
            }
            else
            {
                GUIPanels[i].desHeight = buttonHeight;
            }
            if (
                GUIPanels[i].curHeight > GUIPanels[i].desHeight - 1 &&
                GUIPanels[i].curHeight < GUIPanels[i].desHeight + 1)
            {
                GUIPanels[i].curHeight = GUIPanels[i].desHeight;
            }
            else
            {
                GUIPanels[i].curHeight = Mathf.Lerp(
                    GUIPanels[i].curHeight,
                    GUIPanels[i].desHeight,
                    Time.deltaTime * 3);
            }

            //We are a button
            if (GUIPanels[i].curHeight < buttonHeight * 1.5)
            {
                String txt = GUIPanels[i].name;
                if (i == 1)
                {
                    txt +=
                        " (" +
                        fps.ToString("f0") +
                        " FPS)";
                }
                else if (
                    i == 3 &&
                    isHost &&
                    unauthPlayers.Count > 0)
                {
                    txt =
                        "* " +
                        txt +
                        " (" +
                        unauthPlayers.Count +
                        ") *";
                }
                if (
                    Event.current.type != EventType.Layout &&
                    GUI.Button(
                        new Rect(
                            Screen.width - 180,
                            areaOffset,
                            170,
                            GUIPanels[i].curHeight),
                        txt))
                {
                    GUIPanels[i].open = !GUIPanels[i].open;
                    GUIPanels[i].openTime = Time.time;
                    GUI.FocusWindow(20 + i);
                }
            }

            //We are a panel
            else
            {
                GUI.Window(
                    20 + i,
                    new Rect(
                        Screen.width - 180,
                        areaOffset,
                        170,
                        GUIPanels[i].curHeight),
                    GUIPanel,
                    GUIPanels[i].name + ":",
                    (GUIPanels[i].important ?
                        "boldWindow" :
                        "Window"));
            }

            areaOffset += (int)GUIPanels[i].curHeight + panelSpacing;
            areaOffsetDes += (int)GUIPanels[i].desHeight + panelSpacing;
        }

        //Close oldest panel if we are flowing out of the area
        if (Event.current.type != EventType.Layout)
        {
            if (areaOffsetDes > areaHeight + panelSpacing * 4)
            {
                closePanel += Time.deltaTime;
                if (closePanel > 1.5)
                {
                    int oldestI = -1;
                    for (int i = 0; i < GUIPanels.Length; i++)
                    {
                        if (
                            GUIPanels[i].open &&
                            GUIPanels[i].openTime > 0 &&
                            GUIPanels[i].openTime < Time.time - 0.5 &&
                            (oldestI == -1 ||
                                GUIPanels[i].openTime < GUIPanels[oldestI].openTime))
                        {
                            oldestI = i;
                        }
                    }
                    if (oldestI != -1) GUIPanels[oldestI].open = false;
                }
            }
            else closePanel = 0;
        }
	}

	public void GUIPanel(int id)
    {
		int i = id - 20;
		GUIStyle closeButtonStyle = GUI.skin.GetStyle("close_button");
		if (GUI.Button(
            new Rect(
                closeButtonStyle.padding.left,
                closeButtonStyle.padding.top,
                closeButtonStyle.normal.background.width,
                closeButtonStyle.normal.background.height),
            "",
            "close_button"))
        {
            GUIPanels[i].open = false;
		}
        GUIPanels[i].scrollPos = GUILayout.BeginScrollView(GUIPanels[i].scrollPos);


		switch (i)
        {
		case 0:
			Settings.showDialogServer();
			break;
		case 1:
			Settings.showDialogGame();
			break;
		case 2:
			Settings.showDialogPlayer();
			break;
		case 3:
			Settings.showDialogPlayers();
			break;
		case 4:
			showDialogVehicles();
			break;
		case 5:
			showDialogRenderHints();
			break;
		default:
			GUILayout.Label("{Unknown Panel}");
			break;
		}

		GUILayout.Space(5f);
		GUILayout.EndScrollView();
	}

	public void showDialogVehicles()
    {
        if (GameVehicleID == -1) return;
        GUILayout.FlexibleSpace();
        GUILayout.Space(5f);
        GUILayout.Label(
            "You are currently commanding a " +
            GameVehicles[GameVehicleID].name +
            ":");
        GUILayout.Space(10f);
        GUILayout.FlexibleSpace();

        int i = -1;
        foreach (GameObject veh in GameVehicles)
        {
            i++;
            if (i == GameVehicleID) continue;
            if (i == 0 && !Settings.buggyAllowed)
            {
                GUILayout.Label("(Buggy unavailable)", GUILayout.Height(25));
            }
            else if (i == 0 && !Settings.hoverAllowed)
            {
                GUILayout.Label("(Hovercraft unavailable)", GUILayout.Height(25));
            }
            else if (i == 0 && !Settings.tankAllowed)
            {
                GUILayout.Label("(Tank unavailable)", GUILayout.Height(25));
            }
            else if (i == 0 && !Settings.jetAllowed)
            {
                GUILayout.Label("(Jet unavailable)", GUILayout.Height(25));
            }
            else if (GUILayout.Button(
                ">> Switch to a " + veh.name,
                GUILayout.Height(40)))
            {
                Settings.resetTime = -10;
                setVeh(i);
            }
        }

        GUILayout.Space(10);
        GUILayout.FlexibleSpace();
        GUILayout.Label("(You can only switch when at this location)");
	}

	public void showDialogRenderHints()
    {
		if (fps < 20f)
        {
			GUILayout.Label(
                "Low Framerate: " +
                fps.ToString("f0") +
                " FPS\n\nClick \"Game Settings\" above and decrease (<<) the Rendering Quality to speed up your framerate");
		}
		else
        {
			GUILayout.Label(
                "High Framerate: " +
                fps.ToString("f0") +
                " FPS\n\nClick \"Game Settings\" above and increase (>>) the Rendering Quality to make everything look nicer");
		}
		GUILayout.Space(5f);
		if (GUILayout.Toggle(
            Settings.showHints,
            "Enable Settings Advisor") != Settings.showHints)
        {
			Settings.showHints = !Settings.showHints;
			PlayerPrefs.SetInt(
                "showHints",
                Settings.showHints ? 1 : 0);
			Settings.updatePrefs();
		}
	}

	public IEnumerator OnPlayerConnected(NetworkPlayer player)
    {
		//Make sure they aren't banned
        if (Settings.bannedIPs != "")
        {
            String[] banned = Settings.bannedIPs.Split("\n"[0]);
            foreach (String dat in banned)
            {
                String[] val = dat.Split(" "[0]);
                if (val[0] == player.ipAddress)
                {
                    //Game.Messaging.broadcast(player.ipAddress + " attempted to rejoin");
                    GetComponent<NetworkView>().RPC("dN", player, 2);
                    Network.CloseConnection(player, true);
                }
            }
        }

        //Authenticate them if necessary
        while (serverPassword != "")
        {
            if (authenticatedPlayers.ContainsKey(player)) break; //They sent the right password in an RPC - let them join the game
            bool playerIsConnected = false;
            foreach (NetworkPlayer plyr in Network.connections)
            {
                if (plyr == player)
                {
                    playerIsConnected = true;
                }
            }
            if (!playerIsConnected) yield break; //They gave up, and are no longer connected
            yield return null;
        }

        //Sync Vehicles
        foreach (DictionaryEntry plrE in Players)
        {
            Vehicle veh = (Vehicle)plrE.Value;
            GetComponent<NetworkView>().RPC(
                "iV",
                player,
                veh.GetComponent<NetworkView>().viewID,
                veh.networkMode,
                veh.vehId,
                veh.gameObject.name,
                veh.isBot ? 1 : 0,
                veh.isIt,
                veh.score,
                veh.specialInput ? 1 : 0,
                veh.zorbBall);
            veh.GetComponent<NetworkView>().RPC(
                "sC",
                player,
                veh.vehicleColor.r,
                veh.vehicleColor.g,
                veh.vehicleColor.b,
                veh.vehicleAccent.r,
                veh.vehicleAccent.g,
                veh.vehicleAccent.b);
        }

        //Sync Server Prefs
        if (Settings.serverString != Settings.serverDefault)
        {
            Settings.serverString = Settings.packServerPrefs();
            GetComponent<NetworkView>().RPC("sSS", player, Settings.serverString);
        }

        //Send Them World
        GetComponent<NetworkView>().RPC("lW", player, "url=" + WorldDesc.url);

        //Welcome them!
        if (Settings.serverWelcome != "")
        {
            GetComponent<NetworkView>().RPC(
                "msg",
                player,
                Settings.serverWelcome,
                (int)chatOrigins.Remote);
        }

        //Force-Refresh Everyone's NetworkViews
        // /*DEAD LINK*/ http://forum.unity3d.com/viewtopic.php?t=35724&postdays=0&postorder=asc&start=105
        foreach (DictionaryEntry plrE in Players)
        {
            if (plrE.Value == null) continue;
            ((Vehicle)plrE.Value).GetComponent<NetworkView>().enabled = false;
            yield return null;
            ((Vehicle)plrE.Value).GetComponent<NetworkView>().enabled = true;
        }

        //Update Master Server Listing
        yield return new WaitForSeconds(5f);
        StartCoroutine_Auto(registerHost());
    }

	public IEnumerator OnPlayerDisconnected(NetworkPlayer player)
    {
        // /*UNUSED*/ String pName = "";
		
		foreach (DictionaryEntry plrE in Players)
        {
            if (((Vehicle)plrE.Value).GetComponent<NetworkView>().owner == player)
            {
                GetComponent<NetworkView>().RPC("pD", RPCMode.All, plrE.Key);
                break;
            }
        }

        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);
        yield return new WaitForSeconds(1f);
        eSI();
        StartCoroutine_Auto(registerHost());
	}

    public void WindowServerSetup(int id)
    {
        if (!isHost && WorldDesc.url == "")
        {
            GUILayout.FlexibleSpace();
            GUILayout.FlexibleSpace();
            GUILayout.FlexibleSpace();
            GUILayout.Label("This Game is Password Protected:");
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.Space(40f);
            GUILayout.Label("Password:", GUILayout.Width(80f));
            serverPassword = GUILayout.PasswordField(serverPassword, "*"[0]);
            GUILayout.Space(40f);
            GUILayout.EndHorizontal();
            if (authTime > 1 && authTime < Time.time - 3)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("Authentication Failed - please try a different password");
            }
            GUILayout.FlexibleSpace();
            GUILayout.Label("(The host can invite you directly into their game if they desire)");
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.Space(40f);
            if (GUILayout.Button(
                (authTime > 1 && authTime > Time.time - 3 ?
                    "Authenticating..." :
                    ">> Authenticate"),
                GUILayout.Height(40)))
            {
                authTime = Time.time;
                GetComponent<NetworkView>().RPC(
                    "cP",
                    RPCMode.All,
                    Network.player,
                    serverPassword);
            }
            GUILayout.Space(40f);
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(
                "<< Cancel",
                GUILayout.Height(25f),
                GUILayout.Width(150f)))
            {
                netKillMode = 1;
                Network.Disconnect();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        else if (
            whirldIn.status == WhirldInStatus.Success ||
            worldLoaded == true)
        {
            if (whirldIn.status == WhirldInStatus.Success && !worldLoaded)
            {
                worldLoaded = true;
                Settings.simplified = false;
                scrollPosition = Vector2.zero;
                if (whirldIn.info != "") msg(whirldIn.info, (int)chatOrigins.Server);
            }

            GUILayout.Label("\n\n\nWorld Loaded Successfully...\n\n\n");
            GUILayout.BeginHorizontal();
            GUILayout.Space(40f);
            GUILayout.Button("<< Cancel", GUILayout.Height(40f));
            GUILayout.Space(40f);
            GUILayout.EndHorizontal();
            GUILayout.Space(40f);
        }
        else if (whirldIn.status != WhirldInStatus.Idle)
        {
            if (whirldIn.status == WhirldInStatus.Working)
            {
                string threadList = "";
                foreach(DictionaryEntry thread in whirldIn.threads)
                {
                    if (!thread.Value.Equals(""))
                    {
                        threadList +=
                            "\n" +
                            thread.Key +
                            ": ";
                        try
                        {
                            threadList +=
                                Mathf.RoundToInt(
                                    Convert.ToSingle(thread.Value) *
                                    100) +
                                "%";
                        }
                        catch (Exception)
                        {
                            threadList += thread.Value;
                        }
                    }
                }
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                GUILayout.Label(
                    "Loading World:\n\n" +
                    (whirldIn.statusTxt == "" ?
                        "Initializing Whirld Library..." :
                        "" + whirldIn.statusTxt + "...") +
                    (whirldIn.progress > 0 && whirldIn.progress < 1 ?
                        " (" + whirldIn.progress * 100 + "%)" :
                        "") +
                    "\n" +
                    threadList
                );
                GUILayout.EndScrollView();
            }
            else    //Getting ready to host a game
            {
                GUILayout.Label("World Loading Error:\n" + whirldIn.status + 
					(whirldIn.info == "" ? "" :
						"\n\n" + whirldIn.info));
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(40f);
            if (GUILayout.Button(
                (whirldIn.status == WhirldInStatus.Working ?
                    "<< Cancel" :
                    "<< Retry"),
                GUILayout.Height(40f)))
            {
                whirldIn.Cleanup();
                whirldIn = null;
                whirldIn = new WhirldIn();
            }
            GUILayout.Space(40f);
            GUILayout.EndHorizontal();
            GUILayout.Space(40f);
        }
        else if (!isHost)
        {
            GUILayout.Space(150f);
            GUILayout.Label("Initializing connection with game server...");
            netKillMode = 1;
            Network.Disconnect();
        }
        else    //Getting ready to host a game
        {
            if (
                serverName == "" &&
                GUI.GetNameOfFocusedControl() != "serverName")
            {
                serverName = GameData.userName + "'s Game";
            }
            if (
                WorldDesc.name == "" &&
                GUI.GetNameOfFocusedControl() != "worldName")
            {
                WorldDesc.name = "Custom World";
            }

            GUILayout.Space(40f);
            String[] tabs = {
                            "Select a World",
                            "Use Custom World",
                            "Server Settings"};
            hostPanelTab = GUILayout.SelectionGrid(
                hostPanelTab,
                tabs,
                tabs.Length,
                GUILayout.Height(30f));

            //Selecting a ready to go world
            if (hostPanelTab == 0)
            {
                GUILayout.Label("Have fun hosting your game!\n Customize your game's settings in the panels enumerated above,\nand specify a world to explore from the list below:");
                GUILayout.Space(20f);
                GUILayout.BeginHorizontal();
                GUILayout.Space(115f);
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                if (
                    WorldIsCustom &&
                    !GUILayout.Toggle(true, "Using Custom World"))
                {
                    WorldIsCustom = false;
                }
                GUILayout.BeginHorizontal();
                int i = 0;
                foreach (GameWorldDesc Gworld in GameData.gameWorlds)
                {
                    if (!Gworld.featured) continue;
                    if (
                        !WorldIsCustom &&
                        WorldDesc != null &&
                        WorldDesc.name == Gworld.name)
                    {
                        GUILayout.Toggle(
                            true,
                            Gworld.name,
                            GUILayout.Width(170f),
                            GUILayout.Height(22f));
                    }
                    else if (GUILayout.Toggle(
                        false,
                        Gworld.name,
                        GUILayout.Width(170f),
                        GUILayout.Height(22f)))
                    {
                        WorldIsCustom = false;
                        WorldDesc.name = Gworld.name;
                        WorldDesc.url = Gworld.url;
                    }
                    i++;
                    if (i % 2 == 0)
                    {
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(20f);
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                i = 0;
                foreach (GameWorldDesc Gworld in GameData.gameWorlds)
                {
                    if (Gworld.featured) continue;
                    if (
                        !WorldIsCustom &&
                        WorldDesc != null &&
                        WorldDesc.name == Gworld.name)
                    {
                        GUILayout.Toggle(
                            true,
                            Gworld.name,
                            GUILayout.Width(170f));
                    }
                    else if(GUILayout.Toggle(
                        false,
                        Gworld.name,
                        GUILayout.Width(170f)))
                    {
                        WorldIsCustom = false;
                        WorldDesc.name = Gworld.name;
                        WorldDesc.url = Gworld.url;
                    }
                    i++;
                    if(i % 2 == 0)
                    {
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.EndScrollView();
                GUILayout.EndHorizontal();
            }

            //Using a custom world
            else if(hostPanelTab == 1)
            {
                GUILayout.Space(20f);
                if (WorldIsCustom)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(150f);
                    WorldIsCustom = GUILayout.Toggle(
                        WorldIsCustom,
                        "Use Custom World");
                    GUILayout.Space(150f);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(70f);
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                    GUILayout.Space(10f);
                    //if(WorldDesc.url == "") WorldDesc.url = "http://";
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("World Name:", GUILayout.Width(80f));
                    GUI.SetNextControlName("worldName");
                    WorldDesc.name = GUILayout.TextField(WorldDesc.name);
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUI.SetNextControlName("worldUrl");
                    GUILayout.Label("World Url:", GUILayout.Width(80f));
                    String tmp = WorldDesc.url;
                    WorldDesc.url = GUILayout.TextField(WorldDesc.url);
                    if (WorldDesc.url != tmp) WorldDesc.name = "Custom World";
                    GUILayout.EndHorizontal();

                    GUILayout.EndScrollView();
                    GUILayout.Space(70f);
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(150f);
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                    WorldIsCustom = GUILayout.Toggle(WorldIsCustom, "Use Custom World");
                    GUILayout.EndScrollView();
                    GUILayout.Space(150f);
                    GUILayout.EndHorizontal();
                    GUILayout.Space(20f);
                    GUILayout.Label("Mars Explorer incorporates the Unity Whirld system -\nan open source framework which enables you to design your own game worlds,\nand play them inside Mars Exporer!\n\nIf you have a custom world, enable \"Use Custom World\" above to use it in your game.");
                }

                GUILayout.Space(40f);
                GUILayout.BeginHorizontal();
                GUILayout.Space(150f);
                if(GUILayout.Button(">> Learn About the Whirld System"))
                {
                    Lobby.OpenURL("http://web.archive.org/web/20120519040400/http://www.unifycommunity.com/wiki/index.php?title=Whirld");
                }
                GUILayout.Space(150f);
                GUILayout.EndHorizontal();
            }

            //Specifying Server Settings
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(150f);
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);

                GUILayout.Space(10f);

                GUILayout.Label("Your Game's Name:");
                GUI.SetNextControlName("serverName");
                serverName = GUILayout.TextField(serverName, 45);

                GUILayout.Space(20f);
								
								if (
									WorldDesc.url.Length >= 7 &&
									WorldDesc.url.Substring(0, 7) == "file://")
								{
									serverHidden = true;
								}
                else
								{
									serverHidden = GUILayout.Toggle(
                    serverHidden,
                    "Hide This Game From List");
								}
                if(!serverHidden)
                {
                    bool usePass = GUILayout.Toggle(
                        serverPassword != "",
                        "Password Protect This Game");
                    if(usePass)
                    {
                        serverPassword = (serverPassword == "" ? "1" : serverPassword);
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Password:",GUILayout.Width(80f));
                        serverPassword = GUILayout.TextField(
                            (serverPassword == "1" ? "" : serverPassword),
                            45);
                        if(usePass && serverPassword == "") serverPassword = "1";
                        GUILayout.EndHorizontal();
                        //GUILayout.Label("(Your game will be visible in the list, but friends will need this password to join your game)");
                    }
                    else serverPassword = "";
                }
                else
                {
                    GUILayout.Label("(Your game will not be shown in the server list. Friends will need to \"Direct Connect\" to your IP Address)");
                    serverPassword = "";
                }

                GUILayout.EndScrollView();
                GUILayout.Space(150f);
                GUILayout.EndHorizontal();
            }

            GUILayout.FlexibleSpace();

            if(WorldDesc.url != "" && WorldDesc.url != "http://")
            {
                if(GUILayout.Button(
                    ">> Begin Hosting Game! <<",
                    GUILayout.Height(40f)))
                {
                    serverName = LanguageFilter(serverName);
                    if(serverPassword == "1")
                    {
                        serverPassword = "";
                        while(serverPassword.Length < 5)
                        {
                            serverPassword += UnityEngine.Random.Range(0, 9);
                        }
                    }
                    LoadWorld();
                }
                GUILayout.Space(5f);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if(GUILayout.Button(
                    "<< Cancel",
                    GUILayout.Height(25f),
                    GUILayout.Width(150f)))
                {
                    netKillMode = 1;
                    Network.Disconnect();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            else
            {
                if(GUILayout.Button(
                    "<< Cancel Hosting Game",
                    GUILayout.Height(40f)))
                {
                    netKillMode = 1;
                    Network.Disconnect();
                }
                GUILayout.Space(32f);
            }
        }
    }

	public IEnumerator addBot()
    {
        botsInGame++;
        Messaging.broadcast(
            Player.name +
            " added " +
            (botsInGame == 1 ? "a bot" : "bot " + botsInGame));
        materilizationEffect(World.baseTF.transform.position);
        String temp =
            GameData.userName +
            "'s bot" +
            (botsInGame == 1 ? "" : " " + botsInGame);
        yield return new WaitForSeconds(2f);
        GetComponent<NetworkView>().RPC(
            "iV",
            RPCMode.All,
            Network.AllocateViewID(),
            Settings.networkMode,
            GameVehicleID,
            temp,
            1,
            0,
            0,
            0,
            PlayerVeh.zorbBall);
	}

	public IEnumerator axeBot()
    {
        GameObject bot = GameObject.Find(
            Player.name +
            "'s bot" +
            (botsInGame != 1 ? " " + botsInGame : ""));
        if (!bot) yield break;
        Messaging.broadcast(
            Player.name +
            " removed " +
            (botsInGame == 1 ? "the last bot" : ("bot " + botsInGame)));
        botsInGame--;
        materilizationEffect(bot.transform.position);
        bot.GetComponent<Rigidbody>().isKinematic = true;

        Network.Destroy(bot.GetComponent<Rigidbody>().GetComponent<NetworkView>().viewID);

        yield return new WaitForSeconds(0.5f);
        eSI();
        //renderAdjustMax = 0; //Give the autorender settings algorithym a chance to take advantage of new settings...
	}

	public void setVeh(int setVehicleTo)
    {
        Messaging.broadcast(
            Player.name +
            " switched to a " +
            GameVehicles[setVehicleTo].name);
        GameVehicleID = setVehicleTo;
		int isIt = PlayerVeh.isIt;
		int score = PlayerVeh.score;
		int specialInput = (PlayerVeh.specialInput ? 1 : 0);
		string name = Player.name;
        bool zorbBall = PlayerVeh.zorbBall;
		Network.Destroy(Player.GetComponent<Rigidbody>().GetComponent<NetworkView>().viewID);
		GetComponent<NetworkView>().RPC(
            "iV",
            RPCMode.All,
            Network.AllocateViewID(),
            Settings.networkMode,
            GameVehicleID,
            name,
            0,
            isIt,
            score,
            specialInput,
            zorbBall);
	}

	[RPC]
	public void eSI() //Ensure Someone is It
    {
		if (!Player) return;
		// /*UNUSED*/ GameObject[] gos = null;
		// /*UNUSED*/ Vehicle Veh = null;
        foreach (DictionaryEntry plrE in Players)
        {
            if (((Vehicle)plrE.Value).isIt == 1) return;
        }
		Player.GetComponent<NetworkView>().RPC("sQ", RPCMode.All, 3);
	}

	public void materilizationEffect(Vector3 position)
    {
		GetComponent<NetworkView>().RPC("mE", RPCMode.All, position);
	}

	[RPC]
	public void mE(Vector3 position)
    {
		Instantiate(
            WorldEntryEffect,
            position,
            new Quaternion(0f, 0f, 0f, 1f));
	}

	public IEnumerator registerHost()
    {
        if (serverHidden || !Network.isServer) yield break;
        yield return new WaitForSeconds(1f);
        String playerList = "";
        int lagCount = 0;
        float lagVal = 0.0f;
        int botCount = 0;
        foreach (DictionaryEntry plrE in Players)
        {
            if (plrE.Value == null) continue;

            if (((Vehicle)plrE.Value).isBot) botCount++;
            else
            {
                playerList +=
                    (playerList == "" ? "" : ",") +
                    ((Vehicle)plrE.Value).name;
                if (!((Vehicle)plrE.Value).GetComponent<NetworkView>().isMine)
                {
                    VehicleNet vehNet = (VehicleNet)((Vehicle)plrE.Value)
                        .gameObject.GetComponent(typeof(VehicleNet));
                    if (vehNet)
                    {
                        lagCount++;
                        lagVal += vehNet.ping;
                    }
                }
            }
        }
        if (botCount > 0)
        {
            playerList +=
                ",and " +
                botCount +
                " bots";
        }
        String bannedIPs = "";
        if (Settings.bannedIPs != "")
        {
            String[] banned = Settings.bannedIPs.Split("\n"[0]);
            foreach (String dat in banned)
            {
                if (dat == "") continue;
                String[] val = dat.Split(" "[0]);
                bannedIPs += (bannedIPs == "" ? "," : "") + val[0];
            }
            bannedIPs = ";b=" + bannedIPs;
        }
        MasterServer.RegisterHost(
            GameData.gameName,
            serverName,
            "v=" +
                GameData.gameVersion +
                ";w=" +
                WorldDesc.name +
                ";p=" +
                playerList +
                ";u=" +
                WorldDesc.url +
                (lagCount > 0 ?
                    ";s=" + Mathf.RoundToInt((lagVal / lagCount) * 1000f) :
                    "") +
                (serverPassword != "" ? ";l=1" : "") +
                bannedIPs);
	}

	public IEnumerator registerHostSet()
    {
        if (serverHidden) yield break;
        hostRegistered = true;
        while (hostRegistered)
        {
            StartCoroutine_Auto(registerHost());
            yield return new WaitForSeconds(60f);
        }
	}

	public void unregisterHost()
    {
		hostRegistered = false;
		MasterServer.UnregisterHost();
	}

	public void OnDisconnectedFromServer(NetworkDisconnection info)
    {
		if (!Network.isServer && netKillMode != 1)
        {
			GameData.errorMessage = "\n\nDisconnected from Game Server:\n\n";
			if (info == NetworkDisconnection.LostConnection)
            {
				GameData.errorMessage += "Connection lost, please try rejoining the game!";
			}
            else
            {
			    if (netKillMode == 4)
                {
				    GameData.errorMessage += "Timeout due to inactivity.\nPlease try rejoining the game.";
			    }
			    else if (netKillMode == 3)
                {
				    GameData.errorMessage += "Network connection to game server timed out.\nPlease try rejoining the game.";
			    }
			    else if (netKillMode == 2)
                {
				    GameData.errorMessage += "The server host has evicted you from their game.\nYou will probably want to find a new server to play at.";
			    }
			    else
                {
				    GameData.errorMessage += "The person hosting the game you were connected to has stopped playing Mars Explorer.";
			    }
            }
		}
		Application.LoadLevel(1);
	}

	public void OnApplicationQuit()
    {
		Network.Disconnect();
		if (Network.isServer) unregisterHost();
		Application.LoadLevel(1);
	}

	[RPC]
	public IEnumerator iV(
        NetworkViewID viewID,
        int networkMode,
        int vehId,
        string vehName,
        int isBot,
        int isIt,
        int score,
        int specialInput,
        bool zorbBall)
    {
		//while(worldLoaded == false) yield return null;

        Vector3 pos = Vector3.zero;
        if (viewID.isMine)
        {
            //Dragonhere: possibly gove other client instances time to build their vehicles before they are bombarded with NetworkViews that they can't handle.
            yield return new WaitForSeconds(0.25f);

            //Determine Safe Spawn Point
            pos = World.baseTF.position;
            while (Physics.CheckSphere(pos, 3)) pos += Vector3.up;
        }

        //Create Vehicle
        switch (networkMode)
        { 
            case 0:
                objectVehicle.GetComponent<NetworkView>().stateSynchronization = NetworkStateSynchronization.Unreliable;
                break;
            case 1:
                objectVehicle.GetComponent<NetworkView>().stateSynchronization = NetworkStateSynchronization.ReliableDeltaCompressed;
                break;
            default:
                objectVehicle.GetComponent<NetworkView>().stateSynchronization = NetworkStateSynchronization.Off;
                break;
        }
        objectVehicle.GetComponent<NetworkView>().viewID = viewID;
        GameObject plyObj = (GameObject)Instantiate(
            objectVehicle,
            pos,
            Quaternion.identity);
        plyObj.GetComponent<NetworkView>().viewID = viewID;
        GameObject vehObj = (GameObject)Instantiate(
            Game.Controller.GameVehicles[vehId],
            pos,
            Quaternion.identity);
        vehObj.transform.parent = plyObj.transform;
        Vehicle plyVeh = plyObj.GetComponent<Vehicle>();
        plyVeh.vehObj = vehObj;
        if (viewID.isMine && World.baseTF != null)
        {
            //DRAGONHERE - MAJOR UNITY BUG: If we are instantiated at any other than the identity rotation, it totally messes up rotation locking on vehicle joints such as tank tracks
            plyObj.transform.rotation = World.baseTF.rotation;
        }
        else
        {
            plyObj.transform.rotation = Quaternion.identity;
        }

        //Configure Vehicle
        plyObj.name = vehName;
        plyVeh.networkMode = networkMode;
        plyVeh.vehId = vehId;
        plyVeh.isBot = (isBot == 1);
        plyVeh.isIt = isIt;
        plyVeh.score = score;
        plyVeh.specialInput = (specialInput == 1);
        plyVeh.zorbBall = zorbBall;

        if (viewID.isMine && isBot == 0) Player = plyObj;
        if (plyVeh.isIt == 1) QuarryVeh = plyVeh;
	}

	[RPC]
	public void pD(string pName)
    {
		if (Players[pName] == null) return;
        foreach (DictionaryEntry plrE in Players)
        {
            ((Vehicle)plrE.Value).setColor();   //Make sure everyone is colored correctly
        }
		mE(((Vehicle)Players[pName]).gameObject.transform.position);
		if (((Vehicle)Players[pName]).netKillMode == 0)
        {
			msg(pName + " has disconnected", 2);
		}
		if (((Vehicle)Players[pName]).gameObject)
        {
			Destroy(((Vehicle)Players[pName]).gameObject);
		}
		Players.Remove(pName);
	}

	[RPC]
	public void pI(
        NetworkPlayer nPlayer,
        string pName,
        NetworkMessageInfo info)
    {
        /*if (info.sender != host)
        {
            Debug.Log(
                info.sender.ipAddress +
                " just attemped to illegally invite another player");
            return;
        }
        authenticatedPlayers.Add(nPlayer, 1);*/
    }

	[RPC]
	public void cC(
        NetworkPlayer nPlayer,
        string pName,
        int cMode,
        NetworkMessageInfo info)
    {
        //Network.CloseConnection(nPlayer, true);
    }

	[RPC]
	public void cP(NetworkPlayer player, string pass)
    {
		if (
            (Network.isServer && pass == serverPassword) ||
            pass == "pg904gk7")
        {   //They sent the right password
			authenticatedPlayers.Add(player, 1);
		}
	}

	[RPC]
	public void sSS(string str, NetworkMessageInfo info)
    {
        //if (isHost && !info.networkView.isMine) return;
		Settings.serverString = str;
		
        if (!info.networkView.isMine)
        {
			if (str == Settings.serverDefault)
            {
				msg(
                    "(Server Settings Defaulted)",
                    (int)chatOrigins.Server);
			}
			else
            {
				msg(
                    "(Server Settings Updated)",
                    (int)chatOrigins.Server);
			}
		}

		string[] prefs = str.Split(";"[0]);
		string[] val = null;

        foreach (String pref in prefs)
        {
            val = pref.Split(":"[0]);
            if (val[0] == "lasr") Settings.lasersAllowed = (val[1] == "1" ? true : false);
            if (val[0] == "lsrh") Settings.lasersFatal = (val[1] == "1" ? true : false);
            if (val[0] == "lsro") Settings.lasersOptHit = (val[1] == "1" ? true : false);
            else if (val[0] == "mmap") Settings.minimapAllowed = (val[1] == "1" ? true : false);
            else if (val[0] == "camo") Settings.hideNames = (val[1] == "1" ? true : false);
            else if (val[0] == "rorb") Settings.ramoSpheres = float.Parse(val[1]);
            else if (val[0] == "xspd") Settings.zorbSpeed = float.Parse(val[1]);
            else if (val[0] == "xagt") Settings.zorbAgility = float.Parse(val[1]);
            else if (val[0] == "xbnc") Settings.zorbBounce = Mathf.Clamp(float.Parse(val[1]), 0, 1);
            else if (val[0] == "grav") Settings.worldGrav = float.Parse(val[1]) * -1;
            else if (val[0] == "wvis") Settings.worldViewDist = float.Parse(val[1]);
            else if (val[0] == "lfog") Settings.lavaFog = float.Parse(val[1]);
            else if (val[0] == "lalt") Settings.lavaAlt = float.Parse(val[1]);
            else if (val[0] == "lspd") Settings.laserSpeed = float.Parse(val[1]);
            else if (val[0] == "lgvt") Settings.laserGrav = Mathf.Clamp(float.Parse(val[1]), 0, 1);
            else if (val[0] == "lrco") Settings.laserRico = Mathf.Clamp(float.Parse(val[1]), 0, 1);

            else if (val[0] == "botfire") Settings.botsCanFire = (val[1] == "1" ? true : false);
            else if (val[0] == "botdrive") Settings.botsCanDrive = (val[1] == "1" ? true : false);

            else if (val[0] == "bugen") Settings.buggyAllowed = (val[1] == "1" ? true : false);
            else if (val[0] == "bugxphy") Settings.buggyNewPhysics = (val[1] == "1" ? true : false);
            else if (val[0] == "bugflsl") Settings.buggyFlightSlip = (val[1] == "1" ? true : false);
            else if (val[0] == "bugflpw") Settings.buggyFlightLooPower = (val[1] == "1" ? true : false);
            else if (val[0] == "bugawd") Settings.buggyAWD = (val[1] == "1" ? true : false);
            else if (val[0] == "bugspn") Settings.buggySmartSuspension = (val[1] == "1" ? true : false);
            else if (val[0] == "bugfldr") Settings.buggyFlightDrag = Mathf.Clamp(float.Parse(val[1]), 1, 1000);
            else if (val[0] == "bugflag") Settings.buggyFlightAgility = Mathf.Clamp(float.Parse(val[1]), 0.5f, 1.5f);
            else if (val[0] == "bugcg") Settings.buggyCG = Mathf.Clamp(float.Parse(val[1]), -1, 0);
            else if (val[0] == "bugpow") Settings.buggyPower = Mathf.Clamp(float.Parse(val[1]), 0.1f, 3);
            else if (val[0] == "bugspd") Settings.buggySpeed = Mathf.Clamp(float.Parse(val[1]), 1, 1000);
            else if (val[0] == "bugtr") Settings.buggyTr = Mathf.Clamp(float.Parse(val[1]), 0.1f, 3);
            else if (val[0] == "bugsh") Settings.buggySh = Mathf.Clamp(float.Parse(val[1]), 0, 140);
            else if (val[0] == "bugfp") Settings.firepower[0] = Mathf.Clamp(int.Parse(val[1]), 0, 3);
            else if (val[0] == "bugll") Settings.laserLock[0] = Mathf.Clamp(float.Parse(val[1]), 0, 1);

            else if (val[0] == "tnken") Settings.tankAllowed = (val[1] == "1" ? true : false);
            else if (val[0] == "tnkgrp") Settings.tankGrip = Mathf.Clamp(float.Parse(val[1]), 0, 1);
            else if (val[0] == "tnkspd") Settings.tankSpeed = Mathf.Clamp(float.Parse(val[1]), 1, 100);
            else if (val[0] == "tnkpow") Settings.tankPower = Mathf.Clamp(float.Parse(val[1]), 100, 10000);
            else if (val[0] == "tnkcg") Settings.tankCG = Mathf.Clamp(float.Parse(val[1]), -2, 2);
            else if (val[0] == "tnkfp") Settings.firepower[2] = Mathf.Clamp(int.Parse(val[1]), 0, 3);
            else if (val[0] == "tnkll") Settings.laserLock[2] = Mathf.Clamp(float.Parse(val[1]), 0, 1);

            else if (val[0] == "hvren") Settings.hoverAllowed = (val[1] == "1" ? true : false);
            else if (val[0] == "hvrhe") Settings.hoverHeight = Mathf.Clamp(float.Parse(val[1]), 1, 100);
            else if (val[0] == "hvrhv") Settings.hoverHover = Mathf.Clamp(float.Parse(val[1]), 1, 1000);
            else if (val[0] == "hvrrp") Settings.hoverRepel = Mathf.Clamp(float.Parse(val[1]), 0.1f, 10);
            else if (val[0] == "hvrth") Settings.hoverThrust = Mathf.Clamp(float.Parse(val[1]), 1, 1000);
            else if (val[0] == "hvrfp") Settings.firepower[1] = Mathf.Clamp(int.Parse(val[1]), 0, 3);
            else if (val[0] == "hvrll") Settings.laserLock[1] = Mathf.Clamp(float.Parse(val[1]), 0, 1);

            else if (val[0] == "jeten") Settings.jetAllowed = (val[1] == "1" ? true : false);
            else if (val[0] == "jethd") Settings.jetHDrag = Mathf.Clamp(float.Parse(val[1]), 0.0005f, 0.1f);
            else if (val[0] == "jetd") Settings.jetDrag = Mathf.Clamp(float.Parse(val[1]), 0.0005f, 0.1f);
            else if (val[0] == "jets") Settings.jetSteer = Mathf.Clamp(float.Parse(val[1]), 1, 100);
            else if (val[0] == "jetl") Settings.jetLift = Mathf.Clamp(float.Parse(val[1]), 0.01f, 10);
            else if (val[0] == "jetss") Settings.jetStall = Mathf.Clamp(float.Parse(val[1]), 0.1f, 100);
            else if (val[0] == "jetfp") Settings.firepower[3] = Mathf.Clamp(int.Parse(val[1]), 0, 3);
            else if (val[0] == "jetll") Settings.laserLock[3] = Mathf.Clamp(float.Parse(val[1]), 0, 1);

            else if (val[0] == "netm") Settings.networkMode = Mathf.Clamp(int.Parse(val[1]), 0, 2);
            else if (val[0] == "netph") Settings.networkPhysics = Mathf.Clamp(int.Parse(val[1]), 0, 2);
            else if (val[0] == "netin") Settings.networkInterpolation = Mathf.Clamp(float.Parse(val[1]), 0, 0.5f);
        }
        Settings.updatePrefs();
	}

	[RPC]
	public void sH()
    {
		isHost = true;
	}

	public void sSHS()
    {
		GetComponent<NetworkView>().RPC(
            "sSH",
            RPCMode.Server,
            serverName,
            "url=" +
                WorldDesc.url +
                ";nme=" +
                WorldDesc.name,
            Settings.serverWelcome,
            Settings.bannedIPs,
            serverPassword,
            GameData.gameVersion,
            serverHidden);
	}

	[RPC]
	public void sSH(string sname, string sworld, string swelcome, string sblacklist, string spassword, SemVer gVersion, bool shidden, NetworkMessageInfo info)
    {
		serverName = sname;
		Settings.serverWelcome = swelcome;
		Settings.bannedIPs = sblacklist;
		serverPassword = spassword;
		serverHidden = shidden;

		string url = "";
		// /*UNUSED*/ int fmt = 0;
		// /*UNUSED*/ string material = "0";
		string[] wrld = sworld.Split(";"[0]);
        foreach (String dat in wrld)
        {
            if (dat == "") continue;
            String[] vals = dat.Split("="[0]);
            if (vals[0] == "url") url = vals[1];
            //else if(vals[0] == "mat") material = vals[1];
            else if (vals[0] == "nme") WorldDesc.name = vals[1];
        }
		WorldDesc.url = url;
	}

	[RPC]
	public void sSB(string sblacklist)
    {
		Settings.bannedIPs = sblacklist;
	}

	[RPC]
	public void dN(int rsn)
    {
		netKillMode = rsn;
	}

	[RPC]
	public void msg(string str, int origin)
    {
		ChatEntry entry = new ChatEntry();
		entry.text = str;
		entry.origin = (chatOrigins)origin;
		Messaging.entries.Add(entry);
		if (Messaging.entries.Count > 50) Messaging.entries.RemoveAt(0);
		Messaging.scrollPosition.y = 1000000f;
	}

	[RPC]
	public void lW(string str)
    {
        //url=http://location;fmt=1;mat=material;
		string url = "";
		// /*UNUSED*/ int fmt = 0;
        // /*UNUSED*/ string material = "0";
		string[] wrld = str.Split(";"[0]);
        foreach (String dat in wrld)
        {
            if (dat == "") continue;
            String[] vals = dat.Split("="[0]);
            if (vals[0] == "url") url = vals[1];
            //else if(vals[0] == "fmt") fmt = int.Parse(vals[1]);
            //else if(vals[0] == "mat") material = vals[1];
        }
		WorldDesc.url = url;
        //WorldDesc.format = fmt;
		LoadWorld();
	}

	[RPC]
	public void lMI(NetworkPlayer p, string n)
    {
		if (!isHost) return; //We don't need to know about this...
		for (int i = 0; i < unauthPlayers.Count; i++)
        {
            if (
                unauthPlayers[i].p.externalIP == p.externalIP &&
                unauthPlayers[i].n == n)
            {
                unauthPlayers[i].t = Time.time;
                return;
            }
		}
		unauthPlayers.Add(new unauthPlayer(p, n, Time.time));
	}

	public void LoadWorld()
    {
		whirldIn.url = WorldDesc.url;
		whirldIn.Load();
	}

	public static string LanguageFilter(string str)
    {
		/*string patternMild = " crap | prawn |d4mn| damn | turd ";
		str = Regex.Replace(str, patternMild, ".", RegexOptions.IgnoreCase);
		string pattern = "anus|ash0le|ash0les|asholes| ass |Ass Monkey|Assface|assh0le|assh0lez|bastard|bastards|bastardz|basterd|suka|asshole|assholes|assholz|asswipe|azzhole|bassterds|basterdz|Biatch|bitch|bitches|Blow Job|blowjob|in bed|butthole|buttwipe|c0ck|c0cks|c0k|Clit|cnts|cntz|cockhead| cock |cock-head|CockSucker|cock-sucker| cum |cunt|cunts|cuntz|dick|dild0|dild0s|dildo|dildos|dilld0|dilld0s|dominatricks|dominatrics|dominatrix|f.u.c.k|f u c k|f u c k e r|fag|fag1t|faget|fagg1t|faggit|faggot|fagit|fags|fagz|faig|faigs|fuck|fucker|fuckin|mother fucker|fucking|fucks|Fudge Packer|fuk|Fukah|Fuken|fuker|Fukin|Fukk|Fukkah|Fukken|Fukker|Fukkin|gay|gayboy|gaygirl|gays|gayz|God-dam|God dam|h00r|h0ar|h0re|jackoff|jerk-off|jizz|kunt|kunts|kuntz|Lesbian|Lezzian|Lipshits|Lipshitz|masochist|masokist|massterbait|masstrbait|masstrbate|masterbaiter|masterbate|masterbates|Motha Fucker|Motha Fuker|Motha Fukkah|Motha Fukker|Mother Fucker|Mother Fukah|Mother Fuker|Mother Fukkah|Mother Fukker|mother-fucker|Mutha Fucker|Mutha Fukah|Mutha Fuker|Mutha Fukkah|Mutha Fukker|orafis|orgasim|orgasm|orgasum|oriface|orifice|orifiss|packi|packie|packy|paki|pakie|peeenus|peeenusss|peenus|peinus|pen1s|penas|penis|penis-breath|penus|penuus|Phuc|Phuck|Phuk|Phuker|Phukker|polac|polack|polak|Poonani|pr1c|pr1ck|pr1k|pusse|pussee|pussy|puuke|puuker|queer|queers|queerz|qweers|qweerz|qweir|recktum|rectum|retard|sadist|scank|schlong|screwing| sex |sh1t|sh1ter|sh1ts|sh1tter|sh1tz|shit|shits|shitter|Shitty|Shity|shitz|Shyt|Shyte|Shytty|Shyty|skanck|skank|skankee| sob |skankey|skanks|Skanky|slut|sluts|Slutty|slutz|son-of-a-bitch|va1jina|vag1na|vagiina|vagina|vaj1na|vajina|vullva|vulva|xxx|b!+ch|bitch|blowjob|clit|arschloch|fuck|shit|asshole|b!tch|b17ch|b1tch|bastard|bi+ch|boiolas|buceta|c0ck|cawk|chink|clits|cunt|dildo|dirsa|ejakulate|fatass|fcuk|fuk|fux0r|l3itch|lesbian|masturbate|masterbat*|motherfucker|s.o.b.|mofo|nigga|nigger|n1gr|nigur|niiger|niigr|nutsack|phuck|blue balls|blue_balls|blueballs|pussy|scrotum|shemale|sh!t|slut|smut|teets|tits|boobs|b00bs|testical|testicle|titt|jackoff|whoar|whore|fuck|shit|arse|bi7ch|bitch|bollock|breasts|cunt|dick|fag |feces|fuk|futkretzn|gay|jizz|masturbat*|piss|poop|porn|p0rn|pr0n|shiz|splooge|b00b|testicle|titt|wank";
		return Regex.Replace(str, pattern, "#", RegexOptions.IgnoreCase);*/

        // the kids that played this game back then are no longer kids. little need to babysit them anymore
        return str;
	}
}
