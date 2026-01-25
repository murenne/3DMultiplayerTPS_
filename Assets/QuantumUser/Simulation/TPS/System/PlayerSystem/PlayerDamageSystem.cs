using Photon.Deterministic;
using Quantum.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum.TPSroject
{
    [Preserve]
    public unsafe class PlayerDamageSystem : SystemMainThreadFilter<PlayerDamageSystem.Filter>, ISignalOnCollisionProjectileHitCharacterSignal, ISignalOnPlayerDiedSignal
    {
        public struct Filter
        {
            public EntityRef entityRef;
            public CharacterController3D* characterController;
            public PlayerStatusComponent* playerStatusComponent;
            public PlayerDatabaseComponent* playerDatabaseComponent;
        }

        public override void Update(Frame f, ref PlayerDamageSystem.Filter filter)
        {

        }

        public void OnCollisionProjectileHitCharacterSignal(Frame f, CollisionInfo3D info, ProjectileComponent* projectileComponent, PlayerStatusComponent* playerStatusComponent)
        {
            //info.entity is the projectile
            //info.other is the character

            if (projectileComponent->Owner == info.Other)
            {
                info.IgnoreCollision = true;
                return;
            }

            if (playerStatusComponent->currentHealthPoint <= 0)
            {
                return;
            }

            PlayerDatabaseComponent* playerDatabaseComponent = f.Unsafe.GetPointer<PlayerDatabaseComponent>(info.Other);

            // get projectile's data
            playerStatusComponent->enemyEntityRef = projectileComponent->Owner;
            var enmeyProjectileData = f.FindAsset<PlayerPartsData_Gun>(projectileComponent->gunData.Id);

            // reduce hp and tough point
            playerStatusComponent->currentHealthPoint = FPMath.Max(0, playerStatusComponent->currentHealthPoint - enmeyProjectileData.projectileAttackPoint);
            playerStatusComponent->currentToughPoint = FPMath.Max(0, playerStatusComponent->currentToughPoint - enmeyProjectileData.projectileDownPoint);
            f.Events.OnPlayerDamageReceived(info.Other, playerStatusComponent->currentHealthPoint, playerDatabaseComponent->bodyDatabase.healthPoint);

            // damage buffer time
            playerStatusComponent->damageBufferTimer.TimerSetup(5);


            // check dead
            if (playerStatusComponent->currentHealthPoint <= 0)
            {
                // dead event and destroy player entity
                f.Events.OnPlayerDied(info.Other);
                f.Signals.OnPlayerDiedSignal(info.Other, playerStatusComponent->PlayerTeam);
                f.Destroy(info.Other);
            }
        }

        public void OnPlayerDiedSignal(Frame f, EntityRef playerEntityRef, PlayerTeam playerTeam)
        {
            int teamIndex = (int)playerTeam;
            if (f.Global->teamPlayerCount[teamIndex] > 0)
            {
                f.Global->teamPlayerCount[teamIndex] -= 1;
            }
        }
    }
}
