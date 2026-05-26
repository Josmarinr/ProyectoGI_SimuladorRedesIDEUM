using NUnit.Framework;
using SimRedes.Network;
using SimRedes.Tangible;
using UnityEngine;

namespace Tests.EditMode.Tangible
{
    public class TestRouteBuilderState
    {
        private TopologyManager CreateTopologyWithRouter(int discId = 1)
        {
            var go = new GameObject("TestTopology");
            var topology = go.AddComponent<TopologyManager>();
            topology.AddNode(discId, SimRedes.Network.DeviceType.Router, Vector2.zero);
            return topology;
        }

        [Test]
        public void IsComplete_AllFieldsSet_ReturnsTrue()
        {
            var builder = new RouteBuilderState
            {
                DestinationNetwork = "192.168.1.0",
                SubnetMask = "255.255.255.0",
                NextHop = "192.168.1.254",
                OutInterface = "G0/0"
            };

            Assert.IsTrue(builder.IsComplete);
        }

        [Test]
        public void IsComplete_MissingDestination_ReturnsFalse()
        {
            var builder = new RouteBuilderState
            {
                DestinationNetwork = null,
                SubnetMask = "255.255.255.0",
                NextHop = "192.168.1.254",
                OutInterface = "G0/0"
            };

            Assert.IsFalse(builder.IsComplete);
        }

        [Test]
        public void IsComplete_MissingSubnetMask_ReturnsFalse()
        {
            var builder = new RouteBuilderState
            {
                DestinationNetwork = "192.168.1.0",
                SubnetMask = null,
                NextHop = "192.168.1.254",
                OutInterface = "G0/0"
            };

            Assert.IsFalse(builder.IsComplete);
        }

        [Test]
        public void IsComplete_MissingNextHop_ReturnsFalse()
        {
            var builder = new RouteBuilderState
            {
                DestinationNetwork = "192.168.1.0",
                SubnetMask = "255.255.255.0",
                NextHop = null,
                OutInterface = "G0/0"
            };

            Assert.IsFalse(builder.IsComplete);
        }

        [Test]
        public void IsComplete_MissingInterface_ReturnsFalse()
        {
            var builder = new RouteBuilderState
            {
                DestinationNetwork = "192.168.1.0",
                SubnetMask = "255.255.255.0",
                NextHop = "192.168.1.254",
                OutInterface = null
            };

            Assert.IsFalse(builder.IsComplete);
        }

        [Test]
        public void IsComplete_EmptyStrings_ReturnsFalse()
        {
            var builder = new RouteBuilderState
            {
                DestinationNetwork = "",
                SubnetMask = "255.255.255.0",
                NextHop = "192.168.1.254",
                OutInterface = "G0/0"
            };

            Assert.IsFalse(builder.IsComplete);
        }

        [Test]
        public void IsComplete_ProtocolNotRequired_ReturnsTrue()
        {
            var builder = new RouteBuilderState
            {
                DestinationNetwork = "192.168.1.0",
                SubnetMask = "255.255.255.0",
                NextHop = "192.168.1.254",
                OutInterface = "G0/0",
                Protocol = null
            };

            Assert.IsTrue(builder.IsComplete);
        }

        [Test]
        public void ApplyToRouter_WhenComplete_AddsStaticRoute()
        {
            var topology = CreateTopologyWithRouter(1);
            var router = topology.GetNode(1);

            var builder = new RouteBuilderState
            {
                RouterDiscId = 1,
                DestinationNetwork = "10.0.0.0",
                SubnetMask = "255.0.0.0",
                NextHop = "10.0.0.1",
                OutInterface = "G0/0"
            };

            builder.ApplyToRouter(topology);

            var entries = router.RoutingTable.GetAllEntries();
            Assert.AreEqual(1, entries.Count);
            Assert.AreEqual("10.0.0.0", entries[0].DestinationNetwork);
            Assert.AreEqual("255.0.0.0", entries[0].SubnetMask);
            Assert.AreEqual("10.0.0.1", entries[0].NextHop);
            Assert.AreEqual("Static", entries[0].Protocol);
        }

        [Test]
        public void ApplyToRouter_WithProtocolOverride_SetsProtocol()
        {
            var topology = CreateTopologyWithRouter(1);
            var router = topology.GetNode(1);

            var builder = new RouteBuilderState
            {
                RouterDiscId = 1,
                DestinationNetwork = "10.0.0.0",
                SubnetMask = "255.0.0.0",
                NextHop = "10.0.0.1",
                OutInterface = "G0/0",
                Protocol = "RIP"
            };

            builder.ApplyToRouter(topology);

            var entries = router.RoutingTable.GetAllEntries();
            Assert.AreEqual(1, entries.Count);
            Assert.AreEqual("RIP", entries[0].Protocol);
        }

        [Test]
        public void ApplyToRouter_WithOSPFProtocol_SetsProtocol()
        {
            var topology = CreateTopologyWithRouter(1);
            var router = topology.GetNode(1);

            var builder = new RouteBuilderState
            {
                RouterDiscId = 1,
                DestinationNetwork = "10.0.0.0",
                SubnetMask = "255.0.0.0",
                NextHop = "10.0.0.1",
                OutInterface = "G0/0",
                Protocol = "OSPF"
            };

            builder.ApplyToRouter(topology);

            var entries = router.RoutingTable.GetAllEntries();
            Assert.AreEqual(1, entries.Count);
            Assert.AreEqual("OSPF", entries[0].Protocol);
        }

        [Test]
        public void ApplyToRouter_WhenIncomplete_DoesNothing()
        {
            var topology = CreateTopologyWithRouter(1);
            var router = topology.GetNode(1);

            var builder = new RouteBuilderState
            {
                RouterDiscId = 1,
                DestinationNetwork = "10.0.0.0",
                SubnetMask = "255.0.0.0",
                NextHop = null, // missing
                OutInterface = "G0/0"
            };

            builder.ApplyToRouter(topology);

            Assert.AreEqual(0, router.RoutingTable.GetAllEntries().Count);
        }

        [Test]
        public void ApplyToRouter_NonExistentRouter_DoesNothing()
        {
            var topology = CreateTopologyWithRouter(1);

            var builder = new RouteBuilderState
            {
                RouterDiscId = 999, // doesn't exist
                DestinationNetwork = "10.0.0.0",
                SubnetMask = "255.0.0.0",
                NextHop = "10.0.0.1",
                OutInterface = "G0/0"
            };

            // Should not throw
            Assert.DoesNotThrow(() => builder.ApplyToRouter(topology));
        }

        [Test]
        public void Reset_ClearsAllRouteFields()
        {
            var builder = new RouteBuilderState
            {
                DestinationNetwork = "192.168.1.0",
                SubnetMask = "255.255.255.0",
                NextHop = "192.168.1.254",
                OutInterface = "G0/0"
            };

            builder.Reset();

            Assert.IsNull(builder.DestinationNetwork);
            Assert.IsNull(builder.SubnetMask);
            Assert.IsNull(builder.NextHop);
            Assert.IsNull(builder.OutInterface);
        }

        [Test]
        public void Reset_DoesNotClearProtocol()
        {
            var builder = new RouteBuilderState
            {
                DestinationNetwork = "192.168.1.0",
                SubnetMask = "255.255.255.0",
                NextHop = "192.168.1.254",
                OutInterface = "G0/0",
                Protocol = "RIP"
            };

            builder.Reset();

            // Protocol is NOT reset by design
            Assert.AreEqual("RIP", builder.Protocol);
        }

        [Test]
        public void ApplyToRouter_CanBeCalledMultipleTimes()
        {
            var topology = CreateTopologyWithRouter(1);
            var router = topology.GetNode(1);

            var builder = new RouteBuilderState
            {
                RouterDiscId = 1,
                DestinationNetwork = "10.0.0.0",
                SubnetMask = "255.0.0.0",
                NextHop = "10.0.0.1",
                OutInterface = "G0/0"
            };

            builder.ApplyToRouter(topology);
            builder.ApplyToRouter(topology);
            builder.ApplyToRouter(topology);

            // Each call adds a new route (no dedup)
            Assert.AreEqual(3, router.RoutingTable.GetAllEntries().Count);
        }
    }
}
