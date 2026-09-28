using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimRedes.UI;

namespace SimRedes.Simulation
{
    /// <summary>
    /// Actividad de seleccion de la mejor ruta usando longest prefix match.
    /// Presenta escenarios con multiples rutas candidatas y evalua la respuesta del usuario.
    /// </summary>
    public class BestRouteActivity : MonoBehaviour
    {
        [Header("UI References")]
        public Text destIPText;
        public Text resultText;
        public Text scoreText;
        public Button nextButton;

        /// <summary>
        /// Escenario individual con una IP destino, opciones de ruta y la respuesta correcta.
        /// </summary>
        private class RouteScenario
        {
            public string DestinationIP;
            public List<RouteOption> Options;
            public int CorrectIndex;
            public string Explanation;
        }

        /// <summary>
        /// Representa una entrada de ruta candidata con red destino, mascara, siguiente salto, interfaz, metrica y protocolo.
        /// </summary>
        private class RouteOption
        {
            public string DestinationNetwork;
            public string SubnetMask;
            public string NextHop;
            public string OutInterface;
            public int Metric;
            public string Protocol;
        }

        private List<RouteScenario> scenarios;
        private int currentScenarioIndex = -1;
        private int selectedOptionIndex = -1;
        private bool answered = false;
        private int score = 0;
        private int totalAnswered = 0;

        private List<Button> optionButtons = new List<Button>();

        private void Start()
        {
            InitializeScenarios();
            NextScenario();
        }

        /// <summary>
        /// Inicializa la lista de escenarios predefinidos con IPs destino, opciones de ruta y la respuesta correcta.
        /// </summary>
        private void InitializeScenarios()
        {
            scenarios = new List<RouteScenario>
            {
                new RouteScenario
                {
                    DestinationIP = "10.1.1.100",
                    CorrectIndex = 2,
                    Explanation = "10.1.1.0/24 tiene el prefijo mas largo (24 > 16 > 8 > 0), por lo tanto es la ruta mas especifica.",
                    Options = new List<RouteOption>
                    {
                        new RouteOption { DestinationNetwork = "10.0.0.0", SubnetMask = "255.0.0.0", NextHop = "192.168.1.1", OutInterface = "G0/0", Metric = 5, Protocol = "RIP" },
                        new RouteOption { DestinationNetwork = "10.1.0.0", SubnetMask = "255.255.0.0", NextHop = "192.168.1.2", OutInterface = "G0/1", Metric = 3, Protocol = "OSPF" },
                        new RouteOption { DestinationNetwork = "10.1.1.0", SubnetMask = "255.255.255.0", NextHop = "192.168.1.3", OutInterface = "G0/2", Metric = 1, Protocol = "Static" },
                        new RouteOption { DestinationNetwork = "0.0.0.0", SubnetMask = "0.0.0.0", NextHop = "192.168.1.254", OutInterface = "G0/3", Metric = 1, Protocol = "Static" }
                    }
                },
                new RouteScenario
                {
                    DestinationIP = "172.16.5.50",
                    CorrectIndex = 1,
                    Explanation = "172.16.5.0/24 tiene prefijo 24 > 16, es mas especifica que 172.16.0.0/16.",
                    Options = new List<RouteOption>
                    {
                        new RouteOption { DestinationNetwork = "172.16.0.0", SubnetMask = "255.255.0.0", NextHop = "10.0.0.1", OutInterface = "G0/0", Metric = 3, Protocol = "RIP" },
                        new RouteOption { DestinationNetwork = "172.16.5.0", SubnetMask = "255.255.255.0", NextHop = "10.0.0.2", OutInterface = "G0/1", Metric = 2, Protocol = "OSPF" },
                        new RouteOption { DestinationNetwork = "0.0.0.0", SubnetMask = "0.0.0.0", NextHop = "10.0.0.254", OutInterface = "G0/2", Metric = 1, Protocol = "Static" }
                    }
                },
                new RouteScenario
                {
                    DestinationIP = "192.168.1.10",
                    CorrectIndex = 0,
                    Explanation = "192.168.1.0/24 tiene prefijo 24 > 16 > 0, es la mas especifica. Aunque la ruta por defecto tiene la misma metrica, gana el prefijo mas largo.",
                    Options = new List<RouteOption>
                    {
                        new RouteOption { DestinationNetwork = "192.168.1.0", SubnetMask = "255.255.255.0", NextHop = "192.168.1.1", OutInterface = "G0/0", Metric = 1, Protocol = "Static" },
                        new RouteOption { DestinationNetwork = "192.168.0.0", SubnetMask = "255.255.0.0", NextHop = "192.168.1.2", OutInterface = "G0/1", Metric = 2, Protocol = "RIP" },
                        new RouteOption { DestinationNetwork = "0.0.0.0", SubnetMask = "0.0.0.0", NextHop = "192.168.1.254", OutInterface = "G0/2", Metric = 1, Protocol = "Static" }
                    }
                },
                new RouteScenario
                {
                    DestinationIP = "10.20.30.1",
                    CorrectIndex = 2,
                    Explanation = "10.20.30.0/24 tiene prefijo 24. 10.0.0.0/8 tambien coincide pero con prefijo 8. Gana la mas especifica.",
                    Options = new List<RouteOption>
                    {
                        new RouteOption { DestinationNetwork = "10.0.0.0", SubnetMask = "255.0.0.0", NextHop = "192.168.1.1", OutInterface = "G0/0", Metric = 5, Protocol = "RIP" },
                        new RouteOption { DestinationNetwork = "10.20.0.0", SubnetMask = "255.255.0.0", NextHop = "192.168.2.1", OutInterface = "G0/1", Metric = 3, Protocol = "OSPF" },
                        new RouteOption { DestinationNetwork = "10.20.30.0", SubnetMask = "255.255.255.0", NextHop = "192.168.3.1", OutInterface = "G0/2", Metric = 2, Protocol = "Static" },
                        new RouteOption { DestinationNetwork = "0.0.0.0", SubnetMask = "0.0.0.0", NextHop = "192.168.1.254", OutInterface = "G0/3", Metric = 1, Protocol = "Static" }
                    }
                }
            };
        }

        /// <summary>
        /// Avanza al siguiente escenario de ruta. Incrementa el indice y reinicia el estado de seleccion.
        /// </summary>
        public void NextScenario()
        {
            currentScenarioIndex = (currentScenarioIndex + 1) % scenarios.Count;
            selectedOptionIndex = -1;
            answered = false;

            ShowScenario(currentScenarioIndex);
        }

        /// <summary>
        /// Muestra el escenario en el indice dado: actualiza textos, limpia botones previos y crea botones para cada opcion de ruta.
        /// </summary>
        /// <param name="index">Indice del escenario a mostrar.</param>
        private void ShowScenario(int index)
        {
            var scenario = scenarios[index];

            if (destIPText != null)
            {
                destIPText.text = $"Destino: {scenario.DestinationIP}";
            }

            if (resultText != null)
            {
                resultText.text = "Selecciona la mejor ruta para llegar al destino.";
                resultText.color = Color.white;
            }

            if (scoreText != null)
            {
                scoreText.text = $"Puntaje: {score} / {totalAnswered}";
            }

            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(false);
            }

            ClearOptionButtons();

            Font arialFont = UIComponents.GetFont();

            GameObject panelObj = FindBestRoutePanel();
            if (panelObj == null) return;

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            if (panelRect == null) return;
            float panelHeight = panelRect.sizeDelta.y;
            float startY = 120;
            float yStep = 80;

            int routesCount = scenario.Options.Count;
            float totalHeight = routesCount * yStep + 40;
            float adjustedStartY = Mathf.Min(startY, (panelHeight / 2) - 20);

            optionButtons.Clear();
            for (int i = 0; i < scenario.Options.Count; i++)
            {
                int capturedIndex = i;
                var route = scenario.Options[i];
                float yPos = adjustedStartY - (i * yStep);

                GameObject btnObj = new GameObject("RouteBtn_" + i);
                btnObj.transform.SetParent(panelObj.transform, false);
                RectTransform btnRect = btnObj.AddComponent<RectTransform>();
                btnRect.anchorMin = new Vector2(0.5f, 0.5f);
                btnRect.anchorMax = new Vector2(0.5f, 0.5f);
                btnRect.anchoredPosition = new Vector2(0, yPos);
                btnRect.sizeDelta = new Vector2(700, 70);

                Image btnImg = btnObj.AddComponent<Image>();
                btnImg.color = new Color(0.15f, 0.15f, 0.2f, 1);
                btnImg.raycastTarget = true;

                Button btn = btnObj.AddComponent<Button>();
                btn.targetGraphic = btnImg;
                btn.transition = Selectable.Transition.ColorTint;
                ColorBlock colors = btn.colors;
                colors.highlightedColor = new Color(0.25f, 0.25f, 0.35f, 1);
                btn.colors = colors;

                GameObject textObj = new GameObject("RouteText_" + i);
                textObj.transform.SetParent(btnObj.transform, false);
                RectTransform textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(12, 4);
                textRect.offsetMax = new Vector2(-12, -4);

                Text label = textObj.AddComponent<Text>();
                label.text = string.Format("{0}/{1}  via  {2}  ({3}, metrica {4})",
                    route.DestinationNetwork,
                    SimRedes.Network.IPValidation.GetPrefixLength(route.SubnetMask).ToString(),
                    route.NextHop,
                    route.Protocol,
                    route.Metric);
                label.font = arialFont;
                label.fontSize = 22;
                label.color = Color.white;
                label.alignment = TextAnchor.MiddleLeft;

                btn.onClick.AddListener(() => OnRouteSelected(capturedIndex));
                optionButtons.Add(btn);
            }
        }

        /// <summary>
        /// Maneja la seleccion de una opcion de ruta. Evalua si es correcta, actualiza puntaje, colorea los botones y habilita el boton siguiente.
        /// </summary>
        /// <param name="optionIndex">Indice de la opcion seleccionada por el usuario.</param>
        private void OnRouteSelected(int optionIndex)
        {
            if (answered) return;
            answered = true;
            selectedOptionIndex = optionIndex;
            totalAnswered++;

            var scenario = scenarios[currentScenarioIndex];

            bool correct = (optionIndex == scenario.CorrectIndex);
            if (correct) score++;

            if (resultText != null)
            {
                if (correct)
                {
                    resultText.text = string.Format("CORRECTO! {0}", scenario.Explanation);
                    resultText.color = Color.green;
                }
                else
                {
                    var correctOption = scenario.Options[scenario.CorrectIndex];
                    resultText.text = string.Format(
                        "INCORRECTO. La mejor ruta era: {0}/{1} via {2} ({3}, metrica {4}). {5}",
                        correctOption.DestinationNetwork,
                        SimRedes.Network.IPValidation.GetPrefixLength(correctOption.SubnetMask).ToString(),
                        correctOption.NextHop,
                        correctOption.Protocol,
                        correctOption.Metric,
                        scenario.Explanation);
                    resultText.color = Color.red;
                }
            }

            if (scoreText != null)
            {
                scoreText.text = string.Format("Puntaje: {0} / {1}", score, totalAnswered);
            }

            for (int i = 0; i < optionButtons.Count; i++)
            {
                var colors = optionButtons[i].colors;
                if (i == scenario.CorrectIndex)
                {
                    colors.normalColor = Color.green;
                    optionButtons[i].GetComponent<Image>().color = new Color(0, 0.5f, 0, 0.5f);
                }
                else if (i == optionIndex && !correct)
                {
                    colors.normalColor = Color.red;
                    optionButtons[i].GetComponent<Image>().color = new Color(0.5f, 0, 0, 0.5f);
                }
                optionButtons[i].colors = colors;
                optionButtons[i].interactable = false;
            }

            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Destruye los botones de opcion de ruta creados en el panel y limpia la lista de botones.
        /// </summary>
        private void ClearOptionButtons()
        {
            GameObject panelObj = FindBestRoutePanel();
            if (panelObj == null) return;

            var toDestroy = new List<GameObject>();
            foreach (Transform child in panelObj.transform)
            {
                if (child.gameObject.name.StartsWith("RouteBtn_") || child.gameObject.name.StartsWith("RouteText_"))
                {
                    toDestroy.Add(child.gameObject);
                }
            }
            foreach (var obj in toDestroy)
            {
                // B4: liberar los Sprite de los botones antes de destruirlos
                UIComponents.SafeDestroyPanelSprites(obj);
                Destroy(obj);
            }
            optionButtons.Clear();
        }

        private GameObject FindBestRoutePanel()
        {
            GameObject panelObj = GameObject.Find("BestRoutePanel");
            return panelObj;
        }

    }
}
