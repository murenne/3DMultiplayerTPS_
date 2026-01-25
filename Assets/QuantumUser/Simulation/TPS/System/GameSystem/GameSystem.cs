using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum.TPSroject
{
    [Preserve]
    public unsafe class GameSystem : SystemMainThread
    {
        public override void Update(Frame f)
        {
            RuntimeGameSettingsConfig runtimeGameSettingsConfig = f.FindAsset<RuntimeGameSettingsConfig>(f.RuntimeConfig.GameSettingsConfig.Id);

            switch (f.Global->gameState)
            {
                case GameState.None:
                    UpdateNone(f, runtimeGameSettingsConfig);
                    break;

                case GameState.Initializing:
                    UpdateInitializing(f, runtimeGameSettingsConfig);
                    break;

                case GameState.Starting:
                    UpdateStarting(f, runtimeGameSettingsConfig);
                    break;

                case GameState.Running:
                    UpdateRunning(f, runtimeGameSettingsConfig);
                    break;

                case GameState.GameOver:
                    UpdateGameOver(f, runtimeGameSettingsConfig);
                    break;
            }
        }

        private void UpdateNone(Frame f, RuntimeGameSettingsConfig config)
        {
            f.Global->gameState = GameState.Initializing;
            f.Global->gameStateTimer.TimerSetup(config.InitializationDuration);

            f.Events.OnGameInitializing();
        }

        private void UpdateInitializing(Frame f, RuntimeGameSettingsConfig config)
        {
            // set team player count
            f.Global->teamPlayerCount[0] = 0;
            f.Global->teamPlayerCount[1] = 0;

            var filter = f.Filter<PlayerStatusComponent>();
            while (filter.Next(out EntityRef entity, out PlayerStatusComponent playerStatusComponent))
            {
                if (playerStatusComponent.currentHealthPoint > 0)
                {
                    var teamIndex = (int)playerStatusComponent.PlayerTeam;

                    if (teamIndex >= 0 && teamIndex < f.Global->teamPlayerCount.Length)
                    {
                        f.Global->teamPlayerCount[teamIndex] += 1;
                    }
                }
            }

            f.Global->gameStateTimer.TimerTick(f.DeltaTime);
            if (f.Global->gameStateTimer.IsDone)
            {
                f.Global->gameState = GameState.Starting;
                f.Global->gameStateTimer.TimerSetup(config.GameStartDuration);

                f.Events.OnGameStarting();
            }
        }

        private void UpdateStarting(Frame f, RuntimeGameSettingsConfig config)
        {
            f.Global->gameStateTimer.TimerTick(f.DeltaTime);

            if (f.Global->gameStateTimer.IsDone)
            {
                f.Global->gameState = GameState.Running;

                f.Events.OnGameRunning();
            }
        }

        private void UpdateRunning(Frame f, RuntimeGameSettingsConfig config)
        {
            var teamACount = f.Global->teamPlayerCount[0];
            var teamBCount = f.Global->teamPlayerCount[1];

            if (teamACount <= 0 || teamBCount <= 0)
            {
                f.Global->gameState = GameState.GameOver;
                f.Global->gameStateTimer.TimerSetup(config.GameOverDuration);

                f.Events.OnGameOver();
            }
        }

        private void UpdateGameOver(Frame f, RuntimeGameSettingsConfig config)
        {
            f.Global->gameStateTimer.TimerTick(f.DeltaTime);

            if (f.Global->gameStateTimer.IsDone)
            {
                // choice 1 : restart the game
                //f.Global->gameState = GameState.Initializing; // 或者 Starting
                //f.Global->gameStateTimer.TimerSetup(config.InitializationDuration);
                // RespawnPlayers(f);

                // choice 2 : completely end, do nothing and wait for exit
            }
        }

        // reset game state
        //private void RespawnPlayers(Frame f)
        //{
        //    foreach (var (playerEntityRef, _) in f.Unsafe.GetComponentBlockIterator<PlayerStatus>())
        //    {
        //        f.Signals.OnPlayerRespawned(playerEntityRef, true);
        //    }
        //}
    }
}
