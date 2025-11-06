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
    private Vector3 target;

    private void Awake()
    {
        RectTransform rect = panel.GetComponent<RectTransform>();
        target = new Vector3(panel.Offset.x, panel.Offset.y + rect.sizeDelta.y, panel.Offset.z);
        panel.Offset = new Vector2(panel.Offset.x, target.y);

        WaveManager.OnWaveStarted += OnWaveStarted;
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
                    .Create(panel.Offset, Vector3.zero, slideInDuration)
                    .WithEase(ease)
                    .BindToFlexalonOffset(panel)
                    .AddTo(gameObject)
            )
            .Append(
                LMotion
                    .Create(0, 1f, 1f)
                    .WithOnComplete(() => waveText.SetText($"Wave {waveNumber + 1}"))
                    .RunWithoutBinding()
            )
            .AppendInterval(showDuration)
            .Append(
                LMotion
                    .Create(panel.Offset, target, slideInDuration)
                    .WithEase(ease)
                    .BindToFlexalonOffset(panel)
                    .AddTo(gameObject)
            )
            .Run();
    }

    private void OnDestroy()
    {
        WaveManager.OnWaveStarted -= OnWaveStarted;
    }
}
