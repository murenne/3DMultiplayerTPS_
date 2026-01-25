using Photon.Deterministic;
using UnityEngine.Scripting;
using UnityEngine;
using System.Collections.Generic;

namespace Quantum.TPSroject
{
    [Preserve]
    public unsafe class PlayerSpawnSystem : SystemSignalsOnly, ISignalOnPlayerAdded, ISignalOnComponentAdded<PlayerTargetComponent>
    {
        public void OnPlayerAdded(Frame f, PlayerRef playerRef, bool firstTime)
        {
            // get runtime player data and create player entity
            RuntimePlayer runtimePlayerData = f.GetPlayerData(playerRef);
            EntityPrototype characterPrototype = f.FindAsset<EntityPrototype>(runtimePlayerData.PlayerAvatar);
            EntityRef playerEntityRef = f.Create(characterPrototype);

            // get player's components
            PlayerStatusComponent* playerStatusComponent = f.Unsafe.GetPointer<PlayerStatusComponent>(playerEntityRef);
            Transform3D* playerTransform = f.Unsafe.GetPointer<Transform3D>(playerEntityRef);
            playerStatusComponent->playerRef = playerRef;

            // check team count 
            int teamACount = 0;
            int teamBCount = 0;
            HashSet<EntityRef> occupiedSpawners = new HashSet<EntityRef>();
            var playerFilter = f.Filter<PlayerStatusComponent>();
            while (playerFilter.Next(out EntityRef entityRef, out PlayerStatusComponent enemyStatusComponent))
            {
                if (entityRef == playerEntityRef)
                {
                    continue;
                }

                if (enemyStatusComponent.PlayerTeam == PlayerTeam.A)
                {
                    teamACount++;
                }
                else if (enemyStatusComponent.PlayerTeam == PlayerTeam.B)
                {
                    teamBCount++;
                }

                if (enemyStatusComponent.SpawnerEntityRef != EntityRef.None)
                {
                    occupiedSpawners.Add(enemyStatusComponent.SpawnerEntityRef);
                }
            }

            // decide what team to go
            PlayerTeam targetTeam;
            if (teamACount < teamBCount)
            {
                targetTeam = PlayerTeam.A;
            }
            else if (teamBCount < teamACount)
            {
                targetTeam = PlayerTeam.B;
            }
            else
            {
                targetTeam = (PlayerTeam)f.RNG->Next(0, 2);
            }

            // collect all available spawn points for the target team
            List<EntityRef> targetTeamSpawners = new List<EntityRef>();
            var spawnerFilter = f.Filter<PlayerSpawnerComponent>();
            while (spawnerFilter.Next(out EntityRef spawnerEntity, out PlayerSpawnerComponent playerSpawnerComponent))
            {
                if (playerSpawnerComponent.PlayerTeam == targetTeam && !occupiedSpawners.Contains(spawnerEntity))
                {
                    targetTeamSpawners.Add(spawnerEntity);
                }
            }

            // choose a random spawner from the available spawners
            if (targetTeamSpawners.Count > 0)
            {
                int randomIndex = f.RNG->Next(0, targetTeamSpawners.Count);
                EntityRef chosenSpawnerEntityRef = targetTeamSpawners[randomIndex];
                Transform3D* spawnerTransform = f.Unsafe.GetPointer<Transform3D>(chosenSpawnerEntityRef);

                playerStatusComponent->SpawnerEntityRef = chosenSpawnerEntityRef;
                playerStatusComponent->PlayerTeam = targetTeam;
                playerTransform->Position = spawnerTransform->Position;
                playerTransform->Rotation = spawnerTransform->Rotation;
            }
            else
            {
                playerStatusComponent->PlayerTeam = targetTeam;
                playerTransform->Position = FPVector3.Zero;
                Debug.LogError($"No Spawner found for Team {targetTeam}!");
            }
        }

        public unsafe void OnAdded(Frame f, EntityRef entity, PlayerTargetComponent* playerTargetComponent)
        {
            // Initialize player Target Component 
            playerTargetComponent->currentTarget = EntityRef.None;
            playerTargetComponent->currentTargetIndex = -1;
            playerTargetComponent->targetCount = 0;
        }

        /// <summary>
        /// if you want to assign team and spawn point based on PlayerSpawnerComponent's PlayerRef( you should add it in playerSpawnerComponent by youself ) use this method
        /// </summary>
        // private void AssignTeamAndSpawnPoint(Frame f, PlayerRef playerRef, PlayerStatusComponent* playerStatusComponent, Transform3D* playerTransform)
        // {
        //     // assign team and spawn point
        //     var filtered = f.Filter<PlayerSpawnerComponent, Transform3D>();
        //     while (filtered.NextUnsafe(out var spawnerEntityRef, out var spawner, out var spawnerTransform))
        //     {
        //         if (spawner->PlayerRef == playerRef)
        //         {
        //             playerStatusComponent->SpawnerEntityRef = spawnerEntityRef;
        //             playerStatusComponent->PlayerTeam = spawner->PlayerTeam;
        //             playerTransform->Position = spawnerTransform->Position;
        //             playerTransform->Rotation = spawnerTransform->Rotation;
        //             break;
        //         }
        //     }
        // }
    }
}
