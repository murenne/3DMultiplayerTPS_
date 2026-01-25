using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum.TPSroject
{
    [Preserve]
    public unsafe class ProjectileSystem : SystemMainThreadFilter<ProjectileSystem.Filter>, ISignalOnSpawnProjectileSignal, ISignalOnCollisionProjectileHitCharacterSignal, ISignalOnCollisionProjectileHitStaticObjectSignal
    {
        public struct Filter
        {
            public EntityRef Entity;
            public ProjectileComponent* ProjectileComponent;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            ProjectileComponent* projectileComponent = filter.ProjectileComponent;

            projectileComponent->projectileDurationTimer.TimerTick(f.DeltaTime);
            if (projectileComponent->projectileDurationTimer.IsDone)
            {
                f.Destroy(filter.Entity);
            }
        }

        public void OnSpawnProjectileSignal(Frame f, EntityRef ownerEntityRef, PlayerPartsData_Gun gunData)
        {
            // creat projectile entity
            EntityRef projectileEntity = f.Create(gunData.projectilePrototype);

            // get components
            Transform3D* projectileTransform = f.Unsafe.GetPointer<Transform3D>(projectileEntity);
            Transform3D* ownerTransform = f.Unsafe.GetPointer<Transform3D>(ownerEntityRef);
            PlayerDatabaseComponent* playerDatabaseComponent = f.Unsafe.GetPointer<PlayerDatabaseComponent>(ownerEntityRef);
            PlayerActionComponent* playerActionComponent = f.Unsafe.GetPointer<PlayerActionComponent>(ownerEntityRef);
            ProjectileComponent* projectileComponent = f.Unsafe.GetPointer<ProjectileComponent>(projectileEntity);
            PhysicsBody3D* projectileBody = f.Unsafe.GetPointer<PhysicsBody3D>(projectileEntity);

            // setup projectile transform and data
            projectileComponent->Owner = ownerEntityRef;
            projectileComponent->gunData = gunData;
            projectileTransform->Rotation = ownerTransform->Rotation;
            projectileTransform->Position = playerActionComponent->actionsParameter.gunAction.projectileSpawnPosition;

            // setup projectile velocity
            var direction = playerActionComponent->actionsParameter.gunAction.projectileDirection.Normalized;
            projectileBody->Velocity = direction * playerDatabaseComponent->gunDatabase.projectileSpeed;

            // setup projectile duration
            projectileComponent->projectileDurationTimer.TimerSetup(playerDatabaseComponent->gunDatabase.projectileDuration);
        }

        public void OnCollisionProjectileHitCharacterSignal(Frame f, CollisionInfo3D info, ProjectileComponent* projectileComponent, PlayerStatusComponent* playerStatusComponent)
        {
            if (projectileComponent->Owner == info.Other)
            {
                info.IgnoreCollision = true;
                return;
            }

            f.Destroy(info.Entity);
        }

        public void OnCollisionProjectileHitStaticObjectSignal(Frame f, CollisionInfo3D info, ProjectileComponent* projectileComponent)
        {
            f.Destroy(info.Entity);
        }
    }
}
