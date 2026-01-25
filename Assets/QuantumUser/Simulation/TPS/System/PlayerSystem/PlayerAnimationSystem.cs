using Photon.Deterministic;
using Quantum.Physics3D;
using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum.TPSroject
{

    [Preserve]
    public unsafe class PlayerAnimationSystem : SystemMainThreadFilter<PlayerAnimationSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef entityRef;
            public CharacterController3D* characterController;
            public PlayerStatusComponent* playerStatusComponent;
            public PlayerActionComponent* playerActionComponent;
            public PlayerMovementComponent* playerMovementComponent;
            public PlayerTargetComponent* playerTargetComponent;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            EntityRef entityRef = filter.entityRef;
            PlayerStatusComponent* playerStatusComponent = filter.playerStatusComponent;
            PlayerMovementComponent* playerMovementComponent = filter.playerMovementComponent;
            CharacterController3D* kcc = filter.characterController;

            // 保存上一帧状态
            PlayerLocomotionType lastLocomotionType = playerStatusComponent->lastLocomotionType;
            PlayerPostureType lastPostureType = playerStatusComponent->lastPostureType;

            // 当前状态
            PlayerLocomotionType currentLocomotionType = playerStatusComponent->playerLocomotionType;
            PlayerPostureType currentPostureType = playerStatusComponent->playerPostureType;

            // 🔥 特殊状态优先（每次都触发，因为是瞬间动作）
            if (currentLocomotionType == PlayerLocomotionType.Break && lastLocomotionType != PlayerLocomotionType.Break)
            {
                f.Events.OnPlayerBreaked(entityRef);
                playerStatusComponent->lastLocomotionType = currentLocomotionType;
                return;
            }

            if (currentLocomotionType == PlayerLocomotionType.Stun && lastLocomotionType != PlayerLocomotionType.Stun)
            {
                f.Events.OnPlayerStunned(entityRef);
                playerStatusComponent->lastLocomotionType = currentLocomotionType;
                return;
            }

            if (currentLocomotionType == PlayerLocomotionType.Dash && lastLocomotionType != PlayerLocomotionType.Dash)
            {
                f.Events.OnPlayerDashed(entityRef);
                playerStatusComponent->lastLocomotionType = currentLocomotionType;
                return;
            }

            // 🔥 跳跃/下落（只在姿态改变时触发一次）
            if (currentPostureType == PlayerPostureType.Jumping || currentPostureType == PlayerPostureType.Falling)
            {
                // 只在刚进入 Jumping/Falling 状态时触发
                if (lastPostureType != PlayerPostureType.Jumping && lastPostureType != PlayerPostureType.Falling)
                {
                    // 根据 Y 速度判断是上升还是下落
                    if (kcc->Velocity.Y > 0)
                    {
                        f.Events.OnPlayerJumped(entityRef);
                    }
                    else
                    {
                        f.Events.OnPlayerFalled(entityRef);
                    }
                }
                // 如果已经在空中，检测从上升变成下落
                else if (kcc->Velocity.Y <= 0 && playerStatusComponent->lastVerticalSpeed > 0)
                {
                    f.Events.OnPlayerFalled(entityRef);
                }

                playerStatusComponent->lastVerticalSpeed = kcc->Velocity.Y;
                playerStatusComponent->lastPostureType = currentPostureType;
                return;
            }

            // 🔥 站立时的移动状态（只在状态改变时触发）
            if (currentPostureType == PlayerPostureType.Standing)
            {
                if (lastLocomotionType != currentLocomotionType || lastPostureType != currentPostureType)
                {
                    switch (currentLocomotionType)
                    {
                        case PlayerLocomotionType.Idle:
                            f.Events.OnPlayerIdled(entityRef);
                            break;

                        case PlayerLocomotionType.Move:
                            f.Events.OnPlayerMoved(entityRef);
                            break;

                        case PlayerLocomotionType.Brake:
                            f.Events.OnPlayerBraked(entityRef);
                            break;
                    }
                }
            }

            // 🔥 更新上一帧状态
            playerStatusComponent->lastLocomotionType = currentLocomotionType;
            playerStatusComponent->lastPostureType = currentPostureType;
        }







        public void PlayerAnimationUpdate(Frame f, EntityRef entityRef, Input* input)
        {
            PlayerStatusComponent* playerStatusComponent = f.Unsafe.GetPointer<PlayerStatusComponent>(entityRef);
            PlayerActionComponent* playerActionComponent = f.Unsafe.GetPointer<PlayerActionComponent>(entityRef);
            PlayerDatabaseComponent* playerDatabaseComponent = f.Unsafe.GetPointer<PlayerDatabaseComponent>(entityRef);
            CharacterController3D* kcc = f.Unsafe.GetPointer<CharacterController3D>(entityRef);
            FP currentVerticalSpeed = kcc->Velocity.Y;

            if (playerStatusComponent->IsStunning)
            {
                return;
            }

            if (playerStatusComponent->playerPostureType == PlayerPostureType.Standing)
            {
                switch (playerStatusComponent->playerLocomotionType)
                {
                    case PlayerLocomotionType.Idle:
                        {
                            f.Events.OnPlayerIdled(entityRef);
                            break;
                        }
                    case PlayerLocomotionType.Move:
                        {
                            f.Events.OnPlayerMoved(entityRef);
                            break;
                        }
                    case PlayerLocomotionType.Brake:
                        {
                            f.Events.OnPlayerBraked(entityRef);
                            break;
                        }
                }
            }
            else if (playerStatusComponent->playerPostureType == PlayerPostureType.Jumping)
            {
                f.Events.OnPlayerJumped(entityRef);
            }


            if (playerStatusComponent->playerLocomotionType == PlayerLocomotionType.Break)
            {
                //f.Events.OnPlayerFaceUpDownStarted(entityRef);
            }

            if (playerStatusComponent->playerLocomotionType == PlayerLocomotionType.Dash)
            {
                f.Events.OnPlayerDashed(entityRef);
            }
        }
    }
}

