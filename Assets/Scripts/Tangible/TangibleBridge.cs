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

        // Antirrebote: no remover inmediatamente cuando TangibleEngine reporta la
        // ausencia de un disco. En su lugar, marcarlo como "pendiente de remocion"
        // y solo removerlo si permanece ausente por REMOVAL_DELAY segundos.
        // Esto evita parpadeo cuando el touch frame deja de detectar el disco
        // por frames sueltos (comun en mesas IDEUM).
        private Dictionary<int, float> pendingRemovals = new Dictionary<int, float>();
        private const float REMOVAL_DELAY = 1.0f;

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
        /// Procesa remociones pendientes: si un disco ha estado ausente por REMOVAL_DELAY
        /// segundos, lo remueve definitivamente del sistema.
        /// </summary>
        private void Update()
        {
            float now = Time.unscaledTime;
            var toRemove = new List<int>();
            foreach (var kvp in pendingRemovals)
            {
                if (now - kvp.Value >= REMOVAL_DELAY)
                    toRemove.Add(kvp.Key);
            }
            foreach (int tangibleId in toRemove)
            {
                ActuallyRemoveDisc(tangibleId);
                pendingRemovals.Remove(tangibleId);
            }
        }

        /// <summary>
        /// Ejecuta la remocion real de un disco: lo elimina de TangibleDiscManager y
        /// del mapeo tangibleId→uniqueId.
        /// </summary>
        private void ActuallyRemoveDisc(int tangibleId)
        {
            try
            {
                var manager = UnityEngine.Object.FindAnyObjectByType<TangibleDiscManager>();
                if (manager == null) return;

                if (!tangibleIdToUniqueId.TryGetValue(tangibleId, out int uniqueId))
                {
                    Log("WARN", $"Remove fallido: TE.Id={tangibleId} no estaba mapeado");
                    return;
                }

                manager.SimulateDiscRemoved(uniqueId);
                tangibleIdToUniqueId.Remove(tangibleId);
                Log("INFO", $"Disco removido (tras antirrebote): TE.Id={tangibleId} -> uniqueId={uniqueId}");
            }
            catch (System.Exception ex)
            {
                Log("ERROR", $"ActuallyRemoveDisc fallo: {ex.Message}");
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
                // Si estaba pendiente de remocion, cancelar (el disco sigue presente)
                if (pendingRemovals.Remove(tangible.Id))
                {
                    Log("INFO", $"Antirrebote: cancelada remocion pendiente de TE.Id={tangible.Id}");
                }

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
        /// Procesa la ausencia reportada de un disco fisico: en lugar de removerlo
        /// inmediatamente, lo marca como "pendiente de remocion". Solo se remueve
        /// si permanece ausente por REMOVAL_DELAY segundos (ver Update()).
        /// </summary>
        /// <param name="tangible">Dato del disco reportado como ausente por TangibleEngine.</param>
        private void HandleTangibleRemoved(TE.Tangible tangible)
        {
            try
            {
                if (!tangibleIdToUniqueId.ContainsKey(tangible.Id))
                {
                    Log("WARN", $"Tangible removido pero no estaba mapeado: TE.Id={tangible.Id}");
                    return;
                }

                // Marcar como pendiente de remocion (no remover aun)
                pendingRemovals[tangible.Id] = Time.unscaledTime;
                Log("INFO", $"Disco marcado para remocion pendiente: TE.Id={tangible.Id}");
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
        /// Convierte coordenadas TUIO (TangibleEngine) a coordenadas del Canvas (4096x2160).
        /// TangibleEngine siempre reporta en coordenadas del touch frame IDEUM (1920x1080),
        /// independientemente de la resolucion de pantalla.
        /// </summary>
        /// <param name="screenPosition">Posicion en pixeles TUIO (1920x1080).</param>
        /// <returns>Posicion escalada al espacio del Canvas (4096x2160).</returns>
        private Vector2 ConvertToCanvasPosition(Vector2 screenPosition)
        {
            // TE/TUIO siempre reporta en 1920x1080 (resolucion del touch frame IDEUM)
            const float TUIO_WIDTH = 1920f;
            const float TUIO_HEIGHT = 1080f;

            return new Vector2(
                screenPosition.x * (canvasWidth / TUIO_WIDTH),
                screenPosition.y * (canvasHeight / TUIO_HEIGHT)
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