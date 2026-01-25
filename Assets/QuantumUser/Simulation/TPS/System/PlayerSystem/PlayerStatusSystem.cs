using Photon.Deterministic;
using Quantum.Physics3D;
using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum.TPSroject
{
    [Preserve]
    public unsafe class PlayerStatusSystem : SystemMainThreadFilter<PlayerStatusSystem.Filter>, ISignalOnComponentAdded<PlayerStatusComponent>
    {
        public struct Filter
        {
            public EntityRef entityRef;
            public CharacterController3D* characterController;
            public PlayerStatusComponent* playerStatusComponent;
            public PlayerActionComponent* playerActionComponent;
            public PlayerMovementComponent* playerMovementComponent;
            public PlayerDatabaseComponent* playerDatabaseComponent;
        }

        // Update is called once per frame
        public override void Update(Frame f, ref Filter filter)
        {
            EntityRef entityRef = filter.entityRef;

            InputUpdate(f, entityRef, out Input* input);

            if (input == null)
            {
                return;
            }

            PlayerPostureUpdate(f, entityRef, input);
            PlayerLocomotionUpdate(f, entityRef, input);
        }

        public void InputUpdate(Frame f, EntityRef entityRef, out Input* input)
        {
            PlayerMovementComponent* playerMovementComponent = f.Unsafe.GetPointer<PlayerMovementComponent>(entityRef);
            PlayerStatusComponent* playerStatusComponent = f.Unsafe.GetPointer<PlayerStatusComponent>(entityRef);
            input = default;

            if (f.Unsafe.TryGetPointer(entityRef, out playerStatusComponent))
            {
                if (playerStatusComponent->playerRef == PlayerRef.None || !playerStatusComponent->playerRef.IsValid)
                {
                    return;
                }

                input = f.GetPlayerInput(playerStatusComponent->playerRef);
            }
        }

        public void PlayerPostureUpdate(Frame f, EntityRef entityRef, Input* input)
        {
            PlayerActionComponent* playerActionComponent = f.Unsafe.GetPointer<PlayerActionComponent>(entityRef);
            PlayerStatusComponent* playerStatusComponent = f.Unsafe.GetPointer<PlayerStatusComponent>(entityRef);
            CharacterController3D* kcc = f.Unsafe.GetPointer<CharacterController3D>(entityRef);

            FP verticalSpeed = kcc->Velocity.Y;

            // ground
            if (kcc->Grounded)
            {
                playerStatusComponent->playerPostureType = PlayerPostureType.Standing;
                return;
            }

            // air
            if (verticalSpeed > FP._0_10)
            {
                playerStatusComponent->playerPostureType = PlayerPostureType.Jumping;
            }
            else
            {
                playerStatusComponent->playerPostureType = PlayerPostureType.Falling;
            }
        }

        public void PlayerLocomotionUpdate(Frame f, EntityRef entityRef, Input* input)
        {
            PlayerStatusComponent* playerStatusComponent = f.Unsafe.GetPointer<PlayerStatusComponent>(entityRef);
            PlayerActionComponent* playerActionComponent = f.Unsafe.GetPointer<PlayerActionComponent>(entityRef);
            CharacterController3D* kcc = f.Unsafe.GetPointer<CharacterController3D>(entityRef);

            if (playerStatusComponent->IsBreaking)
            {
                playerStatusComponent->playerLocomotionType = PlayerLocomotionType.Break;
            }
            else if (playerActionComponent->actionsParameter.dashAction.isDashing)
            {
                playerStatusComponent->playerLocomotionType = PlayerLocomotionType.Dash;
            }
            else if (input->Movement.Magnitude == 0)
            {
                if (kcc->Velocity.XOZ.Magnitude > 0 && kcc->Grounded)
                {
                    playerStatusComponent->playerLocomotionType = PlayerLocomotionType.Brake;
                }
                else
                {
                    playerStatusComponent->playerLocomotionType = PlayerLocomotionType.Idle;
                }
            }
            else if (input->Movement.Magnitude != 0)
            {
                playerStatusComponent->playerLocomotionType = PlayerLocomotionType.Move;
            }
        }

        public unsafe void OnAdded(Frame f, EntityRef entity, PlayerStatusComponent* component)
        {
            PlayerStatusComponent* playerStatusComponent = f.Unsafe.GetPointer<PlayerStatusComponent>(entity);
            PlayerDatabaseComponent* playerDatabaseComponent = f.Unsafe.GetPointer<PlayerDatabaseComponent>(entity);

            playerStatusComponent->currentHealthPoint = playerDatabaseComponent->bodyDatabase.healthPoint;
            playerStatusComponent->currentToughPoint = playerDatabaseComponent->bodyDatabase.toughPoint;
        }
    }
}
