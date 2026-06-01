using NUnit.Framework;
using SimRedes.Network;
using UnityEngine;

namespace Tests.EditMode.Network
{
    public class TestVLANManager
    {
        private VLANManager vlan;
        private NetworkNode node1, node2, node3;

        [SetUp]
        public void SetUp()
        {
            vlan = new VLANManager();
            node1 = new NetworkNode(1, SimRedes.Network.DeviceType.PC, Vector2.zero);
            node2 = new NetworkNode(2, SimRedes.Network.DeviceType.PC, Vector2.zero);
            node3 = new NetworkNode(3, SimRedes.Network.DeviceType.PC, Vector2.zero);
        }

        [Test]
        public void Constructor_CreatesDefaultVLAN1()
        {
            var vlans = vlan.GetAllVLANs();
            Assert.Contains(VLANManager.DEFAULT_VLAN, vlans);
            Assert.AreEqual(1, vlans.Count);
        }

        [Test]
        public void CreateVLAN_AutoId_ReturnsNextId()
        {
            int id = vlan.CreateVLAN(0);
            Assert.AreEqual(10, id);
        }

        [Test]
        public void CreateVLAN_AutoId_Increments()
        {
            int id1 = vlan.CreateVLAN(0);
            int id2 = vlan.CreateVLAN(0);
            Assert.AreEqual(10, id1);
            Assert.AreEqual(11, id2);
        }

        [Test]
        public void CreateVLAN_SpecificId_CreatesAndReturnsIt()
        {
            int id = vlan.CreateVLAN(100);
            Assert.AreEqual(100, id);
            var vlans = vlan.GetAllVLANs();
            Assert.Contains(100, vlans);
        }

        [Test]
        public void CreateVLAN_DuplicateId_DoesNotDuplicate()
        {
            vlan.CreateVLAN(100);
            vlan.CreateVLAN(100);
            var vlans = vlan.GetAllVLANs();
            Assert.AreEqual(2, vlans.Count); // VLAN 1 (default) + VLAN 100
        }

        [Test]
        public void DeleteVLAN_DefaultVLAN_DoesNothing()
        {
            vlan.DeleteVLAN(VLANManager.DEFAULT_VLAN);
            var vlans = vlan.GetAllVLANs();
            Assert.Contains(VLANManager.DEFAULT_VLAN, vlans);
            Assert.AreEqual(1, vlans.Count);
        }

        [Test]
        public void DeleteVLAN_ExistingVLAN_RemovesIt()
        {
            vlan.CreateVLAN(100);
            vlan.DeleteVLAN(100);
            var vlans = vlan.GetAllVLANs();
            Assert.IsFalse(vlans.Contains(100));
        }

        [Test]
        public void DeleteVLAN_WithNodes_ReassignsToVLAN1()
        {
            vlan.AssignToVLAN(node1, 100);
            vlan.DeleteVLAN(100);
            Assert.AreEqual(VLANManager.DEFAULT_VLAN, vlan.GetNodeVLAN(node1));
        }

        [Test]
        public void DeleteVLAN_NonExistent_NoException()
        {
            Assert.DoesNotThrow(() => vlan.DeleteVLAN(999));
        }

        [Test]
        public void AssignToVLAN_NodeAssigned_ReturnsVlanId()
        {
            vlan.AssignToVLAN(node1, 10);
            Assert.AreEqual(10, vlan.GetNodeVLAN(node1));
        }

        [Test]
        public void AssignToVLAN_AutoCreatesVLAN()
        {
            vlan.AssignToVLAN(node1, 99);
            var vlans = vlan.GetAllVLANs();
            Assert.Contains(99, vlans);
        }

        [Test]
        public void AssignToVLAN_Reassign_RemovesFromOldVLAN()
        {
            vlan.AssignToVLAN(node1, 10);
            vlan.AssignToVLAN(node1, 20);
            Assert.AreEqual(20, vlan.GetNodeVLAN(node1));
            Assert.AreEqual(0, vlan.GetNodesInVLAN(10).Count);
        }

        [Test]
        public void AssignToVLAN_MultipleNodes_SameVLAN()
        {
            vlan.AssignToVLAN(node1, 10);
            vlan.AssignToVLAN(node2, 10);
            Assert.AreEqual(2, vlan.GetNodesInVLAN(10).Count);
        }

        [Test]
        public void GetNodeVLAN_UnassignedNode_ReturnsDefault()
        {
            var unassigned = new NetworkNode(99, SimRedes.Network.DeviceType.PC, Vector2.zero);
            Assert.AreEqual(VLANManager.DEFAULT_VLAN, vlan.GetNodeVLAN(unassigned));
        }

        [Test]
        public void GetNodesInVLAN_NonExistentVLAN_ReturnsEmpty()
        {
            var nodes = vlan.GetNodesInVLAN(999);
            Assert.AreEqual(0, nodes.Count);
        }

        [Test]
        public void GetNodesInVLAN_ReturnsCopy()
        {
            vlan.AssignToVLAN(node1, 10);
            var nodesCopy = vlan.GetNodesInVLAN(10);
            nodesCopy.Clear();
            Assert.AreEqual(1, vlan.GetNodesInVLAN(10).Count);
        }

        [Test]
        public void CanCommunicate_SameVLAN_ReturnsTrue()
        {
            vlan.AssignToVLAN(node1, 10);
            vlan.AssignToVLAN(node2, 10);
            Assert.IsTrue(vlan.CanCommunicate(node1, node2));
        }

        [Test]
        public void CanCommunicate_DifferentVLAN_ReturnsFalse()
        {
            vlan.AssignToVLAN(node1, 10);
            vlan.AssignToVLAN(node2, 20);
            Assert.IsFalse(vlan.CanCommunicate(node1, node2));
        }
    }
}
