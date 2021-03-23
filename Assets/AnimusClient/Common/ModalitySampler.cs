using System;
using System.Reflection;
using Animus.ClientSDK;
using UnityEngine;

namespace AnimusCommon
{
    public class ModalitySampler : MonoBehaviour
    {
	    public object client;

        public MethodInfo initModality;
        public MethodInfo setModality;
        public MethodInfo closeModality;
        
        public string modalityName;
        
        public string robotID;
        public bool internalFlag;

        private bool _modalityEnabled;
        
        private FpsLag setModalityFPS;

        private Sample updateNewSamp;
        private GetModalityProto threadNewSamp;
        private Sample myDecodedData;
        private object sampToDecode;
        
        private void Start()
        {
            if (!internalFlag)
            {
                Debug.Log($"Started {modalityName} modality sampler");
                var driverSuccess = (bool)initModality.Invoke(client, new object[0]);
                
                if (driverSuccess)
                {
                    Debug.Log($"{modalityName} Modality opened successfully");
                }
                else
                {
                    Debug.Log($"{modalityName} Modality failed to open");
                }
            }
            else
            {
                Debug.Log($"Starting {modalityName} in internal mode");
            }
            
            Debug.Log($"{modalityName} Human Modality and sampler thread opened successfully");
            
            var openModality = new OpenModalityProto
            {
                ModalityName = modalityName,
                Fps = 30,
            };
            var transportSuccess = AnimusClient.AnimusClient.OpenModality(robotID, openModality);
            if (transportSuccess.Success)
            {
                Debug.Log($"{modalityName} Transport Modality opened successfully");
                _modalityEnabled = true;
                setModalityFPS = new FpsLag(modalityName, 30, "set modality");
            }
            else
            {
                Debug.Log("Open modality error: " + transportSuccess.Description);
            }
        }

        private void Update()
        {
            if (!_modalityEnabled || internalFlag) return;
        
            try
            {
                threadNewSamp = AnimusClient.AnimusClient.GetModality(robotID, modalityName, false);
                if (threadNewSamp == null)
                {
                    return;
                }
                
                if (threadNewSamp.Error != null)
                {
                    if (threadNewSamp.Error.Success)
                    {
                        updateNewSamp = AnimusUtils.DecodeData(threadNewSamp.Sample);
                        setModality.Invoke(client, new object[] {updateNewSamp.Data});
                        setModalityFPS.increment(-1);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Set Modality error for {modalityName} modality: {e}");
            }
        }

        public void StopModality()
        {
            if (!_modalityEnabled) return;

            var closeDriverSuccess = false;
            if (!internalFlag)
            {
                closeDriverSuccess =  (bool) closeModality.Invoke(client, new object[0]);
            }
            else
            {
                closeDriverSuccess = true;
            }
            
            _modalityEnabled = false;
            var closeTransportSuccess = AnimusClient.AnimusClient.CloseModality(robotID, modalityName);
            
            if (closeTransportSuccess.Success && closeDriverSuccess)
            {
                Debug.Log($"{modalityName} Modality closed successfully");
            }
        }
    }
}
