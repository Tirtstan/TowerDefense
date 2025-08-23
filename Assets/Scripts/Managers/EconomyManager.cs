using System;
using UnityEngine;

public class EconomyManager : Singleton<EconomyManager>
{
    /// <summary>
    /// Event triggered when the currency amount is updated. Provides the <b>new</b> currency amount.
    /// </summary>
    public static event Action<int> OnCurrencyUpdated;

    /// <summary>
    /// Event triggered when the currency amount changes. Provides the <b>change</b> in currency amount.
    /// </summary>
    public static event Action<int> OnCurrencyChanged;

    [Header("Currency Settings")]
    [SerializeField]
    private int startingCurrency = 100;

    [SerializeField]
    private int maxCurrency = 9999;
    private int currencyAmount;

    private void Start() => AddCurrency(startingCurrency);

    public void AddCurrency(int amount)
    {
        currencyAmount += amount;
        currencyAmount = Mathf.Clamp(currencyAmount, 0, maxCurrency);

        OnCurrencyUpdated?.Invoke(currencyAmount);
        OnCurrencyChanged?.Invoke(amount);
    }

    public void RemoveCurrency(int amount) => AddCurrency(-amount);

    public int GetCurrencyAmount() => currencyAmount;

    public void SetCurrencyAmount(int amount)
    {
        int oldValue = currencyAmount;
        currencyAmount = Mathf.Clamp(amount, 0, maxCurrency);

        OnCurrencyUpdated?.Invoke(currencyAmount);
        OnCurrencyChanged?.Invoke(currencyAmount - oldValue);
    }

    public void ResetCurrencyToDefault()
    {
        int oldValue = currencyAmount;
        currencyAmount = startingCurrency;

        OnCurrencyUpdated?.Invoke(currencyAmount);
        OnCurrencyChanged?.Invoke(currencyAmount - oldValue);
    }

    public int GetMaxCurrency() => maxCurrency;
}
