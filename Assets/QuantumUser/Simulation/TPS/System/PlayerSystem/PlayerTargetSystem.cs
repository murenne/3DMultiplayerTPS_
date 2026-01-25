using Photon.Deterministic;
using Quantum.Physics3D;
using UnityEngine.Scripting;

namespace Quantum.TPSroject
{
    [Preserve]
    public unsafe class PlayerTargetSystem : SystemMainThreadFilter<PlayerTargetSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef entityRef;
            public Transform3D* transform;
            public PlayerStatusComponent* playerStatusComponent;
            public PlayerTargetComponent* playerTargetComponent;
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

            RefreshAllTargetList(f, entityRef);

            TargetSelection(f, entityRef, input);
        }

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

        /// <summary>
        /// refresh all target list
        /// </summary>
        /// <param name="f"></param>
        /// <param name="entityRef"></param>
        private void RefreshAllTargetList(Frame f, EntityRef entityRef)
        {
            PlayerTargetComponent* playerTargetComponent = f.Unsafe.GetPointer<PlayerTargetComponent>(entityRef);

            playerTargetComponent->targetCount = 0;

            var playerIterator = f.Unsafe.GetComponentBlockIterator<PlayerStatusComponent>();
            foreach (var entityData in playerIterator)
            {
                EntityRef targetEntity = entityData.Entity;
                if (targetEntity == entityRef)
                {
                    continue;
                }

                if (playerTargetComponent->targetCount < playerTargetComponent->targetArray.Length)
                {
                    playerTargetComponent->targetArray[playerTargetComponent->targetCount] = targetEntity;
                    playerTargetComponent->targetCount++;
                }
            }
        }

        /// <summary>
        /// select target 
        /// </summary>
        /// <param name="f"></param>
        /// <param name="entityRef"></param>
        /// <param name="input"></param>
        private void TargetSelection(Frame f, EntityRef entityRef, Input* input)
        {
            PlayerTargetComponent* playerTargetComponent = f.Unsafe.GetPointer<PlayerTargetComponent>(entityRef);

            // no targets available then return
            if (playerTargetComponent->targetCount == 0)
            {
                playerTargetComponent->currentTarget = EntityRef.None;
                playerTargetComponent->currentTargetIndex = -1;
                return;
            }

            // if no current target, or current target is invalid, set to first target in array
            bool currentTargetInvalid = playerTargetComponent->currentTarget == EntityRef.None || !f.Exists(playerTargetComponent->currentTarget);

            if (currentTargetInvalid)
            {
                playerTargetComponent->currentTargetIndex = 0;
                playerTargetComponent->currentTarget = playerTargetComponent->targetArray[0];
            }

            // if press switch target key, switch to next target in array
            if (input->SwitchTarget.WasPressed)
            {
                playerTargetComponent->currentTargetIndex++;

                // if index exceeds the number of targets, wrap around to the first (0)
                if (playerTargetComponent->currentTargetIndex >= playerTargetComponent->targetCount)
                {
                    playerTargetComponent->currentTargetIndex = 0;
                }

                // update entity reference
                playerTargetComponent->currentTarget = playerTargetComponent->targetArray[playerTargetComponent->currentTargetIndex];
            }
        }
    }
}