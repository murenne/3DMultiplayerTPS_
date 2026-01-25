using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial struct Input
    {
        public bool IsActionInputWasPressed(ActionType ActionType)
        {
            switch (ActionType)
            {
                case ActionType.Jump:
                    return Jump.WasPressed;
                case ActionType.Dash:
                    return Dash.WasPressed;
                case ActionType.Attack_Gun:
                    return Gun.IsDown;
                default:
                    throw new System.ArgumentException($"Unknown {nameof(ActionType)}: {ActionType}", nameof(ActionType));
            }
        }

        public FPVector2 Movement
        {
            //用DecodeDirection将MovementEncoded分解成二维方向向量
            get => DecodeDirection(MovementEncoded);
            //用EncodeDirection将传进来的一个二维方向向量编码成一个字节，并将结果存储到MovementEncoded中
            set => MovementEncoded = EncodeDirection(value);
        }

        //将输入的Vector2进行编码（转换成角度并存在一个字节中）
        private byte EncodeDirection(FPVector2 direction)
        {
            //是否为默认值
            if (direction == default)
            {
                //返回默认值：0
                return default;
            }

            //计算direction（需要编码的二维方向向量）和向上的向量的弧度，并将其转换为角度（* FP.Rad2Deg;）
            FP angle = FPVector2.RadiansSigned(FPVector2.Up, direction) * FP.Rad2Deg;
            //将角度调整为0到360度以内，然后除以 2 并加 1，使其范围在 1 到 180 之间。
            angle = (((angle + 360) % 360) / 2) + 1;
            //作为int返回
            return (byte)angle.AsInt;
        }

        //将存有角度的字节解码（转换成Vector2）
        private FPVector2 DecodeDirection(byte directionEncoded)
        {
            //是否为默认值
            if (directionEncoded == default)
            {
                //直接返回默认向量：FPVector2.default
                return default;
            }

            //得到原始角度
            int angle = (directionEncoded - 1) * 2;

            //得到解码后的方向向量并返回这个值
            return FPVector2.Rotate(FPVector2.Up, angle * FP.Deg2Rad);
        }
    }
}
