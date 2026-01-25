using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public unsafe partial struct PlayerActionInfo
    {
        public bool IsInputBuffering => inputBufferTimer.IsRunning;
        public bool IsActivating => activateTimer.IsRunning;
        public bool IsCoolingDown => cooldownTimer.IsRunning;

        /// <summary>
        /// Press the action button
        /// </summary>
        public void BufferInput(Frame f)
        {
            PlayerActionData playerActionData = f.FindAsset<PlayerActionData>(actionData.Id);
            inputBufferTimer.TimerSetup(playerActionData.inputBufferDuration);
        }

        /// <summary>
        /// Try to activate action
        /// </summary>
        public void TryActivateAction(Frame f, EntityRef entityRef, PlayerRef playerRef)
        {
            PlayerStatusComponent* playerStatusComponent = f.Unsafe.GetPointer<PlayerStatusComponent>(entityRef);
            PlayerActionComponent* playerActionComponent = f.Unsafe.GetPointer<PlayerActionComponent>(entityRef);
            Transform3D* transform = f.Unsafe.GetPointer<Transform3D>(entityRef);
            PlayerActionData playerActionData = f.FindAsset<PlayerActionData>(actionData.Id);

            if (IsCoolingDown || playerStatusComponent->IsIncapacitating || playerActionComponent->HasActiveAction)
            {
                return;
            }

            inputBufferTimer.TimerReset();
            activateTimer.TimerSetup(playerActionData.activateDuration);
            isActionActiveStart = true;

            // Active Action Index
            playerActionComponent->activeActionInfo.activeActionIndex = (int)actionType;
            playerActionComponent->activeActionInfo.castDirection = GetCastDirection(f, playerRef, transform);

            playerActionData.OnActionStart(f, entityRef);
        }

        /// <summary>
        /// Update action
        /// </summary>
        public void ActionUpdate(Frame f, EntityRef entityRef)
        {
            PlayerActionData playerActionData = f.FindAsset<PlayerActionData>(actionData.Id);

            isActionActiveStart = false;
            isActionActiveEnd = false;

            inputBufferTimer.TimerTick(f.DeltaTime);
            cooldownTimer.TimerTick(f.DeltaTime);

            //如果活跃状态
            if (IsActivating)
            {
                PlayerStatusComponent* playerStatusComponent = f.Unsafe.GetPointer<PlayerStatusComponent>(entityRef);

                //如果在禁用状态（硬直，break），直接打断action
                if (playerStatusComponent->IsIncapacitating)
                {
                    if (!(playerActionData is PlayerActionData_Gun))
                    {
                        StopAction(f, entityRef);
                    }
                }

                activateTimer.TimerTick(f.DeltaTime);

                if (activateTimer.IsDone)
                {
                    cooldownTimer.TimerSetup(playerActionData.cooldownDuration);

                    isActionActiveEnd = true;
                    playerActionData.OnActionEnd(f, entityRef);

                    StopAction(f, entityRef);
                }
                else
                {
                    playerActionData.OnActionUpdate(f, entityRef);
                }
            }
        }

        /// <summary>
        /// Stop action
        /// </summary>
        public void StopAction(Frame f, EntityRef entityRef)
        {
            PlayerActionComponent* playerActionComponent = f.Unsafe.GetPointer<PlayerActionComponent>(entityRef);
            playerActionComponent->activeActionInfo.activeActionIndex = -1;

            activateTimer.TimerReset();
        }

        private FPVector3 GetCastDirection(Frame f, PlayerRef playerRef, Transform3D* transform)
        {
            Input* input = f.GetPlayerInput(playerRef);
            var castDirection = input->Movement.Magnitude > 0 ? input->CameraMovementDirection : transform->Forward;

            return castDirection;
        }

        public void ResetCooldown()
        {
            cooldownTimer.TimerReset();
        }
    }
}
