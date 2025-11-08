using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HorizontalSpinner : Selectable, IMoveHandler
{
    [System.Serializable]
    public class SpinnerEvent : UnityEvent<int> { }

    [Header("Spinner")]
    [SerializeField]
    [Tooltip("Values to choose from (display strings).")]
    private string[] values;

    [Header("Configs")]
    [SerializeField]
    [Tooltip("Wrap around when reaching ends.")]
    private bool wrap = true;

    [SerializeField]
    [Tooltip("Disable left/right buttons at the ends when not wrapping.")]
    private bool disableButtonsAtEnds;

    [SerializeField]
    [Tooltip("Allow move input (left/right) to change value.")]
    private bool useMoveInput = true;

    [SerializeField]
    private Button leftButton;

    [SerializeField]
    private Button rightButton;

    [SerializeField]
    private TextMeshProUGUI displayText;

    [Space]
    public SpinnerEvent onValueChanged;
    private int currentIndex = 0;

    protected override void Awake()
    {
        base.Awake();

        if (leftButton != null)
            leftButton.onClick.AddListener(Prev);
        if (rightButton != null)
            rightButton.onClick.AddListener(Next);

        UpdateUI();
    }

    public void Prev()
    {
        if (values == null || values.Length == 0)
            return;

        int prevIndex = currentIndex - 1;
        if (prevIndex < 0)
        {
            if (wrap)
                prevIndex = values.Length - 1;
            else
                prevIndex = 0;
        }

        SetIndex(prevIndex);
    }

    public void Next()
    {
        if (values == null || values.Length == 0)
            return;

        int nextIndex = currentIndex + 1;
        if (nextIndex >= values.Length)
        {
            if (wrap)
                nextIndex = 0;
            else
                nextIndex = values.Length - 1;
        }

        SetIndex(nextIndex);
    }

    public void SetIndex(int index)
    {
        index = Mathf.Clamp(index, 0, Mathf.Max(0, values.Length - 1));
        if (index == currentIndex)
        {
            UpdateUI();
            return;
        }

        currentIndex = index;
        UpdateUI();

        onValueChanged?.Invoke(currentIndex);
    }

    private void UpdateUI()
    {
        if (!Application.isPlaying)
            return;

        string currentValue =
            (values != null && values.Length > 0 && currentIndex >= 0 && currentIndex < values.Length)
                ? values[currentIndex]
                : "";

        displayText.SetText(currentValue);

        if (disableButtonsAtEnds && !wrap && values != null && values.Length > 0)
        {
            leftButton.interactable = currentIndex > 0;
            rightButton.interactable = currentIndex < values.Length - 1;
        }
    }

    public void AddOptions(List<string> newValues)
    {
        values = newValues.ToArray();
        currentIndex = 0;
        UpdateUI();
    }

    public void ClearOptions()
    {
        values = new string[0];
        currentIndex = 0;
        UpdateUI();
    }

    public string GetValue() => GetValueAt(currentIndex);

    public string GetValueAt(int index) =>
        (values != null && values.Length > 0 && index >= 0 && index < values.Length) ? values[index] : "";

    public override void OnMove(AxisEventData eventData)
    {
        if (!IsInteractable() || !IsActive())
            return;

        if (!useMoveInput)
        {
            base.OnMove(eventData);
            return;
        }

        if (eventData.moveDir == MoveDirection.Left)
            Prev();
        else if (eventData.moveDir == MoveDirection.Right)
            Next();

        base.OnMove(eventData);
    }
}
