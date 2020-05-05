﻿///<summary>
/// This class handles all the events that are broadcast across classes. The general architecture is
/// that each Controller class receives delegate function calls from children objects and forward the call 
/// to the Events Controller to be broadcast to all classes. 
/// Listeners for these events are defined in the Controller classes. 
///</summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventsController : MonoBehaviour
{
    #region Access

    public static EventsController instance = null;
    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    #endregion Access

    #region Events
    // EyeLink Controller Events.
    public delegate void EyeLateUpdate(Vector2 gazePosition, float[] gazeTargets, float[] gazeCounts);
    public static event EyeLateUpdate OnEyeLateUpdate;
    public void SendEyeLateUpdateEvent(Vector2 gazePosition, float[] gazeTargets, float[] gazeCounts)
    {
        // this line uses a shortcut (?) as a condition, basically it means: 
        // If OnEyeLateUpdate is not null (?), then call the (.)Invoke method. 
        OnEyeLateUpdate?.Invoke(gazePosition, gazeTargets, gazeCounts);
    }

    // Player Controller Events.
    public delegate void PlayerLateUpdate(Vector3 position, float rotation, float state, float hInput, float vInput);
    public static event PlayerLateUpdate OnPlayerLateUpdate;
    public void SendPlayerLateUpdateEvent(Vector3 position, float rotation, float state, float hInput, float vInput)
    {
        OnPlayerLateUpdate?.Invoke(position, rotation, state, hInput, vInput);
    }

    // PhotoDiode Events
    public delegate void PhotoDiodeUpdate(float intensity);
    public static event PhotoDiodeUpdate OnPhotoDiodeUpdate;
    public void SendPhotoDiodeUpdate(float intensity)
    {
        OnPhotoDiodeUpdate?.Invoke(intensity);
    }

    // MonkeyLogic Controller Events. 
    // Out Events
    // Eye Calibration Data received
    public delegate void EyeCalibrationUpdate(EyeCalibrationParameters parameters);
    public static event EyeCalibrationUpdate OnEyeCalibrationUpdate;
    public void SendEyeCalibrationUpdate(EyeCalibrationParameters parameters)
    {
        OnEyeCalibrationUpdate?.Invoke(parameters);
    }


    public delegate void PlaybackParamUpdate(PlaybackTrialParameters parameters);
    public static event PlaybackParamUpdate OnPlaybackParamUpdate;
    public void SendPlaybackParamUpdate(PlaybackTrialParameters parameters)
    {
        OnPlaybackParamUpdate?.Invoke(parameters);
    }

    public delegate void PlaybackDataUpdate(PlaybackTrialData data);
    public static event PlaybackDataUpdate OnPlaybackDataUpdate;
    public void SendPlaybackDataUpdate(PlaybackTrialData data)
    {
        OnPlaybackDataUpdate?.Invoke(data);
    }

    public delegate void PlaybackStart();
    public static event PlaybackStart OnPlaybackStart;
    public void StartPlayback()
    {
        OnPlaybackStart?.Invoke();
    }



    public delegate void Begin();
    public static event Begin OnBegin;
    public void SendBegin()
    {
        OnBegin?.Invoke();
    }

    public delegate void End();
    public static event End OnEnd;
    public void SendEnd()
    {
        OnEnd?.Invoke();
    }

    public delegate void Pause();
    public static event Pause OnPause;
    public void SendPause()
    {
        OnPause?.Invoke();
    }

    public delegate void Resume();
    public static event Resume OnResume;
    public void SendResume()
    {
        OnResume?.Invoke();
    }
    #endregion Events

    // MonkeyLogic publish events
    public delegate void PublishFrame(double[] to_publish);
    public static event PublishFrame OnPublishFrame;
    public void SendPublishFrame(double[] to_publish)
    {
        OnPublishFrame?.Invoke(to_publish);
    }

    public delegate void PublishTrial(string to_publish);
    public static event PublishTrial OnPublishTrial;
    public void SendPublishTrial(string to_publish)
    {
        OnPublishTrial?.Invoke(to_publish);
    }

}
