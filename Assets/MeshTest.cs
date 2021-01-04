using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class MeshTest : MonoBehaviour
{
    [DllImport("InfiniTAMUnityPlugin")]
    static extern unsafe void UpdateVectorArray(Vector3* vecArray, int vecSize);

    [DllImport("InfiniTAMUnityPlugin")]
    static extern void Init(ref int result);
    
    void UpdateVectorArray(Vector3[] vecArray)
    {
        unsafe
        {
            //Pin array then send to C++
            fixed (Vector3* vecPtr = vecArray)
            {
                UpdateVectorArray(vecPtr, vecArray.Length);
            }
        }
    }
    
    void Start()
    {
        int result = 1;
        Init(ref result);
        Debug.Log("Result of init was: " + result);
        
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        Vector3[] vertices = mesh.vertices;
        UpdateVectorArray(vertices);

        //Re-assign the modified mesh
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
    }
}
