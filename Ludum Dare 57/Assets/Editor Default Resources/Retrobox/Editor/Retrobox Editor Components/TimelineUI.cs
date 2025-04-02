using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Retro;

namespace RetroEditor {

    public class TimelineUI : Component {
        static UnityEngine.Color timelineColour = new Color(0.5f, 0.5f, 0.5f);
        static Texture2D timelineTexture;
        static Rect timelineRect;
        Rect selectedFrameRect;

        Texture2D border;

        //Selection
        Texture2D frameBorderFirst;
        Texture2D frameBorderMiddle;
        Texture2D frameBorderLast;


        static UnityEngine.Color groupColour = new Color(0.7f, 0.7f, 0.7f);
        static Texture2D groupTexture;
        static Rect groupsRect;
        public float layersW = 260;

        public float windowH = 256;
        public Vector2 scrollPos = Vector2.zero;

        Rect partition => e.canvasPartition;

        float toolbarH => e.toolbarUI.toolbarH;
        Rect toolbarRect => e.toolbarUI.toolbarRect;

        public TimelineUI(RetroboxEditor editor) {
            e = editor;


            groupTexture = new Texture2D(1, 1);
            groupTexture.SetPixel(0, 0, groupColour);
            groupTexture.Apply();

            timelineTexture = new Texture2D(1, 1);
            timelineTexture.SetPixel(0, 0, timelineColour);
            timelineTexture.Apply();



            frameBorderFirst = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_selected_frame_border1.png"));
            frameBorderMiddle = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_selected_frame_border2.png"));
            frameBorderLast = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_selected_frame_border3.png"));

        }


        //draw the background for the timeline
        public void DrawWindowBG() {
            Rect pos = e.position;

            float x = 0;
            float y = SpriteWindowUI.window.height;
            float w = pos.width;
            float h = pos.height - SpriteWindowUI.window.height;


            using (new GUILayout.AreaScope(new Rect(x, y, w, h))) {

                groupsRect = new Rect(0, toolbarH + partition.height, layersW, pos.height - y - toolbarH - partition.height);
                GUI.DrawTexture(groupsRect, groupTexture);

                timelineRect = new Rect(layersW, toolbarH + partition.height, pos.width - layersW, pos.height - y - toolbarH - partition.height);
                GUI.DrawTexture(timelineRect, timelineTexture);
            }
        }

        //Draw the Timeline
        public void Draw() {
            scrollPos = GUILayout.BeginScrollView(scrollPos, false, false, GUILayout.Height(windowH - toolbarH - partition.height));

            using (new GUILayout.VerticalScope(GUILayout.Width(e.position.width - layersW - 16))) {

                float workingtimelineHeight = 11; //offset between diamonds and row bars
                GUILayout.Space(e.padding_);

                DrawFrameFocus(e.selectedFrameIndex);

                using (new GUILayout.VerticalScope()) {
                    for (int i = 0; i < sheet.layers.Count; i++) { //for every layer

                        e.layerUI.DrawLayer(sheet.layers[i], workingtimelineHeight);
                        GUILayout.Space(e.padding_);
                        workingtimelineHeight += e.padding_; //margin per expanded layer
                        workingtimelineHeight += 27;
                        //GUILayout.Space(32); //top margin per layer

                    }
                }
            }
            GUILayout.EndScrollView();
        }

        //Draw the layers on the left side of the screen
        public void DrawLayersPanel() {
            float oldScrollx = scrollPos.x;
            GUILayoutOption height = GUILayout.Height(windowH - toolbarH - partition.height);
            GUILayoutOption width = GUILayout.Width(layersW);
            scrollPos = GUILayout.BeginScrollView(new Vector2(0, scrollPos.y), false, false, GUIStyle.none, GUIStyle.none, height, width);
            scrollPos.x = oldScrollx;
            using (new GUILayout.VerticalScope(GUILayout.Width(layersW))) {

                Rect workingGroupRect = new Rect(
                    groupsRect.x,
                    groupsRect.y - toolbarH - partition.height + e.propertiesOffset,
                    layersW,
                    32
                );

                float layerWidth = layersW;//300; //this is a hard variable from the HBLayer style.

                for (int i = 0; i < sheet.layers.Count; i++) {
                    e.layerUI.DrawLayerControl(sheet.layers[i], layerWidth, workingGroupRect);
                    workingGroupRect.y += (workingGroupRect.height);
                }
            }
            GUILayout.EndScrollView();
        }


        void DrawFrameFocus(int selectedFrameIndex) {
            selectedFrameRect = new Rect(selectedFrameIndex * toolbarH, scrollPos.y, toolbarH + 1, timelineRect.height);
            GUI.DrawTexture(selectedFrameRect, e.frameUI.selectedFrameTexture);

        }


        public void DrawTimelineThumbnails(List<Texture2D> frameTextures) {
            //selected frame goes here
            selectedFrameRect = new Rect(-scrollPos.x + layersW + (e.selectedFrameIndex * toolbarH), 0, toolbarH, toolbarH);

            //frameThumbs
            using (new GUILayout.AreaScope(new Rect(layersW, 0, toolbarRect.width - layersW, toolbarRect.height))) {
                GUILayout.BeginScrollView(new Vector2(scrollPos.x, 0), false, false, GUIStyle.none, GUIStyle.none, GUILayout.Height(toolbarH), GUILayout.Width(toolbarRect.width - layersW));

                GUI.DrawTexture(new Rect(e.selectedFrameIndex * toolbarH, 0, toolbarH, toolbarH), RetroboxEditor.selectedTexture);// draw white background

                if (e.targetIsRetroSheet) {

                    using (new GUILayout.HorizontalScope()) {

                        float hm = toolbarH - 4;
                        for (int i = 0; i < sheet.spriteList.Count; i++) { //draw frames
                            border = (i == 0) ? frameBorderFirst : frameBorderMiddle; //the first border has a left edge, the others dont to preserve 1px edge.
                            float ratio = (float)(frameTextures[i].height) / (float)(frameTextures[i].width);
                            float x = i * (toolbarH);
                            float y = 2 + (hm - (hm * ratio)) * 0.5f;
                            float w = ratio < 1 ? hm : hm * ratio;
                            float h = ratio < 1 ? hm * ratio : hm;
                            GUI.DrawTexture(new Rect(x, y, w, h), frameTextures[i]); //sprite
                            GUI.Label(new Rect(x + 4, y - 12, w, h), i.ToString()); //sprite

                            if (GUILayout.Button(border, GUI.skin.GetStyle("HBIcon"), GUILayout.Width(toolbarH), GUILayout.Height(toolbarH))) { //button
                                if (Event.current.button == 0) {
                                    GUI.FocusControl(null);
                                    e.selectedFrameIndex = i;

                                } else if (Event.current.button == 1) {
                                    //right click
                                    GenericMenu menu = new GenericMenu();
                                    menu.AddItem(new GUIContent("Duplicate frame"), false, e.DuplicateFrame, i);
                                    menu.AddItem(new GUIContent("Delete frame"), false, e.DeleteFrame, i);
                                    menu.ShowAsContext();
                                }
                            }
                        }

                    }
                }

                GUI.DrawTexture(new Rect(0, 0, e.position.width, toolbarH), frameBorderLast);
                GUILayout.EndScrollView();
            }
        }
    }

}