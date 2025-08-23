#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace HierarchyDesigner
{
    internal class HD_Manager_Session : ScriptableSingleton<HD_Manager_Session>
    {
        #region Properties
        #region Hierarchy Designer Advanced Settings
        public bool ExpandHierarchyOnStartupOnce = false;
        #endregion

        #region Hierarchy Designer Main Window
        public bool IsPatchNotesLoaded = false;
        public string PatchNotesContent = string.Empty;
        public HD_Window_Main.CurrentWindow currentWindow = HD_Window_Main.CurrentWindow.Home;
        #endregion

        #region Hierarchy Designer Shared Texture
        public Font FallbackFont = null;
        public Texture2D FallbackTexture = null;
        #endregion
        #endregion
    }
}
#endif