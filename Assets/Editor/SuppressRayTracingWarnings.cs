using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Suprime warnings de compilacion de shaders ray tracing en plataformas sin soporte.
///
/// Unity 6000.4 + URP 17.4 incluye shaders del modulo UnifiedRayTracing que contienen
/// #pragma require raytracing. En macOS/Metal la compilacion falla con:
///   "Platform doesn't support Ray Tracing Shader compilation"
///
/// Estos shaders (TraceRenderingLayerMask, DynamicGISkyOcclusion, TraceVirtualOffset)
/// son internos de Unity para ProbeVolumes. Las variantes RT fallan en macOS, pero
/// las variantes compute shader (CS) funcionan correctamente.
///
/// El proyecto no usa ray tracing (simulador 2D para mesa tangible IDEUM).
/// Este script elimina las variantes ray tracing tanto en build como en el Editor.
/// </summary>
public class SuppressRayTracingWarnings : IPreprocessShaders
{
    public int callbackOrder => 0;

    /// <summary>
    /// IPreprocessShaders: elimina variantes con keyword UNITY_RAY_TRACING
    /// durante la compilacion para builds (Windows x86_64).
    /// </summary>
    public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
    {
        var rtKeyword = new ShaderKeyword("UNITY_RAY_TRACING");
        if (!rtKeyword.IsValid()) return;

        for (int i = data.Count - 1; i >= 0; i--)
        {
            if (data[i].shaderKeywordSet.IsEnabled(rtKeyword))
            {
                data.RemoveAt(i);
            }
        }
    }

    // ==================== EDITOR-TIME FILTER ====================

    /// <summary>
    /// Registra el filtro de logs al cargar el proyecto en el Editor.
    /// Intercepta los warnings de compilacion de shaders ray tracing
    /// antes de que lleguen a la consola.
    /// </summary>
    [InitializeOnLoadMethod]
    private static void Init()
    {
        // Asegurar que el pipeline global esta configurado
        if (Shader.globalRenderPipeline != "UniversalPipeline")
            Shader.globalRenderPipeline = "UniversalPipeline";

        // Reemplazar el log handler para filtrar warnings especificos
        var logger = Debug.unityLogger;
        if (logger.logHandler is RayTracingLogFilter) return; // ya aplicado
        logger.logHandler = new RayTracingLogFilter(logger.logHandler);
    }
}

/// <summary>
/// Filtro de logs que suprime los warnings de compilacion de shaders
/// ray tracing en el Editor. No afecta otros logs.
/// </summary>
internal class RayTracingLogFilter : ILogHandler
{
    private readonly ILogHandler originalHandler;

    public RayTracingLogFilter(ILogHandler original)
    {
        originalHandler = original;
    }

    public void LogFormat(LogType logType, Object context, string format, params object[] args)
    {
        // Suprimir warnings de compilacion de shaders ray tracing
        if (logType == LogType.Warning && format != null &&
            (format.Contains("Ray Tracing Shader") ||
             format.Contains("raytracing") ||
             format.Contains("UNITY_RAY_TRACING")))
        {
            return; // no pasar al handler original
        }

        originalHandler.LogFormat(logType, context, format, args);
    }

    public void LogException(System.Exception exception, Object context)
    {
        originalHandler.LogException(exception, context);
    }
}
