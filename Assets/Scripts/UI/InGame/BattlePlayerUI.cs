using DG.Tweening;
using MEC;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattlePlayerUI : MonoBehaviour
{
    [SerializeField] ProfileImageList profileImageList;
    [SerializeField] Image profileImage;
    [SerializeField] Image profileBackground;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] Color idleColor, answeredColor, positiveColor, negativeColor;
    Player player;
    public void Set(Player player)
    {
        if (this.player != null) this.player.profileIndex.OnValueChanged -= OnProfileIndexChange;
        this.player = player;

        if(player != null)
        {
            player.profileIndex.OnValueChanged += OnProfileIndexChange;
            OnProfileIndexChange(-1, player.profileIndex.Value);
            player.score.OnValueChanged += OnScoreChange;
            player.answered.OnValueChanged += OnAnsweredChange;
        }
        else
        {
            profileImage.gameObject.SetActive(false);
            profileBackground.color = idleColor;
        }
    }
    void OnProfileIndexChange(int prev, int current)
    {
        if (current == -1) profileImage.gameObject.SetActive(false);
        else
        {
            profileImage.gameObject.SetActive(true);
            profileImage.sprite = profileImageList.profileImages[current];
        }
    }
    void OnScoreChange(float prev, float current)
    {
        scoreText.gameObject.SetActive(current != -1.0f);
        scoreText.text = (current - GameManager.Instance.scoreThreshold).ToString("0.0");
        scoreText.color = current >= GameManager.Instance.scoreThreshold ? positiveColor : negativeColor;
        profileBackground.color = current == -1.0f ? idleColor : (current >= GameManager.Instance.scoreThreshold ? positiveColor : negativeColor);
    }
    void OnAnsweredChange(bool prev, bool current)
    {
        if(current) profileBackground.color = answeredColor;
    }
}