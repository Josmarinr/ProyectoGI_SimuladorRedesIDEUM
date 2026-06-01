using UnityEngine;

namespace SimRedes.Network
{
    /// <summary>
    /// Tipos de disco fisico o virtual disponibles en la mesa IDEUM.
    /// Los IDs 1-3 son fisicos; 4-18 son virtuales para actividades.
    /// </summary>
    public enum DiscType
    {
        /// <summary>Dispositivo de enrutamiento (ID 1, fisico).</summary>
        Router,
        /// <summary>Dispositivo de conmutacion (ID 2, fisico).</summary>
        Switch,
        /// <summary>Host final (ID 3, fisico).</summary>
        PC,
        /// <summary>Conexion entre dispositivos (ID 4).</summary>
        Enlace,
        /// <summary>Simulacion de errores (ID 5).</summary>
        Fallo,
        /// <summary>Protocolo de enrutamiento (ID 6).</summary>
        Protocolo,
        /// <summary>Red de destino para ruta (ID 7).</summary>
        RedDestino,
        /// <summary>Valor de metrica (ID 8).</summary>
        Metrica,
        /// <summary>Interfaz de salida (ID 9).</summary>
        InterfazSalida,
        /// <summary>Modo de enrutamiento (ID 10).</summary>
        ModoEnrutamiento,
        /// <summary>Comando ip route (ID 11).</summary>
        IpRoute,
        /// <summary>IP de destino (ID 12).</summary>
        Destino,
        /// <summary>Mascara de subred (ID 13).</summary>
        Mascara,
        /// <summary>Siguiente salto (ID 14).</summary>
        ProximoSalto,
        /// <summary>Router vecino para protocolos dinamicos (ID 15).</summary>
        Vecino,
        /// <summary>Red a anunciar en protocolos dinamicos (ID 16).</summary>
        AnunciarRed,
        /// <summary>Costo del enlace (ID 17).</summary>
        Costo,
        /// <summary>Ancho de banda del enlace (ID 18).</summary>
        BW
    }

    /// <summary>
    /// Configuracion de discos tangible-virtuales. Define ID, tipo, etiqueta, color y descripcion
    /// para cada disco de la mesa IDEUM (IDs 1-18).
    /// </summary>
    public class DiscConfiguration
    {
        /// <summary>Identificador unico del disco (1-18).</summary>
        public int DiscId { get; set; }
        /// <summary>Tipo de disco (Router, Switch, PC, etc.).</summary>
        public DiscType Type { get; set; }
        /// <summary>Etiqueta visible del disco.</summary>
        public string Label { get; set; }
        /// <summary>Color representativo del disco en la UI.</summary>
        public Color DisplayColor { get; set; }
        /// <summary>Descripcion textual del proposito del disco.</summary>
        public string Description { get; set; }

        /// <summary>
        /// Configuracion predeterminada para los 18 discos del sistema.
        /// Los IDs 1-3 son fisicos; IDs 4-18 son virtuales.
        /// </summary>
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

        /// <summary>
        /// Obtiene la configuracion de un disco por su ID.
        /// Si no existe, devuelve una configuracion generica por defecto.
        /// </summary>
        /// <param name="discId">Identificador del disco (1-18).</param>
        /// <returns>Configuracion del disco encontrada, o una generica si no existe.</returns>
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