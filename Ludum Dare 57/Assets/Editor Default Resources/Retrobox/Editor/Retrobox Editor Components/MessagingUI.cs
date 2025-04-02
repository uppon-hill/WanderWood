using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace RetroEditor {

    public class MessagingUI : Component {

        static Texture2D messageTexture;
        static UnityEngine.Color messageColour = new Color(0.7f, 0.7f, 0.7f);

        public MessagingUI(RetroboxEditor editor) {
            e = editor;

            messageTexture = new Texture2D(1, 1);
            messageTexture.SetPixel(0, 0, messageColour);
            messageTexture.Apply();
        }

        public void Message(params string[] strings) {
            using (new GUILayout.AreaScope(SpriteWindowUI.window)) {

                GUI.DrawTexture(SpriteWindowUI.window, SpriteWindowUI.spriteWindowTexture); //background
                GUI.DrawTexture(new Rect(SpriteWindowUI.window.width * 0.5f - 200f, SpriteWindowUI.window.height * 0.5f - 100f, 400, 200), messageTexture); //notice background

                GUILayout.FlexibleSpace();
                foreach (string s in strings) {
                    using (new GUILayout.HorizontalScope()) {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(s);
                        GUILayout.FlexibleSpace();
                    }
                }
                GUILayout.FlexibleSpace();

            }
        }

        void MessageWithButton(string buttonLabel, System.Action action, params string[] strings) {
            using (new GUILayout.AreaScope(SpriteWindowUI.window)) {

                GUI.DrawTexture(SpriteWindowUI.window, SpriteWindowUI.spriteWindowTexture); //background
                GUI.DrawTexture(new Rect(SpriteWindowUI.window.width * 0.5f - 200f, SpriteWindowUI.window.height * 0.5f - 100f, 400, 200), messageTexture); //notice background

                GUILayout.FlexibleSpace();
                using (new GUILayout.VerticalScope()) {
                    foreach (string s in strings) {
                        using (new GUILayout.HorizontalScope()) {
                            GUILayout.FlexibleSpace();
                            GUILayout.Label(s);
                            GUILayout.FlexibleSpace();
                        }
                    }
                    GUILayout.Space(e.margin_);

                    using (new GUILayout.HorizontalScope()) {

                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(buttonLabel)) {
                            action.Invoke();
                        }
                        GUILayout.FlexibleSpace();
                    }

                }
                GUILayout.FlexibleSpace();

            }
        }


        public void NoPreferences() {
            string msg = "Could not find preferences file in /Resources. \n \nMove an existing file to /Resources to continue \nor generate a new one below.";
            MessageWithButton(
                "New Preferences!",
                RetroEditor.Utilities.GeneratePreferences,
                msg
            );
        }

        public void NothingSelected() {

            ShowImporter();
            /*
            MessageWithButton(
                "New Sheet!",
                e.NewRetroSheet,
                "Nothing Selected...",
                "Create a new animation at this location?"
            );
            */

        }

        void ShowImporter() {
            using (new GUILayout.AreaScope(SpriteWindowUI.window)) {

                GUI.DrawTexture(SpriteWindowUI.window, SpriteWindowUI.spriteWindowTexture); //background
                GUI.DrawTexture(new Rect(SpriteWindowUI.window.width * 0.5f - 200f, SpriteWindowUI.window.height * 0.5f - 100f, 400, 200), messageTexture); //notice background

                GUILayout.FlexibleSpace();

                using (new GUILayout.VerticalScope()) {
                    using (new GUILayout.HorizontalScope()) {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("Import new sheet?");
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.Space(e.margin_);

                    using (new GUILayout.VerticalScope()) {
                        using (new GUILayout.HorizontalScope()) {

                            GUILayout.FlexibleSpace();
                            using (new GUILayout.VerticalScope()) {
                                //input fields for vector2 size
                                e.importSize = EditorGUILayout.Vector2IntField("Size", e.importSize, GUILayout.Width(150)); //input fields for vector2 size
                                e.importPivot = EditorGUILayout.Vector2Field("Pivot", e.importPivot, GUILayout.Width(150)); //input fields for vector2 pivot
                            }
                            GUILayout.FlexibleSpace();
                        }

                    }

                }
                GUILayout.FlexibleSpace();

            }

        }
    }

}