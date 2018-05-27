using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TestLoader))]
public class TestGui : MonoBehaviour {

	static float barHeightWidth = 25;
    static float buttonWidth = 50;
    static float listWidth = 150;
    static float listItemHeight = 25;
    static float timeHeight = 20;

    public bool showMenu = true;

    Dictionary<string,string> testItems = new Dictionary<string, string>();

    string urlField;

	float screenFactor;   

    float startTime = -1;
#if !NO_GLTFAST
	float time1 = -1;
#endif
#if UNITY_GLTF
	float time2 = -1;
#endif

    Vector2 scrollPos;

	private void Awake()
	{

		screenFactor = Mathf.Max( 1, Mathf.Floor( Screen.dpi / 100f ));

		barHeightWidth *= screenFactor;
        buttonWidth *= screenFactor;
        listWidth *= screenFactor;
        listItemHeight *= screenFactor;
        timeHeight *= screenFactor;
        
#if PLATFORM_WEBGL && !UNITY_EDITOR
        // Hide UI in glTF compare web
        HideUI();
#endif

#if !UNITY_EDITOR
        string prefix = "https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/";
#else
        string prefix = "http://localhost:8080/glTF-Sample-Models/2.0/";
#endif

		testItems.Add("Duck", prefix+"Duck/glTF-Binary/Duck.glb");
        testItems.Add("AnimatedMorphCube", prefix+"AnimatedMorphCube/glTF-Binary/AnimatedMorphCube.glb");
        testItems.Add("DamagedHelmet", prefix+"DamagedHelmet/glTF-Binary/DamagedHelmet.glb");
        testItems.Add("Avocado", prefix+"Avocado/glTF-Binary/Avocado.glb");
        testItems.Add("BoxVertexColors", prefix+"BoxVertexColors/glTF-Binary/BoxVertexColors.glb");
        testItems.Add("Box", prefix+"Box/glTF-Binary/Box.glb");
        testItems.Add("BoxTextured", prefix+"BoxTexturedNonPowerOfTwo/glTF-Binary/BoxTextured.glb");
        testItems.Add("BoxInterleaved", prefix+"BoxInterleaved/glTF-Binary/BoxInterleaved.glb");
        testItems.Add("BoomBox", prefix+"BoomBox/glTF-Binary/BoomBox.glb");
        testItems.Add("Buggy", prefix+"Buggy/glTF-Binary/Buggy.glb");
        testItems.Add("CesiumMan", prefix+"CesiumMan/glTF-Binary/CesiumMan.glb");
        testItems.Add("2CylinderEngine", prefix+"2CylinderEngine/glTF-Binary/2CylinderEngine.glb");
        testItems.Add("Lantern", prefix+"Lantern/glTF-Binary/Lantern.glb");
        testItems.Add("WaterBottle", prefix+"WaterBottle/glTF-Binary/WaterBottle.glb");
        testItems.Add("BrainStem", prefix+"BrainStem/glTF-Binary/BrainStem.glb");
        testItems.Add("BarramundiFish", prefix+"BarramundiFish/glTF-Binary/BarramundiFish.glb");
        testItems.Add("ReciprocatingSaw", prefix+"ReciprocatingSaw/glTF-Binary/ReciprocatingSaw.glb");
        testItems.Add("MetalRoughSpheres", prefix+"MetalRoughSpheres/glTF-Binary/MetalRoughSpheres.glb");
        testItems.Add("TextureSettingsTest", prefix+"TextureSettingsTest/glTF-Binary/TextureSettingsTest.glb");
        testItems.Add("BoxAnimated", prefix+"BoxAnimated/glTF-Binary/BoxAnimated.glb");
        testItems.Add("VC", prefix+"VC/glTF-Binary/VC.glb");
        testItems.Add("CesiumMilkTruck", prefix+"CesiumMilkTruck/glTF-Binary/CesiumMilkTruck.glb");
        testItems.Add("AnimatedMorphSphere", prefix+"AnimatedMorphSphere/glTF-Binary/AnimatedMorphSphere.glb");
        testItems.Add("RiggedSimple", prefix+"RiggedSimple/glTF-Binary/RiggedSimple.glb");
        testItems.Add("Corset", prefix+"Corset/glTF-Binary/Corset.glb");
        testItems.Add("RiggedFigure", prefix+"RiggedFigure/glTF-Binary/RiggedFigure.glb");
        testItems.Add("GearboxAssy", prefix+"GearboxAssy/glTF-Binary/GearboxAssy.glb");
        testItems.Add("NormalTangentTest", prefix+"NormalTangentTest/glTF-Binary/NormalTangentTest.glb");
        testItems.Add("Monster", prefix+"Monster/glTF-Binary/Monster.glb");
        testItems.Add("BoxTextured2", prefix+"BoxTextured/glTF-Binary/BoxTextured.glb");

        var tl = GetComponent<TestLoader>();
        tl.urlChanged += UrlChanged;
        tl.time1Update += Time1Update;
        tl.time2Update += Time2Update;
	}

	void UrlChanged(string newUrl)
    {
        startTime = Time.realtimeSinceStartup;
        urlField = newUrl;
    }

    void Time1Update(float time)
    {
#if !NO_GLTFAST
		time1 = time;
#endif
    }

    void Time2Update(float time)
    {
#if UNITY_GLTF
		time2 = time;
#endif
    }

	private void OnGUI()
	{
		if(!float.IsNaN(screenFactor)) {
			// Init time gui style adjustments
			var guiStyle = GUI.skin.button;
            guiStyle.fontSize = Mathf.RoundToInt(14 * screenFactor);
			screenFactor = float.NaN;
		}

		float width = Screen.width;
		float height = Screen.height;

        if(showMenu) {
            GUI.BeginGroup( new Rect(0,0,width,barHeightWidth) );
            urlField = GUI.TextField( new Rect(0,0,width-buttonWidth,barHeightWidth),urlField);
    		if(GUI.Button( new Rect(width-buttonWidth,0,buttonWidth,barHeightWidth),"Load")) {
                GetComponent<TestLoader>().LoadUrl(urlField);
            }
            GUI.EndGroup();
    
            float listItemWidth = listWidth-16;
            scrollPos = GUI.BeginScrollView(
                new Rect(0,barHeightWidth,listWidth,height-barHeightWidth),
                scrollPos,
                new Rect(0,0,listItemWidth, listItemHeight*testItems.Count)
            );
    
            float y=0;
            foreach( var item in testItems ) {
                if(GUI.Button(new Rect(0,y,listItemWidth,listItemHeight),item.Key)) {
                    GetComponent<TestLoader>().LoadUrl(item.Value);
                }
                y+=listItemHeight;
            }
    
            GUI.EndScrollView();
        }
        string label;

        float now = (Time.realtimeSinceStartup-startTime)*1000;
        #if !NO_GLTFAST
        if(startTime>=0 || time1>=0) {
			label = string.Format(
                "glTFast time: {0:0.00} ms"
                ,time1>=0 ? time1 : now
                );
			GUI.Label(new Rect(listWidth+10,height-timeHeight,width-listWidth-10,timeHeight),label);
        }
        #endif
        #if UNITY_GLTF
        if(startTime>=0 || time2>=0) {
            label = string.Format(
                "UnityGLTF time: {0:0.00} ms"
                , time2>=0 ? time2 : now
                );
            GUI.Label(new Rect(listWidth+10,height-timeHeight,width-listWidth-10,timeHeight),label);
        }
        #endif
	}

    public void HideUI() {
        showMenu = false;
    }
}
