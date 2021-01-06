using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;

[StructLayout(LayoutKind.Sequential)]
public struct MeshInfo
{
    public int meshId, numVertices, numFaceIndices;
}

public class InfiniTAMConnector : MonoBehaviour
{
    public GameObject prefab;
    
    private static Dictionary<int, GameObject> activeMeshes;
    
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
    
    private const string colorsMutexName = "COLORS_MUTEX_";
    private const string colorsFileName = "COLORS_SHAREDMEMORY_";

    // Start is called before the first frame update
    void Start()
    {
        if (activeMeshes == null)
        {
            activeMeshes = new Dictionary<int, GameObject>();
        }
        
        sharedMeshBuffers[0] = new SharedMeshData(0);
        sharedMeshBuffers[1] = new SharedMeshData(1);
        
        Debug.Log("Init shared memory successful");
    }

    // Update is called once per frame
    void Update()
    {
        ReadSharedMemory();
    }

    Color[] CreateColorsFromVector4Array(Vector4[] colorsAsVectorArray)
    {
        Color[] result = new Color[colorsAsVectorArray.Length];

        for (int i = 0; i < colorsAsVectorArray.Length; i++)
        {
            result[i] = new Color(colorsAsVectorArray[i].x, colorsAsVectorArray[i].y, colorsAsVectorArray[i].z, colorsAsVectorArray[i].w);
        }
        
        return result;
    }

    public void UpdateMesh(GameObject mesh, Vector3[] newVertices, Vector3[] normals, int[] faceIndices, Vector4[] colorsAsVectors)
    {
        Color[] colors = CreateColorsFromVector4Array(colorsAsVectors);
        
        Mesh newMesh = new Mesh();
        newMesh.vertices = newVertices;
        newMesh.normals = normals;
        newMesh.triangles = faceIndices;
        newMesh.colors = colors;

        mesh.GetComponent<MeshFilter>().mesh = newMesh;
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
            return;
        }
            
        // Debug.Log("New mesh update on mesh " + meshInfo.meshId);

        Vector3[] vertices = new Vector3[meshInfo.numVertices];
        currentBuffer.sharedMemories[1].Lock();
        currentBuffer.sharedMemories[1].accessor.ReadArray<Vector3>(0, vertices, 0, meshInfo.numVertices);
        currentBuffer.sharedMemories[1].Unlock();

        // TODO NORMALS
        
        Vector3[] normals = new Vector3[meshInfo.numVertices];
        currentBuffer.sharedMemories[1].Lock();
        currentBuffer.sharedMemories[1].accessor.ReadArray<Vector3>(0, normals, 0, meshInfo.numVertices);
        currentBuffer.sharedMemories[1].Unlock();

        int[] faceIndices = new int[meshInfo.numFaceIndices];
        currentBuffer.sharedMemories[3].Lock();
        currentBuffer.sharedMemories[3].accessor.ReadArray<int>(0, faceIndices, 0, meshInfo.numFaceIndices);
        currentBuffer.sharedMemories[3].Unlock();
        
        Vector4[] colors = new Vector4[meshInfo.numVertices];
        currentBuffer.sharedMemories[4].Lock();
        currentBuffer.sharedMemories[4].accessor.ReadArray<Vector4>(0, colors, 0, meshInfo.numVertices);
        currentBuffer.sharedMemories[4].Unlock();
        
        // Update active mesh
        if (activeMeshes.ContainsKey(meshInfo.meshId))
        {
            UpdateMesh(activeMeshes[meshInfo.meshId], vertices, normals, faceIndices, colors);
        }
        // Create new mesh
        else
        {
            GameObject newGameObject = Instantiate(prefab);
            newGameObject.name = "Mesh" + meshInfo.meshId;
            UpdateMesh(newGameObject, vertices, normals, faceIndices, colors);
            activeMeshes.Add(meshInfo.meshId, newGameObject);
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
            sharedMemories = new SharedMemoryAccess[5];
            
            sharedMemories[0] = new SharedMemoryAccess(meshInfoMutexName + bufferNumber, meshInfoFileName + bufferNumber);
            sharedMemories[1] = new SharedMemoryAccess(verticesMutexName + bufferNumber, verticesFileName + bufferNumber);
            sharedMemories[2] = new SharedMemoryAccess(normalsMutexName + bufferNumber, normalsFileName + bufferNumber);
            sharedMemories[3] = new SharedMemoryAccess(facesMutexName + bufferNumber, facesFileName + bufferNumber);
            sharedMemories[4] = new SharedMemoryAccess(colorsMutexName + bufferNumber, colorsFileName + bufferNumber);
        }
    }
}


