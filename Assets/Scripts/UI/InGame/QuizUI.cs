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
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuizUI : MonoBehaviour
{
    [SerializeField] TMP_Text quizContent;
    [SerializeField] TMP_InputField answerField;

    [Header("Tween")]
    [SerializeField] RectTransform moveTarget;
    [SerializeField] CanvasGroup fadeTarget;
    [SerializeField] float tweenTime = 0.5f;
    Tween tween;

    bool initialized = false;
    void Initialize()
    {
        initialized = true;
        answerField.onSubmit.AddListener((answer) =>
        {
            GameManager.Instance.SubmitAnswerRpc(answer);
            EventSystem.current.SetSelectedGameObject(null);
            Hide();
        });
    }
    private void OnEnable()
    {
        if (!initialized) Initialize();
    }
    public void Show()
    {
        if (tween == null) tween = DOTween.Sequence()
                .Append(moveTarget.DOAnchorMin(new Vector2(0, 0), tweenTime))
                .Join(moveTarget.DOAnchorMax(new Vector2(1, 1), tweenTime))
                .Join(fadeTarget.DOFade(1.0f, tweenTime))
                .SetAutoKill(false);
        gameObject.SetActive(true);
        tween.OnStepComplete(null);
        tween.PlayForward();

        quizContent.text = "Generating...";
        answerField.gameObject.SetActive(false);
    }
    public void Set(string quiz)
    {
        quizContent.text = quiz;
        answerField.text = string.Empty;
        answerField.gameObject.SetActive(true);
    }
    public void Hide()
    {
        tween.OnStepComplete(() => gameObject.SetActive(false));
        tween.PlayBackwards();
    }
}