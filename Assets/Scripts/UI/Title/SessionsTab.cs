using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SessionsTab : Tab
{
    [SerializeField] SessionsListUI sessionList;
    [SerializeField] TMP_InputField playerNameField, joinIDField;
    [SerializeField] Button refreshButton, createSessionButton, joinWithIDButton;

    [Header("Tab Tween")]
    [SerializeField] CanvasGroup backgroundGroup;
    [SerializeField] CanvasGroup listGroupLeft, listGroupRight;
    [SerializeField] float appearTime = 0.2f;
    bool initialized = false;
    void Initialize()
    {
        initialized = true;
        refreshButton.onClick.AddListener(sessionList.Refresh);
        createSessionButton.onClick.AddListener(CreateSession);
        joinWithIDButton.onClick.AddListener(JoinWithID);
    }
    private void OnEnable()
    {
        if (!initialized) Initialize();
    }
    Tween tween;
    public override void Open(ITabOpener opener)
    {
        base.Open(opener);
        if (tween == null) tween = DOTween.Sequence()
                .Append(backgroundGroup.DOFade(1.0f, appearTime))
                .Append(listGroupLeft.DOFade(1.0f, appearTime))
                .Append(listGroupRight.DOFade(1.0f, appearTime))
                .SetAutoKill(false);
        sessionList.Refresh();
        tween.OnStepComplete(null);
        tween.PlayForward();
    }
    public override void OpenUpdate(ITabOpener opener)
    {
        base.OpenUpdate(opener);
        if (InputManager.Instance.UIEscapeInput() && !SessionManager.Instance.sessionMoving) opener.CloseTab();
    }
    public override void Close()
    {
        tween.OnStepComplete(() => base.Close());
        tween.PlayBackwards();
    }
    void CreateSession() => SessionManager.Instance.StartSession();
    void JoinWithID() => SessionManager.Instance.JoinSessionById(joinIDField.text);
}