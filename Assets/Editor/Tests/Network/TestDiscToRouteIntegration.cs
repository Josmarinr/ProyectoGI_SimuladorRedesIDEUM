using NUnit.Framework;
using SimRedes.Network;
using SimRedes.Tangible;
using SimRedes.Simulation;
using UnityEngine;

namespace Tests.EditMode.Network
{
    /// <summary>
    /// Tests de integración para el flujo Disco → Tabla de Enrutamiento → UI.
    ///
    /// Flujo: DiscEventHandler.HandleRoutingConfigDisc()
    ///      → RouteBuilderState (4 campos: RedDestino, Mascara, ProximoSalto, InterfazSalida)
    ///      → ApplyToRouter() → router.RoutingTable
    ///      → RoutingSimulator.GetRoutingTableSummary() (puente a UI)
    ///
    /// Estos tests NO dependen de componentes UI (Text, Canvas) — prueban
    /// la tubería de datos que alimenta la UI.
    /// </summary>
    public class TestDiscToRouteIntegration
    {
        /// <summary>
        /// Crea un TopologyManager con un router en la posicion cero.
        /// </summary>
        private TopologyManager CreateTopologyWithRouter(int discId = 1)
        {
            var go = new GameObject("TestTopology");
            var topology = go.AddComponent<TopologyManager>();
            topology.AddNode(discId, SimRedes.Network.DeviceType.Router, Vector2.zero);
            return topology;
        }

        /// <summary>
        /// Crea un TopologyManager con dos routers en posiciones distintas.
        /// </summary>
        private TopologyManager CreateTopologyWithTwoRouters(int discId1 = 1, int discId2 = 2)
        {
            var go = new GameObject("TestTopology");
            var topology = go.AddComponent<TopologyManager>();
            topology.AddNode(discId1, SimRedes.Network.DeviceType.Router, new Vector2(0, 0));
            topology.AddNode(discId2, SimRedes.Network.DeviceType.Router, new Vector2(200, 0));
            return topology;
        }

        /// <summary>
        /// Simula la colocacion secuencial de los 4 discos de configuracion
        /// de ruta (RedDestino, Mascara, ProximoSalto, InterfazSalida).
        ///
        /// Reproduce el mismo comportamiento de HandleRoutingConfigDisc() en
        /// DiscEventHandler:
        ///   1. RedDestino    → setea DestinationNetwork → IsComplete=false → log
        ///   2. Mascara       → setea SubnetMask         → IsComplete=false → log
        ///   3. ProximoSalto  → setea NextHop            → IsComplete=false → log
        ///   4. InterfazSalida→ setea OutInterface        → IsComplete=true  → ApplyToRouter
        /// </summary>
        private void SimulateDiscRouteSequence(TopologyManager topology, int routerDiscId,
            string destNetwork = "192.168.1.0", string mask = "255.255.255.0",
            string nextHop = "192.168.1.254", string outInterface = "G0/0",
            string protocol = null)
        {
            // Disco 1: RedDestino
            var builder = new RouteBuilderState
            {
                RouterDiscId = routerDiscId,
                DestinationNetwork = destNetwork
            };
            Assert.IsFalse(builder.IsComplete, "Solo RedDestino no deberia completar la ruta");

            // Disco 2: Mascara
            builder.SubnetMask = mask;
            Assert.IsFalse(builder.IsComplete, "RedDestino + Mascara no deberia completar la ruta");

            // Disco 3: ProximoSalto
            builder.NextHop = nextHop;
            Assert.IsFalse(builder.IsComplete, "3 campos no deberian completar la ruta");

            // Disco 4: InterfazSalida (completa la ruta)
            builder.OutInterface = outInterface;
            Assert.IsTrue(builder.IsComplete, "Los 4 campos deberian completar la ruta");

            // Protocolo opcional (seteado por disco ModoEnrutamiento antes de completar)
            if (!string.IsNullOrEmpty(protocol))
                builder.Protocol = protocol;

            builder.ApplyToRouter(topology);
        }

        // ====================================================================
        // TESTS: Disco → Tabla de Enrutamiento
        // ====================================================================

        [Test]
        public void RedDestinoDisc_Only_DoesNotAddRoute()
        {
            // ====================
            // Arrange
            // ====================
            var topology = CreateTopologyWithRouter(1);
            var router = topology.GetNode(1);

            // ====================
            // Act: Simular SOLO el disco RedDestino
            // ====================
            var builder = new RouteBuilderState
            {
                RouterDiscId = 1,
                DestinationNetwork = "192.168.1.0"
            };

            // ====================
            // Assert: Sin ruta — faltan 3 campos
            // ====================
            Assert.IsFalse(builder.IsComplete);
            Assert.AreEqual(0, router.RoutingTable.GetAllEntries().Count,
                "Un solo disco de configuracion no debe agregar una ruta");

            Object.DestroyImmediate(topology.gameObject);
        }

        [Test]
        public void CompleteDiscSequence_AddsRouteToRoutingTable()
        {
            // ====================
            // Arrange
            // ====================
            var topology = CreateTopologyWithRouter(1);
            var router = topology.GetNode(1);

            // ====================
            // Act: Simular los 4 discos
            // ====================
            SimulateDiscRouteSequence(topology, 1);

            // ====================
            // Assert: 1 ruta agregada con campos correctos
            // ====================
            var entries = router.RoutingTable.GetAllEntries();
            Assert.AreEqual(1, entries.Count, "4 discos de configuracion deben agregar 1 ruta");

            Assert.AreEqual("192.168.1.0",     entries[0].DestinationNetwork, "RedDestino incorrecto");
            Assert.AreEqual("255.255.255.0",   entries[0].SubnetMask,         "Mascara incorrecta");
            Assert.AreEqual("192.168.1.254",   entries[0].NextHop,            "ProximoSalto incorrecto");
            Assert.AreEqual("G0/0",            entries[0].OutInterface,       "InterfazSalida incorrecta");
            Assert.AreEqual("Static",          entries[0].Protocol,           "Protocolo por defecto debe ser Static");
            Assert.AreEqual(0,                 entries[0].Metric,             "Metrica por defecto debe ser 0");

            Object.DestroyImmediate(topology.gameObject);
        }

        [Test]
        public void CompleteDiscSequence_MultipleRounds_MultipleRoutes()
        {
            // ====================
            // Arrange
            // ====================
            var topology = CreateTopologyWithRouter(1);
            var router = topology.GetNode(1);

            // ====================
            // Act: 2 rondas de 4 discos
            // ====================
            SimulateDiscRouteSequence(topology, 1,
                destNetwork: "10.0.0.0",     mask: "255.0.0.0",
                nextHop: "10.0.0.1",         outInterface: "G0/0");

            SimulateDiscRouteSequence(topology, 1,
                destNetwork: "172.16.0.0",   mask: "255.255.0.0",
                nextHop: "172.16.0.1",       outInterface: "G0/1");

            // ====================
            // Assert: 2 rutas independientes
            // ====================
            var entries = router.RoutingTable.GetAllEntries();
            Assert.AreEqual(2, entries.Count, "2 rondas de 4 discos deben producir 2 rutas");

            Assert.AreEqual("10.0.0.0",   entries[0].DestinationNetwork, "Primera ruta incorrecta");
            Assert.AreEqual("172.16.0.0", entries[1].DestinationNetwork, "Segunda ruta incorrecta");

            Object.DestroyImmediate(topology.gameObject);
        }

        [Test]
        public void IpRouteDisc_AddsDefaultRoute()
        {
            // ====================
            // Arrange: Simular el comportamiento del disco IpRoute
            // (case DiscType.IpRoute en HandleRoutingConfigDisc)
            // ====================
            var topology = CreateTopologyWithRouter(1);
            var router = topology.GetNode(1);

            // ====================
            // Act: Como hace HandleIpRouteDisc()
            // ====================
            router.RoutingTable.AddStaticRoute("0.0.0.0", "0.0.0.0", "192.168.1.254", "G0/0");

            // ====================
            // Assert: Ruta por defecto agregada
            // ====================
            var entries = router.RoutingTable.GetAllEntries();
            Assert.AreEqual(1, entries.Count, "Disco IpRoute debe agregar 1 ruta");
            Assert.AreEqual("0.0.0.0",       entries[0].DestinationNetwork, "Debe ser ruta por defecto");
            Assert.AreEqual("0.0.0.0",       entries[0].SubnetMask,         "Mascara debe ser 0.0.0.0");
            Assert.AreEqual("192.168.1.254", entries[0].NextHop,            "Gateway predeterminado incorrecto");

            Object.DestroyImmediate(topology.gameObject);
        }

        [Test]
        public void DiscSequence_WithProtocolOverride_RouteHasCorrectProtocol()
        {
            // ====================
            // Arrange
            // ====================
            var topology = CreateTopologyWithRouter(1);
            var router = topology.GetNode(1);

            // ====================
            // Act: Simular disco ModoEnrutamiento antes de completar
            // (caso OSPF — builder.Protocol = "OSPF")
            // ====================
            SimulateDiscRouteSequence(topology, 1,
                destNetwork: "10.0.0.0", mask: "255.0.0.0",
                nextHop: "10.0.0.1", outInterface: "G0/0",
                protocol: "OSPF");

            // ====================
            // Assert: Protocolo OSPF en lugar de Static
            // ====================
            var entries = router.RoutingTable.GetAllEntries();
            Assert.AreEqual(1, entries.Count);
            Assert.AreEqual("OSPF", entries[0].Protocol,
                "El protocolo debe ser OSPF cuando se configura via disco ModoEnrutamiento");

            Object.DestroyImmediate(topology.gameObject);
        }

        [Test]
        public void MultipleRouters_DiscRoutes_AreIndependent()
        {
            // ====================
            // Arrange: 2 routers
            // ====================
            var topology = CreateTopologyWithTwoRouters(1, 2);
            var router1 = topology.GetNode(1);
            var router2 = topology.GetNode(2);

            // ====================
            // Act: Configurar rutas diferentes en cada router
            // ====================
            SimulateDiscRouteSequence(topology, 1,
                destNetwork: "10.0.0.0", mask: "255.0.0.0",
                nextHop: "10.0.0.1", outInterface: "G0/0");

            SimulateDiscRouteSequence(topology, 2,
                destNetwork: "172.16.0.0", mask: "255.255.0.0",
                nextHop: "172.16.0.1", outInterface: "G0/1",
                protocol: "RIP");

            // ====================
            // Assert: Cada router tiene SU ruta
            // ====================
            var routes1 = router1.RoutingTable.GetAllEntries();
            var routes2 = router2.RoutingTable.GetAllEntries();

            Assert.AreEqual(1, routes1.Count, "Router 1 debe tener 1 ruta");
            Assert.AreEqual(1, routes2.Count, "Router 2 debe tener 1 ruta");

            Assert.AreEqual("10.0.0.0",   routes1[0].DestinationNetwork, "Router 1: red incorrecta");
            Assert.AreEqual("172.16.0.0", routes2[0].DestinationNetwork, "Router 2: red incorrecta");

            // Protocolos diferentes
            Assert.AreEqual("Static", routes1[0].Protocol, "Router 1: protocolo por defecto");
            Assert.AreEqual("RIP",    routes2[0].Protocol, "Router 2: protocolo override");

            Object.DestroyImmediate(topology.gameObject);
        }

        // ====================================================================
        // TESTS: Tabla de Enrutamiento → UI (via RoutingSimulator)
        // ====================================================================

        [Test]
        public void DiscRoute_AppearsInRoutingSimulatorSummary()
        {
            // ====================
            // Arrange: Ruta configurada por discos
            // ====================
            var topology = CreateTopologyWithRouter(1);
            var router = topology.GetNode(1);

            SimulateDiscRouteSequence(topology, 1,
                destNetwork: "192.168.1.0", mask: "255.255.255.0",
                nextHop: "192.168.1.254", outInterface: "G0/0");

            // ====================
            // Act: Obtener el resumen que usan las actividades para mostrar en UI
            // ====================
            var summary = RoutingSimulator.GetRoutingTableSummary(router);

            // ====================
            // Assert: El resumen contiene la informacion de la ruta del disco
            // (GetRoutingTableSummary es el puente entre RoutingTable y la UI)
            // ====================
            StringAssert.Contains("192.168.1.0",  summary,
                "El resumen debe contener la red destino configurada por discos");
            StringAssert.Contains("192.168.1.254", summary,
                "El resumen debe contener el next-hop configurado por discos");
            StringAssert.Contains("Static",        summary,
                "El resumen debe contener el protocolo de la ruta");
            StringAssert.Contains("G0/0",          summary,
                "El resumen debe contener la interfaz de salida");

            Object.DestroyImmediate(topology.gameObject);
        }

        [Test]
        public void DiscRoute_SummarizesAllRounds()
        {
            // ====================
            // Arrange: 2 rondas de discos
            // ====================
            var topology = CreateTopologyWithRouter(1);
            var router = topology.GetNode(1);

            SimulateDiscRouteSequence(topology, 1,
                destNetwork: "10.0.0.0",   mask: "255.0.0.0",
                nextHop: "10.0.0.1",       outInterface: "G0/0");

            SimulateDiscRouteSequence(topology, 1,
                destNetwork: "172.16.0.0", mask: "255.255.0.0",
                nextHop: "172.16.0.1",     outInterface: "G0/1");

            // ====================
            // Act
            // ====================
            var summary = RoutingSimulator.GetRoutingTableSummary(router);

            // ====================
            // Assert: Ambas rutas aparecen
            // ====================
            StringAssert.Contains("10.0.0.0",   summary, "Ruta 1 debe estar en el resumen");
            StringAssert.Contains("172.16.0.0", summary, "Ruta 2 debe estar en el resumen");
            StringAssert.Contains("G0/0",       summary, "Interfaz ruta 1 debe estar en el resumen");
            StringAssert.Contains("G0/1",       summary, "Interfaz ruta 2 debe estar en el resumen");

            Object.DestroyImmediate(topology.gameObject);
        }

        [Test]
        public void DiscRoute_CanBeFoundByBestRoute()
        {
            // ====================
            // Arrange: Ruta configurada por discos
            // ====================
            var topology = CreateTopologyWithRouter(1);
            var router = topology.GetNode(1);

            SimulateDiscRouteSequence(topology, 1,
                destNetwork: "10.0.0.0", mask: "255.0.0.0",
                nextHop: "10.0.0.1", outInterface: "G0/0");

            // ====================
            // Act: Buscar mejor ruta para una IP dentro de 10.0.0.0/8
            // ====================
            var best = router.RoutingTable.FindBestRoute("10.1.1.1");

            // ====================
            // Assert: Ruta del disco es encontrada
            // ====================
            Assert.IsNotNull(best, "Debe encontrar una ruta para IP dentro de la red configurada");
            Assert.AreEqual("10.0.0.0",  best.DestinationNetwork, "Red destino incorrecta");
            Assert.AreEqual("10.0.0.1",  best.NextHop,            "NextHop incorrecto");

            Object.DestroyImmediate(topology.gameObject);
        }

        [Test]
        public void DiscRoute_LongestPrefixMatch_Works()
        {
            // ====================
            // Arrange: 2 rutas con diferente prefijo, configuradas por discos
            // ====================
            var topology = CreateTopologyWithRouter(1);
            var router = topology.GetNode(1);

            // Ruta /8
            SimulateDiscRouteSequence(topology, 1,
                destNetwork: "10.0.0.0",   mask: "255.0.0.0",
                nextHop: "10.0.0.1",       outInterface: "G0/0");

            // Ruta /16
            SimulateDiscRouteSequence(topology, 1,
                destNetwork: "10.1.0.0",   mask: "255.255.0.0",
                nextHop: "10.1.0.1",       outInterface: "G0/1");

            // ====================
            // Act: IP que matchea ambas (10.1.5.100)
            // ====================
            var best = router.RoutingTable.FindBestRoute("10.1.5.100");

            // ====================
            // Assert: Debe elegir la de prefijo mas largo (/16)
            // ====================
            Assert.IsNotNull(best, "Debe encontrar una ruta");
            Assert.AreEqual("10.1.0.0", best.DestinationNetwork,
                "Longest prefix match debe elegir 10.1.0.0/16 sobre 10.0.0.0/8");
            Assert.AreEqual("10.1.0.1", best.NextHop, "NextHop incorrecto");

            Object.DestroyImmediate(topology.gameObject);
        }
    }
}
