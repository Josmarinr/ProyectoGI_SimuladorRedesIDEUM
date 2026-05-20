using UnityEngine;

namespace SimRedes.Network
{
    public enum LinkType
    {
        Ethernet,
        Serial,
        Wireless,
        Fiber
    }

    public class NetworkLink
    {
        public string Id { get; set; }
        public NetworkNode SourceNode { get; set; }
        public NetworkNode DestinationNode { get; set; }
        public string SourceInterface { get; set; }
        public string DestinationInterface { get; set; }
        public LinkType Type { get; set; }
        public int Bandwidth { get; set; }
        public int Delay { get; set; }
        public int Cost { get; set; }
        public bool IsActive { get; set; }
        public bool IsConnected { get; set; }

        public NetworkLink()
        {
            Bandwidth = 1000;
            Delay = 10;
            Cost = 1;
            IsActive = true;
            IsConnected = true;
            Type = LinkType.Ethernet;
        }

        public NetworkLink(NetworkNode source, NetworkNode dest, string srcInt, string dstInt) : this()
        {
            SourceNode = source;
            DestinationNode = dest;
            SourceInterface = srcInt;
            DestinationInterface = dstInt;
            Id = $"{source.Id}-{dest.Id}";
        }

        public void SetFault(string faultType)
        {
            switch (faultType)
            {
                case "cable_desconectado":
                    IsConnected = false;
                    break;
                case "interfaz_down":
                    IsActive = false;
                    break;
            }
        }

        public void ClearFault()
        {
            IsConnected = true;
            IsActive = true;
        }

        public bool IsFunctional()
        {
            return IsActive && IsConnected && SourceNode.IsActive && DestinationNode.IsActive;
        }
    }
}