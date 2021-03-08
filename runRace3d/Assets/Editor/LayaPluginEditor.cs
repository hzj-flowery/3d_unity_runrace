using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

public class LayaPluginEditor : EditorWindow
{
	private static string materialPath = "/Materials";
	[MenuItem("Tools/创建材质球")]
	static void CreateMaterial()
	{
		if(!Directory.Exists(Application.dataPath + materialPath))
		{
			AssetDatabase.CreateFolder("Assets", "Materials");
			AssetDatabase.Refresh();
		}
		GameObject[] selectObjs = Selection.gameObjects;
		List<GameObject> objs = GetAllObj(selectObjs);
		for(int i = 0; i < objs.Count; i++)
		{
			Debug.Log(objs[i].name);
			MeshRenderer meshRenderer = objs[i].GetComponent<MeshRenderer>();
			if(meshRenderer == null)
			{
				continue;
			}

			Material material = meshRenderer.sharedMaterial;
			if(material == null)
			{
				material = new Material(Shader.Find("Standard"));
			}
			else
			{
				Texture tex = material.mainTexture;
				material = new Material(Shader.Find("Standard"));
				material.mainTexture = tex;
			}
			AssetDatabase.CreateAsset(material, "Assets/Materials/" + objs[i].name + ".mat");
			meshRenderer.sharedMaterial = material;
		}
		AssetDatabase.Refresh();
	}

	[MenuItem("Tools/创建预制体")]
	static void CreatePrefab()
	{
		if(!Directory.Exists(Application.dataPath + "/Prefabs"))
		{
			AssetDatabase.CreateFolder("Assets", "Prefabs");
			AssetDatabase.Refresh();
		}
		string[] prefabIds = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/Prefabs" });
		string[] prefabNames = new string[prefabIds.Length];
		for(int i = 0; i < prefabIds.Length; i++)
		{
			string pName = AssetDatabase.GUIDToAssetPath(prefabIds[i]);
			pName = pName.Split(new string[] { "Assets/Prefabs/", ".prefab" }, StringSplitOptions.RemoveEmptyEntries)[0];
			prefabNames[i] = pName;
		}

		GameObject[] selectObjs = Selection.gameObjects;
		for(int i = 0; i < selectObjs.Length; i++)
		{
			//Debug.Log(selectObjs[i].name);
			//PrefabUtility.CreatePrefab("Assets/Prefabs/" + selectObjs[i].name + ".prefab", selectObjs[i]);

			string objName = selectObjs[i].name;
			objName = objName.Split(' ')[0];
			bool isHave = false;
			for(int j = 0; j < prefabNames.Length; j++)
			{
				if(string.Equals(objName, prefabNames[j]))
				{
					isHave = true;
					break;
				}
			}

			if(isHave)
			{
				Debug.Log(objName);
				GameObject pObj = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/" + objName + ".prefab", typeof(GameObject)) as GameObject;
				Vector3 p = selectObjs[i].transform.position;
				Vector3 r = selectObjs[i].transform.eulerAngles;
				Vector3 s = selectObjs[i].transform.localScale;
				string oName = selectObjs[i].name;

				GameObject cP = PrefabUtility.ConnectGameObjectToPrefab(selectObjs[i], pObj);
				cP.name = oName;
				cP.transform.position = p;
				cP.transform.eulerAngles = r;
				cP.transform.localScale = s;
			}
			else
			{
				var newprefab = PrefabUtility.CreateEmptyPrefab("Assets/Prefabs/" + objName + ".prefab");
				PrefabUtility.ReplacePrefab(selectObjs[i], newprefab, ReplacePrefabOptions.ConnectToPrefab);
			}
		}
		AssetDatabase.Refresh();
	}

	[MenuItem("Tools/应用预制体")]
	static void ApplyPrefab()
	{
		GameObject[] selectObjs = Selection.gameObjects;
		for(int i = 0; i < selectObjs.Length; i++)
		{
			PrefabType pType = PrefabUtility.GetPrefabType(selectObjs[i]);
			UnityEngine.Object prefabAsset = null;
			if(selectObjs[i] != null)
			{
				prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(selectObjs[i]);
				if(prefabAsset != null)
				{
					PrefabUtility.ReplacePrefab(selectObjs[i], prefabAsset, ReplacePrefabOptions.ConnectToPrefab);
				}
			}
		}
		AssetDatabase.Refresh();
	}

	[MenuItem("Tools/创建碰撞体")]
	static void CreateCollider()
	{
		GameObject[] selectObjs = Selection.gameObjects;
		List<GameObject> objs = GetAllObj(selectObjs);
		for(int i = 0; i < objs.Count; i++)
		{
			if(objs[i].GetComponent<BoxCollider>() != null)
			{
				continue;
			}

			if(objs[i].GetComponent<MeshRenderer>() == null)
			{
				continue;
			}
			objs[i].AddComponent<BoxCollider>();
		}
	}

	[MenuItem("Tools/移除碰撞体")]
	static void RemoveCollider()
	{
		GameObject[] selectObjs = Selection.gameObjects;
		List<GameObject> objs = GetAllObj(selectObjs);
		for(int i = 0; i < objs.Count; i++)
		{
			if(objs[i].GetComponent<BoxCollider>() == null)
			{
				continue;
			}
			DestroyImmediate(objs[i].GetComponent<BoxCollider>());
		}
	}

	[MenuItem("Tools/压缩动画")]
	public static void CompressAnim()
	{
		try
		{
			GameObject[] selectGos = Selection.gameObjects;
			for(int i = 0; i < selectGos.Length; i++)
			{
				string path = AssetDatabase.GetAssetPath(selectGos[i]);
				Debug.Log(path);
				UnityEngine.Object[] objects = AssetDatabase.LoadAllAssetsAtPath(path);

				foreach(var o in objects)
				{
					if(o is AnimationClip)
					{
						AnimationClip clip = o as AnimationClip;
						ReduceScaleKey(clip, "localscale");
						ReduceFloatPrecision(clip);
					}
				}
			}
		}
		catch(Exception)
		{
			EditorUtility.ClearProgressBar();
			throw;
		}
		EditorUtility.ClearProgressBar();
	}

	[MenuItem("Tools/压缩图片")]
	public static void CompressTex()
	{
		//UnityEngine.Object[] selectGos = Selection.objects;
		//for(int i = 0; i < selectGos.Length; i++)
		//{
		//	string path = AssetDatabase.GetAssetPath(selectGos[i]);
		//	Debug.Log(path);

		//	Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
		//	TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(path);
		//	ti.isReadable = true;
		//	AssetDatabase.ImportAsset(path);
		//	Texture2D newTex = TextureScale.Point(tex, 128, 128);
		//	byte[] buffer = newTex.EncodeToPNG();
		//	Debug.Log(Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path) + "-low" + Path.GetExtension(path));
		//	File.WriteAllBytes(Path.GetDirectoryName(path) +"/" +Path.GetFileNameWithoutExtension(path) + "-low" + Path.GetExtension(path), buffer);
		//}
		//AssetDatabase.Refresh();
	}


	private static void ReduceScaleKey(AnimationClip clip, string keyName)
	{
		EditorCurveBinding[] curves = AnimationUtility.GetCurveBindings(clip);

		for(int j = 0; j < curves.Length; j++)
		{
			EditorCurveBinding curveBinding = curves[j];

			if(curveBinding.propertyName.ToLower().Contains(keyName))
			{
				AnimationUtility.SetEditorCurve(clip, curveBinding, null);
			}
		}
	}

	private static void ReduceFloatPrecision(AnimationClip clip)
	{
		EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);

		for(int j = 0; j < bindings.Length; j++)
		{
			EditorCurveBinding curveBinding = bindings[j];
			AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, curveBinding);

			if(curve == null || curve.keys == null)
			{
				continue;
			}

			Keyframe[] keys = curve.keys;
			for(int k = 0; k < keys.Length; k++)
			{
				Keyframe key = keys[k];
				key.value = float.Parse(key.value.ToString("f3"));
				key.inTangent = float.Parse(key.inTangent.ToString("f3"));
				key.outTangent = float.Parse(key.outTangent.ToString("f3"));
				keys[k] = key;
			}
			curve.keys = keys;

			AnimationUtility.SetEditorCurve(clip, curveBinding, curve);
		}
	}

	static List<GameObject> GetAllObj(GameObject[] parentObjs)
	{
		List<GameObject> objs = new List<GameObject>();
		for(int i = 0; i < parentObjs.Length; i++)
		{
			for(int j = 0; j < parentObjs[i].GetComponentsInChildren<Transform>().Length; j++)
			{
				Debug.Log(parentObjs[i].GetComponentsInChildren<Transform>()[j].name);
				objs.Add(parentObjs[i].GetComponentsInChildren<Transform>()[j].gameObject);
			}
		}
		return objs;
	}
}

public class AnimCompress : AssetPostprocessor
{
	public const string ScaleKeyName = "localscale";
	public const string RotationKeyName = "localrotation";

	public static void OnPostprocessModel(GameObject go)
	{
		List<AnimationClip> clips = new List<AnimationClip>(AnimationUtility.GetAnimationClips(go));
		if(clips.Count == 0)
		{
			AnimationClip[] objectList = UnityEngine.Object.FindObjectsOfType(typeof(AnimationClip)) as AnimationClip[];
			//AnimationClip[] objectList = go .FindObjectsOfType(typeof(AnimationClip)) as AnimationClip[];
			if(objectList != null)
			{
				clips.AddRange(objectList);
			}
		}

		Debug.Log(clips.Count);

		for(int i = 0; i < clips.Count; i++)
		{
			CompressAnim(clips[i]);
		}
	}

	public static void CompressAnim(AnimationClip clip)
	{
		ReduceScaleKey(clip, ScaleKeyName);
		ReduceFloatPrecision(clip);
	}

	private static void ReduceScaleKey(AnimationClip clip, string keyName)
	{
		EditorCurveBinding[] curves = AnimationUtility.GetCurveBindings(clip);

		for(int j = 0; j < curves.Length; j++)
		{
			EditorCurveBinding curveBinding = curves[j];

			if(curveBinding.propertyName.ToLower().Contains(keyName))
			{
				AnimationUtility.SetEditorCurve(clip, curveBinding, null);
			}
		}
	}

	private static void ReduceFloatPrecision(AnimationClip clip)
	{
		EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);

		for(int j = 0; j < bindings.Length; j++)
		{
			EditorCurveBinding curveBinding = bindings[j];
			AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, curveBinding);

			if(curve == null || curve.keys == null)
			{
				continue;
			}

			Keyframe[] keys = curve.keys;
			for(int k = 0; k < keys.Length; k++)
			{
				Keyframe key = keys[k];
				key.value = float.Parse(key.value.ToString("f3"));
				key.inTangent = float.Parse(key.inTangent.ToString("f3"));
				key.outTangent = float.Parse(key.outTangent.ToString("f3"));
				keys[k] = key;
			}
			curve.keys = keys;

			AnimationUtility.SetEditorCurve(clip, curveBinding, curve);
		}
	}
}
