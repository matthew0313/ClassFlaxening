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

[CreateAssetMenu(fileName = "ProfileImageList", menuName = "Scriptables/ProfileImageList", order = 0)]
public class ProfileImageList : ScriptableObject
{
    [SerializeField] List<Sprite> m_profileImages = new();
    public List<Sprite> profileImages => m_profileImages;
}