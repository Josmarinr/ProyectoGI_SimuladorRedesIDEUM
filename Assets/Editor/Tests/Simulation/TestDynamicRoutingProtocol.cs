using NUnit.Framework;
using SimRedes.Network;
using SimRedes.Simulation;
using UnityEngine;

namespace Tests.EditMode.Simulation
{
    /// <summary>
    /// Tests for DynamicRoutingProtocol.
    /// Validates protocol lifecycle, status reporting, and virtual disc config (15-18).
    /// </summary>
    public class TestDynamicRoutingProtocol
    {
        private GameObject protocolGo;
        private DynamicRoutingProtocol protocol;
        private GameObject topologyGo;
        private TopologyManager topology;

        [SetUp]
        public void SetUp()
        {
            protocolGo = new GameObject("Protocol");
            protocol = protocolGo.AddComponent<DynamicRoutingProtocol>();
            topologyGo = new GameObject("Topology");
            topology = topologyGo.AddComponent<TopologyManager>();
        }

        [TearDown]
        public void TearDown()
        {
            if (protocol != null && protocolGo != null)
                Object.DestroyImmediate(protocolGo);
            if (topology != null && topologyGo != null)
                Object.DestroyImmediate(topologyGo);
        }

        // ================================================================
        // Helpers
        // ================================================================

        /// <summary>
        /// Creates two routers at positions 0,0 and 200,0 with a link between them.
        /// Also sets the protocol's topology field via reflection.
        /// </summary>
        private void CreateTwoRoutersWithLink()
        {
            topology.AddNode(1, SimRedes.Network.DeviceType.Router, Vector2.zero);
            topology.AddNode(2, SimRedes.Network.DeviceType.Router, new Vector2(200, 0));
            topology.AddLink(1, 2);
            SetField("topology", topology);
        }

        /// <summary>
        /// Reads a private field via reflection.
        /// </summary>
        private T GetField<T>(string fieldName)
        {
            var field = typeof(DynamicRoutingProtocol).GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (T)field.GetValue(protocol);
        }

        /// <summary>
        /// Sets a private field via reflection.
        /// </summary>
        private void SetField<T>(string fieldName, T value)
        {
            var field = typeof(DynamicRoutingProtocol).GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(protocol, value);
        }

        // ================================================================
        // Protocol Lifecycle
        // ================================================================

        [Test]
        public void StartProtocol_WithTwoRouters_SetsRunningState()
        {
            CreateTwoRoutersWithLink();
            protocol.StartProtocol();

            Assert.IsTrue(GetField<bool>("isRunning"));
            Assert.AreEqual(0, protocol.GetAdvertisementCount());
        }

        [Test]
        public void StartProtocol_WithLessThanTwoRouters_DoesNotStart()
        {
            topology.AddNode(1, SimRedes.Network.DeviceType.Router, Vector2.zero);
            SetField("topology", topology);
            protocol.StartProtocol();

            Assert.IsFalse(GetField<bool>("isRunning"));
        }

        [Test]
        public void StartProtocol_WithoutTopologyManager_DoesNotThrow()
        {
            // Bug #4 fixed: StartProtocol() now has null guard for topology.
            // Should log error and return gracefully.
            Assert.DoesNotThrow(() => protocol.StartProtocol());
            Assert.IsFalse(GetField<bool>("isRunning"));
        }

        [Test]
        public void StopProtocol_AfterStart_StopsExecution()
        {
            CreateTwoRoutersWithLink();
            protocol.StartProtocol();
            protocol.StopProtocol();

            Assert.IsFalse(GetField<bool>("isRunning"));
        }

        // ================================================================
        // Protocol Configuration
        // ================================================================

        [Test]
        public void ProtocolType_DefaultIsRIP()
        {
            Assert.AreEqual(DynamicRoutingProtocol.ProtocolType.RIP, protocol.protocol);
        }

        [Test]
        public void ProtocolType_SetToOSPF_ReturnsCorrectType()
        {
            protocol.protocol = DynamicRoutingProtocol.ProtocolType.OSPF;
            Assert.AreEqual(DynamicRoutingProtocol.ProtocolType.OSPF, protocol.protocol);
        }

        // ================================================================
        // Status Reporting
        // ================================================================

        [Test]
        public void GetProtocolStatus_NotRunning_ReturnsStopped()
        {
            string status = protocol.GetProtocolStatus();
            Assert.IsTrue(status.Contains("Detenido") || status.Contains("detenido"),
                $"Expected stopped status, got: {status}");
            Assert.IsTrue(status.Contains("RIP"), $"Expected RIP in status, got: {status}");
        }

        [Test]
        public void GetProtocolStatus_AfterStart_ReturnsRunning()
        {
            CreateTwoRoutersWithLink();
            protocol.StartProtocol();
            string status = protocol.GetProtocolStatus();
            Assert.IsTrue(status.Contains("Ejecutando") || status.Contains("ejecutando"),
                $"Expected running status, got: {status}");
            Assert.IsTrue(status.Contains("0 ads") || status.Contains("0"),
                $"Expected 0 ads, got: {status}");
        }

        // ================================================================
        // Router Routes Summary
        // ================================================================

        [Test]
        public void GetRouterRoutesSummary_WithRoutes_ReturnsFormattedSummary()
        {
            topology.AddNode(1, SimRedes.Network.DeviceType.Router, Vector2.zero);
            var router = topology.GetNode(1);
            router.RoutingTable.AddStaticRoute("192.168.1.0", "255.255.255.0", "10.0.0.1", "G0/0");
            // AddRipRoute signature: (destNetwork, nextHop, iface, hops) — no mask, last param is int
            router.RoutingTable.AddRipRoute("10.0.0.0", "10.0.0.2", "G0/1", 1);

            string summary = protocol.GetRouterRoutesSummary(router);
            Assert.IsTrue(summary.Contains("192.168.1.0"), "Should contain first route destination");
            Assert.IsTrue(summary.Contains("10.0.0.0"), "Should contain second route destination");
        }

        [Test]
        public void GetRouterRoutesSummary_NoRoutes_ReturnsSinRutas()
        {
            topology.AddNode(1, SimRedes.Network.DeviceType.Router, Vector2.zero);
            var router = topology.GetNode(1);
            string summary = protocol.GetRouterRoutesSummary(router);
            Assert.AreEqual("Sin rutas", summary);
        }

        // ================================================================
        // Virtual Disc Configuration (15-18)
        // ================================================================

        [Test]
        public void SetManualNeighbor_SetsInternalField()
        {
            protocol.SetManualNeighbor("Router_2");
            Assert.AreEqual("Router_2", GetField<string>("manualNeighbor"));
        }

        [Test]
        public void SetManualNetwork_SetsInternalField()
        {
            protocol.SetManualNetwork("10.0.0.0/8");
            Assert.AreEqual("10.0.0.0/8", GetField<string>("manualNetwork"));
        }

        [Test]
        public void SetCustomCost_SetsInternalField()
        {
            protocol.SetCustomCost(42);
            Assert.AreEqual(42, GetField<int?>("customCost"));
        }

        [Test]
        public void SetCustomBandwidth_ValidValue_SetsField()
        {
            protocol.SetCustomBandwidth(500);
            Assert.AreEqual(500, GetField<int>("customBandwidth"));
        }

        // ================================================================
        // Route Management
        // ================================================================

        [Test]
        public void ClearAllRoutes_AfterStartClearsEverything()
        {
            CreateTwoRoutersWithLink();
            protocol.StartProtocol();
            var router = topology.GetAllNodes().Find(n => n.Type == SimRedes.Network.DeviceType.Router);
            if (router != null)
                router.RoutingTable.AddStaticRoute("192.168.1.0", "255.255.255.0", "10.0.0.1", "G0/0");

            protocol.ClearAllRoutes();

            Assert.IsFalse(GetField<bool>("isRunning"));
            Assert.AreEqual(0, protocol.GetAdvertisementCount());
            if (router != null)
                Assert.AreEqual(0, router.RoutingTable.GetAllEntries().Count);
        }

        [Test]
        public void StartProtocol_Restart_ResetsAdvertisementCount()
        {
            CreateTwoRoutersWithLink();
            protocol.StartProtocol();
            protocol.StopProtocol();
            protocol.StartProtocol();
            Assert.AreEqual(0, protocol.GetAdvertisementCount());
        }
    }
}
