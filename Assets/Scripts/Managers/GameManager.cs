using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using MEC;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] Player playerPrefab;

    public readonly NetworkVariable<GameState> gameState = new(GameState.Intermission);
    public readonly List<Player> players = new();
    public event Action onPlayersChange;
    private void Awake()
    {
        Instance = this;
        if (NetworkManager.IsHost)
        {
            Player player = Instantiate(playerPrefab);
            player.NetworkObject.SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId, true);
        }
    }
    private void OnEnable()
    {
        NetworkManager.Singleton.OnConnectionEvent += OnConnectionEvent;
    }
    private void OnDisable()
    {
        NetworkManager.Singleton.OnConnectionEvent -= OnConnectionEvent;
    }
    public Player GetPlayer(ulong clientID) => players.Find(player => player.OwnerClientId == clientID);
    public void AddPlayer(Player player)
    {
        players.Add(player);
        players.Sort((a, b) => a.OwnerClientId.CompareTo(b.OwnerClientId));
        onPlayersChange?.Invoke();
    }
    public void RemovePlayer(Player player)
    {
        players.Remove(player);
        onPlayersChange?.Invoke();
    }
    private void OnConnectionEvent(NetworkManager manager, ConnectionEventData data)
    {
        if(IsHost && data.EventType == ConnectionEvent.PeerConnected)
        {
            Player player = Instantiate(playerPrefab);
            player.NetworkObject.SpawnAsPlayerObject(data.ClientId, true);
        }
    }
    public event Action onRoundStart;
    public void StartRound()
    {
        if (!IsHost || gameState.Value != GameState.Intermission) return;
        SessionManager.Instance.activeSession.AsHost().IsLocked = true;
        gameState.Value = GameState.RoundInProgress;
    }
}
[System.Serializable]
public enum GameState
{
    Intermission,
    RoundInProgress
}