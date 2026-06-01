using NUnit.Framework;
using SimRedes.Network;

namespace Tests.EditMode.Network
{
    public class TestNATManager
    {
        private NATManager nat;
        private const string PUBLIC_IP = "200.100.50.1";

        [SetUp]
        public void SetUp()
        {
            nat = new NATManager();
        }

        [Test]
        public void Constructor_DefaultValues()
        {
            Assert.IsFalse(nat.IsEnabled);
            Assert.AreEqual("192.168.1.1", nat.GetRouterIP());
            // Verify public IP via summary
            var summary = nat.GetNATSummary();
            StringAssert.Contains("200.100.50.1", summary);
            StringAssert.Contains("Entries: 0", summary);
        }

        [Test]
        public void SetPublicIP_UpdatesIP()
        {
            nat.SetPublicIP("10.0.0.1");
            var summary = nat.GetNATSummary();
            StringAssert.Contains("10.0.0.1", summary);
            StringAssert.DoesNotContain("200.100.50.1", summary);
        }

        [Test]
        public void SetRouterIP_GetRouterIP_Roundtrip()
        {
            nat.SetRouterIP("10.0.0.254");
            Assert.AreEqual("10.0.0.254", nat.GetRouterIP());
        }

        [Test]
        public void AddStaticNAT_AddsEntry()
        {
            nat.AddStaticNAT("10.0.0.1", "200.100.50.10");
            var table = nat.GetNATTable();
            Assert.AreEqual(1, table.Count);
            Assert.AreEqual("10.0.0.1", table[0].InternalIP);
            Assert.AreEqual("200.100.50.10", table[0].ExternalIP);
            Assert.AreEqual(NATType.Static, table[0].Type);
        }

        [Test]
        public void AddDynamicNAT_AddsEntry()
        {
            nat.AddDynamicNAT("10.0.0.1");
            var table = nat.GetNATTable();
            Assert.AreEqual(1, table.Count);
            Assert.AreEqual("10.0.0.1", table[0].InternalIP);
            Assert.AreEqual(NATType.Dynamic, table[0].Type);
        }

        [Test]
        public void AddPAT_AddsEntryWithPort()
        {
            nat.AddPAT("10.0.0.1", 80);
            var table = nat.GetNATTable();
            Assert.AreEqual(1, table.Count);
            Assert.AreEqual("10.0.0.1", table[0].InternalIP);
            Assert.AreEqual(80, table[0].InternalPort);
            Assert.AreEqual(NATType.PAT, table[0].Type);
        }

        [Test]
        public void LookupInternal_WithPort_Matching()
        {
            nat.AddPAT("10.0.0.1", 80);
            var entry = nat.LookupInternal("10.0.0.1", 80);
            Assert.IsNotNull(entry);
            Assert.AreEqual(80, entry.InternalPort);
        }

        [Test]
        public void LookupInternal_WithoutPort_ReturnsFirst()
        {
            nat.AddStaticNAT("10.0.0.1", "200.100.50.10");
            nat.AddPAT("10.0.0.1", 80);
            var entry = nat.LookupInternal("10.0.0.1");
            Assert.IsNotNull(entry);
            Assert.AreEqual(NATType.Static, entry.Type);
        }

        [Test]
        public void LookupInternal_NonExistent_ReturnsNull()
        {
            var entry = nat.LookupInternal("10.0.0.99");
            Assert.IsNull(entry);
        }

        [Test]
        public void LookupExternal_ExistingIP_ReturnsEntry()
        {
            nat.AddStaticNAT("10.0.0.1", "200.100.50.10");
            var entry = nat.LookupExternal("200.100.50.10");
            Assert.IsNotNull(entry);
            Assert.AreEqual("10.0.0.1", entry.InternalIP);
        }

        [Test]
        public void LookupExternal_NonExistent_ReturnsNull()
        {
            var entry = nat.LookupExternal("1.2.3.4");
            Assert.IsNull(entry);
        }

        [Test]
        public void TranslatePacket_Outgoing_Match_ReturnsTrue()
        {
            nat.AddStaticNAT("10.0.0.1", "200.100.50.10");
            Assert.IsTrue(nat.TranslatePacket("10.0.0.1", "8.8.8.8", isOutgoing: true));
        }

        [Test]
        public void TranslatePacket_Outgoing_NoMatch_ReturnsFalse()
        {
            Assert.IsFalse(nat.TranslatePacket("10.0.0.99", "8.8.8.8", isOutgoing: true));
        }

        [Test]
        public void TranslatePacket_Incoming_WithPort_Match()
        {
            nat.AddPAT("10.0.0.1", 80);
            var table = nat.GetNATTable();
            int extPort = table[0].ExternalPort.Value;
            Assert.IsTrue(nat.TranslatePacket("8.8.8.8", PUBLIC_IP, destPort: extPort, isOutgoing: false));
        }

        [Test]
        public void TranslatePacket_Incoming_Static_Match()
        {
            nat.AddStaticNAT("10.0.0.1", "200.100.50.10");
            Assert.IsTrue(nat.TranslatePacket("8.8.8.8", "200.100.50.10", isOutgoing: false));
        }

        [Test]
        public void TranslatePacket_Incoming_NoMatch_ReturnsFalse()
        {
            Assert.IsFalse(nat.TranslatePacket("8.8.8.8", "1.2.3.4", isOutgoing: false));
        }

        [Test]
        public void RemoveEntry_RemovesFromTable()
        {
            nat.AddStaticNAT("10.0.0.1", "200.100.50.10");
            var entry = nat.GetNATTable()[0];
            nat.RemoveEntry(entry);
            Assert.AreEqual(0, nat.GetNATTable().Count);
        }

        [Test]
        public void ClearNAT_EmptiesTable()
        {
            nat.AddStaticNAT("10.0.0.1", "200.100.50.10");
            nat.AddDynamicNAT("10.0.0.2");
            nat.ClearNAT();
            Assert.AreEqual(0, nat.GetNATTable().Count);
        }

        [Test]
        public void GetNATTable_ReturnsCopy()
        {
            nat.AddStaticNAT("10.0.0.1", "200.100.50.10");
            var copy1 = nat.GetNATTable();
            copy1.Clear();
            Assert.AreEqual(1, nat.GetNATTable().Count);
        }

        [Test]
        public void TranslateInternalToExternal_Disabled_ReturnsSame()
        {
            nat.IsEnabled = false;
            string result = nat.TranslateInternalToExternal("10.0.0.1");
            Assert.AreEqual("10.0.0.1", result);
        }

        [Test]
        public void TranslateInternalToExternal_Static_ReturnsExternal()
        {
            nat.IsEnabled = true;
            nat.AddStaticNAT("10.0.0.1", "200.100.50.10");
            string result = nat.TranslateInternalToExternal("10.0.0.1");
            Assert.AreEqual("200.100.50.10", result);
        }

        [Test]
        public void TranslateInternalToExternal_PAT_ReturnsWithPort()
        {
            nat.IsEnabled = true;
            nat.AddPAT("10.0.0.1", 80);
            string result = nat.TranslateInternalToExternal("10.0.0.1");
            StringAssert.StartsWith(PUBLIC_IP, result);
            StringAssert.Contains(":", result);
        }

        [Test]
        public void TranslateInternalToExternal_Dynamic_AutoCreates()
        {
            nat.IsEnabled = true;
            string result = nat.TranslateInternalToExternal("10.0.0.1");
            StringAssert.StartsWith(PUBLIC_IP, result);
            StringAssert.Contains(":", result);
            Assert.AreEqual(1, nat.GetNATTable().Count);
        }

        [Test]
        public void TranslateExternalToInternal_Disabled_ReturnsSame()
        {
            nat.IsEnabled = false;
            string result = nat.TranslateExternalToInternal("200.100.50.10");
            Assert.AreEqual("200.100.50.10", result);
        }

        [Test]
        public void TranslateExternalToInternal_Static_ReturnsInternal()
        {
            nat.IsEnabled = true;
            nat.AddStaticNAT("10.0.0.1", "200.100.50.10");
            string result = nat.TranslateExternalToInternal("200.100.50.10");
            Assert.AreEqual("10.0.0.1", result);
        }
    }
}
