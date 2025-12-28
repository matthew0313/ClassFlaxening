using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
public class Player : NetworkBehaviour
{
    public static Player localPlayer { get; private set; }

    public readonly NetworkVariable<FixedString512Bytes> playerName = new(new FixedString128Bytes("NoName"), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public readonly NetworkVariable<int> profileIndex = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public readonly NetworkVariable<bool> answered = new(false);
    public readonly NetworkVariable<float> score = new(-1.0f);
    private void Start()
    {
        GameManager.Instance.AddPlayer(this);
        if (IsOwner)
        {
            localPlayer = this;
            SessionManager.Instance.onPlayerNameChange += OnPlayerNameChange;
            OnPlayerNameChange(SessionManager.Instance.playerName);
        }
    }

    private void OnPlayerNameChange(string playerName)
    {
        this.playerName.Value = playerName;
    }

    public override void OnDestroy()
    {
        GameManager.Instance.RemovePlayer(this);
        if (IsOwner)
        {
            SessionManager.Instance.onPlayerNameChange -= OnPlayerNameChange;
        }
        base.OnDestroy();
    }
}