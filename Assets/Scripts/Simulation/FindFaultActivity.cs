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
        public Text faultDescriptionText;
        public Text hintText;
        public Text resultText;
        public Button solveButton;
        public Text statusText;

        private TopologyManager topologyManager;
        private FaultScenario currentFault;
        private List<FaultScenario> availableFaults;
        private NetworkNode affectedNode;
        private NetworkLink affectedLink;

        private void Start()
        {
            topologyManager = Object.FindAnyObjectByType<TopologyManager>();
            if (topologyManager == null)
            {
                UnityEngine.Debug.LogError("[FindFault] No TopologyManager disponible");
                return;
            }

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
            if (topologyManager == null) return;
            var nodes = topologyManager.GetAllNodes();
            if (nodes.Count < 2) return;

            affectedNode = null;
            affectedLink = null;

            switch (faultName)
            {
                case "Cable Desconectado":
                    var links = topologyManager.GetAllLinks();
                    if (links.Count > 0)
                    {
                        affectedLink = links[0];
                        affectedLink.SetFault("cable_desconectado");
                    }
                    break;

                case "Interfaz Down":
                    if (nodes.Exists(n => n.Type == SimRedes.Network.DeviceType.Router))
                    {
                        affectedNode = nodes.Find(n => n.Type == SimRedes.Network.DeviceType.Router);
                        affectedNode.IsAdminDown = true;
                    }
                    break;

                case "IP Incorrecta":
                case "Máscara de Subred Incorrecta":
                    if (nodes.Exists(n => n.Type == SimRedes.Network.DeviceType.Router))
                    {
                        affectedNode = nodes.Find(n => n.Type == SimRedes.Network.DeviceType.Router);
                        affectedNode.SetInterfaceIP("G0/0", "192.168.100.99", "255.255.255.0");
                    }
                    break;

                case "Default Gateway Faltante":
                    if (nodes.Exists(n => n.Type == SimRedes.Network.DeviceType.PC))
                    {
                        affectedNode = nodes.Find(n => n.Type == SimRedes.Network.DeviceType.PC);
                        affectedNode.IpAddress = "";
                        affectedNode.SubnetMask = "";
                    }
                    break;
            }

            UnityEngine.Debug.Log($"[FindFault] Fallo aplicado: {faultName}");
        }

        public void OnSolveClicked()
        {
            if (currentFault == null) return;

            if (ValidateSolution())
            {
                currentFault.IsSolved = true;
                if (resultText != null)
                {
                    resultText.text = "✅ ¡CORRECTO! El fallo ha sido resuelto";
                    resultText.color = Color.green;
                }

                FixFault(currentFault.Name);

                UnityEngine.Debug.Log("[FindFault] Fallo resuelto correctamente");
            }
            else
            {
                if (resultText != null)
                {
                    resultText.text = "❌ Incorrecto. Intenta de nuevo.";
                    resultText.color = Color.red;
                }
            }

            Invoke(nameof(GenerateNewFault), 3f);
        }

        private void FixFault(string faultName)
        {
            if (topologyManager == null) return;

            switch (faultName)
            {
                case "Cable Desconectado":
                    if (affectedLink != null)
                    {
                        affectedLink.SetFault("");
                    }
                    break;

                case "Interfaz Down":
                    if (affectedNode != null)
                    {
                        affectedNode.IsAdminDown = false;
                    }
                    break;

                case "IP Incorrecta":
                case "Máscara de Subred Incorrecta":
                    if (affectedNode != null && affectedNode.Type == SimRedes.Network.DeviceType.Router)
                    {
                        affectedNode.SetInterfaceIP("G0/0", "192.168.1.1", "255.255.255.0");
                    }
                    break;

                case "Default Gateway Faltante":
                    if (affectedNode != null && affectedNode.Type == SimRedes.Network.DeviceType.PC)
                    {
                        affectedNode.IpAddress = "192.168.1.10";
                        affectedNode.SubnetMask = "255.255.255.0";
                    }
                    break;
            }

            UnityEngine.Debug.Log($"[FindFault] Fallo reparado: {faultName}");
        }

        private bool ValidateSolution()
        {
            if (topologyManager == null) return false;

            switch (currentFault.Name)
            {
                case "Cable Desconectado":
                    return affectedLink != null && affectedLink.IsFunctional();

                case "Interfaz Down":
                    return affectedNode == null || !affectedNode.IsAdminDown;

                case "IP Incorrecta":
                case "Máscara de Subred Incorrecta":
                    return affectedNode != null &&
                           IPValidation.IsValidIP(affectedNode.IpAddress) &&
                           IPValidation.IsValidSubnetMask(affectedNode.SubnetMask);

                case "Default Gateway Faltante":
                    return affectedNode != null &&
                           IPValidation.IsValidIP(affectedNode.IpAddress) &&
                           IPValidation.IsValidSubnetMask(affectedNode.SubnetMask);

                default:
                    return true;
            }
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