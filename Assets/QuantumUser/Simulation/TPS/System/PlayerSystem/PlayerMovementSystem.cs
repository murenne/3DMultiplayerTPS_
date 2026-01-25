using Photon.Deterministic;
using Quantum.Physics3D;
using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum.TPSroject
{
    [Preserve]
    public unsafe class PlayerMovementSystem : SystemMainThreadFilter<PlayerMovementSystem.Filter>
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
            if (f.Global->gameState == GameState.Initializing)
            {
                return;
            }

            EntityRef entityRef = filter.entityRef;

            InputUpdate(f, entityRef, out Input* input);
            if (input == null)
            {
                return;
            }

            PlayerMoveUpdate(f, ref filter, input);
            PlayerRotationUpdate(f, entityRef, input);
        }

        // input 
        public void InputUpdate(Frame f, EntityRef entityRef, out Input* input)
        {
            input = default;
            if (f.Unsafe.TryGetPointer(entityRef, out PlayerStatusComponent* playerStatusComponent))
            {
                if (playerStatusComponent->playerRef == PlayerRef.None || !playerStatusComponent->playerRef.IsValid)
                {
                    return;
                }

                input = f.GetPlayerInput(playerStatusComponent->playerRef);
            }
        }

        // update movement
        public void PlayerMoveUpdate(Frame f, ref Filter filter, Input* input)
        {
            var entityRef = filter.entityRef;
            var playerMovement = filter.playerMovementComponent;
            var kcc = filter.characterController;
            var playerStatusComponent = filter.playerStatusComponent;

            if (playerStatusComponent->playerLocomotionType == PlayerLocomotionType.Dash)
            {
                return;
            }

            FPVector3 inputDirection = new FPVector3(
                input->CameraMovementDirection.X,
                FP._0,
                input->CameraMovementDirection.Z
            );

            bool hasInput = inputDirection.Magnitude > FP._0_10;
            FPVector3 moveDirection = hasInput ? inputDirection.Normalized : FPVector3.Zero;
            kcc->Move(f, entityRef, moveDirection);

            // current horizontal velocity
            playerMovement->currentHorizontalVelocity = new FPVector3(
                kcc->Velocity.X,
                FP._0,
                kcc->Velocity.Z
            );

            // average horizontal velocity
            if (playerMovement->currentHorizontalVelocity.Magnitude > FP._0_01)
            {
                playerMovement->averageHorizontalVelocity = CalculateAverageHorizontalVelocity(f, entityRef, playerMovement->currentHorizontalVelocity);
            }
            else
            {
                playerMovement->averageHorizontalVelocity = FPVector3.Zero;
            }
        }

        // calculate average horizontal velocity
        public FPVector3 CalculateAverageHorizontalVelocity(Frame f, EntityRef entityRef, FPVector3 newVelocity)
        {
            PlayerMovementComponent* playerMovementComponent = f.Unsafe.GetPointer<PlayerMovementComponent>(entityRef);

            playerMovementComponent->velocityCache[playerMovementComponent->currentCacheIndex.AsInt] = newVelocity;
            playerMovementComponent->currentCacheIndex = playerMovementComponent->currentCacheIndex + 1;
            playerMovementComponent->currentCacheIndex %= 3;

            FPVector3 averageHorizontalVelocity = FPVector3.Zero;
            for (int i = 0; i < playerMovementComponent->velocityCache.Length; i++)
            {
                averageHorizontalVelocity += playerMovementComponent->velocityCache[i];
            }

            return averageHorizontalVelocity / 3;
        }

        // player rotation 
        private void PlayerRotationUpdate(Frame f, EntityRef entityRef, Input* input)
        {
            PlayerActionComponent* playerActionComponent = f.Unsafe.GetPointer<PlayerActionComponent>(entityRef);
            PlayerStatusComponent* playerStatusComponent = f.Unsafe.GetPointer<PlayerStatusComponent>(entityRef);
            PlayerTargetComponent* playerTargetComponent = f.Unsafe.GetPointer<PlayerTargetComponent>(entityRef);
            PlayerDatabaseComponent* playerDatabaseComponent = f.Unsafe.GetPointer<PlayerDatabaseComponent>(entityRef);
            CharacterController3D* kcc = f.Unsafe.GetPointer<CharacterController3D>(entityRef);
            Transform3D* transform = f.Unsafe.GetPointer<Transform3D>(entityRef);

            if (playerStatusComponent->IsStunning || playerStatusComponent->IsBreaking)
                return;

            FPVector3 targetDirection = FPVector3.Zero;
            if (playerTargetComponent->currentTarget != EntityRef.None && f.Exists(playerTargetComponent->currentTarget))
            {
                Transform3D targetTransform = f.Get<Transform3D>(playerTargetComponent->currentTarget);
                targetDirection = targetTransform.Position - transform->Position;
            }
            else
            {
                targetDirection = input->CameraMovementDirection;
            }

            targetDirection.Y = FP._0;

            if (targetDirection.Magnitude < FP._0_10)
            {
                return;
            }

            FP turnSpeed = kcc->Grounded
                ? playerDatabaseComponent->bodyDatabase.groundTurnSpeed
                : playerDatabaseComponent->bodyDatabase.airTurnSpeed;

            // calculate target rotation
            FPQuaternion targetRotation = FPQuaternion.LookRotation(targetDirection);

            // rotate towards target rotation
            transform->Rotation = FPQuaternion.RotateTowards(
                transform->Rotation,
                targetRotation,
                turnSpeed * f.DeltaTime * 50
            );
        }
    }
}