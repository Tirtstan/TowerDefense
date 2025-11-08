using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class GenerateCurrency : MonoBehaviour
{
    /// <summary>
    /// Event triggered when currency amount changes. Provides the current currency amount and percentage (0-1).
    /// </summary>
    public event Action<int, float> OnCurrencyAmountChanged;

    /// <summary>
    /// Event triggered when currency is claimed. Provides the amount claimed and percentage (1.0).
    /// </summary>
    public event Action<int, float> OnCurrencyClaimed;

    /// <summary>
    /// Event triggered when currency expires. Provides the percentage (0.0).
    /// </summary>
    public event Action<float> OnCurrencyExpired;

    [Header("Components")]
    [SerializeField]
    private Tower tower;

    [SerializeField]
    private TowerSelectable towerSelectable;

    [Header("Currency Settings")]
    [SerializeField]
    [Tooltip("Total amount of currency to generate")]
    private int currencyAmount = 50;

    [SerializeField]
    [Tooltip("Time in seconds to generate the currency amount")]
    private float durationDifference = 3f;

    [SerializeField]
    [Tooltip("Time in seconds currency stays at threshold before expiring")]
    private float expirationTime = 6f;

    private float currentCurrencyAmount;
    private float generationProgress;
    private bool isAtThreshold;
    private float timeAtThreshold;
    private int previousDisplayAmount;
    private float currentGenerationDuration;
    private float AttackInterval => tower != null ? tower.GetEffectiveStats().AttackInterval : 1f;

    public int CurrentCurrencyAmount => Mathf.FloorToInt(currentCurrencyAmount);
    public float CurrentPercentage => currencyAmount > 0 ? Mathf.Clamp01(currentCurrencyAmount / currencyAmount) : 0f;
    public bool HasCurrencyToClaim => currentCurrencyAmount >= currencyAmount;
    public int ClaimThreshold => currencyAmount;

    private void Awake()
    {
        if (tower == null)
            tower = GetComponent<Tower>();

        if (towerSelectable == null)
            towerSelectable = GetComponent<TowerSelectable>();

        currentCurrencyAmount = 0f;
        generationProgress = 0f;
        isAtThreshold = false;
        timeAtThreshold = 0f;
        previousDisplayAmount = 0;
        RandomizeGenerationDuration();
    }

    private void RandomizeGenerationDuration()
    {
        currentGenerationDuration = Random.Range(
            Mathf.Max(0.1f, AttackInterval - durationDifference),
            AttackInterval + durationDifference
        );
    }

    private void OnEnable()
    {
        SelectionSystem.OnSelected += HandleTowerSelected;
    }

    private void OnDisable()
    {
        SelectionSystem.OnSelected -= HandleTowerSelected;
    }

    private void Update()
    {
        if (isAtThreshold)
        {
            timeAtThreshold += Time.deltaTime;
            if (timeAtThreshold >= expirationTime)
                ExpireCurrency();
        }
        else if (currentCurrencyAmount < currencyAmount)
        {
            generationProgress += Time.deltaTime;
            float progress = Mathf.Clamp01(generationProgress / currentGenerationDuration);
            currentCurrencyAmount = progress * currencyAmount;

            int currentDisplayAmount = CurrentCurrencyAmount;
            if (currentDisplayAmount != previousDisplayAmount)
            {
                previousDisplayAmount = currentDisplayAmount;
                OnCurrencyAmountChanged?.Invoke(currentDisplayAmount, CurrentPercentage);
            }
        }
        else
        {
            // Reached threshold, start expiration timer
            currentCurrencyAmount = currencyAmount;
            isAtThreshold = true;
            timeAtThreshold = 0f;
            previousDisplayAmount = CurrentCurrencyAmount;
            OnCurrencyAmountChanged?.Invoke(CurrentCurrencyAmount, CurrentPercentage);
        }
    }

    private void HandleTowerSelected(IGameSelectable selectable)
    {
        if (selectable is MonoBehaviour mb && mb == towerSelectable && HasCurrencyToClaim)
            ClaimCurrency();
    }

    public void ClaimCurrency()
    {
        if (!HasCurrencyToClaim)
            return;

        int claimedAmount = CurrentCurrencyAmount;
        currentCurrencyAmount = 0f;
        generationProgress = 0f;
        isAtThreshold = false;
        timeAtThreshold = 0f;
        previousDisplayAmount = 0;
        RandomizeGenerationDuration();

        EconomyManager.Instance.Deposit(claimedAmount);

        OnCurrencyClaimed?.Invoke(claimedAmount, 1.0f);
        OnCurrencyAmountChanged?.Invoke(CurrentCurrencyAmount, CurrentPercentage);
    }

    private void ExpireCurrency()
    {
        currentCurrencyAmount = 0f;
        generationProgress = 0f;
        isAtThreshold = false;
        timeAtThreshold = 0f;
        previousDisplayAmount = 0;
        RandomizeGenerationDuration();

        OnCurrencyExpired?.Invoke(0f);
        OnCurrencyAmountChanged?.Invoke(CurrentCurrencyAmount, CurrentPercentage);
    }

    private void Reset()
    {
        if (tower == null)
            tower = GetComponent<Tower>();

        if (towerSelectable == null)
            towerSelectable = GetComponent<TowerSelectable>();
    }
}
