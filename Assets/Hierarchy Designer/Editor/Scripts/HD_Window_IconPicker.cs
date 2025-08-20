#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HierarchyDesigner
{
    internal sealed class HD_Window_IconPicker : EditorWindow
    {
        #region Properties
        private const float TileSize = 25f;
        private const float TilePadding = 5f;

        private GameObject targetGO;
        private string targetGlobalId;
        private string search = string.Empty;

        private Vector2 scroll;
        private List<Texture2D> builtinIcons = new();
        private List<(Texture2D tex, string guid)> assetIcons = new();
        private List<(Texture2D tex, string label)> componentIcons = new();
        #endregion

        #region Initialization
        public static void Open(GameObject go)
        {
            if (go == null) return;
            HD_Window_IconPicker window = CreateInstance<HD_Window_IconPicker>();
            window.titleContent = new GUIContent("Main Icon Picker");
            window.targetGO = go;
            window.targetGlobalId = GlobalObjectId.GetGlobalObjectIdSlow(go).ToString();
            window.position = new Rect(GUIUtility.GUIToScreenPoint(Event.current.mousePosition), new Vector2(560, 480));
            window.minSize = new Vector2(420, 360);
            window.ShowAuxWindow();
            window.Focus();
        }

        private void OnEnable()
        {
            GatherBuiltinIcons();
            GatherComponentIcons();
            GatherAssetIcons();
        }
        #endregion

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUI.SetNextControlName("search");
                search = GUILayout.TextField(search, GUI.skin.FindStyle("ToolbarSeachTextField") ?? EditorStyles.toolbarTextField, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(60))) search = string.Empty;
                using (new EditorGUI.DisabledScope(targetGO == null))
                {
                    if (GUILayout.Button(HD_Settings_IconOverrides.Has(targetGlobalId) ? "Clear Override" : "No Override", EditorStyles.toolbarButton, GUILayout.Width(110)))
                    {
                        if (HD_Settings_IconOverrides.Clear(targetGlobalId)) Close();
                    }
                }
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawSection("Common Component Icons", DrawComponentGrid);
            GUILayout.Space(6);
            DrawSection("Built-in / Editor Icons", DrawBuiltinGrid);
            GUILayout.Space(6);
            DrawSection("Project Textures", DrawAssetGrid);
            EditorGUILayout.EndScrollView();
        }

        #region Methods
        private void DrawSection(string label, Action drawer)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            GUILayout.Space(2);
            drawer?.Invoke();
        }

        private void DrawBuiltinGrid()
        {
            List<Texture2D> list = Filter(builtinIcons, t => t.name);
            DrawIconGrid(list, t =>
            {
                HD_Settings_IconOverrides.SetBuiltin(targetGlobalId, t);
                Close();
            }, true);
        }

        private void DrawAssetGrid()
        {
            List<(Texture2D tex, string guid)> list = Filter(assetIcons, x => x.tex != null ? x.tex.name : string.Empty);
            DrawIconGrid(list.Select(x => x.tex).ToList(), t =>
            {
                string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(t));
                HD_Settings_IconOverrides.SetAsset(targetGlobalId, guid);
                Close();
            }, true);
        }

        private void DrawComponentGrid()
        {
            List<(Texture2D tex, string label)> list = Filter(componentIcons, x => (x.label ?? string.Empty) + " " + (x.tex ? x.tex.name : string.Empty));
            if (list == null || list.Count == 0)
            {
                EditorGUILayout.HelpBox("No component icons found for current filter.", MessageType.Info);
                return;
            }

            int perRow = Mathf.Max(1, Mathf.FloorToInt((position.width - 20) / (TileSize + TilePadding)));
            int i = 0;
            while (i < list.Count)
            {
                EditorGUILayout.BeginHorizontal();
                for (int c = 0; c < perRow && i < list.Count; c++, i++)
                {
                    (Texture2D tex, string label) = list[i];
                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(TileSize + TilePadding)))
                    {
                        Rect r = GUILayoutUtility.GetRect(TileSize, TileSize, GUILayout.ExpandWidth(false));
                        if (tex != null) GUI.DrawTexture(r, tex, ScaleMode.ScaleToFit, true);
                        if (GUI.Button(r, GUIContent.none, GUIStyle.none))
                        {
                            HD_Settings_IconOverrides.SetBuiltin(targetGlobalId, tex);
                            Close();
                        }
                        GUILayout.Label(!string.IsNullOrEmpty(label) ? label : (tex ? tex.name : "(null)"),
                            EditorStyles.miniLabel, GUILayout.Width(TileSize + TilePadding));
                    }
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2);
            }
        }

        private void DrawIconGrid(IList<Texture2D> icons, Action<Texture2D> onPick, bool showName)
        {
            if (icons == null || icons.Count == 0)
            {
                EditorGUILayout.HelpBox("No icons found for current filter.", MessageType.Info);
                return;
            }

            int perRow = Mathf.Max(1, Mathf.FloorToInt((position.width - 20) / (TileSize + TilePadding)));
            int i = 0;
            while (i < icons.Count)
            {
                EditorGUILayout.BeginHorizontal();
                for (int c = 0; c < perRow && i < icons.Count; c++, i++)
                {
                    Texture2D tex = icons[i];
                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(TileSize + TilePadding)))
                    {
                        Rect r = GUILayoutUtility.GetRect(TileSize, TileSize, GUILayout.ExpandWidth(false));
                        if (tex != null) GUI.DrawTexture(r, tex, ScaleMode.ScaleToFit, true);
                        if (GUI.Button(r, GUIContent.none, GUIStyle.none)) onPick?.Invoke(tex);
                        if (showName)
                        {
                            GUILayout.Label(tex != null ? tex.name : "(null)", EditorStyles.miniLabel, GUILayout.Width(TileSize + TilePadding));
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2);
            }
        }

        private List<T> Filter<T>(IEnumerable<T> src, Func<T, string> key)
        {
            if (string.IsNullOrEmpty(search)) return src.ToList();
            string s = search.Trim().ToLowerInvariant();
            return src.Where(x => (key(x) ?? string.Empty).ToLowerInvariant().Contains(s)).ToList();
        }

        private void GatherBuiltinIcons()
        {
            Texture2D[] all = Resources.FindObjectsOfTypeAll<Texture2D>();
            foreach (var t in all)
            {
                if (t == null) continue;
                if (t.width <= 64 && t.height <= 64)
                {
                    if (t.name.Contains(" Icon") || t.name.StartsWith("d_") || t.name.EndsWith(" Icon") || t.name.EndsWith(" Icon Small"))
                        builtinIcons.Add(t);
                }
            }
            builtinIcons = builtinIcons.GroupBy(t => t != null ? t.name : string.Empty).Select(g => g.First()).OrderBy(t => t != null ? t.name : string.Empty).ToList();
        }

        private void GatherAssetIcons()
        {
            assetIcons.Clear();
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex == null) continue;
                if (tex.width <= 256 && tex.height <= 256)
                    assetIcons.Add((tex, guid));
            }
            assetIcons = assetIcons.OrderBy(x => x.tex != null ? x.tex.name : string.Empty).ToList();
        }

        private void GatherComponentIcons()
        {
            componentIcons.Clear();

            TryAddIconByName("DirectionalLight Icon", "Directional Light");
            TryAddIconByName("SpotLight Icon", "Spot Light");
            TryAddIconByName("AreaLight Icon", "Area Light");
            TryAddIconByType("UnityEngine.Light, UnityEngine.CoreModule", "Light");

            TryAddIconByName("Camera Icon", "Camera");
            TryAddIconByType("UnityEngine.Camera, UnityEngine.CoreModule", "Camera");

            TryAddIconByName("AudioSource Icon", "Audio Source");
            TryAddIconByType("UnityEngine.AudioSource, UnityEngine.CoreModule", "Audio Source");
            TryAddIconByName("AudioListener Icon", "Audio Listener");
            TryAddIconByType("UnityEngine.AudioListener, UnityEngine.CoreModule", "Audio Listener");

            TryAddIconByName("MeshRenderer Icon", "Mesh Renderer");
            TryAddIconByType("UnityEngine.MeshRenderer, UnityEngine.CoreModule", "Mesh Renderer");
            TryAddIconByName("SkinnedMeshRenderer Icon", "Skinned Mesh Renderer");
            TryAddIconByType("UnityEngine.SkinnedMeshRenderer, UnityEngine.CoreModule", "Skinned Mesh Renderer");
            TryAddIconByName("SpriteRenderer Icon", "Sprite Renderer");
            TryAddIconByType("UnityEngine.SpriteRenderer, UnityEngine.CoreModule", "Sprite Renderer");
            TryAddIconByName("LineRenderer Icon", "Line Renderer");
            TryAddIconByType("UnityEngine.LineRenderer, UnityEngine.CoreModule", "Line Renderer");
            TryAddIconByName("TrailRenderer Icon", "Trail Renderer");
            TryAddIconByType("UnityEngine.TrailRenderer, UnityEngine.CoreModule", "Trail Renderer");
            TryAddIconByName("ParticleSystem Icon", "Particle System");
            TryAddIconByType("UnityEngine.ParticleSystem, UnityEngine.ParticleSystemModule", "Particle System");
            TryAddIconByName("ParticleSystemForceField Icon", "Particle Force Field");

            TryAddIconByName("BoxCollider Icon", "Box Collider");
            TryAddIconByType("UnityEngine.BoxCollider, UnityEngine.PhysicsModule", "Box Collider");
            TryAddIconByName("SphereCollider Icon", "Sphere Collider");
            TryAddIconByType("UnityEngine.SphereCollider, UnityEngine.PhysicsModule", "Sphere Collider");
            TryAddIconByName("CapsuleCollider Icon", "Capsule Collider");
            TryAddIconByType("UnityEngine.CapsuleCollider, UnityEngine.PhysicsModule", "Capsule Collider");
            TryAddIconByName("MeshCollider Icon", "Mesh Collider");
            TryAddIconByType("UnityEngine.MeshCollider, UnityEngine.PhysicsModule", "Mesh Collider");

            TryAddIconByName("BoxCollider2D Icon", "Box Collider 2D");
            TryAddIconByType("UnityEngine.BoxCollider2D, UnityEngine.Physics2DModule", "Box Collider 2D");
            TryAddIconByName("CircleCollider2D Icon", "Circle Collider 2D");
            TryAddIconByType("UnityEngine.CircleCollider2D, UnityEngine.Physics2DModule", "Circle Collider 2D");
            TryAddIconByName("CapsuleCollider2D Icon", "Capsule Collider 2D");
            TryAddIconByType("UnityEngine.CapsuleCollider2D, UnityEngine.Physics2DModule", "Capsule Collider 2D");
            TryAddIconByName("EdgeCollider2D Icon", "Edge Collider 2D");
            TryAddIconByType("UnityEngine.EdgeCollider2D, UnityEngine.Physics2DModule", "Edge Collider 2D");
            TryAddIconByName("PolygonCollider2D Icon", "Polygon Collider 2D");
            TryAddIconByType("UnityEngine.PolygonCollider2D, UnityEngine.Physics2DModule", "Polygon Collider 2D");

            TryAddIconByName("Rigidbody Icon", "Rigidbody");
            TryAddIconByType("UnityEngine.Rigidbody, UnityEngine.PhysicsModule", "Rigidbody");
            TryAddIconByName("Rigidbody2D Icon", "Rigidbody 2D");
            TryAddIconByType("UnityEngine.Rigidbody2D, UnityEngine.Physics2DModule", "Rigidbody 2D");

            TryAddIconByName("Animator Icon", "Animator");
            TryAddIconByType("UnityEngine.Animator, UnityEngine.AnimationModule", "Animator");
            TryAddIconByName("Animation Icon", "Animation");
            TryAddIconByType("UnityEngine.Animation, UnityEngine.AnimationModule", "Animation");

            TryAddIconByName("Terrain Icon", "Terrain");
            TryAddIconByType("UnityEngine.Terrain, UnityEngine.TerrainModule", "Terrain");

            TryAddIconByName("ReflectionProbe Icon", "Reflection Probe");
            TryAddIconByType("UnityEngine.ReflectionProbe, UnityEngine.CoreModule", "Reflection Probe");
            TryAddIconByName("LightProbeGroup Icon", "Light Probe Group");
            TryAddIconByType("UnityEngine.LightProbeGroup, UnityEngine.CoreModule", "Light Probe Group");

            TryAddIconByName("Canvas Icon", "Canvas");
            TryAddIconByType("UnityEngine.Canvas, UnityEngine.UIModule", "Canvas");
            TryAddIconByName("RectTransform Icon", "RectTransform");
            TryAddIconByType("UnityEngine.RectTransform, UnityEngine.CoreModule", "RectTransform");
            TryAddIconByName("Image Icon", "UI Image");
            TryAddIconByType("UnityEngine.UI.Image, UnityEngine.UI", "UI Image");
            TryAddIconByName("Text Icon", "UI Text");
            TryAddIconByType("UnityEngine.UI.Text, UnityEngine.UI", "UI Text");
            TryAddIconByName("Button Icon", "UI Button");
            TryAddIconByType("UnityEngine.UI.Button, UnityEngine.UI", "UI Button");
            TryAddIconByName("EventSystem Icon", "Event System");
            TryAddIconByType("UnityEngine.EventSystems.EventSystem, UnityEngine.UI", "Event System");
            TryAddIconByType("TMPro.TextMeshProUGUI, Unity.TextMeshPro", "TextMeshPro UGUI");
            TryAddIconByType("TMPro.TextMeshPro, Unity.TextMeshPro", "TextMeshPro");

            TryAddIconByName("NavMeshAgent Icon", "NavMesh Agent");
            TryAddIconByType("UnityEngine.AI.NavMeshAgent, UnityEngine.AIModule", "NavMesh Agent");
            TryAddIconByType("UnityEngine.AI.NavMeshObstacle, UnityEngine.AIModule", "NavMesh Obstacle");

            TryAddIconByName("Grid Icon", "Grid");
            TryAddIconByType("UnityEngine.Grid, UnityEngine.GridModule", "Grid");
            TryAddIconByName("Tilemap Icon", "Tilemap");
            TryAddIconByType("UnityEngine.Tilemaps.Tilemap, UnityEngine.TilemapModule", "Tilemap");
            TryAddIconByName("TilemapRenderer Icon", "Tilemap Renderer");
            TryAddIconByType("UnityEngine.Tilemaps.TilemapRenderer, UnityEngine.TilemapModule", "Tilemap Renderer");

            TryAddIconByName("VideoPlayer Icon", "Video Player");
            TryAddIconByType("UnityEngine.Video.VideoPlayer, UnityEngine.VideoModule", "Video Player");
            TryAddIconByName("PlayableDirector Icon", "Playable Director");
            TryAddIconByType("UnityEngine.Playables.PlayableDirector, UnityEngine.DirectorModule", "Playable Director");

            TryAddIconByName("Sprite Icon", "Sprite");
            TryAddIconByName("PhysicsMaterial2D Icon", "Physics Material 2D");

            componentIcons = componentIcons
                .Where(x => x.tex != null)
                .GroupBy(x => x.tex)
                .Select(g => g.First())
                .OrderBy(x => x.label)
                .ToList();
        }

        private void TryAddIconByName(string iconName, string label)
        {
            GUIContent c = EditorGUIUtility.IconContent(iconName);
            Texture2D tex = c?.image as Texture2D;
            if (tex != null) AddComponentIcon(tex, label);
        }

        private void TryAddIconByType(string qualifiedTypeName, string fallbackLabel)
        {
            Type t = Type.GetType(qualifiedTypeName);
            if (t == null) return;
            Texture2D icon = EditorGUIUtility.ObjectContent(null, t)?.image as Texture2D;
            if (icon != null) AddComponentIcon(icon, fallbackLabel);
        }

        private void AddComponentIcon(Texture2D tex, string label)
        {
            componentIcons.Add((tex, label));
        }
        #endregion
    }
}
#endif