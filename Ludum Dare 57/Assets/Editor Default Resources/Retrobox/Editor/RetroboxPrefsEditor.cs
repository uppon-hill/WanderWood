using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(Retro.RetroboxPrefs))]
public class RetroboxPrefsEditor : Editor {

    string oldFocusControl;
    Retro.RetroboxPrefs myTarget;
    string newBoxName;
    Retro.Shape newShape;

    string editingBox; //for renaming boxes (current name)
    string editingToBox; //for renaming boxes (new name)

    string newPropName;
    Retro.PDataType newPropType;
    string editingProp;
    string editingToProp;

    string newFramePropName;
    Retro.PDataType newFramePropType;
    string editingFrameProp;
    string editingToFrameProp;

    float labelWidth = 100;
    private void OnEnable() {
        newBoxName = "New BoxType";
        editingBox = null;
        editingToBox = null;

        newPropName = "New PropType";
        editingProp = null;
        editingToProp = null;

        newFramePropName = "New FramePropType";
        editingFrameProp = null;
        editingToFrameProp = null;
    }

    public override void OnInspectorGUI() {
        myTarget = (Retro.RetroboxPrefs)target;

        using (new EditorGUILayout.VerticalScope()) {
            DrawNewShapeField();
            DrawBoxTypes();
            GUILayout.Space(30);
            DrawPointTypesList();
            GUILayout.Space(30);
            DrawBoxPropertiesList();
            GUILayout.Space(30);
            DrawFramePropertiesList();
            GUILayout.Space(30);
            DrawUpdateButton();

        }

        //save if we've made any changes
        if (!GUI.GetNameOfFocusedControl().Equals(oldFocusControl)) { //save if we change control focus
            Save();
        }

        if (Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyDown) { //save if we press enter
            Save();
        }

        oldFocusControl = GUI.GetNameOfFocusedControl();

    }

    //--- PREFERENCES
    public void DrawBoxTypes() {
        //Draw some labels to start

        //a single field is worth 
        float fieldWidth = (Screen.width - 20 - 100) * 0.20f;

        EditorGUILayout.LabelField("Group Types", EditorStyles.boldLabel, GUILayout.MinWidth(100));
        // GUILayout.MaxWidth(labelWidth)
        /*
            using (new EditorGUILayout.HorizontalScope(GUILayout.MaxWidth(Screen.width))) {
                EditorGUILayout.LabelField("Colour");
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Box Type");
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Phys Layer");
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Phys Material");
                EditorGUILayout.Space(10);

                EditorGUILayout.LabelField("Rounded");
                EditorGUILayout.Space(60);

            }
    */

        //draw each of our list contents
        for (int i = 0; i < myTarget.boxDictionary.Count; i++) {
            DrawBoxType(i);
        }

        EditorGUILayout.Space();
    }

    void DrawBoxType(int i) {
        using (new EditorGUILayout.HorizontalScope()) {

            //properties
            DrawColourField(myTarget.boxDictionary, i);

            GUILayout.Space(10);

            DrawLayerNameField(myTarget.boxDictionary, i);

            GUILayout.Space(10);

            Undo.RecordObject(myTarget, "set physics layer");
            GUI.SetNextControlName("layer " + i);
            myTarget.boxDictionary[i].physicsLayer = EditorGUILayout.IntField(myTarget.boxDictionary[i].physicsLayer, GUILayout.Width(30));

            //Clamp the integer between 0 and 31 (32 layer slots supported by Unity)
            if (myTarget.boxDictionary[i].physicsLayer < 0) myTarget.boxDictionary[i].physicsLayer = 0;
            if (myTarget.boxDictionary[i].physicsLayer > 31) myTarget.boxDictionary[i].physicsLayer = 31;

            EditorGUILayout.LabelField(LayerMask.LayerToName(myTarget.boxDictionary[i].physicsLayer), GUILayout.MaxWidth(labelWidth));

            GUILayout.Space(10);

            Undo.RecordObject(myTarget, "set physics material");
            GUI.SetNextControlName("material " + i);
            myTarget.boxDictionary[i].material = (PhysicsMaterial2D)EditorGUILayout.ObjectField(myTarget.boxDictionary[i].material, typeof(PhysicsMaterial2D), false);

            DrawRoundedField(i);
            GUILayout.Space(10);

            DrawAggressorField(i);
            GUILayout.Space(10);

            DrawListControls(myTarget.boxDictionary, i);
        }
    }

    void DrawRoundedField(int i) {
        Undo.RecordObject(myTarget, "set rounded");
        GUI.SetNextControlName("rounded " + i);
        string label = myTarget.boxDictionary[i].isRounded ? "rounded" : "square";

        myTarget.boxDictionary[i].isRounded = EditorGUILayout.Toggle(myTarget.boxDictionary[i].isRounded, GUILayout.MaxWidth(20));
        EditorGUILayout.LabelField(label, GUILayout.MaxWidth(labelWidth * 0.5f));
    }

    void DrawAggressorField(int i) {
        Undo.RecordObject(myTarget, "set collider type");
        GUI.SetNextControlName("collidee " + i);
        string label = myTarget.boxDictionary[i].isAggressor ? "attacks" : "defends";

        myTarget.boxDictionary[i].isAggressor = EditorGUILayout.Toggle(myTarget.boxDictionary[i].isAggressor, GUILayout.MaxWidth(20));
        EditorGUILayout.LabelField(label, GUILayout.MaxWidth(labelWidth * 0.5f));
    }


    void DrawColourField(BoxDataDictionary d, int i) {
        Undo.RecordObject(myTarget, "set colour");
        GUI.SetNextControlName("colour " + i);
        d[i].colour = EditorGUILayout.ColorField(d[i].colour, GUILayout.MaxWidth(50));

    }



    void DrawLayerNameField(BoxDataDictionary d, int i) {
        Undo.RecordObject(myTarget, "set layer name");
        GUI.SetNextControlName("name " + i);
        if (i != d.IndexOf(editingBox)) {
            GUI.enabled = false;
            EditorGUILayout.TextField(d[i].boxTypeName);
            GUI.enabled = true;

        } else {
            editingToBox = EditorGUILayout.TextField(editingToBox);
        }

        Undo.RecordObject(myTarget, "edit box type");
        GUI.SetNextControlName("edit " + i);
        if (i != d.IndexOf(editingBox)) {
            if (GUILayout.Button("...", EditorStyles.miniButton)) PrefsBeginEdit(d, i);
        } else {
            if (GUILayout.Button("...", EditorStyles.miniButton)) PrefsBeginEdit(d, i);
        }

    }

    public void DrawPointTypesList() {
        if (myTarget.pointDictionary == null) {
            myTarget.pointDictionary = new BoxDataDictionary();
        }
        //Draw some labels to start
        float fieldWidth = (Screen.width - 20 - 100) * 0.25f;

        EditorGUILayout.LabelField("Group Types", EditorStyles.boldLabel, GUILayout.MinWidth(100));

        using (new EditorGUILayout.HorizontalScope()) {
            EditorGUILayout.LabelField("Colour", GUILayout.Width(fieldWidth * .5f));
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Group Name", GUILayout.Width(fieldWidth - 10));
            GUILayout.Space(10);

        }

        //draw each of our list contents
        for (int i = 0; i < myTarget.pointDictionary.Count; i++) {
            using (new EditorGUILayout.HorizontalScope()) {

                //properties
                DrawColourField(myTarget.pointDictionary, i);
                GUILayout.Space(10);
                DrawLayerNameField(myTarget.pointDictionary, i);
                GUILayout.Space(10);
                DrawListControls(myTarget.pointDictionary, i);
            }
        }
        EditorGUILayout.Space();

    }

    void DrawNewShapeField() {

        //draw the "Add new box type" button.
        EditorGUILayout.BeginHorizontal();

        newBoxName = GUILayout.TextField(newBoxName);
        newShape = (Retro.Shape)EditorGUILayout.EnumPopup(newShape, GUILayout.MaxWidth(labelWidth));

        if (GUILayout.Button("+ Box Type", EditorStyles.miniButton, GUILayout.MaxWidth(labelWidth)) && !string.IsNullOrEmpty(newBoxName)) {
            Undo.RecordObject(myTarget, "add new box type");
            try {
                Color colour = new Color(Random.value, Random.value, Random.value, 1);
                Retro.BoxData newBoxData = new Retro.BoxData(colour, newBoxName, 0, false, newShape);
                myTarget.GetShapeDictionary(newShape).Add(newBoxName, newBoxData);
            } catch (System.Exception e) {
                Debug.Log(e);
            }
            //myTarget.boxTypes.Add(new Retro.BoxData());
        }
        EditorGUILayout.EndHorizontal();
    }



    void PrefsBeginEdit(BoxDataDictionary d, int i) {
        if (editingBox != null) {
            PrefsCancelEdit();
        }
        editingBox = d.GetKey(i);
        editingToBox = editingBox;
    }

    private void PrefsEndEdit() {

        if (!editingToBox.Equals(editingBox)) {
            int updated = 0;
            string[] sheetReferences = AssetDatabase.FindAssets("t:Sheet");
            Retro.Sheet[] sheets = new Retro.Sheet[sheetReferences.Length];
            for (int i = 0; i < sheetReferences.Length; i++) {

                sheets[i] = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(sheetReferences[i]), typeof(Retro.Sheet)) as Retro.Sheet;

                foreach (Retro.Layer l in sheets[i].layers) {
                    if (l.myBoxType.Equals(editingBox)) {
                        l.myBoxType = editingToBox; //do something
                        updated++;//we did something
                        EditorUtility.SetDirty(sheets[i]);//we did something
                    }
                }

            }


            Retro.BoxData d = myTarget.boxDictionary[editingBox];
            //myTarget.boxDictionary.Remove(editingBox);
            //myTarget.boxDictionary.Add(editingToBox, d);
            //myTarget.boxDictionary[editingToBox].boxTypeName = editingToBox;

            //myTarget.boxNames.Insert(myTarget.boxNames.IndexOf(editingBox), editingToBox);
            //myTarget.boxNames.Remove(editingBox);

            myTarget.boxDictionary.InsertAt(myTarget.boxDictionary.IndexOf(editingBox), editingToBox, d);
            myTarget.boxDictionary.Remove(editingBox);
            myTarget.boxDictionary[editingToBox].boxTypeName = editingToBox;


            EditorUtility.SetDirty(myTarget);
            AssetDatabase.SaveAssets();
            Debug.Log(sheetReferences.Length + " total sheets found, " + updated + " sheets with '" + editingBox + "' groups renamed to '" + editingToBox + "'.");
        }
        editingBox = null;
        editingToBox = null;

    }

    private void PrefsCancelEdit() {
        editingBox = null;
        editingToBox = null;
    }


    //--- PROPERTIES
    public void DrawBoxPropertiesList() {
        float fieldWidth = (Screen.width - 95);

        EditorGUILayout.LabelField("Hitbox Properties", EditorStyles.boldLabel, GUILayout.MinWidth(100));

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Name", GUILayout.MaxWidth(labelWidth));
        EditorGUILayout.LabelField("Data Type", GUILayout.MaxWidth(labelWidth));
        EditorGUILayout.EndHorizontal();


        //draw each of our list contents
        for (int i = 0; i < myTarget.propsDictionary.Count; i++) {
            DrawBoxProperty(i);

        }
        GUILayout.Space(10);

        DrawNewPropertyTypeField();
    }

    void DrawBoxProperty(int i) {
        using (new EditorGUILayout.HorizontalScope()) {

            Undo.RecordObject(myTarget, "set property name");
            GUI.SetNextControlName("name " + i);
            if (i != myTarget.propsDictionary.IndexOf(editingProp)) {
                GUI.enabled = false;
                EditorGUILayout.TextField(myTarget.propsDictionary.GetKey(i));
                GUI.enabled = true;

            } else {
                editingToProp = EditorGUILayout.TextField(editingToProp);
            }

            Undo.RecordObject(myTarget, "edit property name");
            GUI.SetNextControlName("edit " + i);
            if (i != myTarget.propsDictionary.IndexOf(editingProp)) {
                if (GUILayout.Button("...", EditorStyles.miniButton, GUILayout.Width(20))) PropsBeginEdit(i);
            } else {
                if (GUILayout.Button("✓", EditorStyles.miniButton, GUILayout.Width(20))) PropsEndEdit();
            }

            GUILayout.Space(10);
            EditorGUILayout.LabelField(myTarget.propsDictionary[i].ToString(), GUILayout.MaxWidth(labelWidth));


            //list controls
            DrawListControls(myTarget.propsDictionary, i);
        }
    }

    void DrawNewPropertyTypeField() {
        //draw the "Add new box type" button.
        using (new EditorGUILayout.HorizontalScope()) {
            float npfMargin = 40;
            float commitWidth = 100;
            newPropName = GUILayout.TextField(newPropName);
            newPropType = (Retro.PDataType)EditorGUILayout.EnumPopup(newPropType, GUILayout.MaxWidth(labelWidth));
            if (GUILayout.Button("+ Property", EditorStyles.miniButton, GUILayout.Width(commitWidth)) && !string.IsNullOrEmpty(newPropName)) {
                Undo.RecordObject(myTarget, "add new prop type");
                try {
                    myTarget.propsDictionary.Add(newPropName, newPropType);
                } catch (System.Exception e) {
                    Debug.Log(e);
                }
                //myTarget.boxTypes.Add(new Retro.BoxData());
            }
        }
    }


    void PropsBeginEdit(int i) {
        if (editingProp != null) {
            PropsCancelEdit();
        }
        editingProp = myTarget.propsDictionary.GetKey(i);
        editingToProp = editingProp;
    }

    private void PropsEndEdit() {

        if (!editingToProp.Equals(editingProp)) {
            int updated = 0;
            string[] sheetReferences = AssetDatabase.FindAssets("t:Sheet");
            Retro.Sheet[] sheets = new Retro.Sheet[sheetReferences.Length];
            for (int i = 0; i < sheetReferences.Length; i++) {

                sheets[i] = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(sheetReferences[i]), typeof(Retro.Sheet)) as Retro.Sheet;

                foreach (Retro.Layer l in sheets[i].layers) {
                    foreach (Retro.FrameData f in l.frameDataById.Values) {

                        Retro.BoxProperty newP = new Retro.BoxProperty("null", Retro.PDataType.Bool);
                        bool found = false;
                        foreach (KeyValuePair<string, Retro.BoxProperty> p in f.props) {
                            if (p.Key.Equals(editingProp)) {
                                found = true;
                                newP = p.Value;
                                newP.name = editingToProp;
                                break;
                            }
                        }
                        if (found) {
                            f.props.Remove(editingProp);
                            f.props.Add(editingToProp, newP);
                            updated++;//we did something
                            EditorUtility.SetDirty(sheets[i]);//we did something
                        }
                    }

                }
            }
            Retro.PDataType d = myTarget.propsDictionary[editingProp];
            /*
            myTarget.propsDictionary.Remove(editingProp);
            myTarget.propsDictionary.Add(editingToProp, d);
            //myTarget.propsDictionary[editingToProp].name = editingToProp;
            myTarget.propNames.Insert(myTarget.propNames.IndexOf(editingProp), editingToProp);
            myTarget.propNames.Remove(editingProp);
            */

            myTarget.propsDictionary.InsertAt(myTarget.propsDictionary.IndexOf(editingProp), editingToProp, d);
            myTarget.propsDictionary.Remove(editingProp);

            EditorUtility.SetDirty(myTarget);
            AssetDatabase.SaveAssets();
            Debug.Log(sheetReferences.Length + " total sheets found, " + updated + " sheets with '" + editingProp + "' hitbox properties renamed to '" + editingToProp + "'.");
        }
        editingProp = null;
        editingToProp = null;

    }

    private void PropsCancelEdit() {
        editingProp = null;
        editingToProp = null;
    }

    //--- FRAME PROPERTIES
    public void DrawFramePropertiesList() {
        float fieldWidth = (Screen.width - 95);

        EditorGUILayout.LabelField("Frame Properties", EditorStyles.boldLabel, GUILayout.MinWidth(100));

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Name");
        EditorGUILayout.LabelField("Data Type", GUILayout.MaxWidth(labelWidth));
        EditorGUILayout.EndHorizontal();


        //draw each of our list contents
        for (int i = 0; i < myTarget.framePropsDictionary.Count; i++) {
            DrawFrameProperty(i);
        }

        EditorGUILayout.Space();

        //draw the "Add new box type" button.
        using (new EditorGUILayout.HorizontalScope()) {
            float npfMargin = 40;
            float commitWidth = 100;
            float newPropFieldWidth = (Screen.width - commitWidth - npfMargin) * 0.5f;
            newFramePropName = GUILayout.TextField(newFramePropName);
            newFramePropType = (Retro.PDataType)EditorGUILayout.EnumPopup(newFramePropType, GUILayout.MaxWidth(labelWidth));
            if (GUILayout.Button("+ Property", EditorStyles.miniButton, GUILayout.MaxWidth(labelWidth)) && !string.IsNullOrEmpty(newFramePropName)) {
                Undo.RecordObject(myTarget, "add new prop type");
                try {
                    myTarget.framePropsDictionary.Add(newFramePropName, newFramePropType);
                } catch (System.Exception e) {
                    Debug.Log(e);
                }
                //myTarget.boxTypes.Add(new Retro.BoxData());
            }
        }
    }

    void DrawFrameProperty(int i) {
        using (new EditorGUILayout.HorizontalScope()) {

            Undo.RecordObject(myTarget, "set property name");
            GUI.SetNextControlName("name " + i);
            if (i != myTarget.framePropsDictionary.IndexOf(editingFrameProp)) {
                GUI.enabled = false;
                EditorGUILayout.TextField(myTarget.framePropsDictionary.GetKey(i));
                GUI.enabled = true;
            } else {
                editingToFrameProp = EditorGUILayout.TextField(editingToFrameProp);
            }

            Undo.RecordObject(myTarget, "edit property name");
            GUI.SetNextControlName("edit " + i);
            if (i != myTarget.framePropsDictionary.IndexOf(editingFrameProp)) {
                if (GUILayout.Button("...", EditorStyles.miniButton, GUILayout.Width(20))) PropsBeginEdit(i);
            } else {
                if (GUILayout.Button("✓", EditorStyles.miniButton, GUILayout.Width(20))) PropsEndEdit();
            }

            GUILayout.Space(10);
            EditorGUILayout.LabelField(myTarget.framePropsDictionary[i].ToString(), GUILayout.MaxWidth(labelWidth));


            //list controls
            DrawListControls(myTarget.framePropsDictionary, i);
        }
    }
    void FramePropsBeginEdit(int i) {
        if (editingFrameProp != null) {
            FramePropsCancelEdit();
        }
        editingFrameProp = myTarget.framePropsDictionary.GetKey(i);
        editingToFrameProp = editingFrameProp;
    }

    private void FramePropsEndEdit() {

        if (!editingToFrameProp.Equals(editingFrameProp)) {
            int updated = 0;
            string[] sheetReferences = AssetDatabase.FindAssets("t:Sheet");
            Retro.Sheet[] sheets = new Retro.Sheet[sheetReferences.Length];
            for (int i = 0; i < sheetReferences.Length; i++) {

                sheets[i] = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(sheetReferences[i]), typeof(Retro.Sheet)) as Retro.Sheet;

                for (int j = 0; j < sheets[i].propertiesList.Count; j++) {
                    foreach (Retro.BoxProperty p in sheets[i].propertiesList[j].frameProperties) {
                        if (p.name.Equals(editingFrameProp)) {
                            p.name = editingToFrameProp; //do something

                            updated++;//we did something
                            EditorUtility.SetDirty(sheets[i]);//we did something
                        }
                    }
                }

            }
            Retro.PDataType d = myTarget.framePropsDictionary[editingFrameProp];
            /*
            myTarget.framePropsDictionary.Remove(editingFrameProp);
            myTarget.framePropsDictionary.Add(editingToFrameProp, d);
            //myTarget.framePropsDictionary[editingToFrameProp].name = editingToFrameProp;
            myTarget.propNames.Insert(myTarget.propNames.IndexOf(editingFrameProp), editingToFrameProp);
            myTarget.propNames.Remove(editingFrameProp);
            */

            myTarget.framePropsDictionary.InsertAt(myTarget.framePropsDictionary.IndexOf(editingFrameProp), editingToFrameProp, d);
            myTarget.framePropsDictionary.Remove(editingFrameProp);

            EditorUtility.SetDirty(myTarget);
            AssetDatabase.SaveAssets();
            Debug.Log(sheetReferences.Length + " total sheets found, " + updated + " sheets with '" + editingFrameProp + "' hitbox properties renamed to '" + editingToFrameProp + "'.");
        }
        editingFrameProp = null;
        editingToFrameProp = null;

    }

    private void FramePropsCancelEdit() {
        editingFrameProp = null;
        editingToFrameProp = null;
    }

    //list controls
    void DrawListControls<Tkey, TValue>(SerializableDictionary<Tkey, TValue> dictionary, int i) {
        Undo.RecordObject(myTarget, "move up");
        GUI.SetNextControlName("up " + i);
        if (GUILayout.Button("▲", EditorStyles.miniButton, GUILayout.Width(20))) dictionary.MoveUp(i);

        Undo.RecordObject(myTarget, "move down");
        GUI.SetNextControlName("down " + i);
        if (GUILayout.Button("▼", EditorStyles.miniButton, GUILayout.Width(20))) dictionary.MoveDown(i);

        Undo.RecordObject(myTarget, "remove box type");
        GUI.SetNextControlName("remove " + i);
        if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(20))) {
            dictionary.Remove(i);
        }

    }

    //List Operations
    //move an object up in the list
    List<T> MoveUp<T>(List<T> list, int i) {
        if (i > 0) {
            T s = list[i - 1];
            list[i - 1] = list[i];
            list[i] = s;
        }
        return list;
    }

    //move an object down in the list
    List<T> MoveDown<T>(List<T> list, int i) {
        if (i < list.Count - 1) {
            T s = list[i + 1];
            list[i + 1] = list[i];
            list[i] = s;
        }
        return list;
    }

    //remove an object from the list the the given index
    void Remove(int i) {
        myTarget.boxDictionary.Remove(i);
    }
    public void DrawUpdateButton() {
        if (GUILayout.Button("Update")) {
            UpdatePreferencesFile();
        }
    }

    public void UpdatePreferencesFile() {
        Debug.Log("attempting update...");


        EditorUtility.SetDirty(myTarget);
        AssetDatabase.SaveAssets();
        Debug.Log("Update Complete");
    }
    void Save() {
        EditorUtility.SetDirty(myTarget);

    }
}

