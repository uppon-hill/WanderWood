using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Retro;
using System.Linq;

namespace RetroEditor {
    public class FrameUI : Component {
        public Texture2D frameIcon;
        //Timeline icons
        Texture2D keyFrame;
        Texture2D emptyFrame;
        Texture2D copyFrame;

        public Texture2D selectedFrameTexture;
        Texture2D selectedFrameIconBorder;

        public FrameUI(RetroboxEditor editor) {
            e = editor;
            emptyFrame = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons11.png"));
            keyFrame = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons12.png"));
            copyFrame = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons14.png"));

            selectedFrameTexture = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_selected_frame_texture.png"));
            selectedFrameIconBorder = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_selected_frame_icon_border.png"));
        }

        public void DrawTimelineFrame(Layer layer, int index, float workingtimelineHeight) {

            float toolbarH = e.toolbarUI.toolbarH;
            Frame f = layer.frames[index];
            if (index < sheet.spriteList.Count - 1) { //if we're not the 2nd last frame, draw a line onwards from us.
                if (f.kind == Frame.Kind.KeyFrame || (f.kind == Frame.Kind.CopyFrame && e.followingKeyframe)) {
                    try {
                        //set the colour we'll be using
                        GUI.DrawTexture(new Rect((toolbarH * 0.5f) + (index * toolbarH), workingtimelineHeight, toolbarH, 9), RetroboxEditor.colourTextures[layer.myBoxType]);
                    } catch (System.Exception e) {
                        Debug.LogError("The active Retrobox Preferences file does not contain an entry for '" + layer.myBoxType + "'. " + e);
                    }
                }
            }

            if (f.kind == Frame.Kind.KeyFrame) {//draw the correct keyframe icon
                frameIcon = keyFrame;
                e.followingKeyframe = true;
            } else if (f.kind == Frame.Kind.Empty) {
                frameIcon = emptyFrame;
                e.followingKeyframe = false;
            } else if (f.kind == Frame.Kind.CopyFrame) {
                frameIcon = copyFrame;
            }

            if (layer == e.selectedLayer && index == e.selectedFrameIndex) {//BORDER if we are the selected frame icon
                DrawFrameSelection(layer, index, workingtimelineHeight);
            }

            if (GUILayout.Button(frameIcon, GUI.skin.GetStyle("HBIcon"), GUILayout.Width(toolbarH))) {//display the keyframe icon
                e.selectedLayer = layer;
                e.selectedFrameIndex = index;
            }
        }

        public void DrawFrameSelection(Layer layer, int index, float workingtimelineHeight) {
            Frame f = layer.frames[index];
            if (GUI.Button(new Rect(-2 + (index * 48), workingtimelineHeight - 12, 52, 32), selectedFrameIconBorder, GUI.skin.GetStyle("HBIcon"))) {//keyframe icon border
                if (f.kind == Frame.Kind.KeyFrame) {
                    SetEmptyFrame(layer, index);

                } else if (f.kind == Frame.Kind.Empty) {
                    SetCopyFrame(layer, index);

                } else if (f.kind == Frame.Kind.CopyFrame) {
                    SetKeyFrame(layer, index);
                }
            }
        }

        public void SetCopyFrame(Layer layer, int index) {

            Undo.RecordObject(sheet, "change frame to copy frame");
            Frame f = layer.frames[index];
            f.dataId = index > 0 ? layer.frames[index - 1].dataId : System.Guid.Empty.ToString();
            f.kind = Frame.Kind.CopyFrame;
            if (layer.GetPreviousKeyFrame(index) != null) {
                layer.ResyncFrames(layer.frames.IndexOf(layer.GetPreviousKeyFrame(index)));
            }

        }
        public void SetEmptyFrame(Layer layer, int i) {
            Undo.RecordObject(sheet, "change frame to empty frame");
            Frame f = layer.frames[i];
            //if we're a keyframe, remove our data from the FrameDataDictionary
            layer.RemoveKeyFrameData(f);
            //make us empty
            f.kind = Frame.Kind.Empty;
            f.dataId = System.Guid.Empty.ToString();

            //resync the sheet
            layer.ResyncFrames(i);
        }

        public void SetKeyFrame(Layer layer, int index) {

            Vector2 spriteSize = sheet.spriteList[e.selectedFrameIndex].rect.size;

            Undo.RecordObject(sheet, "change frame to key frame");
            //Assumes this was a copy frame
            Frame f = layer.frames[index];
            f.kind = Frame.Kind.KeyFrame;

            FrameData d = new FrameData();
            d = new FrameData();
            d.position = (spriteSize / 2f) + new Vector2(Random.Range(-4, 4), Random.Range(-4, 4));
            d.forwardHandle = d.position + (Vector2.down * 8f);
            d.backHandle = d.position + (Vector2.down * 8f);
            d.size = new Vector2(16, 16);

            if (f.dataId != null && layer.frameDataById.ContainsKey(f.dataId)) {

                //get the index of the start keyframe
                int aIndex = layer.frames.IndexOf(layer.frames.Where(x => x.dataId == f.dataId).First());
                FrameData aFrame = layer.frameDataById[f.dataId];

                //if this layer is a point layer, and there is another keyframe some time after this one, make a new keyframe that interpolates the two
                if (layer.kind == Retro.Shape.Point && index < layer.frames.Count - 1) {
                    if (layer.GetNextKeyFrame(index) is Frame nextFrame) {
                        FrameData bFrame = layer.frameDataById[nextFrame.dataId];
                        d = SubdivideCurve(layer, index, aFrame, bFrame);
                    }
                } else {
                    //otherwise just clone the existing frame?
                    d = FrameData.Clone(layer.frameDataById[f.dataId]);
                }
            }

            d.keyFrameId = f.guid;
            f.dataId = d.guid;
            layer.frameDataById.Add(d.guid, d);
            layer.ResyncFrames(index);

        }

        public FrameData SubdivideCurve(Layer layer, int index, FrameData aFrame, FrameData bFrame) {
            Frame f = layer.frames[index];
            FrameData d;
            int aIndex = layer.frames.IndexOf(layer.frames.Where(x => x.dataId == f.dataId).First());
            //get the index of the end keyframe
            int bIndex = layer.frames.IndexOf(layer.GetNextKeyFrame(index));

            float t = ((float)(index - aIndex)) / (bIndex - aIndex);

            //Subdivide the existing curve
            //this results in the in-frame handles changing
            //the middle handles being created
            //the out-frame handles also changing

            Vector2[] newA, newB;
            newA = new Vector2[4];
            newB = new Vector2[4];
            Curve.Subdivide(aFrame.position, aFrame.forwardHandle, bFrame.backHandle, bFrame.position, t, out newA, out newB);

            aFrame.forwardHandle = newA[1];
            bFrame.backHandle = newB[1];
            d = new FrameData();
            d.position = newA[3];
            d.forwardHandle = newB[2];
            d.backHandle = newA[2];
            return d;
        }


    }
}