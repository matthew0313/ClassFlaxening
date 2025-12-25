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

public class IntermissionPlayerUI : MonoBehaviour
{
    [SerializeField] List<GameObject> profileImageList = new();
    [SerializeField] TMP_Text playerNameText;
    [SerializeField] Button profileNextButton, profilePrevButton;
    private void Awake()
    {
        profileNextButton.onClick.AddListener(() => Player.localPlayer.profileIndex.Value = (Player.localPlayer.profileIndex.Value + 1) % profileImageList.Count);
        profilePrevButton.onClick.AddListener(() => Player.localPlayer.profileIndex.Value = (Player.localPlayer.profileIndex.Value - 1) % profileImageList.Count);
    }
    int currentProfile = -1;
    Player player;
    public void Set(Player player)
    {
        if (this.player != null) this.player.profileIndex.OnValueChanged -= OnProfileIndexChange;
        this.player = player;

        if(player != null)
        {
            player.profileIndex.OnValueChanged += OnProfileIndexChange;
            OnProfileIndexChange(-1, player.profileIndex.Value);
            playerNameText.gameObject.SetActive(true);
            playerNameText.text = player.playerName.Value.ToString();
            profileNextButton.gameObject.SetActive(player.IsOwner);
            profilePrevButton.gameObject.SetActive(player.IsOwner);
        }
        else
        {
            if (currentProfile != -1) profileImageList[currentProfile].SetActive(false);
            currentProfile = -1;
            playerNameText.gameObject.SetActive(false);
            profileNextButton.gameObject.SetActive(false);
            profilePrevButton.gameObject.SetActive(false);
        }
    }
    void OnProfileIndexChange(int prev, int current)
    {
        if (currentProfile != -1) profileImageList[currentProfile].SetActive(false);
        profileImageList[current].SetActive(true);
        currentProfile = current;
    }
}