using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

public static class ShaderVariantTool {

	delegate void Callback(List<string> strings);

	public abstract class KeywordIteratorBase {
		public abstract IEnumerator IterateKeywords();
	}

	public class KeywordIterator : KeywordIteratorBase {
		
		bool includeNone;
		string[] keywordElements;

		public KeywordIterator( string[] keywordElements, bool includeNone=true ) {
			this.keywordElements = keywordElements;
			this.includeNone = includeNone;
		}

		public override IEnumerator IterateKeywords() {
			if(includeNone) {
				yield return null;
			}
			foreach( var k in keywordElements ) {
				yield return k;
			}
		}
	}

	static string ListToString(List<string> list) {
		var sb = new StringBuilder();
		foreach(var s in list) {
			sb.Append(s);
			sb.Append(';');
		}
		return sb.ToString();
	}

	static void Helper( KeywordIterator[] kwis, List<string> fixValues, int startIndex, Callback cb ) {

		var it = kwis[startIndex].IterateKeywords();
		it.MoveNext();

		List<string> kws;

		do {
			kws = new List<string>(fixValues);

			if(it.Current!=null) {
				var keywords = ((string)it.Current).Split(';');
				kws.AddRange(keywords);
				// kws.Add((string)it.Current);
			}
			
			if( (startIndex+1)<kwis.Length) {
				Helper(kwis, kws, startIndex+1, cb);
			} else {
				cb(kws);
			}
		} while(it.MoveNext());
	}

	[MenuItem("Tools/glTFast/Create Shader Variant")]
	static void CreateShaderVariant () {
		var shader = Shader.Find("Standard");
		var passes = new PassType[] { PassType.ForwardBase, PassType.ForwardAdd };
		var sh = new ShaderVariantCollection();

		var kwis = new KeywordIterator[] {
			new KeywordIterator( new string[] {
				"DIRECTIONAL"
			}, false),
			new KeywordIterator( new string[] {
				"_NORMALMAP"
			}),
			new KeywordIterator( new string[] {
				"FOG_EXP2"
			}),
			new KeywordIterator( new string[] {
				"_ALPHATEST_ON",
				"_ALPHABLEND_ON",
				"_ALPHAPREMULTIPLY_ON"
			})
		};

		foreach(var pass in passes) {

			Helper( kwis, new List<string>(), 0, keywords => {
				try {
					var variant = new ShaderVariantCollection.ShaderVariant(shader, pass, keywords.ToArray() );
					sh.Add( variant );
				} catch (System.ArgumentException e) {
					Debug.LogWarningFormat("{0} not added: {1}",ListToString(keywords),e.Message);
				}
			} 	);
		}

		AssetDatabase.CreateAsset(sh,"Assets/TestVariant.shadervariants");
	}
}
