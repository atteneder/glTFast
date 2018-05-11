#if UNITY_GLTF
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.IO;
using GLTF;
using GLTF.Schema;
using UnityGLTF;
using UnityGLTF.Loader;

/// <summary>
/// This is a copy of UnityGLTF.GLTFComponent with the onLoadComplete event exposed.
/// </summary>
public class UnityGLTFLoader : MonoBehaviour
{


    public string GLTFUri;
    public bool Multithreaded = true;
    public bool UseStream = false;

    public int MaximumLod = 300;
    public GLTFSceneImporter.ColliderType Colliders = GLTFSceneImporter.ColliderType.None;

	public UnityAction onLoadComplete;

    IEnumerator Start()
    {
        GLTFSceneImporter sceneImporter = null;
        ILoader loader = null;

        if (UseStream)
        {
            // Path.Combine treats paths that start with the separator character
            // as absolute paths, ignoring the first path passed in. This removes
            // that character to properly handle a filename written with it.
            GLTFUri = GLTFUri.TrimStart(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
            string fullPath = Path.Combine(Application.streamingAssetsPath, GLTFUri);
            string directoryPath = URIHelper.GetDirectoryName(fullPath);
            loader = new FileLoader(directoryPath);
            sceneImporter = new GLTFSceneImporter(
                Path.GetFileName(GLTFUri),
                loader
                );
        }
        else
        {
            string directoryPath = URIHelper.GetDirectoryName(GLTFUri);
            loader = new WebRequestLoader(directoryPath);
            sceneImporter = new GLTFSceneImporter(
                URIHelper.GetFileFromUri(new Uri(GLTFUri)),
                loader
                );

        }

        sceneImporter.SceneParent = gameObject.transform;
        sceneImporter.Collider = Colliders;
        sceneImporter.MaximumLod = MaximumLod;
        yield return sceneImporter.LoadScene(-1, Multithreaded,HandleAction);
    }

    void HandleAction(GameObject obj)
    {
        if(onLoadComplete!=null) {
            onLoadComplete();
        }
    }
}
#endif
