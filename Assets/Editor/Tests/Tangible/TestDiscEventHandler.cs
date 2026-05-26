using NUnit.Framework;
using UnityEngine;
using SimRedes.Network;
using SimRedes.Tangible;
using SimRedes.Simulation;
using System.Collections.Generic;

namespace Tests.EditMode.Tangible
{
    /// <summary>
    /// Tests for DiscEventHandler.
    /// Validates disc placement/removal for physical discs (1-3) and
    /// routing configuration for virtual discs (7-18).
    /// </summary>
    public class TestDiscEventHandler
    {
        private GameObject handlerGo;
        private DiscEventHandler handler;
        private GameObject topologyGo;
        private TopologyManager topology;
        private GameObject discManagerGo;
        private TangibleDiscManager discManager;
        private GameObject dynActGo;
        private DynamicRoutingActivity dynAct;

        [SetUp]
        public void SetUp()
        {
            handlerGo = new GameObject("Handler");
            handler = handlerGo.AddComponent<DiscEventHandler>();
            topologyGo = new GameObject("Topology");
            topology = topologyGo.AddComponent<TopologyManager>();
            discManagerGo = new GameObject("DiscManager");
            discManager = discManagerGo.AddComponent<TangibleDiscManager>();

            // Invoke lifecycle methods via reflection (not auto-called in EditMode)
            InvokeMethod(handler, "Awake");
            InvokeMethod(handler, "Start");  // Subscribes to TangibleDiscManager events
            InvokeMethod(discManager, "Awake");
        }

        [TearDown]
        public void TearDown()
        {
            if (dynActGo != null) Object.DestroyImmediate(dynActGo);
            if (handlerGo != null) Object.DestroyImmediate(handlerGo);
            if (discManagerGo != null) Object.DestroyImmediate(discManagerGo);
            if (topologyGo != null) Object.DestroyImmediate(topologyGo);
        }

        // ================================================================
        // Helpers
        // ================================================================

        /// <summary>
        /// Invokes a method by name on a MonoBehaviour via reflection.
        /// Supports both public and non-public instance methods.
        /// </summary>
        private void InvokeMethod(MonoBehaviour mb, string methodName)
        {
            var flags = System.Reflection.BindingFlags.Instance
                      | System.Reflection.BindingFlags.NonPublic
                      | System.Reflection.BindingFlags.Public;
            var method = mb.GetType().GetMethod(methodName, flags);
            if (method != null)
                method.Invoke(mb, null);
        }

        /// <summary>
        /// Invokes the private HandleRoutingConfigDisc method via reflection.
        /// </summary>
        private void InvokeHandleRoutingConfigDisc(int discId, Vector2 position, DiscConfiguration config)
        {
            var method = typeof(DiscEventHandler).GetMethod("HandleRoutingConfigDisc",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(method, "HandleRoutingConfigDisc method not found");
            method.Invoke(handler, new object[] { discId, position, config, topology });
        }

        /// <summary>
        /// Reads the private routeBuilders dictionary via reflection.
        /// </summary>
        private Dictionary<int, RouteBuilderState> GetRouteBuilders()
        {
            var field = typeof(DiscEventHandler).GetField("routeBuilders",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (Dictionary<int, RouteBuilderState>)field.GetValue(handler);
        }

        /// <summary>
        /// Returns the correct DiscId for a given DiscType.
        /// DiscConfiguration.DefaultConfiguration maps DiscId (1-18) to DiscType.
        /// The DiscType enum values (0-17) do NOT match the DiscIds.
        /// </summary>
        private int GetDiscIdForType(DiscType type)
        {
            foreach (var config in DiscConfiguration.DefaultConfiguration)
            {
                if (config.Type == type)
                    return config.DiscId;
            }
            return -1;
        }

        // ================================================================
        // Physical Disc Placement (1-3)
        // ================================================================

        [Test]
        public void HandleDiscPlaced_RouterDisc_CreatesNode()
        {
            discManager.SimulateDiscPlaced(1, Vector2.zero);
            var nodes = topology.GetAllNodes();
            Assert.AreEqual(1, nodes.Count);
            Assert.AreEqual(SimRedes.Network.DeviceType.Router, nodes[0].Type);
        }

        [Test]
        public void HandleDiscPlaced_SwitchDisc_CreatesNode()
        {
            discManager.SimulateDiscPlaced(2, Vector2.zero);
            var nodes = topology.GetAllNodes();
            Assert.AreEqual(1, nodes.Count);
            Assert.AreEqual(SimRedes.Network.DeviceType.Switch, nodes[0].Type);
        }

        [Test]
        public void HandleDiscPlaced_PCDisc_CreatesNode()
        {
            discManager.SimulateDiscPlaced(3, Vector2.zero);
            var nodes = topology.GetAllNodes();
            Assert.AreEqual(1, nodes.Count);
            Assert.AreEqual(SimRedes.Network.DeviceType.PC, nodes[0].Type);
        }

        [Test]
        public void HandleDiscPlaced_NonPhysicalDisc_DoesNotCreateNode()
        {
            // discType 7 = RedDestino (routing config disc, not physical)
            discManager.SimulateDiscPlaced(7, Vector2.zero);
            Assert.AreEqual(0, topology.GetAllNodes().Count);
        }

        // ================================================================
        // Disc Removal
        // ================================================================

        [Test]
        public void HandleDiscRemoved_ExistingNode_RemovesFromTopology()
        {
            int uniqueId = discManager.SimulateDiscPlaced(1, Vector2.zero);
            Assert.AreEqual(1, topology.GetAllNodes().Count);
            discManager.SimulateDiscRemoved(uniqueId);
            Assert.AreEqual(0, topology.GetAllNodes().Count);
        }

        [Test]
        public void HandleDiscRemoved_NonExistentNode_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => discManager.SimulateDiscRemoved(999));
        }

        // ================================================================
        // Routing Configuration Discs (7-14)
        // ================================================================

        [Test]
        public void HandleRoutingConfigDisc_RedDestinoDisc_CreatesBuilderEntry()
        {
            topology.AddNode(1, SimRedes.Network.DeviceType.Router, Vector2.zero);
            var router = topology.GetNode(1);
            int redDestinoId = GetDiscIdForType(DiscType.RedDestino); // 7
            var config = DiscConfiguration.GetConfiguration(redDestinoId);
            InvokeHandleRoutingConfigDisc(1, Vector2.zero, config);

            var builders = GetRouteBuilders();
            Assert.IsTrue(builders.ContainsKey(router.DiscId));
            Assert.AreEqual("192.168.1.0", builders[router.DiscId].DestinationNetwork);
        }

        [Test]
        public void HandleRoutingConfigDisc_FourDiscs_CompleteRoute()
        {
            topology.AddNode(1, SimRedes.Network.DeviceType.Router, Vector2.zero);
            var router = topology.GetNode(1);
            InvokeHandleRoutingConfigDisc(1, Vector2.zero,
                DiscConfiguration.GetConfiguration(GetDiscIdForType(DiscType.RedDestino)));    // 7
            InvokeHandleRoutingConfigDisc(1, Vector2.zero,
                DiscConfiguration.GetConfiguration(GetDiscIdForType(DiscType.Mascara)));       // 13
            InvokeHandleRoutingConfigDisc(1, Vector2.zero,
                DiscConfiguration.GetConfiguration(GetDiscIdForType(DiscType.ProximoSalto)));  // 14
            InvokeHandleRoutingConfigDisc(1, Vector2.zero,
                DiscConfiguration.GetConfiguration(GetDiscIdForType(DiscType.InterfazSalida))); // 9

            var entries = router.RoutingTable.GetAllEntries();
            Assert.AreEqual(1, entries.Count);
            Assert.AreEqual("192.168.1.0", entries[0].DestinationNetwork);
        }

        [Test]
        public void HandleRoutingConfigDisc_IpRouteDisc_AddsDefaultRoute()
        {
            topology.AddNode(1, SimRedes.Network.DeviceType.Router, Vector2.zero);
            var router = topology.GetNode(1);
            int ipRouteId = GetDiscIdForType(DiscType.IpRoute); // 11
            InvokeHandleRoutingConfigDisc(1, Vector2.zero, DiscConfiguration.GetConfiguration(ipRouteId));

            var entries = router.RoutingTable.GetAllEntries();
            Assert.AreEqual(1, entries.Count);
            Assert.AreEqual("0.0.0.0", entries[0].DestinationNetwork);
            Assert.AreEqual("0.0.0.0", entries[0].SubnetMask);
            Assert.AreEqual("192.168.1.254", entries[0].NextHop);
        }

        [Test]
        public void HandleRoutingConfigDisc_ModoEnrutamiento_TogglesProtocol()
        {
            topology.AddNode(1, SimRedes.Network.DeviceType.Router, Vector2.zero);
            var router = topology.GetNode(1);
            int modoId = GetDiscIdForType(DiscType.ModoEnrutamiento); // 10
            InvokeHandleRoutingConfigDisc(1, Vector2.zero, DiscConfiguration.GetConfiguration(modoId));

            var builders = GetRouteBuilders();
            Assert.IsTrue(builders.ContainsKey(router.DiscId));
            // First toggle: Protocol starts null, null != "OSPF" → becomes "OSPF"
            Assert.AreEqual("OSPF", builders[router.DiscId].Protocol);
        }

        [Test]
        public void HandleRoutingConfigDisc_Metrica_AdjustsLastRouteMetric()
        {
            topology.AddNode(1, SimRedes.Network.DeviceType.Router, Vector2.zero);
            var router = topology.GetNode(1);
            router.RoutingTable.AddStaticRoute("192.168.1.0", "255.255.255.0", "10.0.0.1", "G0/0");
            int metricaId = GetDiscIdForType(DiscType.Metrica); // 8
            InvokeHandleRoutingConfigDisc(1, Vector2.zero, DiscConfiguration.GetConfiguration(metricaId));

            var entries = router.RoutingTable.GetAllEntries();
            Assert.AreEqual(10, entries[^1].Metric);
        }

        [Test]
        public void HandleRoutingConfigDisc_DestinoDisc_SetsDestinationNetwork()
        {
            topology.AddNode(1, SimRedes.Network.DeviceType.Router, Vector2.zero);
            var router = topology.GetNode(1);
            int destinoId = GetDiscIdForType(DiscType.Destino); // 12
            InvokeHandleRoutingConfigDisc(1, Vector2.zero, DiscConfiguration.GetConfiguration(destinoId));

            var builders = GetRouteBuilders();
            Assert.IsTrue(builders.ContainsKey(router.DiscId));
            Assert.AreEqual("192.168.1.0", builders[router.DiscId].DestinationNetwork);
        }

        // ================================================================
        // Virtual Discs 15-18 (Vecino, AnunciarRed, Costo, BW)
        // ================================================================

        [Test]
        public void HandleRoutingConfigDisc_VecinoDisc_CallsSetNeighborRouter()
        {
            topology.AddNode(1, SimRedes.Network.DeviceType.Router, Vector2.zero);
            var router = topology.GetNode(1);
            dynActGo = new GameObject("DynAct");
            dynAct = dynActGo.AddComponent<DynamicRoutingActivity>();
            int vecinoId = GetDiscIdForType(DiscType.Vecino); // 15
            InvokeHandleRoutingConfigDisc(1, Vector2.zero, DiscConfiguration.GetConfiguration(vecinoId));

            Assert.IsFalse(string.IsNullOrEmpty(dynAct.neighborRouter));
            Assert.IsTrue(dynAct.neighborRouter.Contains("Router"));
        }

        [Test]
        public void HandleRoutingConfigDisc_VecinoDisc_WithoutActivity_DoesNotThrow()
        {
            topology.AddNode(1, SimRedes.Network.DeviceType.Router, Vector2.zero);
            var router = topology.GetNode(1);
            // NO DynamicRoutingActivity created — should log and not throw
            int vecinoId = GetDiscIdForType(DiscType.Vecino); // 15
            Assert.DoesNotThrow(() =>
            {
                InvokeHandleRoutingConfigDisc(1, Vector2.zero, DiscConfiguration.GetConfiguration(vecinoId));
            });
        }

        [Test]
        public void HandleRoutingConfigDisc_AnunciarRedDisc_CallsSetNetworkToAdvertise()
        {
            topology.AddNode(1, SimRedes.Network.DeviceType.Router, Vector2.zero);
            var router = topology.GetNode(1);
            dynActGo = new GameObject("DynAct");
            dynAct = dynActGo.AddComponent<DynamicRoutingActivity>();
            int anunciarId = GetDiscIdForType(DiscType.AnunciarRed); // 16
            InvokeHandleRoutingConfigDisc(1, Vector2.zero, DiscConfiguration.GetConfiguration(anunciarId));

            Assert.AreEqual("10.0.0.0/8", dynAct.networkToAdvertise);
        }

        [Test]
        public void HandleRoutingConfigDisc_CostoDisc_CallsSetLinkCost()
        {
            topology.AddNode(1, SimRedes.Network.DeviceType.Router, Vector2.zero);
            var router = topology.GetNode(1);
            dynActGo = new GameObject("DynAct");
            dynAct = dynActGo.AddComponent<DynamicRoutingActivity>();
            int costoId = GetDiscIdForType(DiscType.Costo); // 17
            InvokeHandleRoutingConfigDisc(1, Vector2.zero, DiscConfiguration.GetConfiguration(costoId));

            Assert.AreEqual(15, dynAct.linkCost);
        }

        [Test]
        public void HandleRoutingConfigDisc_BWDisc_CallsSetBandwidth()
        {
            topology.AddNode(1, SimRedes.Network.DeviceType.Router, Vector2.zero);
            var router = topology.GetNode(1);
            dynActGo = new GameObject("DynAct");
            dynAct = dynActGo.AddComponent<DynamicRoutingActivity>();
            int bwId = GetDiscIdForType(DiscType.BW); // 18
            InvokeHandleRoutingConfigDisc(1, Vector2.zero, DiscConfiguration.GetConfiguration(bwId));

            Assert.AreEqual(100, dynAct.bandwidth);
        }

        // ================================================================
        // Edge Cases
        // ================================================================

        [Test]
        public void HandleRoutingConfigDisc_WithoutRouterNearby_DoesNotAddRoute()
        {
            // Router is far from the disc position
            topology.AddNode(1, SimRedes.Network.DeviceType.Router, new Vector2(1000, 1000));
            var router = topology.GetNode(1);
            int redDestinoId = GetDiscIdForType(DiscType.RedDestino); // 7
            InvokeHandleRoutingConfigDisc(1, Vector2.zero, DiscConfiguration.GetConfiguration(redDestinoId));

            // No route should be added since no router is near the disc position
            Assert.AreEqual(0, router.RoutingTable.GetAllEntries().Count);
        }
    }
}
