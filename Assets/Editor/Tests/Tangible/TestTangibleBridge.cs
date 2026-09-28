using NUnit.Framework;
using SimRedes.Tangible;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tests.EditMode.Tangible
{
    /// <summary>
    /// Tests for TangibleBridge.
    /// Tests pattern mapping, coordinate conversion, mapping state, and null safety.
    /// Does NOT depend on TE.TangibleEngine runtime (Start() is avoided in null-safety tests).
    /// P2: conversion assertions use the CENTER-RELATIVE convention ((0,0) = canvas
    /// center) — a deliberate behavior change, see the comments in each test.
    /// </summary>
    public class TestTangibleBridge
    {
        private GameObject bridgeGo;
        private TangibleBridge bridge;

        [SetUp]
        public void SetUp()
        {
            // P2: ConvertToCanvasPosition now reads the active CanvasScaler resolution
            // when one exists (fallback: serialized 4096x2160). To keep the absolute
            // assertions of the conversion tests deterministic in EditMode, remove any
            // residual CanvasScaler leaked by other fixtures. The rest of the tests in
            // this fixture do not depend on the canvas, so this is harmless for them.
            foreach (var scaler in Object.FindObjectsByType<CanvasScaler>(
                FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(scaler);
            }

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
            // P2/H1 — DELIBERATE ASSERTION CHANGE (old convention: corner-origin
            // canvas pixels, "result must be non-negative"). ConvertToCanvasPosition
            // now returns CENTER-RELATIVE canvas coordinates ((0,0) = canvas center),
            // so values below the center are negative by design.
            // In EditMode there is no active CanvasScaler ( SetUp removes strays), so
            // the serialized fallback 4096x2160 applies; the touch frame is assumed
            // 1920x1080 (IDEUM frame, see TouchFrame header in TangibleBridge).
            // Method name kept for continuity: the "fallback" verified is now
            // canvasWidth/canvasHeight instead of Display.main.
            Vector2 result = Vector2.zero;
            Assert.DoesNotThrow(() =>
            {
                result = InvokeConvertToCanvasPosition(new Vector2(100, 200));
            });
            float expectedX = 100f * (4096f / 1920f) - 4096f / 2f;
            float expectedY = 200f * (2160f / 1080f) - 2160f / 2f;
            Assert.AreEqual(expectedX, result.x, 0.01f,
                "X must be scaled to canvas units AND re-centered (P2 center-relative convention)");
            Assert.AreEqual(expectedY, result.y, 0.01f,
                "Y must be scaled to canvas units AND re-centered (P2 center-relative convention)");
        }

        [Test]
        public void ConvertToCanvasPosition_ScreenToCanvas()
        {
            // P2/H1 — DELIBERATE ASSERTION CHANGE (old convention: "X should be
            // positive"). The center of the touch frame (960,540) — i.e. the center
            // of the IDEUM table — must map EXACTLY to (0,0) of the canvas.
            // This holds for ANY canvas resolution: 960*(W/1920) - W/2 == 0.
            Vector2 result = Vector2.zero;
            Assert.DoesNotThrow(() =>
            {
                result = InvokeConvertToCanvasPosition(new Vector2(960, 540));
            });
            Assert.AreEqual(0f, result.x, 0.001f,
                "Touch-frame center X must map to canvas center 0 (P2 center-relative convention)");
            Assert.AreEqual(0f, result.y, 0.001f,
                "Touch-frame center Y must map to canvas center 0 (P2 center-relative convention)");
        }

        [Test]
        public void ConvertToCanvasPosition_EdgeCoordinates()
        {
            // P2/H1 — DELIBERATE ASSERTION CHANGE (old convention: "(0,0) screen
            // should map to (0,0) canvas"). Under the center-relative convention the
            // touch-frame corners map to the canvas half-extents RELATIVE TO CENTER:
            // (0,0) → (-2048,-1080) and (1920,1080) → (2048,1080) with the 4096x2160
            // fallback. Also kept the original intent that large coordinates work.
            Vector2 origin = InvokeConvertToCanvasPosition(Vector2.zero);
            Assert.AreEqual(-2048f, origin.x, 0.01f,
                "(0,0) frame corner must map to -half canvas X (center-relative)");
            Assert.AreEqual(-1080f, origin.y, 0.01f,
                "(0,0) frame corner must map to -half canvas Y (center-relative)");

            Vector2 far = Vector2.zero;
            Assert.DoesNotThrow(() =>
            {
                far = InvokeConvertToCanvasPosition(new Vector2(1920, 1080));
            });
            Assert.AreEqual(2048f, far.x, 0.01f,
                "(1920,1080) frame corner must map to +half canvas X (center-relative)");
            Assert.AreEqual(1080f, far.y, 0.01f,
                "(1920,1080) frame corner must map to +half canvas Y (center-relative)");
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
