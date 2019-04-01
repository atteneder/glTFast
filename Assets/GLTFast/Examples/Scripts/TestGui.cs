using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(TestLoader))]
public class TestGui : MonoBehaviour {

	static float barHeightWidth = 25;
    static float buttonWidth = 50;
    static float listWidth = 150;
    static float listItemHeight = 25;
    static float timeHeight = 20;

    public bool showMenu = true;
    // Load files locally (from streaming assets) or via HTTP
    public bool local = false;

    List<System.Tuple<string,string>> testItems = new List<System.Tuple<string, string>>();
    List<System.Tuple<string,string>> testItemsLocal = new List<System.Tuple<string, string>>();

    string urlField;

	float screenFactor;   

    float startTime = -1;
    float minFrame = float.MaxValue;
    float maxFrame = float.MinValue;
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
        
        StartCoroutine(InitGui());

        var tl = GetComponent<TestLoader>();
        tl.urlChanged += UrlChanged;
        tl.time1Update += Time1Update;
        tl.time2Update += Time2Update;
	}

    IEnumerator InitGui() {

        #if !UNITY_EDITOR
        string prefix = GltfSampleModels.baseUrl;
#else
        string prefix = GltfSampleModels.baseUrlLocal;
#endif

        var prefixLocal = GltfSampleModels.localPath;

        var names = new List<string>();

        yield return GltfSampleModels.LoadGltfFileUrls();
        names.AddRange(GltfSampleModels.gltfFileUrls);

        yield return GltfSampleModels.LoadGlbFileUrls();
        names.AddRange(GltfSampleModels.glbFileUrls);

        foreach( var n in names ) {
            var t = GltfSampleModels.GetNameFromPath(n);
            testItems.Add( new System.Tuple<string, string>(
                t,
                string.Format(
                    "{0}/{1}"
                    ,prefix
                    ,n
                    )
                )
            );
            testItemsLocal.Add( new System.Tuple<string, string>(
                t,
                string.Format(
                    "{0}/{1}"
                    ,prefixLocal
                    ,n
                    )
                )
            );
        }
	}

	void UrlChanged(string newUrl)
    {
        startTime = Time.realtimeSinceStartup;
        urlField = newUrl;
        minFrame = float.MaxValue;
        maxFrame = float.MinValue;
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

            guiStyle = GUI.skin.toggle;
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
            local = GUI.Toggle(new Rect(listWidth,barHeightWidth,listWidth*2,barHeightWidth),local,local?"local":"http");
            scrollPos = GUI.BeginScrollView(
                new Rect(0,barHeightWidth,listWidth,height-barHeightWidth),
                scrollPos,
                new Rect(0,0,listItemWidth, listItemHeight*testItems.Count)
            );

            GUIDrawItems( local ? testItemsLocal : testItems, listItemWidth );
    
            GUI.EndScrollView();
        }
        string label;

        float now = (Time.realtimeSinceStartup-startTime)*1000;
        #if !NO_GLTFAST
        if(startTime>=0 || time1>=0) {
            if(startTime>=0 && time1<0) UpdateFrameTimes();
			label = string.Format(
                "glTFast time: {0:0.00} ms (min: {1:0.0} ms max: {2:0.0} ms)"
                ,time1>=0 ? time1 : now
                ,minFrame < float.MaxValue ? minFrame : '-'
                ,maxFrame > float.MinValue ? maxFrame : '-'
                );
			GUI.Label(new Rect(listWidth+10,height-timeHeight,width-listWidth-10,timeHeight),label);
        }
        #endif
        #if UNITY_GLTF
        if(startTime>=0 || time2>=0) {
            if(startTime>=0 && time2<0) UpdateFrameTimes();
            label = string.Format(
                "UnityGLTF time: {0:0.00} ms (min: {1:0.0} ms max: {2:0.0} ms)"
                , time2>=0 ? time2 : now
                ,minFrame < float.MaxValue ? minFrame : '-'
                ,maxFrame > float.MinValue ? maxFrame : '-'
                );
            GUI.Label(new Rect(listWidth+10,height-timeHeight,width-listWidth-10,timeHeight),label);
        }
        #endif
	}

    void UpdateFrameTimes() {
        minFrame = Mathf.Min(minFrame, Time.deltaTime * 1000 );
        maxFrame = Mathf.Max(maxFrame, Time.deltaTime * 1000 );
    }
    void GUIDrawItems( List<System.Tuple<string,string>> items, float listItemWidth) {
        float y = 0;
        foreach( var item in items ) {
            if(GUI.Button(new Rect(0,y,listItemWidth,listItemHeight),item.Item1)) {
                GetComponent<TestLoader>().LoadUrl(item.Item2);
            }
            y+=listItemHeight;
        }
    }
    public void HideUI() {
        showMenu = false;
    }
}
