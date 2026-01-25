using Quantum;
using Quantum.TPSroject;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public unsafe class LocalPlayerUIController : MonoBehaviour
{
    private const string VICTORY_TEXT = "VICTORY!";
    private const string DEFEAT_TEXT = "DEFEAT!";

    [Header("Game Start")]
    [SerializeField] private GameObject _gameStartObject;// 3-2-1
    [SerializeField] private TextMeshProUGUI _gameCountdownText;

    [Header("Game Over")]
    [SerializeField] private GameObject _gameOverObject; // Victory or Defeat
    [SerializeField] private TextMeshProUGUI _gameOverText;

    [Header("Action UI")]
    [SerializeField] private GameObject _gameActionUIGroupObject;

    private void Awake()
    {
        //Frame f = QuantumRunner.Default?.Game?.Frames.Predicted;
        //Frame f = QuantumRunner.Default?.Game?.Frames.Verified;

        QuantumEvent.Subscribe<EventOnGameInitializing>(this, OnGameInitializing);
        QuantumEvent.Subscribe<EventOnGameStarting>(this, OnGameStarting);
        QuantumEvent.Subscribe<EventOnGameRunning>(this, OnGameRunning);
        QuantumEvent.Subscribe<EventOnGameOver>(this, OnGameOver);

        ResetUI();
    }

    // initialize game 
    private void OnGameInitializing(EventOnGameInitializing eventData)
    {
        ResetUI();
    }

    // start game
    private void OnGameStarting(EventOnGameStarting eventData)
    {
        _gameActionUIGroupObject.SetActive(true);
        var actionUIController = _gameActionUIGroupObject.GetComponent<ActionUIController>();
        actionUIController.ActionUIInitialize();

        if (_gameStartObject)
        {
            // play 3-2-1 animation
            _gameStartObject.SetActive(true);
            StartCoroutine(SimpleCountdownRoutine());
        }
    }

    // game running
    private void OnGameRunning(EventOnGameRunning eventData)
    {
        if (_gameStartObject)
        {
            _gameStartObject.SetActive(false);
        }
    }

    // game over
    private void OnGameOver(EventOnGameOver eventData)
    {
        Frame f = eventData.Game.Frames.Verified;

        var localPlayer = LocalPlayerManager.Instance.LocalPlayer;
        if (localPlayer == null)
        {
            return;
        }

        _gameOverObject.SetActive(true);

        PlayerTeam myTeam = localPlayer.PlayerTeam;
        var teamACount = f.Global->teamPlayerCount[0];
        var teamBCount = f.Global->teamPlayerCount[1];

        if (teamACount == 0 && teamBCount == 0)
        {
            _gameOverText.text = DEFEAT_TEXT;
        }
        else if (teamACount == 0)
        {
            _gameOverText.text = myTeam == PlayerTeam.A ? DEFEAT_TEXT : VICTORY_TEXT;
        }
        else if (teamBCount == 0)
        {
            _gameOverText.text = myTeam == PlayerTeam.B ? DEFEAT_TEXT : VICTORY_TEXT;
        }
    }

    private void ResetUI()
    {
        if (_gameStartObject)
        {
            _gameStartObject.SetActive(false);
        }

        if (_gameOverObject)
        {
            _gameOverObject.SetActive(false);
        }

        if (_gameActionUIGroupObject)
        {
            _gameActionUIGroupObject.SetActive(true);
        }
    }

    private IEnumerator SimpleCountdownRoutine()
    {
        _gameCountdownText.text = "3";
        yield return new WaitForSeconds(1.0f);

        _gameCountdownText.text = "2";
        yield return new WaitForSeconds(1.0f);

        _gameCountdownText.text = "1";
        yield return new WaitForSeconds(1.0f);
    }
}
