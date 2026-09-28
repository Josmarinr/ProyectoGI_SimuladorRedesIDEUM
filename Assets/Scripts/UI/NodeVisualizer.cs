using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using SimRedes.Network;

namespace SimRedes.UI
{
    /// <summary>
    /// Visualiza los nodos de la topologia como iconos y los enlaces como lineas
    /// dentro del canvas. Convencion de posiciones: CENTRO-RELATIVA ((0,0) = centro
    /// del canvas), igual que todos los escritores de node.Position. P2: los
    /// contenedores son RectTransforms con anclas centradas, DrawLinks refresca las
    /// posiciones de los iconos antes de calcular la geometria, resuelve la
    /// topologia de forma perezosa (tras recrear el GameManager) y re-apila la capa
    /// nodos+enlaces sobre los paneles del mismo canvas.
    /// </summary>
    public class NodeVisualizer : MonoBehaviour
    {
        [Header("References")]
        public Transform nodeContainer;
        public Transform linkContainer;

        [Header("Diagnostics")]
        // P2: telemetria de enlaces para confirmar en la mesa IDEUM. Apagada por
        // defecto (sin logs en el hot path). Se activa con F9 en runtime (patron
        // MemoryDiagnostics/F10) o marcando el checkbox en el Inspector durante la
        // reproduccion en Editor. Ver LogLinkDiagnostics().
        [SerializeField] private bool logLinkDiagnostics = false;

        private Dictionary<int, GameObject> nodeObjects = new Dictionary<int, GameObject>();
        private TopologyManager topology;
        // Instancia a la que estan suscritos los eventos. Puede diferir de `topology`
        // tras una recreacion del GameManager: `topology` se re-resuelve perezosamente
        // en DrawLinks y aqui se registra a quien nos suscribimos realmente.
        private TopologyManager subscribedTopology;
        private NodeInteractionController interactionController;

        private void Start()
        {
            interactionController = Object.FindAnyObjectByType<NodeInteractionController>();

            if (EnsureSubscribed())
                DrawLinks();
        }

        /// <summary>
        /// Alterna el diagnostico de enlaces con F9 (patron MemoryDiagnostics/F10).
        /// Un unico chequeo por frame y solo si hay teclado; el log estructurado
        /// [LinkDiag] lo emite DrawLinks unicamente con logLinkDiagnostics activo.
        /// </summary>
        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.f9Key.wasPressedThisFrame)
            {
                logLinkDiagnostics = !logLinkDiagnostics;
                Debug.Log($"[NodeVisualizer] Diagnostico de enlaces {(logLinkDiagnostics ? "ACTIVADO" : "DESACTIVADO")} (F9)");
            }
        }

        private void OnNodeAdded(NetworkNode node)
        {
            CreateNodeVisual(node);
        }

        private void OnNodeRemoved(NetworkNode node)
        {
            if (nodeObjects.TryGetValue(node.DiscId, out var obj))
            {
                if (obj != null) Destroy(obj);
                nodeObjects.Remove(node.DiscId);
            }
            DrawLinks();
        }

        private void OnTopologyChanged()
        {
            DrawLinks();
        }

        /// <summary>
        /// Resuelve perezosamente el TopologyManager (el cache de Start puede quedar
        /// fake-null tras recrear el GameManager), se suscribe a sus eventos si la
        /// instancia cambio y sincroniza los nodos que aun no tienen icono.
        /// Idempotente: con la instancia ya suscrita no hace nada (sin re-suscribir
        /// dos veces ni re-sincronizar en cada DrawLinks).
        /// </summary>
        /// <returns>True si hay una topologia activa a la que estar suscrito.</returns>
        private bool EnsureSubscribed()
        {
            // Unity-null check: cubre "nunca resuelta" y "destruida (fake-null)".
            if (topology == null)
                topology = Object.FindAnyObjectByType<TopologyManager>();
            if (topology == null) return false;

            if (!ReferenceEquals(topology, subscribedTopology))
            {
                Unsubscribe();
                subscribedTopology = topology;
                topology.OnNodeAdded += OnNodeAdded;
                topology.OnNodeRemoved += OnNodeRemoved;
                topology.OnTopologyChanged += OnTopologyChanged;
                SyncExistingNodes();
            }
            return true;
        }

        /// <summary>
        /// Cancela la suscripcion a la instancia cacheada. No-op si la instancia ya
        /// fue destruida: sus handlers murieron con ella.
        /// </summary>
        private void Unsubscribe()
        {
            if (subscribedTopology == null) return;
            subscribedTopology.OnNodeAdded -= OnNodeAdded;
            subscribedTopology.OnNodeRemoved -= OnNodeRemoved;
            subscribedTopology.OnTopologyChanged -= OnTopologyChanged;
            subscribedTopology = null;
        }

        /// <summary>
        /// Crea los iconos de los nodos existentes en la topologia. Idempotente:
        /// CreateNodeVisual ignora los DiscId ya presentes en nodeObjects.
        /// </summary>
        private void SyncExistingNodes()
        {
            if (topology == null) return;
            foreach (var node in topology.GetAllNodes())
                OnNodeAdded(node);
        }

        private void CreateNodeVisual(NetworkNode node)
        {
            if (nodeObjects.ContainsKey(node.DiscId)) return;
            if (nodeContainer == null)
            {
                Debug.LogError("[NodeVisualizer] nodeContainer es null, no se puede crear nodo visual");
                return;
            }

            Color nodeColor = UIComponents.Colors.GetColorForDeviceType(node.Type);
            string labelText = GetLabelForDeviceType(node.Type);
            int capturedDiscId = node.DiscId;

            GameObject nodeObj = new GameObject($"Node_{node.Name}", typeof(RectTransform));
            nodeObj.transform.SetParent(nodeContainer, false);

            RectTransform rect = (RectTransform)nodeObj.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = node.Position;
            rect.sizeDelta = new Vector2(80, 80);

            Image img = nodeObj.AddComponent<Image>();
            img.color = nodeColor;
            img.sprite = UIComponents.CreateDeviceIcon(64, nodeColor);
            img.type = Image.Type.Simple;

            var trigger = nodeObj.AddComponent<EventTrigger>();
            AddClickEvent(trigger, () => OnNodeClicked(capturedDiscId));

            Outline outline = nodeObj.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2, 2);

            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(nodeObj.transform, false);
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            Text text = labelObj.AddComponent<Text>();
            text.text = labelText;
            text.color = Color.white;
            text.fontSize = 14;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = UIComponents.GetFont();

            nodeObjects[node.DiscId] = nodeObj;

            DrawLinks();
        }

        private string GetLabelForDeviceType(SimRedes.Network.DeviceType type)
        {
            switch (type)
            {
                case SimRedes.Network.DeviceType.Router:
                    return "Router";
                case SimRedes.Network.DeviceType.Switch:
                    return "Switch";
                case SimRedes.Network.DeviceType.PC:
                    return "PC";
                default:
                    return "Disco";
            }
        }

        /// <summary>
        /// Crea un contenedor de nodos/enlaces con RectTransform propio, anclas y
        /// pivote centrados (0.5,0.5) y tamano cero. Los iconos y las lineas (anclas
        /// 0.5,0.5) resuelven contra esta rect degenerada en el centro de la capa:
        /// su posicion en el canvas es exactamente node.Position bajo la convencion
        /// centrada, con el mismo resultado tanto si el padre tiene RectTransform
        /// como si no (los dos comportamientos posibles de Unity convergen en (0,0)).
        /// Usado por SceneBootstrap y ActivityDispatcher, los dos puntos de creacion
        /// del visualizador.
        /// </summary>
        /// <param name="name">Nombre del GameObject contenedor.</param>
        /// <param name="parent">Transform padre (la capa NodeVisualizer).</param>
        /// <returns>RectTransform del contenedor.</returns>
        public static RectTransform CreateContainer(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            return rect;
        }

        /// <summary>
        /// Reinicia el cache de iconos. Para ser llamado por las rutas de limpieza
        /// que barren los hijos de los contenedores DIRECTAMENTE sin pasar por
        /// OnNodeRemoved (SceneCleanupService.ClearSimulation y
        /// DebugDiscSimulator.ClearAllDiscs): sin este reset, nodeObjects conserva
        /// entradas stale y DrawLinks salteaba los enlaces en silencio (P2/H4).
        /// No toca las suscripciones ni el contenido de los contenedores: el
        /// llamador es quien barre los hijos.
        /// </summary>
        public void ResetVisuals()
        {
            nodeObjects.Clear();
        }

        /// <summary>
        /// Redibuja todos los enlaces de la topologia. Se invoca en cada evento de
        /// topologia (OnTopologyChanged/OnNodeAdded/OnNodeRemoved) y por
        /// LinkModeController al crear/borrar enlaces; nunca lanza excepciones.
        /// Orden: re-apilar la capa, limpiar lineas, resolver topologia (perezoso,
        /// P2/H4), refrescar posiciones de iconos (P2/H2), calcular geometria con
        /// las posiciones ACTUALES y, si esta activo, emitir la telemetria [LinkDiag].
        /// </summary>
        public void DrawLinks()
        {
            if (linkContainer == null)
            {
                Debug.LogWarning("[NodeVisualizer] linkContainer es null, no se pueden dibujar enlaces");
                return;
            }

            // P2/H3: toda la capa (nodos + enlaces) se coloca como un solo hermano
            // al final, por encima de los paneles creados por el HUD en el MISMO
            // canvas. No se usa contra los fondos CreateClickOutsideToClose: esos
            // tienen su propio Canvas con sortingOrder=50 y su papel de modal
            // (B5/M1) no debe alterarse.
            transform.SetAsLastSibling();

            // P2/H4: resolver la topologia ANTES de limpiar, porque la primera
            // suscripcion sincroniza nodos y eso dispara DrawLinks anidados
            // (con la limpieza antes, el dibujo anidado se duplicaria al volver).
            if (!EnsureSubscribed())
            {
                ClearLinkChildren();
                Debug.LogWarning("[NodeVisualizer] topology es null en DrawLinks");
                return;
            }

            ClearLinkChildren();

            RefreshNodePositions();

            var allLinks = topology.GetAllLinks();

            foreach (var link in allLinks)
            {
                if (link.SourceNode == null || link.DestinationNode == null)
                {
                    Debug.LogWarning("[NodeVisualizer] Enlace con nodo nulo detectado");
                    continue;
                }

                if (!nodeObjects.TryGetValue(link.SourceNode.DiscId, out var srcObj) ||
                    !nodeObjects.TryGetValue(link.DestinationNode.DiscId, out var dstObj))
                {
                    continue;
                }

                if (srcObj == null || dstObj == null) continue;

                CreateLinkLine(srcObj.GetComponent<RectTransform>(),
                              dstObj.GetComponent<RectTransform>(),
                              link.IsFunctional());
            }

            LogLinkDiagnostics(allLinks.Count);
        }

        /// <summary>
        /// Destruye las lineas dibujadas antes de recalcularlas. Diferida en runtime
        /// (Destroy); inmediata en EditMode, donde Object.Destroy loguea un Error
        /// del entorno y fallaria los tests. El snapshot de hijos evita modificar la
        /// coleccion durante la iteracion con DestroyImmediate.
        /// </summary>
        private void ClearLinkChildren()
        {
            if (linkContainer.childCount == 0) return;

            var children = new List<GameObject>(linkContainer.childCount);
            foreach (Transform child in linkContainer)
            {
                if (child != null) children.Add(child.gameObject);
            }

            foreach (var child in children)
            {
                if (Application.isPlaying) Destroy(child);
                else DestroyImmediate(child);
            }
        }

        /// <summary>
        /// Sincroniza el anchoredPosition de cada icono con el node.Position actual
        /// (P2/H2: mover un disco fisico solo actualiza node.Position en
        /// TopologyManager; el icono se creo una unica vez en CreateNodeVisual).
        /// O(n) con lookup O(1), y solo dentro de DrawLinks (eventos de topologia),
        /// nunca por frame.
        /// </summary>
        private void RefreshNodePositions()
        {
            if (topology == null) return;

            foreach (var kvp in nodeObjects)
            {
                if (kvp.Value == null) continue;
                var node = topology.GetNode(kvp.Key);
                if (node == null) continue;
                var rect = kvp.Value.GetComponent<RectTransform>();
                if (rect != null && rect.anchoredPosition != node.Position)
                    rect.anchoredPosition = node.Position;
            }
        }

        /// <summary>
        /// Emite UNA linea estructurada con los contadores y la posicion de la
        /// primera linea, solo si logLinkDiagnostics esta activo (F9 o Inspector).
        /// Proposito: con una sola corrida en la mesa IDEUM discriminar H1 (origen
        /// de coordenadas), H2 (iconos desactualizados), H3 (capa tapada) y H4
        /// (topologia/cache stale) segun que valores aparezcan.
        /// </summary>
        /// <param name="linkCount">Cantidad de enlaces en la topologia.</param>
        private void LogLinkDiagnostics(int linkCount)
        {
            if (!logLinkDiagnostics) return;

            string first = "none";
            if (linkContainer.childCount > 0 && linkContainer.GetChild(0) is RectTransform firstRect)
            {
                Vector2 screen = RectTransformUtility.WorldToScreenPoint(null, firstRect.position);
                first = $"anchored={firstRect.anchoredPosition} screen={screen}";
            }

            Debug.Log($"[LinkDiag] links={linkCount} linkChildren={linkContainer.childCount} nodeObjects={nodeObjects.Count} first={first}");
        }

        private void CreateLinkLine(RectTransform from, RectTransform to, bool isActive)
        {
            GameObject lineObj = new GameObject("Link", typeof(RectTransform));
            lineObj.transform.SetParent(linkContainer, false);

            // Usar RawImage en lugar de Image - no necesita sprite y siempre renderiza
            RawImage lineImage = lineObj.AddComponent<RawImage>();
            // P2: las lineas de 10px no deben tragarse los taps que el modo
            // CONECTAR necesita sobre los nodos (raycastTarget desactivado).
            lineImage.raycastTarget = false;
            Color lineColor = isActive ? Color.green : Color.red;
            lineImage.color = lineColor;

            // Usar textura blanca 1x1 compartida (evita crear cientos de Texture2D)
            lineImage.texture = UIComponents.GetSharedWhiteTexture();

            RectTransform rect = (RectTransform)lineObj.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);

            Vector2 direction = to.anchoredPosition - from.anchoredPosition;
            float distance = direction.magnitude;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            rect.sizeDelta = new Vector2(Mathf.Max(distance, 2f), 10);
            rect.anchoredPosition = from.anchoredPosition + direction / 2f;
            rect.localEulerAngles = new Vector3(0, 0, angle);
        }

        private void AddClickEvent(EventTrigger trigger, UnityEngine.Events.UnityAction action)
        {
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener((data) => action());
            trigger.triggers.Add(entry);
        }

        private void OnNodeClicked(int discId)
        {
            CleanupNullReferences();

            UnityEngine.Debug.Log($"[NodeVisualizer] Nodo clickeado: {discId}");

            if (topology == null) return;

            var node = topology.GetNode(discId);
            if (node == null) return;

            HighlightSelectedNode(discId);

            if (interactionController != null)
            {
                if (interactionController.IsLinkModeActive())
                {
                    interactionController.CloseIPConfigPanelPublic();
                    interactionController.HandleNodeClick(discId);
                }
                else if (interactionController.IsPingModeActive())
                {
                    interactionController.CloseIPConfigPanelPublic();
                    interactionController.HandleNodeClick(discId);
                }
                else if (interactionController.IsIPConfigPanelOpen() && discId != interactionController.GetCurrentIPConfigNodeDiscId())
                {
                    interactionController.CloseIPConfigPanelPublic();
                    interactionController.ShowIPConfigPanel(node, discId);
                }
                else if (!interactionController.IsIPConfigPanelOpen())
                {
                    interactionController.ShowIPConfigPanel(node, discId);
                }
            }
        }

        private void HighlightSelectedNode(int discId)
        {
            CleanupNullReferences();

            foreach (var kvp in nodeObjects)
            {
                if (kvp.Value == null) continue;

                var outline = kvp.Value.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.effectColor = (kvp.Key == discId) ? Color.yellow : Color.black;
                    outline.effectDistance = (kvp.Key == discId) ? new Vector2(4, 4) : new Vector2(2, 2);
                }
            }
        }

        private void CleanupNullReferences()
        {
            var keysToRemove = new List<int>();
            foreach (var kvp in nodeObjects)
            {
                if (kvp.Value == null)
                    keysToRemove.Add(kvp.Key);
            }
            foreach (var key in keysToRemove)
            {
                nodeObjects.Remove(key);
            }
        }

        public void UpdateNodeLabel(int discId, string newLabel)
        {
            if (nodeObjects.TryGetValue(discId, out var nodeObj))
            {
                var labelObj = nodeObj.transform.Find("Label");
                if (labelObj != null)
                {
                    var text = labelObj.GetComponent<Text>();
                    if (text != null)
                    {
                        text.text = newLabel;
                        if (newLabel.Contains("\n"))
                        {
                            text.fontSize = 10;
                        }
                        else
                        {
                            text.fontSize = 14;
                        }
                    }

                    var labelRect = labelObj.GetComponent<RectTransform>();
                    if (labelRect != null)
                    {
                        labelRect.sizeDelta = new Vector2(120, newLabel.Contains("\n") ? 60 : 40);
                    }
                }
            }
        }

        public void SelectNodeByDiscId(int discId)
        {
            HighlightSelectedNode(discId);
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }
    }
}
