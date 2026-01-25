using UnityEngine.Scripting;

namespace Quantum.TPSroject
{
    [Preserve]
    public unsafe class CollisionsSystem : SystemSignalsOnly, ISignalOnCollisionEnter3D
    {
        public void OnCollisionEnter3D(Frame f, CollisionInfo3D info)
        {
            // info is the collision information
            // info.entity is the information of the entity itself (the initiator of the collision, or the entity to which the current script is attached)
            // info.Other is the information of the other party

            ProjectileCollision(f, info);
            CharacterCollision(f, info);
        }

        /// <summary>
        /// Projectile hits something
        /// </summary>
        private void ProjectileCollision(Frame f, CollisionInfo3D info)
        {
            if (f.Unsafe.TryGetPointer<ProjectileComponent>(info.Entity, out var projectileComponent))
            {
                // hit a character
                if (f.Unsafe.TryGetPointer<PlayerStatusComponent>(info.Other, out var playerStatusComponent))
                {
                    f.Signals.OnCollisionProjectileHitCharacterSignal(info, projectileComponent, playerStatusComponent);
                }
                // hit a projectile
                else if (f.Unsafe.TryGetPointer<ProjectileComponent>(info.Other, out var enemyProjectileComponent))
                {
                    info.IgnoreCollision = true;
                }
                // hit a static object
                else if (info.Other == Quantum.EntityRef.None && info.IsStatic)
                {
                    f.Signals.OnCollisionProjectileHitStaticObjectSignal(info, projectileComponent);
                }
            }
        }

        /// <summary>
        /// Character hits something
        /// </summary>
        private void CharacterCollision(Frame f, CollisionInfo3D info)
        {
            if (f.Unsafe.TryGetPointer<PlayerStatusComponent>(info.Entity, out var playerStatusComponent))
            {
                // Hits a static object in the scene other than the ground
                if (info.IsStatic && info.StaticData.Layer != 3)
                {
                    if (playerStatusComponent->IsBreaking && !playerStatusComponent->breakStatusEffect.isHitWall)
                    {
                        f.Signals.OnCollisionCharacterHitNormalStaticObjectSignal(info, playerStatusComponent);
                    }
                }
            }
        }
    }
}
