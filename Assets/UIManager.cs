using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI text;
    public bool useTracking = true;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            useTracking = !useTracking;

            if (useTracking)
            {
                InfiniTAMConnector.sendTransformToCam = true;
                FlyCam.sendTransformToCamera = false;

                text.text = "Tracking";
            }
            else
            {
                InfiniTAMConnector.sendTransformToCam = false;
                FlyCam.sendTransformToCamera = true;
                
                text.text = "Free View";
            }
        }
    }
}
