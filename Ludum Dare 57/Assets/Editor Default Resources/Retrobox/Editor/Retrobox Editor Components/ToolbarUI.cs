using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Retro;

namespace RetroEditor {

    public class ToolbarUI : Component {
        static Texture2D toolbarTexture;

        static UnityEngine.Color toolbarColour = new Color(.75f, .75f, .75f);
        public Rect toolbarRect;
        public float toolbarH = 48;

        //Toolbar icons
        Texture2D newLayer;
        Texture2D targetFile;


        //Texture2D properties;
        Texture2D previous;
        Texture2D play;
        Texture2D pause;
        Texture2D next;


        public ToolbarUI(RetroboxEditor editor) {
            e = editor;

            toolbarTexture = new Texture2D(1, 1);
            toolbarTexture.SetPixel(0, 0, toolbarColour);
            toolbarTexture.Apply();

            previous = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons5.png"));
            play = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons6.png"));
            pause = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons13.png"));
            next = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons7.png"));

            targetFile = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons22.png"));
            newLayer = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons1.png"));

        }

        //draw the toolbar, which sits between the sprite window and the timeline
        public void Draw() {

            float toolbarY = e.canvasPartition.y + e.canvasPartition.height;
            toolbarRect = new Rect(0, toolbarY, e.position.width, toolbarH);
            GUI.DrawTexture(toolbarRect, toolbarTexture);
            using (new GUILayout.AreaScope(toolbarRect)) {
                using (new GUILayout.HorizontalScope()) {

                    Rect toolbarControlsRect = new Rect(toolbarRect.x, 0, e.timelineUI.layersW, toolbarRect.height);

                    GUI.DrawTexture(toolbarControlsRect, toolbarTexture);
                    using (new GUILayout.AreaScope(toolbarControlsRect)) {
                        using (new GUILayout.HorizontalScope()) {

                            GUILayout.Space(3);


                            if (e.targetIsRetroSheet) {
                                DrawTargetFileButton();
                                DrawNewLayerButton();
                                GUILayout.Space(80);
                                DrawPlayerControls();
                                GUILayout.Space(3);
                            } else {
                                GUILayout.Space(e.timelineUI.layersW - toolbarH + 3);
                            }
                        }
                    }
                    e.timelineUI.DrawTimelineThumbnails(e.frameTextures);
                }
            }
        }

        void DrawTargetFileButton() {
            if (GUILayout.Button(new GUIContent(targetFile, "target file"), GUI.skin.GetStyle("HBIcon"))) {
                EditorGUIUtility.PingObject(sheet);
            }
        }

        void DrawNewLayerButton() {
            if (GUILayout.Button(new GUIContent(newLayer, "new layer"), GUI.skin.GetStyle("HBIcon"))) {
                GenericMenu menu = new GenericMenu();
                foreach (Shape s in Shape.GetValues(typeof(Shape))) {
                    menu.AddItem(new GUIContent(s.ToString()), false, e.AddNewLayer, s);
                }
                menu.ShowAsContext();
            }
        }

        void DrawPlayerControls() {

            if (GUILayout.Button(new GUIContent(previous, "previous frame"), GUI.skin.GetStyle("HBIcon"))) {  //previous
                if (e.selectedFrameIndex == 0) {
                    e.selectedFrameIndex = sheet.spriteList.Count - 1;
                } else {
                    e.selectedFrameIndex = (e.selectedFrameIndex - 1) % (sheet.spriteList.Count);
                }
            }

            if (GUILayout.Button((e.playing) ? new GUIContent(pause, "pause") : new GUIContent(play, "play"), GUI.skin.GetStyle("HBIcon"))) {   //play
                e.playing = !e.playing;
                if (e.playing) {
                    e.frameOnPlay = e.selectedFrameIndex;
                    e.timeAtPlay = EditorApplication.timeSinceStartup;
                }
            }

            if (GUILayout.Button(new GUIContent(next, "next frame"), GUI.skin.GetStyle("HBIcon"))) {   //next
                e.selectedFrameIndex = (e.selectedFrameIndex + 1) % (sheet.spriteList.Count);
            }
        }


    }
}
