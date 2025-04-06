using System;
using System.Linq;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

internal class EndNameEdit : EndNameEditAction {
    #region implemented abstract members of EndNameEditAction
    public override void Action(int instanceId, string pathName, string resourceFile) {
        AssetDatabase.CreateAsset(EditorUtility.InstanceIDToObject(instanceId), AssetDatabase.GenerateUniqueAssetPath(pathName));
    }

    #endregion
}

/// <summary>
/// Scriptable object window.
/// </summary>
public class ScriptableObjectWindow : EditorWindow {
    Vector2 scrollPos;
    private int selectedIndex;
    //private string[] names;

    private static Type[] types;

    private static string[] names;


    private static Type[] Types {
        get { return types; }
        set {
            types = value;
            names = types.Select(t => t.FullName).ToArray();
        }
    }

    public static void Init(Type[] scriptableObjects) {
        Types = scriptableObjects;

        var window = EditorWindow.GetWindow<ScriptableObjectWindow>(true, "Create a new ScriptableObject", true);
        window.ShowPopup();
    }

    /// <summary>
    /// Returns the assembly that contains the script code for this project (currently hard coded)
    /// </summary>
    private static Assembly GetAssembly() {
        return Assembly.Load(new AssemblyName("Assembly-CSharp"));
    }

    [MenuItem("Window/Insignia/Scriptable Object Window")]
    public static void Init() {
        var assembly = GetAssembly();

        // Get all classes derived from ScriptableObject
        var allScriptableObjects = (from t in assembly.GetTypes()
                                    where t.IsSubclassOf(typeof(ScriptableObject))
                                    select t).OrderBy(x => x.Name).ToArray();


        Types = allScriptableObjects;

        ScriptableObjectWindow window = (ScriptableObjectWindow)GetWindow(typeof(ScriptableObjectWindow));
        window.minSize = new Vector2(50, 100);
        window.Show();
        GUIContent titleContent = new GUIContent("Scriptables");
        window.titleContent = titleContent;
    }

    public void OnGUI() {
        if (names == null) {
            Init();
        }

        scrollPos = GUILayout.BeginScrollView(scrollPos);
        for (int i = 0; i < names.Length; i++) {
            if (GUILayout.Button(names[i])) {

                var asset = ScriptableObject.CreateInstance(types[i]);

                ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                    asset.GetInstanceID(),
                    ScriptableObject.CreateInstance<EndNameEdit>(),
                    string.Format("{0}.asset", names[i]),
                    AssetPreview.GetMiniThumbnail(asset),
                    null);

            }
        }
        GUILayout.EndScrollView();

    }

    public static ScriptableObject CreateScriptableAtPath(Type type, string path) {
        var asset = CreateInstance(type);

        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }
}