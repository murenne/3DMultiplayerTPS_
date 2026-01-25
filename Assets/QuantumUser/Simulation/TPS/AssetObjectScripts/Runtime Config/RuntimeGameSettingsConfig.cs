using Photon.Deterministic;
using UnityEngine;

namespace Quantum.TPSroject
{
    public partial class RuntimeGameSettingsConfig : AssetObject
    {
        public FP InitializationDuration = 3;
        public FP GameStartDuration = 3;
        public FP GameDuration = 300;
        public FP GoalDuration = 3;
        public FP GameOverDuration = 3;

        public FP PlayerRespawnHeight = -20;
        public FP BallRespawnHeight = -20;

        public LayerMask PlayerLayerMask;

        [Header("KCC")]
        public CharacterController3DConfig characterController3DConfig_1;
        public CharacterController3DConfig characterController3DConfig_2;
        public CharacterController3DConfig characterController3DConfig_3;
        public CharacterController3DConfig characterController3DConfig_4;

        public Collections.QDictionary<EntityRef, CharacterController3D> collections = new Collections.QDictionary<EntityRef, CharacterController3D>();
    }










    public struct Collection
    {
        public int Count;
        public PlayerRef playerRef;
    }
}
