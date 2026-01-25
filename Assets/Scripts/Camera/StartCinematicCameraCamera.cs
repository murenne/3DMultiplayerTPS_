using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Quantum;
using Quantum.TPSroject;

public class StartCinematicCamera : MonoBehaviour, ICinematicCamera
{
    public CinematicCameraType cinematicCameraType => CinematicCameraType.StartCamera;

    [SerializeField] CinemachineBlendDefinition _defaultBlend;
    [SerializeField] private CinemachineVirtualCamera _teamAStartCinematicCamera;
    [SerializeField] private CinemachineVirtualCamera _teamBStartCinematicCamera;
    private CinemachineBrain _brain;

    private void Awake()
    {
        if (_teamAStartCinematicCamera)
        {
            _teamAStartCinematicCamera.enabled = false;
        }
        if (_teamBStartCinematicCamera)
        {
            _teamBStartCinematicCamera.enabled = false;
        }

        _brain = FindFirstObjectByType<CinemachineBrain>();
    }

    private void Start()
    {
        if (CinematicCameraManager.Instance != null)
        {
            CinematicCameraManager.Instance.RegisterCamera(this);
        }
    }

    private void OnDestroy()
    {
        if (CinematicCameraManager.Instance != null)
        {
            CinematicCameraManager.Instance.UnregisterCamera(cinematicCameraType);
        }
    }

    public void Play()
    {
        if (_brain != null)
        {
            _brain.m_DefaultBlend = _defaultBlend;
        }

        var localPlayer = LocalPlayerManager.Instance?.LocalPlayer;
        if (localPlayer == null)
        {
            return;
        }

        if (localPlayer.PlayerTeam == PlayerTeam.A)
        {
            if (_teamAStartCinematicCamera)
            {
                _teamAStartCinematicCamera.enabled = true;
            }
        }
        else
        {
            if (_teamBStartCinematicCamera)
            {
                _teamBStartCinematicCamera.enabled = true;
            }
        }
    }

    public void End()
    {
        if (_teamAStartCinematicCamera)
        {
            _teamAStartCinematicCamera.enabled = false;
        }
        if (_teamBStartCinematicCamera)
        {
            _teamBStartCinematicCamera.enabled = false;
        }
    }
}
