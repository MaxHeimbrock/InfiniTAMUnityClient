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

/*
[StructLayout(LayoutKind.Sequential)]
public struct Vector3f
{
    public float x, y, z;
}

public struct Vector3fArray
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public Vector3f[] vectors;
}
*/

public class SharedMemoryTest : MonoBehaviour
{
    private static Mutex mut;
    MemoryMappedFile memMapFile;
    MemoryMappedViewAccessor accessor;
    MeshInfo data;
    
    private static Mutex vmut;
    MemoryMappedFile vmemMapFile;
    MemoryMappedViewAccessor vaccessor;
    
    // Start is called before the first frame update
    void Start()
    {
        mut = new Mutex(false, "NameOfMutexObject");
        vmut = new Mutex(false, "VERTICES_MUTEX");
        InitSharedMemory();
    }

    // Update is called once per frame
    void Update()
    {
        if (accessor != null)
        {
            ReadSharedMemory();
        }
    }

    public void ReadSharedMemory()
    {
        mut.WaitOne();

        accessor.Read<MeshInfo>(0, out data);
        Debug.Log("MeshID: " + data.meshId);
        Debug.Log("numFaces: " + data.numFaceIndices);
        Debug.Log("numVertices: " + data.numVertices);

        mut.ReleaseMutex();
        
        vmut.WaitOne();

        BinaryReader reader = new BinaryReader(vmemMapFile.CreateViewStream());
        
        for (int i = 0; i < 1000; i++)
        {
            float value = reader.ReadSingle();

            if (value != 0)
            {
                Debug.Log("HERE NOT NULL: i =" + i + " value = " + value);
            }
        }
        
        /*
        vaccessor.Read<Vector3fArray>(0, out vdata);
        Debug.Log("int: " + vdata.x);
        Debug.Log("Vector (" + vdata.vectors[0].x + ", " + vdata.vectors[0].y + ", " + vdata.vectors[0].z + ")" );
        */
        vmut.ReleaseMutex();
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
            
            // vaccessor = vmemMapFile.CreateViewAccessor();
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
        vmemMapFile.Dispose();
        accessor.Dispose();
        vaccessor.Dispose();
	}
}
