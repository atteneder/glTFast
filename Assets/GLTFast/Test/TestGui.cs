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

        string[] names = {
            "2CylinderEngine",
            "AlphaBlendModeTest",
            "AnimatedCube",
            "AnimatedMorphCube",
            "AnimatedMorphSphere",
            "AnimatedTriangle",
            "Avocado",
            "BarramundiFish",
            "BoomBox",
            "BoomBoxWithAxes",
            "Box",
            "BoxAnimated",
            "BoxInterleaved",
            "BoxTextured",
            "BoxTexturedNonPowerOfTwo",
            "BoxVertexColors",
            "BrainStem",
            "Buggy",
            "Cameras",
            "CesiumMan",
            "CesiumMilkTruck",
            "Corset",
            "Cube",
            "DamagedHelmet",
            "Duck",
            "FlightHelmet",
            "GearboxAssy",
            "Lantern",
            "MetalRoughSpheres",
            "Monster",
            "MorphPrimitivesTest",
            "MultiUVTest",
            "NormalTangentMirrorTest",
            "NormalTangentTest",
            "OrientationTest",
            "ReciprocatingSaw",
            "RiggedFigure",
            "RiggedSimple",
            "SciFiHelmet",
            "SimpleMeshes",
            "SimpleMorph",
            "SimpleSparseAccessor",
            "SpecGlossVsMetalRough",
            "Sponza",
            "Suzanne",
            "TextureCoordinateTest",
            "TextureSettingsTest",
            "TextureTransformTest",
            "Triangle",
            "TriangleWithoutIndices",
            "TwoSidedPlane",
            "VC",
            "VertexColorTest",
            "WaterBottle",
        };

        bool binary = false;
        foreach( var n in names ) {
            testItems.Add(
                n,
                string.Format(
                    "{0}{1}/glTF{2}/{1}.gl{3}"
                    ,prefix
                    ,n
                    ,binary ? "-binary" : ""
                    ,binary ? "b" : "tf"
                    )
                );
        }

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
