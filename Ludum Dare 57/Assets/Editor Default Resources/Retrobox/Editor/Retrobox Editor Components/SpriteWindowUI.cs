using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Retro;
using System.Linq;

namespace RetroEditor {
    public class SpriteWindowUI : Component {

        //"static variables"
        public static UnityEngine.Color spritewindowColour = new Color(0.4f, 0.4f, 0.4f);
        public static Texture2D spriteWindowTexture;
        public static Rect window;

        //editor variables
        public Vector2 viewOffset = Vector2.zero;

        public float spriteScale = 1; //the sprite scale multiplier
        public Vector2 screenSpriteOrigin = new Vector2(0, 0); //to position the sprite in the centre of the window
        public Texture2D spritePreview;

        public Rect spriteRect => new Rect(
             screenSpriteOrigin.x + viewOffset.x,
             screenSpriteOrigin.y + viewOffset.y,
             spritePreview.width * spriteScale,
             spritePreview.height * spriteScale
           );

        static UnityEngine.Color spriteGridColour = new Color(0.6f, 0.6f, 0.6f);
        static Texture2D spriteGridTexture;
        static Rect spriteGridRect;
        Rect position => e.position;



        //Sprite window icons
        Texture2D zoom1;
        Texture2D zoom2;
        Texture2D zoom4;
        Texture2D zoomFill;
        Texture2D activeZoomTexture;
        public int zoomSetting = 1;

        public SpriteWindowUI(RetroboxEditor editor) {
            e = editor;

            spriteWindowTexture = new Texture2D(1, 1);
            spriteWindowTexture.SetPixel(0, 0, spritewindowColour);
            spriteWindowTexture.Apply();

            spriteGridTexture = new Texture2D(1, 1);
            spriteGridTexture.SetPixel(0, 0, spriteGridColour);
            spriteGridTexture.Apply();

            zoomFill = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_sprite_window_tools1.png"));
            zoom1 = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_sprite_window_tools2.png"));
            zoom2 = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_sprite_window_tools3.png"));
            zoom4 = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_sprite_window_tools4.png"));

            zoomSetting = 0;
            UpdateActiveZoomTexture();

        }

        public void SetupCanvas() {
            window.x = 0;
            window.y = 0;
            window.width = position.width;
            window.height = position.height - e.timelineUI.windowH;
        }


        public void DrawCanvas() {
            GUILayout.FlexibleSpace();
            using (new GUILayout.HorizontalScope()) {
                GUILayout.FlexibleSpace();

                GUI.DrawTexture(window, spriteWindowTexture); //background

                DrawSpriteWindowBackground(spriteRect);
                DrawSprite(spriteRect);
                e.DrawFrameShapes();

                Event ev = Event.current;
                DeselectOnWindowClick(ev);
                DrawZoomToggle(ev);

                if (e.mode == RetroboxEditor.EditorMode.Move) {
                    EditorGUIUtility.AddCursorRect(window, MouseCursor.Pan);
                }

                GUILayout.FlexibleSpace();
            }
            GUILayout.FlexibleSpace();
        }


        void DrawSpriteWindowBackground(Rect spriteRect) {
            GUI.DrawTexture(spriteRect, RetroboxEditor.selectedTexture); //sprite background
            if (e.gridSetting) DrawGrid(spriteRect);
        }

        void DrawSprite(Rect spriteRect) {
            using (new GUILayout.HorizontalScope()) {
                GUI.DrawTexture(new Rect(spriteRect.x, spriteRect.y, spriteRect.width, spriteRect.height), spritePreview, ScaleMode.ScaleToFit); //our sprite
            }
        }


        void DeselectOnWindowClick(Event e) {
            if (e.type == EventType.MouseDown && window.Contains(e.mousePosition)) {
                GUI.FocusControl(null);
            }

        }

        void DrawGrid(Rect r) { //what a doozy...

            Rect gr = new Rect(r.x, r.y, 16 * spriteScale, 16 * spriteScale); //define a single 16x16 tile

            for (int i = 0; i < spritePreview.width / 16; i++) { //iterate over X

                for (int j = 0; j < spritePreview.height / 16; j += 2) { //iterate over 
                    GUI.DrawTexture(gr, spriteGridTexture); //draw box
                    gr.y += (gr.height * 2); //increment y axis
                }

                gr.height = r.y + r.height - gr.y; //prepare for last box "remainder" height
                if (r.y + r.height - gr.y > 0) GUI.DrawTexture(gr, spriteGridTexture); //draw last box in column
                gr.height = 16 * spriteScale; //reset for new column
                gr.y = (i % 2 != 0) ? r.y : r.y + gr.height; //reset the entire y axis
                gr.x += (gr.width); //increment x axis

            }

            //Additional column if necessary
            if (r.x + r.width - gr.x > 0) {
                gr.width = r.x + r.width - gr.x; //prepare for last boxes "remainder" width

                for (int j = 0; j < spritePreview.height / 16; j += 2) { //iterate over Y
                    GUI.DrawTexture(gr, spriteGridTexture); //draw box
                    gr.y += (gr.height * 2); //increment y axis
                }

                gr.height = r.y + r.height - gr.y; //prepare for last box "remainder" height (AND width)
                if (r.y + r.height - gr.y > 0) GUI.DrawTexture(gr, spriteGridTexture); //draw last box in column
                                                                                       //gr.height = 16 * spriteScale; //reset for new column
            }

        }

        public void UpdateViewOffsetWithScale(Vector2 mPos, float scale, float prevScale) { //adjust our canvas offset to be anchored by the mouse position as we scale.

            //find the screen position of the pixel under the mouse
            Vector2 pixelFromSpriteOrigin = (Event.current.mousePosition - screenSpriteOrigin - viewOffset) / spriteScale;
            Vector2 spriteOrigin = screenSpriteOrigin + viewOffset;
            Vector2 pixelScreenPos = spriteOrigin + (pixelFromSpriteOrigin * prevScale);

            //scale the sprite
            viewOffset *= scale / prevScale;

            //find the new screen position of the pixel
            Vector2 newScreenSpriteOrigin = new Vector2(
                (window.width * 0.5f) - (spritePreview.width * 0.5f * scale),
                (window.height * 0.5f) - (spritePreview.height * 0.5f * scale)
            );

            Vector2 newSpriteOrigin = newScreenSpriteOrigin + viewOffset;
            Vector2 newPixelScreenPos = newSpriteOrigin + (pixelFromSpriteOrigin * scale);

            //translate the sprite by the difference of those two positions.
            viewOffset += pixelScreenPos - newPixelScreenPos;
        }

        public void UpdateScale() { //set the zoom scale of the canvas
            spriteScale = GetScaleFromZoom(zoomSetting);
            screenSpriteOrigin.x = (window.width * 0.5f) - (spritePreview.width * 0.5f * spriteScale);
            screenSpriteOrigin.y = (window.height * 0.5f) - (spritePreview.height * 0.5f * spriteScale);

            //handle the canvas view bounds X
            if (viewOffset.x > spritePreview.width * spriteScale * 0.5f) viewOffset.x = spritePreview.width * spriteScale * 0.5f;
            if (viewOffset.x < -spritePreview.width * spriteScale * 0.5f) viewOffset.x = -spritePreview.width * spriteScale * 0.5f;

            //handle the canvas view bounds Y
            if (viewOffset.y > spritePreview.height * spriteScale * 0.5f) viewOffset.y = spritePreview.height * spriteScale * 0.5f;
            if (viewOffset.y < -spritePreview.height * spriteScale * 0.5f) viewOffset.y = -spritePreview.height * spriteScale * 0.5f;

        }

        public void DrawZoomToggle(Event e) {
            using (new GUILayout.AreaScope(new Rect(8, SpriteWindowUI.window.height - (32 + 8), (32 * 2) + (8 * 2), 32))) {
                using (new GUILayout.HorizontalScope()) {
                    if (GUILayout.Button(new GUIContent(activeZoomTexture, "cycle zoom setting"), GUIStyle.none)) {
                        if (e.button == 0) {
                            zoomSetting++;
                            if (zoomSetting > 3) zoomSetting = 0;
                            switch (zoomSetting) {
                                case 0:
                                    activeZoomTexture = zoomFill;
                                    viewOffset.x = 0;
                                    viewOffset.y = 0;
                                    break;
                                case 1:
                                    activeZoomTexture = zoom1;
                                    break;
                                case 2:
                                    activeZoomTexture = zoom2;
                                    break;
                                case 3:
                                    activeZoomTexture = zoom4;
                                    break;
                            }
                        }
                    }

                }
            }
        }

        public void UpdateActiveZoomTexture() {
            switch (zoomSetting) {
                case 0:
                    activeZoomTexture = zoomFill;
                    return;
                case 1:
                    activeZoomTexture = zoom1;
                    return;

                case 2:
                    activeZoomTexture = zoom2;
                    return;

                case 3:
                    activeZoomTexture = zoom4;
                    return;

            }
            activeZoomTexture = zoomFill;
        }

        public void HandleScrollToZoom(Event ev) {
            int prevZoomSetting = zoomSetting;

            //scroll to cycle through zoom layers
            if (ev.type == EventType.ScrollWheel) {
                if (zoomSetting == 0) {
                    zoomSetting = Mathf.Clamp((int)(((SpriteWindowUI.window.height - 20) / (sheet.spriteList[0].rect.height)) * 0.5f), 0, 3);
                    e.shouldRepaint = true;
                }

                if (ev.delta.y > 0) { //scrolling down = zooming out
                    if (zoomSetting > 1) {
                        zoomSetting--;
                        e.shouldRepaint = true;
                    }
                } else { //scrolling up = zooming in
                    if (zoomSetting < 3) {
                        zoomSetting++;
                        e.shouldRepaint = true;
                    }
                }
            }

            if (prevZoomSetting != zoomSetting && zoomSetting != 0) {
                UpdateViewOffsetWithScale(
                    ev.mousePosition,
                    GetScaleFromZoom(zoomSetting),
                    GetScaleFromZoom(prevZoomSetting)
                );
            }



        }
        public float GetScaleFromZoom(int zoom) {
            switch (zoom) {
                case 0:
                    return (SpriteWindowUI.window.height - 20) / sheet.spriteList[0].rect.height;
                case 1:
                    return 2;
                case 2:
                    return 4;
                case 3:
                    return 8;
            }
            return 2;
        }
    }
}
