using UnityEngine;
using Quantum;

[RequireComponent(typeof(Camera))]
public unsafe class UICameraController : MonoBehaviour
{
    private Camera _uiCamera;

    // 如果你没有把 UI Camera 设为 Main Camera 的子物体
    // 你需要拖拽 Main Camera 到这里来实现位置同步
    [Header("Optional: Sync Target")]
    [SerializeField] private Transform _targetMainCamera;

    private void Awake()
    {
        _uiCamera = GetComponent<Camera>();

        QuantumEvent.Subscribe<EventOnGameInitializing>(this, OnGameInitializing);
        QuantumEvent.Subscribe<EventOnGameStarting>(this, OnGameStarting);
        // 为了防止中途加入或其他情况，Running 时也确保它是开的
        QuantumEvent.Subscribe<EventOnGameRunning>(this, OnGameRunning);
    }

    private void Start()
    {
        // --- 补丁逻辑：防止进入场景时事件已经触发过了 ---
        var f = QuantumRunner.Default?.Game?.Frames.Predicted;
        if (f != null)
        {
            if (f.Global->gameState == GameState.Initializing)
            {
                _uiCamera.enabled = false;
            }
            else
            {
                _uiCamera.enabled = true;
            }
        }
    }

    private void LateUpdate()
    {
        // 位置同步逻辑
        // 如果你的 UI Camera 已经是 Main Camera 的子物体，这段可以删掉
        if (_targetMainCamera != null)
        {
            transform.position = _targetMainCamera.position;
            transform.rotation = _targetMainCamera.rotation;
        }
    }

    private void OnDestroy()
    {
        // QuantumEvent.UnsubscribeListener(this);
    }

    // --- 事件处理 ---

    private void OnGameInitializing(EventOnGameInitializing e)
    {
        // Initializing 阶段：关闭 UI 相机 (只显示特写)
        _uiCamera.enabled = false;
    }

    private void OnGameStarting(EventOnGameStarting e)
    {
        // Starting 阶段：打开 UI 相机 (显示倒计时等 UI)
        _uiCamera.enabled = true;
    }

    private void OnGameRunning(EventOnGameRunning e)
    {
        // Running 阶段：确保打开
        _uiCamera.enabled = true;
    }
}