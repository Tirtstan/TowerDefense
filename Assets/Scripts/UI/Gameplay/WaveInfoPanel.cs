using System;
using Flexalon;
using LitMotion;
using LitMotion.Extensions;
using LitMotion.Extensions.Flexalon;
using TMPro;
using UnityEngine;

public class WaveInfoPanel : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private FlexalonObject panel;

    [SerializeField]
    private TextMeshProUGUI waveText;

    [Header("Animation")]
    [SerializeField]
    [Range(0.1f, 10f)]
    private float showDuration = 5f;

    [SerializeField]
    [Range(0.1f, 2f)]
    private float slideInDuration = 0.5f;

    [SerializeField]
    private Ease ease;
    private float target;

    private void Awake()
    {
        WaveManager.OnWaveStarted += OnWaveStarted;
    }

    private void Start()
    {
        RectTransform rect = panel.GetComponent<RectTransform>();
        target = panel.Offset.y + rect.sizeDelta.y * 2;
        panel.Offset = new Vector2(panel.Offset.x, target);
        waveText.SetText("Wave 0");
    }

    private void OnWaveStarted(Wave wave)
    {
        ShowWaveAnnouncement(wave.WaveNumber);
    }

    private void ShowWaveAnnouncement(int waveNumber)
    {
        if (waveNumber <= 0)
            waveText.SetText("Wave 0");

        LSequence
            .Create()
            .Append(
                LMotion
                    .Create(panel.Offset.y, 0f, slideInDuration)
                    .WithEase(ease)
                    .BindToFlexalonOffsetY(panel)
                    .AddTo(gameObject)
            )
            .Append(
                LMotion
                    .Create(0, 1f, 2f)
                    .WithOnComplete(() => waveText.SetText($"Wave {waveNumber + 1}"))
                    .RunWithoutBinding()
            )
            .AppendInterval(showDuration)
            .Append(
                LMotion
                    .Create(panel.Offset.y, target, slideInDuration)
                    .WithEase(ease)
                    .BindToFlexalonOffsetY(panel)
                    .AddTo(gameObject)
            )
            .Run();
    }

    private void OnDestroy()
    {
        WaveManager.OnWaveStarted -= OnWaveStarted;
    }
}
