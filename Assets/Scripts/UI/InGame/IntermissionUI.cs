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

public class IntermissionUI : MonoBehaviour
{
    [SerializeField] IntermissionPlayerUI[] intermissionPlayerUIs = new IntermissionPlayerUI[SessionManager.maxPlayers];
    [SerializeField] Button startButton;

    [Header("Tween")]
    [SerializeField] CanvasGroup fadeTarget;
    [SerializeField] float tweenTime = 0.5f;
    Tween hideTween;
    public void Hide()
    {
        if(hideTween == null) hideTween = DOTween.Sequence()
                .Append(fadeTarget.DOFade(0.0f, tweenTime))
                .SetAutoKill(false);
        hideTween.OnStepComplete(() => gameObject.SetActive(false));
        hideTween.PlayForward();
    }
    private void Start()
    {
        startButton.gameObject.SetActive(NetworkManager.Singleton.IsHost);
        startButton.onClick.AddListener(() => GameManager.Instance.StartRound());
        GameManager.Instance.onPlayersChange += OnPlayersChange;
        OnPlayersChange();
    }
    void OnPlayersChange()
    {
        for (int i = 0; i < SessionManager.maxPlayers; i++)
        {
            if (i < GameManager.Instance.players.Count) intermissionPlayerUIs[i].Set(GameManager.Instance.players[i]);
            else intermissionPlayerUIs[i].Set(null);
        }
    }
    private void OnDestroy()
    {
        GameManager.Instance.onPlayersChange -= OnPlayersChange;
    }
}