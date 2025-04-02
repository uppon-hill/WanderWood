using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Retro;
namespace RetroEditor {

    public class HitboxUI : Component {

        Rect box;
        Rect lHandle;
        Rect rHandle;
        Rect tHandle;
        Rect bHandle;
        Rect tlHandle;
        Rect trHandle;
        Rect blHandle;
        Rect brHandle;

        Rect wholeHandle;

        Frame resizingFrame;
        Layer resizingLayer;
        Rect prevBox;    //a mirror of the hitbox currently being edited before we started

        enum HandleType { Left, Right, Top, Bottom, TopLeft, TopRight, BottomLeft, BottomRight, Whole, None };
        HandleType editingHandle;
        public Stack<KeyValuePair<Rect, UnityEditor.MouseCursor>> mouseHandles;

        public HitboxUI(RetroboxEditor editor) {
            e = editor;
            resizingFrame = null;
            resizingLayer = null;
        }

        //draw a hitbox
        public void DrawHitbox(Layer layer, bool visible = true) {
            e.SetHandleColour(layer);
            SetupHitboxBounds(layer);

            //setup grabby handles
            int h = 6;
            int h2 = 3;
            SetupHandles(h);

            //is the mouse over a handle?
            if (Event.current.type == EventType.MouseDown && !e.playing) {
                if (e.LayerIsSelected(layer)) {
                    DetermineSelectedHandle(layer);
                }

            }

            //make the edit
            bool shouldResize =
                layer.frames.IndexOf(resizingFrame) == e.selectedFrameIndex &&
                Event.current.type == EventType.MouseDrag &&
                !e.mode.Equals(RetroboxEditor.EditorMode.Move) &&
                !e.resize;

            if (shouldResize) {
                ResizeHitbox(layer);
            }

            if (visible) {
                DrawHandles(layer, h, h2);
            }
            //did we click this box?
            if (e.ClickedRect(box)) {
                e.clickedFrames.Add(e.parsingLayer);
            }
        }

        public void SetupHitboxBounds(Layer layer) {
            //adjust size to match screen.
            box = layer.GetFrameData(e.selectedFrameIndex).rect;
            box = e.FrameToCanvasSpace(box);

            //rounding...
            box.x = (int)box.x;
            box.y = (int)box.y;
            box.width = (int)box.width;
            box.height = (int)box.height;
        }

        public void SetupHandles(int h) {
            int h2 = h * 2;
            tHandle = new Rect((box.x + box.width * .5f) - h, box.y - h, h2, h2); //top
            bHandle = new Rect((box.x + box.width * .5f) - h, box.y + box.height - h, h2, h2); //bottom
            lHandle = new Rect(box.x - h, (box.y + box.height * .5f) - h, h2, h2); //left
            rHandle = new Rect(box.x + box.width - h, (box.y + box.height * .5f) - h, h2, h2); //right

            tlHandle = new Rect(box.x - h, box.y - h, h2, h2); //topleft
            trHandle = new Rect((box.x + box.width) - h, box.y - h, h2, h2); //topright
            blHandle = new Rect(box.x - h, box.y + box.height - h, h2, h2); //bottom
            brHandle = new Rect((box.x + box.width) - h, box.y + box.height - h, h2, h2); //bottom

            wholeHandle = new Rect(box.x, box.y, box.width, box.height); //All
        }


        public void DetermineSelectedHandle(Layer layer) {
            editingHandle = GetEditingHandle(Event.current.mousePosition);

            if (editingHandle == HandleType.None && wholeHandle.Contains(Event.current.mousePosition)) {
                editingHandle = HandleType.Whole;
            }

            if (editingHandle != HandleType.None) {
                resizingFrame = layer.frames[e.selectedFrameIndex];//we are now editing this hitbox only
                resizingLayer = layer;
                FrameData fd = resizingLayer.GetFrameData(e.selectedFrameIndex);
                prevBox = new Rect(fd.rect.x, fd.rect.y, fd.rect.width, fd.rect.height);//where we started
                e.prevMouse = Event.current.mousePosition;
            }
        }

        public void DrawHandles(Layer layer, int h, int h2) {
            //draw hitbox lines
            Handles.DrawAAPolyLine(3, new Vector3(box.x, box.y), new Vector3(box.x + box.width, box.y));//top
            Handles.DrawAAPolyLine(3, new Vector3(box.x, box.y + box.height), new Vector3(box.x + box.width, box.y + box.height));//bottom
            Handles.DrawAAPolyLine(3, new Vector3(box.x + box.width, box.y), new Vector3(box.x + box.width, box.y + box.height));//right
            Handles.DrawAAPolyLine(3, new Vector3(box.x, box.y), new Vector3(box.x, box.y + box.height));//left

            if (e.LayerIsSelected(layer) && layer.frames[e.selectedFrameIndex].IsKeyFrame() && !e.playing) {
                //draw filled in box
                Handles.DrawSolidRectangleWithOutline(new Rect(box.x, box.y, box.width, box.height), new Color(Handles.color.r, Handles.color.g, Handles.color.b, 0.16f), Color.clear); //top

                //draw handles
                Handles.DrawSolidRectangleWithOutline(new Rect(box.x + box.width / 2 - h2, box.y - h2, h, h), Handles.color, Color.clear); //top
                Handles.DrawSolidRectangleWithOutline(new Rect(box.x + box.width / 2 - h2, box.y + box.height - h2, h, h), Handles.color, Color.clear); //bottom
                Handles.DrawSolidRectangleWithOutline(new Rect(box.x - h2, box.y + box.height / 2 - h2, h, h), Handles.color, Color.clear); //left
                Handles.DrawSolidRectangleWithOutline(new Rect(box.x + box.width - h2, box.y + box.height / 2 - h2, h, h), Handles.color, Color.clear); //right
                Handles.DrawSolidRectangleWithOutline(new Rect(box.x - h2, box.y - h2, h, h), Handles.color, Color.clear); //tl
                Handles.DrawSolidRectangleWithOutline(new Rect(box.x + box.width - h2, box.y - h2, h, h), Handles.color, Color.clear); //tr
                Handles.DrawSolidRectangleWithOutline(new Rect(box.x - h2, box.y + box.height - h2, h, h), Handles.color, Color.clear); //bl
                Handles.DrawSolidRectangleWithOutline(new Rect(box.x + box.width - h2, box.y + box.height - h2, h, h), Handles.color, Color.clear); //br

                //draw mouse icons
                mouseHandles.Push(new KeyValuePair<Rect, MouseCursor>(wholeHandle, MouseCursor.MoveArrow)); //all
                mouseHandles.Push(new KeyValuePair<Rect, MouseCursor>(lHandle, MouseCursor.ResizeHorizontal)); //left
                mouseHandles.Push(new KeyValuePair<Rect, MouseCursor>(tHandle, MouseCursor.ResizeVertical)); //top
                mouseHandles.Push(new KeyValuePair<Rect, MouseCursor>(rHandle, MouseCursor.ResizeHorizontal)); //right
                mouseHandles.Push(new KeyValuePair<Rect, MouseCursor>(bHandle, MouseCursor.ResizeVertical)); //bottom
                mouseHandles.Push(new KeyValuePair<Rect, MouseCursor>(tlHandle, MouseCursor.ResizeUpLeft));//left
                mouseHandles.Push(new KeyValuePair<Rect, MouseCursor>(trHandle, MouseCursor.ResizeUpRight)); //top
                mouseHandles.Push(new KeyValuePair<Rect, MouseCursor>(blHandle, MouseCursor.ResizeUpRight)); //right
                mouseHandles.Push(new KeyValuePair<Rect, MouseCursor>(brHandle, MouseCursor.ResizeUpLeft)); //bottom

            }
        }

        public void ResizeHitbox(Layer layer) {
            e.selectedLayer = layer;
            Vector2 offset = (Event.current.mousePosition - e.spriteUI.screenSpriteOrigin - e.spriteUI.viewOffset) / e.spriteUI.spriteScale;
            Vector2 newPos = new Vector2(Mathf.RoundToInt(offset.x), Mathf.RoundToInt(offset.y));

            FrameData c = layer.GetFrameData(e.selectedFrameIndex);
            Undo.RecordObject(sheet, "resize hitbox");
            if (editingHandle == HandleType.Left) {
                c.position = new Vector2(newPos.x, c.position.y);
                float x = (prevBox.width + (prevBox.x - c.rect.x));
                c.size = new Vector2(x, c.size.y);

            } else if (editingHandle == HandleType.Right) {

                float x = (newPos.x - prevBox.x);
                c.size = new Vector2(x, c.size.y);

            } else if (editingHandle == HandleType.Top) {
                float height = (prevBox.height + (prevBox.y - newPos.y));
                c.position = new Vector2(c.position.x, newPos.y);
                c.size = new Vector2(c.size.x, height);

            } else if (editingHandle == HandleType.Bottom) {
                c.size = new Vector2(c.size.x, newPos.y - prevBox.y);

            } else if (editingHandle == HandleType.TopLeft) {
                c.position = newPos;
                c.size = prevBox.size + (prevBox.position - c.position);

            } else if (editingHandle == HandleType.TopRight) {
                c.position = new Vector2(c.position.x, newPos.y);
                c.size = new Vector2(newPos.x - prevBox.x, prevBox.height + (prevBox.y - c.rect.y));

            } else if (editingHandle == HandleType.BottomLeft) {
                c.position = new Vector2(newPos.x, c.position.y);
                c.size = new Vector2(prevBox.width + (prevBox.x - c.rect.x), newPos.y - prevBox.y);

            } else if (editingHandle == HandleType.BottomRight) {
                c.size = newPos - prevBox.position;

            } else if (editingHandle == HandleType.Whole) {
                c.position = prevBox.position + (Event.current.mousePosition - e.prevMouse) / e.spriteUI.spriteScale;
                c.position = new Vector2((int)c.position.x, (int)c.position.y);
            }
        }

        HandleType GetEditingHandle(Vector2 mPos) {
            HandleType h = HandleType.None;
            if (lHandle.Contains(mPos)) {
                h = HandleType.Left;
            } else if (rHandle.Contains(mPos)) {
                h = HandleType.Right;
            } else if (tHandle.Contains(mPos)) {
                h = HandleType.Top;
            } else if (bHandle.Contains(mPos)) {
                h = HandleType.Bottom;
            } else if (tlHandle.Contains(mPos)) {
                h = HandleType.TopLeft;
            } else if (trHandle.Contains(mPos)) {
                h = HandleType.TopRight;
            } else if (blHandle.Contains(mPos)) {
                h = HandleType.BottomLeft;
            } else if (brHandle.Contains(mPos)) {
                h = HandleType.BottomRight;
            }
            return h;
        }

        public void ConfigureMouseCursor() {
            int c = mouseHandles.Count;
            for (int d = 0; d < c; d++) {
                KeyValuePair<Rect, MouseCursor> kv = mouseHandles.Pop();
                EditorGUIUtility.AddCursorRect(kv.Key, kv.Value);
            }

        }


        public void HandleBoxResizeEnd(Event ev) {

            if (ev.type == EventType.MouseUp) { //finish resizing a hitbox
                if (resizingLayer != null) {
                    FrameData r = resizingLayer.GetFrameData(e.selectedFrameIndex);
                    if (resizingFrame.IsKeyFrame() || resizingFrame.IsCopyFrame()) {

                        //just in case we happened to have resized one and leave it with negative dimensions, adjust them
                        if (r.rect.width < 0) {
                            r.size = new Vector2(Mathf.Abs(r.rect.width), r.size.y);
                            r.position = new Vector2(r.position.x - r.rect.width, r.position.y);
                        }
                        if (r.rect.height < 0) {
                            r.size = new Vector2(r.size.x, Mathf.Abs(r.rect.height));
                            r.position = new Vector2(r.position.x, r.position.y - r.rect.height);
                        }
                    }

                    e.shouldRepaint = true;
                    resizingFrame = null;
                    resizingLayer = null;
                }
            }
        }


    }

}