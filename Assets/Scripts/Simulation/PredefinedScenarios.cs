using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimRedes.Simulation
{
    public class PredefinedScenarios : MonoBehaviour
    {
        public static PredefinedScenarios Instance { get; private set; }

        public enum ScenarioDifficulty { Basico, Intermedio, Avanzado }

        [System.Serializable]
        public class NetworkScenario
        {
            public string name;
            public string description;
            public ScenarioDifficulty difficulty;
            public List<DeviceConfig> devices;
            public List<LinkConfig> links;
            public List<IPConfig> ipConfigurations;
            public List<FaultConfig> faults;
            public string[] objectives;
            public string[] hints;
        }

        [System.Serializable]
        public class FaultConfig
        {
            public int deviceIndex;
            public string faultType;
            public string ip;
            public string mask;
        }

        [System.Serializable]
        public class DeviceConfig
        {
            public string type;
            public float x, y;
        }

        [System.Serializable]
        public class LinkConfig
        {
            public int fromIndex, toIndex;
        }

        [System.Serializable]
        public class IPConfig
        {
            public int deviceIndex;
            public string ip;
            public string mask;
        }

        private List<NetworkScenario> scenarios = new List<NetworkScenario>();

        private void Awake()
        {
            Instance = this;
            UnityEngine.Debug.Log("[PredefinedScenarios] Awake - Antes de InitializeScenarios");
            InitializeScenarios();
            UnityEngine.Debug.Log($"[PredefinedScenarios] Awake - Despues: {scenarios.Count} escenarios");
        }

        private void InitializeScenarios()
        {
            UnityEngine.Debug.Log("[PredefinedScenarios] Iniciando inicializacion de escenarios");

            scenarios.Add(new NetworkScenario
            {
                name = "Estrella Simple",
                description = "Un router central conectado a tres PCs. Configura las IPs para que los PCs puedan comunicarse.",
                difficulty = ScenarioDifficulty.Basico,
                devices = new List<DeviceConfig>
                {
                    new DeviceConfig { type = "Router", x = 0.5f, y = 0.5f },
                    new DeviceConfig { type = "PC", x = 0.25f, y = 0.25f },
                    new DeviceConfig { type = "PC", x = 0.75f, y = 0.25f },
                    new DeviceConfig { type = "PC", x = 0.5f, y = 0.8f }
                },
                links = new List<LinkConfig>
                {
                    new LinkConfig { fromIndex = 0, toIndex = 1 },
                    new LinkConfig { fromIndex = 0, toIndex = 2 },
                    new LinkConfig { fromIndex = 0, toIndex = 3 }
                },
                ipConfigurations = new List<IPConfig>
                {
                    new IPConfig { deviceIndex = 0, ip = "192.168.1.1", mask = "255.255.255.0" },
                    new IPConfig { deviceIndex = 1, ip = "192.168.1.10", mask = "255.255.255.0" },
                    new IPConfig { deviceIndex = 2, ip = "192.168.1.20", mask = "255.255.255.0" },
                    new IPConfig { deviceIndex = 3, ip = "192.168.1.30", mask = "255.255.255.0" }
                },
                objectives = new string[]
                {
                    "Configurar IPs en todos los dispositivos",
                    "Verificar conectividad entre PCs usando Ping",
                    "Revisar la tabla ARP del router"
                },
                hints = new string[]
                {
                    "Todos los PCs deben estar en la misma subred 192.168.1.0/24",
                    "La puerta de enlace de los PCs es 192.168.1.1"
                },
                faults = new List<FaultConfig>()
            });

            scenarios.Add(new NetworkScenario
            {
                name = "Dos Routers",
                description = "Dos routers conectados entre si, cada uno con PCs. Configura enrutamiento estatico.",
                difficulty = ScenarioDifficulty.Intermedio,
                devices = new List<DeviceConfig>
                {
                    new DeviceConfig { type = "Router", x = 0.3f, y = 0.5f },
                    new DeviceConfig { type = "Router", x = 0.7f, y = 0.5f },
                    new DeviceConfig { type = "PC", x = 0.1f, y = 0.25f },
                    new DeviceConfig { type = "PC", x = 0.1f, y = 0.75f },
                    new DeviceConfig { type = "PC", x = 0.9f, y = 0.25f },
                    new DeviceConfig { type = "PC", x = 0.9f, y = 0.75f }
                },
                links = new List<LinkConfig>
                {
                    new LinkConfig { fromIndex = 0, toIndex = 1 },
                    new LinkConfig { fromIndex = 0, toIndex = 2 },
                    new LinkConfig { fromIndex = 0, toIndex = 3 },
                    new LinkConfig { fromIndex = 1, toIndex = 4 },
                    new LinkConfig { fromIndex = 1, toIndex = 5 }
                },
                ipConfigurations = new List<IPConfig>
                {
                    new IPConfig { deviceIndex = 0, ip = "10.0.0.1", mask = "255.255.255.252" },
                    new IPConfig { deviceIndex = 1, ip = "10.0.0.2", mask = "255.255.255.252" },
                    new IPConfig { deviceIndex = 2, ip = "192.168.1.10", mask = "255.255.255.0" },
                    new IPConfig { deviceIndex = 3, ip = "192.168.1.20", mask = "255.255.255.0" },
                    new IPConfig { deviceIndex = 4, ip = "192.168.2.10", mask = "255.255.255.0" },
                    new IPConfig { deviceIndex = 5, ip = "192.168.2.20", mask = "255.255.255.0" }
                },
                objectives = new string[]
                {
                    "Configurar IPs en todos los dispositivos",
                    "Agregar rutas estaticas en ambos routers",
                    "Verificar ping entre PCs de diferentes redes"
                },
                hints = new string[]
                {
                    "Router 1 necesita ruta hacia 192.168.2.0/24 via 10.0.0.2",
                    "Router 2 necesita ruta hacia 192.168.1.0/24 via 10.0.0.1"
                },
                faults = new List<FaultConfig>()
            });

            scenarios.Add(new NetworkScenario
            {
                name = "Topologia en Anillo",
                description = "Tres routers en anillo con switches. Simula enrutamiento dinamico RIP.",
                difficulty = ScenarioDifficulty.Intermedio,
                devices = new List<DeviceConfig>
                {
                    new DeviceConfig { type = "Router", x = 0.5f, y = 0.2f },
                    new DeviceConfig { type = "Router", x = 0.2f, y = 0.7f },
                    new DeviceConfig { type = "Router", x = 0.8f, y = 0.7f },
                    new DeviceConfig { type = "Switch", x = 0.5f, y = 0.45f },
                    new DeviceConfig { type = "PC", x = 0.35f, y = 0.35f },
                    new DeviceConfig { type = "PC", x = 0.65f, y = 0.35f }
                },
                links = new List<LinkConfig>
                {
                    new LinkConfig { fromIndex = 0, toIndex = 1 },
                    new LinkConfig { fromIndex = 1, toIndex = 2 },
                    new LinkConfig { fromIndex = 2, toIndex = 0 },
                    new LinkConfig { fromIndex = 0, toIndex = 3 },
                    new LinkConfig { fromIndex = 3, toIndex = 4 },
                    new LinkConfig { fromIndex = 3, toIndex = 5 }
                },
                ipConfigurations = new List<IPConfig>
                {
                    new IPConfig { deviceIndex = 0, ip = "172.16.0.1", mask = "255.255.255.0" },
                    new IPConfig { deviceIndex = 1, ip = "172.16.1.1", mask = "255.255.255.0" },
                    new IPConfig { deviceIndex = 2, ip = "172.16.2.1", mask = "255.255.255.0" },
                    new IPConfig { deviceIndex = 4, ip = "172.16.0.10", mask = "255.255.255.0" },
                    new IPConfig { deviceIndex = 5, ip = "172.16.0.20", mask = "255.255.255.0" }
                },
                objectives = new string[]
                {
                    "Configurar todas las IPs",
                    "Iniciar protocolo RIP en los 3 routers",
                    "Verificar convergencia y tablas de enrutamiento"
                },
                hints = new string[]
                {
                    "Activa RIP en cada router antes de iniciar el protocolo",
                    "Verifica que todas las redes aparezcan en las tablas"
                },
                faults = new List<FaultConfig>()
            });

            scenarios.Add(new NetworkScenario
            {
                name = "Red en Arbol",
                description = "Jerarquia de routers y switches. Practica enrutamiento estatico y dinamico.",
                difficulty = ScenarioDifficulty.Avanzado,
                devices = new List<DeviceConfig>
                {
                    new DeviceConfig { type = "Router", x = 0.5f, y = 0.15f },
                    new DeviceConfig { type = "Router", x = 0.25f, y = 0.45f },
                    new DeviceConfig { type = "Router", x = 0.75f, y = 0.45f },
                    new DeviceConfig { type = "Switch", x = 0.15f, y = 0.7f },
                    new DeviceConfig { type = "Switch", x = 0.35f, y = 0.7f },
                    new DeviceConfig { type = "Switch", x = 0.65f, y = 0.7f },
                    new DeviceConfig { type = "Switch", x = 0.85f, y = 0.7f },
                    new DeviceConfig { type = "PC", x = 0.05f, y = 0.9f },
                    new DeviceConfig { type = "PC", x = 0.25f, y = 0.9f },
                    new DeviceConfig { type = "PC", x = 0.55f, y = 0.9f },
                    new DeviceConfig { type = "PC", x = 0.75f, y = 0.9f }
                },
                links = new List<LinkConfig>
                {
                    new LinkConfig { fromIndex = 0, toIndex = 1 },
                    new LinkConfig { fromIndex = 0, toIndex = 2 },
                    new LinkConfig { fromIndex = 1, toIndex = 3 },
                    new LinkConfig { fromIndex = 1, toIndex = 4 },
                    new LinkConfig { fromIndex = 2, toIndex = 5 },
                    new LinkConfig { fromIndex = 2, toIndex = 6 },
                    new LinkConfig { fromIndex = 3, toIndex = 7 },
                    new LinkConfig { fromIndex = 4, toIndex = 8 },
                    new LinkConfig { fromIndex = 5, toIndex = 9 },
                    new LinkConfig { fromIndex = 6, toIndex = 10 }
                },
                ipConfigurations = new List<IPConfig>
                {
                    new IPConfig { deviceIndex = 0, ip = "10.0.0.1", mask = "255.255.255.0" },
                    new IPConfig { deviceIndex = 1, ip = "10.0.1.1", mask = "255.255.255.0" },
                    new IPConfig { deviceIndex = 2, ip = "10.0.2.1", mask = "255.255.255.0" },
                    new IPConfig { deviceIndex = 7, ip = "192.168.1.10", mask = "255.255.255.0" },
                    new IPConfig { deviceIndex = 8, ip = "192.168.1.20", mask = "255.255.255.0" },
                    new IPConfig { deviceIndex = 9, ip = "192.168.2.10", mask = "255.255.255.0" },
                    new IPConfig { deviceIndex = 10, ip = "192.168.2.20", mask = "255.255.255.0" }
                },
                objectives = new string[]
                {
                    "Configurar IPs en todos los niveles",
                    "Agregar rutas estaticas o iniciar OSPF",
                    "Verificar conectividad de extremo a extremo"
                },
                hints = new string[]
                {
                    "Cada subred necesita una ruta en el router padre",
                    "OSPF automatizara las rutas si lo prefieres"
                },
                faults = new List<FaultConfig>()
            });

            scenarios.Add(new NetworkScenario
            {
                name = "Detectar Fallos",
                description = "Topologia con fallos incorporados. Encuentra y soluciona los problemas.",
                difficulty = ScenarioDifficulty.Intermedio,
                devices = new List<DeviceConfig>
                {
                    new DeviceConfig { type = "Router", x = 0.3f, y = 0.5f },
                    new DeviceConfig { type = "Router", x = 0.7f, y = 0.5f },
                    new DeviceConfig { type = "Switch", x = 0.5f, y = 0.5f },
                    new DeviceConfig { type = "PC", x = 0.1f, y = 0.8f },
                    new DeviceConfig { type = "PC", x = 0.9f, y = 0.8f }
                },
                links = new List<LinkConfig>
                {
                    new LinkConfig { fromIndex = 0, toIndex = 2 },
                    new LinkConfig { fromIndex = 1, toIndex = 2 },
                    new LinkConfig { fromIndex = 0, toIndex = 3 },
                    new LinkConfig { fromIndex = 1, toIndex = 4 }
                },
                ipConfigurations = new List<IPConfig>
                {
                    new IPConfig { deviceIndex = 0, ip = "192.168.0.1", mask = "255.255.255.0" },
                    new IPConfig { deviceIndex = 1, ip = "192.168.0.2", mask = "255.255.255.0" },
                    new IPConfig { deviceIndex = 3, ip = "192.168.1.10", mask = "255.255.255.0" },
                    new IPConfig { deviceIndex = 4, ip = "192.168.2.20", mask = "255.255.255.0" }
                },
                objectives = new string[]
                {
                    "Usar ping para identificar donde esta el fallo",
                    "Revisar configuraciones de IP y mascaras",
                    "Corregir los problemas encontrados"
                },
                hints = new string[]
                {
                    "Comienza verificando la conectividad basica con ping",
                    "Revisa que las IPs esten en subredes correctas",
                    "Verifica que los enlaces no esten caidos"
                },
                faults = new List<FaultConfig>
                {
                    new FaultConfig { deviceIndex = 0, faultType = "badmask", ip = "192.168.0.1", mask = "255.255.0.0" },
                    new FaultConfig { deviceIndex = 4, faultType = "badip", ip = "192.168.2.20", mask = "255.255.255.0" }
                }
            });
        }

        public List<NetworkScenario> GetScenarios()
        {
            UnityEngine.Debug.Log($"[PredefinedScenarios] GetScenarios: {scenarios.Count} escenarios");
            return new List<NetworkScenario>(scenarios);
        }

        public List<NetworkScenario> GetScenariosByDifficulty(ScenarioDifficulty difficulty)
        {
            var result = new List<NetworkScenario>();
            foreach (var s in scenarios)
            {
                if (s.difficulty == difficulty)
                    result.Add(s);
            }
            return result;
        }

        public NetworkScenario GetScenario(int index)
        {
            if (index >= 0 && index < scenarios.Count)
            {
                UnityEngine.Debug.Log($"[PredefinedScenarios] Obteniendo escenario {index}: {scenarios[index].name}");
                return scenarios[index];
            }
            UnityEngine.Debug.LogWarning($"[PredefinedScenarios] Indice {index} fuera de rango (0-{scenarios.Count - 1})");
            return null;
        }

        public int GetScenarioCount() => scenarios.Count;
    }
}