using System;
using UnityEngine;

[Serializable]
public class CameraVehicle : MonoBehaviour
{
	public GUILayer guiLayer;

	public Transform arrow;
	public Vector3 lastDir;
	public float lastY;
	public float sensitivityX;
	public float sensitivityY;
	public float rotationX;
	public float rotationY;
	public float heightBoost;
	public float targetHeight;
	public Quaternion gyroTation;
	public Quaternion wr;
	// /*unused*/ private bool underwater;
	public RaycastHit hit;
	public MotionBlur mb;
	public GlowEffect glowEffect;
	public ColorCorrectionEffect colorEffect;
	public float worldTime;
    public GameObject camTarget;
    public Vehicle camTargetVeh;

	public CameraVehicle()
    {
		lastDir = new Vector3(1f, 1f, 1f);
		sensitivityX = 15f;
		sensitivityY = 15f;
		rotationX = 10f;
		rotationY = 10f;
		//underwater = false;
		worldTime = 0f;
	}

	public void Start()
    {
		Time.timeScale = 1f;
		Quaternion trot = transform.rotation;
		Vector3 ea = trot.eulerAngles;
		ea.x = 270.01f;
		trot.eulerAngles = ea;
		transform.rotation = trot;
	}

	public void LateUpdate()
    {
		if (
			!Game.Player ||
			!Game.PlayerVeh ||
			!Game.PlayerVeh.ridePos
		)
        {
			return;
		}

        //QuarryCam
        camTarget = Game.Player;
        camTargetVeh = Game.PlayerVeh;
        if (
            Game.Settings.quarryCam &&
            (bool)Game.QuarryVeh &&
            (bool)Game.QuarryVeh.ridePos)
        {
            camTarget = Game.QuarryVeh.gameObject;
            camTargetVeh = Game.QuarryVeh;
        }

		//Blur
		if (mb.enabled)
        {
			mb.blurAmount -= Time.deltaTime * 0.13f;
			if (mb.blurAmount < 0f)
            {
				mb.enabled = false;
			}
		}

		//Audio
		if (Game.Settings.useHypersound == 1)
        {
			Game.Settings.gameMusic.pitch = Mathf.Clamp(
				-0.5f + camTarget.GetComponent<Rigidbody>().velocity.magnitude / 15f,
				0.8f,
				1.5f
			);
		}

		//Cursor Locking
		if (
			Game.Settings.camMode == 0 ||
			Input.GetButton("Fire2") ||
			(Input.GetButton("Snipe") && !Game.Messaging.chatting)
		)
        {
			Screen.lockCursor = true;
		}
		else
        {
			Screen.lockCursor = false;
		}

		//Snipe Zooming
		if (Input.GetButton("Snipe") && !Game.Messaging.chatting)
        {
			if (Camera.main.fieldOfView == 60f)
            {
				Camera.main.fieldOfView = 10f;
			}
			Camera.main.fieldOfView = Camera.main.fieldOfView + Input.GetAxis("Mouse ScrollWheel") * -1f;
			if (Camera.main.fieldOfView > 50f)
            {
				Camera.main.fieldOfView = 50f;
			}
			else if (Camera.main.fieldOfView < 1.5f)
            {
				Camera.main.fieldOfView = 1.5f;
			}
		}
		else if (Camera.main.fieldOfView != 60f)
        {
			Camera.main.fieldOfView = 60f;
		}

		//Green Arrow
		Vector3 targetPos = transform.InverseTransformPoint(GetComponent<Camera>().ScreenToWorldPoint(new Vector3(
			GetComponent<Camera>().pixelWidth - 30f - 65f,
			30f,
			GetComponent<Camera>().nearClipPlane + 0.05f
		)));
		if (Vector3.Distance(targetPos, arrow.localPosition) < 0.002f)
        {
			arrow.localPosition = Vector3.Lerp(
				arrow.localPosition,
				targetPos,
				Time.deltaTime * 0.005f
			);
		}
		else
        {
			arrow.localPosition = targetPos;
		}
		arrow.rotation = Quaternion.Lerp(
			arrow.rotation, 
			Quaternion.LookRotation(
				((camTargetVeh.isIt != 0 || !Game.QuarryVeh) ?
					World.baseTF :
					Game.QuarryVeh.gameObject.transform
				).position - camTarget.transform.position
			),
			Time.deltaTime * 15f
		);
		arrow.localScale = Vector3.one * Mathf.Lerp(
			0.03f,
			1f,
			Camera.main.fieldOfView / 60f
		);

		//HotKeys
		if (Input.GetKeyDown(KeyCode.Escape) && Game.Settings.camMode == 0)
        { // Unlock Cursor
			Game.Settings.camMode = 1;
			PlayerPrefs.SetInt("cam", 1);
		}
		if (!Game.Messaging.chatting)
        {
			if (Input.GetKeyDown(KeyCode.Alpha1))
            {
				Game.Settings.camMode = 0;
				PlayerPrefs.SetInt("cam", 0);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
				Game.Settings.camMode = 1;
				PlayerPrefs.SetInt("cam", 1);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
				Game.Settings.camDist = 0f;
				PlayerPrefs.SetFloat("camDist", 0f);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
				Game.Settings.camDist = 20f;
				PlayerPrefs.SetFloat("camDist", 20f);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
				Game.Settings.camMode = 2;
				PlayerPrefs.SetInt("cam", 2);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
				Game.Settings.camMode = 3;
				PlayerPrefs.SetInt("cam", 3);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
				Game.Settings.gyroCam = (Game.Settings.gyroCam ? false : true);
				PlayerPrefs.SetInt("gyroCam", (Game.Settings.gyroCam ? 1 : 0));
			}
		}

		//Constants
		float camDist = (float)camTargetVeh.camOffset + Game.Settings.camDist;

		//World Entry Effect
		if (worldTime < 7f)
        {
			worldTime = Time.time - Game.Controller.worldLoadTime;
			transform.position = Vector3.Lerp(
				transform.position,
				camTarget.transform.position,
				Time.deltaTime * 1f
			);
			wr = Quaternion.LookRotation(
				camTarget.transform.position - transform.position,
				Vector3.up
			);
			if (worldTime > 1f)
            {
				transform.rotation = Quaternion.Slerp(
					transform.rotation,
					wr,
					Time.deltaTime * 0.5f * Mathf.Min(1f, (worldTime - 1f) * 0.5f)
				);
			}
		}

		//Ride
		else if (
			Game.Settings.camMode == 0 ||
			Input.GetButton("Fire2") ||
			(Input.GetButton("Snipe") && !Game.Messaging.chatting)
		)
        {
			transform.position = camTargetVeh.ridePos.position;
			
			if (Input.GetButtonDown("Fire2") || Input.GetKeyDown(KeyCode.Alpha1))
            {
				rotationX = 0f;
				rotationY = 0f;
				if (Game.Settings.gyroCam)
                {
					gyroTation = Quaternion.Euler(
						0f,
						camTargetVeh.ridePos.rotation.eulerAngles.y,
						0f
					);
				}
			}
			
			if (Game.Settings.gyroCam)
            {
				transform.rotation = gyroTation;
			}
			else
            {
				transform.rotation = camTargetVeh.ridePos.rotation;
			}
			
			rotationX += Input.GetAxis("Mouse X") * (Input.GetButton("Snipe") ? 0.5f : 2f);
			rotationY += Input.GetAxis("Mouse Y") * (Input.GetButton("Snipe") ? 0.5f : 2f);
			
			if (rotationX < -360f)
            {
				rotationX += 360f;
			}
			else if (rotationX > 360f)
            {
				rotationX -= 360f;
			}
			if (rotationY < -360f)
            {
				rotationY += 360f;
			}
			else if (rotationY > 360f)
            {
				rotationY -= 360f;
			}
			
			rotationX = Mathf.Clamp(rotationX, -360f, 360f);
			if (Game.Settings.gyroCam)
            {
				rotationY = Mathf.Clamp(rotationY, -100f, 100f);
			}
			else
            {
				rotationY = Mathf.Clamp(rotationY, -20f, 100f);
			}
			transform.localRotation *= Quaternion.AngleAxis(rotationX, Vector3.up);
			transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);
		}

		//Chase
		else if (Game.Settings.camMode == 1)
        {
		
			//Smooth
			if (Game.Settings.camChase == 0)
            {
				transform.position = Vector3.Lerp(
					transform.position,
					camTarget.transform.position - Vector3.Normalize(
						camTarget.transform.position - transform.position
					) * camDist + Vector3.one * (camTargetVeh.camSmooth ?
						0f :
						Mathf.Lerp(0f, 15f, camDist / 30f)
					),
					Time.deltaTime * 3.5f
				);
				transform.rotation = Quaternion.Slerp(
					transform.rotation,
					Quaternion.LookRotation(
						camTarget.transform.position - transform.position,
						(Game.Settings.flightCam ?
							camTarget.transform.up :
							Vector3.up
						)
					),
					Time.deltaTime * 4f
				);
				if (Physics.Linecast(
					transform.position + Vector3.up * 50f,
					transform.position + Vector3.down * 1f, out hit,
					1 << 8
				) && 
                    hit.collider.GetType() != typeof(TerrainCollider)
				)
                {
					Vector3 tpos = transform.position;
					float y = tpos.y + 51 - hit.distance;
					tpos.y = y;
					transform.position = tpos;
				}
			}

			//Agile
			else if (Game.Settings.camChase == 1)
            {
				if (
					(bool)camTarget.transform.gameObject.GetComponent<Rigidbody>() &&
					camTarget.transform.gameObject.GetComponent<Rigidbody>().velocity.sqrMagnitude > 0.1f &&
					camTarget.transform.gameObject.GetComponent<Rigidbody>().velocity.normalized.y < 0.8f &&
					camTarget.transform.gameObject.GetComponent<Rigidbody>().velocity.normalized.y > -0.8f
				)
                {
					lastDir = Vector3.Lerp(
						lastDir,
						camTarget.transform.gameObject.GetComponent<Rigidbody>().velocity.normalized,
						0.1f
					);
				}
				else
                {
					Vector3.Lerp(
						lastDir,
						new Vector3(lastDir.x, 0f, lastDir.z),
						0.1f
					);
				}
				Vector3 newPos = (
					camTarget.transform.position + 
					lastDir * -(camDist) + 
					Vector3.up * (camDist / 3f)
				);
				Vector3 tpos = transform.position;
				float y = tpos.y + (camTarget.transform.position.y - lastY) * Time.deltaTime;
				tpos.y = y;
				transform.position = tpos;
				transform.position = Vector3.Lerp(
					transform.position,
					newPos,
					Time.deltaTime * 4f
				);
				lastY = camTarget.transform.position.y;
				transform.rotation = Quaternion.Slerp(
					transform.rotation,
					Quaternion.LookRotation(
						camTarget.transform.position - transform.position,
						(Game.Settings.flightCam ?
							camTarget.transform.up :
							Vector3.up
						)
					),
					Time.deltaTime * 4f
				);
				if (
					Physics.Linecast(
						transform.position + Vector3.up * 50f,
						transform.position + Vector3.down,
						out hit,
						1 << 8
					) &&
					hit.collider.GetType() != typeof(TerrainCollider)
				)
                {
					Vector3 trapos = transform.position;
					float vecy = trapos.y + 51 - hit.distance;
					trapos.y = vecy;
					transform.position = trapos;
				}
			}

			//Arcade
			else if (
				Game.Settings.camChase == 2 &&
				camTarget.transform.GetComponent<Rigidbody>().velocity.magnitude > 0f
			)
            {
				float heightDamping = 3f;
				float rotationDamping = 3f;
				float wantedRotationAngle = Quaternion.LookRotation(
					camTarget.transform.GetComponent<Rigidbody>().velocity
				).eulerAngles.y;
				wantedRotationAngle += Mathf.Lerp(
					30f,
					10f,
					camDist / 30f
				) * Input.GetAxis("Horizontal");
				float wantedHeight = (
					camTarget.transform.position.y +
					Mathf.Lerp(0.1f, 15f, camDist / 30f) +
					heightBoost
				);
				targetHeight = Mathf.Lerp(0.5f, 3f, camDist / 30f);
				
				float currentRotationAngle = transform.eulerAngles.y;
				float currentHeight = transform.position.y;
				currentRotationAngle = Mathf.LerpAngle(
					currentRotationAngle,
					wantedRotationAngle,
					rotationDamping * Time.deltaTime
				);
				currentHeight = Mathf.Lerp(
					currentHeight,
					wantedHeight,
					heightDamping * Time.deltaTime
				);
				Quaternion currentRotation = Quaternion.Euler(
					0f,
					currentRotationAngle,
					0f
				);
				Vector3 pos = camTarget.transform.position;
				pos.y += targetHeight; //Look ABOVE the target
				transform.position = pos;
				transform.position -= currentRotation * Vector3.forward * camDist;
				Vector3 tpos = transform.position;
				tpos.y = currentHeight;
				transform.position = tpos;

				//Terrain Avoidance
				RaycastHit hit = default(RaycastHit);
				if (
					Physics.Raycast(
						transform.position + Vector3.up * 49.4f,
						Vector3.down,
						out hit,
						50f,
						1 << 8
					) &&
					hit.collider.GetType() != typeof(TerrainCollider)
				)
                { //We are under terrain
					Physics.Linecast( //Determine how far forward we need to go to be out of it
						transform.position,
						camTarget.transform.position + Vector3.up * currentHeight,
						out hit,
						1 << 8
					);
					transform.position += currentRotation * Vector3.forward * hit.distance;
					heightBoost = hit.distance * 0.7f;
				}
				else
                {
					heightBoost = 0f;
				}
				
				transform.LookAt(pos);
			}
		}

		//Soar
		else if (Game.Settings.camMode == 2)
        {
			transform.position = Vector3.Lerp(
				transform.position,
				camTarget.transform.position + Vector3.up * 40f,
				Time.deltaTime * 0.3f
			);
			wr = Quaternion.LookRotation(
				camTarget.transform.position - transform.position,
				Vector3.up
			);
			transform.rotation = Quaternion.Slerp(
				transform.rotation,
				wr,
				Time.deltaTime * 2f
			);
		}

		//Spectate
		else if (Game.Settings.camMode == 3)
        {
			transform.rotation = Quaternion.Slerp(
				transform.rotation,
				Quaternion.LookRotation(camTarget.transform.position - transform.position),
				Time.deltaTime * 1.5f
			);
			transform.Translate(new Vector3(
				Input.GetAxis("camX") * Time.deltaTime * 50f,
				Input.GetAxis("camY") * Time.deltaTime * 40f,
				Input.GetAxis("camZ") * Time.deltaTime * 50f
			));
		}

		//Roam
		else if (Game.Settings.camMode == 4)
        {
			Vector3 tpos = transform.position;
			tpos.x += Input.GetAxis("camX") * Time.deltaTime * 50;
			tpos.y += Input.GetAxis("camY") * Time.deltaTime * 50;
			tpos.z += Input.GetAxis("camZ") * Time.deltaTime * 50;
			transform.position = tpos;
			rotationX += Input.GetAxis("Mouse X") * sensitivityX;
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
			transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);
		}
	}

	public void OnPreRender()
    {
		if (Game.Settings.fogColor == Color.clear)
        {
			//Game has just initialized - don't start adding effects yet...
			return;
		}
		
		if (
			transform.position.y < Game.Settings.lavaAlt ||
			Physics.Raycast(
				transform.position + Vector3.up * 200f,
				Vector3.up * -1f,
				200f,
				1 << 4
			)
		)
        {
			RenderSettings.fogColor = World.seaFogColor;
			RenderSettings.fogDensity = Game.Settings.lavaFog;
			glowEffect.enabled = Game.Settings.renderLevel > 2;
			glowEffect.glowTint = World.seaGlowColor;
			Camera.main.clearFlags = CameraClearFlags.SolidColor;
		}
		else
        {
			RenderSettings.fogColor = Game.Settings.fogColor;
			RenderSettings.fogDensity = Game.Settings.worldFog;
			glowEffect.enabled = false;
			Camera.main.clearFlags = ((Camera.main.farClipPlane > 2000f) ?
				CameraClearFlags.Skybox :
				CameraClearFlags.SolidColor
			);
		}
		
		colorEffect.enabled = glowEffect.enabled;
		Camera.main.backgroundColor = RenderSettings.fogColor;
	}
}
