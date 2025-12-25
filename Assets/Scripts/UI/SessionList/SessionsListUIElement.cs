using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

public class SessionsListUIElement : MonoBehaviour
{
    [SerializeField] TMP_Text sessionID, sessionInfo;
    [SerializeField] Button joinButton;

    [Header("Tween")]
    [SerializeField] RectTransform scaleTarget;
    [SerializeField] float scaleTime = 0.3f;
    [SerializeField] Ease scaleEase = Ease.OutCirc;
    [SerializeField] CanvasGroup fadeTarget;
    [SerializeField] float fadeTime = 0.2f;
    Tween appearTween;
    public void Show()
    {
        if (appearTween == null) appearTween = DOTween.Sequence()
                .Append(scaleTarget.DOScale(Vector3.one, scaleTime).SetEase(scaleEase))
                .Join(fadeTarget.DOFade(1.0f, fadeTime)).SetAutoKill(false);
        joinButton.onClick.AddListener(Join);
        gameObject.SetActive(true);
        appearTween.OnStepComplete(null);
        appearTween.PlayForward();
    }
    public void Hide()
    {
        joinButton.onClick.RemoveListener(Join);
        appearTween.OnStepComplete(() => gameObject.SetActive(false));
        appearTween.PlayBackwards();
    }
    ISessionInfo info;
    public void Set(ISessionInfo info)
    {
        this.info = info;
        sessionID.text = $"ID: {info.Id}";
        sessionInfo.text = $"플레이어 수: {info.MaxPlayers - info.AvailableSlots}/{info.MaxPlayers}";
    }
    void Join()
    {
        if (info == null) return;
        SessionManager.Instance.JoinSessionById(info.Id);
    }
}