using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Animus.Data;
using Animus.RobotProto;
using Google.Protobuf;
using UnityEngine.Scripting;
using Debug = UnityEngine.Debug;


namespace AnimusCommon
{
    public static class AnimusUtils
    {
        public enum EmotionName
        {
            angry,
            fear,
            sad,
            happy,
            surprised,
            neutral
        }

        public static void discover_modalities(Type driverType, Robot robot)
        {
            var initModalities = new List<string>();
            var closeModalities = new List<string>();
            var setModalities = new List<string>();
            var getModalities = new List<string>();

            var inputModalities = new List<string>();
            var outputModalities = new List<string>();

            foreach (var method in driverType.GetMethods())
            {
                if (method.Name.Contains("_initialise"))
                {
                    initModalities.Add(method.Name.Replace("_initialise", ""));
                }
                else if (method.Name.Contains("_close"))
                {
                    closeModalities.Add(method.Name.Replace("_close", ""));
                }
                else if (method.Name.Contains("_set"))
                {
                    setModalities.Add(method.Name.Replace("_set", ""));
                }
                else if (method.Name.Contains("_get"))
                {
                    getModalities.Add(method.Name.Replace("_get", ""));
                }
            }

            var allinit = String.Join(", ", initModalities);
            Debug.Log(allinit);

            foreach (var thisMod in initModalities)
            {
                Debug.Log(thisMod);
                if (closeModalities.Contains(thisMod))
                {
                    if (setModalities.Contains(thisMod))
                    {
                        inputModalities.Add(thisMod);
                    }
                    else if (getModalities.Contains(thisMod))
                    {
                        outputModalities.Add(thisMod);
                    }
                    else
                    {
                        Debug.Log($"Modality is incomplete. Requires a {thisMod}_get or {thisMod}_set");
                    }
                }
                else
                {
                    Debug.Log($"Modality is incomplete. Requires a {thisMod}_close");
                }
            }

            // robot.RobotConfig.OutputModalities = outputModalities.ToArray();
            // robot.InputModalities = inputModalities.ToArray();
        }

        public static double ConvertToTimestamp(DateTime value)
        {
            //create Timespan by subtracting the value provided from
            //the Unix Epoch
            var span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());

            //return the total seconds (which is a UNIX timestamp)
            return (double) span.TotalSeconds;
        }

        public static Sample DecodeData(DataMessage dmsg)
        {
            var returnSample = new Sample(dmsg.DataType, null);
            if (dmsg.DataType == DataMessage.Types.DataType.Image)
            {
                var msg = new ImageSamples();
                msg.MergeFrom(dmsg.Data);
                returnSample.Data = msg;
                return returnSample;
            } 
            else if (dmsg.DataType == DataMessage.Types.DataType.Audio)
            {
                var msg = new AudioSamples();
                msg.MergeFrom(dmsg.Data);
                returnSample.Data = msg;
                return returnSample;
            } 
            else if (dmsg.DataType == DataMessage.Types.DataType.String)
            {
                var msg = new StringSample();
                msg.MergeFrom(dmsg.Data);
                returnSample.Data = msg;
                return returnSample;
            } 
            else if (dmsg.DataType == DataMessage.Types.DataType.Float32Arr)
            {
                var msg = new Float32Array();
                msg.MergeFrom(dmsg.Data);
                returnSample.Data = msg;
                return returnSample;
            } 
            else if (dmsg.DataType == DataMessage.Types.DataType.Int64Arr)
            {
                var msg = new Int64Array();
                msg.MergeFrom(dmsg.Data);
                returnSample.Data = msg;
                return returnSample;
            }
            else if (dmsg.DataType == DataMessage.Types.DataType.Motor)
            {
                var msg = new MotorSample();
                msg.MergeFrom(dmsg.Data);
                returnSample.Data = msg;
                return returnSample;
            }
            else if (dmsg.DataType == DataMessage.Types.DataType.Blob)
            {
                var msg = new BlobSample();
                msg.MergeFrom(dmsg.Data);
                returnSample.Data = msg;
                return returnSample;
            }
            else
            {
                Debug.Log("Unknown message");
                return null;
            }
        }
    }

    public class Sample
    {
        public DataMessage.Types.DataType DataType;
        public IMessage Data;

        public Sample(DataMessage.Types.DataType dtype, IMessage msg)
        {
            this.DataType = dtype;
            this.Data = msg;
        }
    }

    public class Diode
     {
         private Queue<Sample> _q;
         private int _qBuffer;
         private static Mutex mut = new Mutex();
         
         public Diode(int bufferLength)
         {
             _q = new Queue<Sample>();
             _qBuffer = bufferLength;
         }

         public void Enq(Sample nextSample)
         {
             if (nextSample == null) return;

             mut.WaitOne();
             _q.Enqueue(nextSample);
             if (_q.Count > _qBuffer)
             {
                 _ = _q.Dequeue();
             }
             mut.ReleaseMutex();
         }

         public Sample Deq()
         {
             mut.WaitOne();
             var ret = _q.Count > 0 ?  _q.Dequeue() : null;
             mut.ReleaseMutex();
             return ret;
         }
     }
     
     [Preserve]
     public class FpsLag
     {
         private Stopwatch _stpw;
         private int _count;
         private double _cumulativeLag;
         private readonly string _channelName;
         private string _mod;
         private string _desc;
         private readonly int _interval;
         public float AverageFps;
         public float AverageLag;
         public bool verbose;

         public FpsLag(string channel_name, int interval, string description)
         {
             _channelName = channel_name;
             Debug.Log("Started FpsLag " + channel_name);
             _stpw = Stopwatch.StartNew();
             _count = 0;
             _cumulativeLag = 0;
             _interval = interval;
             _desc = description;
             verbose = true;
         }

         public bool increment(double time)
         {
             _cumulativeLag += AnimusUtils.ConvertToTimestamp(DateTime.Now) - time;
             _count++;
             if (_count < _interval)
             {
                 return false;
             }

             var numMs = _stpw.ElapsedMilliseconds;
             AverageFps = ((float) _count) / (numMs / 1000.0f);
             AverageLag = (float) _cumulativeLag*1000 / _count;

             if (verbose)
             {
                 if (time < 0)
                 {
                     Debug.Log($"{_channelName} : {AverageFps:F2} ------- {_desc}");
                 }
                 else
                 {
                     Debug.Log($"{_channelName} : {AverageFps:F2} ------- {AverageLag:F2}ms lag -- {_desc}");
                 }
             }
             _cumulativeLag = 0;
             _count = 0;
             _stpw.Reset();
             _stpw.Start();
             return true;
         }
     }
}
