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
            int uniqueId = BASE_DISC_ID + instanceCounter;
            instanceCounter++;

            activeDiscs[uniqueId] = position;
            discTypeToDiscId[uniqueId] = discType;
            OnDiscPlaced?.Invoke(uniqueId, position);

            UnityEngine.Debug.Log($"[DiscManager] Disco creado: tipo={discType}, uniqueId={uniqueId}");
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
            if (activeDiscs.Remove(discId))
            {
                discTypeToDiscId.Remove(discId);
                OnDiscRemoved?.Invoke(discId);
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