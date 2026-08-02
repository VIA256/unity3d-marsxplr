using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class Vehicle : MonoBehaviour
{
	public int camOffset = 2;
	public Transform ridePos;
	public LayerMask terrainMask = ~1<<4;

	public GameObject laserAimer;
	public GameObject laserAimerLocked;
	public GameObject laserLock;
	public ParticleEmitter bubbles;

	public int vehId = 0;
	public int networkMode = 0;
	[NonSerialized]
	public string shortName;
	[NonSerialized]
	public int isIt = 0;
	[NonSerialized]
	public float lastTag = 0.00f;
	private float startTime;
	[NonSerialized]
	public float lastReset = 0.00f;
	[NonSerialized]
	public int score;
	[NonSerialized]
	public int scoreTime;
	public Vector4 input;
	public bool specialInput = false;
	public bool inputThrottle;
	[NonSerialized]
	public bool zorbBall = false;
	[NonSerialized]
	public bool brakes = false;
	[NonSerialized]
	public bool camSmooth;
	[NonSerialized]
	public Vector3 velocity;

	public bool isBot = false;
	[NonSerialized]
	public bool isPlayer = false;
	[NonSerialized]
	public bool isResponding = false;
	[NonSerialized]
	public string netCode = " (No Connection)";
	[NonSerialized]
	public GameObject vehObj;
	[NonSerialized]
	public Rigidbody myRigidbody;
	[NonSerialized]
	public GameObject ramoSphere;
	public GameObject ramoSphereObj;
	[NonSerialized]
	public float ramoSphereScale;
	private GameObject marker;
	private GameObject markerQuarry;
	[NonSerialized]
	public VehicleNet vehicleNet = null;
	[NonSerialized]
	public int netKillMode = 0;
	private float updateTick = 0f;

	public Color vehicleColor;
	public Color vehicleAccent;
	public Material[] materialMain;
	public Material[] materialAccent;
	private bool updateColor = false;

	public IEnumerator Start()
	{
		if (GetComponent<NetworkView>().viewID.isMine)
		{
			VehicleLocal vehicleLocal = (VehicleLocal)gameObject
                .AddComponent(typeof(VehicleLocal));
			vehicleLocal.vehicle = this;
		}
		if (GetComponent<NetworkView>().viewID.isMine && !isBot)
		{
			marker = (GameObject)Instantiate(
                Game.objectMarkerMe,
                transform.position,
                transform.rotation);
			marker.transform.parent = transform;
			isPlayer = true;
			Game.PlayerVeh = this;
			VehicleMe vehicleMe = (VehicleMe)gameObject
                .AddComponent(typeof(VehicleMe));
			vehicleMe.vehicle = this;
			vehicleColor.r = PlayerPrefs.GetFloat("vehColR");
			vehicleColor.g = PlayerPrefs.GetFloat("vehColG");
			vehicleColor.b = PlayerPrefs.GetFloat("vehColB");
			vehicleAccent.r = PlayerPrefs.GetFloat("vehColAccR");
			vehicleAccent.g = PlayerPrefs.GetFloat("vehColAccG");
			vehicleAccent.b = PlayerPrefs.GetFloat("vehColAccB");
			if (Game.Settings.colorCustom)
			{
				Game.Settings.saveVehicleColor();
			}
			setColor();
		}
		else
		{
			Destroy(laserAimer);
			Destroy(laserAimerLocked);
			marker = (GameObject)Instantiate(
                Game.objectMarker,
                transform.position,
                transform.rotation);
			marker.transform.parent = transform;
			markerQuarry = (GameObject)Instantiate(
                Game.objectMarkerQuarry,
                transform.position,
                transform.rotation);
			markerQuarry.transform.parent = transform;

			if (isBot && GetComponent<NetworkView>().viewID.isMine)
			{
				VehicleBot vehicleBot = (VehicleBot)gameObject
                    .AddComponent(typeof(VehicleBot));
				vehicleBot.vehicle = this;
				vehicleColor = Game.PlayerVeh.vehicleColor;
				vehicleAccent = Game.PlayerVeh.vehicleAccent;
			}
			else
			{
				vehicleNet = (VehicleNet)gameObject
                    .AddComponent(typeof(VehicleNet));
				vehicleNet.vehicle = this;
			}
		}

		gameObject.AddComponent(typeof(Rigidbody));
		myRigidbody = GetComponent<Rigidbody>();
		myRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

		if (Game.Players.ContainsKey(name))
		{
			Game.Players.Remove(name);
		}
		Game.Players.Add(name, this);

        foreach (DictionaryEntry plrE in Game.Players)
        {
            ((Vehicle)plrE.Value).setColor(); //Make sure everyone is colored correctly
        }

        vehObj.BroadcastMessage("InitVehicle", this);
        lastTag = Time.time;
        startTime = Time.time;
        Game.Settings.updateObjects();

        //Force-Refresh Everyone's NetworkView - http://forum.unity3d.com/viewtopic.php?t=35724&postdays=0&postorder=asc&start=105
        GetComponent<NetworkView>().enabled = false;
        yield return null;
        GetComponent<NetworkView>().enabled = true;
	}

	public void Update()
	{
        bubbles.emit = (
            transform.position.y < Game.Settings.lavaAlt - 2 ||
            Physics.Raycast(
                transform.position + (Vector3.up * 200),
                Vector3.down,
                198f,
                1 << 4));
        bubbles.maxEnergy = bubbles.minEnergy = (bubbles.emit ? 5 : 0);

		if (isBot || !GetComponent<NetworkView>().isMine)
		{
			if (isIt != 0 && (bool)markerQuarry && !markerQuarry.activeInHierarchy)
			{
				markerQuarry.SetActive(true);
				marker.SetActive(false);
			}
			else if (isIt == 0 && (bool)markerQuarry && markerQuarry.activeInHierarchy)
			{
				markerQuarry.SetActive(false);
				marker.SetActive(true);
			}
			if (isIt != 0 && (bool)Game.Player)
			{
				Game.Controller.quarryDist = Vector3.Distance(
                    transform.position,
                    Game.Player.transform.position);
			}
		}

		if (updateColor)
		{
			updateColor = false;
		    bool isGreen = (
                isIt != 0 &&
                Game.Players.Count > 1 ? true : false);
		    if(materialMain.Length > 0) {
			    Color targetColor = (isGreen ?
                    Game.Controller.vehicleIsItColor :
                    vehicleColor);
			    materialMain[0].color = Color.Lerp(
                    materialMain[0].color,
                    targetColor,
                    Time.deltaTime * 2);
                Color matmaincol = materialMain[0].color;
                matmaincol.a = 0.5f;
                materialMain[0].color = matmaincol;

			    if(materialAccent.Length > 0) materialMain[0].SetColor (
                    "_SpecColor",
                    materialAccent[0].color);
			    else materialMain[0].SetColor (
                    "_SpecColor",
                    vehicleAccent);
                if (
                    materialMain[0].color.r < targetColor.r - .05 ||
                    materialMain[0].color.r > targetColor.r + .05 ||
                    materialMain[0].color.g < targetColor.g - .05 ||
                    materialMain[0].color.g > targetColor.g + .05 ||
                    materialMain[0].color.b < targetColor.b - .05 ||
                    materialMain[0].color.b > targetColor.b + .05)
                {
                    updateColor = true;
                }
			    if(materialMain.Length > 1)
                    for(int i = 1; i < materialMain.Length; i++)
                        materialMain[i].color = materialMain[0].color;
		    }
		    if(materialAccent.Length > 0) {
			    Color targetColor = (isGreen ?
                    Game.Controller.vehicleIsItAccent :
                    vehicleAccent);
			    materialAccent[0].color = Color.Lerp(
                    materialAccent[0].color,
                    targetColor,
                    Time.deltaTime * 2);
			    
                Color mataccentcol = materialAccent[0].color;
                mataccentcol.a = 0.5f;
                materialAccent[0].color = mataccentcol;

                materialAccent[0].SetColor (
                    "_SpecColor",
                    materialMain[0].color);
                if (
                    materialAccent[0].color.r < targetColor.r - .05 ||
                    materialAccent[0].color.r > targetColor.r + .05 ||
                    materialAccent[0].color.g < targetColor.g - .05 ||
                    materialAccent[0].color.g > targetColor.g + .05 ||
                    materialAccent[0].color.b < targetColor.b - .05 ||
                    materialAccent[0].color.b > targetColor.b + .05)
                {
                    updateColor = true;
                }
			    if(materialAccent.Length > 1)
                    for(int i = 1; i < materialAccent.Length; i++)
                        materialAccent[i].color = materialAccent[0].color;
		    }
		}

		if (Time.time > updateTick)
		{
			updateTick = Time.time + 1f;
			if (!Game.Players.ContainsKey(name))
			{
                Game.Players.Add(name, this);   //Dragonhere: this helps to prevent multi-quarry syndrome
			}
		}
	}

	public void FixedUpdate()
	{
		if (ramoSphereScale != 0f && (bool)ramoSphere)
		{
			ramoSphere.transform.localScale = Vector3.Lerp(
                ramoSphere.transform.localScale,
                Vector3.one * ramoSphereScale,
                Time.fixedDeltaTime);
			if (
                ramoSphere.transform.localScale.x > ramoSphereScale - 0.01f &&
                ramoSphere.transform.localScale.x < ramoSphereScale + 0.01f)
			{
				ramoSphereScale = 0f;
			}
		}
	}

	public void OnGUI()
	{
		if (
            !myRigidbody ||
            (netCode != "" && Time.time < startTime + 5f))
		{
			return;
		}
		GUI.skin = Game.Skin;
		float gUIAlpha = Game.GUIAlpha;

		Color guicol = GUI.color;
		guicol.a = gUIAlpha;
		GUI.color = guicol;

		GUI.depth = -1;
		if (GetComponent<NetworkView>().isMine && !isBot)
		{
            Rigidbody targetRigidbody = myRigidbody;
            if (
                Game.Settings.quarryCam &&
                (bool)Game.QuarryVeh &&
                (bool)Game.QuarryVeh.myRigidbody)
            {
                targetRigidbody = Game.QuarryVeh.myRigidbody;
            }
			GUI.Button(
                new Rect(
                    (float)Screen.width * 0.5f - 75f,
                    (float)Screen.height - 30f,
                    150f,
                    20f),
                (targetRigidbody.velocity.magnitude < 0.05f ?
                    "Static" :
                    Mathf.RoundToInt(targetRigidbody.velocity.magnitude * 2.23f) +
                        " MPH") +
                    "     " +
                    Mathf.RoundToInt(targetRigidbody.transform.position.y) +
                    " ALT" +
                    ((isIt != 0 || Game.Settings.quarryCam) ?
                        "" :
                        ("     " +
                            Mathf.RoundToInt(Game.Controller.quarryDist) +
                            " DST")),
                "hudText");
		}
		GUI.depth = 5;
		Vector3 pos = Camera.main.WorldToScreenPoint(transform.position);
        bool mainTag = GetComponent<NetworkView>().isMine && !isBot;
        if(Game.Settings.quarryCam && (bool)Game.QuarryVeh)
        {
            mainTag = (this == Game.QuarryVeh);
        }
        if (
            mainTag ||
            !Game.Settings.hideNames ||
            (
                Vector3.Distance(
                    new Vector3(pos.x, pos.y, 0),
                    Input.mousePosition) < 40 &&
                !Physics.Linecast(
                    transform.position,
                    Camera.main.transform.position, 1 << 8)))
        {
            if (pos.z > 0 || mainTag)
            {
                if (pos.z < 0f)
                {
                    pos.z = 0f;
                }
                float sizeX = Mathf.Max(
                    50f,
                    Mathf.Min(150f, (float)Screen.width * 0.16f) -
                        pos.z / 1.5f);
                float sizeY = Mathf.Max(
                    20f,
                    Mathf.Min(50f, (float)Screen.width * 0.044f) -
                        pos.z * 0.2f);
                if (
                    (pos.z <= 1f || pos.y < sizeY * 1.9f) &&
                        mainTag)
                {
                    if (pos.z <= 1f)
                    {
                        pos.x = Screen.width / 2;
                    }
                    pos.y = sizeY + 100f;
                }
                GUI.Button(
                    new Rect(
                        pos.x - sizeX * 0.5f,
                        (float)Screen.height - pos.y + sizeY * 1f,
                        sizeX,
                        sizeY),
                    name +
                        "\n" +
                        shortName +
                        " " +
                        score +
                        netCode,
                    "player_nametag" +
                        ((isIt == 0) ?
                            "" :
                            "_it"));
            }
        }
	}

    public IEnumerator OnPrefsUpdated()
    {
        if (laserAimer) laserAimer.GetComponent<ParticleEmitter>().emit = true;
        if (laserAimerLocked) laserAimerLocked.GetComponent<ParticleEmitter>().emit = true;
        if (Game.Settings.ramoSpheres != 0f)
        {
            yield return new WaitForSeconds(1f);
            Vector3 tnsor = GetComponent<Rigidbody>().inertiaTensor;
            Vector3 cg = GetComponent<Rigidbody>().centerOfMass;
            if (!ramoSphere)
            {
                ramoSphere = (GameObject)Instantiate(
                    ramoSphereObj,
                    transform.position,
                    transform.rotation);
                ramoSphere.transform.parent = transform;
                Collider[] colliders = (Collider[])vehObj.GetComponentsInChildren<Collider>();
                foreach (Collider cldr in colliders)
                {
                    Physics.IgnoreCollision(ramoSphere.GetComponent<Collider>(), cldr);
                }
            }
            ramoSphere.SetActive(false); //DRAGONHERE - MAJOR UNITY BUG: We need to set this all the time, as colliders that are instantiated using a prefab and are then thrown inside of rightbodies are not properly initialized until some of their settings are toggled
            ramoSphereScale = (((Game.Settings.ramoSpheres) * 15) +
                camOffset * 1);
            zorbBall = Game.Settings.zorbSpeed != 0f ? zorbBall : false;
            if (ramoSphere.GetComponent<Collider>().isTrigger == zorbBall)
            {
                ramoSphere.GetComponent<Collider>().isTrigger = !zorbBall;
                ramoSphere.transform.localScale = Vector3.zero;
                ramoSphere.GetComponent<Collider>().enabled = true;
                ramoSphere.SendMessage("colorSet", zorbBall); //ANOTHER UNITY BUG - for some reason, SendMessage isn't working like it should...
                ramoSphere.GetComponent<RamoSphere>().colorSet(zorbBall);
            }
            else ramoSphere.GetComponent<Collider>().enabled = true;
            GetComponent<Rigidbody>().inertiaTensor = tnsor;
            GetComponent<Rigidbody>().centerOfMass = cg;
        }
        else if (ramoSphere)
        {
            ramoSphereScale = 0.1f;
            yield return new WaitForSeconds(2f);
            Destroy(ramoSphere);
        }

        if (Game.Settings.laserLock[vehId] > 0f)
        {
            laserLock.SetActive(true);
            laserLock.transform.localScale = Vector3.one *
                (((Game.Settings.laserLock[vehId]) + camOffset * 0.1f) * 10f);
        }
        else
        {
            laserLock.SetActive(false);
            laserLock.transform.localScale = Vector3.zero;
        }
    }

	public void OnCollisionEnter(Collision collision)
	{
		if (
            (bool)ramoSphere &&
            !ramoSphere.GetComponent<Collider>().isTrigger)
		{
			ramoSphere.SendMessage("OnCollisionEnter", collision);
		}
	}

    //Called when we ram a quarry, and will become quarry
	public void OnRam(GameObject other)
	{
		Vehicle veh = (Vehicle)other.GetComponent(typeof(Vehicle));
		if (
            (bool)veh &&
            veh.isIt == 1 &&
            veh.isResponding &&
            (Time.time - lastTag >= 3f) &&
            (Time.time - veh.lastTag >= 3f))
		{
			lastTag = Time.time;
			GetComponent<NetworkView>().RPC("sQ", RPCMode.All, 1);
		}
	}

	public void OnLaserHit(bool isFatal)
	{
		if (
            isFatal &&
            Game.Settings.lasersFatal &&
            Vector3.Distance(
                transform.position,
                World.baseTF.position) > 10f)
		{
			myRigidbody.isKinematic = true;
			GetComponent<NetworkView>().RPC("lR", RPCMode.All);
		}
	}

	[RPC]
	public IEnumerator lR()
	{
        if (
            Time.time - lastReset < 3f ||
            !myRigidbody ||
            !World.baseTF)
        {
            yield break; //We are already resetting...
        }
        lastReset = Time.time;
        if (isPlayer || isBot)
        {
            myRigidbody.isKinematic = true;
        }
        Game.Controller.mE(transform.position);
        Game.Controller.mE(World.baseTF.position);
        ramoSphereScale = 0.01f;
        yield return new WaitForSeconds(2f);
        if (ramoSphere) Destroy(ramoSphere);
        if (isPlayer || isBot)
        {
            transform.position = World.baseTF.position;
            myRigidbody.isKinematic = false;
        }
        OnPrefsUpdated(); //Rebuild a new ramosphere
	}

	[RPC]
	public void fR(
        NetworkViewID LaunchedByViewID,
        string id,
        Vector3 pos,
        Vector3 ang,
        NetworkMessageInfo info)
	{
        GameObject btemp = (GameObject)Instantiate(
            Game.objectRocket,
            pos,
            Quaternion.Euler(ang));
        Rocket r = (Rocket)btemp.GetComponent(typeof(Rocket));
		r.laserID = id;
		if (!info.networkView.isMine)
		{
			r.lag = (float)(Network.time - info.timestamp);
		}
		r.launchVehicle = this;
	}

	[RPC]
	public void fS(
        NetworkViewID LaunchedByViewID,
        string id,
        Vector3 pos,
        Vector3 ang,
        NetworkMessageInfo info)
	{
        GameObject btemp = (GameObject)Instantiate(
            Game.objectRocketSnipe,
            pos,
            Quaternion.Euler(ang));
        Rocket r = (Rocket)btemp.GetComponent(typeof(Rocket));
		r.laserID = id;
		if (!info.networkView.isMine)
		{
			r.lag = (float)(Network.time - info.timestamp);
		}
		r.launchVehicle = this;
	}

	[RPC]
	public void fRl(
        NetworkViewID LaunchedByViewID,
        string id,
        Vector3 pos,
        NetworkViewID targetViewID,
        NetworkMessageInfo info)
	{
		GameObject btemp = (GameObject)Instantiate(
            Game.objectRocket,
            pos,
            Quaternion.identity);
        Rocket r = (Rocket)btemp.GetComponent(typeof(Rocket));
		r.laserID = id;
		if (!info.networkView.isMine)
		{
			r.lag = (float)(Network.time - info.timestamp);
		}
		r.launchVehicle = this;
        foreach (DictionaryEntry plrE in Game.Players)
        {
            if (((Vehicle)plrE.Value).GetComponent<NetworkView>().viewID == targetViewID)
            {
                r.targetVehicle = (Vehicle)plrE.Value;
                break;
            }
        }
	}

	[RPC]
	public void fSl(
        NetworkViewID LaunchedByViewID,
        string id,
        Vector3 pos,
        NetworkViewID targetViewID,
        NetworkMessageInfo info)
	{
        GameObject btemp = (GameObject)Instantiate(
            Game.objectRocketSnipe,
            pos,
            Quaternion.identity);
        Rocket r = (Rocket)btemp.GetComponent(typeof(Rocket));
		r.laserID = id;
		if (!info.networkView.isMine)
		{
			r.lag = (float)(Network.time - info.timestamp);
		}
		r.launchVehicle = this;
        foreach (DictionaryEntry plrE in Game.Players)
        {
            if (((Vehicle)plrE.Value).GetComponent<NetworkView>().viewID == targetViewID)
            {
                r.targetVehicle = (Vehicle)plrE.Value;
                break;
            }
        }
	}

	[RPC]
	public void lH(string n, Vector3 pos)
	{
		GameObject go = GameObject.Find("lsr#" + n);
		if ((bool)go)
		{
            ((Rocket)go.GetComponent(typeof(Rocket))).laserHit(
                gameObject,
                transform.TransformPoint(pos),
                Vector3.up);
		}
	}

	[RPC]
	public void sP(Vector3 pos, Quaternion rot, NetworkMessageInfo info)
	{
		if (!vehicleNet)
		{
			return;
		}
		if (GetComponent<NetworkView>().stateSynchronization != NetworkStateSynchronization.Off)
		{
			Debug.Log("sP NvN: " + gameObject.name);
			return;
		}
		vehicleNet.rpcPing = (float)(Network.time - info.timestamp);

		if (
            vehicleNet.states[0] != null &&
            ((double)vehicleNet.states[0].t >= info.timestamp))
		{
			Debug.Log("sP OoO: " +
                vehicleNet.states[0].t +
                " * " +
                Time.time);
			return;
		}

		for (int k = vehicleNet.states.Length - 1; k > 0; k--)
		{
            vehicleNet.states[k] = vehicleNet.states[k - 1];
		}
		vehicleNet.states[0] = new State(
            pos,
            rot,
            (float)info.timestamp,
            0f,
            0f);
		
        float png = (float)(Network.time - (double)vehicleNet.states[0].t);
		vehicleNet.jitter = Mathf.Lerp(
            vehicleNet.jitter,
            Mathf.Abs(vehicleNet.ping - png),
            1f / Network.sendRate);
		vehicleNet.ping = Mathf.Lerp(
            vehicleNet.ping,
            png,
            1f / Network.sendRate);
	}

	[RPC]
	public void sT(float time, NetworkMessageInfo info)
	{
        if (!(bool)vehicleNet && !GetComponent<NetworkView>().isMine) return;

        //We are recieving the ping back
        //Debug.Log(name + time + networkView.viewID);

        if (time > 0f)
        {
            vehicleNet.calcPing = Mathf.Lerp(
                vehicleNet.calcPing,
                (Time.time - vehicleNet.lastPing) /
                (float)(vehicleNet.wePinged ? 1 : 2),
                0.5f);
            vehicleNet.wePinged = false;
        }

        //We are authoratative instance, and are being "pinged".
        else if (GetComponent<NetworkView>().isMine)
        {
            GetComponent<NetworkView>().RPC("sT", RPCMode.Others, 1f);
        }
        else
        {
            vehicleNet.lastPing = Time.time;
            vehicleNet.wePinged = ((info.sender == Network.player) ?
                true :
                false);
        }
	}

	[RPC]
	public void s4(int x, int y, int z, int w)
	{
		input = new Vector4(x / 10, y / 10, z / 10, w / 10);
	}

	[RPC]
	public void sI(bool input)
	{
		specialInput = input;
		gameObject.BroadcastMessage(
            "OnSetSpecialInput",
            SendMessageOptions.DontRequireReceiver);
	}

	[RPC]
	public void sB(bool input)
	{
		brakes = input;
	}

	[RPC]
	public void sZ(bool input)
	{
		zorbBall = input;
		StartCoroutine_Auto(OnPrefsUpdated());
	}

	[RPC]
	public void sQ(int mode)
	{
        foreach (DictionaryEntry plrE in Game.Players)
        {
            ((Vehicle)plrE.Value).isIt = 0;
            ((Vehicle)plrE.Value).setColor();
        }
		isIt = 1;
		Game.QuarryVeh = this;
		setColor();
		switch (mode)
		{
		case 1:
			Game.Controller.msg(
                gameObject.name + " rammed the Quarry",
                (int)chatOrigins.Server);
			break;
		case 2:
			Game.Controller.msg(
                gameObject.name + " is now the Quarry",
                (int)chatOrigins.Server);
			break;
		case 3:
			Game.Controller.msg(
                gameObject.name + " Defaulted to Quarry",
                (int)chatOrigins.Server);
			break;
		default:
			return;
		}
		lastTag = Time.time;
	}

	[RPC]
	public void iS(string name)
	{
		score++;
		Game.Controller.msg(
            gameObject.name + " Got  " + name,
            (int)chatOrigins.Server);
	}

	[RPC]
	public void dS(string name)
	{
		score--;
	}

	[RPC]
	public void iT()
	{
		scoreTime++;
	}

	[RPC]
	public void sS(int s)
	{
		score = s;
	}

	[RPC]
	public void sC(
        float cR,
        float cG,
        float cB,
        float aR,
        float aG,
        float aB)
	{
		vehicleColor.r = cR;
		vehicleColor.g = cG;
		vehicleColor.b = cB;
		vehicleAccent.r = aR;
		vehicleAccent.g = aG;
		vehicleAccent.b = aB;
		updateColor = true;
	}

	public void setColor()
	{
		updateColor = true;
	}

	[RPC]
	public void dN(int rsn)
	{
		netKillMode = rsn;
	}
}
