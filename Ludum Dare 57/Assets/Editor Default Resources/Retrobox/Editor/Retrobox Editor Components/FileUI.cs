using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Retro;
using System.Security.Cryptography;


namespace RetroEditor {
    public class FileUI : Component {
        Texture2D toolbarTexture;
        Color backgroundColour = new Color(.75f, .75f, .75f, 1);
        Vector2 margin = new Vector2(5, 2);

        int textHeight = 12;
        float height = 30;
        float buttonWidth = 20;
        Texture2D file;
        GUIStyle style = new GUIStyle();

        public FileUI(RetroboxEditor editor) {
            e = editor;
            toolbarTexture = new Texture2D(1, 1);
            toolbarTexture.SetPixel(0, 0, backgroundColour);

            file = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons18.png"));
        }

        public void Draw() {
            //draw a small strip at the top of the window to show the current file
            Rect fileRect = new Rect(0, 0, e.position.width, height);
            GUI.DrawTexture(fileRect, toolbarTexture);

            Rect contentRect = new Rect(fileRect.x + margin.x, fileRect.y + margin.y, fileRect.width - margin.x * 2, fileRect.height - margin.y * 2);

            //font size 12
            style.fontSize = textHeight;
            style.normal.textColor = Color.black;

            using (new GUILayout.AreaScope(contentRect)) {
                using (new GUILayout.HorizontalScope()) {
                    DrawBurgerMenu();

                    if (e.myTarget != null) {
                        DrawLabel();
                    }
                }
            }
        }

        void DrawBurgerMenu() {
            //File
            GUI.SetNextControlName("burger");
            if (GUILayout.Button(new GUIContent(file, "Menu"), GUI.skin.GetStyle("HBIcon"), GUILayout.Width(buttonWidth))) {
                if (Event.current.button == 0) {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("New Empty Sheet"), false, e.NewRetroSheet);
                    menu.AddItem(new GUIContent("Target preferences file"), false, e.TargetPreferences);
                    //menu.AddItem(new GUIContent("Generate preferences"), false, GeneratePreferences);
                    menu.AddItem(new GUIContent("Update all sheets"), false, Updater.TryUpdate, (e, e.version));
                    menu.AddItem(new GUIContent("About Retrobox"), false, e.AboutRetrobox);
                    menu.ShowAsContext();
                }
            }

        }

        void DrawLabel() {
            //left align label and vertically center it
            GUILayout.BeginVertical(GUILayout.Height(height));
            GUILayout.FlexibleSpace();
            GUILayout.Label(e.myTarget.name, style);
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();

            //close button with the same font style, but smaller
            float buttonHeight = height * 0.8f;
            if (GUILayout.Button("X", GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight), GUILayout.MaxHeight(buttonHeight))) {
                e.myTarget = null;
                e.mySerializedObject = null;
                Selection.activeObject = null;
            }
        }


    }
}