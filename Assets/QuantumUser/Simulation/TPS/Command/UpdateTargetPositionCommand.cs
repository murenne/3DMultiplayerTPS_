using Photon.Deterministic;
using Quantum;

//该脚本定义了一个command
//该脚本必须放在QuantumUser文件夹下


public class UpdateTargetPositionCommand : DeterministicCommand
{
    public string targetName;
    public int test;

    public override void Serialize(Photon.Deterministic. BitStream stream)
    {
        // 向 bitstream 写入数据
        stream.Serialize(ref targetName);
        stream.Serialize(ref test);
    }

    //需要执行的内容
    public void Execute(Frame f)
    {

    }

}
