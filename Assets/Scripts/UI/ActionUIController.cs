using Quantum;
using System.Collections.Generic;
using UnityEngine;

public unsafe class ActionUIController : MonoBehaviour
{
    [System.Serializable]
    public struct ActionUIInfo
    {
        public ActionType actionType;
        public ActionUI actionUIPrefab;
    }
    [SerializeField]
    private List<ActionUIInfo> _ActionUIInfoList;

    private Dictionary<ActionType, ActionUI> _uiActionsByActionTypeDictionary;

    private void LateUpdate()
    {
        if (_uiActionsByActionTypeDictionary == null)
        {
            return;
        }

        if (LocalPlayerManager.Instance == null || LocalPlayerManager.Instance.LocalPlayer == null)
        {
            return;
        }

        Frame f = QuantumRunner.Default?.Game?.Frames.Predicted;
        if (f == null)
        {
            return;
        }

        UpdateActions(f, LocalPlayerManager.Instance.LocalPlayer);
    }

    public void ActionUIInitialize()
    {
        if (_uiActionsByActionTypeDictionary != null)
        {
            return;
        }

        _uiActionsByActionTypeDictionary = new Dictionary<ActionType, ActionUI>(_ActionUIInfoList.Count);

        foreach (var actionUIInfo in _ActionUIInfoList)
        {
            if (actionUIInfo.actionUIPrefab != null)
            {
                _uiActionsByActionTypeDictionary.Add(actionUIInfo.actionType, actionUIInfo.actionUIPrefab);
                actionUIInfo.actionUIPrefab.gameObject.SetActive(true);
            }
        }

        Frame f = QuantumRunner.Default?.Game?.Frames.Predicted;
        if (f != null && LocalPlayerManager.Instance?.LocalPlayer != null)
        {
            UpdateActions(f, LocalPlayerManager.Instance.LocalPlayer);
        }
    }

    public void UpdateActions(Frame f, PlayerViewController playerViewController)
    {
        EntityRef entity = playerViewController.EntityView.EntityRef;

        if (f.Exists(entity) == false)
        {
            return;
        }

        Quantum.Input* input = f.GetPlayerInput(playerViewController.PlayerRef);
        PlayerStatusComponent* playerStatusComponent = f.Unsafe.GetPointer<PlayerStatusComponent>(playerViewController.EntityView.EntityRef);
        PlayerActionComponent* playerActionComponent = f.Unsafe.GetPointer<PlayerActionComponent>(playerViewController.EntityView.EntityRef);

        for (int i = 0; i < playerActionComponent->actionInfoArray.Length; i++)
        {
            ActionType actionType = (ActionType)i;
            if (_uiActionsByActionTypeDictionary.TryGetValue(actionType, out ActionUI actionUI))
            {
                ref PlayerActionInfo playerAction = ref playerActionComponent->actionInfoArray[i];
                actionUI.UpdateActionUI(f, in playerAction, input->IsActionInputWasPressed(actionType));
            }
        }
    }
}
