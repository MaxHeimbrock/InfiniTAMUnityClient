using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

[StructLayout(LayoutKind.Sequential)]
public struct MeshInfo
{
    public int meshId, numVertices, numFaceIndices;
}

[StructLayout(LayoutKind.Sequential)]
public struct Vector3fArray
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100000)]
    public Vector3[] vectors;
}

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
    
    Vector3fArray vertices;

    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        
        // mut = new Mutex(false, "NameOfMutexObject");
        // vmut = new Mutex(false, "VERTICES_MUTEX");
        // vvmut = new Mutex(false, "VERTICES_ARRAY_MUTEX");
        // vvmut = new Mutex(false, "TEST");
        
        sharedMemories = new SharedMemoryAccess[2];
        InitSharedMemory();
    }

    // Update is called once per frame
    void Update()
    {
        ReadSharedMemory();
    }

    public void UpdateMesh()
    {
        mesh = GetComponent<MeshFilter>().mesh;

        //Re-assign the modified mesh
        mesh.vertices = vertices.vectors;
        mesh.RecalculateBounds();
    }
    
    public void ReadSharedMemory()
    {
        sharedMemories[0].Lock();
        sharedMemories[0].accessor.Read<MeshInfo>(0, out meshInfo);
        sharedMemories[0].Unlock();
        
        Debug.Log("MeshID: " + meshInfo.meshId);
        Debug.Log("numFaces: " + meshInfo.numFaceIndices);
        Debug.Log("numVertices: " + meshInfo.numVertices);
        
        /*
        mut.WaitOne();

        accessor.Read<MeshInfo>(0, out data);
        Debug.Log("MeshID: " + data.meshId);
        Debug.Log("numFaces: " + data.numFaceIndices);
        Debug.Log("numVertices: " + data.numVertices);

        mut.ReleaseMutex();
        
        
        vmut.WaitOne();

        /*
        BinaryReader reader = new BinaryReader(vmemMapFile.CreateViewStream());
        
        for (int i = 0; i < 1000; i++)
        {
            float value = reader.ReadSingle();

            if (value != 0)
            {
                Debug.Log("HERE NOT NULL: i =" + i + " value = " + value);
            }
        }
        
        
        
        vaccessor.Read<Vector3>(0, out vdata);
        Debug.Log("Vector (" + vdata.x + ", " + vdata.y + ", " + vdata.z + ")" );
        
        vmut.ReleaseMutex();
        
        vvmut.WaitOne();

        vvaccessor.Read<Vector3fArray>(0, out vvdata);
        Debug.Log("Vector (" + vvdata.vectors[0].x + ", " + vvdata.vectors[0].y + ", " + vvdata.vectors[1].z + ")" );
        
        
        
        /*
        vvaccessor.Read<Vector3>(3 * sizeof(float), out vvdata);
        Debug.Log("Vector (" + vvdata.x + ", " + vvdata.y + ", " + vvdata.z + ")" );
        
        
        vvmut.ReleaseMutex();
        
        */
    }
    
    public void InitSharedMemory()
    {
        sharedMemories[0] = new SharedMemoryAccess(meshInfoMutexName + number, meshInfoFileName + number);
        // sharedMemories[1] = new SharedMemoryAccess(verticesMutexName + number, verticesFileName + number);
    }
	
	public void OnDestroy()
	{
		Debug.Log("End");

        for (int i = 0; i < sharedMemories.Length; i++)
        {
            sharedMemories[i].Destroy();
        }
    }

    private class SharedMemoryAccess
    {
        private string mutexName; 
        private string mmfName; 
    
        private static Mutex mutex;
        private MemoryMappedFile mmf;
        public MemoryMappedViewAccessor accessor;

        public SharedMemoryAccess(string mutexName, string mmfName)
        {
            mutex = new Mutex(false, mutexName);
            
            try 
            {
                mmf = MemoryMappedFile.OpenExisting(mmfName);
                accessor = mmf.CreateViewAccessor();
            }
            catch (Exception e)
            {
                Debug.Log("Could not open existing shared memory " + mmfName );
            }
        }

        public void Destroy()
        {
            mutex.Dispose();
            mmf.Dispose();
            accessor.Dispose();
        }

        public bool Lock()
        {
            return mutex.WaitOne();
        }

        public void Unlock()
        {
            mutex.ReleaseMutex();
        }
    }
}


