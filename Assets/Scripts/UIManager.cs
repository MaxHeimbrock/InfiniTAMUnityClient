using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI trackingStateText;
    public TextMeshProUGUI loggerText;
    public bool useTracking = true;

    private static string log = "";
    private static bool newMessage = false;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (newMessage)
        {
            loggerText.text = log;
            newMessage = false;
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            useTracking = !useTracking;

            if (useTracking)
            {
                UnityAnimusClient.sendTransformToCam = true;
                FlyCam.sendTransformToCamera = false;

                trackingStateText.text = "Tracking";
            }
            else
            {
                UnityAnimusClient.sendTransformToCam = false;
                FlyCam.sendTransformToCamera = true;
                
                trackingStateText.text = "Free View";
            }
        }
    }

    public static void WriteToLogger(string logMessage)
    {
        log = log + "\n\n[" + 
              System.DateTime.Now.ToString("HH:mm:ss") + "]\n" +
              logMessage;
        newMessage = true;
    }
}
