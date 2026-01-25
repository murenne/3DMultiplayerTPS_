namespace Quantum
{
    public partial struct PlayerStatusComponent
    {
        public bool IsStunning => stunStatusEffect.durationTimer.IsRunning;
        public bool IsBreaking => breakStatusEffect.durationTimer.IsRunning;
        public bool IsDamageBuffering => damageBufferTimer.IsRunning;
        public bool IsIncapacitating => IsStunning || IsBreaking;
    }
}
