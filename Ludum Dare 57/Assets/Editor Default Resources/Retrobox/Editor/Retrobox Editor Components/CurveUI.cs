using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Retro;

namespace RetroEditor {

    public class CurveUI : Component {

        public CurveUI(RetroboxEditor editor) {
            e = editor;
        }

        public Curve targetCurveProperty;
        public int editingCurvePoint = -1;
        public Rect curvePropRect = new Rect();

        int layerEditingCurvePoint = -1;

        public void DrawCurveEditor(Rect rect) {
            curvePropRect = new Rect(rect.min, rect.size);
            Rect dims = new Rect(rect.min, rect.size);
            GUI.DrawTexture(dims, PropertiesUI.propWindowTexture);
            dims.position += Vector2.one * e.margin_;
            dims.size -= Vector2.one * e.margin_ * 2;

            dims.y += dims.height;
            dims.height = -dims.height;
            CaptureCurveInputs(dims, true, false);

            Color prevColour = Handles.color;
            Handles.color *= 2;
            targetCurveProperty.DrawCurve(dims, true, false);
            Handles.color = prevColour;
        }

        public void CaptureCurveInputs(Rect bounds, bool handles = true, bool anchors = true) {
            Vector2 o = new Vector2(bounds.x, bounds.y);

            if (targetCurveProperty == null) {
                targetCurveProperty = Curve.Linear;
            } else {

                if (Event.current.type == EventType.MouseDown) {
                    if (!e.playing) {
                        //check each of the points to see if the mouse is within them.
                        int curveHandleIndex = targetCurveProperty.GetEditingHandle(Event.current.mousePosition, bounds, handles, anchors);
                        if (curveHandleIndex != -1) {
                            editingCurvePoint = curveHandleIndex;
                        }
                    }
                } else if (Event.current.type == EventType.MouseUp) {
                    editingCurvePoint = -1;
                }

                if (editingCurvePoint != -1) {
                    //if we find one, set its position to the relative mouse position
                    targetCurveProperty.EditCurve(editingCurvePoint, Event.current.mousePosition, bounds);
                }
            }
        }

        public void ResetTargetCurvePropertySelection() {
            targetCurveProperty = null;
        }

        public void DrawPoint(Layer layer, bool visible = true) {
            e.SetHandleColour(layer);
            Vector2 size = Vector2.one * e.spriteUI.spriteScale;

            FrameData d = layer.GetFrameData(e.selectedFrameIndex);
            Rect box = d.rect;
            box = e.FrameToCanvasSpace(box);

            Handles.DrawSolidRectangleWithOutline(new Rect(box.position - (size * 0.5f), size * 0.5f), Handles.color, Color.clear); //top
            Handles.DrawWireCube((Vector3)box.position, Vector3.one * 16);


            Frame previousKeyFrame = layer.GetPreviousKeyFrame(e.selectedFrameIndex);
            Frame startFrame = layer.GetCurrentKeyFrameOrPrevious(e.selectedFrameIndex);
            Frame nextKeyFrame = layer.GetNextKeyFrame(e.selectedFrameIndex);

            //draw a curve starting from this keyframe
            if (startFrame != null && nextKeyFrame != null) {
                DrawLayerCurve(layer, startFrame, nextKeyFrame, e.spriteUI.spriteRect, e.LayerIsSelected(layer), true);
            }

            //also draw the previous curve that this keyframe is the end of, if possible?

            if (startFrame != null && previousKeyFrame != null && startFrame != previousKeyFrame) {
                DrawLayerCurve(layer, previousKeyFrame, startFrame, e.spriteUI.spriteRect, false, false);

            }


        }

        public void DrawLayerCurve(Layer layer, Frame startFrame, Frame endFrame, Rect canvasBounds, bool drawHandles = true, bool drawAnchors = true) {
            int startIndex = layer.frames.IndexOf(startFrame);
            int endIndex = layer.frames.IndexOf(endFrame);
            FrameData startData = layer.frameDataById[startFrame.dataId];
            FrameData endData = layer.frameDataById[endFrame.dataId];

            Rect spriteSpaceBounds = new Rect(Vector2.zero, sheet.spriteList[e.selectedFrameIndex].rect.size);

            Curve c = new Curve(startData.position, startData.forwardHandle, endData.backHandle, endData.position);

            //check each of the points to see if the mouse is within them.
            int curveHandleIndex = c.GetEditingHandle(Event.current.mousePosition, spriteSpaceBounds, canvasBounds, drawHandles, drawAnchors);

            if (e.Clicked() && curveHandleIndex != -1) {
                e.clickedFrames.Add(layer);
            }

            //Edit curve
            if (Event.current.type == EventType.MouseDown) {
                if (!e.playing) {
                    if (curveHandleIndex != -1) {
                        layerEditingCurvePoint = curveHandleIndex;
                    }
                }
            } else if (Event.current.type == EventType.MouseUp) {
                layerEditingCurvePoint = -1;
            }

            if (drawHandles) {

                if (layerEditingCurvePoint != -1) {

                    //if we find one, set its position to the relative mouse position
                    c.EditCurve(layerEditingCurvePoint, Event.current.mousePosition, spriteSpaceBounds, canvasBounds, false);
                    //resizingFrame = parsingFrame;//we are now editing this hitbox only
                    //resizingLayer = parsingLayer;
                    startData.position = c.a;
                    startData.forwardHandle = c.a1;
                    endData.backHandle = c.b1;
                    endData.position = c.b;
                }

            }
            //start.curve.DebugMousePos(Event.current.mousePosition, spriteSpaceBounds, canvasBounds);
            c.DrawCurve(spriteSpaceBounds, canvasBounds, drawHandles, drawAnchors);

            //Draw target property Curve along this curve, if one exists.
            if (targetCurveProperty != null && targetCurveProperty.points.Length > 0) {
                DrawPropertyCurveAlongLayerCurve(targetCurveProperty, layer, startIndex, endIndex, spriteSpaceBounds, canvasBounds);
            }
        }

        public void DrawPropertyCurveAlongLayerCurve(Curve curveProp, Layer layer, int startIndex, int endIndex, Rect spriteSpaceBounds, Rect canvasBounds) {
            //if we're looking at a curve
            int duration = endIndex - startIndex;

            //loop through all of the frames from the start to the end of the curve.
            for (int i = 0; i < duration; i++) {
                //one frame ahead of this, just because frame 0 is kind of useless.
                float curveTime = curveProp.EvaluateForX((float)(i + 1) / (float)duration).y;

                Curve frameCurve = layer.CurveFromKeyFrames(startIndex);
                //draw the points along that curve, one for each frame
                Vector2 markerPos = frameCurve.EvaluateDrawnCurve(spriteSpaceBounds, canvasBounds, curveTime);
                Color prevColour = Handles.color;
                bool white = (startIndex + i + 1) == e.selectedFrameIndex || (startIndex + i) == e.selectedFrameIndex;
                Handles.color = white ? Color.white : prevColour * 3;
                //if the current frame is selected, make that one a bit brighter
                Vector2 size = Vector2.one * 8;
                Rect r = new Rect(markerPos - (size * 0.5f), size * 0.5f);
                Handles.DrawSolidRectangleWithOutline(r, Handles.color, Color.clear);

                Handles.color = prevColour;
            }
        }

    }

}