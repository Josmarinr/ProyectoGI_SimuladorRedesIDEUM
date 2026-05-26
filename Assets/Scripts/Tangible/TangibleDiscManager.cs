using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimRedes.Tangible
{
    public class TangibleDiscManager : MonoBehaviour
    {
        public static TangibleDiscManager Instance { get; private set; }

        public event Action<int, Vector2> OnDiscPlaced;
        public event Action<int, Vector2> OnDiscMoved;
        public event Action<int> OnDiscRemoved;

        private Dictionary<int, Vector2> activeDiscs = new Dictionary<int, Vector2>();
        private Dictionary<int, int> discTypeToDiscId = new Dictionary<int, int>();
        private int instanceCounter = 1;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

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

        public void UpdateDiscPosition(int uniqueId, Vector2 position)
        {
            if (activeDiscs.ContainsKey(uniqueId))
            {
                activeDiscs[uniqueId] = position;
                OnDiscMoved?.Invoke(uniqueId, position);
                UnityEngine.Debug.Log($"[DiscManager] Disco movido: uniqueId={uniqueId}, pos={position}");
            }
        }

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

        public Dictionary<int, Vector2> GetActiveDiscs()
        {
            return new Dictionary<int, Vector2>(activeDiscs);
        }

        public bool IsDiscActive(int discId)
        {
            return activeDiscs.ContainsKey(discId);
        }

        public Vector2? GetDiscPosition(int discId)
        {
            return activeDiscs.TryGetValue(discId, out Vector2 pos) ? pos : null;
        }

        public int GetDiscType(int discId)
        {
            return discTypeToDiscId.TryGetValue(discId, out int type) ? type : 0;
        }

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