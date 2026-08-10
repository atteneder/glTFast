using UnityEngine;
using GLTFast;

namespace GLTFast.Documentation.Examples
{
    /// <summary>
    /// Loads a local GLB file at runtime using glTFast.
    /// WebP textures are decoded automatically when the com.netpyoung.webp package is installed.
    /// No special setup required — just set the file path in the Inspector and press Play.
    /// </summary>
    public class LoadGltfWithWebp : MonoBehaviour
    {
        [Header("Model")]
        [Tooltip("Full path to a local .glb file.\nExample: D:\\Models\\mymodel.glb")]
        public string glbFilePath = "";

        GltfImport m_GltfImport;

        async void Start()
        {
            if (string.IsNullOrWhiteSpace(glbFilePath))
            {
                Debug.LogError("[GlbLoader] No file path set! Set the path in the Inspector.");
                return;
            }

            string fileUri = "file:///" + glbFilePath.Replace("\\", "/");
            Debug.Log($"[GlbLoader] Loading: {fileUri}");

            m_GltfImport = new GltfImport();
            bool success = await m_GltfImport.Load(fileUri);

            if (!success)
            {
                Debug.LogError("[GlbLoader] Load FAILED. Check console for errors.");
                return;
            }

            Debug.Log("[GlbLoader] Model loaded successfully.");

            bool instantiated = await m_GltfImport.InstantiateMainSceneAsync(transform);

            if (instantiated)
            {
                Debug.Log("[GlbLoader] Model instantiated!");
            }
            else
            {
                Debug.LogError("[GlbLoader] Instantiation failed.");
            }
        }

        void OnDestroy()
        {
            m_GltfImport?.Dispose();
        }
    }
}
