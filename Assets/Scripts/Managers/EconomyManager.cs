using System;
using QFSW.QC;
using UnityEngine;

[CommandPrefix("economy.")]
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

    [Header("Debug")]
    [SerializeField]
    private bool infiniteMoney = false;
    private int currencyAmount;

    private void Start() => Deposit(startingCurrency);

    [Command("deposit", "Deposits the specified amount of currency.")]
    public void Deposit(int amount)
    {
        currencyAmount += amount;
        currencyAmount = Mathf.Clamp(currencyAmount, 0, maxCurrency);

        OnCurrencyUpdated?.Invoke(currencyAmount);
        OnCurrencyChanged?.Invoke(amount);
    }

    [Command("spend", "Spends the specified amount of currency.")]
    public void Spend(int amount)
    {
        if (infiniteMoney)
            return;

        Deposit(-amount);
    }

    [Command("get_currency", "Gets the current currency amount.")]
    public int GetCurrencyAmount() => currencyAmount;

    [Command("set_currency", "Sets the currency amount to the specified value.")]
    public void SetCurrencyAmount(int amount)
    {
        int oldValue = currencyAmount;
        currencyAmount = Mathf.Clamp(amount, 0, maxCurrency);

        OnCurrencyUpdated?.Invoke(currencyAmount);
        OnCurrencyChanged?.Invoke(currencyAmount - oldValue);
    }

    public bool CanAfford(int amount) => infiniteMoney || currencyAmount >= amount;

    [Command("reset_currency", "Resets the currency amount to the starting value.")]
    public void ResetCurrencyToDefault()
    {
        int oldValue = currencyAmount;
        currencyAmount = startingCurrency;

        OnCurrencyUpdated?.Invoke(currencyAmount);
        OnCurrencyChanged?.Invoke(currencyAmount - oldValue);
    }

    public int GetMaxCurrency() => maxCurrency;

    [Command("set_infinite_money", "Sets infinite money mode.")]
    public void SetInfiniteMoney(bool enabled)
    {
        infiniteMoney = enabled;
        Debug.Log($"Infinite money: {(infiniteMoney ? "Enabled" : "Disabled")}");
    }
}
