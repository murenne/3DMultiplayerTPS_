using Photon.Deterministic;
using UnityEngine;
namespace Quantum
{
    public unsafe partial class PlayerActionData_Jump : PlayerActionData
    {
        public PlayerActionData_Jump()
        {
            actionType = ActionType.Jump;
        }

        // update action
        public override void OnActionStart(Frame f, EntityRef entityRef)
        {
            PlayerActionComponent* playerActionComponent = f.Unsafe.GetPointer<PlayerActionComponent>(entityRef);
            playerActionComponent->actionsParameter.jumpAction.isJumping = true;
        }

        public override void OnActionUpdate(Frame f, EntityRef entityRef)
        {

        }

        public override void OnActionEnd(Frame f, EntityRef entityRef)
        {
            PlayerActionComponent* playerActionComponent = f.Unsafe.GetPointer<PlayerActionComponent>(entityRef);
            playerActionComponent->actionsParameter.jumpAction.isJumping = false;
        }
    }
}


