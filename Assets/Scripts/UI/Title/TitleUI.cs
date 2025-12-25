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

public class TitleUI : MonoBehaviour, ITabOpener
{
    [Header("Joining")]
    [SerializeField] TMP_Text joiningText;
    [SerializeField] float joiningTextEditInterval = 1.0f;
    [SerializeField] CanvasGroup joiningFadeGroup;
    [SerializeField] float joiningFadeTime = 0.5f;
    private void OnEnable()
    {
        NetworkManager.Singleton.OnConnectionEvent += OnConnectionEvent;
        SessionManager.Instance.onSessionMoveStart += OnSessionMoveStart;
        SessionManager.Instance.onSessionMoveFail += OnSessionMoveFail;
    }

    private void OnDisable()
    {
        NetworkManager.Singleton.OnConnectionEvent -= OnConnectionEvent;
        SessionManager.Instance.onSessionMoveStart -= OnSessionMoveStart;
        SessionManager.Instance.onSessionMoveFail -= OnSessionMoveFail;
    }
    private void OnConnectionEvent(NetworkManager manager, ConnectionEventData data)
    {
        if(data.EventType == ConnectionEvent.ClientConnected && manager.IsHost)
        {
            manager.SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }
    }
    Tab currentTab;
    public void OpenTab(Tab tab)
    {
        if (currentTab != null) currentTab.Close();
        currentTab = tab;
        if(currentTab != null) currentTab.Open(this);
    }
    private void Update()
    {
        if(currentTab != null) currentTab.OpenUpdate(this);
    }
    public void CloseTab()
    {
        if (currentTab != null) currentTab.Close();
        currentTab = null;
    }
    Tween joiningTween;
    CoroutineHandle joiningTextUpdate;
    private void OnSessionMoveStart()
    {
        if (joiningTween == null) joiningTween = DOTween.Sequence()
                .Append(joiningFadeGroup.DOFade(1.0f, joiningFadeTime))
                .SetAutoKill(false);
        joiningTextUpdate = Timing.RunCoroutine(JoiningTextUpdate().CancelWith(gameObject));
        joiningFadeGroup.blocksRaycasts = true;
        joiningTween.PlayForward();
    }
    private void OnSessionMoveFail()
    {
        Timing.KillCoroutines(joiningTextUpdate);
        joiningFadeGroup.blocksRaycasts = false;
        joiningTween.PlayBackwards();
    }
    IEnumerator<float> JoiningTextUpdate()
    {
        while (true)
        {
            joiningText.text = "게임 참가 중.";
            yield return Timing.WaitForSeconds(joiningTextEditInterval);
            joiningText.text = "게임 참가 중..";
            yield return Timing.WaitForSeconds(joiningTextEditInterval);
            joiningText.text = "게임 참가 중...";
            yield return Timing.WaitForSeconds(joiningTextEditInterval);
        }
    }
}