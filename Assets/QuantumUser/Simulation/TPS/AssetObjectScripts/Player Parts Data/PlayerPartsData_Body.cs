using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public unsafe partial class PlayerPartsData_Body : PlayerPartsData
    {
        public PlayerPartsData_Body()
        {
            // 初始化partType为Body
            this.partType = PlayerPartsType.Body;
        }

        [Header("Config")]
        public AssetRef<CharacterController3DConfig> defaultConfig;
        public AssetRef<CharacterController3DConfig> dashConfig;

        [Header("Base Status")]
        public FP healthPoint;
        public FP toughPoint;

        [Header("Base Movement")]
        public FP minMoveSpeed;
        public FP maxMoveSpeed;
        public FP accelerationDuration;
        public FP groundTurnSpeed;
        public FP brakeMultiplier;

        [Header("Base Jump")]
        public FP maxJumpHeight;
        public FP gravity;
        public FP fallSpeedMultiplier;
        public FP airTurnSpeed;
        public FP jumpLandStunDuration;

        [Header("Base Dash")]
        public FP dashMoveSpeed;

        [Header("Base Down")]
        public FP toughPointRecoverAmount;
        public FP breakDuration;
    }
}
