using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassLoader : MonoBehaviour {

    public string prefix = "http://localhost:8080/glTF-Sample-Models/2.0/";
    public string localPrefix = "/Users/Shared/SDK/UnityGLTF/UnityGLTF/www/";

    public string[] urls = {
        "AnimatedMorphCube/glTF-Binary/AnimatedMorphCube.glb",
        "DamagedHelmet/glTF-Binary/DamagedHelmet.glb",
        "Avocado/glTF-Binary/Avocado.glb",
        "BoxVertexColors/glTF-Binary/BoxVertexColors.glb",
        "Box/glTF-Binary/Box.glb",
        "BoxTexturedNonPowerOfTwo/glTF-Binary/BoxTextured.glb",
        "BoxInterleaved/glTF-Binary/BoxInterleaved.glb",
        "BoomBox/glTF-Binary/BoomBox.glb",
        "Buggy/glTF-Binary/Buggy.glb",
        "CesiumMan/glTF-Binary/CesiumMan.glb",
        "Duck/glTF-Binary/Duck.glb",
        "2CylinderEngine/glTF-Binary/2CylinderEngine.glb",
        "Lantern/glTF-Binary/Lantern.glb",
        "WaterBottle/glTF-Binary/WaterBottle.glb",
        "BrainStem/glTF-Binary/BrainStem.glb",
        "BarramundiFish/glTF-Binary/BarramundiFish.glb",
        "ReciprocatingSaw/glTF-Binary/ReciprocatingSaw.glb",
        "MetalRoughSpheres/glTF-Binary/MetalRoughSpheres.glb",
        "TextureSettingsTest/glTF-Binary/TextureSettingsTest.glb",
        "BoxAnimated/glTF-Binary/BoxAnimated.glb",
        "VC/glTF-Binary/VC.glb",
        "CesiumMilkTruck/glTF-Binary/CesiumMilkTruck.glb",
        "AnimatedMorphSphere/glTF-Binary/AnimatedMorphSphere.glb",
        "RiggedSimple/glTF-Binary/RiggedSimple.glb",
        "Corset/glTF-Binary/Corset.glb",
        "RiggedFigure/glTF-Binary/RiggedFigure.glb",
        "GearboxAssy/glTF-Binary/GearboxAssy.glb",
        "NormalTangentTest/glTF-Binary/NormalTangentTest.glb",
        "Monster/glTF-Binary/Monster.glb",
        "BoxTextured/glTF-Binary/BoxTextured.glb"
    };

	// Use this for initialization
	IEnumerator Start () {

        // Wait a bit to make sure profiling works
        yield return new WaitForSeconds(1);

		foreach( var url in urls ) {
            var go = new GameObject(System.IO.Path.GetFileNameWithoutExtension(url));

#if UNITY_GLTF
            var gltf = go.AddComponent<UnityGLTF.GLTFComponent>();
            gltf.GLTFUri = prefix+url;
#endif
            
#if !NO_GLTFAST
            GLTFast.GLTFast.LoadGlbFile( localPrefix+url, go.transform );
#endif
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
