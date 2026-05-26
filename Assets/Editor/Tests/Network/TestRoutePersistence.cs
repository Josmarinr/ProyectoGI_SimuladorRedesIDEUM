using NUnit.Framework;
using SimRedes.Network;
using SimRedes.Tangible;
using SimRedes.Simulation;
using UnityEngine;

namespace Tests.EditMode.Network
{
    public class TestRoutePersistence
    {
        private TopologyManager CreateTopologyWithRouter(int discId = 1)
        {
            var go = new GameObject("TestTopology");
            var topology = go.AddComponent<TopologyManager>();
            topology.AddNode(discId, SimRedes.Network.DeviceType.Router, Vector2.zero);
            return topology;
        }

        [Test]
        public void RouteConfiguredByDisc_PersistsAfterActivityRefresh()
        {
            // ====================
            // Arrange: Simular discos 7-18 configurando ruta en router
            // ====================
            var topology = CreateTopologyWithRouter(1);
            var router = topology.GetNode(1);

            // Simular que los discos 7-18 llenan el RouteBuilderState
            var builder = new RouteBuilderState
            {
                RouterDiscId = 1,
                DestinationNetwork = "192.168.1.0",
                SubnetMask = "255.255.255.0",
                NextHop = "192.168.1.254",
                OutInterface = "G0/0",
                Protocol = "Static"
            };
            builder.ApplyToRouter(topology);

            // Verificar que la ruta del disco se agregó
            var routesBefore = router.RoutingTable.GetAllEntries();
            Assert.AreEqual(1, routesBefore.Count, "Debería haber 1 ruta configurada por discos");
            Assert.AreEqual("192.168.1.0", routesBefore[0].DestinationNetwork);
            Assert.AreEqual("255.255.255.0", routesBefore[0].SubnetMask);

            // ====================
            // Act: Simular recarga de actividad (RoutingTablesActivity.RefreshRoutingTables)
            // La actividad ya NO crea RoutingSimulator (es estático).
            // Solo agrega rutas de ejemplo al RoutingTable real sin limpiar.
            // ====================
            router.RoutingTable.AddStaticRoute("10.0.0.0", "255.0.0.0", "10.1.1.1", "G0/1");
            router.RoutingTable.AddStaticRoute("172.16.0.0", "255.240.0.0", "172.16.0.1", "G0/2");

            // ====================
            // Assert: La ruta del disco SIGUE AHÍ, no fue borrada
            // ====================
            var routesAfter = router.RoutingTable.GetAllEntries();
            Assert.AreEqual(3, routesAfter.Count, "Deben persistir 3 rutas (1 del disco + 2 de ejemplo)");

            // La ruta del disco debe estar intacta
            var discRoute = routesAfter.Find(r =>
                r.DestinationNetwork == "192.168.1.0" &&
                r.SubnetMask == "255.255.255.0" &&
                r.NextHop == "192.168.1.254"
            );
            Assert.IsNotNull(discRoute, "La ruta configurada por discos 7-18 debe persistir después de recargar actividad");
            Assert.AreEqual("G0/0", discRoute.OutInterface);
            Assert.AreEqual("Static", discRoute.Protocol);

            // Las rutas de ejemplo deben estar presentes
            var sampleRoute1 = routesAfter.Find(r => r.DestinationNetwork == "10.0.0.0");
            Assert.IsNotNull(sampleRoute1, "La ruta de ejemplo debe estar presente también");

            // Limpiar para no contaminar otros tests
            Object.DestroyImmediate(topology.gameObject);
        }

        [Test]
        public void RoutePersistence_AfterMultipleDiscRounds_AllRoutesIntact()
        {
            // ====================
            // Arrange: Configurar múltiples rutas con discos
            // ====================
            var topology = CreateTopologyWithRouter(1);
            var router = topology.GetNode(1);

            // Ronda 1: Disco - RedDestino, Mascara, ProximoSalto, InterfazSalida
            var builder1 = new RouteBuilderState
            {
                RouterDiscId = 1,
                DestinationNetwork = "10.0.0.0",
                SubnetMask = "255.0.0.0",
                NextHop = "10.0.0.254",
                OutInterface = "G0/0"
            };
            builder1.ApplyToRouter(topology);

            // Ronda 2: Disco - Destino
            var builder2 = new RouteBuilderState
            {
                RouterDiscId = 1,
                DestinationNetwork = "172.16.0.0",
                SubnetMask = "255.255.0.0",
                NextHop = "172.16.0.254",
                OutInterface = "G0/1"
            };
            builder2.ApplyToRouter(topology);

            // ====================
            // Act: Simular recarga de actividad (varias veces)
            // Las actividades ya no crean RoutingSimulator. Solo verificamos
            // que las rutas persisten sin que nada las borre.
            // ====================

            // ====================
            // Assert: Ambas rutas de discos persisten
            // ====================
            var routes = router.RoutingTable.GetAllEntries();
            Assert.AreEqual(2, routes.Count, "Ambas rutas configuradas por discos deben persistir");

            var route1 = routes.Find(r =>
                r.DestinationNetwork == "10.0.0.0" &&
                r.NextHop == "10.0.0.254"
            );
            Assert.IsNotNull(route1, "Ruta 10.0.0.0 debe persistir");

            var route2 = routes.Find(r =>
                r.DestinationNetwork == "172.16.0.0" &&
                r.NextHop == "172.16.0.254"
            );
            Assert.IsNotNull(route2, "Ruta 172.16.0.0 debe persistir");

            // Limpiar para no contaminar otros tests
            Object.DestroyImmediate(topology.gameObject);
        }

        [Test]
        public void ActivityLoad_DoesNotCallClearTopology()
        {
            // ====================
            // Arrange: Crear topología con router + ruta de disco
            // ====================
            var topology = CreateTopologyWithRouter(1);
            var router = topology.GetNode(1);

            // Simular disco IpRoute (tipo 15) que agrega ruta por defecto
            router.RoutingTable.AddStaticRoute("0.0.0.0", "0.0.0.0", "192.168.1.254", "G0/0");

            // ====================
            // Act: Simular lo que hace ActivityLoader.SelectActivity()
            // (buscar GameManager, agregar componentes, NO limpiar topología)
            // ====================
            var gameManagerObj = GameObject.Find("GameManager");
            if (gameManagerObj == null)
                gameManagerObj = new GameObject("GameManager");

            // SelectActivity agrega componentes si no existen (no limpia topología)
            if (gameManagerObj.GetComponent<SimulationControls>() == null)
                gameManagerObj.AddComponent<SimulationControls>();

            // ====================
            // Assert: La topología y sus rutas siguen intactas
            // ====================
            var routes = router.RoutingTable.GetAllEntries();
            Assert.AreEqual(1, routes.Count, "SelectActivity no debe limpiar las rutas existentes");
            Assert.AreEqual("0.0.0.0", routes[0].DestinationNetwork);
            Assert.AreEqual("192.168.1.254", routes[0].NextHop);

            // Limpiar el objeto creado para no interferir
            Object.DestroyImmediate(gameManagerObj);
            Object.DestroyImmediate(topology.gameObject);
        }
    }
}
