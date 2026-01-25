using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public unsafe partial class PlayerActionData : AssetObject
    {
        public FP inputBufferDuration = FP._0_10;
        public FP activateDuration = FP._0_25;
        public FP cooldownDuration = FP._5;

        [Header("UI関連")]
        [SerializeField] private GameObject _actionUIPrefab;
        public bool HasActionUIPrefab => _actionUIPrefab != null;
        public GameObject ActionUIPrefab => _actionUIPrefab;
        public ActionAvailabilityCondition actionAvailabilityCondition;

        [Header("type")]
        public ActionType actionType;


        public virtual void OnActionStart(Frame f, EntityRef entityRef) { }
        public virtual void OnActionUpdate(Frame f, EntityRef entityRef) { }
        public virtual void OnActionEnd(Frame f, EntityRef entityRef) { }
    }
}