using NUnit.Framework;
using SimRedes.Simulation;
using UnityEngine;

namespace SimRedes.Simulation
{
    public class TestScoringSystem
    {
        private ScoringSystem scoringSystem;
        private GameObject go;

        [SetUp]
        public void Setup()
        {
            go = new GameObject("TestScoring");
            scoringSystem = go.AddComponent<ScoringSystem>();
            scoringSystem.StartSession("TestActivity");
        }

        [TearDown]
        public void Teardown()
        {
            if (ScoringSystem.Instance == scoringSystem)
            {
                // Reflection to reset singleton instance
                var field = typeof(ScoringSystem).GetField("Instance",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                if (field != null)
                    field.SetValue(null, null);
            }
            if (go != null)
                Object.DestroyImmediate(go);
        }

        [Test]
        public void StartSession_ResetsAllCounters()
        {
            // Arrange
            scoringSystem.AddTaskCompleted("test");
            scoringSystem.AddFaultFound("test");
            scoringSystem.AddRouteConfigured("test");
            scoringSystem.AddPingSuccess("A", "B");

            // Act
            scoringSystem.StartSession("NewActivity");

            // Assert
            Assert.AreEqual(0, scoringSystem.GetCurrentScore());
            Assert.AreEqual(0, scoringSystem.GetTasksCompleted());
            Assert.AreEqual(0, scoringSystem.GetFaultsFound());
            Assert.AreEqual(0, scoringSystem.GetRoutesConfigured());
            Assert.AreEqual(0, scoringSystem.GetPingsSuccess());
        }

        [Test]
        public void AddTaskCompleted_IncrementsCountAndScore()
        {
            // Arrange
            int initialScore = scoringSystem.GetCurrentScore();
            int initialTasks = scoringSystem.GetTasksCompleted();

            // Act
            scoringSystem.AddTaskCompleted("Build topology");

            // Assert
            Assert.AreEqual(initialTasks + 1, scoringSystem.GetTasksCompleted());
            Assert.IsTrue(scoringSystem.GetCurrentScore() > initialScore);
        }

        [Test]
        public void AddFaultFound_IncrementsFaultsAndScore()
        {
            // Arrange
            int initialFaults = scoringSystem.GetFaultsFound();
            int initialScore = scoringSystem.GetCurrentScore();

            // Act
            scoringSystem.AddFaultFound("Broken link");

            // Assert
            Assert.AreEqual(initialFaults + 1, scoringSystem.GetFaultsFound());
            Assert.IsTrue(scoringSystem.GetCurrentScore() > initialScore);
        }

        [Test]
        public void AddRouteConfigured_IncrementsRoutesAndScore()
        {
            // Arrange
            int initialRoutes = scoringSystem.GetRoutesConfigured();
            int initialScore = scoringSystem.GetCurrentScore();

            // Act
            scoringSystem.AddRouteConfigured("10.0.0.0/24 via 192.168.1.1");

            // Assert
            Assert.AreEqual(initialRoutes + 1, scoringSystem.GetRoutesConfigured());
            Assert.IsTrue(scoringSystem.GetCurrentScore() > initialScore);
        }

        [Test]
        public void AddPingSuccess_IncrementsPingsAndScore()
        {
            // Arrange
            int initialPings = scoringSystem.GetPingsSuccess();
            int initialScore = scoringSystem.GetCurrentScore();

            // Act
            scoringSystem.AddPingSuccess("PC1", "PC2");

            // Assert
            Assert.AreEqual(initialPings + 1, scoringSystem.GetPingsSuccess());
            Assert.IsTrue(scoringSystem.GetCurrentScore() > initialScore);
        }

        [Test]
        public void AddPenalty_ReducesScore()
        {
            // Arrange
            scoringSystem.AddTaskCompleted("earn points");
            int scoreBeforePenalty = scoringSystem.GetCurrentScore();

            // Act
            scoringSystem.AddPenalty("Wrong route", -50);

            // Assert
            Assert.IsTrue(scoringSystem.GetCurrentScore() < scoreBeforePenalty);
        }

        [Test]
        public void Score_NeverGoesBelowZero()
        {
            // Arrange
            // Score is 0 after StartSession

            // Act
            scoringSystem.AddPenalty("Massive penalty", -9999);

            // Assert
            Assert.IsTrue(scoringSystem.GetCurrentScore() >= 0);
            Assert.AreEqual(0, scoringSystem.GetCurrentScore());
        }

        [TestCase(500, 5)]
        [TestCase(600, 5)]
        [TestCase(499, 4)]
        [TestCase(400, 4)]
        [TestCase(399, 3)]
        [TestCase(300, 3)]
        [TestCase(299, 2)]
        [TestCase(200, 2)]
        [TestCase(199, 1)]
        [TestCase(0, 1)]
        public void GetGrade_ReturnsCorrectGrade(int score, int expectedGrade)
        {
            // Arrange - need to set the internal currentScore
            // Use reflection to set the private field
            var field = typeof(ScoringSystem).GetField("currentScore",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(field, "currentScore field not found");
            field.SetValue(scoringSystem, score);

            // Act
            int grade = scoringSystem.GetGrade();

            // Assert
            Assert.AreEqual(expectedGrade, grade);
        }

        [Test]
        public void GetEvents_ReturnsCopy()
        {
            // Arrange
            scoringSystem.AddTaskCompleted("test");

            // Act
            var events1 = scoringSystem.GetEvents();
            var events2 = scoringSystem.GetEvents();
            events1.Clear();

            // Assert - events2 should still have the original entry
            Assert.AreEqual(1, events2.Count);
            Assert.AreEqual(0, events1.Count);
        }

        [Test]
        public void GetSessionSummary_ContainsRelevantInfo()
        {
            // Arrange
            scoringSystem.AddTaskCompleted("Build topology");
            scoringSystem.AddFaultFound("Broken link");
            scoringSystem.AddRouteConfigured("10.0.0.0/24");
            scoringSystem.AddPingSuccess("PC1", "PC2");

            // Act
            string summary = scoringSystem.GetSessionSummary();

            // Assert
            Assert.IsNotNull(summary);
            Assert.IsTrue(summary.Contains("Puntuacion"), "Summary should contain score info");
            Assert.IsTrue(summary.Contains("Tareas"), "Summary should contain tasks info");
            Assert.IsTrue(summary.Contains("Fallos"), "Summary should contain faults info");
            Assert.IsTrue(summary.Contains("Rutas"), "Summary should contain routes info");
            Assert.IsTrue(summary.Contains("Pings"), "Summary should contain pings info");
            Assert.IsTrue(summary.Contains("Tiempo"), "Summary should contain time info");
        }
    }
}
