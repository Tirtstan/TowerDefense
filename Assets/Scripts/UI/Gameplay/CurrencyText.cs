using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class CurrencyText : MonoBehaviour
{
    private TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        EconomyManager.OnCurrencyUpdated += UpdateCurrencyText;
    }

    public void UpdateCurrencyText(int amount)
    {
        text.SetText(amount.ToString());
    }

    private void OnDisable()
    {
        EconomyManager.OnCurrencyUpdated -= UpdateCurrencyText;
    }
}
