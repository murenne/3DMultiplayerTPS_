using Quantum;
using UnityEngine;
using UnityEngine.UI;

public class ActionUI : MonoBehaviour
{
    private static readonly int READY_HASH = Animator.StringToHash("Ready");
    private static readonly int ACTIVATE_HASH = Animator.StringToHash("Activate");
    private static readonly int FAILED_HASH = Animator.StringToHash("Activation Failed");
    private static readonly int EMPTY_STATE_HASH = Animator.StringToHash("Empty");

    [SerializeField] private Image _revealingMask;
    [SerializeField] private Image _hidingMask;
    [SerializeField] private Animator _animator;
    private bool _isReady = true;

    public void UpdateActionUI(Frame f, in PlayerActionInfo playerAction, bool wasButtonPressed)
    {
        float normalizedCooldown = playerAction.cooldownTimer.NormalizedTime.AsFloat;
        _revealingMask.fillAmount = normalizedCooldown;
        _hidingMask.fillAmount = 1 - normalizedCooldown;

        bool wasReady = _isReady;
        _isReady = _revealingMask.fillAmount >= 0.99f;

        if (_isReady)
        {
            if (!wasReady)
            {
                _animator.SetTrigger(READY_HASH);
            }
        }
        else
        {
            if (wasReady)
            {
                _animator.SetTrigger(ACTIVATE_HASH);
            }
            else if (wasButtonPressed)
            {
                AnimatorStateInfo animatorState = _animator.GetCurrentAnimatorStateInfo(0);
                if (animatorState.tagHash == EMPTY_STATE_HASH)
                {
                    if (playerAction.cooldownTimer.TimeLeft > (playerAction.inputBufferTimer.TimeLeft - f.DeltaTime))
                    {
                        _animator.SetTrigger(FAILED_HASH);
                    }
                }
            }
        }
    }
}
