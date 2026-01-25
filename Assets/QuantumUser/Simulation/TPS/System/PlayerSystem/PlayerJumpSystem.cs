using Photon.Deterministic;
using Quantum.Physics3D;
using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum.TPSroject
{
    [Preserve]
    public unsafe class PlayerJumpSystem : SystemMainThreadFilter<PlayerJumpSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef entityRef;
            public CharacterController3D* characterController;
            public PlayerStatusComponent* playerStatusComponent;
            public PlayerActionComponent* playerActionComponent;
            public PlayerMovementComponent* playerMovementComponent;
            public PlayerTargetComponent* playerTargetComponent;
            public Transform3D* transform;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            PlayerActionComponent* playerActionComponent = f.Unsafe.GetPointer<PlayerActionComponent>(filter.entityRef);
            PlayerDatabaseComponent* playerDatabaseComponent = f.Unsafe.GetPointer<PlayerDatabaseComponent>(filter.entityRef);
            CharacterController3D* kcc = filter.characterController;
            CharacterController3DConfig characterController3DConfig = f.FindAsset<CharacterController3DConfig>(kcc->Config.Id);

            var jumpActionInfo = playerActionComponent->actionInfoArray[(int)ActionType.Jump];
            if (jumpActionInfo.isActionActiveStart)
            {
                kcc->Jump(f, false);
            }

            if (!kcc->Grounded && kcc->Velocity.Y < 0)
            {
                kcc->Velocity.Y -= playerDatabaseComponent->bodyDatabase.fallSpeedMultiplier * f.DeltaTime;
            }

            FP maxFallSpeed = characterController3DConfig.Gravity.Y * FP._2;
            if (kcc->Velocity.Y < maxFallSpeed)
            {
                kcc->Velocity.Y = maxFallSpeed;
            }

            if (kcc->Grounded && kcc->Velocity.Y < FP._0)
            {
                kcc->Velocity.Y = FP._0;
            }

            FP currentVerticalVelocity = kcc->Velocity.Y;
            playerActionComponent->actionsParameter.jumpAction.currentVerticalVelocity = currentVerticalVelocity;
        }
    }
}
