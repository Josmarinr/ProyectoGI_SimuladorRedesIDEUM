using UnityEngine;
using UnityEngine.InputSystem;

namespace SimRedes.Core
{
    /// <summary>
    /// Diagnostico de memoria en tiempo de ejecucion. Se auto-instala al cargar
    /// la escena (sin tocar SceneSetup): crea un GameObject oculto con
    /// DontDestroyOnLoad que cada 5 segundos registra UNA linea compacta con el
    /// conteo de Sprite, Texture2D, Object y GameObject en memoria. Sirve para
    /// medir fugas: la curva debe quedar plana en idle y al arrastrar discos.
    /// Tecla F10 alterna el diagnostico; activo por defecto.
    /// Solo se instala en Editor o Development Builds (la decision es en tiempo
    /// de ejecucion: el preprocessor DEVELOPMENT_BUILD esta deprecado en
    /// Unity 6000.6+); en builds de liberacion no crea nada.
    /// </summary>
    public class MemoryDiagnostics : MonoBehaviour
    {
        /// <summary>Intervalo entre lineas de diagnostico, en segundos.</summary>
        private const float LOG_INTERVAL = 5f;

        // Activo por defecto: funciona sin conocer la tecla de toggle.
        private bool diagEnabled = true;
        private float timer = 0f;

        /// <summary>
        /// Auto-instalacion: crea el GameObject del diagnostico despues de cargar
        /// la escena. No depende de SceneSetup para no alterar el arranque.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            // Gate en tiempo de ejecucion: solo Editor o Development Builds.
            if (!Application.isEditor && !Debug.isDebugBuild) return;

            var go = new GameObject("MemoryDiagnostics");
            go.hideFlags = HideFlags.HideInHierarchy;
            UnityEngine.Object.DontDestroyOnLoad(go);
            go.AddComponent<MemoryDiagnostics>();

            // Log directo (no AppLogger): el diagnostico debe verse sin EnableLogging.
            UnityEngine.Debug.Log("[MemDiag] Diagnostico de memoria ACTIVO - tecla F10 alterna on/off");
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.f10Key.wasPressedThisFrame)
            {
                diagEnabled = !diagEnabled;
                UnityEngine.Debug.Log(diagEnabled
                    ? "[MemDiag] ACTIVADO"
                    : "[MemDiag] DESACTIVADO");
            }

            if (!diagEnabled) return;

            timer += Time.deltaTime;
            if (timer < LOG_INTERVAL) return;
            timer = 0f;
            LogCounts();
        }

        /// <summary>
        /// Registra una linea compacta con los conteos de objetos en memoria.
        /// </summary>
        private static void LogCounts()
        {
            int sprites = Resources.FindObjectsOfTypeAll<Sprite>().Length;
            int textures = Resources.FindObjectsOfTypeAll<Texture2D>().Length;
            int objects = Resources.FindObjectsOfTypeAll<UnityEngine.Object>().Length;
            int gos = Resources.FindObjectsOfTypeAll<GameObject>().Length;
            UnityEngine.Debug.Log($"[MemDiag] sprites={sprites} textures={textures} objects={objects} gos={gos}");
        }
    }
}
