using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class Settings : MonoBehaviour
{
	[HideInInspector]
	public int showBox = 0;
	[HideInInspector]
	public bool simplified = true;
	public AudioSource gameMusic;
	public AudioClip[] musicTracks;
	public PhysicMaterial zorbPhysics;
	[HideInInspector]
	public int renderLevel = 0;
	[HideInInspector]
	public int renderAdjustMax = 0;
	[HideInInspector]
	public float renderViewCap = 1000f;
	public bool enteredfullscreen = false;
	[HideInInspector]
	public bool renderAutoAdjust = false;
	[HideInInspector]
	public int renderAdjustTime = 8;
	[HideInInspector]
	public bool showHints = true;
	[HideInInspector]
	public float serverUpdateTime = 0.00f;
	[HideInInspector]
	public float colorUpdateTime = 0.00f;
	public bool colorCustom = false;
	public bool disableHints = false;
	public Color fogColor = Color.clear;

	// /*UNUSED, DEPRECATED*/ private bool useFog = true;
    // /*UNUSED, DEPRECATED*/ private bool detailedObjects = true;
    // /*UNUSED, DEPRECATED*/ private bool useParticles = true;
    // /*UNUSED, DEPRECATED*/ private bool useTrails = true;
	public bool useMinimap = true;
    // /*UNUSED, DEPRECATED*/ private bool foliage = true;
    // /*UNUSED, DEPRECATED*/ private bool terrainQuality = true;
    // /*UNUSED, DEPRECATED*/ private bool terrainDetail = true;
    // /*UNUSED, DEPRECATED*/ private bool terrainLighting = true;
	public int useMusic = 1;
	public bool useSfx = true;
	public int useHypersound = 0;
    // /*UNUSED, DEPRECATED*/ private bool syncFps = false;
    // /*UNUSED, DEPRECATED*/ private bool playerOnlyLight = false;
    // /*UNUSED, DEPRECATED*/ private Vector2 scrollPosition;
	[HideInInspector]
	public SSAOEffect camSSAO;
	public ContrastStretchEffect camContrast;

	[HideInInspector]
	public string txt;
	[HideInInspector]
	public string str;
	[HideInInspector]
	public string serverString;
	[HideInInspector]
	public string bannedIPs;
	public static string serverDefault;

	public string serverWelcome = "";

	public int camMode = 0;
	public int camChase = 0;
	public float camDist = 0;
	public bool flightCam = false;
	public bool gyroCam = false;
    public bool quarryCam = false;

	public float worldGrav = -9.81f;
	public float worldFog = 0.001f;
	public float worldViewDist = 5000f;
	public float lavaFog = 0.005f;
	public float lavaAlt = -300.00f;
	public float laserSpeed = 180.00f;
	public float laserGrav = 0f;
	public float laserRico = 0f;
	public bool laserLocking;

	public float resetTime;
	public bool lasersAllowed = true;
	public bool lasersFatal = false;
	public bool lasersOptHit = false;
	public float ramoSpheres = 0f;
	public float zorbSpeed = 7f;
	public float zorbAgility = 0f;
	public float zorbBounce = 0.5f;
	public bool minimapAllowed = true;
	public bool hideNames = false;
	public bool botsCanFire = true;
	public bool botsCanDrive = true;

	public int[] firepower;
	public float[] laserLock;

	public bool buggyAllowed = true;
	public bool buggyFlightSlip = false;
	public bool buggySmartSuspension = true;
	public bool buggyNewPhysics = false;
	public bool buggyAWD = true;
	public float buggyCG = -0.40f;
	public float buggyPower = 1.00f;
	public float buggySpeed = 30.00f;
	public bool buggyFlightLooPower = false;
	public float buggyFlightDrag = 300.00f;
	public float buggyFlightAgility = 1.00f;
	public float buggyTr = 1.00f;
	public float buggySh = 70.00f;
	public float buggySl = 50.00f;

	public bool tankAllowed = true;
	public float tankPower = 2000.00f;
	public float tankSpeed = 25.00f;
	public float tankGrip = 0.1f;
	public float tankCG = -0.20f;

	public bool hoverAllowed = true;
	public float hoverHeight = 15.00f;
	public float hoverHover = 100.00f;
	public float hoverRepel = 2.50f;
	public float hoverThrust = 220.0f;

	public bool jetAllowed = true;
	public float jetHDrag = 0.01f;
	public float jetDrag = 0.001f;
	public float jetSteer = 20;
	public float jetLift = 0.5f;
	public float jetStall = 20;

	public int networkMode = 0;
	public int networkPhysics = 0;
	public float networkInterpolation = 0.0f;
	public bool isAdmin = false;

	public static bool isDesktopPlatform()
	{
		return Application.platform == RuntimePlatform.OSXPlayer ||
			Application.platform == RuntimePlatform.WindowsPlayer ||
			Application.platform == RuntimePlatform.LinuxPlayer;
	}

	public void Start()
	{
		simplified = true;
		getPrefs();
		updatePrefs(); //Init music
		if (GameData.userName == "Aubrey (admin)")
		{
			isAdmin = true;
		}
	}

	public void getPrefs()
	{
		renderLevel = PlayerPrefs.GetInt("renderLevel", 4);
		renderViewCap = PlayerPrefs.GetFloat("viewCap", 1000f);
		Application.targetFrameRate = (int)PlayerPrefs.GetFloat("targetFrameRate", 100f);
		renderAutoAdjust = false;
		showHints = PlayerPrefs.GetInt("showHints", 1) != 0;
		useMusic = PlayerPrefs.GetInt("useMusic", 1);
		useSfx = PlayerPrefs.GetInt("useSfx", 1) != 0;
		useHypersound = PlayerPrefs.GetInt("useHypersound", 0);
		useMinimap = PlayerPrefs.GetInt("useMinimap", 1) != 0;
		camMode = PlayerPrefs.GetInt("cam", 1);
		camChase = PlayerPrefs.GetInt("camChase", 1);
		camDist = PlayerPrefs.GetFloat("camDist", 0.01f);
		flightCam = PlayerPrefs.GetInt("flightCam", 0) != 0;
		gyroCam = PlayerPrefs.GetInt("gyroCam", 0) != 0;
		quarryCam = PlayerPrefs.GetInt("quarryCam", 0) != 0;
	}

	public void showDialogGame()
	{
		GUILayout.Label("Resolution:");
		if (GUILayout.Button((!Screen.fullScreen ?
            "Enter" :
            "Exit") + " Fullscreen (0)"))
		{
			toggleFullscreen();
		}

        if (
            Screen.fullScreen ||
            Settings.isDesktopPlatform())
        {
            GUILayout.BeginHorizontal();
            if (
                (Screen.resolutions[0].width < Screen.width ||
                    Screen.resolutions[0].height < Screen.height) &&
                GUILayout.Button("<<", GUILayout.Width(28)))
            {
                for (int i = Screen.resolutions.Length - 1; i >= 0; i--)
                {
                    if (!((Screen.resolutions[i].width <= Screen.width &&
                            Screen.resolutions[i].height < Screen.height) ||
                        (Screen.resolutions[i].width < Screen.width &&
                            Screen.resolutions[i].height <= Screen.height)))
                    {
                        continue;
                    }
                    Screen.SetResolution(
                        Screen.resolutions[i].width,
                        Screen.resolutions[i].height,
                        Screen.fullScreen);
                    break;
                }
            }
            GUILayout.Label(Screen.width + "X" + Screen.height);
            if (
                (Screen.resolutions[Screen.resolutions.Length - 1].width > Screen.width ||
                Screen.resolutions[Screen.resolutions.Length - 1].height > Screen.height) &&
                GUILayout.Button(">>", GUILayout.Width(28)))
            {
                foreach (Resolution res in Screen.resolutions)
                {
                    if (!((res.width >= Screen.width &&
                            res.height > Screen.height) ||
                        (res.width > Screen.width &&
                            res.height >= Screen.height))) continue;
                    Screen.SetResolution(
                        res.width,
                        res.height,
                        Screen.fullScreen);
                    break;
                }
            }
            GUILayout.EndHorizontal();
        }
		
		GUILayout.FlexibleSpace();
		GUILayout.Space(20f);
		GUILayout.Label("Rendering Quality:");

		GUILayout.BeginHorizontal();
		if (renderLevel == 1)
		{
			GUILayout.Label("Fastest");
		}
		else if (renderLevel == 2)
		{
			GUILayout.Label("Fast");
		}
		else if (renderLevel == 3)
		{
			GUILayout.Label("Simple");
		}
		else if (renderLevel == 4)
		{
			GUILayout.Label("Good");
		}
		else if (renderLevel == 5)
		{
			GUILayout.Label("Beautiful");
		}
		else if (renderLevel == 6)
		{
			GUILayout.Label("Fantastic");
		}

		GUILayout.Label("(" + Game.Controller.fps.ToString("f0") + " FPS)");
		
        GUILayout.EndHorizontal();
		
        GUILayout.BeginHorizontal();
		if (renderLevel > 1)
		{
            if (GUILayout.Button("<<"))
            {
			    renderLevel--;
			    PlayerPrefs.SetInt("renderLevel", renderLevel);
			    updatePrefs();
            }
		}

		if (renderLevel > 1 && renderLevel < 6)
		{
			GUILayout.Space(5f);
		}

		if (renderLevel < 6)
		{
            if (GUILayout.Button(">>"))
            {
                renderLevel++;
                PlayerPrefs.SetInt("renderLevel", renderLevel);
                updatePrefs();
            }
		}
		GUILayout.EndHorizontal();

		GUILayout.Space(10f);
        GUILayout.Label(
            "Visibility Cap:   (" +
            (renderViewCap == 1000f ?
                "MAX" :
                Mathf.Floor(renderViewCap).ToString()) +
            ")");
		float cg = GUILayout.HorizontalSlider(
            renderViewCap,
            200f,
            1000f);
		if (renderViewCap != cg)
		{
			renderViewCap = cg;
			PlayerPrefs.SetFloat("viewCap", cg);
			updatePrefs();
		}

		GUILayout.Space(5f);
        GUILayout.Label(
            "FPS Cap:   (" +
            (Application.targetFrameRate == -1 ?
                "MAX" :
                Application.targetFrameRate.ToString()) +
            ")");
		
        if (Application.targetFrameRate == -1)
		{
			Application.targetFrameRate = 100;
		}
		cg = GUILayout.HorizontalSlider(
            Application.targetFrameRate,
            10f,
            100f);
		if ((float)Application.targetFrameRate != cg)
		{
			Application.targetFrameRate = (int)cg;
			PlayerPrefs.SetFloat("targetFrameRate", cg);
		}
		if (
            Application.targetFrameRate == 0 ||
            Application.targetFrameRate == 100)
		{
			Application.targetFrameRate = -1;
		}

		GUILayout.FlexibleSpace();
		GUILayout.Space(20f);
		GUILayout.Label("Interface:");

		if (minimapAllowed && GUILayout.Toggle(useMinimap, "Enable Minimap") != useMinimap)
		{
			useMinimap = !useMinimap;
			PlayerPrefs.SetInt("useMinimap", useMinimap ? 1 : 0);
			updatePrefs();
		}

		if (GUILayout.Toggle(showHints, "Enable Settings Advisor") != showHints)
		{
			showHints = !showHints;
			PlayerPrefs.SetInt("showHints", showHints ? 1 : 0);
			updatePrefs();
		}

		GUILayout.FlexibleSpace();
		GUILayout.Space(20f);
		GUILayout.Label("Audio:");

		if (GUILayout.Toggle(useSfx, "Sound Effects Enabled") != useSfx)
		{
			useSfx = !useSfx;
			PlayerPrefs.SetInt("useSfx", useSfx ? 1 : 0);
		}

		if (GUILayout.Toggle(useMusic == 0, "No Music") != (useMusic == 0))
		{
			useMusic = 0;
			PlayerPrefs.SetInt("useMusic", 0);
			updatePrefs();
		}

		if (GUILayout.Toggle(useMusic == 1, "Classic") != (useMusic == 1))
		{
			useMusic = 1;
			PlayerPrefs.SetInt("useMusic", 1);
			updatePrefs();
		}
		if (GUILayout.Toggle(useMusic == 2, "Ambient") != (useMusic == 2))
		{
			useMusic = 2;
			PlayerPrefs.SetInt("useMusic", 2);
			updatePrefs();
		}
		if (GUILayout.Toggle(useMusic == 3, "Carefree") != (useMusic == 3))
		{
			useMusic = 3;
			PlayerPrefs.SetInt("useMusic", 3);
			updatePrefs();
		}

		if (GUILayout.Toggle(useHypersound == 1, "HyperSound") != (useHypersound == 1))
		{
			useHypersound = ((useHypersound != 1) ? 1 : 0);
			PlayerPrefs.SetInt("useHypersound", (useHypersound != 0) ? 1 : 0);
			updatePrefs();
		}
	}

	public void showDialogPlayer()
	{
		GUILayout.Space(20f);

		if (resetTime > 0f)
		{
			GUILayout.Label(
                "Reset In " +
                (10f - (Time.time - resetTime)));
		}
		else if (
            resetTime > -1f &&
            GUILayout.Button("Reset My Position (/r)"))
		{
			resetTime = Time.time;
			Game.Player.GetComponent<Rigidbody>().isKinematic = true;
			Game.Messaging.broadcast(
                Game.Player.name +
                " Resetting in 10 seconds...");
		}

		if (zorbSpeed != 0f)
		{
			GUILayout.Space(10f);
			if (GUILayout.Button(
                (Game.PlayerVeh.zorbBall ?
                    "Deactivate" :
                    "Activate") +
                " My Xorb (/x)"))
			{
				Game.Player.GetComponent<NetworkView>().RPC(
                    "sZ",
                    RPCMode.All,
                    !Game.PlayerVeh.zorbBall);
			}
		}

		GUILayout.Space(20f);

		GUILayout.Label("Camera Mode:");

        //Ride (1, alt)
        //Chase (2)
        //Soar (5)
        //Spectate
        //Wander

        //Distance (3-4)
        //GyRide (6)
        //HyperCam

		if (GUILayout.Toggle(camMode == 0, "Ride (1, alt)") != (camMode == 0))
		{
			camMode = 0;
			PlayerPrefs.SetInt("cam", 0);
		}
		if (GUILayout.Toggle(camMode == 1, "Chase (2)") != (camMode == 1))
		{
			camMode = 1;
			PlayerPrefs.SetInt("cam", 1);
		}
		if (GUILayout.Toggle(camMode == 2, "Soar(5)") != (camMode == 2))
		{
			camMode = 2;
			PlayerPrefs.SetInt("cam", 2);
		}
		if (GUILayout.Toggle(camMode == 3, "Spectate(6)") != (camMode == 3))
		{
			camMode = 3;
			PlayerPrefs.SetInt("cam", 3);
		}
		if (GUILayout.Toggle(camMode == 4, "Roam") != (camMode == 4))
		{
			camMode = 4;
			PlayerPrefs.SetInt("cam", 4);
		}

		if (GUILayout.Toggle(gyroCam, "GyRide Enabled (7)") != gyroCam)
		{
			gyroCam = !gyroCam;
			PlayerPrefs.SetInt("gyroCam", gyroCam ? 1 : 0);
		}

		if (GUILayout.Toggle(flightCam, "HyperCam Enabled") != flightCam)
		{
			flightCam = !flightCam;
			PlayerPrefs.SetInt("flightCam", flightCam ? 1 : 0);
		}

        if (GUILayout.Toggle(quarryCam, "QuarryCam Enabled") != quarryCam)
        {
            quarryCam = !quarryCam;
            PlayerPrefs.SetInt("quarryCam", quarryCam ? 1 : 0);
        }

		float cg;
		if (camMode == 0)
		{
			GUILayout.Space(10f);
			GUILayout.Label("(Press (2) or (esc) keys to unlock your cursor)");
			GUILayout.Space(10f);
		}

		else if (camMode == 1)
		{
			GUILayout.Space(5f);
			GUILayout.Label("Chase Strategy:");
			if (GUILayout.Toggle(camChase == 0, "Smooth") != (camChase == 0))
			{
				camChase = 0;
				PlayerPrefs.SetInt("camChase", 0);
			}
			if (GUILayout.Toggle(camChase == 1, "Agile") != (camChase == 1))
			{
				camChase = 1;
				PlayerPrefs.SetInt("camChase", 1);
			}
			if (GUILayout.Toggle(camChase == 2, "Arcade") != (camChase == 2))
			{
				camChase = 2;
				PlayerPrefs.SetInt("camChase", 2);
			}
			GUILayout.Space(5f);
			GUILayout.Label("Chase Distance: (3-4)");
			cg = GUILayout.HorizontalSlider(camDist, 0f, 20f);
			if (camDist != cg)
			{
				camDist = cg;
				PlayerPrefs.SetFloat("camDist", cg);
			}
		}

		else if (camMode == 3 || camMode == 4)
		{
			GUILayout.Space(10f);
			GUILayout.Label("(Move camera with UIOJKL keys)");
			GUILayout.Space(10f);
		}

		GUILayout.FlexibleSpace();

		GUILayout.FlexibleSpace();
		GUILayout.Space(20f);
		GUILayout.Label("Vehicle Color:");
		if (Game.PlayerVeh.isIt != 0 && Game.Players.Count > 1)
		{
			GUILayout.Label("(You are quarry, and therefore green)");
		}

		if (GUILayout.Button((!colorCustom) ?
            "Random Coloring" :
            "Randomize Coloring"))
		{
			StartCoroutine(ramdomizeVehicleColor());
		}

		GUILayout.BeginHorizontal();
		cg = GUILayout.HorizontalSlider(
            Game.PlayerVeh.vehicleColor.r,
            0f,
            1f);
		if (Game.PlayerVeh.vehicleColor.r != cg)
		{
			Game.PlayerVeh.vehicleColor.r = cg;
			updateVehicleAccent();
			updateVehicleColor();
		}
		GUILayout.Label("Red", GUILayout.Width(65f));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		cg = GUILayout.HorizontalSlider(
            Mathf.Min(
                Mathf.Lerp(
                    0.1f,
                    0.8f,
                    (Game.PlayerVeh.vehicleColor.r +
                        Game.PlayerVeh.vehicleColor.b) / 2f),
                Game.PlayerVeh.vehicleColor.g),
            0f,
            Mathf.Lerp(
                0.1f,
                0.8f,
                (Game.PlayerVeh.vehicleColor.r +
                    Game.PlayerVeh.vehicleColor.b) / 2f));
		if (Game.PlayerVeh.vehicleColor.g != cg)
		{
			Game.PlayerVeh.vehicleColor.g = cg;
			updateVehicleAccent();
			updateVehicleColor();
		}
		GUILayout.Label("Green", GUILayout.Width(65f));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		cg = GUILayout.HorizontalSlider(
            Game.PlayerVeh.vehicleColor.b,
            0f,
            1f);
		if (Game.PlayerVeh.vehicleColor.b != cg)
		{
			Game.PlayerVeh.vehicleColor.b = cg;
			updateVehicleAccent();
			updateVehicleColor();
		}
		GUILayout.Label("Blue", GUILayout.Width(65f));
		GUILayout.EndHorizontal();

		GUILayout.Space(5f);
		GUILayout.Label("Accent Color:");

		GUILayout.BeginHorizontal();
		cg = GUILayout.HorizontalSlider(
            Game.PlayerVeh.vehicleAccent.r,
            0f,
            1f);
		if (Game.PlayerVeh.vehicleAccent.r != cg)
		{
			Game.PlayerVeh.vehicleAccent.r = cg;
			updateVehicleColor();
		}
		GUILayout.Label("Red", GUILayout.Width(65f));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		cg = GUILayout.HorizontalSlider(
            Game.PlayerVeh.vehicleAccent.g,
            0f,
            1f);
		if (Game.PlayerVeh.vehicleAccent.g != cg)
		{
			Game.PlayerVeh.vehicleAccent.g = cg;
			updateVehicleColor();
		}
		GUILayout.Label("Green", GUILayout.Width(65f));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		cg = GUILayout.HorizontalSlider(
            Game.PlayerVeh.vehicleAccent.b,
            0f,
            1f);
		if (Game.PlayerVeh.vehicleAccent.b != cg)
		{
			Game.PlayerVeh.vehicleAccent.b = cg;
			updateVehicleColor();
		}
		GUILayout.Label("Blue", GUILayout.Width(65f));
		GUILayout.EndHorizontal();

		GUILayout.FlexibleSpace();
	}

	public void showDialogPlayers()
	{
		GUILayout.FlexibleSpace();

		if (
            Game.Controller.isHost &&
            Game.Controller.unauthPlayers.Count > 0)
		{
			GUILayout.Label("Joining Players:");

			for (
                int i = 0;
                i < Game.Controller.unauthPlayers.Count;
                i++)
			{
				GUILayout.Space(10f);
                GUILayout.Label(Game.Controller.unauthPlayers[i].n);
				GUILayout.TextArea(Game.Controller.unauthPlayers[i].p.externalIP);
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Evict"))
				{
                    GetComponent<NetworkView>().RPC(
                        "dN",
                        Game.Controller.unauthPlayers[i].p,
                        2);
                    if (Network.isServer)
                    {
                        Network.CloseConnection(
                            Game.Controller.unauthPlayers[i].p,
                            true);
                    }
                    else
                    {
                        GetComponent<NetworkView>().RPC(
                            "cC",
                            RPCMode.Server,
                            Game.Controller.unauthPlayers[i].p,
                            Game.Controller.unauthPlayers[i].n,
                            0);
                    }
				}
                else if (
                    GUILayout.Button("Invite") &&
                    !(bool)Game.Controller.authenticatedPlayers[Game.Controller.unauthPlayers[i].p])
                {
                    if (Network.isServer)
                    {
                        Game.Controller.authenticatedPlayers.Add(
                            Game.Controller.unauthPlayers[i].p,
                            1);
                    }
                    else
                    {
                        GetComponent<NetworkView>().RPC(
                            "pI",
                            RPCMode.Server,
                            Game.Controller.unauthPlayers[i].p,
                            Game.Controller.unauthPlayers[i].n);
                    }
                }
				GUILayout.EndHorizontal();
			}

			GUILayout.FlexibleSpace();

			GUILayout.Space(20f);
			GUILayout.Label("Active Players:");
		}

		// /*UNUSED*/ GameObject[] gos;
		Vehicle veh;
		VehicleNet vehNet;
        foreach (DictionaryEntry plrE in Game.Players)
        {
            veh = (Vehicle)plrE.Value;
            GameObject go = veh.gameObject;
            vehNet = (VehicleNet)go.GetComponentInChildren(typeof(VehicleNet));
            GUILayout.Space(10f);
            GUILayout.Label(plrE.Key + (veh.isPlayer ? " (Me)" : ""));
            GUILayout.TextArea(
                (go.GetComponent<NetworkView>().isMine ?
                    "" :
                    (vehNet ?
                        Mathf.RoundToInt(vehNet.ping * 1000) +
                            " png - " +
                            Mathf.RoundToInt(vehNet.jitter * 1000) +
                            " jtr" +
                            veh.netCode +
                            "\n" :
                        "")) +
                (vehNet && veh.networkMode == 2 ?
                    Mathf.RoundToInt(vehNet.calcPing) +
                        " CalcPng - " +
                        Mathf.RoundToInt(vehNet.rpcPing) +
                        " TmstmpOfst\n" :
                    "") +
                (Network.isServer ?
                    go.GetComponent<NetworkView>().owner.externalIP +
                        " " +
                        go.GetComponent<NetworkView>().owner.ipAddress :
                    "") +
                "\n" +
                    go.GetComponent<NetworkView>().viewID.ToString() +
                    " " +
                    veh.networkMode /*+ "/" + go.networkView.owner.ToString()*/,
                GUILayout.Height(30));
            GUILayout.BeginHorizontal();
            if (
                (Game.Controller.isHost || isAdmin) &&
                !veh.isBot &&
                !go.GetComponent<NetworkView>().isMine &&
                GUILayout.Button("Evict"))
            {
                Game.Messaging.broadcast(
                    go.name +
                    " was evicted by "
                    + Game.Player.name);
                if (Network.isServer)
                {
                    GetComponent<NetworkView>().RPC(
                        "dN",
                        ((Vehicle)plrE.Value).GetComponent<NetworkView>().owner,
                        2);
                    ((Vehicle)plrE.Value).GetComponent<NetworkView>().RPC(
                        "dN",
                        RPCMode.All,
                        2);
                    Network.CloseConnection(
                        ((Vehicle)plrE.Value).GetComponent<NetworkView>().owner,
                        true);
                }
                else
                {
                    GetComponent<NetworkView>().RPC(
                        "cC",
                        RPCMode.Server,
                        ((Vehicle)plrE.Value).GetComponent<NetworkView>().owner,
                        plrE.Key,
                        1);
                }
            }
            else if (
                (Game.Controller.isHost || isAdmin) &&
                !veh.isBot &&
                !go.GetComponent<NetworkView>().isMine &&
                GUILayout.Button("Ban"))
            {
                Game.Messaging.broadcast(
                    go.name +
                    " was banned by " +
                    Game.Player.name);
                if (Network.isServer)
                {
                    bannedIPs += (bannedIPs != "" ? "\n" : "") +
                        ((Vehicle)plrE.Value).GetComponent<NetworkView>().owner.ipAddress +
                        " " +
                        go.name;
                    GetComponent<NetworkView>().RPC(
                        "dN",
                        ((Vehicle)plrE.Value).GetComponent<NetworkView>().owner,
                        2);
                    ((Vehicle)plrE.Value).GetComponent<NetworkView>().RPC(
                        "dN",
                        RPCMode.All,
                        2);
                    Network.CloseConnection(
                        ((Vehicle)plrE.Value).GetComponent<NetworkView>().owner,
                        true);
                    Game.Controller.registerHost();
                }
                else
                {
                    GetComponent<NetworkView>().RPC(
                        "cC",
                        RPCMode.Server,
                        ((Vehicle)plrE.Value).GetComponent<NetworkView>().owner,
                        plrE.Key,
                        2);
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }

		if (Game.Controller.isHost)
		{
			GUILayout.Space(20f);
			GUILayout.Label("Banned Players:");
			if (bannedIPs != "" && GUILayout.Button("Unban All"))
			{
				bannedIPs = "";
				Game.Controller.StartCoroutine(Game.Controller.registerHost());
				updateServerPrefs();
			}
			string cm = GUILayout.TextField(bannedIPs);
			if (cm != bannedIPs)
			{
				bannedIPs = cm;
				updateServerPrefs();
			}
		}
	}

	public void showDialogServer()
	{
	    if (Game.Controller.isHost || isAdmin)
		{
			if (GUILayout.Button(">> Default All <<"))
			{
				Game.Controller.GetComponent<NetworkView>().RPC(
                    "sSS",
                    RPCMode.All,
                    serverDefault);
			}

			GUILayout.Space(20f);
			GUILayout.Label("Server Name:");
			string cm = GUILayout.TextField(Game.Controller.serverName, 45);
			if (cm != Game.Controller.serverName)
			{
				Game.Controller.serverName = cm;
				updateServerPrefs();
			}

			GUILayout.Space(20f);
			GUILayout.Label("Server Features:");

			Game.Controller.serverHidden = GUILayout.Toggle(
                Game.Controller.serverHidden,
                "Hide Server from List");
			if (
                Game.Controller.serverHidden &&
                Game.Controller.hostRegistered)
			{
				Game.Controller.unregisterHost();
				updateServerPrefs();
			}
			else if (
                !Game.Controller.serverHidden &&
                !Game.Controller.hostRegistered)
			{
				Game.Controller.StartCoroutine(Game.Controller.registerHostSet());
				updateServerPrefs();
			}

			GUILayout.BeginHorizontal();
			GUILayout.Label("Password?");
			cm = GUILayout.TextField(Game.Controller.serverPassword);
			if (cm != Game.Controller.serverPassword)
			{
				Game.Controller.serverPassword = cm;
				updateServerPrefs();
			}
			GUILayout.EndHorizontal();

			GUILayout.Label("Welcome Message:");
			cm = GUILayout.TextField(serverWelcome);
			if (cm != serverWelcome)
			{
				serverWelcome = cm;
				updateServerPrefs();
			}

			GUILayout.Space(20f);
			GUILayout.Label("Game Features:");

			if (GUILayout.Toggle(
                minimapAllowed,
                "Minimap enabled") != minimapAllowed)
			{
				minimapAllowed = !minimapAllowed;
				updateServerPrefs();
			}

			if (GUILayout.Toggle(
                hideNames,
                "Camouflage Badges") != hideNames)
			{
				hideNames = !hideNames;
				updateServerPrefs();
			}

			if (GUILayout.Toggle(
                ramoSpheres != 0f,
                "RORBs Enabled") != (ramoSpheres != 0f))
			{
				ramoSpheres = ((ramoSpheres != 0f) ? 0f : 0.5f);
				if (ramoSpheres != 0f)
				{
					zorbSpeed = 7f;
				}
				updateServerPrefs();
			}

			float cg;
			if (ramoSpheres != 0f)
			{
				GUILayout.BeginHorizontal();
				cg = GUILayout.HorizontalSlider(
                    ramoSpheres,
                    0.001f,
                    1f);
				if (ramoSpheres != cg)
				{
					ramoSpheres = cg;
					updateServerPrefs();
				}
				GUILayout.Label("Size", GUILayout.Width(65f));
				GUILayout.EndHorizontal();

				if (GUILayout.Toggle(
                    zorbSpeed != 0f,
                    "XORBs Available") != (zorbSpeed != 0f))
				{
					zorbSpeed = ((zorbSpeed != 0f) ? 0 : 7);
					updateServerPrefs();
				}

				if (zorbSpeed != 0f)
				{
					GUILayout.BeginHorizontal();
					cg = GUILayout.HorizontalSlider(zorbSpeed, 0.001f, 14f);
					if (zorbSpeed != cg)
					{
						zorbSpeed = cg;
						updateServerPrefs();
					}
					GUILayout.Label("X Speed", GUILayout.Width(65f));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					cg = GUILayout.HorizontalSlider(zorbAgility, -7f, 7f);
					if (zorbAgility != cg)
					{
						zorbAgility = cg;
						updateServerPrefs();
					}
					GUILayout.Label("X Agility", GUILayout.Width(65f));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					cg = GUILayout.HorizontalSlider(zorbBounce, 0f, 1f);
					if (zorbBounce != cg)
					{
						zorbBounce = cg;
						updateServerPrefs();
					}
					GUILayout.Label("X Bounce", GUILayout.Width(65f));
					GUILayout.EndHorizontal();

				}

				GUILayout.Space(10f);
			}

			if (GUILayout.Toggle(
                lasersAllowed,
                "Lasers enabled") != lasersAllowed)
			{
				lasersAllowed = !lasersAllowed;
				updateServerPrefs();
			}

			if (lasersAllowed)
			{
				if (
                    ramoSpheres != 0f &&
                    GUILayout.Toggle(lasersOptHit, "L Hit ORBs") != lasersOptHit)
				{
					lasersOptHit = !lasersOptHit;
					updateServerPrefs();
				}
				if (GUILayout.Toggle(
                    lasersFatal,
                    "L Hits Rematerialize") != lasersFatal)
				{
					lasersFatal = !lasersFatal;
					updateServerPrefs();
				}

				GUILayout.BeginHorizontal();
				cg = GUILayout.HorizontalSlider(laserSpeed, 20f, 340f);
				if ((float)laserSpeed != cg)
				{
					laserSpeed = (int)cg;
					updateServerPrefs();
				}
				GUILayout.Label("Lsr Spd", GUILayout.Width(65f));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				cg = GUILayout.HorizontalSlider(laserGrav, 0f, 1f);
				if (laserGrav != cg)
				{
					laserGrav = cg;
					updateServerPrefs();
				}
				GUILayout.Label("Lsr Gvt", GUILayout.Width(65f));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				cg = GUILayout.HorizontalSlider(laserRico, 0f, 1f);
				if (laserRico != cg)
				{
					laserRico = cg;
					updateServerPrefs();
				}
				GUILayout.Label("Lsr Rco", GUILayout.Width(65f));
				GUILayout.EndHorizontal();

				GUILayout.Space(10f);
			}

			GUILayout.BeginHorizontal();
			cg = GUILayout.HorizontalSlider(
                worldGrav,
                0,//0.81f * -1f,
                18.81f * -1f);
			if (worldGrav != cg)
			{
				worldGrav = cg;
				updateServerPrefs();
			}
			GUILayout.Label("Gravity", GUILayout.Width(65f));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			cg = GUILayout.HorizontalSlider(worldViewDist, 300f, 9700f);
			if (worldViewDist != cg)
			{
				worldViewDist = cg;
				updateServerPrefs();
			}
			GUILayout.Label("Visibility", GUILayout.Width(65f));
			GUILayout.EndHorizontal();

			if ((bool)World.sea)
			{
				GUILayout.BeginHorizontal();
				cg = GUILayout.HorizontalSlider(
                    lavaFog,
                    0.015f,
                    0.01f * -1f);
				if (lavaFog != cg)
				{
					lavaFog = cg;
					updateServerPrefs();
				}
				GUILayout.Label("Sea Vis", GUILayout.Width(65f));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				cg = GUILayout.HorizontalSlider(lavaAlt, -100f, 100f);
				if (lavaAlt != cg)
				{
					lavaAlt = cg;
					updateServerPrefs();
				}
				GUILayout.Label("Sea Alt", GUILayout.Width(65f));
				GUILayout.EndHorizontal();
			}

			GUILayout.Space(20f);
			GUILayout.Label("Bots:");

			if (GUILayout.Toggle(botsCanFire, "Can Fire") != botsCanFire)
			{
				botsCanFire = !botsCanFire;
				updateServerPrefs();
			}

			if (GUILayout.Toggle(botsCanDrive, "Can Drive") != botsCanDrive)
			{
				botsCanDrive = !botsCanDrive;
				updateServerPrefs();
			}

			GUILayout.Space(10f);

			GUILayout.BeginHorizontal();
			if (
                GUILayout.Button("Add Bot") &&
                Game.Controller.botsInGame < int.MaxValue)
			{
				Game.Controller.StartCoroutine(Game.Controller.addBot());
			}
			if (Game.Controller.botsInGame > 0)
			{
				GUILayout.Space(5f);
				if (GUILayout.Button("Axe Bot"))
				{
					Game.Controller.StartCoroutine(Game.Controller.axeBot());
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(20f);
			GUILayout.Label("Buggy:");

			if (GUILayout.Toggle(buggyAllowed, "Available") != buggyAllowed)
			{
				buggyAllowed = !buggyAllowed;
				updateServerPrefs();
			}
			if (buggyAllowed)
			{
				if (GUILayout.Toggle(
                    buggyFlightSlip,
                    "Stall Blending") != buggyFlightSlip)
				{
					buggyFlightSlip = !buggyFlightSlip;
					updateServerPrefs();
				}
				if (GUILayout.Toggle(
                    buggyFlightLooPower,
                    "Powered Loops") != buggyFlightLooPower)
				{
					buggyFlightLooPower = !buggyFlightLooPower;
					updateServerPrefs();
				}
				if (GUILayout.Toggle(
                    buggySmartSuspension,
                    "Smart Suspension") != buggySmartSuspension)
				{
					buggySmartSuspension = !buggySmartSuspension;
					updateServerPrefs();
				}

				GUILayout.BeginHorizontal();
				cg = GUILayout.HorizontalSlider(buggyFlightDrag, 50f, 550f);
				if (buggyFlightDrag != cg)
				{
					buggyFlightDrag = cg;
					updateServerPrefs();
				}
				GUILayout.Label("Fl Speed", GUILayout.Width(65f));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				cg = GUILayout.HorizontalSlider(
                    buggyFlightAgility,
                    0.5f,
                    1.5f);
				if (buggyFlightAgility != cg)
				{
					buggyFlightAgility = cg;
					updateServerPrefs();
				}
				GUILayout.Label("Fl Agility", GUILayout.Width(65f));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				cg = GUILayout.HorizontalSlider(
                    buggyCG,
                    -0.1f,
                    -0.7f);
				if (buggyCG != cg)
				{
					buggyCG = cg;
					updateServerPrefs();
				}
				GUILayout.Label("Stability", GUILayout.Width(65f));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				cg = GUILayout.HorizontalSlider(buggyPower, 0.5f, 1.5f);
				if (buggyPower != cg)
				{
					buggyPower = cg;
					updateServerPrefs();
				}
				GUILayout.Label("Power", GUILayout.Width(65f));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				cg = GUILayout.HorizontalSlider(buggySpeed, 5f, 55f);
				if (buggySpeed != cg)
				{
					buggySpeed = cg;
					updateServerPrefs();
				}
				GUILayout.Label("Speed", GUILayout.Width(65f));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				cg = GUILayout.HorizontalSlider(buggyTr, 0.1f, 1.9f);
				if (buggyTr != cg)
				{
					buggyTr = cg;
					updateServerPrefs();
				}
				GUILayout.Label("Traction", GUILayout.Width(65f));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				cg = GUILayout.HorizontalSlider(buggySh, 20f, 120f);
				if (buggySh != cg)
				{
					buggySh = cg;
					updateServerPrefs();
				}
				GUILayout.Label("Shocks", GUILayout.Width(65f));
				GUILayout.EndHorizontal();

				if (lasersAllowed)
				{
					GUILayout.BeginHorizontal();
					cg = GUILayout.HorizontalSlider(firepower[0], 0f, 3f);
					if ((float)firepower[0] != cg)
					{
						firepower[0] = (int)cg;
						updateServerPrefs();
					}
					GUILayout.Label("Firepower", GUILayout.Width(65f));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					cg = GUILayout.HorizontalSlider(laserLock[0], 0f, 1f);
					if (laserLock[0] != cg)
					{
						laserLock[0] = cg;
						updateServerPrefs();
					}
					GUILayout.Label("Lsr Lck", GUILayout.Width(65f));
					GUILayout.EndHorizontal();
				}

			}

			GUILayout.Space(20f);
			GUILayout.Label("Tank:");

			if (GUILayout.Toggle(tankAllowed, "Available") != tankAllowed)
			{
				tankAllowed = !tankAllowed;
				updateServerPrefs();
			}

			if (tankAllowed)
			{
				GUILayout.BeginHorizontal();
				cg = GUILayout.HorizontalSlider(tankCG, 1f, 1.4f * -1f);
				if (tankCG != cg)
				{
					tankCG = cg;
					updateServerPrefs();
				}
				GUILayout.Label("Stability", GUILayout.Width(65f));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				cg = GUILayout.HorizontalSlider(tankGrip, 0f, 0.2f);
				if (tankGrip != cg)
				{
					tankGrip = cg;
					updateServerPrefs();
				}
				GUILayout.Label("Grip", GUILayout.Width(65f));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				cg = GUILayout.HorizontalSlider(tankSpeed, 10f, 40f);
				if (tankSpeed != cg)
				{
					tankSpeed = cg;
					updateServerPrefs();
				}
				GUILayout.Label("Speed", GUILayout.Width(65f));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				cg = GUILayout.HorizontalSlider(tankPower, 500f, 3500f);
				if (tankPower != cg)
				{
					tankPower = cg;
					updateServerPrefs();
				}
				GUILayout.Label("Power", GUILayout.Width(65f));
				GUILayout.EndHorizontal();

				if (lasersAllowed)
				{
					GUILayout.BeginHorizontal();
					cg = GUILayout.HorizontalSlider(firepower[2], 0f, 3f);
					if ((float)firepower[2] != cg)
					{
						firepower[2] = (int)cg;
						updateServerPrefs();
					}
					GUILayout.Label("Firepower", GUILayout.Width(65f));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					cg = GUILayout.HorizontalSlider(laserLock[2], 0f, 1f);
					if (laserLock[2] != cg)
					{
						laserLock[2] = cg;
						updateServerPrefs();
					}
					GUILayout.Label("Lsr Lck", GUILayout.Width(65f));
					GUILayout.EndHorizontal();
				}
			}

			GUILayout.Space(20f);
			GUILayout.Label("Hovercraft:");

			if (GUILayout.Toggle(hoverAllowed, "Available") != hoverAllowed)
			{
				hoverAllowed = !hoverAllowed;
				updateServerPrefs();
			}

			if (hoverAllowed)
			{
				GUILayout.BeginHorizontal();
				cg = GUILayout.HorizontalSlider(hoverHeight, 5f, 25f);
				if (hoverHeight != cg)
				{
					hoverHeight = cg;
					updateServerPrefs();
				}
				GUILayout.Label("Height", GUILayout.Width(65f));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				cg = GUILayout.HorizontalSlider(hoverHover, 20f, 180f);
				if (hoverHover != cg)
				{
					hoverHover = cg;
					updateServerPrefs();
				}
				GUILayout.Label("Hover", GUILayout.Width(65f));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				cg = GUILayout.HorizontalSlider(hoverRepel, 0.5f, 4.5f);
				if (hoverRepel != cg)
				{
					hoverRepel = cg;
					updateServerPrefs();
				}
				GUILayout.Label("Repulsion", GUILayout.Width(65f));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				cg = GUILayout.HorizontalSlider(hoverThrust, 20f, 420f);
				if (hoverThrust != cg)
				{
					hoverThrust = cg;
					updateServerPrefs();
				}
				GUILayout.Label("Thrust", GUILayout.Width(65f));
				GUILayout.EndHorizontal();

				if (lasersAllowed)
				{
					GUILayout.BeginHorizontal();
					cg = GUILayout.HorizontalSlider(firepower[1], 0f, 3f);
					if ((float)firepower[1] != cg)
					{
						firepower[1] = (int)cg;
						updateServerPrefs();
					}
					GUILayout.Label("Firepower", GUILayout.Width(65f));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					cg = GUILayout.HorizontalSlider(laserLock[1], 0f, 1f);
					if (laserLock[1] != cg)
					{
						laserLock[1] = cg;
						updateServerPrefs();
					}
					GUILayout.Label("Lsr Lck", GUILayout.Width(65f));
					GUILayout.EndHorizontal();
				}
			}

			GUILayout.Space(20f);
			GUILayout.Label("Jet:");

			if (GUILayout.Toggle(jetAllowed, "Available") != jetAllowed)
			{
				jetAllowed = !jetAllowed;
				updateServerPrefs();
			}

			if (jetAllowed)
			{
				GUILayout.BeginHorizontal();
				cg = GUILayout.HorizontalSlider(jetHDrag, 0.005f, 0.015f);
				if (jetHDrag != cg)
				{
					jetHDrag = cg;
					updateServerPrefs();
				}
				GUILayout.Label("HoverDrag", GUILayout.Width(65f));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				cg = GUILayout.HorizontalSlider(jetDrag, 0.0005f, 0.0015f);
				if (jetDrag != cg)
				{
					jetDrag = cg;
					updateServerPrefs();
				}
				GUILayout.Label("Drag", GUILayout.Width(65f));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				cg = GUILayout.HorizontalSlider(jetSteer, 5f, 35f);
				if ((float)jetSteer != cg)
				{
					jetSteer = (int)cg;
					updateServerPrefs();
				}
				GUILayout.Label("Agility", GUILayout.Width(65f));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				cg = GUILayout.HorizontalSlider(jetLift, 0.1f, 0.9f);
				if (jetLift != cg)
				{
					jetLift = cg;
					updateServerPrefs();
				}
				GUILayout.Label("Lift", GUILayout.Width(65f));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				cg = GUILayout.HorizontalSlider(jetStall, 1f, 39f);
				if ((float)jetStall != cg)
				{
					jetStall = (int)cg;
					updateServerPrefs();
				}
				GUILayout.Label("Stall", GUILayout.Width(65f));
				GUILayout.EndHorizontal();

				if (lasersAllowed)
				{
					GUILayout.BeginHorizontal();
					cg = GUILayout.HorizontalSlider(firepower[3], 0f, 3f);
					if ((float)firepower[3] != cg)
					{
						firepower[3] = (int)cg;
						updateServerPrefs();
					}
					GUILayout.Label("Firepower", GUILayout.Width(65f));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					cg = GUILayout.HorizontalSlider(laserLock[3], 0f, 1f);
					if (laserLock[3] != cg)
					{
						laserLock[3] = cg;
						updateServerPrefs();
					}
					GUILayout.Label("Lsr Lck", GUILayout.Width(65f));
					GUILayout.EndHorizontal();
				}
			}

			GUILayout.Space(20f);
			GUILayout.Label("Network Mode:");

			if (GUILayout.Toggle(networkMode == 0, "UDP") != (networkMode == 0))
			{
				networkMode = 0;
				updateServerPrefs();
			}
			if (GUILayout.Toggle(networkMode == 1, "RDC") != (networkMode == 1))
			{
				networkMode = 1;
				updateServerPrefs();
			}
			if (GUILayout.Toggle(networkMode == 2, "RPC") != (networkMode == 2))
			{
				networkMode = 2;
				updateServerPrefs();
			}

			if (networkMode == 0)
			{
				GUILayout.Label("\"UDP\" is the fastest mode, but may result in players with \"No Connection\"");
			}
			else if (networkMode == 1)
			{
				GUILayout.Label("\"RDC\" sacrifices speed for reliability");
			}
			else
			{
				GUILayout.Label("\"RPC\" guarantees reliability at the expense of speed");
			}

			GUILayout.Space(20f);
			GUILayout.Label("Network Physics:");

			if (GUILayout.Toggle(networkPhysics == 0, "Advanced") != (networkPhysics == 0))
			{
				networkPhysics = 0;
				updateServerPrefs();
			}
			if (GUILayout.Toggle(networkPhysics == 1, "Enhanced") != (networkPhysics == 1))
			{
				networkPhysics = 1;
				updateServerPrefs();
			}
			if (GUILayout.Toggle(networkPhysics == 2, "Simplified") != (networkPhysics == 2))
			{
				networkPhysics = 2;
				updateServerPrefs();
			}

			if (networkPhysics == 0)
			{
				GUILayout.Label("\"Advanced\" is optimized for smooth movement and realistic collisions over the internet");
			}
			else if (networkPhysics == 1)
			{
				GUILayout.Label("\"Enhanced\" provides maximum movement precision at the cost of higher processor and network load");
			}
			else
			{
				GUILayout.Label("\"Simplified\" provides smooth movement and maximum framerates in games which don't need highly accurate vehicle collisions");
			}

			GUILayout.Space(20f);
			GUILayout.Label("Network Interpolation:");

			GUILayout.BeginHorizontal();
			cg = GUILayout.HorizontalSlider(networkInterpolation, 0f, 0.5f);
			if (networkInterpolation != cg)
			{
				networkInterpolation = cg;
				updateServerPrefs();
			}
			GUILayout.Label((!(networkInterpolation < 0.01f)) ? (networkInterpolation * 1000f + " ms") : "Auto", GUILayout.Width(65f));
			GUILayout.EndHorizontal();

		}
		else
		{
			GUILayout.Space(20f);
			GUILayout.Label("(NOTE: all these parameters are adjustable only by the server host. You can't change anything in this window)");
			GUILayout.Space(60f);

			GUILayout.Toggle(minimapAllowed, "Minimap enabled");
			GUILayout.Toggle(hideNames, "Camouflage Badges");

			GUILayout.Toggle(ramoSpheres != 0f, "RORBs Enabled");
			if (ramoSpheres != 0f)
			{
				GUILayout.BeginHorizontal();
				GUILayout.HorizontalSlider(ramoSpheres, 0.001f, 1f);
				GUILayout.Label("Size", GUILayout.Width(65f));
				GUILayout.EndHorizontal();

				GUILayout.Toggle(zorbSpeed != 0f, "XORBs Available");
				if (zorbSpeed != 0f)
				{
					GUILayout.BeginHorizontal();
					GUILayout.HorizontalSlider(zorbSpeed, 0.001f, 14f);
					GUILayout.Label("X Speed", GUILayout.Width(65f));
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					GUILayout.HorizontalSlider(zorbAgility, -7f, 7f);
					GUILayout.Label("X Agility", GUILayout.Width(65f));
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					GUILayout.HorizontalSlider(zorbBounce, 0f, 1f);
					GUILayout.Label("X Bounce", GUILayout.Width(65f));
					GUILayout.EndHorizontal();
				}
				GUILayout.Space(10f);
			}

			GUILayout.Toggle(lasersAllowed, "Lasers enabled");
			if (lasersAllowed)
			{
				if (ramoSpheres != 0f)
				{
					GUILayout.Toggle(lasersOptHit, "L Hit ORBs");
				}
				GUILayout.Toggle(lasersFatal, "L Hits Rematerialize");
				GUILayout.BeginHorizontal();
				GUILayout.HorizontalSlider(laserSpeed, 20f, 340f);
				GUILayout.Label("Lsr Spd", GUILayout.Width(65f));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.HorizontalSlider(laserGrav, 0f, 1f);
				GUILayout.Label("Lsr Gvt", GUILayout.Width(65f));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.HorizontalSlider(laserRico, 0f, 1f);
				GUILayout.Label("Lsr Rco", GUILayout.Width(65f));
				GUILayout.EndHorizontal();
				GUILayout.Space(10f);
			}
			GUILayout.BeginHorizontal();

			GUILayout.HorizontalSlider(worldGrav, 0.81f * -1f, 18.81f * -1f);
			GUILayout.Label("Gravity", GUILayout.Width(65f));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.HorizontalSlider(worldViewDist, 500f, 9500f);
			GUILayout.Label("Visibility", GUILayout.Width(65f));
			GUILayout.EndHorizontal();

			if ((bool)World.sea)
			{
				GUILayout.BeginHorizontal();
				GUILayout.HorizontalSlider(lavaFog, 0.015f, 0.01f * -1f);
				GUILayout.Label("Lava Fog", GUILayout.Width(65f));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.HorizontalSlider(lavaAlt, -100f, 100f);
				GUILayout.Label("Lava Alt", GUILayout.Width(65f));
				GUILayout.EndHorizontal();
			}

			GUILayout.Space(20f);
			GUILayout.Label("Bots:");
			GUILayout.Toggle(botsCanFire, "Can Fire");
			GUILayout.Toggle(botsCanDrive, "Can Drive");

			GUILayout.Space(20f);
			GUILayout.Label("Buggy:");
			GUILayout.Toggle(buggyAllowed, "Available");
			if (buggyAllowed)
			{
				GUILayout.Toggle(buggyFlightSlip, "Stall Blending");
				GUILayout.Toggle(buggyFlightLooPower, "Powered Loops");
				GUILayout.Toggle(buggySmartSuspension, "Smart Suspension");
				GUILayout.BeginHorizontal();
				GUILayout.HorizontalSlider(buggyFlightDrag, 50f, 550f);
				GUILayout.Label("Fl Speed", GUILayout.Width(65f));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.HorizontalSlider(buggyFlightAgility, 0.5f, 1.5f);
				GUILayout.Label("Fl Agility", GUILayout.Width(65f));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.HorizontalSlider(buggyCG, 0.1f * -1f, 0.7f * -1f);
				GUILayout.Label("Stability", GUILayout.Width(65f));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.HorizontalSlider(buggyPower, 0.5f, 1.5f);
				GUILayout.Label("Power", GUILayout.Width(65f));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.HorizontalSlider(buggySpeed, 5f, 55f);
				GUILayout.Label("Speed", GUILayout.Width(65f));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.HorizontalSlider(buggyTr, 0.1f, 1.9f);
				GUILayout.Label("Traction", GUILayout.Width(65f));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.HorizontalSlider(buggySh, 20f, 120f);
				GUILayout.Label("Shocks", GUILayout.Width(65f));
				GUILayout.EndHorizontal();
				if (lasersAllowed)
				{
					GUILayout.BeginHorizontal();
					GUILayout.HorizontalSlider(firepower[0], 0f, 3f);
					GUILayout.Label("Firepower", GUILayout.Width(65f));
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					GUILayout.HorizontalSlider(laserLock[0], 0f, 1f);
					GUILayout.Label("Lsr Lck", GUILayout.Width(65f));
					GUILayout.EndHorizontal();
				}
			}

			GUILayout.Space(20f);
			GUILayout.Label("Tank:");
			GUILayout.Toggle(tankAllowed, "Available");
			if (tankAllowed)
			{
				GUILayout.BeginHorizontal();
				GUILayout.HorizontalSlider(tankCG, 1f, 1.4f * -1f);
				GUILayout.Label("Stability", GUILayout.Width(65f));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.HorizontalSlider(tankGrip, 0f, 0.2f);
				GUILayout.Label("Grip", GUILayout.Width(65f));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.HorizontalSlider(tankSpeed, 10f, 40f);
				GUILayout.Label("Speed", GUILayout.Width(65f));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.HorizontalSlider(tankPower, 500f, 3500f);
				GUILayout.Label("Power", GUILayout.Width(65f));
				GUILayout.EndHorizontal();
				if (lasersAllowed)
				{
					GUILayout.BeginHorizontal();
					GUILayout.HorizontalSlider(firepower[2], 0f, 3f);
					GUILayout.Label("Firepower", GUILayout.Width(65f));
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					GUILayout.HorizontalSlider(laserLock[2], 0f, 1f);
					GUILayout.Label("Lsr Lck", GUILayout.Width(65f));
					GUILayout.EndHorizontal();
				}
			}

			GUILayout.Space(20f);
			GUILayout.Label("Hovercraft:");
			GUILayout.Toggle(hoverAllowed, "Available");
			if (hoverAllowed)
			{
				GUILayout.BeginHorizontal();
				GUILayout.HorizontalSlider(hoverHeight, 5f, 25f);
				GUILayout.Label("Height", GUILayout.Width(65f));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.HorizontalSlider(hoverHover, 20f, 180f);
				GUILayout.Label("Hover", GUILayout.Width(65f));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.HorizontalSlider(hoverRepel, 0.5f, 4.5f);
				GUILayout.Label("Repulsion", GUILayout.Width(65f));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.HorizontalSlider(hoverThrust, 20f, 420f);
				GUILayout.Label("Thrust", GUILayout.Width(65f));
				GUILayout.EndHorizontal();
				if (lasersAllowed)
				{
					GUILayout.BeginHorizontal();
					GUILayout.HorizontalSlider(firepower[1], 0f, 3f);
					GUILayout.Label("Firepower", GUILayout.Width(65f));
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					GUILayout.HorizontalSlider(laserLock[1], 0f, 1f);
					GUILayout.Label("Lsr Lck", GUILayout.Width(65f));
					GUILayout.EndHorizontal();
				}
			}


			GUILayout.Space(20f);
			GUILayout.Label("Jet:");
			GUILayout.Toggle(jetAllowed, "Available");
			if (jetAllowed)
			{
				GUILayout.BeginHorizontal();
				GUILayout.HorizontalSlider(jetHDrag, 0.005f, 0.015f);
				GUILayout.Label("HoverDrag", GUILayout.Width(65f));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.HorizontalSlider(jetDrag, 0.0005f, 0.0015f);
				GUILayout.Label("Drag", GUILayout.Width(65f));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.HorizontalSlider(jetSteer, 5f, 35f);
				GUILayout.Label("Agility", GUILayout.Width(65f));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.HorizontalSlider(jetLift, 0.1f, 0.9f);
				GUILayout.Label("Lift", GUILayout.Width(65f));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.HorizontalSlider(jetStall, 1f, 39f);
				GUILayout.Label("Stall", GUILayout.Width(65f));
				GUILayout.EndHorizontal();
				if (lasersAllowed)
				{
					GUILayout.BeginHorizontal();
					GUILayout.HorizontalSlider(firepower[3], 0f, 3f);
					GUILayout.Label("Firepower", GUILayout.Width(65f));
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					GUILayout.HorizontalSlider(laserLock[3], 0f, 1f);
					GUILayout.Label("Lsr Lck", GUILayout.Width(65f));
					GUILayout.EndHorizontal();
				}
			}

			GUILayout.Space(20f);
			GUILayout.Label("Networking:");
			GUILayout.Toggle(networkMode == 0, "UDP");
			GUILayout.Toggle(networkMode == 1, "RDC");
			GUILayout.Toggle(networkMode == 2, "RPC");
			GUILayout.Toggle(networkPhysics == 0, "Advanced");
			GUILayout.Toggle(networkPhysics == 1, "Enhanced");
			GUILayout.Toggle(networkPhysics == 2, "Simplified");
			GUILayout.BeginHorizontal();
			GUILayout.HorizontalSlider(networkInterpolation, 0f, 0.5f);
            GUILayout.Label(
                (networkInterpolation < 0.01 ?
                    "Auto" :
                    (networkInterpolation * 1000) + " ms"),
                GUILayout.Width(65));
			GUILayout.EndHorizontal();
		}

		GUILayout.Label("nTime: " + Mathf.RoundToInt((float)Network.time));
		
        GUILayout.Space(20f);
		GUILayout.Label("Settings I/O:");
		serverString = GUILayout.TextField(serverString);
		if (
            Game.Controller.isHost &&
            GUILayout.Button("Apply Custom\nSettings"))
		{
			Game.Controller.GetComponent<NetworkView>().RPC(
                "sSS",
                RPCMode.All,
                serverString);
		}
	}

	public void updatePrefs()
	{

            /*
	    1 Fastest
	    2 Fast
	    3 Simple
	    4 Good
	    5 Beautiful
	    6 Fantastic
	    */

        //This can be automatically overwritten by the _Core GUI, so make sure it is whatever the user set it to when entering a game.
		renderLevel = PlayerPrefs.GetInt("renderLevel", 4);

        //Laser Locking
		laserLocking = false;
        for (int i = 0; i < laserLock.Length; i++)
        {
            if (laserLock[i] > 0f)
            {
                laserLocking = true;
                break;
            }
        }

        //LOD Distance
		if (renderLevel > 4)
		{
			World.lodDist = 1000;
		}
		else if (renderLevel > 3)
		{
			World.lodDist = 400;
		}
		else if (renderLevel > 2)
		{
			World.lodDist = 75;
		}
		else
		{
			World.lodDist = 0;
		}

        //Rendering Effects
		if (renderLevel > 4)
		{
			camContrast.enabled = true;

			if (renderLevel > 5)
			{
				camSSAO.enabled = true;
				camSSAO.m_Downsampling = 2;
			}
			else
			{
				camSSAO.enabled = false;
			}
		}
		else
		{
			camSSAO.enabled = false;
			camContrast.enabled = false;
		}

		if (networkPhysics == 2)
		{
			Network.sendRate = 10f;
		}
		else
		{
			Network.sendRate = 15f;
		}

		if (ramoSpheres == 0f)
		{
			zorbSpeed = 0f;
		}
		if (useMusic == 0)
		{
			gameMusic.enabled = false;
			if (gameMusic.isPlaying)
			{
				gameMusic.Stop();
			}
		}
		else
		{
			gameMusic.enabled = true;
			gameMusic.pitch = 1f;
            gameMusic.clip = musicTracks[useMusic - 1];
			if (!gameMusic.isPlaying)
			{
				gameMusic.Play();
			}
		}

		GameObject.Find("MiniMap").GetComponent<Camera>().enabled = minimapAllowed && useMinimap;
		
        QualitySettings.SetQualityLevel(renderLevel - 1);
		
        Time.fixedDeltaTime = ((renderLevel <= 3) ? 0.025f : 0.02f);
		
        Camera.main.farClipPlane = ((renderViewCap != 1000f) ?
            Mathf.Min(renderViewCap, worldViewDist) :
            worldViewDist);
		worldFog = Mathf.Lerp(
            0.007f,
            0.0003f,
            Camera.main.farClipPlane / 6000f);
		
        if (World.terrains != null)
		{
			foreach (Terrain trn in World.terrains)
			{
                //Details: Rocks, trees, etc
				trn.treeCrossFadeLength = 30f;
				if (renderLevel > 4)
				{   //Fantastic, Beautiful
					trn.detailObjectDistance = 300f;
					trn.treeDistance = 600f;
					trn.treeMaximumFullLODCount = 100;
					trn.treeBillboardDistance = 150f;
				}
				else if (renderLevel > 3)
				{   //Good
					trn.detailObjectDistance = 200f;
					trn.treeDistance = 500f;
					trn.treeMaximumFullLODCount = 50;
					trn.treeBillboardDistance = 100f;
				}
				else if (renderLevel > 2)
				{   //Simple
					trn.detailObjectDistance = 150f;
					trn.treeDistance = 300f;
					trn.treeMaximumFullLODCount = 10;
					trn.treeBillboardDistance = 75f;
				}
				else
				{   //Fast, Fastest
					trn.detailObjectDistance = 0f;
					trn.treeDistance = 0f;
					trn.treeMaximumFullLODCount = 0;
					trn.treeBillboardDistance = 0f;
				}

                //Textures
				trn.basemapDistance = 1500f;

                //Heightmap Resolution
				if (renderLevel > 5)
				{
					trn.heightmapMaximumLOD = 0;
					trn.heightmapPixelError = 5f;
				}
				else if (renderLevel > 2)
				{
					trn.heightmapMaximumLOD = 0;
					trn.heightmapPixelError = 15f;
				}
				else if (renderLevel > 1)
				{
					trn.heightmapMaximumLOD = 0;
					trn.heightmapPixelError = 50f;
				}
				else
				{
					trn.heightmapMaximumLOD = 1;
					trn.heightmapPixelError = 50f;
				}

				/*if (renderLevel > 4)
				{
					trn.lighting = TerrainLighting.Pixel;
				}
				else
				{
					trn.lighting = TerrainLighting.Lightmap;
				}*/
			}
		}

		Physics.gravity = new Vector3(0f, worldGrav, 0f);
		if ((bool)World.sea)
		{
			Vector3 wspos = World.sea.position;
			wspos.y = lavaAlt;
			World.sea.position = wspos;
		}
		zorbPhysics.bounciness = zorbBounce;

		updateObjects();
	}

	public void updateServerPrefs()
	{
		serverUpdateTime = Time.time + 3f;
		updatePrefs();
	}

    public IEnumerator ramdomizeVehicleColor()
    {
        while (!Game.PlayerVeh) yield return null;

        Color pvcol = Game.PlayerVeh.vehicleColor;
        pvcol.r = UnityEngine.Random.value * 0.7f;
        pvcol.b = UnityEngine.Random.value * 0.7f;
        pvcol.g = UnityEngine.Random.value * Mathf.Lerp(
            0.1f,
            0.8f,
            ((Game.PlayerVeh.vehicleColor.r +
                Game.PlayerVeh.vehicleColor.b) / 2)) * 0.7f;
        Game.PlayerVeh.vehicleColor = pvcol;

        colorCustom = false;
        updateVehicleAccent();
        saveVehicleColor();
        Game.PlayerVeh.setColor();
    }

	public void updateVehicleAccent()
	{
		float num = -0.25f;
		Game.PlayerVeh.vehicleAccent.r = Mathf.Max(
            0f,
            Game.PlayerVeh.vehicleColor.r + num);
		Game.PlayerVeh.vehicleAccent.g = Mathf.Max(
            0f,
            Game.PlayerVeh.vehicleColor.g + num);
		Game.PlayerVeh.vehicleAccent.b = Mathf.Max(
            0f,
            Game.PlayerVeh.vehicleColor.b + num);
	}

	public void updateVehicleColor()
	{
		colorUpdateTime = Time.time + 3f;
		colorCustom = true;
		Game.PlayerVeh.setColor();
	}

	public void saveVehicleColor()
	{
		PlayerPrefs.SetInt("vehColCustom", colorCustom ? 1 : 0);
		PlayerPrefs.SetFloat("vehColR", Game.PlayerVeh.vehicleColor.r);
		PlayerPrefs.SetFloat("vehColG", Game.PlayerVeh.vehicleColor.g);
		PlayerPrefs.SetFloat("vehColB", Game.PlayerVeh.vehicleColor.b);
		PlayerPrefs.SetFloat("vehColAccR", Game.PlayerVeh.vehicleAccent.r);
		PlayerPrefs.SetFloat("vehColAccG", Game.PlayerVeh.vehicleAccent.g);
		PlayerPrefs.SetFloat("vehColAccB", Game.PlayerVeh.vehicleAccent.b);
		Game.Player.GetComponent<NetworkView>().RPC(
            "sC",
            RPCMode.Others,
            Game.PlayerVeh.vehicleColor.r,
            Game.PlayerVeh.vehicleColor.g,
            Game.PlayerVeh.vehicleColor.b,
            Game.PlayerVeh.vehicleAccent.r,
            Game.PlayerVeh.vehicleAccent.g,
            Game.PlayerVeh.vehicleAccent.b);
	}

	public string packServerPrefs()
	{
		return
            "lasr:" + (lasersAllowed ? 1 : 0) + ";" +
            "lsrh:" + (lasersFatal ? 1 : 0) + ";" +
            "lsro:" + (lasersOptHit ? 1 : 0) + ";" +
            "mmap:" + (minimapAllowed ? 1 : 0) + ";" +
            "camo:" + (hideNames ? 1 : 0) + ";" +
            "rorb:" + ramoSpheres + ";" +
            "xspd:" + zorbSpeed + ";" +
            "xagt:" + zorbAgility + ";" +
            "xbnc:" + zorbBounce + ";" +
            "grav:" + worldGrav * -1f + ";" +
            "wvis:" + worldViewDist + ";" +
            "lfog:" + lavaFog + ";" +
            "lalt:" + lavaAlt + ";" +
            "lspd:" + laserSpeed + ";" +
            "lgvt:" + laserGrav + ";" +
            "lrco:" + laserRico + ";" +

            "botfire:" + (botsCanFire ? 1 : 0) + ";" +
            "botdrive:" + (botsCanDrive ? 1 : 0) + ";" +

            "bugen:" + (buggyAllowed ? 1 : 0) + ";" +
            "bugflsl:" + (buggyFlightSlip ? 1 : 0) + ";" +
            "bugflpw:" + (buggyFlightLooPower ? 1 : 0) + ";" +
            "bugawd:" + (buggyAWD ? 1 : 0) + ";" +
            "bugspn:" + (buggySmartSuspension ? 1 : 0) + ";" +
            "bugfldr:" + buggyFlightDrag + ";" +
            "bugflag:" + buggyFlightAgility + ";" +
            "bugcg:" + buggyCG + ";" +
            "bugpow:" + buggyPower + ";" +
            "bugspd:" + buggySpeed + ";" +
            "bugtr:" + buggyTr + ";" +
            "bugsl:" + buggySl + ";" +
            "bugsh:" + buggySh + ";" +
            "bugfp:" + firepower[0] + ";" +
            "bugll:" + laserLock[0] + ";" +

            "tnken:" + (tankAllowed ? 1 : 0) + ";" +
            "tnkpow:" + tankPower + ";" +
            "tnkgrp:" + tankGrip + ";" +
            "tnkspd:" + tankSpeed + ";" +
            "tnkcg:" + tankCG + ";" +
            "tnkfp:" + firepower[2] + ";" +
            "tnkll:" + laserLock[2] + ";" +

            "hvren:" + (hoverAllowed ? 1 : 0) + ";" +
            "hvrhe:" + hoverHeight + ";" +
            "hvrhv:" + hoverHover + ";" +
            "hvrrp:" + hoverRepel + ";" +
            "hvrth:" + hoverThrust + ";" +
            "hvrfp:" + firepower[1] + ";" +
            "hvrll:" + laserLock[1] + ";" +

            "jeten:" + (jetAllowed ? 1 : 0) + ";" +
            "jethd:" + jetHDrag + ";" +
            "jetd:" + jetDrag + ";" +
            "jets:" + jetSteer + ";" +
            "jetl:" + jetLift + ";" +
            "jetss:" + jetStall + ";" +
            "jetfp:" + firepower[3] + ";" +
            "jetll:" + laserLock[3] + ";" +

            "netm:" + networkMode + ";" +
            "netph:" + networkPhysics + ";" +
            "netin:" + networkInterpolation + ";" +
        "";
	}

	public void updateObjects()
	{
        foreach (ParticleEmitter pE in FindObjectsOfType(typeof(ParticleEmitter)))
        {
            pE.emit = (renderLevel >= 3);
        }

        foreach (Light light in FindObjectsOfType(typeof(Light)))
        {
            if (light.name != "VehicleLight") continue;
            light.enabled = false;
        }

        foreach (GameObject go in FindObjectsOfType(typeof(GameObject)))
        {
            go.SendMessage("OnPrefsUpdated", SendMessageOptions.DontRequireReceiver);
        }
	}

	public void toggleFullscreen()
	{
		Resolution resolution = Screen.resolutions[Screen.resolutions.Length - 1];
        if (!Screen.fullScreen)
        {
            Screen.SetResolution(resolution.width, resolution.height, true);
            if (Application.platform == RuntimePlatform.OSXDashboardPlayer)
            {
                enteredfullscreen = true;
            }
        }
        else
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
                resolution = Screen.resolutions[Screen.resolutions.Length - 2];
                Screen.SetResolution(resolution.width, resolution.height, false);
            }
            if (enteredfullscreen)
            {
                enteredfullscreen = false;
            }
        }
	}
}
