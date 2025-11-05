using TMPro;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(TextMeshProUGUI))]
public class VersionText : MonoBehaviour
{
    private TextMeshProUGUI versionText;

    private void Awake()
    {
        versionText = GetComponent<TextMeshProUGUI>();
        versionText.SetText($"v{Application.version}");
    }
}
