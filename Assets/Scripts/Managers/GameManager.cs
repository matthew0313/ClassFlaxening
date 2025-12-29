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
using Unity.Collections;
using System.Linq;
using DG.Tweening;




#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] Player playerPrefab;

    [SerializeField] float m_playerTeamMaxHp = 100.0f;
    [SerializeField] float m_maxScore = 10.0f, m_scoreThreshold = 5.0f;
    [SerializeField] float damageMultiplier = 3.0f;
    [SerializeField] List<Enemy> m_enemyList;
    public float playerTeamMaxHp => m_playerTeamMaxHp;
    public float maxScore => m_maxScore;
    public float scoreThreshold => m_scoreThreshold;
    public List<Enemy> enemyList => m_enemyList;
    public Enemy currentEnemy => enemyIndex.Value == -1 ? null : enemyList[enemyIndex.Value];

    [SerializeField] float quizEndTime = 1.0f;
    [SerializeField] float iterateInterval = 0.25f, gameEndTime = 5.0f;
    [SerializeField] AudioSource intermissionMusic, battleMusic;

    public readonly NetworkVariable<GameState> gameState = new(GameState.Intermission);
    public readonly NetworkVariable<int> enemyIndex = new(-1);
    public readonly NetworkVariable<float> enemyHp = new(100.0f);
    public float enemyMaxHp => enemyIndex.Value == -1 ? 100.0f : enemyList[enemyIndex.Value].maxHp;
    public readonly NetworkVariable<float> playerTeamHp = new(100.0f);
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
    private void Start()
    {
        enemyIndex.OnValueChanged += (prev, current) =>
        {
            if (current == -1) return;
            currentEnemy.gameObject.SetActive(true);
            intermissionMusic.DOFade(0.0f, 0.5f);
            battleMusic.Play();
        };
    }
    public event Action onNetworkSpawn;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (currentEnemy != null) currentEnemy.gameObject.SetActive(true);
        onNetworkSpawn?.Invoke();
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
    public void StartRound()
    {
        if (!IsHost || gameState.Value != GameState.Intermission) return;
        SessionManager.Instance.activeSession.AsHost().IsLocked = true;
        SessionManager.Instance.activeSession.AsHost().RefreshAsync();
        gameState.Value = GameState.RoundInProgress;
        enemyIndex.Value = UnityEngine.Random.Range(0, enemyList.Count);
        Timing.RunCoroutine(StartWait().CancelWith(gameObject));
    }
    IEnumerator<float> StartWait()
    {
        yield return Timing.WaitForSeconds(3.0f);
        MakeNewQuiz();
    }
    readonly List<Player> quizReceivers = new();
    void MakeNewQuiz()
    {
        quizReceivers.Clear();
        playerAnswers.Clear();
        foreach(var player in players)
        {
            player.answered.Value = false;
            player.score.Value = -1;
        }
        StartGetQuizRpc();
        AIRequestManager.Instance.GetEnglishQuiz(quiz =>
        {
            foreach (var i in players)
            {
                quizReceivers.Add(i);
                GetQuizRpc(quiz, RpcTarget.Single(i.OwnerClientId, RpcTargetUse.Temp));
            }
        });
    }
    string currentQuiz;
    public event Action onQuizGetStart;
    public event Action<string> onQuizGet;
    readonly Dictionary<Player, string> playerAnswers = new();
    [Rpc(SendTo.Everyone)]
    public void StartGetQuizRpc() => onQuizGetStart?.Invoke();
    [Rpc(SendTo.SpecifiedInParams)]
    public void GetQuizRpc(FixedString4096Bytes quiz, RpcParams rpcParams = new())
    {
        currentQuiz = quiz.ToString();
        onQuizGet?.Invoke(currentQuiz);
    }
    [Rpc(SendTo.Server)]
    public void SubmitAnswerRpc(FixedString512Bytes answer, RpcParams rpcParams = new())
    {
        Player answerer = GetPlayer(rpcParams.Receive.SenderClientId);
        if(playerAnswers.ContainsKey(answerer)) return;
        playerAnswers.Add(answerer, answer.ToString());
        answerer.answered.Value = true;
        if(playerAnswers.Count >= quizReceivers.Count)
        {
            List<string> answers = new();
            foreach(var player in quizReceivers)
            {
                answers.Add(playerAnswers[player]);
            }
            AIRequestManager.Instance.GetQuizScore(currentQuiz, answers, maxScore, (results) =>
            {
                for(int i = 0; i < results.Count; i++)
                {
                    players[i].score.Value = results[i];
                }
                Timing.RunCoroutine(IterateResults(results).CancelWith(gameObject));
            });
        }
    }
    IEnumerator<float> IterateResults(List<float> results)
    {
        yield return Timing.WaitForSeconds(1.0f);
        foreach(var result in results)
        {
            var val = (result - scoreThreshold) * damageMultiplier;
            if (val > 0)
            {
                enemyHp.Value = Mathf.Max(0, enemyHp.Value - val);
                AttackEnemyRpc(val);
            }
            else
            {
                val *= -1;
                playerTeamHp.Value = Mathf.Max(0, playerTeamHp.Value - val);
                AttackTeamRpc(val);
            }
            yield return Timing.WaitForSeconds(iterateInterval);
        }
        yield return Timing.WaitForSeconds(quizEndTime);
        if(playerTeamHp.Value <= 0.0f) DefeatRpc();
        else if (enemyHp.Value <= 0.0f) VictoryRpc();
        else MakeNewQuiz();
    }
    public event Action<float> onEnemyAttack, onTeamAttack;
    public event Action onDefeat, onVictory;
    [Rpc(SendTo.Everyone)] 
    public void AttackEnemyRpc(float damage)
    {
        currentEnemy.Damage();
        onEnemyAttack?.Invoke(damage);
    }
    [Rpc(SendTo.Everyone)] 
    public void AttackTeamRpc(float damage)
    {
        currentEnemy.Attack();
        onTeamAttack?.Invoke(damage);
    }
    [Rpc(SendTo.Everyone)]
    public void DefeatRpc()
    {
        onDefeat?.Invoke();
        OnGameEnd();
        if(IsHost) Timing.RunCoroutine(GameEnding().CancelWith(gameObject));
    }
    [Rpc(SendTo.Everyone)]
    public void VictoryRpc()
    {
        currentEnemy.Die();
        onVictory?.Invoke();
        OnGameEnd();
        if (IsHost) Timing.RunCoroutine(GameEnding().CancelWith(gameObject));
    }
    void OnGameEnd()
    {
        battleMusic.DOFade(0.0f, 0.5f);
    }
    IEnumerator<float> GameEnding()
    {
        if(!IsHost) yield break;
        yield return Timing.WaitForSeconds(gameEndTime);
        SessionManager.Instance.activeSession.AsHost().DeleteAsync();
    }
}
[System.Serializable]
public enum GameState
{
    Intermission,
    RoundInProgress
}