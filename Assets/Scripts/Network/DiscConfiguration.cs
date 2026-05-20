using UnityEngine;

namespace SimRedes.Network
{
    public enum DiscType
    {
        Router,
        Switch,
        PC,
        Enlace,
        Fallo,
        Protocolo,
        RedDestino,
        Metrica,
        InterfazSalida,
        ModoEnrutamiento,
        IpRoute,
        Destino,
        Mascara,
        ProximoSalto,
        Vecino,
        AnunciarRed,
        Costo,
        BW
    }

    public class DiscConfiguration
    {
        public int DiscId { get; set; }
        public DiscType Type { get; set; }
        public string Label { get; set; }
        public Color DisplayColor { get; set; }
        public string Description { get; set; }

        public static DiscConfiguration[] DefaultConfiguration = new DiscConfiguration[]
        {
            new DiscConfiguration { DiscId = 1, Type = DiscType.Router, Label = "Router", DisplayColor = Color.blue, Description = "Dispositivo de enrutamiento" },
            new DiscConfiguration { DiscId = 2, Type = DiscType.Switch, Label = "Switch", DisplayColor = Color.cyan, Description = "Dispositivo de conmutación" },
            new DiscConfiguration { DiscId = 3, Type = DiscType.PC, Label = "PC", DisplayColor = Color.green, Description = "Host final" },
            new DiscConfiguration { DiscId = 4, Type = DiscType.Enlace, Label = "Enlace", DisplayColor = Color.yellow, Description = "Conexión entre dispositivos" },
            new DiscConfiguration { DiscId = 5, Type = DiscType.Fallo, Label = "Fallo", DisplayColor = Color.red, Description = "Simulación de errores" },
            new DiscConfiguration { DiscId = 6, Type = DiscType.Protocolo, Label = "Protocolo", DisplayColor = Color.magenta,   Description = "Protocolo de enrutamiento" },
            new DiscConfiguration { DiscId = 7, Type = DiscType.RedDestino, Label = "Red Destino", DisplayColor = new Color(1, 0.5f, 0), Description = "Red de destino para ruta" },
            new DiscConfiguration { DiscId = 8, Type = DiscType.Metrica, Label = "Metrica", DisplayColor = new Color(1, 0.84f, 0), Description = "Valor de metrica" },
            new DiscConfiguration { DiscId = 9, Type = DiscType.InterfazSalida, Label = "Interfaz Salida", DisplayColor = new Color(0.5f, 0.8f, 1), Description = "Interfaz de salida" },
            new DiscConfiguration { DiscId = 10, Type = DiscType.ModoEnrutamiento, Label = "Modo Enrutamiento", DisplayColor = new Color(1, 0.6f, 0.8f), Description = "Modo de enrutamiento" },
            new DiscConfiguration { DiscId = 11, Type = DiscType.IpRoute, Label = "IP Route", DisplayColor = new Color(0.6f, 0.9f, 0.6f), Description = "Comando ip route" },
            new DiscConfiguration { DiscId = 12, Type = DiscType.Destino, Label = "Destino", DisplayColor = new Color(0.8f, 0.6f, 1), Description = "IP de destino" },
            new DiscConfiguration { DiscId = 13, Type = DiscType.Mascara, Label = "Mascara", DisplayColor = new Color(0.4f, 0.8f, 1), Description = "Mascara de subred" },
            new DiscConfiguration { DiscId = 14, Type = DiscType.ProximoSalto, Label = "Proximo Salto", DisplayColor = new Color(0.2f, 0.9f, 0.7f), Description = "Siguiente salto" },
            new DiscConfiguration { DiscId = 15, Type = DiscType.Vecino, Label = "Vecino", DisplayColor = new Color(1, 0.7f, 0.4f), Description = "Router vecino" },
            new DiscConfiguration { DiscId = 16, Type = DiscType.AnunciarRed, Label = "Anunciar Red", DisplayColor = new Color(1, 0.5f, 0.5f), Description = "Red a anunciar" },
            new DiscConfiguration { DiscId = 17, Type = DiscType.Costo, Label = "Costo", DisplayColor = new Color(0.5f, 1, 0.5f), Description = "Costo del enlace" },
            new DiscConfiguration { DiscId = 18, Type = DiscType.BW, Label = "Ancho Banda", DisplayColor = new Color(0.5f, 0.7f, 1), Description = "Ancho de banda" }
        };

        public static DiscConfiguration GetConfiguration(int discId)
        {
            foreach (var config in DefaultConfiguration)
            {
                if (config.DiscId == discId)
                    return config;
            }
            return new DiscConfiguration { DiscId = discId, Type = DiscType.Router, Label = $"Disco_{discId}", DisplayColor = Color.gray };
        }
    }
}