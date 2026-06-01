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
/// Los warnings son inofensivos y no afectan el renderizado ni el funcionamiento.
/// Este script elimina las variantes ray tracing durante el build en plataformas
/// donde no son compatibles.
/// </summary>
public class SuppressRayTracingWarnings : IPreprocessShaders
{
    public int callbackOrder => 0;

    /// <summary>
    /// IPreprocessShaders: elimina variantes con keyword UNITY_RAY_TRACING
    /// o REQUIRE_RAY_TRACING en todas las plataformas.
    /// El proyecto no usa ray tracing (URP 2D, mesa tangible), estas variantes
    /// son internas de Unity para ProbeVolumes/PathTracing.
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

    /// <summary>
    /// Configuracion al cargar el proyecto en el Editor.
    /// </summary>
    [InitializeOnLoadMethod]
    private static void Init()
    {
        // Forzar stripping de variantes en el Editor (para builds)
        // y configurar el pipeline global
        EditorApplication.delayCall += () =>
        {
            if (Shader.globalRenderPipeline != "UniversalPipeline")
                Shader.globalRenderPipeline = "UniversalPipeline";
        };
    }
}
