using System.Collections.Generic;
using NUnit.Framework;
using SimRedes.Simulation;
using UnityEngine;

namespace SimRedes.Simulation
{
    /// <summary>
    /// Tests for <see cref="FindFaultScenarios"/> (catalogo de datos extraido de
    /// FindFaultActivity.InitializeScenarios). Fija los datos para que una
    /// extraccion futura no altere los escenarios por accidente.
    /// </summary>
    public class TestFindFaultScenarios
    {
        [Test]
        public void Create_ReturnsFourScenarios()
        {
            // Act
            var scenarios = FindFaultScenarios.Create();

            // Assert
            Assert.IsNotNull(scenarios);
            Assert.AreEqual(4, scenarios.Count, "Debe haber exactamente 4 escenarios");
        }

        [Test]
        public void Create_ExpectedNamesAndFaultTypes()
        {
            // Act
            var scenarios = FindFaultScenarios.Create();

            // Assert
            Assert.AreEqual("Cable Ca\u00eddo", scenarios[0].Name);
            Assert.AreEqual("IP Err\u00f3nea", scenarios[1].Name);
            Assert.AreEqual("M\u00e1scara Incorrecta", scenarios[2].Name);
            Assert.AreEqual("PC sin IP", scenarios[3].Name);

            Assert.AreEqual("cable", scenarios[0].FaultType);
            Assert.AreEqual("ip", scenarios[1].FaultType);
            Assert.AreEqual("mask", scenarios[2].FaultType);
            Assert.AreEqual("gateway", scenarios[3].FaultType);
        }

        [Test]
        public void Create_KeyRepairData_MatchesOriginalDataset()
        {
            // Act
            var scenarios = FindFaultScenarios.Create();

            // Escenario 1: sin enlaces (el usuario lo crea con CONECTAR)
            Assert.AreEqual(0, scenarios[0].Links.Count);
            Assert.AreEqual(2, scenarios[0].Devices.Count);
            Assert.AreEqual(2, scenarios[0].IPConfigs.Count);

            // Escenario 2: IP incorrecta y su correccion
            Assert.AreEqual("192.168.100.99", scenarios[1].IPConfigs[0].ip);
            Assert.AreEqual("192.168.1.1", scenarios[1].CorrectIP);
            Assert.AreEqual("255.255.255.0", scenarios[1].CorrectMask);

            // Escenario 3: mascara incorrecta y su correccion
            Assert.AreEqual("255.0.0.0", scenarios[2].IPConfigs[0].mask);
            Assert.AreEqual("255.255.255.0", scenarios[2].CorrectMask);

            // Escenario 4: PC afectado sin IP
            Assert.AreEqual(1, scenarios[3].FaultDeviceIndex);
            Assert.AreEqual("", scenarios[3].IPConfigs[1].ip);
        }

        [Test]
        public void Create_EachCallReturnsIndependentInstances()
        {
            // Act
            var first = FindFaultScenarios.Create();
            var second = FindFaultScenarios.Create();

            // Assert
            Assert.AreNotSame(first, second);
            Assert.AreNotSame(first[0], second[0],
                "Cada llamada debe construir escenarios nuevos (IsSolved no se comparte)");
        }

        [Test]
        public void FindFaultActivity_Awake_LoadsCatalogScenarios()
        {
            // Arrange
            var go = new GameObject("FindFaultActivity");
            var activity = go.AddComponent<FindFaultActivity>();
            var awake = typeof(FindFaultActivity).GetMethod("Awake",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            // Act
            awake.Invoke(activity, null);

            // Assert
            var field = typeof(FindFaultActivity).GetField("scenarios",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(field);
            var loaded = (List<FindFaultScenario>)field.GetValue(activity);

            Assert.IsNotNull(loaded, "Awake debe inicializar los escenarios");
            Assert.AreEqual(4, loaded.Count);

            var expected = FindFaultScenarios.Create();
            for (int i = 0; i < expected.Count; i++)
                Assert.AreEqual(expected[i].Name, loaded[i].Name);

            Object.DestroyImmediate(go);
        }
    }
}
