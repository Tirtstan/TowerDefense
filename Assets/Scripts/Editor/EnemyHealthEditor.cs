using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyHealth))]
public class EnemyHealthEditor : Editor
{
    private float damageAmount = 10f;
    private float healAmount = 10f;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EnemyHealth enemyHealth = (EnemyHealth)target;

        if (!Application.isPlaying)
            return;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Health Debug Tools", EditorStyles.boldLabel);

        // Health bar
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Health:", GUILayout.Width(50));
        float healthPercentage = enemyHealth.CurrentHealth / enemyHealth.MaxHealth;
        Rect healthBarRect = GUILayoutUtility.GetRect(200, 18);
        EditorGUI.ProgressBar(
            healthBarRect,
            healthPercentage,
            $"{enemyHealth.CurrentHealth:F1} / {enemyHealth.MaxHealth:F1}"
        );
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        // Damage controls
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Damage Amount:", GUILayout.Width(100));
        damageAmount = EditorGUILayout.FloatField(damageAmount, GUILayout.Width(60));

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Damage", GUILayout.Width(70)))
        {
            enemyHealth.TakeDamage(damageAmount);
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        // Heal controls
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Heal Amount:", GUILayout.Width(100));
        healAmount = EditorGUILayout.FloatField(healAmount, GUILayout.Width(60));

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Heal", GUILayout.Width(70)))
        {
            enemyHealth.TakeDamage(-healAmount);
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        // Death button
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Kill Enemy", GUILayout.Height(30)))
        {
            enemyHealth.TakeDamage(enemyHealth.CurrentHealth);
        }
        GUI.backgroundColor = Color.white;

        // Auto-repaint to update health bar
        if (Application.isPlaying)
        {
            EditorUtility.SetDirty(target);
            Repaint();
        }
    }
}
