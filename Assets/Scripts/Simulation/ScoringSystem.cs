using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimRedes.Simulation
{
    public class ScoringSystem : MonoBehaviour
    {
        public static ScoringSystem Instance { get; private set; }

        [Header("Configuracion")]
        public int basePoints = 100;
        public int timeBonusMax = 50;
        public int faultPenalty = -20;
        public int routeBonus = 15;
        public int pingSuccessBonus = 10;

        private int currentScore = 0;
        private int tasksCompleted = 0;
        private int faultsFound = 0;
        private int routesConfigured = 0;
        private int pingsSuccess = 0;
        private DateTime sessionStartTime;
        private string currentActivity = "";
        private List<ScoringEvent> events = new List<ScoringEvent>();

        [System.Serializable]
        public class ScoringEvent
        {
            public string description;
            public int points;
            public DateTime timestamp;
        }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

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

        public void AddTaskCompleted(string taskDescription)
        {
            tasksCompleted++;
            int points = basePoints + CalculateTimeBonus();
            AddScore(points, $"Tarea completada: {taskDescription}");
        }

        public void AddFaultFound(string faultDescription)
        {
            faultsFound++;
            int points = basePoints + Mathf.Abs(faultPenalty);
            AddScore(points, $"Fallo encontrado: {faultDescription}");
        }

        public void AddRouteConfigured(string routeInfo)
        {
            routesConfigured++;
            int points = routeBonus;
            AddScore(points, $"Ruta configurada: {routeInfo}");
        }

        public void AddPingSuccess(string source, string destination)
        {
            pingsSuccess++;
            int points = pingSuccessBonus;
            AddScore(points, $"Ping exitoso: {source} -> {destination}");
        }

        public void AddPenalty(string reason, int points)
        {
            AddScore(points, $"Penalizacion: {reason}");
        }

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

        private int CalculateTimeBonus()
        {
            TimeSpan elapsed = DateTime.Now - sessionStartTime;
            int minutes = (int)elapsed.TotalMinutes;
            if (minutes < 2) return timeBonusMax;
            if (minutes < 5) return timeBonusMax / 2;
            return 0;
        }

        public int GetCurrentScore() => currentScore;
        public int GetTasksCompleted() => tasksCompleted;
        public int GetFaultsFound() => faultsFound;
        public int GetRoutesConfigured() => routesConfigured;
        public int GetPingsSuccess() => pingsSuccess;

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

        public List<ScoringEvent> GetEvents() => new List<ScoringEvent>(events);

        public int GetGrade()
        {
            if (currentScore >= 500) return 5;
            if (currentScore >= 400) return 4;
            if (currentScore >= 300) return 3;
            if (currentScore >= 200) return 2;
            return 1;
        }

        public void EndSession()
        {
            UnityEngine.Debug.Log($"[Scoring] Sesion terminada. Puntuacion: {currentScore}");
            currentActivity = "";
        }
    }
}