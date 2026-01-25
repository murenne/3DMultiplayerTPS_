using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quantum;

public class CinematicCameraManager : MonoBehaviour
{
    public static CinematicCameraManager Instance { get; private set; }
    private Dictionary<CinematicCameraType, ICinematicCamera> _cinematicCameraDictionary = new Dictionary<CinematicCameraType, ICinematicCamera>();

    private void Awake()
    {
        Instance = this;
        QuantumEvent.Subscribe<EventOnGameInitializing>(this, OnGameInitializing);
        QuantumEvent.Subscribe<EventOnGameStarting>(this, OnGameStarting);
        QuantumEvent.Subscribe<EventOnGameOver>(this, OnGameOver);
    }

    public void RegisterCamera(ICinematicCamera cinematicCamera)
    {
        if (cinematicCamera != null && !_cinematicCameraDictionary.ContainsKey(cinematicCamera.cinematicCameraType))
        {
            _cinematicCameraDictionary.Add(cinematicCamera.cinematicCameraType, cinematicCamera);
        }
    }

    public void UnregisterCamera(CinematicCameraType cinematicCameraType)
    {
        if (_cinematicCameraDictionary.ContainsKey(cinematicCameraType))
        {
            _cinematicCameraDictionary.Remove(cinematicCameraType);
        }
    }

    private void OnGameInitializing(EventOnGameInitializing e)
    {
        PlayCinematicCamera(CinematicCameraType.StartCamera);
    }

    private void OnGameStarting(EventOnGameStarting e)
    {
        StopAllCinematicCameras();
    }

    private void OnGameOver(EventOnGameOver e)
    {
        // PlayCinematicCamera("WinCinematicCamera");
    }

    public void PlayCinematicCamera(CinematicCameraType cinematicCameraType)
    {
        StopAllCinematicCameras();

        if (_cinematicCameraDictionary.TryGetValue(cinematicCameraType, out var cinematicCamera))
        {
            cinematicCamera.Play();
        }
        else
        {
            Debug.LogWarning($"[CinematicCamera] Camera '{cinematicCameraType}' not found!");
        }
    }

    public void StopAllCinematicCameras()
    {
        foreach (var cinematicCamera in _cinematicCameraDictionary.Values)
        {
            cinematicCamera.End();
        }
    }
}
