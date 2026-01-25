using Quantum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class LocalPlayerManager : MonoBehaviour
{
    public static LocalPlayerManager Instance { get; private set; }
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private PlayerInput _playerInput;
    public CameraController CameraController => _cameraController;
    public PlayerInput PlayerInput => _playerInput;
    public PlayerViewController LocalPlayer { get; private set; }


    public void InitializeLocalPlayer(PlayerViewController localPlayer)
    {
        LocalPlayer = localPlayer;
    }

    private void Awake()
    {
        Instance = this;
    }
}
