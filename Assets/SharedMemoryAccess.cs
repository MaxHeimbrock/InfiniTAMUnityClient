using System;
using System.IO.MemoryMappedFiles;
using System.Threading;
using UnityEngine;

public class SharedMemoryAccess
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