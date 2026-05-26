using NUnit.Framework;
using SimRedes.Network;

namespace Tests.EditMode.Network
{
    public class TestIPValidation
    {
        [Test]
        public void IsValidIP_ValidIPv4_ReturnsTrue()
        {
            Assert.IsTrue(IPValidation.IsValidIP("192.168.1.1"));
            Assert.IsTrue(IPValidation.IsValidIP("10.0.0.1"));
            Assert.IsTrue(IPValidation.IsValidIP("172.16.0.1"));
            Assert.IsTrue(IPValidation.IsValidIP("0.0.0.0"));
            Assert.IsTrue(IPValidation.IsValidIP("255.255.255.255"));
        }

        [Test]
        public void IsValidIP_InvalidFormat_ReturnsFalse()
        {
            Assert.IsFalse(IPValidation.IsValidIP(null));
            Assert.IsFalse(IPValidation.IsValidIP(""));
            Assert.IsFalse(IPValidation.IsValidIP("not-an-ip"));
            Assert.IsFalse(IPValidation.IsValidIP("192.168.1"));
            Assert.IsFalse(IPValidation.IsValidIP("192.168.1.1.1"));
            Assert.IsFalse(IPValidation.IsValidIP("192.168.1.256"));
            Assert.IsFalse(IPValidation.IsValidIP("192.168.1.-1"));
            Assert.IsFalse(IPValidation.IsValidIP("192.168.1.abc"));
        }

        [Test]
        public void IsValidSubnetMask_ValidMasks_ReturnsTrue()
        {
            Assert.IsTrue(IPValidation.IsValidSubnetMask("255.255.255.0"));
            Assert.IsTrue(IPValidation.IsValidSubnetMask("255.255.0.0"));
            Assert.IsTrue(IPValidation.IsValidSubnetMask("255.0.0.0"));
            Assert.IsTrue(IPValidation.IsValidSubnetMask("0.0.0.0"));
            Assert.IsTrue(IPValidation.IsValidSubnetMask("128.0.0.0"));
            Assert.IsTrue(IPValidation.IsValidSubnetMask("255.255.255.252"));
            Assert.IsTrue(IPValidation.IsValidSubnetMask("255.255.255.254"));
            Assert.IsTrue(IPValidation.IsValidSubnetMask("255.255.255.255"));
        }

        [Test]
        public void IsValidSubnetMask_InvalidMasks_ReturnsFalse()
        {
            Assert.IsFalse(IPValidation.IsValidSubnetMask(null));
            Assert.IsFalse(IPValidation.IsValidSubnetMask(""));
            Assert.IsFalse(IPValidation.IsValidSubnetMask("255.255.255.1"));    // non-CIDR (255.255.255.128 es /25, válido)
        }

        [Test]
        public void GetPrefixLength_KnownMasks_ReturnsCorrect()
        {
            Assert.AreEqual(0, IPValidation.GetPrefixLength("0.0.0.0"));
            Assert.AreEqual(8, IPValidation.GetPrefixLength("255.0.0.0"));
            Assert.AreEqual(16, IPValidation.GetPrefixLength("255.255.0.0"));
            Assert.AreEqual(24, IPValidation.GetPrefixLength("255.255.255.0"));
            Assert.AreEqual(30, IPValidation.GetPrefixLength("255.255.255.252"));
            Assert.AreEqual(32, IPValidation.GetPrefixLength("255.255.255.255"));
        }

        [Test]
        public void GetPrefixLength_InvalidMask_ReturnsZero()
        {
            Assert.AreEqual(0, IPValidation.GetPrefixLength(null));
            Assert.AreEqual(0, IPValidation.GetPrefixLength(""));
            Assert.AreEqual(0, IPValidation.GetPrefixLength("not-a-mask"));
        }

        [Test]
        public void ValidateIPField_ValidIP_ReturnsNull()
        {
            Assert.IsNull(IPValidation.ValidateIPField("192.168.1.1"));
        }

        [Test]
        public void ValidateIPField_InvalidIP_ReturnsError()
        {
            Assert.IsNotNull(IPValidation.ValidateIPField(null));
            Assert.IsNotNull(IPValidation.ValidateIPField(""));
            Assert.IsNotNull(IPValidation.ValidateIPField("bad"));
        }

        [Test]
        public void ValidateMaskField_ValidMask_ReturnsNull()
        {
            Assert.IsNull(IPValidation.ValidateMaskField("255.255.255.0"));
        }

        [Test]
        public void ValidateMaskField_InvalidMask_ReturnsError()
        {
            Assert.IsNotNull(IPValidation.ValidateMaskField(null));
            Assert.IsNotNull(IPValidation.ValidateMaskField(""));
            Assert.IsNotNull(IPValidation.ValidateMaskField("255.255.255.1"));
        }

        [Test]
        public void IsInSameNetwork_SameNetwork_ReturnsTrue()
        {
            Assert.IsTrue(IPValidation.IsInSameNetwork("192.168.1.10", "255.255.255.0", "192.168.1.20"));
            Assert.IsTrue(IPValidation.IsInSameNetwork("10.0.0.5", "255.0.0.0", "10.0.0.100"));
        }

        [Test]
        public void IsInSameNetwork_DifferentNetwork_ReturnsFalse()
        {
            Assert.IsFalse(IPValidation.IsInSameNetwork("192.168.1.10", "255.255.255.0", "192.168.2.20"));
            Assert.IsFalse(IPValidation.IsInSameNetwork("10.0.0.5", "255.0.0.0", "20.0.0.1"));
        }

        [Test]
        public void IsInSameNetwork_InvalidArgs_ReturnsFalse()
        {
            Assert.IsFalse(IPValidation.IsInSameNetwork(null, "255.255.255.0", "192.168.1.1"));
            Assert.IsFalse(IPValidation.IsInSameNetwork("192.168.1.1", null, "192.168.1.2"));
            Assert.IsFalse(IPValidation.IsInSameNetwork("192.168.1.1", "255.255.255.0", null));
        }

        [Test]
        public void GetNetworkAddress_ReturnsCorrect()
        {
            Assert.AreEqual("192.168.1.0", IPValidation.GetNetworkAddress("192.168.1.10", "255.255.255.0"));
            Assert.AreEqual("10.0.0.0", IPValidation.GetNetworkAddress("10.0.0.5", "255.0.0.0"));
            Assert.AreEqual("172.16.0.0", IPValidation.GetNetworkAddress("172.16.1.1", "255.255.0.0"));
        }

        [Test]
        public void GetNetworkAddress_InvalidArgs_ReturnsNull()
        {
            Assert.IsNull(IPValidation.GetNetworkAddress(null, "255.255.255.0"));
            Assert.IsNull(IPValidation.GetNetworkAddress("192.168.1.1", null));
            Assert.IsNull(IPValidation.GetNetworkAddress("bad", "255.255.255.0"));
        }

        [Test]
        public void GetBroadcastAddress_ReturnsCorrect()
        {
            Assert.AreEqual("192.168.1.255", IPValidation.GetBroadcastAddress("192.168.1.10", "255.255.255.0"));
            Assert.AreEqual("10.255.255.255", IPValidation.GetBroadcastAddress("10.0.0.5", "255.0.0.0"));
        }

        [Test]
        public void GetGatewayFromIP_ReturnsFirstUsable()
        {
            Assert.AreEqual("192.168.1.1", IPValidation.GetGatewayFromIP("192.168.1.100"));
            Assert.AreEqual("10.0.0.1", IPValidation.GetGatewayFromIP("10.0.0.50"));
            Assert.AreEqual("172.16.0.1", IPValidation.GetGatewayFromIP("172.16.0.254"));
        }

        [Test]
        public void GetGatewayFromIP_InvalidIP_ReturnsNull()
        {
            Assert.IsNull(IPValidation.GetGatewayFromIP(null));
            Assert.IsNull(IPValidation.GetGatewayFromIP("bad"));
        }
    }
}
