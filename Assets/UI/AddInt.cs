using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.VisualScripting;

public class AddInt : NetworkBehaviour, IDependantToNetworkCanvas
{
    private TMP_Text m_text;
    private NetworkVariable<int> value = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public int Value => value.Value;

    void Awake()
    {
        m_text = GetComponentInChildren<TMP_Text>();
        value.OnValueChanged += UpdateText;
    }
    public void AddValue()
    {
        value.Value++;
    }
    private void UpdateText(int currentValue, int newValue)
    {
        m_text.text = newValue.ToString();
    }

    [ServerRpc(RequireOwnership = false)]
    public void PingServerRpc()
    {
        AddValue();
    }
}
