using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimRedes.Tangible
{
    /// <summary>
    /// Administra el estado de todos los discos fisicos y virtuales en la escena.
    /// Mantiene diccionarios de discos activos, sus posiciones y tipos,
    /// y notifica cambios via eventos (OnDiscPlaced, OnDiscMoved, OnDiscRemoved).
    /// Implementa el patron singleton para acceso global.
    /// </summary>
    public class TangibleDiscManager : MonoBehaviour
    {
        /// <summary>
        /// Instancia unica del singleton. Se asigna en Awake y se destruye si ya existe otra.
        /// </summary>
        public static TangibleDiscManager Instance { get; private set; }

        /// <summary>
        /// Se invoca cuando un disco es colocado. Suscriptores: DiscEventHandler, DebugDiscSimulator.
        /// </summary>
        public event Action<int, Vector2> OnDiscPlaced;
        /// <summary>
        /// Se invoca cuando un disco existente cambia de posicion.
        /// </summary>
        public event Action<int, Vector2> OnDiscMoved;
        /// <summary>
        /// Se invoca cuando un disco es removido de la escena.
        /// </summary>
        public event Action<int> OnDiscRemoved;

        private Dictionary<int, Vector2> activeDiscs = new Dictionary<int, Vector2>();
        private Dictionary<int, int> discTypeToDiscId = new Dictionary<int, int>();
        private int instanceCounter = 1;

        /// <summary>
        /// Inicializa el singleton: asigna esta instancia o se autodestruye si ya existe otra.
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        /// <summary>
        /// Registra un nuevo disco en el sistema: le asigna un uniqueId unico,
        /// guarda su tipo y posicion, y dispara OnDiscPlaced.
        /// </summary>
        /// <param name="discType">Tipo de disco (1-18).</param>
        /// <param name="position">Posicion en coordenadas Canvas.</param>
        /// <returns>El uniqueId asignado al nuevo disco.</returns>
        public int SimulateDiscPlaced(int discType, Vector2 position)
        {
            if (discType <= 0 || discType > 18)
                UnityEngine.Debug.LogWarning($"[DiscManager] WARN: discType invalido={discType}");

            int uniqueId = BASE_DISC_ID + instanceCounter;
            instanceCounter++;

            activeDiscs[uniqueId] = position;
            discTypeToDiscId[uniqueId] = discType;

            int subscriberCount = OnDiscPlaced?.GetInvocationList()?.Length ?? 0;
            if (subscriberCount == 0)
                UnityEngine.Debug.LogWarning($"[DiscManager] WARN: OnDiscPlaced sin suscriptores al colocar disco {uniqueId}");

            OnDiscPlaced?.Invoke(uniqueId, position);
            UnityEngine.Debug.Log($"[DiscManager] Disco creado: tipo={discType}, uniqueId={uniqueId}, subs={subscriberCount}");
            return uniqueId;
        }

        /// <summary>
        /// Actualiza la posicion de un disco existente y dispara OnDiscMoved.
        /// </summary>
        /// <param name="uniqueId">Identificador unico del disco a mover.</param>
        /// <param name="position">Nueva posicion en coordenadas Canvas.</param>
        public void UpdateDiscPosition(int uniqueId, Vector2 position)
        {
            if (activeDiscs.ContainsKey(uniqueId))
            {
                activeDiscs[uniqueId] = position;
                OnDiscMoved?.Invoke(uniqueId, position);
                UnityEngine.Debug.Log($"[DiscManager] Disco movido: uniqueId={uniqueId}, pos={position}");
            }
        }

        /// <summary>
        /// Elimina un disco del sistema: lo remueve de los diccionarios y dispara OnDiscRemoved.
        /// </summary>
        /// <param name="discId">Identificador unico del disco a remover.</param>
        public void SimulateDiscRemoved(int discId)
        {
            if (!activeDiscs.ContainsKey(discId))
            {
                UnityEngine.Debug.LogWarning($"[DiscManager] WARN: Intento de remover disco {discId} que no existe");
                return;
            }

            if (activeDiscs.Remove(discId))
            {
                discTypeToDiscId.Remove(discId);
                int subs = OnDiscRemoved?.GetInvocationList()?.Length ?? 0;
                if (subs == 0)
                    UnityEngine.Debug.LogWarning($"[DiscManager] WARN: OnDiscRemoved sin suscriptores al remover {discId}");
                OnDiscRemoved?.Invoke(discId);
                UnityEngine.Debug.Log($"[DiscManager] Disco removido: {discId}");
            }
        }

        /// <summary>
        /// Retorna una copia del diccionario de discos activos (uniqueId -> posicion).
        /// </summary>
        /// <returns>Diccionario con todos los discos activos y sus posiciones.</returns>
        public Dictionary<int, Vector2> GetActiveDiscs()
        {
            return new Dictionary<int, Vector2>(activeDiscs);
        }

        /// <summary>
        /// Verifica si un disco con el ID dado esta actualmente activo en la escena.
        /// </summary>
        /// <param name="discId">Identificador unico del disco.</param>
        /// <returns>True si el disco existe en el diccionario de activos.</returns>
        public bool IsDiscActive(int discId)
        {
            return activeDiscs.ContainsKey(discId);
        }

        /// <summary>
        /// Obtiene la posicion actual de un disco activo.
        /// </summary>
        /// <param name="discId">Identificador unico del disco.</param>
        /// <returns>Posicion del disco, o null si no esta activo.</returns>
        public Vector2? GetDiscPosition(int discId)
        {
            return activeDiscs.TryGetValue(discId, out Vector2 pos) ? pos : null;
        }

        /// <summary>
        /// Obtiene el tipo de disco (1-18) asociado a un uniqueId.
        /// </summary>
        /// <param name="discId">Identificador unico del disco.</param>
        /// <returns>Tipo de disco, o 0 si no esta registrado.</returns>
        public int GetDiscType(int discId)
        {
            return discTypeToDiscId.TryGetValue(discId, out int type) ? type : 0;
        }

        /// <summary>
        /// Elimina todos los discos activos: limpia los diccionarios y notifica
        /// la remocion de cada disco mediante OnDiscRemoved.
        /// </summary>
        public void ClearAllDiscs()
        {
            var discIds = new List<int>(activeDiscs.Keys);
            activeDiscs.Clear();
            discTypeToDiscId.Clear();
            foreach (var id in discIds)
            {
                OnDiscRemoved?.Invoke(id);
            }
        }

        private const int BASE_DISC_ID = 100;
    }
}