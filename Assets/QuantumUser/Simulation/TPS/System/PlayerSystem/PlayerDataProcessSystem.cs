using Photon.Deterministic;
using Quantum.Collections;
using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum.TPSroject
{
    [Preserve]
    public unsafe class PlayerDataProcessSystem : SystemMainThreadFilter<PlayerDataProcessSystem.Filter>, ISignalOnComponentAdded<PlayerDatabaseComponent>, ISignalOnComponentRemoved<PlayerDatabaseComponent>
    {
        public struct Filter
        {
            public EntityRef entityRef;
            public PlayerStatusComponent* playerStatusComponent;
            public PlayerDatabaseComponent* playerDatabaseComponent;
        }

        public override void Update(Frame f, ref Filter filter)
        {

        }

        public void OnAdded(Frame f, EntityRef entity, PlayerDatabaseComponent* playerDatabaseComponent)
        {
            playerDatabaseComponent->equippingPartsIndexesList = f.AllocateList<FP>();// =  list<FP> mylist = new list<FP>()

            // initialize the current parts list
            QList<FP> playerEquippingPartsDataList = f.ResolveList(playerDatabaseComponent->equippingPartsIndexesList);
            for (int i = 0; i < 2; i++)
            {
                playerEquippingPartsDataList.Add(0);
            }

            SetupTotalDatabase(f, entity);
        }

        public void OnRemoved(Frame f, EntityRef entity, PlayerDatabaseComponent* playerDatabaseComponent)
        {
            f.FreeList(playerDatabaseComponent->equippingPartsIndexesList);
            playerDatabaseComponent->equippingPartsIndexesList = default;
        }

        public void SetupTotalDatabase(Frame f, EntityRef entityRef)
        {
            PlayerDatabaseComponent* playerDatabaseComponent = f.Unsafe.GetPointer<PlayerDatabaseComponent>(entityRef);
            PlayerDatabaseComponent totalPlayerDatabaseComponent = new PlayerDatabaseComponent();

            // get parts data from equipped parts
            GetEquippingPartsIndex(f, entityRef, out PlayerPartsData_Body bodyData, out PlayerPartsData_Gun gunData);
            totalPlayerDatabaseComponent.bodyDatabase = GetBodyDataFromParts(bodyData);
            totalPlayerDatabaseComponent.gunDatabase = GetGunDataFromParts(gunData);

            playerDatabaseComponent->bodyDatabase = totalPlayerDatabaseComponent.bodyDatabase;
            playerDatabaseComponent->gunDatabase = totalPlayerDatabaseComponent.gunDatabase;
        }

        public void GetEquippingPartsIndex(Frame f, EntityRef entityRef, out PlayerPartsData_Body bodyData, out PlayerPartsData_Gun gunData)
        {
            PlayerDatabaseComponent* playerDatabaseComponent = f.Unsafe.GetPointer<PlayerDatabaseComponent>(entityRef);
            QList<FP> equippingPartsIndexesList = f.ResolveList(playerDatabaseComponent->equippingPartsIndexesList);//拿到list
            RuntimePartsDataConfig partsDataInventoryConfig = f.FindAsset(f.RuntimeConfig.PartsDataInventoryConfig);
            bodyData = default;
            gunData = default;

            for (int i = 0; i < equippingPartsIndexesList.Count; i++)
            {
                switch (i)
                {
                    case 0://body
                        {
                            FP bodyIndex = equippingPartsIndexesList[i];
                            bodyData = partsDataInventoryConfig.bodysInventory[bodyIndex.AsInt];
                            break;
                        }
                    case 1://gun
                        {
                            FP gunIndex = equippingPartsIndexesList[i];
                            gunData = partsDataInventoryConfig.gunsInventory[gunIndex.AsInt];
                            break;
                        }
                }
            }
        }

        public BodyDatabase GetBodyDataFromParts(PlayerPartsData_Body bodyData)
        {
            BodyDatabase bodyDatabase = new BodyDatabase();

            //config
            bodyDatabase.defaultConfig = bodyData.defaultConfig;
            bodyDatabase.dashConfig = bodyData.dashConfig;

            // base
            bodyDatabase.healthPoint = bodyData.healthPoint;
            bodyDatabase.toughPoint = bodyData.toughPoint;

            // move
            bodyDatabase.minMoveSpeed = bodyData.minMoveSpeed;
            bodyDatabase.maxMoveSpeed = bodyData.maxMoveSpeed;
            bodyDatabase.accelerationDuration = bodyData.accelerationDuration;
            bodyDatabase.groundTurnSpeed = bodyData.groundTurnSpeed;
            bodyDatabase.brakeMultiplier = bodyData.brakeMultiplier;

            // jump
            bodyDatabase.maxJumpHeight = bodyData.maxJumpHeight;
            bodyDatabase.gravity = bodyData.gravity;
            bodyDatabase.fallSpeedMultiplier = bodyData.fallSpeedMultiplier;
            bodyDatabase.airTurnSpeed = bodyData.airTurnSpeed;
            bodyDatabase.jumpLandStunDuration = bodyData.jumpLandStunDuration;

            // dash
            bodyDatabase.dashMoveSpeed = bodyData.dashMoveSpeed;

            // down
            bodyDatabase.toughPointRecoverAmount = bodyData.toughPointRecoverAmount;
            bodyDatabase.breakDuration = bodyData.breakDuration;

            return bodyDatabase;
        }

        public GunDatabase GetGunDataFromParts(PlayerPartsData_Gun gunData)
        {
            GunDatabase gunDatabase = new GunDatabase();

            // Gun
            gunDatabase.gunMuzzleOffset = gunData.gunMuzzleOffset;
            gunDatabase.gunConsecutiveShootIntervalDuration = gunData.gunConsecutiveShootIntervalDuration;

            // Projectile
            gunDatabase.projectileSpeed = gunData.projectileSpeed;
            gunDatabase.projectileAttackPoint = gunData.projectileAttackPoint;
            gunDatabase.projectileDownPoint = gunData.projectileDownPoint;
            gunDatabase.projectileDuration = gunData.projectileDuration;
            gunDatabase.projectileKnockBackPoint = gunData.projectileKnockBackPoint;

            return gunDatabase;
        }
    }
}
