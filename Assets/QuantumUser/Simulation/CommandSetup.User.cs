namespace Quantum {
  using System.Collections.Generic;
  using Photon.Deterministic;
    using Photon.Deterministic.Protocol;

    public static partial class DeterministicCommandSetup {
    static partial void AddCommandFactoriesUser(ICollection<IDeterministicCommandFactory> factories, RuntimeConfig gameConfig, SimulationConfig simulationConfig) {
            // Add or remove commands to the collection.
            // factories.Add(new NavMeshAgentTestSystem.RunTest());

            //注册command
            factories.Add(new UpdateTargetPositionCommand()); //更新敌人位置的command
        }
  }
}
