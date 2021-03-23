using System;
using Animus.ClientSDK;
using Animus.Common;
using AnimusClient.AnimusClientSDK;
using Google.Protobuf;

namespace AnimusClient
{
    public static class AnimusClient {
        public static string Version()
        {
            return animus_client_sdk.VersionGo();
        }

        public static unsafe Error SetupClient(SetupClientProto setup)
        {
            var buffer = setup.ToByteArray();
            fixed (byte* p = buffer)
            {
                IntPtr ptr = (IntPtr)p;
                var protoMsgC = new ProtoMessageC
                {
                    data = new SWIGTYPE_p_void(ptr, false),
                    len = (uint)buffer.Length
                };
                var sdkReturn = animus_client_sdk.SetupGo(protoMsgC);
                
                if (sdkReturn == null)
                {
                    return new Error{Success = false};
                }
                
                var err = new Error();
                err.MergeFrom(sdkReturn.GetBytes());
                return err;
            }
        }
        
        public static unsafe Error LoginUser(LoginProto login)
        {
            var buffer = login.ToByteArray();
            fixed (byte* p = buffer)
            {
                IntPtr ptr = (IntPtr)p;
                var protoMsgC = new ProtoMessageC
                {
                    data = new SWIGTYPE_p_void(ptr, false),
                    len = (uint)buffer.Length
                };
                var sdkReturn = animus_client_sdk.LoginUserGo(protoMsgC);
                
                if (sdkReturn == null)
                {
                    return new Error{Success = false};
                }
                
                var err = new Error();
                err.MergeFrom(sdkReturn.GetBytes());
                return err;
            }
        }

         public static unsafe GetRobotsProtoReply GetRobots(GetRobotsProtoRequest getRobots)
         {
             var buffer = getRobots.ToByteArray();
             fixed (byte* p = buffer)
             {
                 IntPtr ptr = (IntPtr)p;
                 var protoMsgC = new ProtoMessageC
                 {
                     data = new SWIGTYPE_p_void(ptr, false),
                     len = (uint)buffer.Length
                 };
                 var sdkReturn = animus_client_sdk.GetRobotsGo(protoMsgC);
                
                 if (sdkReturn == null)
                 {
                     return new GetRobotsProtoReply
                     {
                         LocalSearchError = new Error{Success = false},
                         RemoteSearchError = new Error{Success = false},
                     };
                 }
                
                 var err = new GetRobotsProtoReply();
                 err.MergeFrom(sdkReturn.GetBytes());
                 return err;
             }
         }
         
         public static unsafe Error Connect(ChosenRobotProto chosenRobot)
         {
             var buffer = chosenRobot.ToByteArray();
             fixed (byte* p = buffer)
             {
                 IntPtr ptr = (IntPtr)p;
                 var protoMsgC = new ProtoMessageC
                 {
                     data = new SWIGTYPE_p_void(ptr, false),
                     len = (uint)buffer.Length
                 };
                 var sdkReturn = animus_client_sdk.ConnectGo(protoMsgC);
                
                 if (sdkReturn == null)
                 {
                     return new Error{Success = false};
                 }
                
                 var err = new Error();
                 err.MergeFrom(sdkReturn.GetBytes());
                 return err;
             }
         }
         
         public static unsafe Error OpenModality(string robotID, OpenModalityProto openModality)
         {
             var buffer = openModality.ToByteArray();
             fixed (byte* p = buffer)
             {
                 IntPtr ptr = (IntPtr)p;
                 var protoMsgC = new ProtoMessageC
                 {
                     data = new SWIGTYPE_p_void(ptr, false),
                     len = (uint)buffer.Length
                 };
                 var sdkReturn = animus_client_sdk.OpenModalityGo(robotID, protoMsgC);
                
                 if (sdkReturn == null)
                 {
                     return new Error{Success = false};
                 }
                
                 var err = new Error();
                 err.MergeFrom(sdkReturn.GetBytes());
                 return err;
             }
         }
         
         public static unsafe Error SetModality(string robotID, string modalityName, int modalityDataType, IMessage msg)
         {
             var buffer = msg.ToByteArray();
             fixed (byte* p = buffer)
             {
                 IntPtr ptr = (IntPtr)p;
                 var protoMsgC = new ProtoMessageC
                 {
                     data = new SWIGTYPE_p_void(ptr, false),
                     len = (uint)buffer.Length
                 };
                 var sdkReturn = animus_client_sdk.SetModalityGo(robotID, modalityName, modalityDataType, protoMsgC);
                
                 if (sdkReturn == null)
                 {
                     return new Error{Success = false};
                 }
                
                 var err = new Error();
                 err.MergeFrom(sdkReturn.GetBytes());
                 return err;
             }
         }
         
         public static unsafe GetModalityProto GetModality(string robotID, string modalityName, bool blocking)
         {
             var sdkReturn = animus_client_sdk.GetModalityGo(robotID, modalityName, (blocking) ? 1:0 );
                
             if (sdkReturn == null)
             {
                 return new GetModalityProto{Error = new Error{Success = false}};
             }
                
             var err = new GetModalityProto();
             err.MergeFrom(sdkReturn.GetBytes());
             return err;
         }
        
         public static unsafe Error CloseModality(string robotID, string modality)
         {
             var sdkReturn = animus_client_sdk.CloseModalityGo(robotID, modality);
                
             if (sdkReturn == null)
             {
                 return new Error{Success = false};
             }
                
             var err = new Error();
             err.MergeFrom(sdkReturn.GetBytes());
             return err;
         }
         
         public static unsafe Error Disconnect(string robotID)
         {
             var sdkReturn = animus_client_sdk.DisconnectGo(robotID);
                
             if (sdkReturn == null)
             {
                 return new Error{Success = false};
             }
                
             var err = new Error();
             err.MergeFrom(sdkReturn.GetBytes());
             return err;
         }
         
         public static void CloseClientInterface()
         {
             animus_client_sdk.CloseClientInterfaceGo();
         }
    }
}
