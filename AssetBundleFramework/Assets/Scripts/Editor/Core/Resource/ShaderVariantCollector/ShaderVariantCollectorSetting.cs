using UnityEngine;
using UnityEditor;

public class ShaderVariantCollectorSetting : ScriptableObject
{
    private const string DefaultSavePath = "Assets/MyShaderVariants.shadervariants";

    public static string GeFileSavePath(string packageName)
    {
        string key = $"{Application.productName}_{packageName}_GeFileSavePath";
        return EditorPrefs.GetString(key, DefaultSavePath);
    }
    public static void SetFileSavePath(string packageName, string savePath)
    {
        string key = $"{Application.productName}_{packageName}_GeFileSavePath";
        EditorPrefs.SetString(key, savePath);
    }

    public static int GeProcessCapacity()
    {
        string key = $"{Application.productName}_GeProcessCapacity";
        return EditorPrefs.GetInt(key, 1000);
    }
    public static void SetProcessCapacity(int capacity)
    {
        string key = $"{Application.productName}_GeProcessCapacity";
        EditorPrefs.SetInt(key, capacity);
    }
}