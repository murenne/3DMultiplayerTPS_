using Photon.Deterministic;
using Quantum.Collections;
using Quantum.TPSroject;
using UnityEngine;

namespace Quantum
{
    public unsafe class PlayerActionData_Gun : PlayerActionData
    {
        public PlayerActionData_Gun()
        {
            actionType = ActionType.Attack_Gun;
        }

        public override void OnActionStart(Frame f, EntityRef entityRef)
        {

        }

        public override void OnActionUpdate(Frame f, EntityRef entityRef)
        {

        }

        public override void OnActionEnd(Frame f, EntityRef entityRef)
        {

        }
    }
}
