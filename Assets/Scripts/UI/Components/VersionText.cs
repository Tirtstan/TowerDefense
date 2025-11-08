using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class VersionText : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField]
    private string versionPrefix = "v";
    private TextMeshProUGUI versionText;

    private void Awake()
    {
        versionText = GetComponent<TextMeshProUGUI>();
        versionText.SetText($"{versionPrefix}{Application.version}");
    }

    private void OnValidate()
    {
        if (versionText == null)
            versionText = GetComponent<TextMeshProUGUI>();
        versionText.SetText($"{versionPrefix}{Application.version}");
    }
}
