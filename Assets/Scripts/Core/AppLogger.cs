using UnityEngine;

namespace SimRedes.Core
{
    public static class AppLogger
    {
        public static bool EnableLogging = false;

        public static void Log(string tag, string message)
        {
            if (EnableLogging)
                UnityEngine.Debug.Log($"[{tag}] {message}");
        }

        public static void LogWarning(string tag, string message)
        {
            if (EnableLogging)
                UnityEngine.Debug.LogWarning($"[{tag}] {message}");
        }

        public static void LogError(string tag, string message)
        {
            if (EnableLogging)
                UnityEngine.Debug.LogError($"[{tag}] {message}");
        }
    }
}