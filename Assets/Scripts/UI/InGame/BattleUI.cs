using DG.Tweening;
using MEC;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PrimeTween;
public class BattleUI : MonoBehaviour
{
    [SerializeField] BattlePlayerUI[] battlePlayerUIs = new BattlePlayerUI[SessionManager.maxPlayers];
    [SerializeField] TMP_Text enemyNameText;
    [SerializeField] AnimatedBarUI enemyHpBar, playerTeamHpBar;
    [SerializeField] CanvasGroup victory, defeat;
    [SerializeField] float endFadeTime = 0.5f;

    [SerializeField] PooledEffect attackEffect;
    Transform enemyEffectAnchor => GameManager.Instance.currentEnemy.effectAnchor;

    private void Start()
    {
        GameManager.Instance.enemyIndex.OnValueChanged += (prev, current) =>
        {
            if (current == -1) return;
            enemyNameText.text = GameManager.Instance.currentEnemy.enemyName;
            OnEnemyHpChange(-1.0f, GameManager.Instance.enemyHp.Value);
        };
        GameManager.Instance.onPlayersChange += OnPlayersChange;
        OnPlayersChange();
        GameManager.Instance.enemyHp.OnValueChanged += OnEnemyHpChange;
        OnEnemyHpChange(-1.0f, GameManager.Instance.enemyHp.Value);
        GameManager.Instance.playerTeamHp.OnValueChanged += OnPlayerTeamHpChange;
        OnPlayerTeamHpChange(-1.0f, GameManager.Instance.playerTeamHp.Value);
        GameManager.Instance.onVictory += () => { victory.gameObject.SetActive(true); victory.DOFade(1.0f, endFadeTime); };
        GameManager.Instance.onDefeat += () => { defeat.gameObject.SetActive(true); defeat.DOFade(1.0f, endFadeTime); };
        GameManager.Instance.onEnemyAttack += (damage) =>
        {
            attackEffect.Get(enemyEffectAnchor);
        };
    }
    void OnPlayersChange()
    {
        for (int i = 0; i < SessionManager.maxPlayers; i++)
        {
            if (i < GameManager.Instance.players.Count) battlePlayerUIs[i].Set(GameManager.Instance.players[i]);
            else battlePlayerUIs[i].Set(null);
        }
    }
    void OnEnemyHpChange(float prev, float current) => enemyHpBar.SetScale(current / GameManager.Instance.enemyMaxHp);
    void OnPlayerTeamHpChange(float prev, float current) => playerTeamHpBar.SetScale(current / GameManager.Instance.playerTeamMaxHp);
}