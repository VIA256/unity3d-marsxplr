//********************************************************************************************************************************************
//*********************************** Whirld - by Aubrey Falconer ****************************************************************************
//**** http://AubreyFalconer.com **** http://web.archive.org/web/20120519040400/http://www.unifycommunity.com/wiki/index.php?title=Whirld ****
//********************************************************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
//using Ionic.Zlib;
using System.IO.Compression;
using UnityEngine;

[Serializable]
public enum WhirldInStatus
{
    Idle,
    Working,
    Success,
    WWWError,
    SyntaxError
}

[Serializable]
public class WhirldIn : System.Object
{
	public WhirldInStatus status = WhirldInStatus.Idle;
	public string statusTxt = "";
	public float progress = 0.00f;
	public string info = "";
	public string url = "";
	public string data;
	public GameObject world;
	public GameObject whirldBuffer;
	public string worldName = "World";
	public string urlPath;
	public Hashtable worldParams = new Hashtable();
	public Hashtable threads = new Hashtable();
	public int threadAssetBundles = 0;
	public int threadTextures = 0;
	public int maxThreads = 5;
	public List<AssetBundle> loadedAssetBundles = new List<AssetBundle>();
	public Hashtable objects = new Hashtable();
	public Hashtable textures = new Hashtable();
	public Hashtable meshMaterials = new Hashtable();
	public Hashtable meshMatLibs = new Hashtable();
    public MonoBehaviour monoBehaviour; //Needed for attaching Coroutines too
	public int readChr = 0;

	public void Load()
	{
		whirldBuffer = new GameObject("WhirldBuffer");
		monoBehaviour = (MonoBehaviour)whirldBuffer.AddComponent(typeof(MonoBehaviourScript));
		
        monoBehaviour.StartCoroutine(Generate());
	}

	public void Cleanup()
	{

        //We are still loading the world
		if ((bool)whirldBuffer && (bool)monoBehaviour)
		{
			monoBehaviour.StopAllCoroutines();
			GameObject.Destroy(whirldBuffer);
		}

        //Unload AssetBundles
		if (loadedAssetBundles.Count > 0)
		{
            foreach (AssetBundle ab in loadedAssetBundles)
            {
                ab.Unload(true);
            }
            loadedAssetBundles.Clear();
		}
	}

	public IEnumerator Generate()
	{

        status = WhirldInStatus.Working;

        if (url != "")
        { 
            
            //Download Whirld File
            statusTxt = "Downloading World Definition";
            info = "";
            urlPath = url.Substring(0, url.LastIndexOf("/") + 1);
            WWW www = new WWW(url);
            while (!www.isDone)
            {
                progress = www.progress;
                yield return new WaitForSeconds(0.1f);
            }
            progress = 1f;

            //Verify Successful Download
            if (www.error != null)
            {
                info =
                    "Failed to download Whirld definition file: " +
                    url +
                    " (" +
                    www.error +
                    ")\n";
                status = WhirldInStatus.WWWError;
                yield break;
            }
            data = www.text;

        }

        //Init
        readChr = 0;
        world = GameObject.Find("World");
        if (world) GameObject.Destroy(world);
        world = new GameObject("World");
        statusTxt = "Parsing World Definition";

        //Sanity Check
        if (
            data == null ||
            data.Length < 10 ||
            (data[0] != '[' && data[0] != '{'))
        {
            status = WhirldInStatus.SyntaxError;
            yield break;
        }

        //Read Whirld Headers
        String n = null;
        String v = null;
        while (true)
        {
            //Read next char
            char s = data[readChr];
            readChr++;

            //Incorrectly nested header []s
            if (readChr >= data.Length)
            {
                status = WhirldInStatus.SyntaxError;
                yield break;
            }

            //Ignore Newlines and Tabs
            else if (s == '\n' || s == '\t') continue;

            else if (s == '{') break;    //Finished reading headers
            else if (s == '[')          //Beginning new header
            {
                n = "";
                v = "";
            }

            //Header name read, read value
            else if (s == ':' && n == "")
            {
                n = v;
                v = "";
            }

            //Header ended
            else if (s == ']')
            {
                //[name] header
                if (n == "")
                {
                    n = v;
                    v = "";
                }

                //AssetBundle
                if (n == "ab") monoBehaviour.StartCoroutine_Auto(LoadAssetBundle(v));

                //StreamedScene
                if (n == "ss") monoBehaviour.StartCoroutine_Auto(LoadStreamedScene(v));

                //Skybox
                else if (n == "rndSkybox") monoBehaviour.StartCoroutine_Auto(LoadSkybox(v));

                //Texture
                else if (n == "txt") monoBehaviour.StartCoroutine_Auto(LoadTexture(v));

                //Mesh
                else if (n == "msh") monoBehaviour.StartCoroutine_Auto(LoadMesh(v));

                //Terrain
                else if (n == "trn") monoBehaviour.StartCoroutine_Auto(LoadTerrain(v));

                //Rendering Settings
                else if (
                    n == "rndFogColor" ||
                    n == "rndFogDensity" ||
                    n == "rndAmbientLight" ||
                    n == "rndHaloStrength" ||
                    n == "rndFlareStrength")
                {
                    String[] vS = v.Split(","[0]);
                    if (n == "rndFogColor")
                    {
                        RenderSettings.fogColor = new Color(
                            float.Parse(vS[0]),
                            float.Parse(vS[1]),
                            float.Parse(vS[2]),
                            1);
                    }
                    else if (n == "rndFogDensity")
                    {
                        RenderSettings.fogDensity = float.Parse(v);
                    }
                    else if (n == "rndAmbientLight")
                    {
                        RenderSettings.ambientLight = new Color(
                            float.Parse(vS[0]),
                            float.Parse(vS[1]),
                            float.Parse(vS[2]),
                            float.Parse(vS[3]));
                    }
                    else if (n == "rndHaloStrength")
                    {
                        RenderSettings.haloStrength = float.Parse(v);
                    }
                    else if (n == "rndFlareStrength")
                    {
                        RenderSettings.flareStrength = float.Parse(v);
                    }
                }

                //Arbitrary Data
                else worldParams.Add(n, v);

            }

            //Header char read
            else v += s;
        }

        statusTxt = "Downloading World Assets";

        //Wait for all "threads" to finish working
        while (threads.Count > 0)
        {
            yield return null;
        }

        //Generate World
        statusTxt = "Initializing World";
        ReadObject(world.transform);

        //Add TerrainControllers to Terrain objects
        foreach (Terrain trn in GameObject.FindObjectsOfType(typeof(Terrain)))
        {
            ((TerrainController)trn.gameObject.AddComponent(typeof(TerrainController))).trnDat = trn.terrainData;
        }

        //Cleanup
        GameObject.Destroy(whirldBuffer);

        //Send Scene Generation Notice to each object
        foreach (GameObject go in GameObject.FindObjectsOfType(typeof(GameObject)))
        {
            go.SendMessage(
                "OnSceneGenerated",
                SendMessageOptions.DontRequireReceiver);
        }

        //Success!
        status = WhirldInStatus.Success;
        statusTxt = "World Loaded Successfully";
        if (info != "")
        {
            Debug.Log("Whirld Loading Info: " + info);
        }
	}

    public IEnumerator LoadAssetBundle(string p)
    {
        threadAssetBundles++;

        while (threads.Count >= maxThreads) yield return null;  //Don't overwhelm the computer by doing too many things @ once

        //Presets
        String thread = System.IO.Path.GetFileNameWithoutExtension(p);
        threads.Add(thread, "");
        String url = p;

        //Download StreamedScene
        url = GetURL(url);
        WWW www = new WWW(url);
        while (!www.isDone)
        {
            threads[thread] = www.progress;
            yield return null;
        }
        if (www.error != null || !www.assetBundle)
        {
            if (!www.assetBundle) info +=
                 "Referenced file is not an AssetBundle: " +
                 url +
                 "\n";
            else info +=
                "Failed to download asset file: " +
                url +
                " (" +
                www.error +
                ")\n";
            threads.Remove(thread);
            threadAssetBundles--;
            yield break;
        }

        //Load AssetBundle
        threads[thread] = "Initializing Bundle";
        loadedAssetBundles.Add(www.assetBundle);

        //Success
        threads.Remove(thread);
        threadAssetBundles--;

    }

	public IEnumerator LoadStreamedScene(string p)
	{
        while (threads.Count >= maxThreads) yield return null;  //Don't overwhelm the computer by doing too many things @ once

        //Presets
        String thread = "SceneData";
        threads.Add(thread, "");
        String nme = "World";
        String url = "Whirld.unity3d";

        //Object Parameters
        if (p != "")    //[ss:sceneName,url]
        {
            String[] pS = p.Split(","[0]);
            if (pS[0] != null) nme = pS[0];
            if (pS[1] != null) url = pS[1];
        }

        //Download StreamedScene
        url = GetURL(url);
        WWW www = new WWW(url);
        while (!www.isDone)
        {
            threads[thread] = www.progress;
            yield return null;
        }
        if (www.error != null || !www.assetBundle)
        {
            if (!www.assetBundle) info +=
                 "StreamedScene file contains no scenes: " +
                 url +
                 "\n";
            else info +=
                "Failed to download asset file: " +
                url +
                " (" +
                www.error +
                ")\n";
            threads.Remove(thread);
            yield break;
        }

        //Wait for all AssetBundles to load
        threads[thread] = "Loading Asset Dependencies";
        while (threadAssetBundles > 0) yield return null;

        threads.Remove(thread);
        thread = "SceneInit";
        threads.Add(thread, "...");

        //Load StreamedScene
        AsyncOperation async = Application.LoadLevelAdditiveAsync(nme);
        float tme = Time.time;
        while (!async.isDone)
        {
            threads[thread] = (Time.time - tme) + "...";
            yield return null;
        }

        //Success
        loadedAssetBundles.Add(www.assetBundle);
        threads.Remove(thread);

	}

	public IEnumerator LoadTexture(string p)    //[txt:name,url,wrapMode,anisoLevel]
	{
        threadTextures++;

        //Don't overwhelm the computer by doing too many things @ once
        while (threads.Count >= maxThreads) yield return null;

        String[] vS = p.Split(","[0]);

        String thread = "Txt" +
            threadTextures +
            " - " +
            vS[0];
        threads.Add(thread, "");

        String url = GetURL(vS[1]);
        WWW www = new WWW(url);
        while (!www.isDone)
        {
            threads[thread] = www.progress;
            yield return null;
        }
        if (www.error != null)
        {
            info +=
                "Failed to download texture: " +
                url +
                " (" +
                www.error +
                ")\n";
            threads.Remove(thread);
            threadTextures--;
            yield break;
        }

        threads[thread] = "Initializing";
        //Texture2D txt = www.texture;
        Texture2D txt = new Texture2D(
            4,
            4,
            TextureFormat.DXT1,
            true);
        www.LoadImageIntoTexture(txt);
        txt.wrapMode = (
            (vS[2] == null || float.Parse(vS[2]) == 0f) ?
                TextureWrapMode.Clamp :
                TextureWrapMode.Repeat);
        txt.anisoLevel = (vS[3] != null ? int.Parse(vS[3]) : 1);
        txt.Apply(true);
        txt.Compress(true);
        textures.Add(vS[0], txt);

        threads.Remove(thread);
        threadTextures--;
	}

	public IEnumerator LoadMeshTexture(string url, string materialName)
	{
        threadTextures++;

        //Don't overwhelm the computer by doing too many things @ once
        while (threads.Count >= maxThreads) yield return null;
        String thread = "MshTxt" +
            threadTextures +
            " - " +
            materialName;
        threads.Add(thread, "");

        url = GetURL(url);
        WWW www = new WWW(url);
        while (!www.isDone)
        {
            threads[thread] = www.progress;
            yield return null;
        }
        if (www.error != null)
        {
            info +=
                "Failed to download mesh texture: " +
                url +
                " (" +
                www.error +
                ")\n";
            threads.Remove(thread);
            threadTextures--;
            yield break;
        }

        threads[thread] = "Initializing";

        Texture2D mshTxt = new Texture2D(
            4,
            4,
            TextureFormat.DXT1,
            true);
        www.LoadImageIntoTexture(mshTxt);
        mshTxt.wrapMode = TextureWrapMode.Repeat;
        mshTxt.Apply(true);
        mshTxt.Compress(true);
        ((Material)meshMaterials[materialName]).mainTexture = mshTxt;

        threads.Remove(thread);
        threadTextures--;
	}

	public IEnumerator LoadMesh(string v) //[msh:name,url]
	{
        Mesh msh = new Mesh();
        List<Vector3> verts = new List<Vector3>();
        List<Vector3> norms = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();
        List<List<int>> triangles = new List<List<int>>();
        List<Material> mats = new List<Material>();

        //Don't overwhelm the computer by doing too many things @ once
        while (threads.Count >= maxThreads) yield return null;

        //Init Thread
        String[] vS = v.Split(","[0]);
        String thread = vS[0];
        threads.Add(thread, "");

        //Download Mesh Object
        int hasCollider = (vS.Length > 2 ? int.Parse(vS[2]) : 0);
        WWW www = new WWW(GetURL(vS[1]));
        while (!www.isDone)
        {
            threads[thread] = www.progress;
            yield return null;
        }
        if (www.error != null)
        {
            info +=
                "Failed to download mesh: " +
                url +
                " (" +
                www.error +
                ")\n";
            threads.Remove(thread);
            yield break;
        }

        //Download All Textures Before Generating Mesh
        //threads[thread] = "Loading Textures";
        //while(threadTextures > 0) yield return null;

        //Uncompress as necessary...
        threads[thread] = "Decompressing";
        yield return null;  //Rebuild GUI as we may be working for a while
        int lastDot = vS[1].LastIndexOf(".");
        String data;
        if (vS[1].Substring(lastDot + 1) == "gz")
        {
            //data = GZipStream.UncompressString(www.bytes);
            
            GZipStream gz = new GZipStream(new MemoryStream(www.bytes), CompressionMode.Decompress);
            byte[] buf = new byte[www.bytes.Length];
            gz.Read(buf, 0, buf.Length);
            data = buf.ToString();

            vS[1] = vS[1].Substring(0, lastDot);
        }
        else data = www.text;

        threads[thread] = "Generating";

        lastDot = vS[1].LastIndexOf(".");
        String ext = vS[1].Substring(lastDot + 1);

        //Binary UnityMesh Object
        if (ext == "utm")
        {
            //MeshSerializer has been depricated - it's totally nonstandard, and it didn't support submeshes anyway
            //Mesh msh = MeshSerializer.ReadMesh(www.bytes);
        }

        //.obj File
        else if (ext == "obj")
        {
            float timer = Time.time + 0.1f;
            String[] file = data.Split("\n"[0]);
            foreach (String str in file)
            {
                if (str == "") continue;
                String[] l = str.Split(" "[0]);
                if (l[0] == "v")
                {
                    verts.Add(new Vector3(
                        -float.Parse(l[1]),
                        float.Parse(l[2]),
                        float.Parse(l[3])));
                }
                else if (l[0] == "vn")
                {
                    norms.Add(new Vector3(
                        float.Parse(l[1]),
                        float.Parse(l[2]),
                        float.Parse(l[3])));
                }
                else if (l[0] == "vt")
                {
                    uvs.Add(new Vector2(
                        float.Parse(l[1]),
                        float.Parse(l[2])));
                }
                else if (l[0] == "f")
                {
                    if (l.Length == 4)
                    {
                        tris.Add(int.Parse(l[2].Substring(
                            0,
                            l[2].IndexOf("/"))) - 1);
                        tris.Add(int.Parse(l[1].Substring(
                            0,
                            l[2].IndexOf("/"))) - 1);
                        tris.Add(int.Parse(l[3].Substring(
                            0,
                            l[2].IndexOf("/"))) - 1);
                    }
                    //Attempt to triangulate face - hardly works, could use better routine here...
                    else
                    {
                        int i;
                        for (i = 2; i < l.Length; i++)
                        {
                            tris.Add(int.Parse(l[i].Substring(
                                0,
                                l[i].IndexOf("/"))) - 1);
                            if (i % 2 == 0)
                            {
                                tris.Add(int.Parse(l[1].Substring(
                                0,
                                l[1].IndexOf("/"))) - 1);
                            }
                        }
                        while (tris.Count % 3 != 0)
                        {
                            tris.Add(int.Parse(l[i = 2].Substring(
                                0,
                                l[i - 2].IndexOf("/"))) - 1);
                        }
                    }
                }
                else if (l[0] == "usemtl")
                {
                    if (meshMaterials.ContainsKey(l[1]))
                    {
                        mats.Add((Material)meshMaterials[l[1]]);
                    }
                    else
                    {
                        info +=
                            "Mesh Material Missing: " +
                            l[1] +
                            "\n";
                        mats.Add(null);
                    }
                    if (tris.Count > 0)
                    {
                        triangles.Add(tris);
                        tris = new List<int>();
                    }
                }
                else if (l[0] == "mtllib") //Time to load a material library!
                {
                    if (!meshMatLibs.ContainsKey(l[1]))
                    {
                        //Only load a material library once, even if it is referenced by multiple meshes
                        meshMatLibs.Add(l[1], true);
                        www = new WWW(GetURL(l[1]));
                        while (!www.isDone)
                        {
                            threads[thread] =
                                "Downloading Material Library (" +
                                Mathf.RoundToInt(www.progress * 100) +
                                "%)";
                            //yield return null;
                        }
                        if (www.error != null)
                        {
                            info +=
                                "Mesh Material Library Undownloadable: " +
                                GetURL(l[1]) +
                                " (" +
                                www.error +
                                ")\n";
                        }
                        else
                        {
                            threads[thread] = "Initializing " + vS[0] + "";
                            //yield return null;
                            String[] meshlib = www.text.Split("\n"[0]);
                            Material curMat = null;
                            int offset = -1;
                            while (true)
                            {
                                offset = www.text.IndexOf("map_Ka", offset + 1);
                                if (offset == -1) break;
                            }
                            foreach (String meshline in meshlib)
                            {
                                String[] ml = meshline.Split(" "[0]);
                                if (ml[0] == "newmtl") //Beginning of new material
                                {
                                    if (curMat) //Save current material
                                    {
                                        meshMaterials.Add(curMat.name, curMat);
                                    }
                                    curMat = new Material(Shader.Find("VertexLit"));
                                    curMat.name = ml[1];
                                }
                                else if (ml[0] == "#Shader") //Set shader of current material
                                {
                                    String shdr = meshline.Substring(8).Replace("Diffuse", "VertexLit");
                                    if (shdr != "VertexLit" && shdr != "VertexLit Fast")
                                    {
                                        curMat.shader = Shader.Find(shdr);
                                    }
                                }
                                else if (ml[0] == "Ka") //Set color of current material
                                {
                                    curMat.color = new Color(
                                        float.Parse(ml[1]),
                                        float.Parse(ml[2]),
                                        float.Parse(ml[3]),
                                        1f);
                                }
                                else if (ml[0] == "Kd")
                                {
                                    curMat.SetColor("_Emission", new Color(
                                        float.Parse(ml[1]),
                                        float.Parse(ml[2]),
                                        float.Parse(ml[3]),
                                        1f));
                                }
                                else if (ml[0] == "Ks")
                                {
                                    curMat.SetColor("_SpecColor", new Color(
                                        float.Parse(ml[1]),
                                        float.Parse(ml[2]),
                                        float.Parse(ml[3]),
                                        1f));
                                }
                                else if (ml[0] == "Ns")
                                {
                                    curMat.SetFloat("_Shininess", float.Parse(ml[1]));
                                }
                                else if (ml[0] == "map_Ka") //Set texture of current material
                                {
                                    curMat.mainTextureOffset = new Vector2(
                                        float.Parse(ml[2]),
                                        float.Parse(ml[3]));
                                    curMat.mainTextureScale = new Vector2(
                                        float.Parse(ml[5]),
                                        float.Parse(ml[6]));
                                    monoBehaviour.StartCoroutine_Auto(LoadMeshTexture(
                                        ml[7],
                                        curMat.name));
                                }
                                else if (ml[0] == "d") //Set alpha cutoff of current material
                                {
                                    //curMat.shader = Shader.Find("Transparent/Cutout/VertexLit");
                                    //curMat.SetFloat("_Cutoff", float.Parse(ml[1]));
                                }
                            }
                            if (curMat) //Save last material (others get saved as file is read)
                            {
                                meshMaterials.Add(curMat.name, curMat);
                            }
                        }
                    }
                }
                if (Time.time > timer) //Refresh GUI 10 times per second to keep the user entertained
                {
                    timer = Time.time + 0.1f;
                    yield return null;
                }
            }

            threads[thread] = "Initializing";

            msh.vertices = verts.ToArray();
            msh.normals = norms.ToArray();
            msh.uv = uvs.ToArray();
            if (triangles.Count > 0)
            {
                triangles.Add(tris);
                msh.subMeshCount = triangles.Count;
                for (int i = 0; i < triangles.Count; i++)
                {
                    msh.SetTriangles(triangles[i].ToArray(), i);
                }
            }
            else msh.triangles = tris.ToArray();
        }

        //Unknown File Type
        else info +=
            "Mesh Type Unrecognized: " +
            vS[0] +
            " " +
            vS[1] +
            " (." +
            ext +
            ")\n";

        if (hasCollider == 1) //This mesh is being created, and it has a renderer
        {
            GameObject mshObj = new GameObject(vS[0]);
            mshObj.AddComponent(typeof(MeshFilter));
            ((MeshFilter)mshObj.GetComponent(typeof(MeshFilter))).mesh = msh;
            mshObj.AddComponent(typeof(MeshRenderer));
            ((MeshRenderer)mshObj.GetComponent(typeof(MeshRenderer))).materials = mats.ToArray();
            if (hasCollider != -1) //This mesh has a collider, and it is the same as it's rendered mesh
            {
                mshObj.AddComponent(typeof(MeshCollider));
                ((MeshCollider)mshObj.GetComponent(typeof(MeshCollider))).sharedMesh = msh;
            }
            if (msh.uv.Length < 1) TextureObject(mshObj);
            objects.Add(vS[0], mshObj);
            mshObj.transform.parent = whirldBuffer.transform;
        }
        else //This mesh has a custom collider
        {
            if (objects.ContainsKey(vS[0])) //This mesh already exists, add a custom collider to it
            {
                GameObject mshObj = new GameObject(vS[0]);
                mshObj.AddComponent(typeof(MeshCollider));
                ((MeshCollider)mshObj.GetComponent(typeof(MeshCollider))).sharedMesh = msh;
                objects.Add(vS[0], mshObj);
                mshObj.transform.parent = whirldBuffer.transform;
            }
        }
        msh.Optimize();

        threads.Remove(thread);

	}

    //v = "name;r:width,height,length,heightmapResolution	//,detailResolution,controlResolution,textureResolution;h:heightMapUrl";
	public IEnumerator LoadTerrain(string v)
	{
        String[] vS2 = v.Split(";"[0]);
        String tName = vS2[0];

        String[] tRes = null;
        String tHtmp = null;
        String tLtmp = null;
        String tSpmp = null;
        String tSpmp2 = null;
        String[] tTxts = null;
        // /*UNUSED*/ String tDtmp = null;

        for (int i2 = 1; i2 < vS2.Length; i2++)
        {
            String[] str = vS2[i2].Split(":"[0]);
            if (str[0] == "r") tRes = str[1].Split(","[0]);
            else if (str[0] == "h") tHtmp = GetURL(str[1]);
            else if (str[0] == "l") tLtmp = GetURL(str[1]);
            else if (str[0] == "s") tSpmp = GetURL(str[1]);
            else if (str[0] == "s2") tSpmp2 = GetURL(str[1]);
            else if (str[0] == "t") tTxts = str[1].Split(","[0]);
            //else if (str[0] == "d") tDtmp = GetURL(str[1]);
        }

        String thread = tName;
        threads.Add(thread, "");
        WWW www = new WWW(tHtmp);
        while (!www.isDone)
        {
            threads[thread] = www.progress;
            yield return null;
        }
        if (www.error != null)
        {
            info +=
                "Terrain Undownloadable: " +
                tName +
                " " +
                tHtmp +
                " (" +
                www.error +
                ")\n";
        }
        else
        {
            threads[thread] = "Initializing";
            //yield return null;

            int tWidth = int.Parse(tRes[0]);
            int tHeight = int.Parse(tRes[1]);
            int tLength = int.Parse(tRes[2]);
            int tHRes = int.Parse(tRes[3]);

            TerrainData trnDat = new TerrainData();

            //Heights
            trnDat.heightmapResolution = tHRes;
            float[,] hmap = trnDat.GetHeights(0, 0, tHRes, tHRes);
            System.IO.BinaryReader br;
            if (true) //Terrain RAW file is compressed
            {
                GZipStream gz = new GZipStream(new MemoryStream(www.bytes), CompressionMode.Decompress);
                byte[] buf = new byte[www.bytes.Length];
                gz.Read(buf, 0, buf.Length);

                br = new System.IO.BinaryReader(new System.IO.MemoryStream(
                    buf));
            }
            //else br = new System.IO.BinaryReader(new System.IO.MemoryStream(www.bytes));
            for (int x = 0; x < tHRes; x++)
            {
                for (int y = 0; y < tHRes; y++)
                {
                    hmap[x, y] = br.ReadUInt16() / 65535.00000000f;
                }
            }
            trnDat.SetHeights(0, 0, hmap);
            trnDat.size = new Vector3(tWidth, tHeight, tLength);

            //Textures
            SplatPrototype[] splatPrototypes = null;
            if (tTxts != null)
            {
                splatPrototypes = new SplatPrototype[tTxts.Length];
                for (int i = 0; i < tTxts.Length; i++)
                {
                    String[] splatTxt = tTxts[i].Split("="[0]);
                    String[] splatTxtSize = splatTxt[1].Split("x"[0]);
                    www = new WWW(GetURL(splatTxt[0]));
                    while (!www.isDone)
                    { 
                        //threads[thread] = "Initializing";
                        //yield return new WaitForSeconds(0.1f);
                    }
                    if (www.error != null)
                    {
                        info +=
                            "Terrain Texture Undownloadable: #" +
                            (i + 1) +
                            " (" +
                            splatTxt[0] +
                            ")\n";
                    }
                    else
                    { 
                        //yield return null;
                        splatPrototypes[i] = new SplatPrototype();
                        splatPrototypes[i].texture = new Texture2D(
                            4,
                            4,
                            TextureFormat.DXT1,
                            true);
                        www.LoadImageIntoTexture(splatPrototypes[i].texture);
                        splatPrototypes[i].texture.Apply(true);
                        splatPrototypes[i].texture.Compress(true);
                        splatPrototypes[i].tileSize = new Vector2(
                            int.Parse(splatTxtSize[0]),
                            int.Parse(splatTxtSize[1]));
                    }
                }
            }
            trnDat.splatPrototypes = splatPrototypes;

            //Lightmap
			/* DRAGONHERE: dont know how to set lightmaps (TerrainData.lightmap is not a real member)
            if (tLtmp != null)
            { 
                //whirld.statusTxt = "Downloading Terrain Lightmap (" + tName + ")";
                www = new WWW(tLtmp);
                while (!www.isDone)
                { 
                    //whirld.progress = www.progress;
                    //yield return new WaitForSeconds(0.1f);
                }
                if (www.error != null)
                {
                    info +=
                        "Terrain Lightmap Undownloadable: " +
                        tName +
                        " " +
                        tLtmp +
                        " (" +
                        www.error +
                        ")\n";
                }
                else
                {
                    trnDat.lightmap = www.texture;
                }
            }*/

            //Splatmap

            if (tSpmp != null)
            {
                Color[] mapColors2 = null;
                if (tSpmp2 != null)
                {
                    //whirld.statusTxt = "Downloading Augmentative Terrain Texturemap (" + tName + ")";
                    www = new WWW(tSpmp2);
                    while (!www.isDone)
                    { 
                        //whirld.progress = www.progress;
                        //yield return new WaitForSeconds(0.1f);
                    }
                    mapColors2 = www.texture.GetPixels();
                }
                //whirld.statusTxt = "Downloading Terrain Texturemap (" + tName + ")";
                www = new WWW(tSpmp);
                while (!www.isDone)
                {
                    //whirld.progress = www.progress;
                    //yield return new WaitForSeconds(0.1f);
                }
                //whirld.statusTxt = "Mapping Terrain Textures...";
                //yield return null;
                if (www.error != null)
                {
                    info +=
                        "Terrain Texturemap Undownloadable: " +
                        tName +
                        " " +
                        tLtmp +
                        " (" +
                        www.error +
                        ")\n";
                }
                else
                {
                    trnDat.alphamapResolution = www.texture.width;
                    float[, ,] splatmapData = trnDat.GetAlphamaps(
                        0,
                        0,
                        www.texture.width,
                        www.texture.width);
                    Color[] mapColors = www.texture.GetPixels();
                    int ht = www.texture.height;
                    int wd = www.texture.width;
                    for (int y = 0; y < ht; y++)
                    {
                        for (int x = 0; x < wd; x++)
                        {
                            for (int z = 0; z < trnDat.alphamapLayers; z++)
                            {
                                if (z < 4)
                                {
                                    splatmapData[x, y, z] = mapColors[x * wd + y][z];
                                }
                                else splatmapData[x, y, z] = mapColors2[x * wd + y][z - 4];
                            }
                        }
                    }
                    trnDat.SetAlphamaps(0, 0, splatmapData);
                }
            }

            //Go !
            GameObject trnObj = new GameObject(tName);
            trnObj.AddComponent(typeof(Terrain));
            ((Terrain)trnObj.GetComponent(typeof(Terrain))).terrainData = trnDat;
            trnObj.AddComponent(typeof(TerrainCollider));
            ((TerrainCollider)trnObj.GetComponent(typeof(TerrainCollider))).terrainData = trnDat;

            objects.Add(tName, trnObj);
            //Delete this temporary terrain object AFTER world is fully loaded
            trnObj.transform.parent = whirldBuffer.transform;
        }

        threads.Remove(thread);
	}

	public IEnumerator LoadSkyboxTexture(string url, int dest)
	{
        threadTextures++;

        //Don't overwhelm the computer by doing too many things @ once
        while (threads.Count >= maxThreads) yield return null;

        //Presets
        String thread = "Skybox" + dest;
        threads.Add(thread, "");


        //Download Skybox Image
        url = GetURL(url);
        WWW www = new WWW(url);
        while (!www.isDone)
        {
            threads[thread] = www.progress;
            yield return null;
        }

        threads.Remove(thread);
        threadTextures--;

        if (www.error != null)
        {
            info +=
                "Failed to download skybox # " +
                dest +
                ": " +
                url +
                " (" +
                www.error +
                ")\n";
            yield break; ;
        }

        Texture2D txt = new Texture2D(
            4,
            4,
            TextureFormat.DXT1,
            true);
        www.LoadImageIntoTexture(txt);
        txt.wrapMode = TextureWrapMode.Clamp;
        txt.Apply(true);
        txt.Compress(true);

        //Wait for everything else to load
        while (threads.Count > 0) yield return null;

        //Assign Texture to Skybox!
        if (dest == 0 || dest == 1)
        {
            RenderSettings.skybox.SetTexture("_FrontTex", txt);
        }
        if (dest == 0 || dest == 2)
        {
            RenderSettings.skybox.SetTexture("_BackTex", txt);
        }
        if (dest == 0 || dest == 3)
        {
            RenderSettings.skybox.SetTexture("_LeftTex", txt);
        }
        if (dest == 0 || dest == 4)
        {
            RenderSettings.skybox.SetTexture("_RightTex", txt);
        }
        if (dest == 0 || dest == 5)
        {
            RenderSettings.skybox.SetTexture("_UpTex", txt);
        }
        if (dest == 0 || dest == 6)
        {
            RenderSettings.skybox.SetTexture("_DownTex", txt);
        }
	}

	public IEnumerator LoadSkybox(string v)
	{
        String[] vS = v.Split(","[0]);
        //Multiple Image Skybox
        if (vS.Length > 5)
        {
            //Material skyMat = RenderSettings.skybox;
            //RenderSettings.skybox = new Material();
            //RenderSettings.skybox.CopyPropertiesFromMaterial(skymat);
            LoadSkyboxTexture(vS[0], 1);
            LoadSkyboxTexture(vS[1], 2);
            LoadSkyboxTexture(vS[2], 3);
            LoadSkyboxTexture(vS[3], 4);
            LoadSkyboxTexture(vS[4], 5);
            LoadSkyboxTexture(vS[5], 6);
            //Wait for everything else to load
            while (threads.Count > 0) yield return null;
            if (vS.Length > 6)
            {
                RenderSettings.skybox.SetColor("_Tint", new Color(
                    float.Parse(vS[6]),
                    float.Parse(vS[7]),
                    float.Parse(vS[8]),
                    0.5f));
            }
        }

        //Single JPG image for all sides
        else if (vS[0].Substring(vS[0].LastIndexOf(".") + 1) == "jpg")
        {
            LoadSkyboxTexture(vS[0], 0);
            //Wait for everything else to load
            while (threads.Count > 0) yield return null;
            if (vS.Length > 1)
            {
                RenderSettings.skybox.SetColor("_Tint", new Color(
                    float.Parse(vS[1]),
                    float.Parse(vS[2]),
                    float.Parse(vS[3]),
                    0.5f));
            }
        }

        //AssetBundle Material Skybox
        else
        {
            //Wait for everything else to load
            while (threads.Count > 0) yield return null;
			
			UnityEngine.Object sbasset = GetAsset(v);
			RenderSettings.skybox = (sbasset as Material) == null ?
				null :
				(Material)sbasset;
            if (!RenderSettings.skybox)
            {
                info +=
                    "Skybox not found: " +
                    v +
                    "\n";
            }
        }

	}

	public UnityEngine.Object GetAsset(string str)
	{
        if (loadedAssetBundles.Count > 0)
        {
            foreach (AssetBundle ab in loadedAssetBundles)
            {
                if (ab.Contains(str)) return ab.Load(str);
            }
        }
        return null;
	}

	public void ReadObject(Transform parent)
	{
		// /*UNUSED*/ string c = null;          //Character
		int i = 0;                              //Index of param
        string n = "";                          //Param name we are reading data for
        string v = "";                          //Value we are building
        List<String> d = new List<String>();    //Array of all values in current param data
        GameObject obj = null;                  //Object we have created

		GameObject goP = default(GameObject);
		WhirldObject whirldObject = default(WhirldObject);
		Light lightSource = default(Light);
		while (true)
		{
            if (readChr >= data.Length) return;

            //Get Char
			char s = data[readChr];

            //Ignore spaces
            if (s == ' ' || s == '\n' || s == '\r' || s == '\t') { ; }

            //Name fully read, begin collecting param value(s)
			else if (s == ':')
			{
				n = v;
				v = "";
			}

            //Move to next section of value
			else if (s == ',')
			{
				d.Add(v);
				v = "";
			}

            //Move to next section of value
			else if (s == '{')
			{
				readChr++;
				ReadObject(obj.transform);
                //Continue to next obj once the child "thread" we just launched has finished parsing objects at it's level
                continue;
			}

            //Assign current value to object, Begin reading new value
			else if (s == ';' || s == '}')
			{

                //Object name just read, create object
                if (!obj)
                {
                    if (objects.ContainsKey(v))
                    {
                        if (objects[v] != null)
                        {
                            goP = (GameObject)objects[v];
                        }
                        else
                        {
                            Debug.Log("Whirld: Objects[" + v + "] is null");
                        }
                        //else goP = gameObject.Find();
                    }
                    else
                    {
                        goP = (GameObject)Resources.Load(v);
                        if ((bool)goP) objects.Add(v, goP);
                    }
                    if ((bool)goP)
                    {
                        obj = (GameObject)GameObject.Instantiate(goP);
                        obj.name = v;
                    }
                    else
                    {
                        obj = new GameObject(v);
                        objects.Add(v, obj);
                    }
                    if (
                        obj.name != "Base" &&
                        obj.name != "Sea" &&
                        obj.name != "JumpPoint" &&
                        obj.name != "Light")
                    {
                        obj.transform.parent = parent;
                    }
                    whirldObject = (WhirldObject)obj.GetComponent(typeof(WhirldObject));
                    if ((bool)whirldObject)
                    {
                        whirldObject.parameters = new Hashtable();
                    }
                    lightSource = (Light)obj.GetComponent(typeof(Light));
                }

                //Object already created, assign property to object
                else
                {
                    if (
                        (n == "p" || (n == "" && i == 1)) &&
                        d.Count == 2)
                    {
                        obj.transform.localPosition = new Vector3(
                            float.Parse(d[0]),
                            float.Parse(d[1]),
                            float.Parse(v));
                    }
                    else if (
                        n == "p" ||
                        (n == "" && i == 1))
                    {
                        obj.transform.localPosition = Vector3.one * float.Parse(v);
                    }
                    else if (
                        (n == "r" || (n == string.Empty && i == 2)) &&
                        d.Count == 3)
                    {
                        obj.transform.rotation = new Quaternion(
                            float.Parse(d[0]),
                            float.Parse(d[1]),
                            float.Parse(d[2]),
                            float.Parse(v));
                    }
                    else if (
                        (n == "r" || (n == string.Empty && i == 2)) &&
                        d.Count == 2)
                    {
                        obj.transform.rotation = Quaternion.Euler(
                            float.Parse(d[0]),
                            float.Parse(d[1]),
                            float.Parse(v));
                    }
                    else if (
                        (n == "r" || (n == string.Empty && i == 2)) &&
                        d.Count == 0)
                    {
                        obj.transform.rotation = Quaternion.identity;
                    }
                    else if (
                        (n == "s" || (n == string.Empty && i == 3)) &&
                        d.Count == 0)
                    {
                        obj.transform.localScale = Vector3.one * float.Parse(v);
                    }
                    else if (
                        n == "s" ||
                        (n == "" && i == 3))
                    {
                        obj.transform.localScale = new Vector3(
                            float.Parse(d[0]),
                            float.Parse(d[1]),
                            float.Parse(v));
                    }
                    else if (n == "cc")
                    {
                        obj.AddComponent(typeof(CombineChildren));
                        worldParams["ccc"] = 1;
                    }
                    else if (n == "m")
                    {
                        //d.Add(v);
                        //ReadMesh(obj, d);
                        info += "Inline Whirld mesh generation not supported\n";
                    }
                    else if ((bool)lightSource && n == "color")
                    {
                        Color lsc = lightSource.color;
                        lsc.r = float.Parse(d[0]);
                        lsc.g = float.Parse(d[1]);
                        lsc.b = float.Parse(v);
                        lightSource.color = lsc;
                    }
                    else if ((bool)lightSource && n == "intensity")
                    {
                        lightSource.intensity = float.Parse(v);
                    }
                    else 
                    {
                        if (whirldObject != null)
                        {
                            
                            //Object Reference
                            if (v.Substring(0, 1) == "#")
                            {
                                whirldObject.parameters.Add(
                                    n,
                                    GetAsset(v.Substring(1)));
                            }

                            //Text
                            else
                            {
                                whirldObject.parameters.Add(n, v);
                            }
                        }
                        else if (n != "")
                        {
                            Debug.Log(
                                obj.name +
                                " Unknown Param: " +
                                n +
                                " > " +
                                v);
                        }
                    }
                }

                //Reset properties
				v = "";
				n = "";
				if (d.Count > 0) d = new List<String>();
				i++;

                //Done reading this object
				if (s == '}')
				{
                    //Finish up this object
					if (
                        obj.name == "cube" ||
                        obj.name == "pyramid" ||
                        obj.name == "cone" ||
                        obj.name == "mesh")
					{
						TextureObject(obj);
					}

                    //Increment ReadChar
					readChr++;
                    
                    //Handle spaces
					while (
                        readChr < data.Length &&
                        (
                            data[readChr] == ' ' ||
                            data[readChr] == '\n' ||
                            data[readChr] == '\r' ||
                            data[readChr] == '\t'))
					{
						readChr++;
					}

                    //Read the next object
                    if (readChr < data.Length && data[readChr] == '{')
                    {
                        readChr++;
                        ReadObject(parent);
                        return;
                    }

                    //Done reading objects at this level of recursion
                    else return;
				}
			}

            //Assign char to property we are reading
			else 
            {
                if (n != null) v += s;
			    else n += s;
            }
			readChr++;
		}
	}

	public void TextureObject(GameObject go)
	{
		MeshFilter mf = (MeshFilter)go.GetComponent(typeof(MeshFilter));
		if (!mf) return;
		Mesh mesh = mf.mesh;
		Vector2[] uvs = new Vector2[mesh.vertices.Length];
		int[] tris = mesh.triangles;
		for (int i = 0; i < tris.Length; i += 3)
		{
            Vector3 a = go.transform.TransformPoint(mesh.vertices[tris[i]]);
            Vector3 b = go.transform.TransformPoint(mesh.vertices[tris[i+1]]);
            Vector3 c = go.transform.TransformPoint(mesh.vertices[tris[i+2]]);
			Vector3 n = Vector3.Cross(a-c, b-c).normalized;
			if (
                Vector3.Dot(Vector3.up, n) >= 0.5f ||
                (Vector3.Dot(-Vector3.up, n) >= 0.5f))
			{
                uvs[tris[i]] = new Vector2(a.x, a.z);
                uvs[tris[i+1]] = new Vector2(b.x, b.z);
                uvs[tris[i+2]] = new Vector2(c.x, c.z);
			}
			else if (
                Vector3.Dot(Vector3.right, n) >= 0.5f ||
                (Vector3.Dot(Vector3.left, n) >= 0.5f))
			{
                uvs[tris[i]] = new Vector2(a.y, a.z);
                uvs[tris[i+1]] = new Vector2(b.y, b.z);
                uvs[tris[i+2]] = new Vector2(c.y, c.z);
			}
			else
			{
                uvs[tris[i]] = new Vector2(a.y, a.x);
                uvs[tris[i + 1]] = new Vector2(b.y, b.x);
                uvs[tris[i + 2]] = new Vector2(c.y, c.x);
			}
		}
		mesh.uv = uvs;
	}

	public String GetURL(String url)
	{
        if (url.Substring(0, 4) != "http") url = urlPath + url;
        return url;
	}
}