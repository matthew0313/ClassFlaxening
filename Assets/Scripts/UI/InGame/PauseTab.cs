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

public class PauseTab : Tab
{
    [SerializeField] Button exitButton;

    [Header("Tween")]
    [SerializeField] RectTransform moveTarget;
    [SerializeField] CanvasGroup fadeTarget;
    [SerializeField] float tweenTime = 0.5f;
    bool initialized = false;
    void Initialize()
    {
        initialized = true;
        exitButton.onClick.AddListener(() => SessionManager.Instance.LeaveSession());
    }
    private void OnEnable()
    {
        if(!initialized) Initialize();
    }
    Tween tween;
    public override void Open(ITabOpener opener)
    {
        base.Open(opener);
        if (tween == null) tween = DOTween.Sequence()
            .Append(moveTarget.DOAnchorMin(new Vector2(0, 0), tweenTime))
            .Join(moveTarget.DOAnchorMax(new Vector2(1, 1), tweenTime))
            .Join(fadeTarget.DOFade(1, tweenTime))
            .SetAutoKill(false);
        tween.OnStepComplete(null);
        tween.PlayForward();
    }
    public override void OpenUpdate(ITabOpener opener)
    {
        base.OpenUpdate(opener);
        if (InputManager.Instance.UIEscapeInput()) opener.CloseTab();
    }
    public override void Close()
    {
        tween.OnStepComplete(() => base.Close());
        tween.PlayBackwards();
    }
}