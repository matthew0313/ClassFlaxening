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
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SessionsTab : MonoBehaviour, ITab
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
    public void Open(ITabOpener opener)
    {
        if (tween == null) tween = DOTween.Sequence()
                .Append(backgroundGroup.DOFade(1.0f, appearTime))
                .Append(listGroupLeft.DOFade(1.0f, appearTime))
                .Append(listGroupRight.DOFade(1.0f, appearTime))
                .SetAutoKill(false);
        gameObject.SetActive(true);
        tween.OnStepComplete(null);
        tween.PlayForward();
    }
    public void OpenUpdate(ITabOpener opener)
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !SessionManager.Instance.sessionMoving) opener.CloseTab();
    }
    public void Close()
    {
        tween.OnStepComplete(() => gameObject.SetActive(false));
        gameObject.SetActive(false);
    }
    void CreateSession() => SessionManager.Instance.StartSession();
    void JoinWithID() => SessionManager.Instance.JoinSessionById(joinIDField.text);
}