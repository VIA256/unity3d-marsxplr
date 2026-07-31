using System;
using System.IO;
using UnityEngine;

// A simple mesh saving/loading functionality.
// This is an utility script, you don't need to add it to any objects.
// See SaveMeshForWeb and LoadMeshFromWeb for example of use.
//
// Uses a custom binary format:
//
//    2 bytes vertex count
//    2 bytes triangle count
//    1 bytes vertex format (bits: 0=vertices, 1=normals, 2=tangents, 3=uvs)
//
//    After that come vertex component arrays, each optional except for positions.
//    Which ones are present depends on vertex format:
//        Positions
//            Bounding box is before the array (xmin,xmax,ymin,ymax,zmin,zmax)
//            Then each vertex component is 2 byte unsigned short, interpolated between the bound axis
//        Normals
//            One byte per component
//        Tangents
//            One byte per component
//        UVs (8 bytes/vertex - 2 floats)
//            Bounding box is before the array (xmin,xmax,ymin,ymax)
//            Then each UV component is 2 byte unsigned short, interpolated between the bound axis
//
//    Finally the triangle indices array: 6 bytes per triangle (3 unsigned short indices)

[Serializable]
public class MeshSerializer : MonoBehaviour
{
    // Reads mesh from an array of bytes. Can return null
    // if the bytes seem invalid.
	public static Mesh ReadMesh(byte[] bytes)
	{
		if (bytes == null || bytes.Length < 5)
		{
			print("Invalid mesh file!");
			return null;
		}

		BinaryReader buf = new BinaryReader(new MemoryStream(bytes));
		
        // read header
        UInt16 vertCount = buf.ReadUInt16();
		UInt16 triCount = buf.ReadUInt16();
		byte format = buf.ReadByte();

        // sanity check
		if ((int)vertCount < 0 || (int)vertCount > 64000)
		{
			print("Invalid vertex count in the mesh data!");
			return null;
		}
		if ((int)triCount < 0 || (int)triCount > 64000)
		{
			print("Invalid triangle count in the mesh data!");
			return null;
		}
		if ((int)format < 1 || ((int)format & 1) == 0 || (int)format > 15)
		{
			print("Invalid vertex format in the mesh data!");
			return null;
		}

		Mesh mesh = new Mesh();
		int i;

        // positions
		Vector3[] verts = new Vector3[(int)vertCount];
		ReadVector3Array16bit(verts, buf);
		mesh.vertices = verts;

		if (((int)format & 2) != 0) // have normals
		{
			Vector3[] normals = new Vector3[(int)vertCount];
			ReadVector3ArrayBytes(normals, buf);
			mesh.normals = normals;
		}

		if (((int)format & 4) != 0) // have tangents
		{
			Vector4[] tangents = new Vector4[(int)vertCount];
			ReadVector4ArrayBytes(tangents, buf);
			mesh.tangents = tangents;
		}

		if (((int)format & 8) != 0) // have UVs
		{
			Vector2[] uvs = new Vector2[(int)vertCount];
			ReadVector2Array16bit(uvs, buf);
			mesh.uv = uvs;
		}
        // triangle indices
		int[] tris = new int[(int)triCount * 3];
		for (i = 0; i < (int)triCount; i++)
		{
            tris[i * 3 + 0] = buf.ReadUInt16();
            tris[i * 3 + 1] = buf.ReadUInt16();
            tris[i * 3 + 2] = buf.ReadUInt16();
		}
		mesh.triangles = tris;

		buf.Close();

		return mesh;
	}

	public static void ReadVector3Array16bit(Vector3[] arr, BinaryReader buf)
	{
		int n = arr.Length;
        if (n == 0) return;

        // Read bounding box
        Vector3 bmin;
        Vector3 bmax;
        bmin.x = buf.ReadSingle();
        bmax.x = buf.ReadSingle();
        bmin.y = buf.ReadSingle();
        bmax.y = buf.ReadSingle();
        bmin.z = buf.ReadSingle();
        bmax.z = buf.ReadSingle();

        // Decode vectors as 16 bit integer components between the bounds
        for (int i = 0; i < n; ++i)
        {
            UInt16 ix = buf.ReadUInt16();
            UInt16 iy = buf.ReadUInt16();
            UInt16 iz = buf.ReadUInt16();
            float xx = ix / 65535.0f * (bmax.x - bmin.x) + bmin.x;
            float yy = iy / 65535.0f * (bmax.y - bmin.y) + bmin.y;
            float zz = iz / 65535.0f * (bmax.z - bmin.z) + bmin.z;
            arr[i] = new Vector3(xx, yy, zz);
        }
    }

	public static void WriteVector3Array16bit(Vector3[] arr, BinaryWriter buf)
	{
        if (arr.Length == 0) return;

        // Calculate bounding box of the array
        Bounds bounds = new Bounds(arr[0], new Vector3(0.001f, 0.001f, 0.001f));
        foreach (Vector3 v in arr)
        {
            bounds.Encapsulate(v);
        }

        // Write bounds to stream
        Vector3 bmin = bounds.min;
        Vector3 bmax = bounds.max;
        buf.Write(bmin.x);
        buf.Write(bmax.x);
        buf.Write(bmin.y);
        buf.Write(bmax.y);
        buf.Write(bmin.z);
        buf.Write(bmax.z);

        // Encode vectors as 16 bit integer components between the bounds
        foreach (Vector3 v in arr)
        {
            int xx = (int)Mathf.Clamp(
                (v.x - bmin.x) / (bmax.x - bmin.x) * 65535.0f,
                0.0f,
                65535.0f);
            int yy = (int)Mathf.Clamp(
                (v.y - bmin.y) / (bmax.y - bmin.y) * 65535.0f,
                0.0f,
                65535.0f);
            int zz = (int)Mathf.Clamp(
                (v.z - bmin.z) / (bmax.z - bmin.z) * 65535.0f,
                0.0f,
                65535.0f);
            UInt16 ix = (UInt16)xx;
            UInt16 iy = (UInt16)yy;
            UInt16 iz = (UInt16)zz;
            buf.Write(ix);
            buf.Write(iy);
            buf.Write(iz);
        }
	}

	public static void ReadVector2Array16bit(Vector2[] arr, BinaryReader buf)
	{
        int n = arr.Length;
        if (n == 0) return;

        // Read bounding box
        Vector2 bmin;
        Vector2 bmax;
        bmin.x = buf.ReadSingle();
        bmax.x = buf.ReadSingle();
        bmin.y = buf.ReadSingle();
        bmax.y = buf.ReadSingle();

        // Decode vectors as 16 bit integer components between the bounds
        for (int i = 0; i < n; ++i)
        {
            UInt16 ix = buf.ReadUInt16();
            UInt16 iy = buf.ReadUInt16();
            float xx = ix / 65535.0f * (bmax.x - bmin.x) + bmin.x;
            float yy = iy / 65535.0f * (bmax.y - bmin.y) + bmin.y;
            arr[i] = new Vector2(xx, yy);
        }
	}

	public static void WriteVector2Array16bit(Vector2[] arr, BinaryWriter buf)
	{
        if (arr.Length == 0) return;

        // Calculate bounding box of the array
        Bounds bounds = new Bounds(arr[0], new Vector2(0.001f, 0.001f));
        foreach (Vector2 v in arr)
        {
            bounds.Encapsulate(v);
        }

        // Write bounds to stream
        Vector2 bmin = bounds.min;
        Vector2 bmax = bounds.max;
        buf.Write(bmin.x);
        buf.Write(bmax.x);
        buf.Write(bmin.y);
        buf.Write(bmax.y);

        // Encode vectors as 16 bit integer components between the bounds
        foreach (Vector3 v in arr)
        {
            int xx = (int)Mathf.Clamp(
                (v.x - bmin.x) / (bmax.x - bmin.x) * 65535.0f,
                0.0f,
                65535.0f);
            int yy = (int)Mathf.Clamp(
                (v.y - bmin.y) / (bmax.y - bmin.y) * 65535.0f,
                0.0f,
                65535.0f);
            UInt16 ix = (UInt16)xx;
            UInt16 iy = (UInt16)yy;
            buf.Write(ix);
            buf.Write(iy);
        }
	}

	public static void ReadVector3ArrayBytes(Vector3[] arr, BinaryReader buf)
	{
        //Decode vectors as 8 bit integer components in -1.0 .. 1.0 range
		int n = arr.Length;
		for (int i = 0; i < n; ++i)
		{
			byte ix = buf.ReadByte();
			byte iy = buf.ReadByte();
			byte iz = buf.ReadByte();
			float xx = ((float)(int)ix - 128f) / 127f;
			float yy = ((float)(int)iy - 128f) / 127f;
			float zz = ((float)(int)iz - 128f) / 127f;
			arr[i] = new Vector3(xx, yy, zz);
		}
	}

	public static void WriteVector3ArrayBytes(Vector3[] arr, BinaryWriter buf)
	{
        // Encode vectors as 8 bit integer components in -1.0 .. 1.0 range
		foreach(Vector3 v in arr)
        {
			byte ix = (byte)Mathf.Clamp(v.x * 127f + 128f, 0f, 255f);
			byte iy = (byte)Mathf.Clamp(v.y * 127f + 128f, 0f, 255f);
			byte iz = (byte)Mathf.Clamp(v.z * 127f + 128f, 0f, 255f);
			buf.Write(ix);
			buf.Write(iy);
			buf.Write(iz);
		}
	}

	public static void ReadVector4ArrayBytes(Vector4[] arr, BinaryReader buf)
	{
        //Decode vectors as 8 bit integer components in -1.0 .. 1.0 range
        int n = arr.Length;
        for (int i = 0; i < n; ++i)
        {
            byte ix = buf.ReadByte();
            byte iy = buf.ReadByte();
            byte iz = buf.ReadByte();
            byte iw = buf.ReadByte();
            float xx = ((float)(int)ix - 128f) / 127f;
            float yy = ((float)(int)iy - 128f) / 127f;
            float zz = ((float)(int)iz - 128f) / 127f;
            float ww = ((float)(int)iw - 128f) / 127f;
            arr[i] = new Vector4(xx, yy, zz, ww);
        }
	}

	public static void WriteVector4ArrayBytes(Vector4[] arr, BinaryWriter buf)
	{
        // Encode vectors as 8 bit integer components in -1.0 .. 1.0 range
        foreach (Vector4 v in arr)
        {
            byte ix = (byte)Mathf.Clamp(v.x * 127f + 128f, 0f, 255f);
            byte iy = (byte)Mathf.Clamp(v.y * 127f + 128f, 0f, 255f);
            byte iz = (byte)Mathf.Clamp(v.z * 127f + 128f, 0f, 255f);
            byte iw = (byte)Mathf.Clamp(v.w * 127f + 128f, 0f, 255f);
            buf.Write(ix);
            buf.Write(iy);
            buf.Write(iz);
            buf.Write(iw);
        }
	}

    // Writes mesh to an array of bytes.
	public static byte[] WriteMesh(Mesh mesh, bool saveTangents)
	{
		if (!mesh)
		{
			print("No mesh given!");
			return null;
		}

		Vector3[] verts = mesh.vertices;
		Vector3[] normals = mesh.normals;
		Vector4[] tangents = mesh.tangents;
		Vector2[] uvs = mesh.uv;
		int[] tris = mesh.triangles;

        // figure out vertex format
		byte format = (byte)1;
		if (normals.Length > 0)
		{
            format |= 2;
		}
		if (saveTangents && tangents.Length > 0)
		{
            format |= 4;
		}
		if (uvs.Length > 0)
		{
            format |= 8;
		}

		MemoryStream stream = new MemoryStream();
		BinaryWriter buf = new BinaryWriter(stream);

        // write header
		UInt16 vertCount = (UInt16)verts.Length;
		UInt16 triCount = (UInt16)(tris.Length / 3);
		buf.Write(vertCount);
		buf.Write(triCount);
		buf.Write(format);
        // vertex components
		WriteVector3Array16bit(verts, buf);
		WriteVector3ArrayBytes(normals, buf);
		if (saveTangents)
		{
			WriteVector4ArrayBytes(tangents, buf);
		}
		WriteVector2Array16bit(uvs, buf);
        // triangle indices
        foreach (int idx in tris)
        {
            UInt16 idx16 = (UInt16)idx;
            buf.Write(idx16);
        }
		buf.Close();

		return stream.ToArray();
	}

    // Writes mesh to a local file, for loading with WWW interface later.
	public static void WriteMeshToFileForWeb(Mesh mesh, string name, bool saveTangents)
	{
		// Write mesh to regular bytes
        byte[] bytes = WriteMesh(mesh, saveTangents);

        // Write to file
        FileStream fs = new FileStream(name, FileMode.Create);
        fs.Write(bytes, 0, bytes.Length);
        fs.Close();
	}

    // Reads mesh from the given WWW (that is finished downloading already)
	public static Mesh ReadMeshFromWWW(WWW download)
	{
		if (download.error != null)
		{
			print("Error downloading mesh: " + download.error);
			return null;
		}

		if (!download.isDone)
		{
			print("Download must be finished already");
			return null;
		}

		byte[] bytes = download.bytes;

		return ReadMesh(bytes);
	}
}
