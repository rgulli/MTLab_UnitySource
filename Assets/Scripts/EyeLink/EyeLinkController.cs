///<summary>
/// This script interfaces with the EyeLink system. The public variables are for configuration
/// and include screen proprotions, EyeLink IP address and such. 
/// 
/// 
/// 
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SREYELINKLib;


public class EyeLinkController : MonoBehaviour
{

    private Vector2 _eyeRaw = new Vector2();
    private Vector2 _eyeDeg = new Vector2();
    private Vector2 _eyePix = new Vector2();
    private string _gazeTargets;

    // Eye Link settings
    private EL_EYE el_Eye = EL_EYE.EL_EYE_NONE;
    private EyeLinkUtil el_Util;
    private EyeLink el;
        
    // Calibration script
    private EyeCalibration eyecal;

    // Gaze 
    private GazeProcessing gaze;

    // Start is called before the first frame update
    void Start()
    {
        eyecal = gameObject.AddComponent<EyeCalibration>();
        gaze = gameObject.AddComponent<GazeProcessing>();
        el = new EyeLink();
        el_Util = new EyeLinkUtil();

        // Add Listeners
        EventsController.OnEyeCalibrationUpdate += gaze.UpdateCalibration;
        EventsController.OnEyeCalibrationUpdate += eyecal.UpdateCalibration;
    }

    private void OnDisable()
    {
        EventsController.OnEyeCalibrationUpdate -= gaze.UpdateCalibration;
        EventsController.OnEyeCalibrationUpdate -= eyecal.UpdateCalibration;
    }

    // Update is called once per frame
    void Update()
    {
        ISample s;
        //Sample s;
        double lastSampleTime = 0.0;

        // if not connected but has eye calibration: Initialize
        if (!el.isConnected() && eyecal.has_calibration)
        {
            
            // Configure eye
            switch (eyecal.GetTrackedEye())
            {
                case 0:
                    el_Eye = EL_EYE.EL_LEFT;
                    break;
                case 1:
                    el_Eye = EL_EYE.EL_RIGHT;
                    break;
                default:
                    el_Eye = EL_EYE.EL_EYE_NONE;
                    break;
            }

            // Connect
            el.setEyelinkAddress(eyecal.GetEyeLinkIP(), -1);
            el.broadcastOpen(); // TODO: Fix. REALLY SLOW if the eyelink is not available. 
            s = null;
        }

        // If connected, has calibration and tracker is in record mode: get sample    
        else if (el.isConnected() && eyecal.has_calibration && el.getTrackerMode() == 14)
        {
            try
            {
                s = el.getNewestSample();
            }
            catch (Exception e)
            {
                s = null;
                //Debug.Log(e.Message);
            }
        }
        else
        {
            s = null;
        }
        
        // Get position on screen in pixels
        if (s != null && s.time != lastSampleTime)
        {
            if (el_Eye != EL_EYE.EL_EYE_NONE)
            {
                if (el_Eye == EL_EYE.EL_BINOCULAR)
                    el_Eye = EL_EYE.EL_LEFT;

                _eyeRaw.x = s.get_px(el_Eye);
                _eyeRaw.y = s.get_py(el_Eye);

                eyecal.RawToPix(_eyeRaw, out _eyeDeg, out _eyePix);
                _gazeTargets = gaze.ProcessGaze(_eyePix);

                lastSampleTime = s.time;
            }
            else
            {
                el_Eye = (EL_EYE)el.eyeAvailable();
            }
        }

        // Update values to the experiment controller
        EventsController.instance.SendEyeLateUpdateEvent(_eyeDeg, _gazeTargets);
    }
    
    private void OnDestroy()
    {
        el.stopRecording();
        el.closeDataFile();
        el.close();
            
    }
}
