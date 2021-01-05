using System.Runtime.InteropServices;
using UnityEngine;

public class SharedMemoryTest : MonoBehaviour
{
    public int number = 0;
    private Mesh mesh;
    
    /*
     * [0] -> MeshInfo
     * [1] -> Vertices
     */

    private SharedMemoryAccess[] sharedMemories;
    
    private const string meshInfoMutexName = "MESHINFO_MUTEX_";
    private const string meshInfoFileName = "MESHINFO_SHAREDMEMORY_";
    
    MeshInfo meshInfo;
    
    private const string verticesMutexName = "VERTICES_MUTEX_";
    private const string verticesFileName = "VERTICES_SHAREDMEMORY_";

    private Vector3 vertex;
    Vector3fArray vertices;

    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        
        sharedMemories = new SharedMemoryAccess[2];
        InitSharedMemory();
    }

    // Update is called once per frame
    void Update()
    {
        ReadSharedMemory();
        UpdateMesh();
    }

    public void UpdateMesh()
    {
        //Re-assign the modified mesh
        mesh.vertices = vertices.vectors;
        mesh.RecalculateBounds();
    }
    
    public void ReadSharedMemory()
    {
        sharedMemories[0].Lock();
        sharedMemories[0].accessor.Read<MeshInfo>(0, out meshInfo);
        sharedMemories[0].Unlock();
        
        sharedMemories[1].Lock();
        sharedMemories[1].accessor.Read<Vector3fArray>(0, out vertices);
        sharedMemories[1].Unlock();
    }
    
    public void InitSharedMemory()
    {
        sharedMemories[0] = new SharedMemoryAccess(meshInfoMutexName + number, meshInfoFileName + number);
        sharedMemories[1] = new SharedMemoryAccess(verticesMutexName + number, verticesFileName + number);
    }
	
	public void OnDestroy()
	{
		Debug.Log("End");

        for (int i = 0; i < sharedMemories.Length; i++)
        {
            sharedMemories[i].Destroy();
        }
    }
}


