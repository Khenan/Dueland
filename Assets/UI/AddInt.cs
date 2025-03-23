using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.VisualScripting;

public class AddInt : NetworkBehaviour, IDependantToNetworkCanvas
{
    private TMP_Text m_text;
    private NetworkVariable<int> value = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public int Value => value.Value;

    void Awake()
    {
        m_text = GetComponentInChildren<TMP_Text>();
    }
    public void AddValue()
    {
        value.Value++;
        UpdateText();
    }
    private void UpdateText()
    {
        m_text.text = value.Value.ToString();
    }

    [ServerRpc(RequireOwnership = false)]
    public void PingServerRpc()
    {
        PingClientRpc();
    }

    [ClientRpc]
    private void PingClientRpc()
    {
        AddValue();
    }
}
