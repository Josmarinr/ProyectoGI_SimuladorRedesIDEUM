using System.Collections.Generic;
using UnityEngine;
using TE;

namespace SimRedes.Tangible
{
    public class TangibleBridge : MonoBehaviour
    {
        [Header("Mapping - PatternId to DiscType")]
        [SerializeField] private int routerPatternId = 1;
        [SerializeField] private int switchPatternId = 2;
        [SerializeField] private int pcPatternId = 3;
        [SerializeField] private int enlacePatternId = 4;
        [SerializeField] private int falloPatternId = 5;
        [SerializeField] private int protocoloPatternId = 6;

        // Trackea la relación entre tangible.Id (del servicio TangibleEngine)
        // y el uniqueId generado por TangibleDiscManager
        private Dictionary<int, int> tangibleIdToUniqueId = new Dictionary<int, int>();

        [Header("Canvas Reference Resolution (de SceneSetup)")]
        [SerializeField] private float canvasWidth = 4096f;
        [SerializeField] private float canvasHeight = 2160f;

        private void Start()
        {
            TE.TangibleEngine.OnTangibleAdded += HandleTangibleAdded;
            TE.TangibleEngine.OnTangibleRemoved += HandleTangibleRemoved;
            TE.TangibleEngine.OnTangibleUpdated += HandleTangibleUpdated;

            UnityEngine.Debug.Log("[TangibleBridge] Conectado a TangibleEngine");
        }

        private void HandleTangibleAdded(TE.Tangible tangible)
        {
            // Si ya existe este tangible (reconexión del servicio), actualizar posición
            if (tangibleIdToUniqueId.ContainsKey(tangible.Id))
            {
                HandleTangibleUpdated(tangible);
                return;
            }

            int discType = MapPatternToDiscType(tangible.PatternId);
            Vector2 position = ConvertToCanvasPosition(new Vector2(tangible.X, tangible.Y));

            var manager = FindObjectOfType<TangibleDiscManager>();
            if (manager != null)
            {
                int uniqueId = manager.SimulateDiscPlaced(discType, position);
                tangibleIdToUniqueId[tangible.Id] = uniqueId;
                UnityEngine.Debug.Log($"[TangibleBridge] Disco añadido: TE.Id={tangible.Id} PatternId={tangible.PatternId} -> uniqueId={uniqueId}");
            }
        }

        private void HandleTangibleRemoved(TE.Tangible tangible)
        {
            var manager = FindObjectOfType<TangibleDiscManager>();
            if (manager == null) return;

            if (tangibleIdToUniqueId.TryGetValue(tangible.Id, out int uniqueId))
            {
                manager.SimulateDiscRemoved(uniqueId);
                tangibleIdToUniqueId.Remove(tangible.Id);
                UnityEngine.Debug.Log($"[TangibleBridge] Disco removido: TE.Id={tangible.Id} -> uniqueId={uniqueId}");
            }
        }

        private void HandleTangibleUpdated(TE.Tangible tangible)
        {
            Vector2 position = ConvertToCanvasPosition(new Vector2(tangible.X, tangible.Y));

            var manager = FindObjectOfType<TangibleDiscManager>();
            if (manager == null) return;

            if (tangibleIdToUniqueId.TryGetValue(tangible.Id, out int uniqueId))
            {
                var existingPos = manager.GetDiscPosition(uniqueId);
                if (existingPos.HasValue && Vector2.Distance(existingPos.Value, position) > 5f)
                {
                    manager.UpdateDiscPosition(uniqueId, position);
                    UnityEngine.Debug.Log($"[TangibleBridge] Disco actualizado: TE.Id={tangible.Id} -> uniqueId={uniqueId} nueva pos={position}");
                }
            }
        }

        private Vector2 ConvertToCanvasPosition(Vector2 screenPosition)
        {
            float displayWidth = Display.main.systemWidth;
            float displayHeight = Display.main.systemHeight;

            // Fallback: si Display no está disponible, usar Screen.currentResolution
            if (displayWidth <= 0 || displayHeight <= 0)
            {
                displayWidth = Screen.currentResolution.width;
                displayHeight = Screen.currentResolution.height;
            }

            if (displayWidth <= 0) displayWidth = 1920;
            if (displayHeight <= 0) displayHeight = 1080;

            return new Vector2(
                screenPosition.x * (canvasWidth / displayWidth),
                screenPosition.y * (canvasHeight / displayHeight)
            );
        }

        private int MapPatternToDiscType(int patternId)
        {
            switch (patternId)
            {
                case 1: return routerPatternId;
                case 2: return switchPatternId;
                case 3: return pcPatternId;
                case 4: return enlacePatternId;
                case 5: return falloPatternId;
                case 6: return protocoloPatternId;
                default: return patternId;
            }
        }

        private void OnDestroy()
        {
            TE.TangibleEngine.OnTangibleAdded -= HandleTangibleAdded;
            TE.TangibleEngine.OnTangibleRemoved -= HandleTangibleRemoved;
            TE.TangibleEngine.OnTangibleUpdated -= HandleTangibleUpdated;
        }
    }
}