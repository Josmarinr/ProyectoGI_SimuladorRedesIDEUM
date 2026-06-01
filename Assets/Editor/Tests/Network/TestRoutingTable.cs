using NUnit.Framework;
using SimRedes.Network;
using UnityEngine;

namespace Tests.EditMode.Network
{
    public class TestRoutingTable
    {
        private NetworkNode CreateTestRouter(int discId = 1)
        {
            return new NetworkNode(discId, SimRedes.Network.DeviceType.Router, Vector2.zero);
        }

        [Test]
        public void Constructor_EmptyTable_HasNoEntries()
        {
            var router = CreateTestRouter();
            var table = router.RoutingTable;

            Assert.AreEqual(0, table.GetAllEntries().Count);
        }

        [Test]
        public void AddStaticRoute_AddsEntryWithCorrectFields()
        {
            var router = CreateTestRouter();
            var table = router.RoutingTable;

            table.AddStaticRoute("10.0.0.0", "255.0.0.0", "192.168.1.1", "G0/0");

            var entries = table.GetAllEntries();
            Assert.AreEqual(1, entries.Count);

            var entry = entries[0];
            Assert.AreEqual("10.0.0.0", entry.DestinationNetwork);
            Assert.AreEqual("255.0.0.0", entry.SubnetMask);
            Assert.AreEqual("192.168.1.1", entry.NextHop);
            Assert.AreEqual("G0/0", entry.OutInterface);
            Assert.AreEqual(0, entry.Metric);
            Assert.AreEqual("Static", entry.Protocol);
        }

        [Test]
        public void AddStaticRoute_MultipleRoutes_AllStored()
        {
            var router = CreateTestRouter();
            var table = router.RoutingTable;

            table.AddStaticRoute("10.0.0.0", "255.0.0.0", "10.0.0.1", "G0/0");
            table.AddStaticRoute("172.16.0.0", "255.240.0.0", "172.16.0.1", "G0/1");
            table.AddStaticRoute("192.168.1.0", "255.255.255.0", "192.168.1.1", "G0/2");

            Assert.AreEqual(3, table.GetAllEntries().Count);
        }

        [Test]
        public void AddRipRoute_SetsCorrectDefaults()
        {
            var router = CreateTestRouter();
            var table = router.RoutingTable;

            table.AddRipRoute("10.0.0.0", "10.0.0.1", "G0/0", 3);

            var entry = table.GetAllEntries()[0];
            Assert.AreEqual("10.0.0.0", entry.DestinationNetwork);
            Assert.AreEqual("255.255.255.0", entry.SubnetMask); // default /24
            Assert.AreEqual(3, entry.Metric);
            Assert.AreEqual("RIP", entry.Protocol);
        }

        [Test]
        public void AddOspfRoute_SetsCorrectDefaults()
        {
            var router = CreateTestRouter();
            var table = router.RoutingTable;

            table.AddOspfRoute("10.0.0.0", "10.0.0.1", "G0/0", 20);

            var entry = table.GetAllEntries()[0];
            Assert.AreEqual("10.0.0.0", entry.DestinationNetwork);
            Assert.AreEqual("255.255.255.0", entry.SubnetMask);
            Assert.AreEqual(20, entry.Metric);
            Assert.AreEqual("OSPF", entry.Protocol);
        }

        [Test]
        public void FindBestRoute_ExactMatch_ReturnsCorrectEntry()
        {
            var router = CreateTestRouter();
            var table = router.RoutingTable;

            table.AddStaticRoute("0.0.0.0", "0.0.0.0", "192.168.1.254", "G0/0");   // default
            table.AddStaticRoute("10.0.0.0", "255.0.0.0", "10.0.0.1", "G0/1");     // /8
            table.AddStaticRoute("10.1.0.0", "255.255.0.0", "10.1.0.1", "G0/2");   // /16

            var best = table.FindBestRoute("10.1.5.100");
            Assert.IsNotNull(best);
            Assert.AreEqual("10.1.0.0", best.DestinationNetwork); // longest prefix match
        }

        [Test]
        public void FindBestRoute_DefaultRoute_AsFallback()
        {
            var router = CreateTestRouter();
            var table = router.RoutingTable;

            table.AddStaticRoute("0.0.0.0", "0.0.0.0", "192.168.1.254", "G0/0");
            table.AddStaticRoute("10.0.0.0", "255.0.0.0", "10.0.0.1", "G0/1");

            var best = table.FindBestRoute("172.20.1.100");
            Assert.IsNotNull(best);
            Assert.AreEqual("0.0.0.0", best.DestinationNetwork); // falls back to default
        }

        [Test]
        public void FindBestRoute_UsesLowestMetric_WhenSamePrefixLength()
        {
            var router = CreateTestRouter();
            var table = router.RoutingTable;

            // Two routes to same network with different metrics (using public API)
            table.AddOspfRoute("10.0.0.0", "10.0.0.2", "G0/1", 10);
            table.AddRipRoute("10.0.0.0", "10.0.0.1", "G0/0", 5);

            var best = table.FindBestRoute("10.0.0.55");
            Assert.IsNotNull(best);
            Assert.AreEqual("10.0.0.1", best.NextHop); // RIP metric 5 < OSPF metric 10
            Assert.AreEqual(5, best.Metric);
        }

        [Test]
        public void FindBestRoute_NoMatch_ReturnsNull()
        {
            var router = CreateTestRouter();
            var table = router.RoutingTable;

            var best = table.FindBestRoute("10.0.0.1");
            Assert.IsNull(best); // empty table
        }

        [Test]
        public void GetPrefixLength_KnownMasks_ReturnsCorrect()
        {
            var entry = new RoutingEntry("0.0.0.0", "255.255.255.0", null, null, 0, null);
            Assert.AreEqual(24, entry.GetPrefixLength());

            entry.SubnetMask = "255.255.0.0";
            Assert.AreEqual(16, entry.GetPrefixLength());

            entry.SubnetMask = "255.0.0.0";
            Assert.AreEqual(8, entry.GetPrefixLength());

            entry.SubnetMask = "0.0.0.0";
            Assert.AreEqual(0, entry.GetPrefixLength());

            entry.SubnetMask = "255.255.255.252";
            Assert.AreEqual(30, entry.GetPrefixLength());

            entry.SubnetMask = "255.255.255.255";
            Assert.AreEqual(32, entry.GetPrefixLength());
        }

        [Test]
        public void GetPrefixLength_UnknownMask_ReturnsZero()
        {
            var entry = new RoutingEntry("0.0.0.0", "255.255.255.1", null, null, 0, null);
            Assert.AreEqual(0, entry.GetPrefixLength());
        }

        [Test]
        public void GetAllEntries_ReturnsCopy_NotReference()
        {
            var router = CreateTestRouter();
            var table = router.RoutingTable;

            table.AddStaticRoute("10.0.0.0", "255.0.0.0", "10.0.0.1", "G0/0");

            var entries1 = table.GetAllEntries();
            var entries2 = table.GetAllEntries();

            entries1.Clear();

            // Second reference should still have the entry (it's a copy)
            Assert.AreEqual(1, entries2.Count);
        }

        // ================================================================
        // EIGRP Route Tests
        // ================================================================

        [Test]
        public void AddEigrpRoute_SetsCorrectDefaults()
        {
            var router = CreateTestRouter();
            var table = router.RoutingTable;

            table.AddEigrpRoute("10.0.0.0", "10.0.0.1", "G0/0", 128);

            var entry = table.GetAllEntries()[0];
            Assert.AreEqual("10.0.0.0", entry.DestinationNetwork);
            Assert.AreEqual("255.255.255.0", entry.SubnetMask); // default /24
            Assert.AreEqual(128, entry.Metric);
            Assert.AreEqual("EIGRP", entry.Protocol);
        }

        [Test]
        public void AddEigrpRoute_MultipleRoutes_AllStored()
        {
            var router = CreateTestRouter();
            var table = router.RoutingTable;

            table.AddEigrpRoute("10.0.0.0", "10.0.0.1", "G0/0", 128);
            table.AddEigrpRoute("172.16.0.0", "172.16.0.1", "G0/1", 256);
            table.AddEigrpRoute("192.168.1.0", "192.168.1.1", "G0/2", 64);

            Assert.AreEqual(3, table.GetAllEntries().Count);
        }

        [Test]
        public void FindBestRoute_EigrpRoute_LowerMetricSelected()
        {
            var router = CreateTestRouter();
            var table = router.RoutingTable;

            // Two EIGRP routes to same network with different metrics (using public API)
            table.AddEigrpRoute("10.0.0.0", "10.0.0.2", "G0/1", 256);
            table.AddEigrpRoute("10.0.0.0", "10.0.0.1", "G0/0", 64);

            var best = table.FindBestRoute("10.0.0.1");
            Assert.IsNotNull(best);
            Assert.AreEqual("10.0.0.1", best.NextHop); // lower metric (64 < 256)
            Assert.AreEqual(64, best.Metric);
        }

        [Test]
        public void FindBestRoute_RespectsLongestPrefix_AcrossProtocols()
        {
            var router = CreateTestRouter();
            var table = router.RoutingTable;

            // /16 route via Static (explicit mask)
            table.AddStaticRoute("10.0.0.0", "255.255.0.0", "10.0.0.2", "G0/1");
            // More specific /24 route via EIGRP (default mask)
            table.AddEigrpRoute("10.0.0.0", "10.0.0.1", "G0/0", 128);

            // 10.0.0.55 matches both /16 and /24. /24 should win.
            var best = table.FindBestRoute("10.0.0.55");
            Assert.IsNotNull(best);
            Assert.AreEqual("EIGRP", best.Protocol); // /24 wins over /16 regardless of protocol or metric
        }

        [Test]
        public void Clear_RemovesAllEntries()
        {
            var router = CreateTestRouter();
            var table = router.RoutingTable;

            table.AddStaticRoute("10.0.0.0", "255.0.0.0", "10.0.0.1", "G0/0");
            table.Clear();

            Assert.AreEqual(0, table.GetAllEntries().Count);
        }

        [Test]
        public void GetTableSummary_ReturnsSummary()
        {
            var router = CreateTestRouter();
            var table = router.RoutingTable;

            Assert.AreEqual("Entradas: 0", table.GetTableSummary());

            table.AddStaticRoute("10.0.0.0", "255.0.0.0", "10.0.0.1", "G0/0");
            Assert.AreEqual("Entradas: 1", table.GetTableSummary());
        }
    }
}
