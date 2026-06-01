using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimRedes.Simulation
{
    /// <summary>
    /// Sistema de puntuacion para actividades del simulador.
    /// Gestiona eventos, calculo de puntos, bonificaciones por tiempo y calificacion final.
    /// </summary>
    public class ScoringSystem : MonoBehaviour
    {
        /// <summary>
        /// Instancia singleton del sistema de puntuacion.
        /// </summary>
        public static ScoringSystem Instance { get; private set; }

        [Header("Configuracion")]
        /// <summary>Puntos base por tarea completada.</summary>
        public int basePoints = 100;
        /// <summary>Puntos maximos de bonificacion por rapidez.</summary>
        public int timeBonusMax = 50;
        /// <summary>Penalizacion por cada fallo (valor negativo).</summary>
        public int faultPenalty = -20;
        /// <summary>Puntos por cada ruta configurada correctamente.</summary>
        public int routeBonus = 15;
        /// <summary>Puntos por cada ping exitoso.</summary>
        public int pingSuccessBonus = 10;

        private int currentScore = 0;
        private int tasksCompleted = 0;
        private int faultsFound = 0;
        private int routesConfigured = 0;
        private int pingsSuccess = 0;
        private DateTime sessionStartTime;
        private string currentActivity = "";
        private List<ScoringEvent> events = new List<ScoringEvent>();

        /// <summary>
        /// Representa un evento de puntuacion individual con descripcion, puntos y momento en que ocurrio.
        /// </summary>
        [System.Serializable]
        public class ScoringEvent
        {
            /// <summary>Descripcion del evento.</summary>
            public string description;
            /// <summary>Puntos otorgados o quitados.</summary>
            public int points;
            /// <summary>Momento en que ocurrio el evento.</summary>
            public DateTime timestamp;
        }

        /// <summary>
        /// Inicializa el singleton. Si ya existe otra instancia, la destruye.
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        /// <summary>
        /// Inicia una nueva sesion de puntuacion para la actividad indicada.
        /// Resetea todos los contadores y la lista de eventos.
        /// </summary>
        /// <param name="activityName">Nombre de la actividad (ej: "BuildTopology").</param>
        public void StartSession(string activityName)
        {
            currentScore = 0;
            tasksCompleted = 0;
            faultsFound = 0;
            routesConfigured = 0;
            pingsSuccess = 0;
            sessionStartTime = DateTime.Now;
            currentActivity = activityName;
            events.Clear();
            UnityEngine.Debug.Log($"[Scoring] Sesion iniciada: {activityName}");
        }

        /// <summary>
        /// Registra una tarea completada y suma puntos incluyendo bonificacion por tiempo.
        /// </summary>
        /// <param name="taskDescription">Descripcion de la tarea realizada.</param>
        public void AddTaskCompleted(string taskDescription)
        {
            tasksCompleted++;
            int points = basePoints + CalculateTimeBonus();
            AddScore(points, $"Tarea completada: {taskDescription}");
        }

        /// <summary>
        /// Registra un fallo encontrado y suma puntos base mas el valor absoluto de la penalizacion.
        /// </summary>
        /// <param name="faultDescription">Descripcion del fallo detectado.</param>
        public void AddFaultFound(string faultDescription)
        {
            faultsFound++;
            int points = basePoints + Mathf.Abs(faultPenalty);
            AddScore(points, $"Fallo encontrado: {faultDescription}");
        }

        /// <summary>
        /// Registra una ruta configurada y suma los puntos de bonificacion por ruta.
        /// </summary>
        /// <param name="routeInfo">Informacion de la ruta configurada.</param>
        public void AddRouteConfigured(string routeInfo)
        {
            routesConfigured++;
            int points = routeBonus;
            AddScore(points, $"Ruta configurada: {routeInfo}");
        }

        /// <summary>
        /// Registra un ping exitoso entre dos dispositivos y suma los puntos correspondientes.
        /// </summary>
        /// <param name="source">Direccion IP o nombre del origen.</param>
        /// <param name="destination">Direccion IP o nombre del destino.</param>
        public void AddPingSuccess(string source, string destination)
        {
            pingsSuccess++;
            int points = pingSuccessBonus;
            AddScore(points, $"Ping exitoso: {source} -> {destination}");
        }

        /// <summary>
        /// Agrega una penalizacion manual con la cantidad de puntos especificada.
        /// </summary>
        /// <param name="reason">Motivo de la penalizacion.</param>
        /// <param name="points">Puntos a restar (valor negativo).</param>
        public void AddPenalty(string reason, int points)
        {
            AddScore(points, $"Penalizacion: {reason}");
        }

        /// <summary>
        /// Acumula puntos al puntaje total (nunca baja de 0) y registra el evento en el historial.
        /// </summary>
        /// <param name="points">Puntos a sumar (positivo) o restar (negativo).</param>
        /// <param name="description">Descripcion del evento.</param>
        private void AddScore(int points, string description)
        {
            currentScore = Mathf.Max(0, currentScore + points);
            events.Add(new ScoringEvent
            {
                description = description,
                points = points,
                timestamp = DateTime.Now
            });
            UnityEngine.Debug.Log($"[Scoring] {points} pts: {description}");
        }

        /// <summary>
        /// Calcula la bonificacion por tiempo transcurrido desde el inicio de la sesion.
        /// Menos de 2 minutos: bonificacion maxima. Entre 2 y 5: mitad. Mas de 5: 0.
        /// </summary>
        private int CalculateTimeBonus()
        {
            TimeSpan elapsed = DateTime.Now - sessionStartTime;
            int minutes = (int)elapsed.TotalMinutes;
            if (minutes < 2) return timeBonusMax;
            if (minutes < 5) return timeBonusMax / 2;
            return 0;
        }

        /// <summary>Puntaje actual acumulado.</summary>
        public int GetCurrentScore() => currentScore;
        /// <summary>Cantidad de tareas completadas.</summary>
        public int GetTasksCompleted() => tasksCompleted;
        /// <summary>Cantidad de fallos encontrados.</summary>
        public int GetFaultsFound() => faultsFound;
        /// <summary>Cantidad de rutas configuradas.</summary>
        public int GetRoutesConfigured() => routesConfigured;
        /// <summary>Cantidad de pings exitosos.</summary>
        public int GetPingsSuccess() => pingsSuccess;

        /// <summary>
        /// Genera un resumen de la sesion con puntuacion, tareas, fallos, rutas, pings y tiempo.
        /// </summary>
        /// <returns>Texto con el resumen formateado.</returns>
        public string GetSessionSummary()
        {
            TimeSpan elapsed = DateTime.Now - sessionStartTime;
            return $"Puntuacion Final: {currentScore}\n" +
                   $"Tareas: {tasksCompleted}\n" +
                   $"Fallos: {faultsFound}\n" +
                   $"Rutas: {routesConfigured}\n" +
                   $"Pings: {pingsSuccess}\n" +
                   $"Tiempo: {elapsed.Minutes}m {elapsed.Seconds}s";
        }

        /// <summary>
        /// Devuelve una copia de la lista de eventos de puntuacion.
        /// </summary>
        /// <returns>Lista de eventos de puntuacion.</returns>
        public List<ScoringEvent> GetEvents() => new List<ScoringEvent>(events);

        /// <summary>
        /// Calcula la calificacion (1-5) segun el puntaje actual.
        /// 500+ = 5, 400+ = 4, 300+ = 3, 200+ = 2, menor = 1.
        /// </summary>
        /// <returns>Calificacion numerica entre 1 y 5.</returns>
        public int GetGrade()
        {
            if (currentScore >= 500) return 5;
            if (currentScore >= 400) return 4;
            if (currentScore >= 300) return 3;
            if (currentScore >= 200) return 2;
            return 1;
        }

        /// <summary>
        /// Finaliza la sesion de puntuacion actual y registra el resultado en el log.
        /// </summary>
        public void EndSession()
        {
            UnityEngine.Debug.Log($"[Scoring] Sesion terminada. Puntuacion: {currentScore}");
            currentActivity = "";
        }
    }
}