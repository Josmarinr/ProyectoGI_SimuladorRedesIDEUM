using System;
using System.Collections.Generic;
using System.Linq;

namespace SimRedes.Network
{
    public class ARPEntry
    {
        public string IPAddress { get; set; }
        public string MACAddress { get; set; }
        public string Interface { get; set; }
        public string Type { get; set; }
        public float Age { get; set; }

        public ARPEntry(string ip, string mac, string iface)
        {
            IPAddress = ip;
            MACAddress = mac;
            Interface = iface;
            Type = "dynamic";
            Age = 0f;
        }
    }

    public class ARPTable
    {
        private List<ARPEntry> entries = new List<ARPEntry>();
        private NetworkNode router;
        private System.Random random = new System.Random();

        public ARPTable(NetworkNode node)
        {
            router = node;
        }

        public void AddEntry(string ip, string mac, string iface)
        {
            var existing = entries.FirstOrDefault(e => e.IPAddress == ip);
            if (existing != null)
            {
                existing.MACAddress = mac;
                existing.Age = 0f;
            }
            else
            {
                entries.Add(new ARPEntry(ip, mac, iface));
            }
        }

        public void AddEntry(string ip, string iface)
        {
            string mac = GenerateRandomMAC();
            AddEntry(ip, mac, iface);
        }

        public ARPEntry FindEntry(string ip)
        {
            return entries.FirstOrDefault(e => e.IPAddress == ip);
        }

        public List<ARPEntry> GetAllEntries()
        {
            return new List<ARPEntry>(entries);
        }

        public void Clear()
        {
            entries.Clear();
        }

        public void AgeEntries(float deltaTime)
        {
            foreach (var entry in entries)
            {
                entry.Age += deltaTime;
            }
        }

        public string GetTableSummary()
        {
            return $"Entradas ARP: {entries.Count}";
        }

        private string GenerateRandomMAC()
        {
            byte[] mac = new byte[6];
            random.NextBytes(mac);
            mac[0] = (byte)(mac[0] & 0xFE);
            return string.Join(":", mac.Select(b => b.ToString("X2")).ToArray());
        }
    }
}
