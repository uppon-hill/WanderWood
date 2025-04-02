using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Retro;
namespace RetroEditor {

    public static class Updater {

        public static void TryUpdate(object data) {
            (RetroboxEditor editor, string version) d = ((RetroboxEditor, string))data;

            RetroboxEditor editor = d.editor;
            string version = d.version;

            int updated = 0;
            string[] sheetReferences = AssetDatabase.FindAssets("t:Sheet");
            Sheet[] sheets = new Sheet[sheetReferences.Length];
            for (int i = 0; i < sheetReferences.Length; i++) {

                sheets[i] = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(sheetReferences[i]), typeof(Sheet)) as Sheet;
                //1.0a no longer supported

                if (sheets[i].GetVersion().Equals("1.0")) {//find old version...(1.0a)

                    sheets[i].layers = new List<Layer>();
                    foreach (Group g in sheets[i].groups) {
                        foreach (Layer l in g.layers) {
                            Layer nl = l;
                            l.myBoxType = g.myBoxType;
                            l.collisionType = (Layer.CollisionType)(g.collisionType);
                            l.kind = g.layerKind;
                            l.visible = g.visible;
                            sheets[i].layers.Add(l);

                        }
                    }
                    //                sheets[i].groups = null;


                    updated++;//we did something

                    EditorUtility.SetDirty(sheets[i]);
                    sheets[i].SetVersion(editor, version); //do something
                }

            }
            editor.Save();
            Debug.Log(sheetReferences.Length + " total sheets found, " + updated + " old sheets updated to latest version (" + version + ")");

        }

    }

}