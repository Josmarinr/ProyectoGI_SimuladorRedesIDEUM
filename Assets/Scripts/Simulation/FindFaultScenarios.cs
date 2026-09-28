using System.Collections.Generic;

namespace SimRedes.Simulation
{
    /// <summary>
    /// Catalogo de datos de los escenarios de la actividad "Encuentra el Fallo".
    /// Contiene unicamente los datos puros (topologia, configuracion IP y fallo)
    /// de los 4 escenarios predefinidos; la logica de carga vive en
    /// <see cref="FindFaultActivity"/>.
    /// </summary>
    public static class FindFaultScenarios
    {
        /// <summary>
        /// Construye los 4 escenarios predefinidos de la actividad.
        /// Los datos son identicos a los originales de FindFaultActivity.InitializeScenarios.
        /// </summary>
        /// <returns>Nueva lista con los escenarios en orden (Cable, IP, Mascara, PC sin IP).</returns>
        public static List<FindFaultScenario> Create()
        {
            return new List<FindFaultScenario>
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
    }
}
