using Photon.Deterministic;
using System;
using UnityEngine;

namespace Quantum
{
    [Serializable]
    public unsafe class PlayerActionData_Dash : PlayerActionData
    {
        public PlayerActionData_Dash()
        {
            actionType = ActionType.Dash;
        }

        public FPAnimationCurve dashMovementCurve;

        public override void OnActionStart(Frame f, EntityRef entityRef)
        {
            PlayerActionComponent* playerActionComponent = f.Unsafe.GetPointer<PlayerActionComponent>(entityRef);
            playerActionComponent->actionsParameter.dashAction.isDashing = true;
        }

        public override void OnActionUpdate(Frame f, EntityRef entityRef)
        {

        }

        public override void OnActionEnd(Frame f, EntityRef entityRef)
        {
            PlayerActionComponent* playerActionComponent = f.Unsafe.GetPointer<PlayerActionComponent>(entityRef);
            playerActionComponent->actionsParameter.dashAction.isDashing = false;
        }
    }
}

