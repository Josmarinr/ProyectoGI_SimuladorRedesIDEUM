using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TE;

namespace SimRedes.Tangible
{
    /// <summary>
    /// Puente entre el servicio TangibleEngine (IDEUM) y el simular.
    /// Traduce eventos de discos fisicos (anadir/mover/quitar) del hardware IDEUM
    /// a llamadas en TangibleDiscManager, mapeando IDs de TangibleEngine a IDs unicos.
    /// </summary>
    public class TangibleBridge : MonoBehaviour
    {
        // Trackea la relación entre tangible.Id (del servicio TangibleEngine)
        // y el uniqueId generado por TangibleDiscManager
        private Dictionary<int, int> tangibleIdToUniqueId = new Dictionary<int, int>();

        [Header("Canvas Reference Resolution (de SceneSetup)")]
        [SerializeField] private float canvasWidth = 4096f;
        [SerializeField] private float canvasHeight = 2160f;

        /// <summary>
        /// Se suscribe a los eventos de TangibleEngine (anadir/mover/quitar)
        /// e inicia la corrutina de verificacion periodica de conectividad.
        /// </summary>
        private void Start()
        {
            try
            {
                TE.TangibleEngine.OnTangibleAdded += HandleTangibleAdded;
                TE.TangibleEngine.OnTangibleRemoved += HandleTangibleRemoved;
                TE.TangibleEngine.OnTangibleUpdated += HandleTangibleUpdated;
                Log("INFO", "Conectado a TangibleEngine — eventos suscritos");
                StartCoroutine(PeriodicConnectivityCheck());
            }
            catch (System.Exception ex)
            {
                Log("ERROR", $"Fallo al suscribirse a eventos TangibleEngine: {ex.Message}");
            }
        }

        private void Log(string level, string message)
        {
            UnityEngine.Debug.Log($"[TangibleBridge][{level}][{System.DateTime.Now:HH:mm:ss.fff}] {message}");
        }

        /// <summary>
        /// Verifica cada 5 segundos (hasta 30s) que TangibleDiscManager exista
        /// y tenga discos activos, alertando si no detecta discos tras 10s.
        /// </summary>
        private System.Collections.IEnumerator PeriodicConnectivityCheck()
        {
            float elapsed = 0f;
            while (elapsed < 30f)
            {
                yield return new UnityEngine.WaitForSeconds(5f);
                elapsed += 5f;
                var manager = UnityEngine.Object.FindAnyObjectByType<TangibleDiscManager>();
                if (manager == null)
                    Log("WARN", $"TangibleDiscManager no encontrado tras {elapsed:F0}s");
                else
                {
                    int count = manager.GetActiveDiscs().Count;
                    Log("INFO", $"Chequeo periodico: {count} discos activos tras {elapsed:F0}s");
                    if (elapsed >= 10f && count == 0)
                        Log("WARN", "Sin discos tras 10s — TangibleEngine podria no estar conectado al servicio IDEUM (localhost:4949)");
                }
            }
        }

        /// <summary>
        /// Procesa la deteccion de un nuevo disco fisico: mapea su PatternId a tipo,
        /// convierte coordenadas y lo registra en TangibleDiscManager.
        /// </summary>
        /// <param name="tangible">Dato del disco detectado por TangibleEngine.</param>
        private void HandleTangibleAdded(TE.Tangible tangible)
        {
            try
            {
                if (tangibleIdToUniqueId.ContainsKey(tangible.Id))
                {
                    HandleTangibleUpdated(tangible);
                    return;
                }

                int discType = MapPatternToDiscType(tangible.PatternId);
                if (discType == -1)
                {
                    Log("WARN", $"Tangible ignorado (no es un disco fisico valido): TE.Id={tangible.Id} PatternId={tangible.PatternId}");
                    return;
                }

                Vector2 position = ConvertToCanvasPosition(new Vector2(tangible.X, tangible.Y));

                var manager = UnityEngine.Object.FindAnyObjectByType<TangibleDiscManager>();
                if (manager != null)
                {
                    int uniqueId = manager.SimulateDiscPlaced(discType, position);
                    tangibleIdToUniqueId[tangible.Id] = uniqueId;
                    Log("INFO", $"Disco añadido: TE.Id={tangible.Id} PatternId={tangible.PatternId} -> uniqueId={uniqueId}");
                }
            }
            catch (System.Exception ex)
            {
                Log("ERROR", $"HandleTangibleAdded falló: {ex.Message}");
            }
        }

        /// <summary>
        /// Procesa la remocion de un disco fisico: lo elimina de TangibleDiscManager
        /// y del mapeo interno.
        /// </summary>
        /// <param name="tangible">Dato del disco removido por TangibleEngine.</param>
        private void HandleTangibleRemoved(TE.Tangible tangible)
        {
            try
            {
                var manager = UnityEngine.Object.FindAnyObjectByType<TangibleDiscManager>();
                if (manager == null) return;

                if (!tangibleIdToUniqueId.TryGetValue(tangible.Id, out int uniqueId))
                {
                    Log("WARN", $"Tangible removido pero no estaba mapeado: TE.Id={tangible.Id}");
                    return;
                }

                manager.SimulateDiscRemoved(uniqueId);
                tangibleIdToUniqueId.Remove(tangible.Id);
                Log("INFO", $"Disco removido: TE.Id={tangible.Id} -> uniqueId={uniqueId}");
            }
            catch (System.Exception ex)
            {
                Log("ERROR", $"HandleTangibleRemoved falló: {ex.Message}");
            }
        }

        /// <summary>
        /// Actualiza la posicion de un disco fisico cuando TangibleEngine reporta movimiento,
        /// solo si la distancia supera el umbral de 5px.
        /// </summary>
        /// <param name="tangible">Dato del disco actualizado por TangibleEngine.</param>
        private void HandleTangibleUpdated(TE.Tangible tangible)
        {
            try
            {
                Vector2 position = ConvertToCanvasPosition(new Vector2(tangible.X, tangible.Y));

                var manager = UnityEngine.Object.FindAnyObjectByType<TangibleDiscManager>();
                if (manager == null) return;

                if (!tangibleIdToUniqueId.TryGetValue(tangible.Id, out int uniqueId))
                {
                    Log("WARN", $"Tangible actualizado pero no mapeado: TE.Id={tangible.Id}");
                    return;
                }

                var existingPos = manager.GetDiscPosition(uniqueId);
                if (existingPos.HasValue && Vector2.Distance(existingPos.Value, position) > 5f)
                {
                    manager.UpdateDiscPosition(uniqueId, position);
                    Log("INFO", $"Disco actualizado: TE.Id={tangible.Id} -> uniqueId={uniqueId} nueva pos={position}");
                }
            }
            catch (System.Exception ex)
            {
                Log("ERROR", $"HandleTangibleUpdated falló: {ex.Message}");
            }
        }

        /// <summary>
        /// Convierte coordenadas de pantalla (TangibleEngine) a coordenadas del Canvas,
        /// usando la resolucion de referencia configurada (4096x2160).
        /// </summary>
        /// <param name="screenPosition">Posicion en pixeles del TangibleEngine.</param>
        /// <returns>Posicion escalada al espacio del Canvas.</returns>
        private Vector2 ConvertToCanvasPosition(Vector2 screenPosition)
        {
            float displayWidth = Display.main.systemWidth;
            float displayHeight = Display.main.systemHeight;

            if (displayWidth <= 0 || displayHeight <= 0)
            {
                Log("WARN", $"Display.main invalido ({displayWidth}x{displayHeight}), usando Screen.currentResolution");
                displayWidth = Screen.currentResolution.width;
                displayHeight = Screen.currentResolution.height;
            }

            if (displayWidth <= 0) { displayWidth = 1920; Log("WARN", "displayWidth forzado a 1920"); }
            if (displayHeight <= 0) { displayHeight = 1080; Log("WARN", "displayHeight forzado a 1080"); }

            return new Vector2(
                screenPosition.x * (canvasWidth / displayWidth),
                screenPosition.y * (canvasHeight / displayHeight)
            );
        }

        /// <summary>
        /// Mapea el PatternId de TangibleEngine al tipo de disco fisico (1=Router, 2=Switch, 3=PC).
        /// Descarta patrones fuera del rango 1-3.
        /// </summary>
        /// <param name="patternId">Identificador de patron del disco fisico.</param>
        /// <returns>Tipo de disco (1-3) o -1 si no es un disco fisico valido.</returns>
        private int MapPatternToDiscType(int patternId)
        {
            // Solo 3 discos fisicos: Router(1), Switch(2), PC(3)
            // Enlaces, fallos y configuracion de routing se manejan desde las actividades
            if (patternId >= 1 && patternId <= 3)
                return patternId;
            Log("WARN", $"PatternId={patternId} no es un disco fisico valido (solo 1-3). Ignorando.");
            return -1; // señal de ignorar
        }

        /// <summary>
        /// Desuscribe todos los eventos de TangibleEngine para evitar fugas de memoria.
        /// </summary>
        private void OnDestroy()
        {
            TE.TangibleEngine.OnTangibleAdded -= HandleTangibleAdded;
            TE.TangibleEngine.OnTangibleRemoved -= HandleTangibleRemoved;
            TE.TangibleEngine.OnTangibleUpdated -= HandleTangibleUpdated;
        }
    }
}