using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimRedes.Network;
using SimRedes.Tangible;
using SimRedes.UI;

namespace SimRedes.Simulation
{
    /// <summary>
    /// Escenario pre-definido para la actividad Encuentra el Fallo.
    /// Define la topologia, configuracion IP, tipo de fallo y que disco tactil lo repara.
    /// </summary>
    public class FindFaultScenario
    {
        /// <summary>Nombre visible del escenario.</summary>
        public string Name;
        /// <summary>Descripcion del fallo para el usuario.</summary>
        public string Description;
        /// <summary>Pista de como resolverlo.</summary>
        public string Hint;
        /// <summary>Etiqueta del boton disco tactil.</summary>
        public string DiscLabel;
        /// <summary>Indica si el usuario ya lo resolvio.</summary>
        public bool IsSolved;
        /// <summary>Dispositivos: (type, x, y) en coordenadas canvas.</summary>
        public List<(Network.DeviceType type, float x, float y)> Devices;
        /// <summary>Enlaces: (fromIndex, toIndex).</summary>
        public List<(int from, int to)> Links;
        /// <summary>Configuracion IP: (deviceIndex, ip, mask).</summary>
        public List<(int deviceIdx, string ip, string mask)> IPConfigs;
        /// <summary>Tipo de fallo: "cable", "ip", "interfaz", "gateway".</summary>
        public string FaultType;
        /// <summary>Indice del dispositivo afectado por el fallo.</summary>
        public int FaultDeviceIndex;
        /// <summary>IP correcta para reparacion (si aplica).</summary>
        public string CorrectIP;
        /// <summary>Mascara correcta para reparacion (si aplica).</summary>
        public string CorrectMask;
    }

    /// <summary>
    /// Actividad "Encuentra el Fallo" con 4 escenarios pre-hechos.
    /// Cada escenario construye una topologia conocida, aplica un fallo
    /// y el usuario lo repara usando discos tactiles (botones virtuales).
    /// </summary>
    public class FindFaultActivity : MonoBehaviour
    {
        [Header("UI References")]
        public Text scenarioTitleText;
        public Text faultDescriptionText;
        public Text hintText;
        public Text resultText;
        public Text statusText;
        public Button discButton;
        public Text discButtonText;
        public Button nextButton;
        public Button prevButton;
        public Text nextButtonText;

        private TopologyManager topologyManager;
        private List<FindFaultScenario> scenarios = null;
        private int currentIndex = 0;
        private NetworkNode affectedNode;
        private NetworkLink affectedLink;

        /// <summary>
        /// Inicializa escenarios inmediatamente al crearse el componente (AddComponent).
        /// Awake() se ejecuta en el mismo frame, ANTES que CreateFindFaultPanel().
        /// Tambien busca TopologyManager para que LoadScenario() pueda usarlo
        /// cuando ConnectUI() sea llamada desde CreateFindFaultPanel() en el mismo frame.
        /// </summary>
        private void Awake()
        {
            if (scenarios == null)
                InitializeScenarios();
            if (topologyManager == null)
                topologyManager = Object.FindAnyObjectByType<TopologyManager>();
        }

        /// <summary>
        /// Intentar encontrar TopologyManager de nuevo en Start (por si no existia en Awake).
        /// </summary>
        private void Start()
        {
            if (topologyManager == null)
                topologyManager = Object.FindAnyObjectByType<TopologyManager>();
        }

        /// <summary>
        /// Conecta las referencias UI y carga el primer escenario.
        /// Llamado por ActivityPanelFactory.CreateFindFaultPanel() DESPUES de crear los
        /// elementos UI. Para entonces Awake() ya inicializo los escenarios.
        /// </summary>
        public void ConnectUI(Text title, Text desc, Text hint, Text result, Text status,
            Button discBtn, Text discBtnText, Button next, Text nextBtnText, Button prev)
        {
            scenarioTitleText = title;
            faultDescriptionText = desc;
            hintText = hint;
            resultText = result;
            statusText = status;
            discButton = discBtn;
            discButtonText = discBtnText;
            nextButton = next;
            nextButtonText = nextBtnText;
            prevButton = prev;

            // Cargar escenario 0 ahora que todo existe (refs UI + escenarios + topologyManager)
            LoadScenario(0);
        }

        /// <summary>Configura los 4 escenarios de la actividad.</summary>
        private void InitializeScenarios()
        {
            scenarios = new List<FindFaultScenario>
            {
                // ─── Escenario 1: Cable Caído ───
                // Fallo: NO se crea el enlace. El usuario debe usar el boton CONECTAR del panel
                // y hacer click en ambos routers para crear el enlace manualmente.
                new FindFaultScenario
                {
                    Name = "Cable Ca\u00eddo",
                    Description = "El enlace entre dos routers est\u00e1 desconectado.\n" +
                                  "Los paquetes no pueden viajar de un router al otro.\n\n" +
                                  "\u25b6 Soluci\u00f3n: Presiona CONECTAR en el panel derecho,\n" +
                                  "luego haz click en Router1 y Router2.",
                    Hint = "Usa el bot\u00f3n CONECTAR del panel de topolog\u00eda,\n" +
                           "luego haz click en ambos routers.",
                    DiscLabel = "VERIFICAR",
                    Devices = new List<(Network.DeviceType type, float x, float y)>
                    {
                        (Network.DeviceType.Router, 350f, 400f),
                        (Network.DeviceType.Router, 700f, 400f)
                    },
                    Links = new List<(int from, int to)>(), // Sin enlace — usuario lo crea
                    IPConfigs = new List<(int deviceIdx, string ip, string mask)>
                    {
                        (0, "192.168.1.1", "255.255.255.0"),
                        (1, "192.168.1.2", "255.255.255.0")
                    },
                    FaultType = "cable",
                    FaultDeviceIndex = 0
                },

                // ─── Escenario 2: IP Errónea ───
                // Fallo: Router tiene IP incorrecta (192.168.100.99 en vez de 192.168.1.1)
                // Arreglo: Click en Router -> IPConfig -> cambiar IP -> APLICAR
                new FindFaultScenario
                {
                    Name = "IP Err\u00f3nea",
                    Description = "El Router tiene una direcci\u00f3n IP incorrecta.\n" +
                                  "Usa la IP 192.168.100.99 pero deber\u00eda ser 192.168.1.1.\n\n" +
                                  "\u25b6 Soluci\u00f3n: Click en Router \u2192 IPConfig\n" +
                                  "\u2192 cambiar IP a 192.168.1.1 \u2192 APLICAR",
                    Hint = "Haz click en el Router, luego en IPConfig,\n" +
                           "escribe 192.168.1.1 y presiona APLICAR.",
                    DiscLabel = "VERIFICAR",
                    Devices = new List<(Network.DeviceType type, float x, float y)>
                    {
                        (Network.DeviceType.Router, 500f, 500f),
                        (Network.DeviceType.PC, 500f, 250f)
                    },
                    Links = new List<(int from, int to)> { (0, 1) },
                    IPConfigs = new List<(int deviceIdx, string ip, string mask)>
                    {
                        (0, "192.168.100.99", "255.255.255.0"),
                        (1, "192.168.1.10", "255.255.255.0")
                    },
                    FaultType = "ip",
                    FaultDeviceIndex = 0,
                    CorrectIP = "192.168.1.1",
                    CorrectMask = "255.255.255.0"
                },

                // ─── Escenario 3: Máscara Incorrecta ───
                // Fallo: Router tiene mascara 255.0.0.0 en vez de 255.255.255.0
                // Arreglo: Click en Router -> IPConfig -> cambiar mascara -> APLICAR
                new FindFaultScenario
                {
                    Name = "M\u00e1scara Incorrecta",
                    Description = "El Router tiene una m\u00e1scara de red incorrecta.\n" +
                                  "Usa 255.0.0.0 pero deber\u00eda ser 255.255.255.0.\n\n" +
                                  "\u25b6 Soluci\u00f3n: Click en Router \u2192 IPConfig\n" +
                                  "\u2192 cambiar M\u00e1scara a 255.255.255.0 \u2192 APLICAR",
                    Hint = "Haz click en el Router, luego en IPConfig,\n" +
                           "cambia la M\u00e1scara a 255.255.255.0 y presiona APLICAR.",
                    DiscLabel = "VERIFICAR",
                    Devices = new List<(Network.DeviceType type, float x, float y)>
                    {
                        (Network.DeviceType.Router, 500f, 500f),
                        (Network.DeviceType.PC, 500f, 250f)
                    },
                    Links = new List<(int from, int to)> { (0, 1) },
                    IPConfigs = new List<(int deviceIdx, string ip, string mask)>
                    {
                        (0, "192.168.1.1", "255.0.0.0"),
                        (1, "192.168.1.10", "255.255.255.0")
                    },
                    FaultType = "mask",
                    FaultDeviceIndex = 0,
                    CorrectIP = "192.168.1.1",
                    CorrectMask = "255.255.255.0"
                },

                // ─── Escenario 4: PC sin IP ───
                // Fallo: PC no tiene IP configurada
                // Arreglo: Click en PC -> IPConfig -> escribir IP -> APLICAR
                new FindFaultScenario
                {
                    Name = "PC sin IP",
                    Description = "El PC no tiene direcci\u00f3n IP configurada.\n" +
                                  "No puede comunicarse con el Router.\n\n" +
                                  "\u25b6 Soluci\u00f3n: Click en PC \u2192 IPConfig\n" +
                                  "\u2192 escribir IP 192.168.1.10 y M\u00e1scara 255.255.255.0\n" +
                                  "\u2192 APLICAR",
                    Hint = "Haz click en el PC, luego en IPConfig,\n" +
                           "escribe 192.168.1.10 y m\u00e1scara 255.255.255.0,\n" +
                           "presiona APLICAR.",
                    DiscLabel = "VERIFICAR",
                    Devices = new List<(Network.DeviceType type, float x, float y)>
                    {
                        (Network.DeviceType.Router, 500f, 500f),
                        (Network.DeviceType.PC, 500f, 250f)
                    },
                    Links = new List<(int from, int to)> { (0, 1) },
                    IPConfigs = new List<(int deviceIdx, string ip, string mask)>
                    {
                        (0, "192.168.1.1", "255.255.255.0"),
                        (1, "", "")
                    },
                    FaultType = "gateway",
                    FaultDeviceIndex = 1,
                    CorrectIP = "192.168.1.10",
                    CorrectMask = "255.255.255.0"
                }
            };
        }

        /// <summary>Construye la topologia, aplica el fallo y actualiza la UI para el escenario dado.</summary>
        /// <param name="index">Indice del escenario (0-3).</param>
        public void LoadScenario(int index)
        {
            if (topologyManager == null)
                topologyManager = Object.FindAnyObjectByType<TopologyManager>();
            if (topologyManager == null)
            {
                Debug.LogError("[FindFault] No TopologyManager disponible para cargar escenario");
                return;
            }
            if (index < 0 || index >= scenarios.Count) return;

            // Limpiar discos tactiles del escenario anterior para evitar estado fantasma
            var discManager = Object.FindAnyObjectByType<TangibleDiscManager>();
            if (discManager != null) discManager.ClearAllDiscs();

            topologyManager.ClearTopology();
            currentIndex = index;
            var scenario = scenarios[index];
            scenario.IsSolved = false;
            affectedNode = null;
            affectedLink = null;

            // 1. Crear dispositivos
            for (int i = 0; i < scenario.Devices.Count; i++)
            {
                var dev = scenario.Devices[i];
                int discId = i + 1;
                topologyManager.AddNode(discId, dev.type, new Vector2(dev.x, dev.y));
            }

            // 2. Crear enlaces
            foreach (var linkDef in scenario.Links)
            {
                int srcDiscId = linkDef.from + 1;
                int dstDiscId = linkDef.to + 1;
                topologyManager.AddLink(srcDiscId, dstDiscId);
            }

            // 3. Configurar IPs
            foreach (var ipDef in scenario.IPConfigs)
            {
                var node = topologyManager.GetNode(ipDef.deviceIdx + 1);
                if (node != null)
                {
                    node.IpAddress = ipDef.ip;
                    node.SubnetMask = ipDef.mask;
                }
            }

            // 4. Identificar nodo/enlace afectado
            foreach (var linkDef in scenario.Links)
            {
                int srcDiscId = linkDef.from + 1;
                int dstDiscId = linkDef.to + 1;
                var link = FindLink(srcDiscId, dstDiscId);
                if (link != null)
                {
                    affectedLink = link;
                    break;
                }
            }
            affectedNode = topologyManager.GetNode(scenario.FaultDeviceIndex + 1);

            // 5. Aplicar fallo
            ApplyFault(scenario);

            // 6. Actualizar UI
            UpdateUI();

            // 7. Actualizar panel de dispositivos
            var devicePanel = Object.FindAnyObjectByType<DevicePanelController>();
            if (devicePanel != null) devicePanel.RefreshDevicesPanel();
        }

        /// <summary>Aplica el fallo activo a la topologia segun el tipo.</summary>
        private void ApplyFault(FindFaultScenario scenario)
        {
            // Escenario "cable": no se crea enlace (Links vacio en InitializeScenarios).
            // El usuario debe usar CONECTAR en el panel para crearlo manualmente.
            switch (scenario.FaultType)
            {
                case "cable":
                    // No hay enlace que fallar — el usuario lo crea con CONECTAR
                    break;
                case "ip":
                    // La IP ya se configuro con el valor incorrecto en IPConfigs (192.168.100.99)
                    break;
                case "mask":
                    // La mascara ya se configuro con el valor incorrecto en IPConfigs (255.0.0.0)
                    break;
                case "gateway":
                    // El PC ya se configuro sin IP (vacio en IPConfigs)
                    break;
            }
        }

        /// <summary>
        /// Valida si el estado actual de la red corresponde a un fallo reparado.
        /// NO repara — solo verifica el estado real del dispositivo/enlace.
        /// El usuario debe arreglar el problema usando los paneles UI (IPConfig, CONECTAR, etc.)
        /// y luego presionar VERIFICAR.
        /// </summary>
        private bool ValidateSolution(FindFaultScenario scenario)
        {
            switch (scenario.FaultType)
            {
                case "cable":
                    // Verificar que existe un enlace entre los dos routers (discId=1 y discId=2)
                    if (topologyManager == null) return false;
                    var link = FindLink(1, 2);
                    return link != null && link.IsFunctional();
                case "ip":
                    // Verificar que el Router tiene la IP correcta
                    return affectedNode != null &&
                           IPValidation.IsValidIP(affectedNode.IpAddress) &&
                           affectedNode.IpAddress == scenario.CorrectIP;
                case "mask":
                    // Verificar que el Router tiene la mascara correcta
                    return affectedNode != null &&
                           IPValidation.IsValidSubnetMask(affectedNode.SubnetMask) &&
                           affectedNode.SubnetMask == scenario.CorrectMask;
                case "gateway":
                    // Verificar que el PC tiene una IP valida y coincide con la correcta
                    return affectedNode != null &&
                           IPValidation.IsValidIP(affectedNode.IpAddress) &&
                           affectedNode.IpAddress == scenario.CorrectIP;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Busca un enlace entre dos discos. Metodo auxiliar porque
        /// TopologyManager.AddLink no devuelve el NetworkLink creado.
        /// </summary>
        /// <param name="discIdA">DiscId del primer extremo del enlace.</param>
        /// <param name="discIdB">DiscId del segundo extremo del enlace.</param>
        /// <returns>El NetworkLink encontrado, o null si no existe.</returns>
        private NetworkLink FindLink(int discIdA, int discIdB)
        {
            if (topologyManager == null) return null;
            var links = topologyManager.GetAllLinks();
            foreach (var link in links)
            {
                if ((link.SourceNode.DiscId == discIdA && link.DestinationNode.DiscId == discIdB) ||
                    (link.SourceNode.DiscId == discIdB && link.DestinationNode.DiscId == discIdA))
                {
                    return link;
                }
            }
            return null;
        }

        // ─── Metodos llamados desde la UI ───

        /// <summary>Callback del boton disco tactil. Repara el fallo si es el disco correcto.</summary>
        public void OnDiscButtonClicked()
        {
            if (currentIndex < 0 || currentIndex >= scenarios.Count) return;
            var scenario = scenarios[currentIndex];
            if (scenario.IsSolved) return;

            // Solo verificar — NO reparar. El usuario debe arreglar el problema
            // a traves de los paneles UI (IPConfig, CONECTAR, etc.)
            if (ValidateSolution(scenario))
            {
                scenario.IsSolved = true;
                if (resultText != null)
                {
                    resultText.text = "\u2705 \u00a1Correcto! El fallo ha sido reparado.";
                    resultText.color = Color.green;
                }
            }
            else
            {
                if (resultText != null)
                {
                    resultText.text = "\u274c El fallo contin\u00faa. Revisa la pista e intenta de nuevo.";
                    resultText.color = Color.red;
                }
            }
        }

        /// <summary>Avanza al siguiente escenario.</summary>
        public void OnNextClicked()
        {
            if (currentIndex < scenarios.Count - 1)
            {
                LoadScenario(currentIndex + 1);
            }
            else
            {
                // Todos completados
                if (scenarioTitleText != null)
                    scenarioTitleText.text = "\u00a1Has completado todos los escenarios!";
                if (faultDescriptionText != null)
                    faultDescriptionText.text = "Excelente trabajo identificando y reparando fallos de red.\n\n" +
                                                "Presiona VOLVER para intentar de nuevo o elige otra actividad.";
                if (hintText != null) hintText.text = "";
                if (resultText != null) resultText.text = "";
                if (discButton != null) discButton.gameObject.SetActive(false);
                if (nextButton != null) nextButton.gameObject.SetActive(false);
                if (prevButton != null) prevButton.gameObject.SetActive(false);
                if (statusText != null) statusText.text = "Completado";
            }
        }

        /// <summary>Vuelve al escenario anterior.</summary>
        public void OnPrevClicked()
        {
            if (currentIndex > 0)
                LoadScenario(currentIndex - 1);
        }

        /// <summary>Actualiza todos los textos UI con el estado del escenario actual.</summary>
        private void UpdateUI()
        {
            if (currentIndex < 0 || currentIndex >= scenarios.Count) return;
            var scenario = scenarios[currentIndex];

            if (scenarioTitleText != null)
                scenarioTitleText.text = $"Escenario {currentIndex + 1}/{scenarios.Count}: {scenario.Name}";

            if (faultDescriptionText != null)
                faultDescriptionText.text = scenario.Description;

            if (hintText != null)
                hintText.text = $"\u2139\ufe0f Pista: {scenario.Hint}";

            if (resultText != null)
                resultText.text = "";

            if (discButtonText != null)
                discButtonText.text = scenario.DiscLabel;

            if (discButton != null)
                discButton.gameObject.SetActive(true);

            if (nextButton != null)
            {
                bool isLast = currentIndex >= scenarios.Count - 1;
                if (nextButtonText != null)
                    nextButtonText.text = isLast ? "FINALIZAR" : "SIGUIENTE \u25b6";
                nextButton.gameObject.SetActive(true);
            }

            if (prevButton != null)
                prevButton.gameObject.SetActive(currentIndex > 0);

            if (statusText != null)
                statusText.text = $"Modo: Encuentra el Fallo | Escenario {currentIndex + 1}/{scenarios.Count}";
        }

        /// <summary>Reinicia la actividad desde el primer escenario.</summary>
        public void Restart()
        {
            LoadScenario(0);
        }
    }
}
