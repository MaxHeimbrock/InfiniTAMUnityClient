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
    private static Mutex mut;
    MemoryMappedFile memMapFile;
    MemoryMappedViewAccessor accessor;
    MeshInfo data;
    
    private static Mutex vmut;
    MemoryMappedFile vmemMapFile;
    MemoryMappedViewAccessor vaccessor;
    private Vector3 vdata;
    
    private static Mutex vvmut;
    MemoryMappedFile vvmemMapFile;
    MemoryMappedViewAccessor vvaccessor;
    private Vector3fArray vvdata;
    
    // Start is called before the first frame update
    void Start()
    {
        mut = new Mutex(false, "NameOfMutexObject");
        vmut = new Mutex(false, "VERTICES_MUTEX");
        vvmut = new Mutex(false, "VERTICES_ARRAY_MUTEX");
        InitSharedMemory();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {
            if (accessor != null)
            {
                ReadSharedMemory();
                UpdateMesh();
            }
        }
    }

    public void UpdateMesh()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        Vector3[] vertices = mesh.vertices;

        //Re-assign the modified mesh
        mesh.vertices = vvdata.vectors;
        mesh.RecalculateBounds();
    }
    
    public void ReadSharedMemory()
    {
        /*
        mut.WaitOne();

        accessor.Read<MeshInfo>(0, out data);
        Debug.Log("MeshID: " + data.meshId);
        Debug.Log("numFaces: " + data.numFaceIndices);
        Debug.Log("numVertices: " + data.numVertices);

        mut.ReleaseMutex();
        */
        
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
        */
        
        
        vaccessor.Read<Vector3>(0, out vdata);
        Debug.Log("Vector (" + vdata.x + ", " + vdata.y + ", " + vdata.z + ")" );
        
        vmut.ReleaseMutex();
        
        vvmut.WaitOne();

        vvaccessor.Read<Vector3fArray>(0, out vvdata);
        Debug.Log("Vector (" + vvdata.vectors[0].x + ", " + vvdata.vectors[0].y + ", " + vvdata.vectors[1].z + ")" );
        
        
        
        /*
        vvaccessor.Read<Vector3>(3 * sizeof(float), out vvdata);
        Debug.Log("Vector (" + vvdata.x + ", " + vvdata.y + ", " + vvdata.z + ")" );
        */
        
        vvmut.ReleaseMutex();
    }
    
    public void InitSharedMemory()
    {
        try 
        {
            Debug.Log("Start");
            
            memMapFile = MemoryMappedFile.OpenExisting("INFO_MAPPING");
            
            accessor = memMapFile.CreateViewAccessor();
        }
        catch (Exception e)
        {
            Debug.Log("Error: " + e.Message);
        }
        
        try 
        {
            Debug.Log("Start v");
            
            vmemMapFile = MemoryMappedFile.OpenExisting("VERTICES");
            
            vaccessor = vmemMapFile.CreateViewAccessor();
        }
        catch (Exception e)
        {
            Debug.Log("Error in v: " + e.Message);
        }
        
        try 
        {
            Debug.Log("Start vv");
            
            vvmemMapFile = MemoryMappedFile.OpenExisting("VERTICES_ARRAY");
            
            vvaccessor = vvmemMapFile.CreateViewAccessor();
        }
        catch (Exception e)
        {
            Debug.Log("Error in v: " + e.Message);
        }
    }
	
	public void OnDestroy()
	{
		Debug.Log("End");
        
        mut.Dispose();
        memMapFile.Dispose();
        accessor.Dispose();
        
        vmut.Dispose();
        vmemMapFile.Dispose();
        vaccessor.Dispose();
        
        vvmut.Dispose();
        vvmemMapFile.Dispose();
        vvaccessor.Dispose();
	}
}
