using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[Serializable]
[RequireComponent(typeof(Camera))]
public class MiniMap : MonoBehaviour
{
	public float camHeight = 650;
	public Terrain[] terrains;

	private bool revertFogState = false;
    // /*UNUSED*/ private bool terrainLighting = false; //DRAGONHERE: TerrainLighting depricated, rewrite this code to use new QualitySettings system
	private int terrainLOD = 0;
	private float terrainTreeDistance = 0;
	private float terrainDetailDistance = 0;
	// /*UNUSED*/ private float terrainBasemapDistance = 0;

	public void Update()
	{
		if (
            !Game.Settings.minimapAllowed ||
            !Game.Settings.useMinimap ||
            Game.Controller.loadingWorld ||
            !Game.Player)
		{
			camera.enabled = false;
			return;
		}
		camera.enabled = true;

		transform.position = Camera.main.transform.position;

		Vector3 tpos = transform.position;
        tpos.y = Game.Player.transform.position.y + camHeight;
		transform.position = tpos;

		transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.Euler(90f, Game.Player.transform.eulerAngles.y, 0f),
            Time.deltaTime);
	}

	public void OnPreRender()
	{
		revertFogState = RenderSettings.fog;
		RenderSettings.fog = false;

        if (World.terrains != null && World.terrains.Length > 0)
        {
            //if(World.terrains[0].lighting == TerrainLighting.Pixel) terrainLightingEnabled = true;
            //else terrainLightingEnabled = false;
            //terrainBasemapDistance = World.terrains[0].basemapDistance;
            terrainLOD = World.terrains[0].heightmapMaximumLOD;
            terrainTreeDistance = World.terrains[0].treeDistance;
            terrainDetailDistance = World.terrains[0].detailObjectDistance;
            foreach (Terrain trn in World.terrains)
            { 
                //trn.basemapDistance = 1000;
                if (Game.Settings.renderLevel > 4)
                {
                    trn.heightmapMaximumLOD = 3;
                }
                else if (Game.Settings.renderLevel > 3)
                {
                    trn.heightmapMaximumLOD = 4;
                }
                else trn.heightmapMaximumLOD = 5;
                //trn.lighting = TerrainLighting.Lightmap;
                trn.treeDistance = 0;
                trn.detailObjectDistance = 0;
            }
        }
	}

	public void OnPostRender()
	{
		RenderSettings.fog = revertFogState;

        if (World.terrains != null)
        {
            foreach (Terrain trn in World.terrains)
            {
                trn.treeDistance = terrainTreeDistance;
                trn.detailObjectDistance = terrainDetailDistance;
                trn.heightmapMaximumLOD = terrainLOD;
            }
        }
	}

	public void OnGUI()
	{
        if (
            !Game.Settings.minimapAllowed ||
            !Game.Settings.useMinimap ||
            !Game.Player) return;
        camHeight = GUI.HorizontalSlider(
            new Rect(
                Screen.width * 0.01f + 25f,
                Screen.height - 20f - Screen.height * 0.001f,
                Screen.width * 0.25f - 50f, 20f),
            camHeight,
            200f,
            1300f);
	}
}
