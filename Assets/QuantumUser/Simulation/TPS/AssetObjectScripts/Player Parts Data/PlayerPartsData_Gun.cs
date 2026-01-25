using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public unsafe partial class PlayerPartsData_Gun : PlayerPartsData
    {
        public PlayerPartsData_Gun()
        {
            // 初始化partType为Gun
            this.partType = PlayerPartsType.Gun;
        }

        public AssetRef<EntityPrototype> projectilePrototype;//子弹Prototype
        public FPVector3 gunMuzzleOffset;//枪口位置（世界坐标）
        public FP projectileAttackPoint;//攻击力
        public FP projectileDownPoint;//Down值
        public FP projectileKnockBackPoint;//被击退的值
        public FP projectileSpeed;//子弹速度
        public FP projectileDuration;//子弹在屏幕中存在的时间
        public FP gunConsecutiveShootIntervalDuration;//连续射击的间隔时间
        public FP downMovenmentDistanceXZ;
        public FP downMovenmentDistanceY;
    }
}
