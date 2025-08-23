using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EconomyManager))]
public class EconomyManagerEditor : Editor
{
    private EconomyManager economyManager;
    private int tempCurrencyAmount;
    private int tempAddAmount = 100;
    private int tempRemoveAmount = 50;

    private void OnEnable()
    {
        economyManager = (EconomyManager)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Runtime controls are only available during play mode.", MessageType.Info);
            return;
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Current Currency:", GUILayout.Width(120));
        EditorGUILayout.LabelField(economyManager.GetCurrencyAmount().ToString(), EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        tempAddAmount = EditorGUILayout.IntField("Add Amount:", tempAddAmount);
        if (GUILayout.Button("Add Currency", GUILayout.Width(100)))
        {
            economyManager.AddCurrency(tempAddAmount);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        tempRemoveAmount = EditorGUILayout.IntField("Remove Amount:", tempRemoveAmount);
        if (GUILayout.Button("Remove Currency", GUILayout.Width(100)))
        {
            economyManager.RemoveCurrency(tempRemoveAmount);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        tempCurrencyAmount = EditorGUILayout.IntField("Set Currency:", tempCurrencyAmount);
        if (GUILayout.Button("Set Currency", GUILayout.Width(100)))
        {
            economyManager.SetCurrencyAmount(tempCurrencyAmount);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset to Default"))
        {
            economyManager.ResetCurrencyToDefault();
        }
        if (GUILayout.Button("Set to Max"))
        {
            economyManager.SetCurrencyAmount(economyManager.GetMaxCurrency());
        }
        if (GUILayout.Button("Set to Zero"))
        {
            economyManager.SetCurrencyAmount(0);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);
        float currencyPercentage = (float)economyManager.GetCurrencyAmount() / economyManager.GetMaxCurrency();
        EditorGUILayout.LabelField("Currency Progress", EditorStyles.boldLabel);
        Rect progressRect = EditorGUILayout.GetControlRect(false, 20);
        EditorGUI.ProgressBar(
            progressRect,
            currencyPercentage,
            $"{economyManager.GetCurrencyAmount()} / {economyManager.GetMaxCurrency()}"
        );

        if (Application.isPlaying)
        {
            Repaint();
        }
    }
}
