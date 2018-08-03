#if !NO_TEST
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GLTFast;

public class SampleModelsTest
{
	static readonly string[] glbFiles = new string[] {
		"glTF-Sample-Models/2.0/AnimatedMorphCube/glTF-Binary/AnimatedMorphCube.glb",
		"glTF-Sample-Models/2.0/DamagedHelmet/glTF-Binary/DamagedHelmet.glb",
		"glTF-Sample-Models/2.0/TextureCoordinateTest/glTF-Binary/TextureCoordinateTest.glb",
		"glTF-Sample-Models/2.0/Avocado/glTF-Binary/Avocado.glb",
		"glTF-Sample-Models/2.0/VertexColorTest/glTF-Binary/VertexColorTest.glb",
		"glTF-Sample-Models/2.0/BoxVertexColors/glTF-Binary/BoxVertexColors.glb",
		"glTF-Sample-Models/2.0/Box/glTF-Binary/Box.glb",
		"glTF-Sample-Models/2.0/BoxTexturedNonPowerOfTwo/glTF-Binary/BoxTexturedNonPowerOfTwo.glb",
		"glTF-Sample-Models/2.0/BoxInterleaved/glTF-Binary/BoxInterleaved.glb",
		"glTF-Sample-Models/2.0/BoomBox/glTF-Binary/BoomBox.glb",
		"glTF-Sample-Models/2.0/Buggy/glTF-Binary/Buggy.glb",
		"glTF-Sample-Models/2.0/CesiumMan/glTF-Binary/CesiumMan.glb",
		"glTF-Sample-Models/2.0/AlphaBlendModeTest/glTF-Binary/AlphaBlendModeTest.glb",
		"glTF-Sample-Models/2.0/Duck/glTF-Binary/Duck.glb",
		"glTF-Sample-Models/2.0/2CylinderEngine/glTF-Binary/2CylinderEngine.glb",
		"glTF-Sample-Models/2.0/Lantern/glTF-Binary/Lantern.glb",
		"glTF-Sample-Models/2.0/WaterBottle/glTF-Binary/WaterBottle.glb",
		"glTF-Sample-Models/2.0/BrainStem/glTF-Binary/BrainStem.glb",
		"glTF-Sample-Models/2.0/BarramundiFish/glTF-Binary/BarramundiFish.glb",
		"glTF-Sample-Models/2.0/NormalTangentMirrorTest/glTF-Binary/NormalTangentMirrorTest.glb",
		"glTF-Sample-Models/2.0/ReciprocatingSaw/glTF-Binary/ReciprocatingSaw.glb",
		"glTF-Sample-Models/2.0/MetalRoughSpheres/glTF-Binary/MetalRoughSpheres.glb",
		"glTF-Sample-Models/2.0/TextureSettingsTest/glTF-Binary/TextureSettingsTest.glb",
		"glTF-Sample-Models/2.0/BoxAnimated/glTF-Binary/BoxAnimated.glb",
		"glTF-Sample-Models/2.0/VC/glTF-Binary/VC.glb",
		"glTF-Sample-Models/2.0/OrientationTest/glTF-Binary/OrientationTest.glb",
		"glTF-Sample-Models/2.0/CesiumMilkTruck/glTF-Binary/CesiumMilkTruck.glb",
		"glTF-Sample-Models/2.0/AnimatedMorphSphere/glTF-Binary/AnimatedMorphSphere.glb",
		"glTF-Sample-Models/2.0/RiggedSimple/glTF-Binary/RiggedSimple.glb",
		"glTF-Sample-Models/2.0/Corset/glTF-Binary/Corset.glb",
		"glTF-Sample-Models/2.0/RiggedFigure/glTF-Binary/RiggedFigure.glb",
		"glTF-Sample-Models/2.0/GearboxAssy/glTF-Binary/GearboxAssy.glb",
		"glTF-Sample-Models/2.0/NormalTangentTest/glTF-Binary/NormalTangentTest.glb",
		"glTF-Sample-Models/2.0/Monster/glTF-Binary/Monster.glb",
		"glTF-Sample-Models/2.0/BoxTextured/glTF-Binary/BoxTextured.glb",
	};

	[Test]
	public void SampleModelsTestCheckFiles()
	{
#if !(UNITY_ANDROID && !UNITY_EDITOR)
		foreach (var file in glbFiles)
		{
			var path = Path.Combine(Application.streamingAssetsPath, file);
			Assert.IsTrue(
				File.Exists(path)
				, "glb file {0} not found"
				, path
			);
		}
#else
		// See https://docs.unity3d.com/Manual/StreamingAssets.html
		Debug.Log("File access doesn't work on Android");
#endif
	}
 
	[UnityTest]
	public IEnumerator SampleModelsTestLoadAllGlb()
	{      
		foreach (var file in glbFiles)
		{
			var path = string.Format(
#if UNITY_ANDROID && !UNITY_EDITOR
				"{0}"
#else
				"file://{0}"
#endif
			    ,Path.Combine(Application.streamingAssetsPath, file)
			);
                             
			Debug.LogFormat("Testing {0}", path);

			var www = new WWW(path);
			yield return www;
			Assert.Null(www.error,www.error);
			var bytes = www.bytes;

			Assert.NotNull(bytes);
			Assert.Greater(bytes.Length, 0);

			var go = new GameObject();         
			var glTFast = new GLTFast.GLTFast();
            var success = glTFast.LoadGlb(bytes, go.transform);
            Assert.True(success);
			yield return null;
			Object.Destroy(go);
        }
	}
}
#endif
