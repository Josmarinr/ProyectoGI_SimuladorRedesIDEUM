using NUnit.Framework;
using SimRedes.Network;

namespace Tests.EditMode.Network
{
    public class TestARPTable
    {
        private ARPTable CreateTable()
        {
            return new ARPTable(new NetworkNode());
        }

        [Test]
        public void Constructor_EmptyTable_HasNoEntries()
        {
            var table = CreateTable();
            var entries = table.GetAllEntries();
            Assert.AreEqual(0, entries.Count);
        }

        [Test]
        public void AddEntry_ThreeParams_AddsEntry()
        {
            var table = CreateTable();
            table.AddEntry("10.0.0.1", "AA:BB:CC:DD:EE:FF", "G0/0");
            var entry = table.FindEntry("10.0.0.1");
            Assert.IsNotNull(entry);
            Assert.AreEqual("10.0.0.1", entry.IPAddress);
            Assert.AreEqual("AA:BB:CC:DD:EE:FF", entry.MACAddress);
            Assert.AreEqual("G0/0", entry.Interface);
        }

        [Test]
        public void AddEntry_NewEntry_HasDynamicTypeAndZeroAge()
        {
            var table = CreateTable();
            table.AddEntry("10.0.0.1", "AA:BB:CC:DD:EE:FF", "G0/0");
            var entry = table.FindEntry("10.0.0.1");
            Assert.AreEqual("dynamic", entry.Type);
            Assert.AreEqual(0f, entry.Age);
        }

        [Test]
        public void AddEntry_DuplicateIP_UpdatesMACAndResetsAge()
        {
            var table = CreateTable();
            table.AddEntry("10.0.0.1", "AA:BB:CC:DD:EE:FF", "G0/0");
            table.AgeEntries(5f);
            table.AddEntry("10.0.0.1", "BB:BB:BB:BB:BB:BB", "G0/1");
            var entry = table.FindEntry("10.0.0.1");
            Assert.AreEqual("BB:BB:BB:BB:BB:BB", entry.MACAddress);
            Assert.AreEqual(0f, entry.Age);
            Assert.AreEqual(1, table.GetAllEntries().Count);
        }

        [Test]
        public void AddEntry_TwoParams_GeneratesRandomMAC()
        {
            var table = CreateTable();
            table.AddEntry("10.0.0.1", "G0/0");
            var entry = table.FindEntry("10.0.0.1");
            Assert.IsNotNull(entry);
            Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(entry.MACAddress, @"^([0-9A-F]{2}:){5}[0-9A-F]{2}$"));
        }

        [Test]
        public void FindEntry_ExistingIP_ReturnsEntry()
        {
            var table = CreateTable();
            table.AddEntry("10.0.0.1", "AA:BB:CC:DD:EE:FF", "G0/0");
            var entry = table.FindEntry("10.0.0.1");
            Assert.IsNotNull(entry);
            Assert.AreEqual("10.0.0.1", entry.IPAddress);
        }

        [Test]
        public void FindEntry_NonExistentIP_ReturnsNull()
        {
            var table = CreateTable();
            table.AddEntry("10.0.0.1", "AA:BB:CC:DD:EE:FF", "G0/0");
            var entry = table.FindEntry("10.0.0.2");
            Assert.IsNull(entry);
        }

        [Test]
        public void FindEntry_NullOrEmpty_ReturnsNull()
        {
            var table = CreateTable();
            Assert.IsNull(table.FindEntry(null));
            Assert.IsNull(table.FindEntry(""));
        }

        [Test]
        public void GetAllEntries_ReturnsCopy_NotReference()
        {
            var table = CreateTable();
            table.AddEntry("10.0.0.1", "AA:BB:CC:DD:EE:FF", "G0/0");
            var entries1 = table.GetAllEntries();
            entries1.Clear();
            var entries2 = table.GetAllEntries();
            Assert.AreEqual(1, entries2.Count);
        }

        [Test]
        public void Clear_RemovesAllEntries()
        {
            var table = CreateTable();
            table.AddEntry("10.0.0.1", "AA:BB:CC:DD:EE:FF", "G0/0");
            table.AddEntry("10.0.0.2", "AA:BB:CC:DD:EE:FF", "G0/0");
            table.Clear();
            Assert.AreEqual(0, table.GetAllEntries().Count);
        }

        [Test]
        public void Clear_EmptyTable_NoException()
        {
            var table = CreateTable();
            Assert.DoesNotThrow(() => table.Clear());
        }

        [Test]
        public void AgeEntries_AccumulatesDeltaTime()
        {
            var table = CreateTable();
            table.AddEntry("10.0.0.1", "AA:BB:CC:DD:EE:FF", "G0/0");
            var entry = table.FindEntry("10.0.0.1");
            table.AgeEntries(2f);
            table.AgeEntries(3f);
            Assert.AreEqual(5f, entry.Age, 0.001);
        }

        [Test]
        public void GetTableSummary_ReturnsCorrectCount()
        {
            var table = CreateTable();
            Assert.AreEqual("Entradas ARP: 0", table.GetTableSummary());
            table.AddEntry("10.0.0.1", "AA:BB:CC:DD:EE:FF", "G0/0");
            Assert.AreEqual("Entradas ARP: 1", table.GetTableSummary());
        }
    }
}
