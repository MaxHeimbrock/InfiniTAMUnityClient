using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Animus.ClientSDK;
using Animus.RobotProto;
using AnimusClient;
using Animus.Structs;
using AnimusCommon;
using Google.Protobuf;
// using AnimusCommon;
using UnityEngine;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

namespace AnimusManager
{
    public class AnimusClientManager : MonoBehaviour
    {
        public bool loginSuccess;
        public bool loginResultAvailable;
        public string loginReturn;

        public bool searchSuccess;
        public bool searchResultsAvailable;
        public string searchReturn;

        public bool connectToRobotFinished;
        public bool connectedToRobotSuccess;

        public bool openModalitiesFinished;
        public bool openModalitiesSuccess;

        public List<Robot> robotDetailsList;
        public string robotResults;
        
        private UnityAnimusClient _client;
        private Robot _chosenRobotDetails;
        private Dictionary<string, ModalitySampler> _modalitySamplers;
        private Dictionary<string, ModalitySetter> _modalitySetters;
        private AudioParams aparams;
        private DateTime lastSearch;
        private int _norobotsCount;
        private int _norobotsMoreDetails = 3;
        private bool _robotConnectionEstablished;

        private void Start()
        {
            loginSuccess = false;
            loginResultAvailable = false;
            loginReturn = "";

            searchResultsAvailable = false;
            searchSuccess = false;
            searchReturn = "";

            lastSearch = DateTime.MinValue;
            _robotConnectionEstablished = false;
            
            Debug.Log(AnimusClient.AnimusClient.Version());

            _client = null;

            aparams = new AudioParams();
            aparams.Channels = 1;
            aparams.SampleRate = 16000;
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                aparams.Backends.Clear();
                aparams.Backends.Add("winmm");
                aparams.TransmitRate = 10;
                aparams.SizeInFrames = false;
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                aparams.Backends.Clear();
                aparams.Backends.Add("");
                aparams.TransmitRate = 10;
                aparams.SizeInFrames = false;
            }
            else if (Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.LinuxPlayer)
            {
                aparams.Backends.Clear();
                aparams.Backends.Add("alsa");
                aparams.TransmitRate = 10;
                aparams.SizeInFrames = false;
            }
            
            var logdir = $"com.{Application.companyName}.{Application.productName}";

            var setupClient = new SetupClientProto();
            setupClient.AudioParams = aparams;
            setupClient.LatencyLogging = true;
            setupClient.LogDir = logdir;

            Debug.Log($"Logging to {logdir}");
            var setupReply = AnimusClient.AnimusClient.SetupClient(setupClient);
            if (!setupReply.Success)
            {
                Debug.Log(setupReply.Description);
            }
            else
            {
                Debug.Log("Client Setup completed successfully");
            }

#if PLATFORM_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Permission.RequestUserPermission(Permission.Microphone);
            }
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageRead);
            }
            
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            }
#endif
        }


        private void Update()
        {
        }

        public void LoginUser(string userEmail, string userPassword)
        {
            StartCoroutine(LoginUserCoroutine(userEmail, userPassword));
        }
        
        public void SearchRobots()
        {
            StartCoroutine(SearchRobotsCoroutine());
        }
        
        public void StartRobotConnection(Robot chosenRobotDetails)
        {
            StartCoroutine(StartRobotConnectionCoroutine(chosenRobotDetails));
        }
        
        public void OpenModalities(string[] enabledModalities)
        {
            StartCoroutine(OpenModalitiesCoroutine(enabledModalities));
        }
        
        public void SetClientClass(UnityAnimusClient clientclass)
        {
            _client = clientclass;
        }
        
        IEnumerator LoginUserCoroutine(string userEmail, string userPassword)
        {
            loginSuccess = false;
            loginResultAvailable = false;
        
            if (userEmail == "" || userPassword == "")
            {
                loginReturn = "User email and password cannot be empty";
                Debug.Log(loginReturn);
                loginResultAvailable = true;
                yield break;
            }
        
            var myNewThread = new Thread(() => ThreadedLogin(userEmail, userPassword));
            myNewThread.Start();
        
            while (!loginResultAvailable)
            {
                yield return null;
            }
        }
        
        IEnumerator SearchRobotsCoroutine()
        {
            robotDetailsList = new List<Robot>();
            searchSuccess = false;
            searchResultsAvailable = false;
            searchReturn = "";
        
            // If not logged in, log in first buddy
            if (!loginSuccess)
            {
                searchReturn = "Must login before trying to search available robots";
                Debug.Log(searchReturn);
                searchResultsAvailable = true;
                yield break;
            }
        
            if ((DateTime.Now - lastSearch).Seconds < 1)
            {
                searchReturn = "Cannot search more than once per second";
                Debug.Log(searchReturn);
                searchResultsAvailable = true;
                yield break;
            }
        
            lastSearch = DateTime.Now;
        
            var chosenRange = new GeoStruct{LowerLat = 0, LowerLong = 0, UpperLat = 0, UpperLong = 0};
            // TODO change search signature to just provide range because local and remote search logic should move into go
            var myNewThread = new Thread(() => ThreadedSearch(false, true, chosenRange));
            myNewThread.Start();
        
            while (!searchResultsAvailable)
            {
                yield return null;
            }
        
            // Debug.Log(robotResults);
            Debug.Log(searchReturn);
            yield return null;
        }
        
        IEnumerator StartRobotConnectionCoroutine(Robot chosenRobotDetails)
        {
            connectedToRobotSuccess = false;
            connectToRobotFinished = false;
        
            // Serialise chosenRobotDetails
        
            Debug.Log($"Connecting to chosen robot {chosenRobotDetails.Make} {chosenRobotDetails.Model} called {chosenRobotDetails.Name} with ID {chosenRobotDetails.RobotId}");
            var connectThread = new Thread(() => ThreadedConnectToRobot(chosenRobotDetails));
        
            connectThread.Start();
            
            // Multiple connection retries
            for (int i = 0; i < 3; i++)
            {
                while (!connectToRobotFinished)
                {
                    yield return null;
                }
        
                if (connectedToRobotSuccess)
                {
                    Debug.Log($"Successfully connected to robot with ID: {chosenRobotDetails.RobotId}");
                    break;
                }
        
                Debug.Log($"Could not connect on attempt {i} of 3");
            }
        
            if (!connectedToRobotSuccess)
            {
                Debug.LogError($"Failed to connect to robot with ID: {chosenRobotDetails.RobotId}");
                UIManager.WriteToLogger("Animus connection failed.");
                CloseInterface();
            }
            else
            {
                _robotConnectionEstablished = true;
                _chosenRobotDetails = chosenRobotDetails;
                UIManager.WriteToLogger("Animus connection successful.");
                UIManager.SetConnectionState(0, true);
            }
        }
        
        IEnumerator OpenModalitiesCoroutine(string[] enabledModalities)
        {
            if (_client == null)
            {
                Debug.Log("No client class defined. Will not open modalities");
            }
        
            openModalitiesFinished = false;
            openModalitiesSuccess = false;
            _modalitySamplers = new Dictionary<string, ModalitySampler>();
            _modalitySetters = new Dictionary<string, ModalitySetter>();
        
            foreach (string mod in enabledModalities)
            {
                bool isInternal = false;
                bool isRobotOutput = _chosenRobotDetails.RobotConfig.OutputModalities.Contains(mod);
                if (_chosenRobotDetails.RobotConfig.InternalModalities != null)
                {
                    if (_chosenRobotDetails.RobotConfig.InternalModalities.Count > 0)
                    {
                        isInternal = _chosenRobotDetails.RobotConfig.InternalModalities.Contains(mod);
                    }
                }
        
                Debug.Log($"Starting {mod} modality coroutine");
        
                // Open human driver modality
                var driverType = _client.GetType();
                var modInit = driverType.GetMethod($"{mod}_initialise");
                var modClose = driverType.GetMethod($"{mod}_close");
                yield return null;
        
                if (modInit != null && modClose != null)
                {
                    if (isRobotOutput)
                    {
                        // A robot output is a human input. So we get from a robot modality and set to the human modality
                        var setHumanModality = driverType.GetMethod($"{mod}_set");
                        var thissampler = this.gameObject.AddComponent<ModalitySampler>();
                        thissampler.initModality = modInit;
                        thissampler.setModality = setHumanModality;
                        thissampler.closeModality = modClose;
                        thissampler.modalityName = mod;
                        thissampler.client = _client;
                        thissampler.robotID = _chosenRobotDetails.RobotId;
                        thissampler.internalFlag = isInternal;
                        _modalitySamplers.Add(mod, thissampler);
                    }
                    else
                    {
                        // A robot input is a human output. So we get from a human modality and set to the robot modality
                        var sampleHumanModality = driverType.GetMethod($"{mod}_get");
                        var thissetter = this.gameObject.AddComponent<ModalitySetter>();
                        thissetter.initModality = modInit;
                        thissetter.getModality = sampleHumanModality;
                        thissetter.closeModality = modClose;
                        thissetter.modalityName = mod;
                        thissetter.client = _client;
                        thissetter.robotID = _chosenRobotDetails.RobotId;
                        thissetter.internalFlag = isInternal;
                        _modalitySetters.Add(mod, thissetter);
                    }
        
                    yield return new WaitForSecondsRealtime(0.5f);
                }
                else
                {
                    Debug.Log($"Human driver does not contain {mod} modality");
                }
            }
        
            openModalitiesFinished = true;
            openModalitiesSuccess = true;
            yield return null;
        }
        
        public void CloseInterface()
        {
            _client = null;
            if (_robotConnectionEstablished)
            {
                foreach (var mod in _modalitySamplers)
                {
                    Debug.Log($"Closing {mod.Key}");
                    mod.Value.StopModality();
                }
        
                _modalitySamplers = new Dictionary<string, ModalitySampler>();
            }
        
            if (_robotConnectionEstablished)
            {
                foreach (var mod in _modalitySetters)
                {
                    Debug.Log($"Closing {mod.Key}");
                    mod.Value.StopModality();
                }
        
                _modalitySetters = new Dictionary<string, ModalitySetter>();
            }
        
            if (!_robotConnectionEstablished) return;
        
            Debug.Log("Close interface triggered");
            AnimusClient.AnimusClient.CloseClientInterface();
            _robotConnectionEstablished = false;
        }
        
        private void ThreadedConnectToRobot(Robot chosenRobot)
        {
            var chosenProto = new ChosenRobotProto {ChosenOne = chosenRobot};
            connectToRobotFinished = false;
            var connectReturn = AnimusClient.AnimusClient.Connect(chosenProto);
            if (connectReturn == null)
            {
                connectedToRobotSuccess = true;
            }
            else
            {
                connectedToRobotSuccess = connectReturn.Success;
            }
            connectToRobotFinished = true;
        }
        
        private void ThreadedSearch(bool getLocal, bool getRemote, GeoStruct chosenRange)
        {
            searchSuccess = false;
            searchResultsAvailable = false;
            searchReturn = "";
            var errorFlag = false;

            var getRobotsRequest = new GetRobotsProtoRequest {GetLocal = false, GetRemote = true, Georange = chosenRange};
            var getRobotsReply = AnimusClient.AnimusClient.GetRobots(getRobotsRequest);

            if (getRobotsReply.LocalSearchError != null)
            {
                if (!getRobotsReply.LocalSearchError.Success)
                {
                    errorFlag = true;
                    searchReturn = getRobotsReply.LocalSearchError.Description;
                }
            }

            if (getRobotsReply.RemoteSearchError != null)
            {
                if (!getRobotsReply.RemoteSearchError.Success)
                {
                    errorFlag = true;
                    searchReturn += " " + getRobotsReply.RemoteSearchError.Description;
                }
            }

            if (errorFlag)
            {
                Debug.Log("Error searching robots: " + searchReturn);
                searchResultsAvailable = true;
                return;
            }

            
            if (getRobotsReply.Robots.Count > 0)
            {
                _norobotsCount = 0;

                foreach (Robot a in getRobotsReply.Robots)
                {
                    robotDetailsList.Add(a); 
                    Debug.Log($"{a.Make} {a.Model} robot called {a.Name}");
                }
                
                Debug.Log("Finished search and robots found");
                searchSuccess = true;
            }
            else
            {
                Debug.Log("Robots not found");
            
                _norobotsCount += 1;
                if (_norobotsCount >= _norobotsMoreDetails)
                {
                    searchReturn =
                        "No robots found. Please search again.\nIf problem persists, check your WiFi connection or the robot's WiFi connection";
                }
                else
                {
                    searchReturn = "No robots found. Please search again.";
                }
            }
            
            searchResultsAvailable = true;
        }
        
        private void ThreadedLogin(string username, string password)
        {
            loginResultAvailable = false;
            var loginProto = new LoginProto();
            loginProto.Password = password;
            loginProto.Username = username;
            loginProto.SystrayLogin = false;

            var loginErr = AnimusClient.AnimusClient.LoginUser(loginProto);
            loginReturn = loginErr.Description;
            loginSuccess = loginErr.Success;
            loginResultAvailable = true;
            if (!loginErr.Success)
            {
                Debug.Log("Login error: " + loginReturn);
            }
            else
            {
                Debug.Log("Login successful");
            }
        }
        
        private void OnApplicationQuit()
        {
            #if !UNITY_EDITOR
            Debug.Log("OnApplicationQuit triggered");
            CloseInterface();
            #endif
        }
        
        private void OnDestroy()
        {
            #if !UNITY_EDITOR
            Debug.Log("OnDestroy triggered");
            CloseInterface();
            #endif

        }
        
        private void OnDisable()
        {
            #if !UNITY_EDITOR
            Debug.Log("OnDisable triggered");
            CloseInterface();
            #endif
        }
    }
}
