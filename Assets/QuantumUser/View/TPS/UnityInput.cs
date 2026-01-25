using Photon.Deterministic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Quantum.TPSroject
{
    public class UnityInput : MonoBehaviour
    {
        private PlayerInput playerInput;

        private void Start()
        {
            QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
        }

        public void PollInput(CallbackPollInput callback)
        {
            Quantum.Input input = new Quantum.Input();

            var localPlayerList = callback.Game.GetLocalPlayers();
            if (localPlayerList.Count == 0)
            {
                return;
            }

            var localPlayerRef = localPlayerList[callback.PlayerSlot];
            LocalPlayerManager localPlayerManager = LocalPlayerManager.Instance;
            if (localPlayerManager.LocalPlayer == null || localPlayerManager.LocalPlayer.PlayerRef != localPlayerRef)
            {
                return;
            }

            PlayerInput playerInput = localPlayerManager.PlayerInput;
            Vector2 movementInput = playerInput.actions["Movement"].ReadValue<Vector2>();
            input.Movement = movementInput.ToFPVector2();
            input.Jump = playerInput.actions["Jump"].IsPressed();
            input.Dash = playerInput.actions["Dash"].IsPressed();
            input.Gun = playerInput.actions["Gun"].IsPressed();
            input.SwitchTarget = playerInput.actions["SwitchTarget"].IsPressed();

            // get camera movement direction
            Vector3 cameraForwardProjection = new Vector3(localPlayerManager.CameraController.Camera.transform.forward.x, 0, localPlayerManager.CameraController.Camera.transform.forward.z).normalized;
            //get camera based movement direction
            Vector3 cameraMovementDirection = cameraForwardProjection * movementInput.y + localPlayerManager.CameraController.Camera.transform.right * movementInput.x;

            //when locking the character's forward direction, use the camera's coordinate system to determine the movement direction
            input.CameraMovementDirection = cameraMovementDirection.ToFPVector3();

            //expected movement direction
            //convert the movement direction in the camera coordinate system to the movement direction in the character's local coordinate system.
            Vector3 localPlayerMovementDirection = localPlayerManager.LocalPlayer.transform.InverseTransformVector(cameraMovementDirection);
            input.PlayerMovementDirection = localPlayerMovementDirection.ToFPVector3();

            callback.SetInput(input, DeterministicInputFlags.Repeatable);
        }
    }
}
