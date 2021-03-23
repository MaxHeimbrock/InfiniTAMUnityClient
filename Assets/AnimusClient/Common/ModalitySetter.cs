using System.Collections;
using System.Reflection;
using Animus.ClientSDK;
using UnityEngine;

namespace AnimusCommon
{
    public class ModalitySetter : MonoBehaviour
    {
        public object client;
        
        public MethodInfo initModality;
        public MethodInfo getModality;
        public MethodInfo closeModality;
        public string modalityName;

        public string robotID;
        public bool internalFlag;
        
        private float requested_rate;
        private bool _modalityEnabled;
        private float delayAccum;
        
        private FpsLag getModalityFPS;

        private int fpsinterval = 200;
        
        
        private void Start()
        {
            requested_rate = 30;
            
            if (!internalFlag)
            {
                _modalityEnabled = false;
                Debug.Log($"Started {modalityName} modality setter");
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
                Debug.Log($"Started {modalityName} modality setter in internal mode");
            }
            
            Debug.Log($"{modalityName} Human Modality opened successfully");
            bool isinterval = false;

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
                getModalityFPS = new FpsLag(modalityName, fpsinterval, "end of update to end of update");
            }
            else
            {
                Debug.Log("Open modality error: " + transportSuccess.Description);
            }

            if  (!internalFlag)
            {
                StartCoroutine(SetterCoroutine());
            }
        }

        IEnumerator SetterCoroutine()
        {
            while (_modalityEnabled)
            {
                var thisSample = (Sample)getModality.Invoke(client, new object[] {});
                if (thisSample != null)
                {
                    var success = AnimusClient.AnimusClient.SetModality(robotID, modalityName, (int) thisSample.DataType, thisSample.Data);
                    getModalityFPS.increment(-1);
                }
                yield return null;
            }
        }

        public void StopModality()
        {
            if (!_modalityEnabled) return;
            
            _modalityEnabled = false;
            var closeTransportSuccess = AnimusClient.AnimusClient.CloseModality(robotID, modalityName);
            
            var closeDriverSuccess = false;
            if (!internalFlag)
            {
                closeDriverSuccess = (bool)closeModality.Invoke(client, new object[0]);
            }
            else
            {
                closeDriverSuccess = true;
            }
            
            if (closeTransportSuccess.Success && closeDriverSuccess)
            {
                Debug.Log($"{modalityName} Modality setter closed successfully");
            }
        }
    }
}
