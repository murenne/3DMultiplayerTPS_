namespace Quantum.TPSroject
{
    using Quantum;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Linq;

    public class PlayersManager : QuantumCallbacks
    {
        public static PlayersManager Instance { get; private set; }
        private Dictionary<EntityRef, PlayerViewController> _playersByEntityRefs = new Dictionary<EntityRef, PlayerViewController>();

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        IEnumerator Start()
        {
            //move
            QuantumEvent.Subscribe<EventOnPlayerIdled>(this, OnPlayerIdled);
            QuantumEvent.Subscribe<EventOnPlayerMoved>(this, OnPlayerMoved);

            //brake
            QuantumEvent.Subscribe<EventOnPlayerBraked>(this, OnPlayerBraked);

            //jump
            QuantumEvent.Subscribe<EventOnPlayerJumped>(this, OnPlayerJumped);

            //dash
            QuantumEvent.Subscribe<EventOnPlayerDashed>(this, OnPlayerDashed);

            //effect
            QuantumEvent.Subscribe<EventOnPlayerStunned>(this, OnPlayerStunned);
            QuantumEvent.Subscribe<EventOnPlayerDamageReceived>(this, OnPlayerDamageReceived);
            QuantumEvent.Subscribe<EventOnPlayerDied>(this, OnPlayerDied);

            //Break
            QuantumEvent.Subscribe<EventOnPlayerBreaked>(this, OnPlayerBreaked);

            //Attack
            QuantumEvent.Subscribe<EventOnPlayerGunAttackStarted>(this, OnPlayerGunAttackStarted);

            yield return null;
            UpdateCameraTargets();
        }

        void Update()
        {
            UpdateCameraTargets();
        }

        #region move
        public void OnPlayerIdled(EventOnPlayerIdled eventData)
        {
            if (_playersByEntityRefs.TryGetValue(eventData.PlayerEntityRef, out PlayerViewController playerViewController))
            {
                playerViewController.OnPlayerIdle(eventData);
            }
        }

        public void OnPlayerMoved(EventOnPlayerMoved eventData)
        {
            if (_playersByEntityRefs.TryGetValue(eventData.PlayerEntityRef, out PlayerViewController playerViewController))
            {
                playerViewController.OnPlayerMove(eventData);
            }
        }
        #endregion

        #region brake
        public void OnPlayerBraked(EventOnPlayerBraked eventData)
        {
            if (_playersByEntityRefs.TryGetValue(eventData.PlayerEntityRef, out PlayerViewController playerViewController))
            {
                playerViewController.OnPlayerBrake(eventData);
            }
        }
        #endregion

        #region jump
        public void OnPlayerJumped(EventOnPlayerJumped eventData)
        {
            if (_playersByEntityRefs.TryGetValue(eventData.PlayerEntityRef, out PlayerViewController playerViewController))
            {
                playerViewController.OnPlayerJump(eventData);
            }
        }
        #endregion

        #region dash 
        public void OnPlayerDashed(EventOnPlayerDashed eventData)
        {
            if (_playersByEntityRefs.TryGetValue(eventData.PlayerEntityRef, out PlayerViewController playerViewController))
            {
                playerViewController.OnPlayerForwardDash(eventData);
            }
        }
        #endregion

        #region effect
        public void OnPlayerStunned(EventOnPlayerStunned eventData)
        {
            if (_playersByEntityRefs.TryGetValue(eventData.PlayerEntityRef, out PlayerViewController playerViewController))
            {
                playerViewController.OnPlayerStun(eventData);
            }
        }

        public void OnPlayerDamageReceived(EventOnPlayerDamageReceived eventData)
        {
            if (_playersByEntityRefs.TryGetValue(eventData.PlayerEntityRef, out PlayerViewController playerViewController))
            {
                playerViewController.OnPlayerDamageReceive(eventData);
            }
        }
        public void OnPlayerDied(EventOnPlayerDied eventData)
        {
            if (_playersByEntityRefs.TryGetValue(eventData.PlayerEntityRef, out PlayerViewController playerViewController))
            {
                playerViewController.OnPlayerDie(eventData);
            }
        }
        #endregion

        #region Break
        public void OnPlayerBreaked(EventOnPlayerBreaked eventData)
        {
            if (_playersByEntityRefs.TryGetValue(eventData.PlayerEntityRef, out PlayerViewController playerViewController))
            {
                playerViewController.OnPlayerBreak(eventData);
            }
        }
        #endregion

        #region Attack
        public void OnPlayerGunAttackStarted(EventOnPlayerGunAttackStarted eventData)
        {
            if (_playersByEntityRefs.TryGetValue(eventData.PlayerEntityRef, out PlayerViewController playerViewController))
            {
                playerViewController.OnPlayerGunAttackStart(eventData);
            }
        }
        #endregion

        /// <summary>
        /// register player
        /// </summary>
        public void RegisterPlayer(QuantumGame game, PlayerViewController playerViewController)
        {
            _playersByEntityRefs.Add(playerViewController.EntityView.EntityRef, playerViewController);

            if (game.PlayerIsLocal(playerViewController.PlayerRef))
            {
                LocalPlayerManager.Instance.InitializeLocalPlayer(playerViewController);
            }

            UpdateCameraTargets();
        }

        /// <summary>
        /// deregister player
        /// </summary>
        public void DeregisterPlayer(PlayerViewController player)
        {
            _playersByEntityRefs.Remove(player.EntityView.EntityRef);

            UpdateCameraTargets();
        }

        /// <summary>
        ///  upadte camera targets
        /// </summary>
        private void UpdateCameraTargets()
        {
            var colliders = GetCapsuleColliders().ToArray();
            LocalPlayerManager.Instance.CameraController.SetTargetArray(colliders);
        }

        /// <summary>
        /// get all players' capsule colliders
        /// </summary>
        IEnumerable<CapsuleCollider> GetCapsuleColliders()
        {
            foreach (var player in _playersByEntityRefs.Values)
            {
                if (player != null && player.Collider != null)
                {
                    yield return player.Collider;
                }
            }
        }

        /// <summary>
        /// 获取玩家
        /// </summary>
        public PlayerViewController GetPlayer(EntityRef playerEntityRef)
        {
            return _playersByEntityRefs[playerEntityRef];
        }
    }
}

