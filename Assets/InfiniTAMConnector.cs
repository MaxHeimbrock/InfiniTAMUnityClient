using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;

[StructLayout(LayoutKind.Sequential)]
public struct MeshInfo
{
    public int meshId, numVertices, numFaceIndices;
}

[StructLayout(LayoutKind.Sequential)]
public struct Vector3fArray
{
    [MarshalAs(UnmanagedType.ByValArray)]
    public Vector3[] vectors;
}

[StructLayout(LayoutKind.Sequential)]
public struct IntArray
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 300000)]
    public int[] faceIndices;
}

public class InfiniTAMConnector : MonoBehaviour
{
    public GameObject prefab;
    
    private static Dictionary<int, Mesh> activeMeshes;
    
    private int currentBufferNumber = 0;

    private SharedMeshData[] sharedMeshBuffers = new SharedMeshData[2];
    
    private const string meshInfoMutexName = "MESHINFO_MUTEX_";
    private const string meshInfoFileName = "MESHINFO_SHAREDMEMORY_";
    
    private const string verticesMutexName = "VERTICES_MUTEX_";
    private const string verticesFileName = "VERTICES_SHAREDMEMORY_";
    
    private const string normalsMutexName = "NORMALS_MUTEX_";
    private const string normalsFileName = "NORMALS_SHAREDMEMORY_";
    
    private const string facesMutexName = "FACES_MUTEX_";
    private const string facesFileName = "FACES_SHAREDMEMORY_";

    // Start is called before the first frame update
    void Start()
    {
        if (activeMeshes == null)
        {
            activeMeshes = new Dictionary<int, Mesh>();
        }
        
        sharedMeshBuffers[0] = new SharedMeshData(0);
        sharedMeshBuffers[1] = new SharedMeshData(1);
    }

    // Update is called once per frame
    void Update()
    {
        ReadSharedMemory();
    }

    public void UpdateMesh(Mesh mesh, Vector3[] newVertices, int[] faceIndices)
    {
        //Re-assign the modified mesh
        Debug.Log(mesh.triangles.Length);
        Debug.Log(newVertices.Length);
        mesh.vertices = newVertices;
        // mesh.triangles = faceIndices;
        mesh.RecalculateBounds();
    }
    
    public void ReadSharedMemory()
    {
        MeshInfo meshInfo;
        SharedMeshData currentBuffer = sharedMeshBuffers[currentBufferNumber];
        
        currentBuffer.sharedMemories[0].Lock();
        currentBuffer.sharedMemories[0].accessor.Read<MeshInfo>(0, out meshInfo);
        currentBuffer.sharedMemories[0].Unlock();

        if (meshInfo.meshId < 0)
        {
            // Debug.Log("No new updates");
            return;
        }
            
        Debug.Log("New mesh update on mesh " + meshInfo.meshId);

        Vector3fArray verticesStruct;
        Vector3[] testArray = new Vector3[meshInfo.numVertices];
        
        currentBuffer.sharedMemories[1].Lock();
        // currentBuffer.sharedMemories[1].accessor.Read<Vector3fArray>(0, out verticesStruct);
        currentBuffer.sharedMemories[1].accessor.ReadArray<Vector3>(0, testArray, 0, meshInfo.numVertices);
        currentBuffer.sharedMemories[1].Unlock();
        
        Debug.Log(testArray[0]);
        Debug.Log(testArray[1]);
        
        IntArray faceIndices;
        
        currentBuffer.sharedMemories[3].Lock();
        currentBuffer.sharedMemories[3].accessor.Read<IntArray>(0, out faceIndices);
        currentBuffer.sharedMemories[3].Unlock();
        
        Debug.Log("(" + faceIndices.faceIndices[0] + "," + faceIndices.faceIndices[1] + "," + faceIndices.faceIndices[2] + ")");

        // Update active mesh
        if (activeMeshes.ContainsKey(meshInfo.meshId))
        {
            // UpdateMesh(activeMeshes[meshInfo.meshId], verticesStruct.vectors, faceIndices.faceIndices);
        }
        // Create new mesh
        else
        {
            // GameObject newGameObject = new GameObject("Mesh" + meshInfo.meshId);
            // newGameObject.AddComponent<MeshFilter>();
            // newGameObject.AddComponent<MeshRenderer>();
            GameObject newGameObject = Instantiate(prefab);
            Mesh mesh = newGameObject.GetComponent<MeshFilter>().mesh;
            // UpdateMesh(mesh, verticesStruct.vectors, faceIndices.faceIndices);
            activeMeshes.Add(meshInfo.meshId, mesh); 
        }
        
        // mark update as read
        currentBuffer.sharedMemories[0].Lock();
        MeshInfo answer = new MeshInfo();
        answer.meshId = -1;
        currentBuffer.sharedMemories[0].accessor.Write<MeshInfo>(0, ref answer);
        currentBuffer.sharedMemories[0].Unlock();
        
        currentBufferNumber = (currentBufferNumber + 1) % 2;
    }

    public void OnDestroy()
    {
        Debug.Log("End");

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < sharedMeshBuffers[i].sharedMemories.Length; j++)
            {
                sharedMeshBuffers[i].sharedMemories[j].Destroy();
            }
        }
    }

    private class SharedMeshData
    {
        public SharedMemoryAccess[] sharedMemories;

        public SharedMeshData(int bufferNumber)
        {
            sharedMemories = new SharedMemoryAccess[4];
            
            sharedMemories[0] = new SharedMemoryAccess(meshInfoMutexName + bufferNumber, meshInfoFileName + bufferNumber);
            sharedMemories[1] = new SharedMemoryAccess(verticesMutexName + bufferNumber, verticesFileName + bufferNumber);
            sharedMemories[2] = new SharedMemoryAccess(normalsMutexName + bufferNumber, normalsFileName + bufferNumber);
            sharedMemories[3] = new SharedMemoryAccess(facesMutexName + bufferNumber, facesFileName + bufferNumber);
        }
    }
}


