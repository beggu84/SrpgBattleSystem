using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEditor;

public class SrpgMtUtils
{
	public static void AddLayer(string layerName)
	{
		SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
		SerializedProperty layersProp = tagManager.FindProperty("layers");

		for (int i = 8; i < layersProp.arraySize; i++)
		{
			SerializedProperty prop = layersProp.GetArrayElementAtIndex(i);
			if (prop.stringValue == layerName)
				return;
		}

		for (int i = 8; i < layersProp.arraySize; i++)
		{
			SerializedProperty prop = layersProp.GetArrayElementAtIndex(i);
			if (prop.stringValue.Length == 0)
			{
				prop.stringValue = layerName;
				tagManager.ApplyModifiedProperties();
				break;
			}
		}
	}

    public static void AddTag(string tagName)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty prop = tagsProp.GetArrayElementAtIndex(i);
            if (prop.stringValue == tagName)
                return;
        }

        tagsProp.InsertArrayElementAtIndex(0);
        SerializedProperty newProp = tagsProp.GetArrayElementAtIndex(0);
        newProp.stringValue = tagName;

        tagManager.ApplyModifiedProperties();
    }

    public static void ShowUnityGrid(bool show)
	{
		Assembly editorAssembly = Assembly.GetAssembly(typeof(Editor));
		Type annotationUtility = editorAssembly.GetType("UnityEditor.AnnotationUtility");
		PropertyInfo property = annotationUtility.GetProperty("showGrid", BindingFlags.Static | BindingFlags.NonPublic);
		property.SetValue(null, show, null);
	}

    public static void SetupGizmos(bool enable)
    {
        Assembly asm = Assembly.GetAssembly(typeof(Editor));
        Type type = asm.GetType("UnityEditor.AnnotationUtility");
        if (type != null)
        {
            MethodInfo getAnnotations = type.GetMethod("GetAnnotations", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo setGizmoEnabled = type.GetMethod("SetGizmoEnabled", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo setIconEnabled = type.GetMethod("SetIconEnabled", BindingFlags.Static | BindingFlags.NonPublic);
            var annotations = getAnnotations.Invoke(null, null);
            foreach (object annotation in (IEnumerable)annotations)
            {
                Type annotationType = annotation.GetType();
                FieldInfo classIdField = annotationType.GetField("classID", BindingFlags.Public | BindingFlags.Instance);
                FieldInfo scriptClassField = annotationType.GetField("scriptClass", BindingFlags.Public | BindingFlags.Instance);
                if (classIdField != null && scriptClassField != null)
                {
                    int classId = (int)classIdField.GetValue(annotation);
                    string scriptClass = (string)scriptClassField.GetValue(annotation);

                    int value = -1;
                    if (scriptClass == "SrpgCanvas")
                        value = enable ? 1 : 0;
                    else if (classId == 20) // camera
                        value = enable ? 0 : 1;

                    setGizmoEnabled.Invoke(null, new object[] { classId, scriptClass, value });
                    setIconEnabled.Invoke(null, new object[] { classId, scriptClass, value });
                }
            }
        }
    }

    public static Texture2D MakeAlphaTexture(int width, int height, Color color)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
                tex.SetPixel(x, y, color);
        }
        tex.Apply();

        return tex;
    }

    //private Texture2D ScaleTexture(Texture2D source, int destWidth, int desetHeight)
    //{
    //    Texture2D dest = new Texture2D(destWidth, desetHeight, source.format, true);
    //    Color[] pixels = dest.GetPixels(0);
    //    float incX = 1.0f / (float)destWidth;
    //    float incY = 1.0f / (float)desetHeight;
    //    for (int i = 0; i < pixels.Length; i++)
    //        pixels[i] = source.GetPixelBilinear(incX * ((float)i % destWidth), incY * ((float)Mathf.Floor(i / destWidth)));

    //    dest.SetPixels(pixels, 0);
    //    dest.Apply();

    //    return dest;
    //}
}
