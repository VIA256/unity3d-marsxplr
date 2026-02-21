using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

//data structure describing a section of skidmarks
[Serializable]
public class markSection
{
    public Vector3 pos;
    public Vector3 normal;
    public Vector3 posl;
    public Vector3 posr;
    public float intensity = 0.0f;
    public int lastIndex = -1;
}

[Serializable]
public class Skidmarks : MonoBehaviour
{
    //maximal number of skidmarks
	private int maxMarks = 256;

    //width of skid marks
	public float markWidth = 0.225f;

    //time interval new mesh segments are generated in
    //the lower this value, the smoother the generated tracks
	private float updateRate = 0.2f;

	// /*UNUSED*/ private int indexShift = 0;
	private int numMarks = 0;
	private float updateTime = 0.0f;
	private bool newTrackFlag = true;
	private bool updateMeshFlag = true;
	private MeshFilter meshFilter;
	private Mesh mesh;

	public markSection[] skidmarks;

    // Use this for initialization
	public void Start()
	{
        //create structures
		skidmarks = new markSection[maxMarks];
		for (int i = 0; i < maxMarks; i++)
		{
            skidmarks[i] = new markSection();
		}
		meshFilter = (MeshFilter)gameObject.GetComponent(typeof(MeshFilter));
		mesh = meshFilter.mesh;
		if (mesh == null)
		{
			mesh = new Mesh();
		}
	}

    //called by the car script to add a skid mark at position pos with the supplied normal.
    //transparency can be specified in the intensity parameter. Connects to the track segment 
    //indexed by lastIndex (or it won't display if lastIndex is -1). returns an index value 
    //which can be passed as lastIndex to the next AddSkidMark call
	public int AddSkidMark(Vector3 pos, Vector3 normal, float intensity, int lastIndex)
	{
		intensity = Mathf.Clamp01(intensity);

        //get index for new segment
		int currIndex = numMarks;

        //reuse lastIndex if we don't need to create a new one this frame
		if (lastIndex != -1 && !newTrackFlag)
		{
			currIndex = lastIndex;
		}

        //setup skidmark structure
        markSection curr = skidmarks[currIndex % maxMarks];
		curr.pos = pos + normal * 0.05f - transform.position;
		curr.normal = normal;
		curr.intensity = intensity;

		if (lastIndex == -1 || newTrackFlag)
		{
			curr.lastIndex = lastIndex;
		}

        //if we have a valid lastIndex, get positions for marks
		if (curr.lastIndex != -1)
		{
            markSection last = skidmarks[curr.lastIndex % maxMarks];
            Vector3 dir = curr.pos - last.pos;
			Vector3 normalized = Vector3.Cross(dir, normal).normalized;

			curr.posl = curr.pos + normalized * markWidth * 0.5f;
			curr.posr = curr.pos - normalized * markWidth * 0.5f;

			if (last.lastIndex == -1)
			{
				last.posl = curr.pos + normalized * markWidth * 0.5f;
				last.posr = curr.pos - normalized * markWidth * 0.5f;
			}
		}
		if (lastIndex == -1 || newTrackFlag)
		{
			numMarks++;
		}
		updateMeshFlag = true;
		return currIndex;
	}

    //regenerate the skidmarks mesh
	public void UpdateMesh()
	{
        //count visible segments
		int segmentCount = 0;
		for (int i = 0; i < numMarks && i < maxMarks; i++)
		{
            if (
                skidmarks[i].lastIndex != -1 &&
                skidmarks[i].lastIndex > numMarks - maxMarks)
            {
                segmentCount++;
            }
		}

        //create skidmark mesh coordinates
		Vector3[] vertices = new Vector3[segmentCount * 4];
		Vector3[] normals = new Vector3[segmentCount * 4];
		Color[] colors = new Color[segmentCount * 4];
		Vector2[] uvs = new Vector2[segmentCount * 4];
		int[] triangles = new int[segmentCount * 6];
		segmentCount = 0;
		for (int i = 0; i < numMarks && i < maxMarks; i++)
		{
            if (
                skidmarks[i].lastIndex != -1 &&
                skidmarks[i].lastIndex > numMarks - maxMarks)
            {
                markSection curr = skidmarks[i];
                markSection last = skidmarks[curr.lastIndex % maxMarks];

                vertices[segmentCount * 4 + 0] = last.posl;
                vertices[segmentCount * 4 + 1] = last.posr;
                vertices[segmentCount * 4 + 2] = curr.posl;
                vertices[segmentCount * 4 + 3] = curr.posr;

                normals[segmentCount * 4 + 0] = last.normal;
                normals[segmentCount * 4 + 1] = last.normal;
                normals[segmentCount * 4 + 2] = curr.normal;
                normals[segmentCount * 4 + 3] = curr.normal;

                colors[segmentCount * 4 + 0] = new Color(1f, 1f, 1f, last.intensity);
                colors[segmentCount * 4 + 1] = new Color(1f, 1f, 1f, last.intensity);
                colors[segmentCount * 4 + 2] = new Color(1f, 1f, 1f, curr.intensity);
                colors[segmentCount * 4 + 3] = new Color(1f, 1f, 1f, curr.intensity);

                uvs[segmentCount * 4 + 0] = new Vector2(0f, 0f);
                uvs[segmentCount * 4 + 1] = new Vector2(1f, 0f);
                uvs[segmentCount * 4 + 2] = new Vector2(0f, 0f);
                uvs[segmentCount * 4 + 3] = new Vector2(1f, 0f);

                triangles[segmentCount * 6 + 0] = segmentCount * 4 + 0;
                triangles[segmentCount * 6 + 1] = segmentCount * 4 + 1;
                triangles[segmentCount * 6 + 2] = segmentCount * 4 + 2;

                triangles[segmentCount * 6 + 3] = segmentCount * 4 + 2;
                triangles[segmentCount * 6 + 4] = segmentCount * 4 + 1;
                triangles[segmentCount * 6 + 5] = segmentCount * 4 + 3;
                segmentCount++;
            }
		}

        //update mesh
		mesh.Clear();
		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.triangles = triangles;
		mesh.colors = colors;
		mesh.uv = uvs;
		updateMeshFlag = false;
	}

	public void Update()
	{
        //update mesh if skidmarks have changed since last frame
		if (updateMeshFlag)
		{
			UpdateMesh();
		}
	}

	public void FixedUpdate()
	{
        //set flag for creating new segments this frame if an update is pending
		newTrackFlag = false;
		updateTime += Time.deltaTime;
		if (updateTime > updateRate)
		{
			newTrackFlag = true;
			updateTime -= updateRate;
		}
	}
}
