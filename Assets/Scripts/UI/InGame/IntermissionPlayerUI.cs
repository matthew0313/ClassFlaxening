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

public class IntermissionPlayerUI : MonoBehaviour
{
    [SerializeField] ProfileImageList profileImageList;
    [SerializeField] Image profileImage;
    [SerializeField] TMP_Text playerNameText;
    [SerializeField] Button profileNextButton, profilePrevButton;
    private void Awake()
    {
        profileNextButton.onClick.AddListener(() => Player.localPlayer.profileIndex.Value = (Player.localPlayer.profileIndex.Value + 1) % profileImageList.profileImages.Count);
        profilePrevButton.onClick.AddListener(() => Player.localPlayer.profileIndex.Value = ((Player.localPlayer.profileIndex.Value - 1 < 0) ? (profileImageList.profileImages.Count - 1) : (Player.localPlayer.profileIndex.Value - 1)));
    }
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
            player.playerName.OnValueChanged += OnPlayerNameChange;
            OnPlayerNameChange(new(), player.playerName.Value);
            profileNextButton.gameObject.SetActive(player.IsOwner);
            profilePrevButton.gameObject.SetActive(player.IsOwner);
        }
        else
        {
            profileImage.gameObject.SetActive(false);
            playerNameText.gameObject.SetActive(false);
            profileNextButton.gameObject.SetActive(false);
            profilePrevButton.gameObject.SetActive(false);
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
    void OnPlayerNameChange(FixedString512Bytes prev, FixedString512Bytes current)
    {
        playerNameText.text = current.ToString();
    }
}