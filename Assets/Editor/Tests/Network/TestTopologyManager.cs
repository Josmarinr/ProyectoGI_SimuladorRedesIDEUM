using NUnit.Framework;
using SimRedes.Network;
using SimRedes.Simulation;
using System.Collections.Generic;
using UnityEngine;

namespace Tests.EditMode.Network
{
    public class TestTopologyManager
    {
        private GameObject topologyGo;
        private TopologyManager topology;

        [SetUp]
        public void SetUp()
        {
            topologyGo = new GameObject("TestTopology");
            topology = topologyGo.AddComponent<TopologyManager>();

            var awakeMethod = typeof(TopologyManager).GetMethod("Awake",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (awakeMethod != null)
                awakeMethod.Invoke(topology, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (topologyGo != null)
                Object.DestroyImmediate(topologyGo);
        }

        // ================================================================
        // NODE CREATION
        // ================================================================

        [Test]
        public void AddNode_Router_CreatesNode()
        {
            // Arrange
            topology.AddNode(1, SimRedes.Network.DeviceType.Router, Vector2.zero);

            // Act
            var node = topology.GetNode(1);

            // Assert
            Assert.IsNotNull(node);
            Assert.AreEqual(1, node.DiscId);
            Assert.AreEqual(SimRedes.Network.DeviceType.Router, node.Type);
        }

        [Test]
        public void AddNode_DuplicateDiscId_DoesNotDuplicate()
        {
            // Arrange
            topology.AddNode(1, SimRedes.Network.DeviceType.Router, Vector2.zero);

            // Act
            topology.AddNode(1, SimRedes.Network.DeviceType.PC, new Vector2(100, 100));

            // Assert
            Assert.AreEqual(1, topology.GetAllNodes().Count);
            var node = topology.GetNode(1);
            Assert.IsNotNull(node);
            // Position should be updated
            Assert.AreEqual(new Vector2(100, 100), node.Position);
            // Type should remain Router (unchanged by duplicate)
            Assert.AreEqual(SimRedes.Network.DeviceType.Router, node.Type);
        }

        [Test]
        public void AddNode_Switch_CreatesSwitch()
        {
            // Act
            topology.AddNode(1, SimRedes.Network.DeviceType.Switch, Vector2.zero);
            var node = topology.GetNode(1);

            // Assert
            Assert.IsNotNull(node);
            Assert.AreEqual(SimRedes.Network.DeviceType.Switch, node.Type);
        }

        [Test]
        public void AddNode_PC_CreatesPC()
        {
            // Act
            topology.AddNode(1, SimRedes.Network.DeviceType.PC, Vector2.zero);
            var node = topology.GetNode(1);

            // Assert
            Assert.IsNotNull(node);
            Assert.AreEqual(SimRedes.Network.DeviceType.PC, node.Type);
        }

        [Test]
        public void AddNode_DuplicateDiscId_UpdatesPosition()
        {
            // Arrange
            topology.AddNode(1, SimRedes.Network.DeviceType.PC, Vector2.zero);

            // Act
            topology.AddNode(1, SimRedes.Network.DeviceType.PC, new Vector2(300, 400));

            // Assert
            var node = topology.GetNode(1);
            Assert.AreEqual(new Vector2(300, 400), node.Position);
        }

        // ================================================================
        // NODE REMOVAL
        // ================================================================

        [Test]
        public void RemoveNode_Existing_RemovesNode()
        {
            // Arrange
            topology.AddNode(1, SimRedes.Network.DeviceType.Router, Vector2.zero);

            // Act
            topology.RemoveNode(1);

            // Assert
            Assert.IsNull(topology.GetNode(1));
            Assert.AreEqual(0, topology.GetAllNodes().Count);
        }

        [Test]
        public void RemoveNode_NonExistent_DoesNothing()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => topology.RemoveNode(999));
            Assert.AreEqual(0, topology.GetAllNodes().Count);
        }

        [Test]
        public void RemoveNode_RemovesAssociatedLinks()
        {
            // Arrange
            topology.AddNode(1, SimRedes.Network.DeviceType.Router, Vector2.zero);
            topology.AddNode(2, SimRedes.Network.DeviceType.PC, new Vector2(100, 0));
            topology.AddLink(1, 2);

            // Act
            topology.RemoveNode(1);

            // Assert
            Assert.IsNull(topology.GetNode(1));
            Assert.AreEqual(0, topology.GetAllLinks().Count,
                "Removing a node should remove all associated links");
        }

        // ================================================================
        // NODE RETRIEVAL
        // ================================================================

        [Test]
        public void GetNode_Existing_ReturnsNode()
        {
            // Arrange
            topology.AddNode(42, SimRedes.Network.DeviceType.Router, Vector2.zero);

            // Act
            var node = topology.GetNode(42);

            // Assert
            Assert.IsNotNull(node);
            Assert.AreEqual(42, node.DiscId);
        }

        [Test]
        public void GetNode_NonExistent_ReturnsNull()
        {
            // Act
            var node = topology.GetNode(999);

            // Assert
            Assert.IsNull(node);
        }

        [Test]
        public void GetAllNodes_ReturnsCopy()
        {
            // Arrange
            topology.AddNode(1, SimRedes.Network.DeviceType.Router, Vector2.zero);

            // Act
            var nodes = topology.GetAllNodes();
            nodes.Clear();

            // Assert
            Assert.AreEqual(0, nodes.Count, "Cleared copy should be empty");
            Assert.AreEqual(1, topology.GetAllNodes().Count,
                "Original should still have 1 node after clearing the copy");
        }

        // ================================================================
        // LINK CREATION
        // ================================================================

        [Test]
        public void AddLink_ExistingNodes_CreatesLink()
        {
            // Arrange
            topology.AddNode(1, SimRedes.Network.DeviceType.Router, Vector2.zero);
            topology.AddNode(2, SimRedes.Network.DeviceType.PC, new Vector2(100, 0));

            // Act
            topology.AddLink(1, 2);

            // Assert
            var links = topology.GetAllLinks();
            Assert.AreEqual(1, links.Count);
            Assert.AreEqual(1, links[0].SourceNode.DiscId);
            Assert.AreEqual(2, links[0].DestinationNode.DiscId);
        }

        [Test]
        public void AddLink_DuplicateLink_DoesNotDuplicate()
        {
            // Arrange
            topology.AddNode(1, SimRedes.Network.DeviceType.Router, Vector2.zero);
            topology.AddNode(2, SimRedes.Network.DeviceType.PC, new Vector2(100, 0));

            // Act
            topology.AddLink(1, 2);
            topology.AddLink(1, 2);
            topology.AddLink(2, 1); // Same link reversed

            // Assert
            Assert.AreEqual(1, topology.GetAllLinks().Count);
        }

        [Test]
        public void AddLink_NonExistentSource_DoesNothing()
        {
            // Arrange
            topology.AddNode(2, SimRedes.Network.DeviceType.PC, new Vector2(100, 0));

            // Act
            topology.AddLink(1, 2);

            // Assert
            Assert.AreEqual(0, topology.GetAllLinks().Count);
        }

        // ================================================================
        // LINK REMOVAL
        // ================================================================

        [Test]
        public void RemoveLink_Existing_RemovesLink()
        {
            // Arrange
            topology.AddNode(1, SimRedes.Network.DeviceType.Router, Vector2.zero);
            topology.AddNode(2, SimRedes.Network.DeviceType.PC, new Vector2(100, 0));
            topology.AddLink(1, 2);

            // Act
            topology.RemoveLink(1, 2);

            // Assert
            Assert.AreEqual(0, topology.GetAllLinks().Count);
        }

        [Test]
        public void RemoveLink_NonExistent_DoesNothing()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => topology.RemoveLink(1, 2));
            Assert.AreEqual(0, topology.GetAllLinks().Count);
        }

        // ================================================================
        // LINK RETRIEVAL
        // ================================================================

        [Test]
        public void GetAllLinks_ReturnsReference()
        {
            // Arrange
            topology.AddNode(1, SimRedes.Network.DeviceType.Router, Vector2.zero);
            topology.AddNode(2, SimRedes.Network.DeviceType.PC, new Vector2(100, 0));
            topology.AddLink(1, 2);

            // Act: Get the link list and clear it
            var links = topology.GetAllLinks();
            links.Clear();

            // Assert: The internal state should also be affected (it's a reference)
            Assert.AreEqual(0, links.Count);
            Assert.AreEqual(0, topology.GetAllLinks().Count,
                "GetAllLinks returns the internal reference, not a copy");
        }

        // ================================================================
        // NODE POSITION
        // ================================================================

        [Test]
        public void UpdateNodePosition_Existing_UpdatesPosition()
        {
            // Arrange
            topology.AddNode(1, SimRedes.Network.DeviceType.PC, Vector2.zero);

            // Act
            topology.UpdateNodePosition(1, new Vector2(200, 300));

            // Assert
            var node = topology.GetNode(1);
            Assert.AreEqual(new Vector2(200, 300), node.Position);
        }

        [Test]
        public void UpdateNodePosition_NonExistent_DoesNothing()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => topology.UpdateNodePosition(999, new Vector2(200, 300)));
        }

        [Test]
        public void FindNodesNear_ReturnsNodesWithinRadius()
        {
            // Arrange
            topology.AddNode(1, SimRedes.Network.DeviceType.PC, new Vector2(0, 0));
            topology.AddNode(2, SimRedes.Network.DeviceType.PC, new Vector2(50, 0));
            topology.AddNode(3, SimRedes.Network.DeviceType.PC, new Vector2(200, 0));

            // Act
            var nearby = topology.FindNodesNear(Vector2.zero, 100f);

            // Assert
            Assert.AreEqual(2, nearby.Count,
                "Should find nodes within 100 units of origin");
            Assert.IsTrue(nearby.Exists(n => n.DiscId == 1));
            Assert.IsTrue(nearby.Exists(n => n.DiscId == 2));
        }

        [Test]
        public void FindNodesNear_EmptyRadius_ReturnsExactMatch()
        {
            // Arrange
            topology.AddNode(1, SimRedes.Network.DeviceType.PC, new Vector2(0, 0));
            topology.AddNode(2, SimRedes.Network.DeviceType.PC, new Vector2(50, 0));

            // Act
            var exact = topology.FindNodesNear(new Vector2(50, 0), 0f);

            // Assert
            Assert.AreEqual(1, exact.Count);
            Assert.AreEqual(2, exact[0].DiscId);
        }

        // ================================================================
        // PATH FINDING
        // ================================================================

        [Test]
        public void FindPath_ConnectedNodes_ReturnsPath()
        {
            // Arrange: chain PC1(1) ↔ Router(2) ↔ PC3(3)
            topology.AddNode(1, SimRedes.Network.DeviceType.PC, new Vector2(0, 0));
            topology.AddNode(2, SimRedes.Network.DeviceType.Router, new Vector2(100, 0));
            topology.AddNode(3, SimRedes.Network.DeviceType.PC, new Vector2(200, 0));
            topology.AddLink(1, 2);
            topology.AddLink(2, 3);

            // Act
            var path = topology.FindPath(1, 3);

            // Assert
            Assert.AreEqual(3, path.Count, "Path should have 3 nodes: PC1 → Router → PC3");
            Assert.AreEqual(1, path[0].DiscId, "First node should be PC1");
            Assert.AreEqual(2, path[1].DiscId, "Middle node should be Router");
            Assert.AreEqual(3, path[2].DiscId, "Last node should be PC3");
        }

        [Test]
        public void FindPath_DisconnectedNodes_ReturnsEmpty()
        {
            // Arrange: two nodes with no link
            topology.AddNode(1, SimRedes.Network.DeviceType.PC, new Vector2(0, 0));
            topology.AddNode(2, SimRedes.Network.DeviceType.PC, new Vector2(200, 0));

            // Act
            var path = topology.FindPath(1, 2);

            // Assert
            Assert.IsEmpty(path, "Disconnected nodes should return empty path");
        }

        [Test]
        public void FindPath_SingleNode_ReturnsThatNode()
        {
            // Arrange
            topology.AddNode(1, SimRedes.Network.DeviceType.Router, Vector2.zero);

            // Act: path from node to itself
            var path = topology.FindPath(1, 1);

            // Assert
            Assert.AreEqual(1, path.Count);
            Assert.AreEqual(1, path[0].DiscId);
        }

        // ================================================================
        // CONNECTIVITY
        // ================================================================

        [Test]
        public void CheckConnectivity_SameSubnetWithLink_ReturnsTrue()
        {
            // Arrange
            topology.AddNode(1, SimRedes.Network.DeviceType.PC, Vector2.zero);
            topology.AddNode(2, SimRedes.Network.DeviceType.PC, new Vector2(100, 0));
            topology.AddLink(1, 2);
            var pc1 = topology.GetNode(1);
            var pc2 = topology.GetNode(2);
            pc1.IpAddress = "192.168.1.10";
            pc1.SubnetMask = "255.255.255.0";
            pc2.IpAddress = "192.168.1.20";
            pc2.SubnetMask = "255.255.255.0";

            // Act
            bool result = topology.CheckConnectivity(1, 2);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void CheckConnectivity_DifferentSubnet_ReturnsFalse()
        {
            // Arrange
            topology.AddNode(1, SimRedes.Network.DeviceType.PC, Vector2.zero);
            topology.AddNode(2, SimRedes.Network.DeviceType.PC, new Vector2(100, 0));
            topology.AddLink(1, 2);
            var pc1 = topology.GetNode(1);
            var pc2 = topology.GetNode(2);
            pc1.IpAddress = "192.168.1.10";
            pc1.SubnetMask = "255.255.255.0";
            pc2.IpAddress = "10.0.0.20";
            pc2.SubnetMask = "255.0.0.0";

            // Act
            bool result = topology.CheckConnectivity(1, 2);

            // Assert
            Assert.IsFalse(result,
                "PCs in different subnets should not be reachable without routing");
        }

        [Test]
        public void CheckConnectivity_DisconnectedNodes_ReturnsFalse()
        {
            // Arrange: same subnet but no link
            topology.AddNode(1, SimRedes.Network.DeviceType.PC, Vector2.zero);
            topology.AddNode(2, SimRedes.Network.DeviceType.PC, new Vector2(100, 0));
            var pc1 = topology.GetNode(1);
            var pc2 = topology.GetNode(2);
            pc1.IpAddress = "192.168.1.10";
            pc1.SubnetMask = "255.255.255.0";
            pc2.IpAddress = "192.168.1.20";
            pc2.SubnetMask = "255.255.255.0";

            // Act
            bool result = topology.CheckConnectivity(1, 2);

            // Assert
            Assert.IsFalse(result,
                "Disconnected nodes should not be reachable");
        }

        [Test]
        public void CheckConnectivity_SwitchIsTransparent()
        {
            // Arrange: PC1(1) ↔ Switch(2) ↔ PC3(3), PCs on same subnet, switch has no IP
            topology.AddNode(1, SimRedes.Network.DeviceType.PC, Vector2.zero);
            topology.AddNode(2, SimRedes.Network.DeviceType.Switch, new Vector2(100, 0));
            topology.AddNode(3, SimRedes.Network.DeviceType.PC, new Vector2(200, 0));
            topology.AddLink(1, 2);
            topology.AddLink(2, 3);
            var pc1 = topology.GetNode(1);
            var pc3 = topology.GetNode(3);
            pc1.IpAddress = "192.168.1.10";
            pc1.SubnetMask = "255.255.255.0";
            pc3.IpAddress = "192.168.1.20";
            pc3.SubnetMask = "255.255.255.0";
            // Switch has no IP

            // Act
            bool result = topology.CheckConnectivity(1, 3);

            // Assert
            Assert.IsTrue(result,
                "Switch should be transparent to L3 connectivity between same-subnet PCs");
        }

        // ================================================================
        // ROUTING TABLE SUMMARY
        // ================================================================

        [Test]
        public void GetRoutingTableSummary_ReturnsFormattedTable()
        {
            // Arrange: Create a router with a static route
            topology.AddNode(1, SimRedes.Network.DeviceType.Router, Vector2.zero);
            var router = topology.GetNode(1);
            router.RoutingTable.AddStaticRoute("192.168.1.0", "255.255.255.0", "10.0.0.1", "G0/0");

            // Act
            var summary = RoutingSimulator.GetRoutingTableSummary(router);

            // Assert
            StringAssert.Contains("192.168.1.0", summary,
                "Summary should contain the destination network");
            StringAssert.Contains("10.0.0.1", summary,
                "Summary should contain the next hop");
            StringAssert.Contains("G0/0", summary,
                "Summary should contain the outgoing interface");
            StringAssert.Contains("Static", summary,
                "Summary should contain the protocol type");
            StringAssert.DoesNotContain("Sin tabla", summary,
                "Should not show empty table message");
        }

        [Test]
        public void GetRoutingTableSummary_EmptyTopology_ReturnsEmpty()
        {
            // Arrange: Node with no routes
            topology.AddNode(1, SimRedes.Network.DeviceType.Router, Vector2.zero);
            var router = topology.GetNode(1);

            // Act
            var summary = RoutingSimulator.GetRoutingTableSummary(router);

            // Assert: Empty table should still show the header but no routes
            StringAssert.Contains("Tabla de Enrutamiento", summary,
                "Empty table should still show the header");
            Assert.IsFalse(string.IsNullOrEmpty(summary),
                "Summary should always return a non-null string");
        }

        // ================================================================
        // CLEAR TOPOLOGY
        // ================================================================

        [Test]
        public void ClearTopology_RemovesAllNodesAndLinks()
        {
            // Arrange
            topology.AddNode(1, SimRedes.Network.DeviceType.Router, Vector2.zero);
            topology.AddNode(2, SimRedes.Network.DeviceType.PC, new Vector2(100, 0));
            topology.AddLink(1, 2);

            // Act
            topology.ClearTopology();

            // Assert
            Assert.AreEqual(0, topology.GetAllNodes().Count);
            Assert.AreEqual(0, topology.GetAllLinks().Count);
        }

        [Test]
        public void ClearTopology_EmptyTopology_DoesNothing()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => topology.ClearTopology());
            Assert.AreEqual(0, topology.GetAllNodes().Count);
            Assert.AreEqual(0, topology.GetAllLinks().Count);
        }
    }
}
