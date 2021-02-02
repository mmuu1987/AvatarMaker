using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Vertices
{
    public float vertices_x;

    public float vertices_y;

    public float vertices_z;
}
[Serializable]
public class UVs
{
    public float UV_x;

    public float UV_y;
}
[Serializable]
public class HeadInfo
{
    public List<Vertices> Vertices;

    public List<UVs> Uvs;

    public List<int> Triangles;

    public byte[] TexBytes;

    public Vector3[] GetVertices()
    {
       List<Vector3> temps = new List<Vector3>();

       foreach (Vertices vertices in Vertices)
       {
           Vector3 temp = Vector3.zero;
           temp.x = vertices.vertices_x;
           temp.y = vertices.vertices_y;
           temp.z = vertices.vertices_z;
           temps.Add(temp);
       }

       return temps.ToArray();
    }

    public Vector2[] GetUVs()
    {
        List<Vector2> temps = new List<Vector2>();

        foreach (UVs uv in Uvs)
        {
            Vector3 temp = Vector3.zero;
            temp.x = uv.UV_x;
            temp.y = uv.UV_y;
           
            temps.Add(temp);
        }

        return temps.ToArray();
    }

    public int[] GetTriangles()
    {
        return Triangles.ToArray();
    }
}