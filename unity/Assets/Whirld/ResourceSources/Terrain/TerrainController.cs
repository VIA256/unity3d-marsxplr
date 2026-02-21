using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainControllerData
{
    public float[, ,] alphaMap;

    public float[,] heightMap;
}

[Serializable]
public class TerrainController : MonoBehaviour
{
    public WhirldObject whirldObject;
    public Terrain trn;
    public TerrainData trnDat;
    public float updateTime = -1;
    public float seaLevel;

    public TerrainControllerData dat;

    public void OnSceneGenerated()
    {
        if (!trnDat) Destroy(this);
        trn = (Terrain)GetComponent(typeof(Terrain));

        //Find WhirldObject
        whirldObject = (WhirldObject)gameObject.GetComponent(typeof(WhirldObject));
        if (!whirldObject ||
            whirldObject.parameters == null ||
            whirldObject.parameters["SeaFloorTexture"] == null)
        {
            Destroy(this);
            return;
        }
        whirldObject.Activate();    //Just in case it hasn't yet activated...

        //Init SplatMap Array
        SplatPrototype[] splatsOld = trnDat.splatPrototypes;
        SplatPrototype[] splatsNew = new SplatPrototype[trnDat.splatPrototypes.Length + 1];

        //Copy Data to New SplatMap Array
        for (int i = 0; i < trnDat.splatPrototypes.Length; i++)
        {
            splatsNew[i] = splatsOld[i];
        }

        //Add SeaFloorTexture Splat Channel to TerrainData Object
        splatsNew[splatsNew.Length - 1] = new SplatPrototype();
        splatsNew[splatsNew.Length - 1].texture = (Texture2D)whirldObject.parameters["SeaFloorTexture"];
        splatsNew[splatsNew.Length - 1].tileSize = new Vector2(15.0f, 15.0f);
        trnDat.splatPrototypes = splatsNew;

        //Prepare to Regenerate SeaFloor AlphaMap Channel
        dat = new TerrainControllerData();
        dat.alphaMap = trnDat.GetAlphamaps(0,
            0,
            trnDat.alphamapResolution,
            trnDat.alphamapResolution);
        dat.heightMap = trnDat.GetHeights(0,
            0,
            trnDat.heightmapWidth,
            trnDat.heightmapHeight);

        ReSplat();
    }

    public IEnumerator OnPrefsUpdated()
    {
        if (!World.sea ||
            seaLevel == World.sea.position.y)
        {
            yield break;
        }

        if (updateTime == -1)
        {
            updateTime = Time.time + 3;
            while (Time.time < updateTime) yield return null;
            ReSplat();
        }
        else
        { 
            //We are just bumping the execution of this coroutine farther into the future
            updateTime = Time.time + 3;
        }
    }

    public void ReSplat()
    {
        updateTime = -1;
        if (World.sea) seaLevel = World.sea.position.y;
        else if (GameObject.Find("Sea"))
        {
            seaLevel = GameObject.Find("Sea").transform.position.y;
        }
        else return;

        //Create new 3D array to work with - JS nD array support is just about nonexistent...
        var alphaMap = trnDat.GetAlphamaps(0,
            0,
            trnDat.alphamapResolution,
            trnDat.alphamapResolution);

        //Synthesize SeaFloor channel based upon raycasts to check for water
        for (int y = 0; y < trnDat.alphamapResolution; y++)
        {
            for (int x = 0; x < trnDat.alphamapResolution; x++)
            { 
                //Check if this pixel is submerged
                bool submerged = false;
                float trnAlt = transform.position.y +
                    dat.heightMap[
                        (int)((float)x /
                            (float)trnDat.alphamapResolution *
                            (float)trnDat.heightmapWidth),
                        (int)((float)y /
                            (float)trnDat.alphamapResolution *
                            (float)trnDat.heightmapHeight)
                    ] * trnDat.size.y;
                if (trnAlt < seaLevel)
                {
                    submerged = true;
                    // /*UNUSED*/ float depth = seaLevel + trnAlt;
                }

                //Update AlphaMap Array
                for (int z = 0; z < trnDat.alphamapLayers; z++)
                {
                    if (submerged)
                    {
                        alphaMap[x, y, z] = (z == trnDat.alphamapLayers - 1 ?
                            1 :
                            0);
                    }
                    else
                    {
                        alphaMap[x, y, z] = (z == trnDat.alphamapLayers - 1 ?
                            0 :
                            dat.alphaMap[x, y, z]);
                    }
                }
            }
        }

        //Assign regenerated AlphaMap to Terrain
        trnDat.SetAlphamaps(0, 0, alphaMap);

        //Recalculate Basemap
        trn.terrainData = trnDat;
        //DRAGONHERE: This is not a function maybe? trnDat.SetBaseMapDirty();
        trnDat.RefreshPrototypes();
        trn.Flush();
    }
}