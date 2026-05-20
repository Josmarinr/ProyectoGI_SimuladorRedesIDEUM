using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using SimRedes.Network;

namespace SimRedes.Simulation
{
    public class FaultScenario
    {
        public string Name;
        public string Description;
        public string Solution;
        public bool IsSolved;
    }

    public class FindFaultActivity : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Text faultDescriptionText;
        [SerializeField] private Text hintText;
        [SerializeField] private Text resultText;
        [SerializeField] private Button solveButton;
        [SerializeField] private Text statusText;

        [Header("Settings")]
        [SerializeField] private bool enableRandomFaults = true;

        private TopologyManager topologyManager;
        private FaultScenario currentFault;
        private List<FaultScenario> availableFaults;

        private void Start()
        {
            topologyManager = FindObjectOfType<TopologyManager>();
            InitializeFaults();

            if (solveButton != null)
            {
                solveButton.onClick.AddListener(OnSolveClicked);
            }

            GenerateRandomFault();
        }

        private void InitializeFaults()
        {
            availableFaults = new List<FaultScenario>
            {
                new FaultScenario
                {
                    Name = "Cable Desconectado",
                    Description = "El cable entre dos dispositivos está desconectado físicamente",
                    Solution = "Colocar un disco de Enlace entre los dispositivos"
                },
                new FaultScenario
                {
                    Name = "IP Incorrecta",
                    Description = "Uno de los dispositivos tiene una IP mal configurada",
                    Solution = "Verificar que las IPs estén en la misma subred"
                },
                new FaultScenario
                {
                    Name = "Máscara de Subred Incorrecta",
                    Description = "La máscara de subred no permite comunicación",
                    Solution = "Ajustar la máscara para el rango correcto"
                },
                new FaultScenario
                {
                    Name = "Interfaz Down",
                    Description = "Una interfaz del router está administrativamente apagada",
                    Solution = "Activar la interfaz con 'no shutdown'"
                },
                new FaultScenario
                {
                    Name = "Default Gateway Faltante",
                    Description = "El PC no tiene gateway configurado para salir de su red",
                    Solution = "Configurar el gateway correcto"
                }
            };
        }

        private void GenerateRandomFault()
        {
            if (availableFaults.Count == 0) return;

            int randomIndex = Random.Range(0, availableFaults.Count);
            currentFault = availableFaults[randomIndex];
            currentFault.IsSolved = false;

            ApplyFault(currentFault.Name);
            UpdateUI();
        }

        private void ApplyFault(string faultName)
        {
            var nodes = topologyManager.GetAllNodes();
            if (nodes.Count < 2) return;

            switch (faultName)
            {
                case "Cable Desconectado":
                    var links = topologyManager.GetAllLinks();
                    if (links.Count > 0)
                    {
                        links[0].SetFault("cable_desconectado");
                    }
                    break;

                case "Interfaz Down":
                    if (nodes.Exists(n => n.Type == SimRedes.Network.DeviceType.Router))
                    {
                        var router = nodes.Find(n => n.Type == SimRedes.Network.DeviceType.Router);
                        router.IsAdminDown = true;
                    }
                    break;

                case "IP Incorrecta":
                case "Máscara de Subred Incorrecta":
                    if (nodes.Exists(n => n.Type == SimRedes.Network.DeviceType.Router))
                    {
                        var router = nodes.Find(n => n.Type == SimRedes.Network.DeviceType.Router);
                        router.SetInterfaceIP("G0/0", "192.168.1.999", "255.255.255.0");
                    }
                    break;

                case "Default Gateway Faltante":
                    if (nodes.Exists(n => n.Type == SimRedes.Network.DeviceType.PC))
                    {
                        var pc = nodes.Find(n => n.Type == SimRedes.Network.DeviceType.PC);
                        pc.IpAddress = "";
                        pc.SubnetMask = "";
                    }
                    break;
            }

            UnityEngine.Debug.Log($"[FindFault] Fallo aplicado: {faultName}");
        }

        private void OnSolveClicked()
        {
            if (currentFault == null) return;

            if (ValidateSolution())
            {
                currentFault.IsSolved = true;
                resultText.text = "✅ ¡CORRECTO! El fallo ha sido resuelto";
                resultText.color = Color.green;

                FixFault(currentFault.Name);

                UnityEngine.Debug.Log("[FindFault] Fallo resuelto correctamente");
            }
            else
            {
                resultText.text = "❌ Incorrecto. Intenta de nuevo.";
                resultText.color = Color.red;
            }

            Invoke(nameof(GenerateNewFault), 3f);
        }

        private void FixFault(string faultName)
        {
            var nodes = topologyManager.GetAllNodes();
            if (nodes.Count < 2) return;

            switch (faultName)
            {
                case "Cable Desconectado":
                    var links = topologyManager.GetAllLinks();
                    foreach (var l in links)
                    {
                        l.SetFault("");
                    }
                    break;

                case "Interfaz Down":
                    foreach (var n in nodes)
                    {
                        n.IsAdminDown = false;
                    }
                    break;

                case "IP Incorrecta":
                case "Máscara de Subred Incorrecta":
                    foreach (var n in nodes)
                    {
                        if (n.Type == SimRedes.Network.DeviceType.Router)
                        {
                            n.SetInterfaceIP("G0/0", "192.168.1.1", "255.255.255.0");
                        }
                    }
                    break;

                case "Default Gateway Faltante":
                    foreach (var n in nodes)
                    {
                        if (n.Type == SimRedes.Network.DeviceType.PC)
                        {
                            n.IpAddress = "192.168.1.10";
                            n.SubnetMask = "255.255.255.0";
                        }
                    }
                    break;
            }

            UnityEngine.Debug.Log($"[FindFault] Fallo reparado: {faultName}");
        }

        private bool ValidateSolution()
        {
            var nodes = topologyManager.GetAllNodes();
            var links = topologyManager.GetAllLinks();

            if (currentFault.Name == "Cable Desconectado")
            {
                return links.Count > 0 && links.All(l => l.IsFunctional());
            }

            if (currentFault.Name == "Interfaz Down")
            {
                return nodes.All(n => !n.IsAdminDown || n.Type != SimRedes.Network.DeviceType.Router);
            }

            if (currentFault.Name == "Default Gateway Faltante")
            {
                return nodes.Exists(n => n.Type == SimRedes.Network.DeviceType.PC &&
                    IPValidation.IsValidIP(n.IpAddress) &&
                    IPValidation.IsValidSubnetMask(n.SubnetMask));
            }

            return true;
        }

        private void GenerateNewFault()
        {
            GenerateRandomFault();
        }

        private void UpdateUI()
        {
            if (faultDescriptionText != null && currentFault != null)
            {
                faultDescriptionText.text = $"Fallo: {currentFault.Name}\n\n{currentFault.Description}";
            }

            if (hintText != null && currentFault != null)
            {
                hintText.text = $"Pista: {currentFault.Solution}";
            }

            if (statusText != null)
            {
                statusText.text = "Modo: Encontrar Fallos";
            }

            if (resultText != null)
            {
                resultText.text = "";
            }
        }

        public void GenerateNewScenario()
        {
            GenerateRandomFault();
        }
    }
}