namespace Quantum.TPSroject
{
    using System;
    using UnityEngine.Scripting;
    //using UnityEngine;

    [Preserve]
    public unsafe class PlayerActionSystem : SystemMainThreadFilter<PlayerActionSystem.Filter>, ISignalOnComponentAdded<PlayerActionComponent>
    {
        public struct Filter
        {
            public EntityRef entityRef;
            public PlayerStatusComponent* playerStatusComponent;
            public PlayerActionComponent* playerActionComponent;
            public PlayerMovementComponent* playerMovementComponent;
            public CharacterController3D* Kcc;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            if (f.Global->gameState == GameState.Initializing)
            {
                return;
            }

            Input* input = default;

            if (f.Unsafe.TryGetPointer(filter.entityRef, out PlayerStatusComponent* playerStatusComponent))
            {
                if (playerStatusComponent->playerRef == PlayerRef.None || !playerStatusComponent->playerRef.IsValid)
                {
                    return;
                }

                input = f.GetPlayerInput(playerStatusComponent->playerRef);
            }

            for (int i = 0; i < filter.playerActionComponent->actionInfoArray.Length; i++)
            {
                ActionType actionType = (ActionType)i;
                ref PlayerActionInfo actionInfo = ref filter.playerActionComponent->actionInfoArray[i];

                actionInfo.ActionUpdate(f, filter.entityRef);

                if (input->IsActionInputWasPressed(actionType))
                {
                    actionInfo.BufferInput(f);
                }

                if (actionInfo.IsInputBuffering)
                {
                    actionInfo.TryActivateAction(f, filter.entityRef, playerStatusComponent->playerRef);
                }
            }
        }

        /// <summary>
        /// 当ActionInventoryComponent被加载时执行
        /// </summary>
        public void OnAdded(Frame f, EntityRef entityRef, PlayerActionComponent* playerActionComponent)
        {
            playerActionComponent->activeActionInfo.activeActionIndex = -1;

            for (int i = 0; i < playerActionComponent->actionInfoArray.Length; i++)
            {
                PlayerActionInfo actionInfo = playerActionComponent->actionInfoArray[i];

                if (actionInfo.actionData.Id.IsValid)
                {
                    PlayerActionData actionData = f.FindAsset<PlayerActionData>(actionInfo.actionData.Id);

                    if (actionData != null)
                    {
                        actionInfo.actionType = actionData.actionType;
                        playerActionComponent->actionInfoArray[i] = actionInfo;
                    }
                }
            }
        }




        //备用singal
        //public void OnActiveActionStopped(Frame f, EntityRef entityRef)
        //{
        //    PlayerActionInventoryComponent* playerActionInventoryComponent = f.Unsafe.GetPointer<PlayerActionInventoryComponent>(entityRef);

        //    if (!playerActionInventoryComponent->HasActiveAction)
        //    {
        //        return;
        //    }

        //    for (int i = 0; i < playerActionInventoryComponent->actions.Length; i++)
        //    {
        //        PlayerAction action = playerActionInventoryComponent->actions[i];

        //        if (action.IsDelayingOrActivating)
        //        {
        //            action.StopAction(f, entityRef);
        //            break;
        //        }
        //    }
        //}

        //备用singal
        //public void OnCooldownsReset(Frame f, EntityRef entityRef)
        //{
        //    PlayerActionInventoryComponent* playerActionInventoryComponent = f.Unsafe.GetPointer<PlayerActionInventoryComponent>(entityRef);

        //    for (int i = 0; i < playerActionInventoryComponent->actions.Length; i++)
        //    {
        //        PlayerAction action = playerActionInventoryComponent->actions[i];

        //        action.ResetCooldown();
        //    }
        //}



    }
}

