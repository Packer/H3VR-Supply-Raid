using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;


public class PackageDrawer : Editor
{

    public static string GetSelectedPathOrFallback()
    {
        string path = "Assets";
        foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
        {
            if (obj.name == "SupplyRaid")
            {
                path = AssetDatabase.GetAssetPath(obj);
                Debug.Log(path);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetFileName(path);
                    break;
                }
            }
        }
        return path;
    }

    //[MenuItem("AssetBundles/Set Asset Bundle From File Name",false, 0)]
    static bool SetAssetBundleNames(AssetImporter package)
    {
        string packageDir = AssetDatabase.GetAssetPath(package);
        string rootFolder = packageDir.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(package)), "");

        string[] fileDirs = Directory.GetFiles(packageDir, "*", SearchOption.AllDirectories);
        for (int i = 0; i < fileDirs.Length; i++)
        {
            if (fileDirs[i] == null || fileDirs[i].Contains(".meta") || fileDirs[i].Contains(".cs"))
            {
                AssignPackageNameToGameObject(AssetImporter.GetAtPath(fileDirs[i]), "None", "None");
                continue;
            }

            //Debug.Log(fileDirs[i]);
            AssignPackageNameToGameObject(AssetImporter.GetAtPath(fileDirs[i]), "supplyraid", "sr");
        }

        bool failed = false;
        var allBundleNames = AssetDatabase.GetAllAssetBundleNames();
        foreach (var bundle in allBundleNames)
        {
            var dependencies = AssetDatabase.GetAssetBundleDependencies(bundle, true);
            if (dependencies.Length == 0)
                continue;
            //var dependencyString = new System.Text.StringBuilder();
            foreach (var dependency in dependencies)
            {
                failed = true;
                Debug.LogError(bundle + " depends on " + dependency);
            }
        }

        if (failed)
        {
            Debug.LogError("PACKAGE BUILD FAILED - Dependencies detected above");
            return false;
        }

        return true;
    }

   static void AssignPackageNameToGameObject(Object go, string packageName, string suffix)
    {
        if (go == null)
            return;


        //var obj = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GetAssetPath(go));
        //AssetDatabase.SetLabels(obj, new [] {packageName});

        AssetImporter assetImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(go));
        assetImporter.assetBundleName = packageName;
        assetImporter.assetBundleVariant = suffix;
    }

    /*
    public static void BuildBundles()
    {
        Debug.Log("PACK BUILD STARTED: " + System.DateTime.Now.ToString("HH:mm:ss"));

        if (SetAssetBundleNames(script))
        {
            Debug.Log("PACKAGE BUILD SUCCESS" + System.DateTime.Now.ToString("HH:mm:ss"));

            BuildAllAssetBundles();
        }
        else
            Debug.LogError("PACKAGE BUILD FAILED" + System.DateTime.Now.ToString("HH:mm:ss"));
    }
    */
    

    [MenuItem("Supply Raid/Build Bundles", priority = 0)]
    static void BuildAllAssetBundles()
    {
        string directory = "Assets/SupplyRaid";
        if (directory == "")
            return;

        var selectedAsset = AssetImporter.GetAtPath(directory);

        if (SetAssetBundleNames(selectedAsset))
        {
            string assetBundleDirectory = "Assets";
            assetBundleDirectory = Directory.GetParent(assetBundleDirectory).FullName;
            assetBundleDirectory += "/AssetBundles";
            if (!Directory.Exists(assetBundleDirectory))
            {
                Directory.CreateDirectory(assetBundleDirectory);
            }
            BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
        }
    }
}
