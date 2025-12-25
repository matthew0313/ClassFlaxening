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

public class InGameUI : MonoBehaviour, ITabOpener
{
    public static InGameUI Instance { get; private set; }

    [SerializeField] IntermissionUI m_intermissionUI;
    public IntermissionUI intermissionUI => m_intermissionUI;

    [Header("Tabs")]
    [SerializeField] PauseTab pauseTab;

    [Header("Exiting")]
    [SerializeField] TMP_Text exitingText;
    [SerializeField] float exitingTextEditInterval = 1.0f;
    [SerializeField] CanvasGroup exitingFadeGroup;
    [SerializeField] float exitingFadeTime = 0.5f;
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        GameManager.Instance.gameState.OnValueChanged += (prev, current) =>
        {
            if (current == GameState.Intermission) intermissionUI.Show();
            else intermissionUI.Hide();
        };
    }
    private void Update()
    {
        if (currentTab != null) currentTab.OpenUpdate(this);
        else if (InputManager.Instance.UIEscapeInput()) OpenTab(pauseTab);
    }
    private void OnEnable()
    {
        SessionManager.Instance.onSessionMoveStart += OnSessionMoveStart;
        SessionManager.Instance.onSessionMoveFail += OnSessionMoveFail;
    }
    Tween exitingTween;
    CoroutineHandle exitingTextUpdate;
    private void OnSessionMoveStart()
    {
        if (exitingTween == null) exitingTween = DOTween.Sequence()
                .Append(exitingFadeGroup.DOFade(1.0f, exitingFadeTime))
                .SetAutoKill(false);
        exitingTextUpdate = Timing.RunCoroutine(ExitingTextUpdate().CancelWith(gameObject));
        exitingFadeGroup.blocksRaycasts = true;
        exitingTween.PlayForward();
    }
    private void OnSessionMoveFail()
    {
        Timing.KillCoroutines(exitingTextUpdate);
        exitingFadeGroup.blocksRaycasts = false;
        exitingTween.PlayBackwards();
    }
    IEnumerator<float> ExitingTextUpdate()
    {
        while (true)
        {
            exitingText.text = "나가는 중.";
            yield return Timing.WaitForSeconds(exitingTextEditInterval);
            exitingText.text = "나가는 중..";
            yield return Timing.WaitForSeconds(exitingTextEditInterval);
            exitingText.text = "나가는 중...";
            yield return Timing.WaitForSeconds(exitingTextEditInterval);
        }
    }
    Tab currentTab;
    public void OpenTab(Tab tab)
    {
        if (currentTab != null) currentTab.Close();
        currentTab = tab;
        if (currentTab != null) currentTab.Open(this);
    }
    public void CloseTab()
    {
        if (currentTab != null) currentTab.Close();
        currentTab = null;
    }
    private void OnDisable()
    {
        SessionManager.Instance.onSessionMoveStart -= OnSessionMoveStart;
        SessionManager.Instance.onSessionMoveFail -= OnSessionMoveFail;
    }
}