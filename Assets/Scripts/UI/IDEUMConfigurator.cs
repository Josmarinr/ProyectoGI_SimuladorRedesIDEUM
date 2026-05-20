using UnityEngine;

namespace SimRedes.UI
{
    public class IDEUMConfigurator : MonoBehaviour
    {
        [Header("Pantalla IDEUM 55\"")]
        [SerializeField] private int screenWidth = 4096;
        [SerializeField] private int screenHeight = 2160;
        [SerializeField] private float screenScale = 1.0f;

        [Header("References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private RectTransform canvasRect;

        private void Awake()
        {
            ConfigureForIDEUM();
        }

        private void ConfigureForIDEUM()
        {
            Screen.SetResolution(screenWidth, screenHeight, FullScreenMode.Windowed);

            if (mainCamera != null)
            {
                mainCamera.orthographic = true;
                mainCamera.orthographicSize = screenHeight / 2f * screenScale;
                mainCamera.transform.position = new Vector3(screenWidth / 2f, screenHeight / 2f, -10);
                mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
            }

            UnityEngine.Debug.Log($"[IDEUM] Configurado para {screenWidth}x{screenHeight}");
        }

        public void SetResolution(int width, int height)
        {
            screenWidth = width;
            screenHeight = height;
            ConfigureForIDEUM();
        }
    }
}