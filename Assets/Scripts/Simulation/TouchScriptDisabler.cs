using UnityEngine;
using TouchScript.Behaviors.Cursors;

namespace SimRedes
{
    public class TouchScriptDisabler : MonoBehaviour
    {
#if UNITY_EDITOR
        [Header("Deshabilitar cursores en Editor")]
        [SerializeField] private bool disableCursorsInEditor = true;
#endif

        private void Awake()
        {
#if UNITY_EDITOR
            if (disableCursorsInEditor)
            {
                DisableTouchScriptCursors();
            }
#endif
        }

        private void DisableTouchScriptCursors()
        {
            var cursorManagers = FindObjectsByType<CursorManager>(FindObjectsSortMode.None);
            foreach (var cm in cursorManagers)
            {
                cm.enabled = false;
                UnityEngine.Debug.Log($"[TouchScript] CursorManager deshabilitado: {cm.name}");
            }

            var touchManager = TouchScript.TouchManager.Instance;
            if (touchManager != null)
            {
                UnityEngine.Debug.Log("[TouchScript] TouchManager encontrado");
            }
        }
    }
}