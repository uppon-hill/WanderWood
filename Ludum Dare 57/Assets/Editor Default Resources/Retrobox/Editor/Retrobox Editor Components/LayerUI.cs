using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Retro;

namespace RetroEditor {
    public class LayerUI : Component {
        Texture2D menuTexture;
        //Layer icons
        Texture2D expand;
        Texture2D collapse;
        Texture2D eyeOpen;
        Texture2D eyeClosed;
        Texture2D trigger;
        Texture2D collider;
        Texture2D noCollision;

        Texture2D isTrigger;
        public LayerUI(RetroboxEditor editor) {
            e = editor;
            menuTexture = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons20.png"));
            eyeOpen = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons3.png"));
            eyeClosed = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons4.png"));

            trigger = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons15.png"));
            collider = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons16.png"));
            noCollision = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons21.png"));

            expand = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons9.png"));
            collapse = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons8.png"));
        }

        public void DrawLayer(Layer l, float workingtimelineHeight) {

            e.followingKeyframe = false;
            using (new GUILayout.HorizontalScope(GUILayout.Height(20))) {
                GUILayout.Space(2); //left margin per group
                for (int k = 0; k < sheet.spriteList.Count; k++) { //for every hitbox in every layer in every group...
                    e.frameUI.DrawTimelineFrame(l, k, workingtimelineHeight);
                }
            }
            GUILayout.Space(7);
        }

        public void DrawLayerControl(Layer layer, float containerWidth, Rect rect) {
            using (new GUILayout.HorizontalScope(GUI.skin.GetStyle("HBLayer"))) {
                DrawLayerMenu(layer, rect);
                DrawLayerLabel(layer, 150);
                DrawVisibilityToggle(layer);
                DrawColliderToggle(layer);
            }


            if (e.RightClicked(rect)) {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Delete layer"), false, e.DeleteLayer, e.parsingLayer);
                menu.ShowAsContext();
            }
        }

        void DrawLayerMenu(Layer layer, Rect rect) {

            try {
                //draw the colour strip
                GUI.DrawTexture(new Rect(0, rect.y, 30, 30), RetroboxEditor.colourTextures[layer.myBoxType]);
            } catch (System.Exception e) {
                Debug.LogError("The active Retrobox Preferences file does not contain an entry for '" + layer.myBoxType + "'. " + e);
            }

            if (GUILayout.Button(new GUIContent(menuTexture, "Layer options"), GUI.skin.GetStyle("HBIcon"), GUILayout.Width(30))) {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Delete layer"), false, e.DeleteLayer, layer);
                menu.ShowAsContext();
            }
        }

        void DrawLayerLabel(Layer layer, float width) {
            if (GUILayout.Button(new GUIContent(layer.myBoxType, "set layer type"), GUI.skin.GetStyle("Label"), GUILayout.MinWidth(width))) {

                if (e.preferences.boxDictionary.ContainsKey(layer.myBoxType)) {
                    GenerateLayerContextMenu(e.preferences.boxDictionary, layer);
                } else if (e.preferences.pointDictionary.ContainsKey(layer.myBoxType)) {
                    GenerateLayerContextMenu(e.preferences.pointDictionary, layer);
                }

            }
        }

        void DrawVisibilityToggle(Layer layer) {
            if (GUILayout.Button((layer.visible) ? new GUIContent(eyeOpen, "hide layer") : new GUIContent(eyeClosed, "show layer"), GUI.skin.GetStyle("HBIcon"))) { //draw visibility
                Undo.RecordObject(sheet, "set visibility to " + !layer.visible);
                layer.visible = !(layer.visible);
            }
        }


        void DrawColliderToggle(Layer layer) {
            GUIContent content = new GUIContent();
            switch (layer.collisionType) {
                case Layer.CollisionType.Collider:
                    content = new GUIContent(collider, "collider");
                    break;
                case Layer.CollisionType.Trigger:
                    content = new GUIContent(trigger, "trigger");
                    break;
                case Layer.CollisionType.NoCollide:
                    content = new GUIContent(noCollision, "no collider");
                    break;
            }

            if (GUILayout.Button(content, GUI.skin.GetStyle("HBIcon"))) { //draw isTrigger
                switch (layer.collisionType) {
                    case Layer.CollisionType.Collider:
                        Undo.RecordObject(sheet, "set trigger to" + layer.collisionType);
                        layer.collisionType = Layer.CollisionType.Trigger;
                        break;
                    case Layer.CollisionType.Trigger:
                        Undo.RecordObject(sheet, "set trigger to" + layer.collisionType);
                        layer.collisionType = Layer.CollisionType.NoCollide;
                        break;
                    case Layer.CollisionType.NoCollide:
                        Undo.RecordObject(sheet, "set trigger to" + layer.collisionType);
                        layer.collisionType = Layer.CollisionType.Collider;
                        break;
                }
            }
        }

        void GenerateLayerContextMenu(BoxDataDictionary dictionary, Layer layer) {
            GenericMenu menu = new GenericMenu();

            for (int j = 0; j < dictionary.Count; j++) {
                List<object> parameters = new List<object>();
                parameters.Add(layer);
                parameters.Add(dictionary.GetKey(j));
                menu.AddItem(new GUIContent(dictionary.GetKey(j)), false, SetLayerType, parameters);

            }
            menu.ShowAsContext();

        }

        //Set the group type to a given enum value (o = list of parameters)
        void SetLayerType(object o) {
            Undo.RecordObject(sheet, "change layer type");
            List<object> p = (List<object>)o;
            Layer l = (Layer)p[0];
            string s = (string)p[1];
            sheet.layers[sheet.layers.IndexOf(l)].myBoxType = s;

        }


    }
}
