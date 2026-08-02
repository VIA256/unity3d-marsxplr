using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class VehicleBot : MonoBehaviour
{
	public Vehicle vehicle;

	private GameObject enemy;
	private int botMovement;
	private int botEnemySelection;
	private int enemyUpdateTime;
	private float rocketFireTime = 0.00f;

	public void Update()
	{
        //Enemy updates
		if (
            (float)enemyUpdateTime == 0f ||
            Time.time - 2f > (float)enemyUpdateTime)
		{
			enemyUpdateTime = (int)Time.time;

			if (botEnemySelection == 1)
			{
                float distance = Mathf.Infinity;
                foreach (DictionaryEntry plrE in Game.Players)
                {
                    if (!((Vehicle)plrE.Value).gameObject) continue;
                    GameObject go = ((Vehicle)plrE.Value).gameObject;
                    Vector3 diff = (go.transform.position - transform.position);
                    float curDistance = diff.sqrMagnitude;
                    if (curDistance < distance && go.name != name)
                    {
                        enemy = go;
                        distance = curDistance;
                    }
                }
			}
			else if (botEnemySelection == 2)
			{
                foreach (DictionaryEntry plrE in Game.Players)
                {
                    if (((Vehicle)plrE.Value).isIt != 0)
                    {
                        enemy = ((Vehicle)plrE.Value).gameObject;
                        break;
                    }
                }
			}
		}

        //Rabbit Hunt Mode
        //if (true)
        //{
            if (vehicle.isIt != 0)
            {
                botMovement = 1;
                botEnemySelection = 1;
            }
            else
            {
                botMovement = 2;
                botEnemySelection = 2;
            }
        //}

        //Tag Mode (UNUSED)
        /*else
        {
            if (vehicle.isIt == 0)
            {
                botMovement = 1;
            }
            else
            {
                botMovement = 2;
                botEnemySelection = 1;
            }
        }*/


		if (Game.Settings.botsCanDrive)
		{
            //Hiding movement
			if (botMovement == 1)
			{
                //Tank
				if (vehicle.vehId == 2)
				{
					vehicle.input.x = (Time.time % 2f == 0f ?
                        0f :
                        UnityEngine.Random.value * 2f - 1f);
					vehicle.input.y = 1f;
				}
                //All others
				else
				{
					vehicle.input.x = UnityEngine.Random.value * 2f - 1f;
					vehicle.input.y = 1f;
				}
			}

            //Persuing movement
			else if (botMovement == 2 && (bool)enemy)
			{
				float distance = Vector3.Distance(
                    enemy.transform.position,
                    vehicle.myRigidbody.position);
				float rotation = Quaternion.LookRotation(
                    enemy.transform.position -
                        vehicle.myRigidbody.position).eulerAngles.y -
                        transform.localRotation.eulerAngles.y;
				if (rotation > 180f)
				{
					rotation -= 360f;
				}
				if (rotation < -180f)
				{
					rotation += 360f;
				}

                //Buggy
				if (vehicle.vehId == 0)
				{
					rotation /= 20f;
					if (distance < 15f)
					{
                        vehicle.input.x = (Mathf.Abs(rotation) < 0.1f ?
                            0f :
                            Mathf.Clamp(rotation, -1f, 1f));
					}
					else
					{
                        vehicle.input.x = (Mathf.Abs(rotation) < 0.1f ?
                            0 :
                            Mathf.Clamp(rotation, -0.6f, 0.6f));
					}
					vehicle.input.y = 1f;
					vehicle.specialInput = false;
				}
                //Hovercraft
				else if (vehicle.vehId == 1)
				{
					rotation /= 15f;
					vehicle.input.x = (Mathf.Abs(rotation) < 0.1f ?
                        0f :
                        Mathf.Clamp(rotation, -1f, 1f));
					vehicle.input.y = 1f;
					vehicle.specialInput = false;
				}
                //Tank
				else if (vehicle.vehId == 2)
				{
					rotation /= 80f;
					vehicle.input.x = (Mathf.Abs(rotation) < 0.3f ?
                        0f :
                        Mathf.Clamp(rotation, -1f, 1f));
					vehicle.input.y = (Mathf.Abs(rotation) > 1f ?
                        0 :
                        1);
					vehicle.specialInput = false;
				}
				else if (vehicle.vehId == 3)
				{
					rotation /= 10f;
					vehicle.input.x = (Mathf.Abs(rotation) < 0.1f ?
                        0f :
                        Mathf.Clamp(rotation, -1f, 1f));
					vehicle.input.y = 1f;
					vehicle.input.z = Physics.Raycast(
                        transform.position,
                        Vector3.up * -1f,
                        4f) ? 1f : 0.3f;
					vehicle.specialInput = true;
				}
			}
		}
		else
		{
			vehicle.input.x = 0f;
			vehicle.input.y = 0f;
		}

        //Fire!
		if (
            (bool)enemy &&
            Game.Settings.botsCanFire &&
            Time.time - 1f > rocketFireTime &&
            !Physics.Linecast(
                transform.position,
                enemy.transform.position,
                1 << 8))
		{
			rocketFireTime = Time.time;
			GetComponent<NetworkView>().RPC(
                "fRl",
                RPCMode.All,
                GetComponent<NetworkView>().viewID,
                Network.time + "",
                vehicle.ridePos.position +
                    vehicle.transform.up * -0.1f,
                enemy.GetComponent<NetworkView>().viewID);
		}

        //Bounds Checking
		if (vehicle.myRigidbody.position.y < -300f)
		{
			vehicle.myRigidbody.velocity = vehicle.myRigidbody.velocity.normalized;
			vehicle.myRigidbody.isKinematic = true;
			vehicle.transform.position = World.baseTF.position;
			vehicle.myRigidbody.isKinematic = false;
			if ((bool)Game.Messaging)
			{
				Game.Messaging.broadcast(name + " fell off the planet");
			}
		}
	}

    public void FixedUpdate()
    {
        if (
            (bool)vehicle.ramoSphere &&
            vehicle.zorbBall &&
            (vehicle.input.y != 0f || vehicle.input.x != 0f))
        {
            GetComponent<Rigidbody>().AddForce(
                Vector3.Scale(
                    new Vector3(1f, 0f, 1f),
                    Camera.main.transform.TransformDirection(new Vector3(
                        vehicle.input.x * Mathf.Max(
                            0f,
                            Game.Settings.zorbSpeed + Game.Settings.zorbAgility),
                        0f,
                        vehicle.input.y * Game.Settings.zorbSpeed))),
                ForceMode.Acceleration);
            GetComponent<Rigidbody>().AddTorque(
                Camera.main.transform.TransformDirection(new Vector3(
                    vehicle.input.y,
                    0f,
                    vehicle.input.x * -1f)) * Game.Settings.zorbSpeed,
                ForceMode.Acceleration);
        }
    }
}
