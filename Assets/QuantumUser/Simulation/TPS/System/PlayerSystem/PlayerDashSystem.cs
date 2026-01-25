using Photon.Deterministic;
using Quantum.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
namespace Quantum.TPSroject
{
    [Preserve]
    public unsafe class PlayerDashSystem : SystemMainThreadFilter<PlayerDashSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef entityRef;
            public PlayerActionComponent* playerActionComponent;
            public PlayerStatusComponent* playerStatusComponent;
            public PlayerMovementComponent* playerMovementComponent;
            public PlayerDatabaseComponent* playerDatabaseComponent;
            public CharacterController3D* characterController;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            EntityRef entityRef = filter.entityRef;
            PlayerActionComponent* playerActionComponent = f.Unsafe.GetPointer<PlayerActionComponent>(entityRef);
            PlayerDatabaseComponent playerDatabaseComponent = *f.Unsafe.GetPointer<PlayerDatabaseComponent>(entityRef);
            CharacterController3D* kcc = f.Unsafe.GetPointer<CharacterController3D>(entityRef);

            var dashActionInfo = playerActionComponent->actionInfoArray[(int)ActionType.Dash];

            if (dashActionInfo.isActionActiveStart)
            {
                playerActionComponent->actionsParameter.dashAction.dashStartInAir = !kcc->Grounded;
                playerActionComponent->actionsParameter.dashAction.isDashing = true;
                var dashConfig = f.FindAsset<CharacterController3DConfig>(playerDatabaseComponent.bodyDatabase.dashConfig.Id);
                kcc->SetConfig(f, dashConfig);
            }

            if (dashActionInfo.isActionActiveEnd)
            {
                playerActionComponent->actionsParameter.dashAction.isDashing = false;
                var defaultConfig = f.FindAsset<CharacterController3DConfig>(playerDatabaseComponent.bodyDatabase.defaultConfig.Id);
                kcc->SetConfig(f, defaultConfig);
            }

            if (playerActionComponent->actionsParameter.dashAction.isDashing)
            {
                PlayerDashUpdate(f, entityRef);
            }
            else if (playerActionComponent->actionsParameter.dashAction.dashStartInAir && !kcc->Grounded)
            {
                PlayerAirInertiaUpdate(f, entityRef);
            }
            else
            {
                playerActionComponent->actionsParameter.dashAction.dashStartInAir = false;
                playerActionComponent->actionsParameter.dashAction.airDashInertiaDirection = FPVector3.Zero;
            }
        }

        public void PlayerDashUpdate(Frame f, EntityRef entityRef)
        {
            PlayerActionComponent* playerActionComponent = f.Unsafe.GetPointer<PlayerActionComponent>(entityRef);
            CharacterController3D* kcc = f.Unsafe.GetPointer<CharacterController3D>(entityRef);

            kcc->Velocity.Y = FP._0;
            FPVector3 dashDirection = playerActionComponent->activeActionInfo.castDirection;
            FPVector3 horizontalDirection = new FPVector3(dashDirection.X, FP._0, dashDirection.Z).Normalized;
            playerActionComponent->actionsParameter.dashAction.airDashInertiaDirection = horizontalDirection;

            kcc->Move(f, entityRef, horizontalDirection);
        }

        public void PlayerAirInertiaUpdate(Frame f, EntityRef entityRef)
        {
            PlayerActionComponent* playerActionComponent = f.Unsafe.GetPointer<PlayerActionComponent>(entityRef);
            CharacterController3D* kcc = f.Unsafe.GetPointer<CharacterController3D>(entityRef);

            Input* input = default;
            if (f.Unsafe.TryGetPointer(entityRef, out PlayerStatusComponent* playerStatusComponent))
            {
                if (playerStatusComponent->playerRef == PlayerRef.None || !playerStatusComponent->playerRef.IsValid)
                {
                    return;
                }

                input = f.GetPlayerInput(playerStatusComponent->playerRef);
            }

            FPVector3 inputDirection = input->CameraMovementDirection;
            inputDirection.Y = FP._0;
            bool hasInput = inputDirection.Magnitude > FP._0_10;

            FPVector3 targetDirection;
            if (hasInput)
            {
                targetDirection = inputDirection.Normalized;
            }
            else
            {
                targetDirection = playerActionComponent->actionsParameter.dashAction.airDashInertiaDirection;
            }

            kcc->Move(f, entityRef, targetDirection);
        }
    }
}
