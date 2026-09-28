using UnityEngine;
using SimRedes.Network;

namespace SimRedes.Simulation
{
    /// <summary>
    /// Arranque compartido de las actividades del simulador.
    /// Centraliza la resolucion del TopologyManager para que ninguna actividad
    /// cree singletons fantasma con new GameObject("TopologyManager").
    /// </summary>
    public static class ActivityStartup
    {
        /// <summary>
        /// Resuelve el TopologyManager activo usando primero el singleton
        /// <see cref="TopologyManager.Instance"/> y, si no esta disponible,
        /// una busqueda en la escena. Nunca crea instancias nuevas: si no hay
        /// ninguno retorna null y la actividad debe tratar esa condicion.
        /// </summary>
        /// <returns>El TopologyManager existente, o null si la escena no tiene uno.</returns>
        public static TopologyManager ResolveTopologyManager()
        {
            var manager = TopologyManager.Instance;
            if (manager != null) return manager;
            return Object.FindAnyObjectByType<TopologyManager>();
        }
    }
}
