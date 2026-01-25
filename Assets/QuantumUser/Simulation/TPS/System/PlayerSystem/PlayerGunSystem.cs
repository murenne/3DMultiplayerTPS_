using Photon.Deterministic;
using Quantum.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
namespace Quantum.TPSroject
{
    [Preserve]
    public unsafe class PlayerGunSystem : SystemMainThreadFilter<PlayerGunSystem.Filter>
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
            PlayerActionComponent* playerActionComponent = filter.playerActionComponent;

            playerActionComponent->actionsParameter.gunAction.consecutiveShootIntervalTimer.TimerTick(f.DeltaTime);

            bool shouldShoot = false;

            var gunActionInfo = filter.playerActionComponent->actionInfoArray[(int)ActionType.Attack_Gun];
            var isAIEnemy = f.Unsafe.TryGetPointer(filter.entityRef, out EnemyFlagComponent* enemyFlagComponent);
            if (gunActionInfo.IsActivating || isAIEnemy)
            {
                shouldShoot = true;
            }

            if (shouldShoot)
            {
                if (filter.playerActionComponent->actionsParameter.gunAction.consecutiveShootIntervalTimer.IsDone)
                {
                    CharacterGunShoot(f, entityRef);
                }
            }
        }

        private void CharacterGunShoot(Frame f, EntityRef entityRef)
        {
            Transform3D* transform = f.Unsafe.GetPointer<Transform3D>(entityRef);
            PlayerDatabaseComponent* playerDatabaseComponent = f.Unsafe.GetPointer<PlayerDatabaseComponent>(entityRef);
            PlayerTargetComponent* playerTargetComponent = f.Unsafe.GetPointer<PlayerTargetComponent>(entityRef);
            PlayerActionComponent* playerActionComponent = f.Unsafe.GetPointer<PlayerActionComponent>(entityRef);

            QList<FP> equippingPartsIndexesList = f.ResolveList(playerDatabaseComponent->equippingPartsIndexesList);
            FP gunIndex = equippingPartsIndexesList[1];
            RuntimePartsDataConfig runtimePartsDataConfig = f.FindAsset(f.RuntimeConfig.PartsDataInventoryConfig);
            var gunData = runtimePartsDataConfig.gunsInventory[gunIndex.AsInt];

            // calculate projectile spawn position and direction
            FPVector3 muzzleWorldPos = transform->TransformPoint(gunData.gunMuzzleOffset);
            FPVector3 shootDirection;
            bool hasValidTarget = playerTargetComponent->currentTarget != EntityRef.None && f.Exists(playerTargetComponent->currentTarget);
            if (hasValidTarget)
            {
                Transform3D* targetTransform = f.Unsafe.GetPointer<Transform3D>(playerTargetComponent->currentTarget);
                FPVector3 targetPosition = targetTransform->Position;

                FPVector3 targetCenterPos = targetPosition + FPVector3.Up * FP._1_50;// TODO 这里好像没有成功

                shootDirection = (targetCenterPos - muzzleWorldPos).Normalized;
            }
            else
            {
                shootDirection = transform->Forward;
            }

            playerActionComponent->actionsParameter.gunAction.projectileSpawnPosition = muzzleWorldPos;
            playerActionComponent->actionsParameter.gunAction.projectileDirection = shootDirection;

            f.Signals.OnSpawnProjectileSignal(entityRef, gunData);
            f.Events.OnPlayerGunAttackStarted(entityRef);
            playerActionComponent->actionsParameter.gunAction.consecutiveShootIntervalTimer.TimerSetup(playerDatabaseComponent->gunDatabase.gunConsecutiveShootIntervalDuration);
        }
    }
}
