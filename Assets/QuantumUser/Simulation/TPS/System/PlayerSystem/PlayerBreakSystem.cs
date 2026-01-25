using Photon.Deterministic;
using Quantum.Collections;
using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum.TPSroject
{
    [Preserve]
    public unsafe class PlayerBreakSystem : SystemMainThreadFilter<PlayerBreakSystem.Filter>, ISignalOnCollisionProjectileHitCharacterSignal, ISignalOnCollisionCharacterHitNormalStaticObjectSignal
    {
        public struct Filter
        {
            public EntityRef entityRef;
            public CharacterController3D* characterController;
            public PhysicsCollider3D* PhysicsCollider;
            public PlayerStatusComponent* playerStatusComponent;
            public PlayerActionComponent* playerActionComponent;
            public PlayerMovementComponent* playerMovementComponent;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            EntityRef entityRef = filter.entityRef;
            PlayerStatusComponent* playerStatusComponent = f.Unsafe.GetPointer<PlayerStatusComponent>(entityRef);
            PlayerDatabaseComponent* playerDatabaseComponent = f.Unsafe.GetPointer<PlayerDatabaseComponent>(entityRef);

            // recover tough point over time
            if (playerStatusComponent->currentToughPoint < playerDatabaseComponent->bodyDatabase.toughPoint)
            {
                playerStatusComponent->damageBufferTimer.TimerTick(f.DeltaTime);
                if (!playerStatusComponent->IsDamageBuffering)
                {
                    playerStatusComponent->currentToughPoint = FPMath.Min(playerDatabaseComponent->bodyDatabase.toughPoint, playerStatusComponent->currentToughPoint + f.DeltaTime * playerDatabaseComponent->bodyDatabase.toughPointRecoverAmount);
                }
            }

            // skip if not breaking
            if (!filter.playerStatusComponent->IsBreaking)
            {
                playerStatusComponent->breakStatusEffect.isHitWall = false;
                return;
            }

            BreakUpdate(f, entityRef);
            StunUpdate(f, entityRef);
        }

        public void OnCollisionCharacterHitNormalStaticObjectSignal(Frame f, CollisionInfo3D info, PlayerStatusComponent* playerStatusComponent)
        {
            //info.entity is character
            //info.other is staticobject

            if (playerStatusComponent->IsBreaking)
            {
                CharacterController3D* kcc = f.Unsafe.GetPointer<CharacterController3D>(info.Entity);
                FP verticalSpeed = kcc->Velocity.Y;
                if (verticalSpeed > 0)
                {
                    verticalSpeed = 0;
                }

                kcc->Velocity = new FPVector3(0, verticalSpeed, 0);
                playerStatusComponent->breakStatusEffect.isHitWall = true;
            }
        }

        public void OnCollisionProjectileHitCharacterSignal(Frame f, CollisionInfo3D info, ProjectileComponent* projectileComponent, PlayerStatusComponent* playerStatusComponent)
        {
            //info.entity is projectile
            //info.other is character

            if (projectileComponent->Owner == info.Other)
            {
                info.IgnoreCollision = true;
                return;
            }

            Transform3D* projectileTransform = f.Unsafe.GetPointer<Transform3D>(info.Entity);
            PlayerDatabaseComponent* playerDatabaseComponent = f.Unsafe.GetPointer<PlayerDatabaseComponent>(info.Other);
            CharacterController3D* kcc = f.Unsafe.GetPointer<CharacterController3D>(info.Other);

            playerStatusComponent->enemyEntityRef = projectileComponent->Owner;
            PlayerDatabaseComponent* enemyDatabaseComponent = f.Unsafe.GetPointer<PlayerDatabaseComponent>(playerStatusComponent->enemyEntityRef);
            var gunData = GetGunDataFromEnemy(f, enemyDatabaseComponent);

            if (playerStatusComponent->currentToughPoint >= 0)
            {
                // apply tough point damage
                playerStatusComponent->currentToughPoint = FPMath.Max(0, playerStatusComponent->currentToughPoint - enemyDatabaseComponent->gunDatabase.projectileDownPoint);
            }

            // enter break
            if (playerStatusComponent->currentToughPoint <= 0 && !playerStatusComponent->IsBreaking)
            {
                // calculate bullet directionq
                FPVector3 bulletDirection = projectileTransform->Forward;
                FPVector3 horizontalDirection = new FPVector3(bulletDirection.X, 0, bulletDirection.Z).Normalized;

                // break movement
                FP horizontalForce = gunData.downMovenmentDistanceXZ;
                FP upwardForce = gunData.downMovenmentDistanceY;

                kcc->Velocity = horizontalDirection * horizontalForce + FPVector3.Up * upwardForce;

                // setup 
                playerStatusComponent->playerLocomotionType = PlayerLocomotionType.Break;
                playerStatusComponent->breakStatusEffect.durationTimer.TimerSetup(1);
                playerStatusComponent->unbeatableTimer.TimerSetup(1);
                playerStatusComponent->breakStatusEffect.isHitWall = false;
            }
        }

        public void BreakUpdate(Frame f, EntityRef entityRef)
        {
            PlayerActionComponent* playerActionComponent = f.Unsafe.GetPointer<PlayerActionComponent>(entityRef);
            PlayerStatusComponent* playerStatusComponent = f.Unsafe.GetPointer<PlayerStatusComponent>(entityRef);
            PlayerDatabaseComponent* playerDatabaseComponent = f.Unsafe.GetPointer<PlayerDatabaseComponent>(entityRef);
            CharacterController3D* kcc = f.Unsafe.GetPointer<CharacterController3D>(entityRef);

            if (playerStatusComponent->playerLocomotionType != PlayerLocomotionType.Break)
                return;

            // if on ground
            if (kcc->Grounded)
            {
                kcc->Velocity = FPVector3.Zero;
                playerStatusComponent->playerLocomotionType = PlayerLocomotionType.Stun;
                playerStatusComponent->stunStatusEffect.durationTimer.TimerSetup(playerDatabaseComponent->bodyDatabase.breakDuration);
            }
        }

        public void StunUpdate(Frame f, EntityRef entityRef)
        {
            PlayerActionComponent* playerActionComponent = f.Unsafe.GetPointer<PlayerActionComponent>(entityRef);
            PlayerStatusComponent* playerStatusComponent = f.Unsafe.GetPointer<PlayerStatusComponent>(entityRef);
            PlayerDatabaseComponent* playerDatabaseComponent = f.Unsafe.GetPointer<PlayerDatabaseComponent>(entityRef);

            if (playerStatusComponent->playerLocomotionType != PlayerLocomotionType.Stun)
                return;

            playerStatusComponent->stunStatusEffect.durationTimer.TimerTick(f.DeltaTime);

            // Stun over
            if (playerStatusComponent->stunStatusEffect.durationTimer.IsDone)
            {
                playerStatusComponent->currentToughPoint = playerDatabaseComponent->bodyDatabase.toughPoint;
                playerStatusComponent->breakStatusEffect.durationTimer.TimerReset();
                playerStatusComponent->stunStatusEffect.durationTimer.TimerReset();
                playerStatusComponent->unbeatableTimer.TimerReset();
                playerStatusComponent->breakStatusEffect.isHitWall = false;

                // return to idle
                playerStatusComponent->playerLocomotionType = PlayerLocomotionType.Idle;
            }
        }

        private PlayerPartsData_Gun GetGunDataFromEnemy(Frame f, PlayerDatabaseComponent* enemyDatabaseComponent)
        {
            QList<FP> equippingPartsIndexesList = f.ResolveList(enemyDatabaseComponent->equippingPartsIndexesList);
            FP gunIndex = equippingPartsIndexesList[1];
            RuntimePartsDataConfig runtimePartsDataConfig = f.FindAsset(f.RuntimeConfig.PartsDataInventoryConfig);

            return runtimePartsDataConfig.gunsInventory[gunIndex.AsInt];
        }
    }
}
