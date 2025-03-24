using UnityEngine;
using Unity.Netcode;
using TMPro;

public class ModifyValue : NetworkBehaviour, IDependantToNetworkCanvas
{
    [SerializeField] private TMP_Text m_valueTxt;
    [SerializeField] private TMP_Text m_valueReachedTxt;
    private NetworkVariable<int> value = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public int Value => value.Value;
    private NetworkVariable<int> valueReached = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public int ValueReached => valueReached.Value;
    [SerializeField] private int valueToReachMin = -100;
    [SerializeField] private int valueToReachMax = 100;
    void Awake()
    {
        value.OnValueChanged += UpdateText;
    }
    void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            ChangeRandomValueServerRpc();
        }
    }

    public void ChangeValue(int _value = 1)
    {
        value.Value += _value;
        IsValueReached(0, value.Value);
    }
    private void UpdateText(int currentValue, int newValue)
    {
        m_valueTxt.text = newValue.ToString();
    }

    public void SetRandomValue()
    {
        valueReached.Value = Random.Range(valueToReachMin, valueToReachMax);
        m_valueReachedTxt.text = valueReached.Value.ToString();
    }
    private void IsValueReached(int currentValue, int newValue)
    {
        if (newValue == valueReached.Value)
        {
            ChangeRandomValueServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void PingServerRpc()
    {
        ChangeValue();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeRandomValueServerRpc()
    {
        SetRandomValue();
    }
}
