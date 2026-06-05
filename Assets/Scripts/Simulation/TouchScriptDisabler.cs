using System.Collections;
using UnityEngine;
using TouchScript.Behaviors.Cursors;

namespace SimRedes
{
    public class TouchScriptDisabler : MonoBehaviour
    {
#if UNITY_EDITOR
        [Header("Deshabilitar en Editor")]
        [SerializeField] private bool disableTouchInEditor = true;
#endif

        private void Start()
        {
#if UNITY_EDITOR
            if (disableTouchInEditor)
                StartCoroutine(DisableTouchScriptCoroutine());
#endif
        }

        private void DisableTouchScriptInputModule()
        {
            var es = UnityEngine.Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (es == null) return;
            // TouchScriptInputModule es internal sealed, pero podemos deshabilitarlo por nombre
            var components = es.GetComponents<MonoBehaviour>();
            foreach (var c in components)
            {
                if (c != null && c.GetType().Name == "TouchScriptInputModule")
                {
                    c.enabled = false;
                    UnityEngine.Debug.Log("[TouchScript] TouchScriptInputModule deshabilitado en EventSystem");
                }
            }
        }

        private IEnumerator DisableTouchScriptCoroutine()
        {
            // Deshabilitar cursores inmediatamente
            var cursorManagers = FindObjectsByType<CursorManager>(FindObjectsSortMode.None);
            foreach (var cm in cursorManagers)
                cm.enabled = false;

            // Medida 1: Deshabilitar TouchScriptInputModule en el EventSystem
            DisableTouchScriptInputModule();

            // Medida 2: Forzar la creacion de TouchManagerInstance para deshabilitarlo 
            // ANTES de que el usuario haga clic (evita el primer doble clic).
            try { var _ = TouchScript.TouchManager.Instance; } catch { /* ignorar */ }

            // Buscar y desactivar "Touch Manager Instance" inmediatamente y en cada frame
            for (int i = 0; i < 60; i++)
            {
                var tmObj = GameObject.Find("TouchManager Instance");
                if (tmObj != null && tmObj.activeSelf)
                {
                    tmObj.SetActive(false);
                    UnityEngine.Debug.Log("[TouchScript] TouchManager Instance desactivado en frame " + i);
                    yield break;
                }
                yield return null;
            }
        }
    }
}