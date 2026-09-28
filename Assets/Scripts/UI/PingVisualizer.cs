using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SimRedes.Network;

namespace SimRedes.UI
{
    public class PingVisualizer : MonoBehaviour
    {
        public static PingVisualizer Instance { get; private set; }

        private Canvas canvas;
        private RectTransform canvasRect;
        private GameObject packetPrefab;
        private Sprite packetPrefabSprite;
        private GameObject packetContainer;
        private bool isAnimating = false;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                canvasRect = canvas.GetComponent<RectTransform>();
            }

            packetContainer = new GameObject("PingPackets");
            packetContainer.transform.SetParent(canvas?.transform ?? null);
            RectTransform containerRect = packetContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;

            CreatePacketPrefab();
        }

        private void CreatePacketPrefab()
        {
            if (packetContainer == null) return;

            packetPrefab = new GameObject("PingPacket");
            packetPrefab.transform.SetParent(packetContainer.transform);

            RectTransform rect = packetPrefab.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(30, 30);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            var img = packetPrefab.AddComponent<Image>();
            img.color = Color.yellow;

            int size = 30;
            Texture2D tex = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];
            int center = size / 2;
            int radius = center - 3;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (dist <= radius)
                        pixels[y * size + x] = Color.yellow;
                    else if (dist <= radius + 1)
                        pixels[y * size + x] = Color.Lerp(Color.yellow, Color.black, (dist - radius) / 2f);
                    else
                        pixels[y * size + x] = Color.clear;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            img.sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            packetPrefabSprite = img.sprite;

            packetPrefab.SetActive(false);
        }

        /// <summary>
        /// Destruye el contenedor PingPackets y los Sprite/texturas de los paquetes
        /// al destruirse el componente. El contenedor esta parenteado al Canvas (que
        /// nunca se descarga), asi que sin esta limpieza sobrevivia al componente
        /// y quedaba fuera de toda limpieza de escena (B5).
        /// </summary>
        private void OnDestroy()
        {
            // Sprite/textura del prefab: destruccion INCONDICIONAL y ANTES del
            // guard del contenedor (B2). Si "PingPackets" se destruye primero en
            // este mismo frame (el orden de Destroy no esta garantizado), el guard
            // de abajo haria early-out y este sprite con su textura quedarian
            // huerfanos. La referencia se mantiene hasta despues del bucle para
            // que el skip de instancias compartidas siga funcionando.
            if (packetPrefabSprite != null)
            {
                if (packetPrefabSprite.texture != null) Destroy(packetPrefabSprite.texture);
                Destroy(packetPrefabSprite);
            }

            if (packetContainer != null)
            {
                // Sprite/texturas propias de los paquetes en vuelo. Se omite el sprite
                // del prefab: es compartido por las instancias y ya se destruyo arriba.
                foreach (Transform child in packetContainer.transform)
                {
                    if (child == null) continue;
                    var img = child.GetComponent<Image>();
                    if (img == null || img.sprite == null) continue;
                    if (img.sprite == packetPrefabSprite) continue;
                    if (img.sprite.texture != null) Destroy(img.sprite.texture);
                    Destroy(img.sprite);
                    img.sprite = null;
                }

                Destroy(packetContainer);
                packetContainer = null;
            }

            // Limpiar la referencia al final: el guard de arriba la usa para
            // identificar el sprite compartido del prefab.
            packetPrefabSprite = null;

            if (Instance == this) Instance = null;
        }

        public void AnimatePing(int sourceDiscId, int destDiscId, System.Action<bool> onComplete)
        {
            UnityEngine.Debug.Log($"[PingVisual] Iniciando ping {sourceDiscId} -> {destDiscId}");

            if (isAnimating)
            {
                UnityEngine.Debug.LogWarning("[PingVisual] Ya hay animacion en progreso");
                onComplete?.Invoke(false);
                return;
            }

            var topology = Object.FindAnyObjectByType<TopologyManager>();
            if (topology == null)
            {
                UnityEngine.Debug.LogError("[PingVisual] TopologyManager no encontrado");
                onComplete?.Invoke(false);
                return;
            }

            var sourceNode = topology.GetNode(sourceDiscId);
            var destNode = topology.GetNode(destDiscId);

            if (sourceNode == null || destNode == null)
            {
                UnityEngine.Debug.LogError($"[PingVisual] Nodo no encontrado: {(sourceNode == null ? sourceDiscId.ToString() : "")} {(destNode == null ? destDiscId.ToString() : "")}");
                onComplete?.Invoke(false);
                return;
            }

            UnityEngine.Debug.Log($"[PingVisual] Nodos encontrados: {sourceNode.Name} -> {destNode.Name}");

            var sourceVisual = FindNodeVisual(sourceDiscId);
            var destVisual = FindNodeVisual(destDiscId);

            if (sourceVisual == null || destVisual == null)
            {
                UnityEngine.Debug.LogError($"[PingVisual] Visual no encontrado: src={sourceVisual != null}, dst={destVisual != null}");
                onComplete?.Invoke(false);
                return;
            }

            UnityEngine.Debug.Log($"[PingVisual] Visuales encontrados, iniciando animacion");

            StartCoroutine(AnimatePacket(sourceVisual, destVisual, sourceNode, destNode, onComplete));
        }

        private IEnumerator AnimatePacket(Transform source, Transform dest, NetworkNode sourceNode, NetworkNode destNode, System.Action<bool> onComplete)
        {
            isAnimating = true;

            UnityEngine.Debug.Log("[PingVisual] Creando paquete...");

            GameObject packet = Instantiate(packetPrefab, packetContainer.transform);
            packet.SetActive(true);

            RectTransform sourceRect = source.GetComponent<RectTransform>();
            RectTransform destRect = dest.GetComponent<RectTransform>();

            if (sourceRect == null || destRect == null)
            {
                UnityEngine.Debug.LogError("[PingVisual] RectTransform null");
                Destroy(packet);
                isAnimating = false;
                onComplete?.Invoke(false);
                yield break;
            }

            var topology = Object.FindAnyObjectByType<TopologyManager>();
            bool connected = false;
            string reason = "";
            List<Transform> pathTransforms = new List<Transform>();

            if (topology != null)
            {
                var path = topology.FindPath(sourceNode.DiscId, destNode.DiscId);

                if (path.Count > 0)
                {
                    pathTransforms.Add(source);
                    foreach (var node in path)
                    {
                        var nodeTransform = FindNodeVisual(node.DiscId);
                        if (nodeTransform != null && !pathTransforms.Contains(nodeTransform))
                            pathTransforms.Add(nodeTransform);
                    }
                    if (pathTransforms.Count > 0 && pathTransforms[pathTransforms.Count - 1] != dest)
                        pathTransforms.Add(dest);
                }

                connected = topology.CheckConnectivity(sourceNode.DiscId, destNode.DiscId);

                if (!connected)
                {
                    reason = BuildConnectivityFailReason(sourceNode, destNode);
                }
            }
            else
            {
                connected = sourceNode.IsActive && destNode.IsActive;
                if (!connected) reason = "Nodos inactivos";
            }

            if (pathTransforms.Count == 0)
            {
                pathTransforms.Add(source);
                pathTransforms.Add(dest);
            }

            if (!connected)
            {
                UnityEngine.Debug.LogWarning($"[PingVisual] Fallo: {reason}");
            }

            RectTransform packetRect = packet.GetComponent<RectTransform>();
            packetRect.anchoredPosition = ((RectTransform)pathTransforms[0].transform).anchoredPosition;

            float segmentDuration = 0.5f;
            float totalDuration = segmentDuration * (pathTransforms.Count - 1);
            float elapsed = 0f;
            int currentSegment = 0;

            while (elapsed < totalDuration)
            {
                elapsed += Time.deltaTime;
                float segmentT = (elapsed / segmentDuration) - currentSegment;

                if (segmentT >= 1f && currentSegment < pathTransforms.Count - 2)
                {
                    currentSegment++;
                    segmentT = 0f;
                }

                if (currentSegment >= pathTransforms.Count - 1)
                    break;

                var from = (RectTransform)pathTransforms[currentSegment];
                var to = (RectTransform)pathTransforms[currentSegment + 1];

                Vector2 startPos = from.anchoredPosition;
                Vector2 endPos = to.anchoredPosition;

                float t = segmentT * segmentT * (3f - 2f * segmentT);

                packetRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

                float scale = 1f + Mathf.Sin(elapsed * 8f) * 0.2f;
                packetRect.localScale = new Vector3(scale, scale, 1f);

                yield return null;
            }

            packetRect.anchoredPosition = ((RectTransform)pathTransforms[pathTransforms.Count - 1]).anchoredPosition;

            Color resultColor = connected ? Color.green : Color.red;

            var img = packet.GetComponent<Image>();
            img.color = resultColor;

            var pixels = new Color[30 * 30];
            int center = 15;
            int radius = 12;

            for (int y = 0; y < 30; y++)
            {
                for (int x = 0; x < 30; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (dist <= radius)
                        pixels[y * 30 + x] = resultColor;
                    else
                        pixels[y * 30 + x] = Color.clear;
                }
            }

            Texture2D tex = new Texture2D(30, 30);
            tex.SetPixels(pixels);
            tex.Apply();
            img.sprite = Sprite.Create(tex, new Rect(0, 0, 30, 30), new Vector2(0.5f, 0.5f), 30);

            string pathStr = pathTransforms.Count > 2 ? $" (via {pathTransforms.Count - 2} nodos)" : "";
            UnityEngine.Debug.Log($"[PingVisual] Paquete llego{pathStr}, resultado: {(connected ? "OK" : "FALLO")} - {reason}");

            yield return new WaitForSeconds(0.4f);

            // Destruir textura y sprite antes de destruir el GameObject para evitar fugas
            var packetImg = packet.GetComponent<Image>();
            if (packetImg != null && packetImg.sprite != null)
            {
                if (packetImg.sprite.texture != null)
                    Destroy(packetImg.sprite.texture);
                Destroy(packetImg.sprite);
            }
            Destroy(packet);
            isAnimating = false;

            UnityEngine.Debug.Log($"[PingVisual] {sourceNode.Name}({sourceNode.IpAddress}) -> {destNode.Name}({destNode.IpAddress}): {(connected ? "OK" : "FALLO")} - {reason}");
            onComplete?.Invoke(connected);
        }

        private Transform FindNodeVisual(int discId)
        {
            var visualizer = Object.FindAnyObjectByType<NodeVisualizer>();
            if (visualizer == null)
            {
                UnityEngine.Debug.LogError("[PingVisual] NodeVisualizer no encontrado");
                return null;
            }

            if (visualizer.nodeContainer == null)
            {
                UnityEngine.Debug.LogError("[PingVisual] nodeContainer es null");
                return null;
            }

            UnityEngine.Debug.Log($"[PingVisual] Buscando nodo con discId={discId}");
            UnityEngine.Debug.Log($"[PingVisual] Hijos en nodeContainer: {visualizer.nodeContainer.childCount}");

            foreach (Transform child in visualizer.nodeContainer)
            {
                if (child != null)
                {
                    UnityEngine.Debug.Log($"[PingVisual] Hijo: {child.name}");
                    if (child.name.EndsWith($"_{discId}"))
                    {
                        UnityEngine.Debug.Log($"[PingVisual] Encontrado: {child.name}");
                        return child;
                    }
                }
            }

            UnityEngine.Debug.LogWarning($"[PingVisual] No se encontro nodo con discId={discId}");
            return null;
        }

        public bool IsAnimating()
        {
            return isAnimating;
        }

        private string BuildConnectivityFailReason(NetworkNode source, NetworkNode dest)
        {
            var reasons = new List<string>();

            if (!IPValidation.IsValidIP(source.IpAddress))
                reasons.Add($"Origen sin IP");
            else if (!IPValidation.IsValidSubnetMask(source.SubnetMask))
                reasons.Add($"Origen sin mascara");

            if (!IPValidation.IsValidIP(dest.IpAddress))
                reasons.Add($"Destino sin IP");

            if (source.Type == Network.DeviceType.PC && dest.Type == Network.DeviceType.PC)
            {
                if (IPValidation.IsValidIP(source.IpAddress) && IPValidation.IsValidIP(dest.IpAddress) &&
                    IPValidation.IsValidSubnetMask(source.SubnetMask))
                {
                    if (!IPValidation.IsInSameNetwork(source.IpAddress, source.SubnetMask, dest.IpAddress))
                        reasons.Add($"Redes diferentes");
                }
            }

            if (source.Type == Network.DeviceType.Router && dest.Type == Network.DeviceType.Router)
            {
                if (IPValidation.IsValidIP(source.IpAddress) && IPValidation.IsValidIP(dest.IpAddress) &&
                    IPValidation.IsValidSubnetMask(source.SubnetMask))
                {
                    if (!IPValidation.IsInSameNetwork(source.IpAddress, source.SubnetMask, dest.IpAddress))
                        reasons.Add($"Redes diferentes");
                }
            }

            return reasons.Count > 0 ? string.Join(", ", reasons) : "Sin conexion";
        }
    }
}