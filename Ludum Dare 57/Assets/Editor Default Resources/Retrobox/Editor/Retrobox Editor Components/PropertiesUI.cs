using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Retro;

namespace RetroEditor {
    public class PropertiesUI : Component {

        public List<BoxProperty> allBoxProperties;
        public List<BoxProperty> remainingProperties;
        public RetroboxPrefs preferences => e.preferences;

        bool expandedGreyProperties;
        public List<BoxProperty> allFrameProperties;
        public List<BoxProperty> remainingFrameProperties;
        //int framePropWindowItemCount = 0;

        int propWindowItemCount = 0;

        static UnityEngine.Color propWindowColour = new Color(0.3f, 0.3f, 0.3f, 0.75f);
        public static Texture2D propWindowTexture;
        public Rect propWindowRect;
        float margin => e.margin_;
        bool openClosed;

        public PropertiesUI(RetroboxEditor editor) {
            e = editor;

            propWindowTexture = new Texture2D(1, 1);
            propWindowTexture.SetPixel(0, 0, propWindowColour);
            propWindowTexture.Apply();

        }

        public void SetupBoxProperties() {
            allBoxProperties = new List<BoxProperty>();
            foreach (KeyValuePair<string, PDataType> kv in preferences.propsDictionary) {
                allBoxProperties.Add(new BoxProperty(kv.Key, preferences.propsDictionary[kv.Key]));
            }
            remainingProperties = allBoxProperties;
            e.shouldRepaint = true;

        }

        public void SetupFrameProperties() {
            //setup frame properties
            allFrameProperties = new List<BoxProperty>();
            foreach (KeyValuePair<string, PDataType> kv in preferences.framePropsDictionary) {
                allFrameProperties.Add(new BoxProperty(kv.Key, preferences.framePropsDictionary[kv.Key]));
            }
            remainingFrameProperties = allFrameProperties;
            e.shouldRepaint = true;

        }


        public void DrawPropertiesWindow(string propertiesName, List<BoxProperty> allProperties, List<BoxProperty> selectedProperties) {

            CalculatePropertiesWindow(allProperties, selectedProperties);

            int basePropertyRowsCount = 4;

            propWindowRect = new Rect(
                margin,
                margin + 24,
                openClosed ? 500 : 250,
                (propWindowItemCount + basePropertyRowsCount) * 21 + 50
            );

            float w = propWindowRect.width - 50;

            GUI.DrawTexture(propWindowRect, propWindowTexture);

            Rect pwr = new Rect(propWindowRect.x + margin,
                propWindowRect.y + margin,
                propWindowRect.width - (margin * 2),
                propWindowRect.height - (margin * 2)
            );

            using (new GUILayout.AreaScope(pwr)) {
                using (new EditorGUILayout.VerticalScope()) {
                    using (new EditorGUILayout.HorizontalScope()) {
                        EditorGUILayout.LabelField(propertiesName, EditorStyles.boldLabel);
                        if (GUILayout.Button(openClosed ? "◀" : "▶", GUILayout.MaxWidth(25))) {
                            ToggleOpenClosed();
                        }
                    }

                    GUILayout.Space(margin); //gap

                    if (e.selectedLayer != null) {
                        DrawFixedProperties(w, e.selectedLayer, e.selectedFrameIndex);
                    }

                    BoxProperty p;
                    for (int i = 0; i < selectedProperties.Count; i++) { //for each instance of a property that we have...
                        p = selectedProperties[i];
                        DrawProperty(w, p, true); //draw it
                    }

                    bool prevExpanded = expandedGreyProperties;
                    expandedGreyProperties = EditorGUILayout.Foldout(expandedGreyProperties, "more properties");

                    if (expandedGreyProperties) {
                        for (int i = 0; i < remainingProperties.Count; i++) { //for each of the remaining grey properties...
                            p = remainingProperties[i];
                            DrawProperty(w, p, false); //draw it.
                        }

                    }

                    if (prevExpanded != expandedGreyProperties) {
                        e.shouldRepaint = true;
                    }
                }
            }

            if (!e.paintedTwice) {
                e.paintedTwice = true;
                e.shouldRepaint = true;
            }
            e.paintedTwice = false;

        }

        void DrawFixedProperties(float width, Layer layer, int frameIndex) {
            float field = width * 0.5f;
            FrameData data = e.selectedFrameData;
            using (new GUILayout.HorizontalScope()) {
                EditorGUILayout.LabelField("position", GUILayout.Width(field));
                Undo.RecordObject(sheet, "changed position");
                data.position = EditorGUILayout.Vector2Field("", data.position, GUILayout.Width(field));
            }

            if (layer.kind == Shape.Point) {

                using (new GUILayout.HorizontalScope()) {
                    EditorGUILayout.LabelField("forward handle", GUILayout.Width(field));
                    Undo.RecordObject(sheet, "changed forward handle");
                    data.forwardHandle = EditorGUILayout.Vector2Field("", data.forwardHandle, GUILayout.Width(field));
                }

                using (new GUILayout.HorizontalScope()) {
                    EditorGUILayout.LabelField("back handle", GUILayout.Width(field));
                    Undo.RecordObject(sheet, "changed back handle");
                    data.backHandle = EditorGUILayout.Vector2Field("", data.backHandle, GUILayout.Width(field));
                }

            } else if (layer.kind == Shape.Box) {

                using (new GUILayout.HorizontalScope()) {
                    EditorGUILayout.LabelField("size", GUILayout.Width(field));
                    Undo.RecordObject(sheet, "changed size");
                    data.size = EditorGUILayout.Vector2Field("", data.size, GUILayout.Width(field));
                }
            }
        }


        void DrawProperty(float width, BoxProperty property, bool active) {
            GUI.enabled = active;
            float field = width * 0.5f;
            float addButtonWidth = 20;
            using (new GUILayout.HorizontalScope()) {

                if (property.dataType != PDataType.FMODEvent) {
                    EditorGUILayout.LabelField(property.name, GUILayout.Width(field));
                }
                //EditorGUILayout.LabelField(p.dataType.ToString(), GUILayout.Width(w * 0.2f));

                switch (property.dataType) { //draw the property

                    case PDataType.Bool:
                        Undo.RecordObject(sheet, "change " + property.name);
                        property.boolVal = EditorGUILayout.Toggle(active ? property.boolVal : false, GUILayout.Width(field));
                        break;

                    case PDataType.String:
                        Undo.RecordObject(sheet, "change " + property.name);
                        property.stringVal = EditorGUILayout.TextField(active ? property.stringVal : "", GUILayout.Width(field));
                        break;

                    case PDataType.Int:
                        Undo.RecordObject(sheet, "change " + property.name);
                        property.intVal = EditorGUILayout.IntField(active ? property.intVal : 0, GUILayout.Width(field));
                        break;

                    case PDataType.Float:
                        Undo.RecordObject(sheet, "change " + property.name);
                        property.floatVal = EditorGUILayout.FloatField(active ? property.floatVal : 0, GUILayout.Width(field));
                        break;

                    case PDataType.Vector2:
                        Undo.RecordObject(sheet, "change " + property.name);
                        property.vectorVal = EditorGUILayout.Vector2Field("", active ? property.vectorVal : new Vector2(), GUILayout.Width(field));
                        break;

                    case PDataType.Curve:
                        //just default to the first curve property that exists
                        if (e.curveUI.targetCurveProperty == null) {
                            e.curveUI.targetCurveProperty = property.curveVal;
                        }
                        //disable the button if we're already selecting it.
                        GUI.enabled = e.curveUI.targetCurveProperty != property.curveVal;
                        Undo.RecordObject(sheet, "change " + property.name);
                        if (GUILayout.Button(GUI.enabled ? "target" : "inspecting")) {
                            e.curveUI.targetCurveProperty = property.curveVal;
                        }
                        GUI.enabled = true;
                        break;

                    case PDataType.FMODEvent:

                        if (!GUI.enabled) {
                            EditorGUILayout.LabelField("FMOD EventReference", GUILayout.Width(width + 3));
                            break;
                        }
                        DrawFMODReference(property);
                        break;
                }

                if (active) { //List controls for this property (remove, add)
                    Undo.RecordObject(sheet, "delete property " + property.name);
                    if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(addButtonWidth))) {
                        if (e.inspectingFrameObject) {
                            e.selectedLayer.GetFrameData(e.selectedFrameIndex).props.Remove(property.name);
                        } else {
                            sheet.propertiesList[e.selectedFrameIndex].frameProperties.Remove(property);
                        }
                        e.shouldRepaint = true;
                    }
                } else {
                    GUI.enabled = true;
                    Undo.RecordObject(sheet, "added property " + property.name);
                    if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(addButtonWidth))) {
                        if (e.inspectingFrameObject) {
                            e.selectedLayer.GetFrameData(e.selectedFrameIndex).props.Add(property.name, new BoxProperty(property.name, property.dataType));
                        } else {
                            sheet.propertiesList[e.selectedFrameIndex].frameProperties.Add(new BoxProperty(property.name, property.dataType));
                        }
                        e.shouldRepaint = true;

                    }
                }
            }
            GUI.enabled = true;
        }

        void CalculatePropertiesWindow(List<BoxProperty> allProperties, List<BoxProperty> selectedProperties) {
            remainingProperties = new List<BoxProperty>();
            foreach (BoxProperty sProp in allProperties) { //list off all of the properties
                bool found = false;
                foreach (BoxProperty cProp in selectedProperties) { //check for instances of those properties
                    if (cProp.name.Equals(sProp.name)) {
                        found = true;
                    }
                }
                if (!found) remainingProperties.Add(sProp); //if we didn't find one, tack it on at the end as a greyed out property
            }

            propWindowItemCount = selectedProperties.Count;
            if (expandedGreyProperties) {
                propWindowItemCount += remainingProperties.Count;
            }
        }

        void ToggleOpenClosed() {
            openClosed = !openClosed;
        }


        void DrawFMODReference(BoxProperty property) {

            Undo.RecordObject(sheet, "change " + property.name);
            int index = e.selectedFrameIndex;

            SerializedObject serializedTarget = new SerializedObject(e.myTarget);
            SerializedProperty allProperties = serializedTarget.FindProperty("propertiesList");
            SerializedProperty currentFrameProperties = allProperties.GetArrayElementAtIndex(index);
            SerializedProperty properties = currentFrameProperties.FindPropertyRelative("frameProperties");
            for (int i = 0; i < properties.arraySize; i++) {
                SerializedProperty currentProperty = properties.GetArrayElementAtIndex(i);
                if (currentProperty.boxedValue is BoxProperty bp && bp.dataType.Equals(PDataType.FMODEvent)) {
                    SerializedProperty fmodProperty = currentProperty.FindPropertyRelative("fmodEventVal");

                    fmodProperty.isExpanded = false;

                    using (new GUILayout.HorizontalScope()) {
                        EditorGUILayout.LabelField("FMOD", GUILayout.Width(40));

                        float oldLabelWidth = EditorGUIUtility.labelWidth;
                        EditorGUIUtility.labelWidth = 1;
                        EditorGUILayout.PropertyField(fmodProperty, GUIContent.none);
                        EditorGUIUtility.labelWidth = oldLabelWidth;
                    }
                }
            }

        }
    }
}