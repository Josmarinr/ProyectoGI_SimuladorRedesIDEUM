using NUnit.Framework;
using SimRedes.UI;
using SimRedes.Network;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Reflection;
using DeviceType = SimRedes.Network.DeviceType;

namespace Tests.EditMode.UI
{
    public class TestConfigPanelFactory
    {
        private GameObject canvasGo;
        private Canvas canvas;
        private GameObject topologyGo;
        private TopologyManager topology;

        [SetUp]
        public void SetUp()
        {
            canvasGo = new GameObject("TestCanvas");
            canvas = canvasGo.AddComponent<Canvas>();

            topologyGo = new GameObject("TestTopology");
            topology = topologyGo.AddComponent<TopologyManager>();
            var awakeMethod = typeof(TopologyManager).GetMethod("Awake",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (awakeMethod != null)
                awakeMethod.Invoke(topology, null);
        }

        [TearDown]
        public void TearDown()
        {
            // Clear TopologyManager singleton
            var instanceField = typeof(TopologyManager).GetField("Instance",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (instanceField != null)
                instanceField.SetValue(null, null);

            // Clean up any panels left in scene by factories
            CleanupPanels();

            UnityEngine.Object.DestroyImmediate(topologyGo);
            UnityEngine.Object.DestroyImmediate(canvasGo);
        }

        private void CleanupPanels()
        {
            string[] panelNames = new string[]
            {
                "IPConfigPanel", "ARPPanel", "RoutingPanel", "AddRoutePanel",
                "VLANPanel", "ACLPanel", "NATPanel",
                "ClickOutsideBG_VLANPanel", "ClickOutsideBG_ACLPanel", "ClickOutsideBG_NATPanel"
            };
            foreach (string name in panelNames)
            {
                GameObject obj = GameObject.Find(name);
                if (obj != null)
                    UnityEngine.Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void CreateIPConfigPanel_WithData_ReturnsValidationText()
        {
            Text validationText = ConfigPanelFactory.CreateIPConfigPanel(
                canvas.transform,
                "TestRouter",
                "192.168.1.1",
                "255.255.255.0",
                onIPChanged: null,
                onMaskChanged: null,
                onApply: null,
                onARP: null,
                onRouting: null,
                onCancel: null
            );

            Assert.IsNotNull(validationText);
            Assert.IsNotNull(GameObject.Find("IPConfigPanel"));
        }

        [Test]
        public void CreateARPPanel_WithNode_ReturnsPanel()
        {
            var node = new NetworkNode(1, DeviceType.Router, Vector2.zero);

            GameObject panel = ConfigPanelFactory.CreateARPPanel(canvas.transform, node);

            Assert.IsNotNull(panel);
            Assert.AreEqual("ARPPanel", panel.name);
        }

        [Test]
        public void CreateRoutingPanel_WithNode_ReturnsPanel()
        {
            var node = new NetworkNode(1, DeviceType.Router, Vector2.zero);

            GameObject panel = ConfigPanelFactory.CreateRoutingPanel(
                canvas.transform,
                node,
                onDeleteRoute: null,
                onAddRoute: null,
                onClose: null
            );

            Assert.IsNotNull(panel);
            Assert.AreEqual("RoutingPanel", panel.name);
        }

        [Test]
        public void CreateAddRoutePanel_WithNode_ReturnsPanel()
        {
            var node = new NetworkNode(1, DeviceType.Router, Vector2.zero);

            GameObject panel = ConfigPanelFactory.CreateAddRoutePanel(
                canvas.transform,
                node,
                onAddRoute: null,
                onCancel: null
            );

            Assert.IsNotNull(panel);
            Assert.AreEqual("AddRoutePanel", panel.name);
            Assert.IsNotNull(panel.transform.Find("ConfirmAddRouteBtn"));
            Assert.IsNotNull(panel.transform.Find("CancelAddRouteBtn"));
        }

        [Test]
        public void CreateVLANPanel_WithTopology_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                ConfigPanelFactory.CreateVLANPanel(canvas.transform, topology);
            });

            GameObject panel = GameObject.Find("VLANPanel");
            Assert.IsNotNull(panel);
        }

        [Test]
        public void CreateACLPanel_WithTopology_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                ConfigPanelFactory.CreateACLPanel(canvas.transform, topology);
            });

            GameObject panel = GameObject.Find("ACLPanel");
            Assert.IsNotNull(panel);
        }

        [Test]
        public void CreateNATPanel_WithTopology_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                ConfigPanelFactory.CreateNATPanel(canvas.transform, topology);
            });

            GameObject panel = GameObject.Find("NATPanel");
            Assert.IsNotNull(panel);
        }

        [Test]
        public void CreateAddRoutePanel_ConfigFields_HaveTextAndPlaceholderStructure()
        {
            var node = new NetworkNode(1, DeviceType.Router, Vector2.zero);

            GameObject panel = ConfigPanelFactory.CreateAddRoutePanel(
                canvas.transform,
                node,
                onAddRoute: null,
                onCancel: null
            );

            // Los3 campos comparten nombre: se recolectan en orden de creacion
            var fields = new List<Transform>();
            foreach (Transform child in panel.transform)
            {
                if (child.name == "ConfigField") fields.Add(child);
            }
            Assert.AreEqual(3, fields.Count);

            string[] expectedPlaceholders = { "192.168.0.0", "255.255.255.0", "192.168.1.1" };
            for (int i = 0; i < fields.Count; i++)
            {
                Transform field = fields[i];
                Assert.AreEqual(2, field.childCount, "ConfigField debe tener Text y Placeholder");

                Image bg = field.GetComponent<Image>();
                Assert.IsNotNull(bg);
                Assert.AreEqual(new Color(0.2f, 0.25f, 0.3f, 0.9f), bg.color);
                Assert.AreEqual(Image.Type.Simple, bg.type);

                InputField input = field.GetComponent<InputField>();
                Assert.IsNotNull(input);
                Assert.AreEqual(bg, input.targetGraphic);
                Assert.AreEqual(string.Empty, input.text);

                Text text = field.Find("Text").GetComponent<Text>();
                Assert.AreEqual(string.Empty, text.text);
                Assert.AreEqual(14, text.fontSize);
                Assert.AreEqual(Color.white, text.color);
                Assert.IsFalse(text.raycastTarget);
                Assert.AreEqual(TextAnchor.MiddleLeft, text.alignment);
                Assert.AreEqual(new Vector2(8, 3), text.rectTransform.offsetMin);
                Assert.AreEqual(new Vector2(-8, -3), text.rectTransform.offsetMax);

                Text placeholder = field.Find("Placeholder").GetComponent<Text>();
                Assert.AreEqual(expectedPlaceholders[i], placeholder.text);
                Assert.AreEqual(new Color(0.5f, 0.5f, 0.5f, 0.7f), placeholder.color);
                Assert.AreEqual(14, placeholder.fontSize);
                Assert.IsTrue(placeholder.raycastTarget);
                Assert.IsTrue(placeholder.enabled);

                Assert.AreEqual(text, input.textComponent);
                Assert.AreEqual(placeholder, input.placeholder);
            }
        }

        [Test]
        public void CreateVLANPanel_VLANInput_HasTextChildWithoutPlaceholder()
        {
            ConfigPanelFactory.CreateVLANPanel(canvas.transform, topology);
            GameObject panel = GameObject.Find("VLANPanel");
            Assert.IsNotNull(panel);

            Transform field = panel.transform.Find("VLANInput");
            Assert.IsNotNull(field);

            // Campo sin fondo visual y sin placeholder: unico hijo Text
            Assert.AreEqual(1, field.childCount);
            Assert.IsNull(field.GetComponent<Image>());

            InputField input = field.GetComponent<InputField>();
            Assert.IsNotNull(input);
            Assert.AreEqual("10", input.text);
            Assert.AreEqual(4, input.characterLimit);
            Assert.AreEqual(InputField.ContentType.IntegerNumber, input.contentType);
            Assert.IsNull(input.targetGraphic);
            Assert.IsNull(input.placeholder);

            Text text = field.Find("Text").GetComponent<Text>();
            Assert.AreEqual(string.Empty, text.text, "El label no se sincroniza en campos sin fondo");
            Assert.AreEqual(16, text.fontSize);
            Assert.AreEqual(UIComponents.Colors.textPrimary, text.color);
            Assert.IsFalse(text.raycastTarget);
            Assert.AreEqual(TextAnchor.MiddleLeft, text.alignment);
            Assert.AreEqual(new Vector2(8, 3), text.rectTransform.offsetMin);
            Assert.AreEqual(new Vector2(-8, -3), text.rectTransform.offsetMax);
            Assert.AreEqual(text, input.textComponent);
        }

        [Test]
        public void CreateACLPanel_NameInput_HasTextChildWithoutPlaceholder()
        {
            ConfigPanelFactory.CreateACLPanel(canvas.transform, topology);
            GameObject panel = GameObject.Find("ACLPanel");
            Assert.IsNotNull(panel);

            Transform field = panel.transform.Find("ACLNameInput");
            Assert.IsNotNull(field);
            Assert.AreEqual(1, field.childCount);
            Assert.IsNull(field.GetComponent<Image>());

            InputField input = field.GetComponent<InputField>();
            Assert.IsNotNull(input);
            Assert.AreEqual("MI_ACL", input.text);
            Assert.IsNull(input.placeholder);

            Text text = field.Find("Text").GetComponent<Text>();
            Assert.AreEqual(string.Empty, text.text, "El label no se sincroniza en campos sin fondo");
            Assert.AreEqual(16, text.fontSize);
            Assert.AreEqual(UIComponents.Colors.textPrimary, text.color);
            Assert.IsFalse(text.raycastTarget);
            Assert.AreEqual(new Vector2(8, 3), text.rectTransform.offsetMin);
            Assert.AreEqual(text, input.textComponent);
        }
    }
}
