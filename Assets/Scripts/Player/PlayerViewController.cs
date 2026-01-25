using Quantum;
using Quantum.TPSroject;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public unsafe class PlayerViewController : QuantumCallbacks
{
    [Header("Components")]
    [SerializeField] private Animator _animator;
    [SerializeField] private CapsuleCollider capsuleCollider;
    public CapsuleCollider Collider => capsuleCollider;

    [Header("Quantum")]
    public Frame frame;
    public PlayerRef PlayerRef { get; private set; }
    public PlayerTeam PlayerTeam { get; private set; }
    public string PlayerName { get; private set; }
    [SerializeField] private QuantumEntityView _entityView;//set in inspector
    public QuantumEntityView EntityView => _entityView;

    [Header("UI")]
    [SerializeField] private Canvas _playerViewCanvas;
    [SerializeField] private TextMeshProUGUI _playerNameText;
    [SerializeField] private TextMeshProUGUI _playerHPText;
    [SerializeField] private Slider _playerHPBar;

    // Start is called before the first frame update
    void Start()
    {
        Frame frame = QuantumRunner.Default?.Game?.Frames.Predicted;
        if (frame != null)
        {
            RuntimePlayer runtimePlayerData = frame.GetPlayerData(PlayerRef);
            PlayerName = runtimePlayerData.PlayerNickname;
            _playerNameText.text = PlayerName;

            PlayerDatabaseComponent* playerDatabaseComponent = frame.Unsafe.GetPointer<PlayerDatabaseComponent>(_entityView.EntityRef);
            _playerHPText.text = playerDatabaseComponent->bodyDatabase.healthPoint.ToString();
        }
    }

    /// <summary>
    /// when the entity is instantiated
    /// </summary>
    public void OnEntityInstantiated(QuantumGame game)
    {
        frame = game.Frames.Predicted;

        PlayerStatusComponent* playerStatusComponent = frame.Unsafe.GetPointer<PlayerStatusComponent>(_entityView.EntityRef);
        PlayerRef = playerStatusComponent->playerRef;
        PlayerTeam = playerStatusComponent->PlayerTeam;
        PlayersManager.Instance.RegisterPlayer(game, this);
    }

    /// <summary>
    /// when the entity is destroyed
    /// </summary>
    public void OnEntityDestroyed(QuantumGame game)
    {
        PlayersManager.Instance.DeregisterPlayer(this);
    }

    // Update is called once per frame
    public override void OnUpdateView(QuantumGame game)
    {
        Frame frame = game.Frames.Predicted;
        if (!frame.Exists(_entityView.EntityRef))
        {
            return;
        }
    }

    private void LateUpdate()
    {
        _playerViewCanvas.transform.forward = Camera.main.transform.forward;
    }

    #region move
    public void OnPlayerIdle(EventOnPlayerIdled eventData)
    {
        _animator.SetBool("Move", false);
        Debug.Log("Play Idle Animation, Audio, FX ect. By Event");
    }

    public void OnPlayerMove(EventOnPlayerMoved eventData)
    {
        _animator.SetBool("Move", true);
        Debug.Log("Play Walk Animation, Audio, FX ect. By Event");
    }
    #endregion

    #region brake
    public void OnPlayerBrake(EventOnPlayerBraked eventData)
    {
        Debug.Log("brake animation");
    }
    #endregion


    #region jump
    public void OnPlayerJump(EventOnPlayerJumped eventData)
    {
        Debug.Log("jump animation");
    }
    #endregion


    #region dash
    public void OnPlayerForwardDash(EventOnPlayerDashed eventData)
    {
        Debug.Log("Dash animation");
    }
    #endregion

    #region effect
    public void OnPlayerStun(EventOnPlayerStunned eventData)
    {
        Debug.Log("stun animation");
    }

    public void OnPlayerDamageReceive(EventOnPlayerDamageReceived eventData)
    {
        Debug.Log("hp bar update");
        _playerHPText.text = eventData.currentHealthPoint.ToString();
        _playerHPBar.value = (float)(eventData.currentHealthPoint / eventData.maxHealthPoint);
    }

    public void OnPlayerDie(EventOnPlayerDied eventData)
    {
        Debug.Log("player died animation");
    }
    #endregion

    #region break
    public void OnPlayerBreak(EventOnPlayerBreaked eventData)
    {
        Debug.Log("player break animation");
    }
    #endregion

    #region Attack 
    public void OnPlayerGunAttackStart(EventOnPlayerGunAttackStarted eventData)
    {
        //_animator.SetTrigger("Attack");
        Debug.Log("player attack animation");
    }
    #endregion
}
