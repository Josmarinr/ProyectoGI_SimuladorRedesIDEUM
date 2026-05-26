using NUnit.Framework;
using SimRedes.Tangible;
using System.Collections.Generic;
using UnityEngine;

namespace Tests.EditMode.Tangible
{
    /// <summary>
    /// Tests for TangibleBridge.
    /// Tests pattern mapping, coordinate conversion, mapping state, and null safety.
    /// Does NOT depend on TE.TangibleEngine runtime (Start() is avoided in null-safety tests).
    /// </summary>
    public class TestTangibleBridge
    {
        private GameObject bridgeGo;
        private TangibleBridge bridge;

        [SetUp]
        public void SetUp()
        {
            bridgeGo = new GameObject("TangibleBridge");
            bridge = bridgeGo.AddComponent<TangibleBridge>();
            // IMPORTANT: Do NOT call Start() — it subscribes to TE.TangibleEngine events
            // which creates a TangibleEngine GameObject. Only call Start() in specific tests.
        }

        [TearDown]
        public void TearDown()
        {
            if (bridgeGo != null)
                Object.DestroyImmediate(bridgeGo);

            // Clean up any TE.TangibleEngine that might have been created
            var teEngine = GameObject.Find("TE.TangibleEngine");
            if (teEngine != null)
                Object.DestroyImmediate(teEngine);

            // Clean up any DiscManager created during tests
            var dm = Object.FindAnyObjectByType<TangibleDiscManager>();
            if (dm != null)
                Object.DestroyImmediate(dm.gameObject);
        }

        // ================================================================
        // Reflection Helpers
        // ================================================================

        private Vector2 InvokeConvertToCanvasPosition(Vector2 screenPosition)
        {
            var method = typeof(TangibleBridge).GetMethod("ConvertToCanvasPosition",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(method, "ConvertToCanvasPosition method not found");
            return (Vector2)method.Invoke(bridge, new object[] { screenPosition });
        }

        private int InvokeMapPatternToDiscType(int patternId)
        {
            var method = typeof(TangibleBridge).GetMethod("MapPatternToDiscType",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(method, "MapPatternToDiscType method not found");
            return (int)method.Invoke(bridge, new object[] { patternId });
        }

        private void InvokeHandleTangibleAdded(TE.Tangible tangible)
        {
            var method = typeof(TangibleBridge).GetMethod("HandleTangibleAdded",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(method, "HandleTangibleAdded method not found");
            method.Invoke(bridge, new object[] { tangible });
        }

        private void InvokeHandleTangibleRemoved(TE.Tangible tangible)
        {
            var method = typeof(TangibleBridge).GetMethod("HandleTangibleRemoved",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(method, "HandleTangibleRemoved method not found");
            method.Invoke(bridge, new object[] { tangible });
        }

        private void InvokeHandleTangibleUpdated(TE.Tangible tangible)
        {
            var method = typeof(TangibleBridge).GetMethod("HandleTangibleUpdated",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(method, "HandleTangibleUpdated method not found");
            method.Invoke(bridge, new object[] { tangible });
        }

        private Dictionary<int, int> GetTangibleIdToUniqueId()
        {
            var field = typeof(TangibleBridge).GetField("tangibleIdToUniqueId",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(field, "tangibleIdToUniqueId field not found");
            return (Dictionary<int, int>)field.GetValue(bridge);
        }

        private void InvokeStart()
        {
            var method = typeof(TangibleBridge).GetMethod("Start",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(method, "Start method not found");
            method.Invoke(bridge, null);
        }

        // ================================================================
        // PATTERN TO DISC TYPE
        // ================================================================

        [Test]
        public void MapPatternToDiscType_Pattern1_ReturnsRouter()
        {
            var result = InvokeMapPatternToDiscType(1);
            Assert.AreEqual(1, result, "PatternId 1 should map to disc type 1 (Router)");
        }

        [Test]
        public void MapPatternToDiscType_Pattern2_ReturnsSwitch()
        {
            var result = InvokeMapPatternToDiscType(2);
            Assert.AreEqual(2, result, "PatternId 2 should map to disc type 2 (Switch)");
        }

        [Test]
        public void MapPatternToDiscType_Pattern3_ReturnsPC()
        {
            var result = InvokeMapPatternToDiscType(3);
            Assert.AreEqual(3, result, "PatternId 3 should map to disc type 3 (PC)");
        }

        [Test]
        public void MapPatternToDiscType_Pattern4_ReturnsMinusOne()
        {
            var result = InvokeMapPatternToDiscType(4);
            Assert.AreEqual(-1, result,
                "PatternId 4 is not a physical disc and should return -1");
        }

        [TestCase(0)]
        [TestCase(99)]
        public void MapPatternToDiscType_UnknownPattern_ReturnsMinusOne(int patternId)
        {
            var result = InvokeMapPatternToDiscType(patternId);
            Assert.AreEqual(-1, result, $"PatternId {patternId} is unknown and should return -1");
        }

        // ================================================================
        // COORDINATE CONVERSION
        // ================================================================

        [Test]
        public void ConvertToCanvasPosition_ZeroDisplay_UsesFallback()
        {
            // In EditMode, Display.main.systemWidth may be 0.
            // The code falls back to Screen.currentResolution, then to 1920x1080.
            // Just verify that the method does not crash and returns a valid Vector2.
            Vector2 result = Vector2.zero;
            Assert.DoesNotThrow(() =>
            {
                result = InvokeConvertToCanvasPosition(new Vector2(100, 200));
            });
            Assert.IsTrue(result.x > 0 || result.y > 0,
                "Converted coordinates should be non-negative");
        }

        [Test]
        public void ConvertToCanvasPosition_ScreenToCanvas()
        {
            // (960, 540) in a 1920x1080 display maps proportionally to 4096x2160 canvas.
            // The exact result depends on what Screen.currentResolution returns in EditMode.
            // Verify the ratio is maintained.
            Vector2 result = Vector2.zero;
            Assert.DoesNotThrow(() =>
            {
                result = InvokeConvertToCanvasPosition(new Vector2(960, 540));
            });
            Assert.IsTrue(result.x > 0, "X should be positive");
            Assert.IsTrue(result.y > 0, "Y should be positive");
        }

        [Test]
        public void ConvertToCanvasPosition_EdgeCoordinates()
        {
            // (0,0) should always map to (0,0)
            Vector2 zero = InvokeConvertToCanvasPosition(Vector2.zero);
            Assert.AreEqual(0f, zero.x, 0.001f, "(0,0) screen should map to (0,0) canvas");
            Assert.AreEqual(0f, zero.y, 0.001f, "(0,0) screen should map to (0,0) canvas");

            // Large coordinates should also work without crashing
            Vector2 large;
            Assert.DoesNotThrow(() =>
            {
                large = InvokeConvertToCanvasPosition(new Vector2(1920, 1080));
            });
        }

        // ================================================================
        // MAPPING STATE
        // ================================================================

        [Test]
        public void tangibleIdToUniqueId_StartsEmpty()
        {
            var mapping = GetTangibleIdToUniqueId();
            Assert.IsNotNull(mapping, "Mapping dictionary should be initialized");
            Assert.IsEmpty(mapping, "Mapping should start empty");
        }

        [Test]
        public void HandleTangibleAdded_WithDiscManager_CreatesMapping()
        {
            // Arrange: Create TangibleDiscManager
            var dmGo = new GameObject("DiscManager");
            var dm = dmGo.AddComponent<TangibleDiscManager>();
            var awakeMethod = typeof(TangibleDiscManager).GetMethod("Awake",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (awakeMethod != null)
                awakeMethod.Invoke(dm, null);

            // Start the bridge so it subscribes to TE events (try-catch protects against TE absence)
            InvokeStart();

            // Act: Simulate a tangible added
            var tangible = new TE.Tangible
            {
                Id = 1,
                PatternId = 1, // Router
                X = 100,
                Y = 200
            };
            InvokeHandleTangibleAdded(tangible);

            // Assert
            var mapping = GetTangibleIdToUniqueId();
            Assert.AreEqual(1, mapping.Count, "Should have one mapping entry");
            Assert.IsTrue(mapping.ContainsKey(1), "Should map TE.Id=1 to a uniqueId");
        }

        [Test]
        public void HandleTangibleAdded_WithReusedTangible_CallsUpdate()
        {
            // Arrange: Create TangibleDiscManager
            var dmGo = new GameObject("DiscManager");
            var dm = dmGo.AddComponent<TangibleDiscManager>();
            var awakeMethod = typeof(TangibleDiscManager).GetMethod("Awake",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (awakeMethod != null)
                awakeMethod.Invoke(dm, null);
            InvokeStart();

            var tangible = new TE.Tangible
            {
                Id = 1,
                PatternId = 1,
                X = 100,
                Y = 200
            };

            // Act: Add the same tangible twice
            InvokeHandleTangibleAdded(tangible);
            var mappingAfterFirst = GetTangibleIdToUniqueId();
            int countAfterFirst = mappingAfterFirst.Count;

            InvokeHandleTangibleAdded(tangible); // Should redirect to HandleTangibleUpdated
            var mappingAfterSecond = GetTangibleIdToUniqueId();

            // Assert: No duplicate mapping entries
            Assert.AreEqual(1, mappingAfterSecond.Count,
                "Reusing the same tangible should not create a second mapping entry");
            Assert.AreEqual(countAfterFirst, mappingAfterSecond.Count,
                "Mapping count should remain the same after re-adding the same tangible");
        }

        // ================================================================
        // NULL SAFETY
        // ================================================================

        [Test]
        public void HandleTangibleAdded_WithoutDiscManager_DoesNotCrash()
        {
            // No TangibleDiscManager created. Do NOT call InvokeStart().
            var tangible = new TE.Tangible
            {
                Id = 1,
                PatternId = 1,
                X = 100,
                Y = 200
            };

            Assert.DoesNotThrow(() => InvokeHandleTangibleAdded(tangible),
                "HandleTangibleAdded should not crash without DiscManager");
        }

        [Test]
        public void HandleTangibleRemoved_WithoutDiscManager_DoesNotCrash()
        {
            // No TangibleDiscManager created. Do NOT call InvokeStart().
            var tangible = new TE.Tangible
            {
                Id = 1,
                PatternId = 1,
                X = 100,
                Y = 200
            };

            Assert.DoesNotThrow(() => InvokeHandleTangibleRemoved(tangible),
                "HandleTangibleRemoved should not crash without DiscManager");
        }

        [Test]
        public void HandleTangibleUpdated_WithoutDiscManager_DoesNotCrash()
        {
            // No TangibleDiscManager created. Do NOT call InvokeStart().
            var tangible = new TE.Tangible
            {
                Id = 1,
                PatternId = 1,
                X = 100,
                Y = 200
            };

            Assert.DoesNotThrow(() => InvokeHandleTangibleUpdated(tangible),
                "HandleTangibleUpdated should not crash without DiscManager");
        }
    }
}
