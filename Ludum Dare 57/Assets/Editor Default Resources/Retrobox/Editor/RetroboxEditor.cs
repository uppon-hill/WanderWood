using UnityEngine;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using System.Collections.Generic;
using System.Linq;
using Retro;
using RetroEditor;
using UnityEngine.Events;

//(c) 2024 Adam Younis
public class RetroboxEditor : EditorWindow {

    public string version = "1.0";

    public SerializedObject mySerializedObject;
    public GUISkin mySkin;
    public RetroboxPrefs preferences;

    //"instance variables"
    public Sheet myTarget;
    public bool targetIsRetroSheet => myTarget != null && mySerializedObject != null && loadedSprites;
    public UnityEngine.Object prevTarget;
    public int selectedFrameIndex = 0;
    public int prevSelectedFrameIndex = 0;
    public List<Texture2D> frameTextures;

    //UI Components
    public SpriteWindowUI spriteUI;
    public TimelineUI timelineUI;
    public ToolbarUI toolbarUI;
    public LayerUI layerUI;
    public FrameUI frameUI;
    public PropertiesUI propertiesUI;
    public HitboxUI hitboxUI;
    public CurveUI curveUI;
    public MessagingUI messagingUI;

    public FileUI fileUI;

    private object copyData;

    public float margin_ = 10;
    public float padding_ = 5;


    public bool resize = false;
    public Rect canvasPartition;

    public Layer parsingLayer;
    public Layer selectedLayer;
    public FrameData selectedFrameData => selectedLayer.GetFrameData(selectedFrameIndex) != null ? selectedLayer.GetFrameData(selectedFrameIndex) : FrameData.emptyData;
    public Layer prevSelectedLayer;
    UnityEvent newSelection;

    public bool followingKeyframe;

    public bool playing; //simulate animation playback;
    public int frameOnPlay;
    public double timeAtPlay;
    public bool loadedSprites = false;

    Vector2 mouseDownPos;
    Vector2 mouseClickPos;

    float mouseDownTime;
    int consecutiveClicks;
    public List<Layer> clickedFrames;

    public bool paintedTwice;

    public bool shouldRepaint;
    public float propertiesOffset;
    Layer toDelete;

    static Dictionary<string, Color> colours;
    public static Dictionary<string, Texture2D> colourTextures;

    public Texture2D logo;

    //Icon Textures ---


    public static Texture2D selectedTexture;
    static UnityEngine.Color selectedColour = new Color(0.7f, 0.7f, 0.7f);

    Texture2D ellipsis;
    Texture2D partitionHandle;

    public bool gridSetting = true;

    public Vector2 prevMouse;    //the position of the mouse at the start of a resize / transform.

    public bool inspectingFrameObject = false;


    public Vector2Int importSize;
    public Vector2 importPivot;


    public enum EditorMode { Default, Move, Play };
    public EditorMode mode;

    [MenuItem("Window/Insignia/Retrobox Editor")]
    public static void Init() {
        RetroboxEditor window = (RetroboxEditor)GetWindow(typeof(RetroboxEditor));
        window.minSize = new Vector2(150, 450);
        window.Show();
        Texture icon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Editor Default Resources/Retrobox/Images/RetroboxEditor.png");
        GUIContent titleContent = new GUIContent("Retrobox", icon);
        window.titleContent = titleContent;

        //on selection change, update the window
        Selection.selectionChanged += window.OnSelectionChange;
    }

    void OnFocus() {
        OnEnable();
    }

    private void OnLostFocus() {
        if (myTarget != null) {
            Save();
        }
    }

    void OnEnable() {
        Undo.undoRedoPerformed += UndoCallback;
        LoadComponents();
        LoadTextures();
        LoadPreferences();
        SetupEditorInstances();
        propertiesUI.SetupBoxProperties();
        propertiesUI.SetupFrameProperties();
        LoadSelectedSheet();

    }

    void LoadComponents() {
        spriteUI = new SpriteWindowUI(this);
        timelineUI = new TimelineUI(this);
        toolbarUI = new ToolbarUI(this);
        frameUI = new FrameUI(this);
        layerUI = new LayerUI(this);
        propertiesUI = new PropertiesUI(this);
        hitboxUI = new HitboxUI(this);
        curveUI = new CurveUI(this);
        messagingUI = new MessagingUI(this);
        fileUI = new FileUI(this);
    }
    void LoadSelectedSheet() {
        if (Selection.activeObject != null) {
            if (Selection.activeObject.GetType() == typeof(Sheet)) {
                myTarget = (Sheet)Selection.activeObject;
                mySerializedObject = new UnityEditor.SerializedObject(myTarget);

                LoadSprites();
                InitializeRetroSheet();

            }
        }

    }

    void SetupEditorInstances() {
        selectedLayer = null;

        prevSelectedLayer = null;
        //selectedFrameIndex = 0;
        //prevSelectedFrameIndex = 0;
        newSelection = new UnityEvent();
        newSelection.AddListener(curveUI.ResetTargetCurvePropertySelection);
        canvasPartition = new Rect(0, 256, 750, 8f);
        paintedTwice = false;
    }

    void LoadTextures() {

        mySkin = (GUISkin)(EditorGUIUtility.Load("Retrobox/Retroskin.guiskin"));
        logo = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RetroBoxEditor_logo.png"));

        selectedTexture = new Texture2D(1, 1);
        selectedTexture.SetPixel(0, 0, selectedColour);
        selectedTexture.Apply();



        ellipsis = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons10.png"));

        partitionHandle = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_windowHandle.png"));



    }


    void LoadPreferences() {
        gridSetting = true;
        colours = new Dictionary<string, Color>();
        colourTextures = new Dictionary<string, Texture2D>();

        preferences = (RetroboxPrefs)(Resources.Load("Retrobox Preferences"));

        if (preferences != null) {
            gridSetting = preferences.cachedGridSetting;
            spriteUI.zoomSetting = preferences.cachedZoomSetting;
            foreach (Shape s in Shape.GetValues(typeof(Shape))) {
                PopulateColourDictionary(preferences.GetShapeDictionary(s));
            }

        }

    }

    void PopulateColourDictionary(BoxDataDictionary d) {
        for (int i = 0; i < d.Count; i++) {
            Color c = d[i].colour;
            string s = d[i].boxTypeName;
            colours.Add(s, c);
            Texture2D groupColourImage = new Texture2D(16, 16);
            groupColourImage = RetroEditor.Utilities.FillImage(groupColourImage, c);
            groupColourImage.Apply();
            colourTextures.Add(s, groupColourImage);
        }
    }

    private void LoadSprites() {
        selectedFrameIndex = 0;
        loadedSprites = false;
        if (myTarget.spriteList != null) {
            if (myTarget.spriteList.Count > 0) {

                spriteUI.spritePreview = GetTextureFromSprite(myTarget.spriteList[selectedFrameIndex], FilterMode.Point);
                frameTextures = new List<Texture2D>();
                for (int i = 0; i < myTarget.spriteList.Count; i++) {
                    frameTextures.Add(GetTextureFromSprite(myTarget.spriteList[i], FilterMode.Point));
                }

                loadedSprites = true;
            }
        }
    }


    private void InitializeRetroSheet() {
        if (myTarget.spriteList != null) {
            bool setup = false;
            if (myTarget.propertiesList == null) {
                myTarget.propertiesList = new List<Properties>();
                setup = true;
            } else if (myTarget.propertiesList.Count != myTarget.spriteList.Count) {
                setup = true;
            }

            if (setup) {
                for (int i = myTarget.propertiesList.Count; i < myTarget.spriteList.Count; i++) {
                    myTarget.propertiesList.Add(new Properties());
                }

            }
        }
    }

    private void OnSelectionChange() {
        Repaint();
    }

    void Update() {
        if (playing) {
            Repaint();
        }
    }

    //Basically our "update" function... Do all of the things.
    void OnGUI() {
        //Debug.Log(resizingLayer == null);
        preferences = (RetroboxPrefs)(EditorGUIUtility.Load("Retrobox/Resources/Retrobox Preferences.asset"));
        GUI.skin = mySkin;

        UpdateSelection();


        if (spriteUI.spritePreview == null && targetIsRetroSheet) {
            LoadSprites();
        }

        CalculatePartition();
        canvasPartition = new Rect(canvasPartition.x, this.position.height - timelineUI.windowH, this.position.width, 8f);

        if (targetIsRetroSheet) mySerializedObject.Update();
        else selectedFrameIndex = 0;

        using (new GUILayout.VerticalScope()) {
            //draw sprite window & frames
            DrawMainWindow();
            DrawProperties();
            DrawPartition();
            toolbarUI.Draw();
            DrawTimelineWindow();
            DrawFileUI();

        }


        if (targetIsRetroSheet) {
            //  SweepCopyFrames(); //fix any copy frames, updating their values to their respective keyframes.
            HandleInputs(); //handle any user input
            spriteUI.UpdateScale(); //carry out any updates to the scale of the sprite
            DeleteMarkedLayer(); //delete a group we decided to delete during this frame
            CheckNewSelection(); //check for a newly selected hitbox
        }

        if (playing) {
            selectedFrameIndex = (frameOnPlay + (int)((EditorApplication.timeSinceStartup - timeAtPlay) * 10)) % myTarget.spriteList.Count;
            //selectedFrame = ((int)(EditorApplication.timeSinceStartup * 10) % myTarget.spriteList.Count);
        }

        CheckDragAndDrop();
    }


    void DrawFileUI() {
        fileUI.Draw();
    }

    public void UpdateSelection() {
        //we're actually focused on an animation
        if (Selection.activeObject != null) {
            if (prevTarget != Selection.activeObject && Selection.activeObject.GetType() == typeof(Sheet)) {
                OnEnable();
            }
            prevTarget = Selection.activeObject;
        }


    }


    public void DrawMainWindow() {
        spriteUI.SetupCanvas();


        if (preferences == null) {
            messagingUI.NoPreferences();

        } else if (targetIsRetroSheet) {
            spriteUI.DrawCanvas();

        } else if (Selection.activeObject != null) {
            if (Selection.activeObject.GetType() != typeof(Sheet)) {
                messagingUI.Message(Selection.activeObject.name + " is not a Retro Animation");
            } else if (!loadedSprites) {

                messagingUI.Message(
                    "The sprite array for " + myTarget.name + " is empty.",
                    "Drop sprites in to begin."
                );
            }
        } else {
            messagingUI.NothingSelected();
        }

        DrawLogo();

    }

    void DrawLogo() {
        //Logo
        Rect window = SpriteWindowUI.window;
        GUI.DrawTexture(new Rect(window.width - logo.width - 12, window.height - logo.height - 8, logo.width, logo.height), logo);
    }

    //draw the timeline contents
    public void DrawTimelineWindow() {
        timelineUI.DrawWindowBG();
        if (!playing && targetIsRetroSheet) {
            using (new GUILayout.HorizontalScope()) {
                timelineUI.DrawLayersPanel();
                timelineUI.Draw();
            }
        }

    }


    public void AddNewLayer(object o) {
        Shape shape = (Shape)o;
        BoxDataDictionary sd = preferences.GetShapeDictionary(shape);

        Layer l = new Layer(sd.GetKey(0), shape);
        SetupLayer(l);
        myTarget.layers.Add(l);
    }

    void SetupLayer(Layer l) {

        //for each of the frames...
        for (int i = 0; i < myTarget.spriteList.Count; i++) {

            if (i == 0) {
                Frame f = new Frame(Frame.Kind.KeyFrame);
                FrameData d = new FrameData();
                d.size = new Vector2(16, 16);
                f.dataId = d.guid;
                d.keyFrameId = f.guid;
                l.frames.Add(f);
                l.frameDataById.Add(d.guid, d);

            } else {
                Frame f = new Frame();
                l.Add(f);
            }
        }
        l.ResyncFrames(0);
    }

    //Draw window handle
    private void DrawPartition() {
        GUI.DrawTexture(canvasPartition, EditorGUIUtility.whiteTexture);
        GUI.DrawTexture(new Rect((canvasPartition.x + canvasPartition.width * 0.5f) - 7, canvasPartition.y, 14, canvasPartition.height), partitionHandle);
        EditorGUIUtility.AddCursorRect(canvasPartition, MouseCursor.ResizeVertical);

    }

    private void CalculatePartition() {
        if (Event.current.type == EventType.MouseDown && canvasPartition.Contains(Event.current.mousePosition)) {
            resize = true;
        }
        if (resize && Event.current.mousePosition.y > 100 && Event.current.mousePosition.y < this.position.height - 100) {
            timelineUI.windowH = this.position.height - Event.current.mousePosition.y;
            canvasPartition.Set(0, Event.current.mousePosition.y, position.width, canvasPartition.height);
        }
        if (Event.current.type == EventType.MouseUp) {
            resize = false;

        }
    }


    //Draw the frames in the editor
    public void DrawFrameShapes() {
        bool selectingNonEmptyFrame = IsALayerSelected() && !selectedLayer.frames[selectedFrameIndex].IsEmpty();
        clickedFrames = new List<Layer>();
        hitboxUI.mouseHandles = new Stack<KeyValuePair<Rect, MouseCursor>>();

        //if we've gone to the trouble of selecting a layer already... Draw that one first
        // if (IsALayerSelected() && selectingNonEmptyFrame) {
        //    DrawFrame(selectedGroup, selectedLayer);
        //}

        for (int i = myTarget.layers.Count - 1; i >= 0; i--) { //for every group in the sheet
            parsingLayer = myTarget.layers[i]; //current group = index...

            if (parsingLayer.visible) { //if we can see the group

                if (
                !LayerIsSelected(parsingLayer) &&
                !parsingLayer.frames[selectedFrameIndex].IsEmpty()) {
                    DrawFrame(parsingLayer, true);
                }
            }
        }

        //draw the selected layer again since handles read front-back while canvas is drawn back-front.
        if (IsALayerSelected() && selectingNonEmptyFrame) {
            DrawFrame(selectedLayer);
        }

        hitboxUI.ConfigureMouseCursor();
        CheckClickedObjects();
    }



    void CheckClickedObjects() {

        if (clickedFrames.Count > 0) { //if we clicked at least one frame
            int max = clickedFrames.Count - 1;
            int index = consecutiveClicks - 1;
            int modIndex = index % clickedFrames.Count;
            selectedLayer = clickedFrames[max - modIndex];//get the nth clicked layer

        } else if (ClickedRect(SpriteWindowUI.window)) {//we clicked empty space, deselect
            if (!ClickedRect(propertiesUI.propWindowRect) && !ClickedRect(curveUI.curvePropRect)) {
                selectedLayer = null;
            }
        }
    }

    void DrawFrame(Layer layer, bool visible = true) {

        switch (layer.kind) {
            case Shape.Box:
                hitboxUI.DrawHitbox(layer, visible);
                break;
            case Shape.Point:
                curveUI.DrawPoint(layer, visible);
                break;
        }
    }

    public void SetHandleColour(Layer layer) {
        try {
            //set the colour we'll be using
            Handles.color = (!IsALayerSelected() || LayerIsSelected(layer)) ?
            colours[layer.myBoxType] :
            Handles.color = SpriteWindowUI.spritewindowColour;
        } catch (System.Exception e) {
            Debug.LogError("The active Retrobox Preferences file does not contain an entry for '" + layer.myBoxType + "'. " + e);
        }
    }


    void DrawProperties() {
        //draw the properties window
        if (!playing && targetIsRetroSheet) {
            if (selectedLayer != null) {
                if (selectedLayer.frames != null) { //if we've got a hitbox selected
                    inspectingFrameObject = true;
                    propertiesUI.DrawPropertiesWindow("Hitbox Properties", propertiesUI.allBoxProperties, selectedLayer.GetFrameData(selectedFrameIndex).props.Values.ToList());

                    if (selectedLayer.kind == Shape.Point) { //and we're on a point curve layer...
                        float curveEditorHeight = 150;
                        curveUI.DrawCurveEditor(new Rect(margin_, propertiesUI.propWindowRect.y + propertiesUI.propWindowRect.height + margin_, 250, curveEditorHeight));
                    }
                }

            } else {
                inspectingFrameObject = false;
                propertiesUI.DrawPropertiesWindow("Frame Properties", propertiesUI.allFrameProperties, myTarget.propertiesList[selectedFrameIndex].frameProperties);

            }

        }
    }

    //Handle a bunch of miscelaneous code... mostly inputs but also resetting the sprite preview.
    //I'll fix this at some point...
    void HandleInputs() {
        Event e = Event.current; //the event being handled
        mode = (e.alt || e.button == 2) ? EditorMode.Move : EditorMode.Default; //mode selection

        if (mode == EditorMode.Move) {
            shouldRepaint = true; //repaint every frame if we're moving
            spriteUI.viewOffset += e.delta * 0.5f; //offset is centred(?)
        }

        hitboxUI.HandleBoxResizeEnd(e);
        HandleKeyPresses(e);
        HandleMouseEvents(e);
        spriteUI.HandleScrollToZoom(e);

        if (selectedFrameIndex != prevSelectedFrameIndex) {// if we've changed frames in the last frame, update.
            spriteUI.spritePreview = frameTextures[selectedFrameIndex];
        }

        if (shouldRepaint) {
            shouldRepaint = false;
            Repaint();
            spriteUI.UpdateActiveZoomTexture();
        }
    }

    void HandleMouseEvents(Event e) {
        if (e.type == EventType.MouseDown && e.button == 0) { //left mouse click down
            mouseDownPos = new Vector2(e.mousePosition.x, e.mousePosition.y);
            mouseDownTime = Time.time;
        }

        if (e.type == EventType.MouseUp && e.button == 0 && Vector2.Distance(e.mousePosition, mouseDownPos) < 3 && Time.time - mouseDownTime < 0.2f) { //left mouse click up
            mouseClickPos = new Vector2(e.mousePosition.x, e.mousePosition.y);
            consecutiveClicks = (Vector2.Distance(mouseClickPos, prevMouse) == 0) ? consecutiveClicks + 1 : 1; //handle consecutive clicks down to click through layers
            prevMouse = new Vector2(mouseClickPos.x, mouseClickPos.y);
            shouldRepaint = true;

        } else {
            mouseClickPos = new Vector2(-1000, -1000);
            if (Vector2.Distance(prevMouse, e.mousePosition) > 0) consecutiveClicks = 0;
        }

    }



    void HandleKeyPresses(Event e) {
        mode = (e.alt || e.button == 2) ? EditorMode.Move : EditorMode.Default; //mode selection
        if (e.type == EventType.KeyDown) { //key presses...

            shouldRepaint = true; //repaint the canvas if we press a button

            switch (e.keyCode) {
                case KeyCode.RightArrow: //next frame
                    selectedFrameIndex = (selectedFrameIndex + 1) % (myTarget.spriteList.Count);
                    break;

                case KeyCode.LeftArrow: //previous frame
                    if (selectedFrameIndex > 0) selectedFrameIndex--;
                    else selectedFrameIndex = myTarget.spriteList.Count - 1;
                    break;

                case KeyCode.BackQuote:  //Fit
                    spriteUI.zoomSetting = 0;
                    spriteUI.viewOffset.x = 0;
                    spriteUI.viewOffset.y = 0;
                    shouldRepaint = true;
                    break;

                case KeyCode.Alpha1:  //1x
                    spriteUI.zoomSetting = 1;
                    shouldRepaint = true;
                    break;

                case KeyCode.Alpha2:  //2x
                    spriteUI.zoomSetting = 2;
                    shouldRepaint = true;
                    break;

                case KeyCode.Alpha3:  //3x
                    spriteUI.zoomSetting = 3;
                    shouldRepaint = true;
                    break;

                case KeyCode.G:  //show grid
                    gridSetting = !gridSetting;
                    break;

                case KeyCode.H: //show/hide all layers
                    ShowHideToggle();
                    break;

                case KeyCode.Space: //play animation
                    playing = !playing;
                    if (playing) {
                        frameOnPlay = selectedFrameIndex;
                        timeAtPlay = EditorApplication.timeSinceStartup;
                    }
                    break;

                case KeyCode.Escape:  //unselect frame
                    selectedLayer = null;
                    break;

                case KeyCode.S: //save (for those of us with OCD)
                    if (Event.current.modifiers == EventModifiers.Control) {
                        Save();
                    }
                    break;

                case KeyCode.C: //Copy
                    if (Event.current.modifiers == EventModifiers.Control) {
                        Copy();
                    }
                    break;

                case KeyCode.V: //Paste
                    if (Event.current.modifiers == EventModifiers.Control) {
                        Paste();
                    }
                    break;
            }

        }
    }

    public void ShowHideToggle() { //show or hide all groups in the sheet.
        bool foundVisible = false;
        foreach (Layer l in myTarget.layers) {
            if (l.visible) {
                foundVisible = true;
                break;
            }
        }

        Undo.RecordObject(myTarget, "show / hide all layers");
        foreach (Layer l in myTarget.layers) {
            l.visible = !foundVisible; //if we found one on, you're all going off. Otherwise all turn on.
        }
    }


    bool IsALayerSelected() {
        return (selectedLayer != null);
    }

    public bool LayerIsSelected(Layer layer) {
        return (selectedLayer == layer);
    }

    public bool Clicked() {
        return (mouseClickPos.x != -1000 && mouseClickPos.y != -1000);
    }

    //Did we click inside the given rectangle?
    public bool ClickedRect(Rect r) {
        return (Clicked() && r.Contains(Event.current.mousePosition));
    }

    //Did we right-click inside the given rectangle?
    public bool RightClicked(Rect r) {
        Event e = Event.current;
        if (e.type == EventType.ContextClick) {
            Vector2 mousepos = e.mousePosition;

            if (r.Contains(mousepos)) {
                e.Use();
                return true;
            }

        }
        return false;
    }



    //Insert a frame (o = int)
    public void DuplicateFrame(object intObject) {
        int i = (int)intObject;
        Undo.RecordObject(myTarget, "duplicate frame");

        //add the new frame
        myTarget.spriteList.Insert(i, myTarget.spriteList[i]);
        //clone frame properties
        myTarget.propertiesList.Insert(i, new Properties(myTarget.propertiesList[i].frameProperties));

        for (int x = 0; x < myTarget.layers.Count; x++) { //iterate through groups
                                                          //clone the frame object
            Frame prevFrame = myTarget.layers[x].frames[i];
            Frame newFrame = Frame.Clone(prevFrame);
            myTarget.layers[x].Add(newFrame, i + 1);
        }

        selectedFrameIndex = i + 1;
        LoadSprites();
    }

    //Delete a frame (o = int)
    public void DeleteFrame(object intObject) {
        int i = (int)intObject;

        if (i < myTarget.spriteList.Count && i >= 0) {
            Undo.RecordObject(myTarget, "remove frame");
            myTarget.spriteList.Remove(myTarget.spriteList[i]);
            myTarget.propertiesList.Remove(myTarget.propertiesList[i]);

            for (int x = 0; x < myTarget.layers.Count; x++) {//for all groups
                myTarget.layers[x].Remove(i);
            }
        }

        if (selectedFrameIndex >= myTarget.spriteList.Count) {
            selectedFrameIndex = myTarget.spriteList.Count - 1;
        }
        LoadSprites();
    }

    //Delete a layer (o = Layer)
    public void DeleteLayer(object o) {
        Layer layer = (Layer)o;
        toDelete = layer;
    }

    //Delete a layer (public group "toDelete")
    public void DeleteMarkedLayer() {
        if (myTarget.layers.Contains(toDelete)) {
            Undo.RecordObject(myTarget, "remove layer");
            myTarget.layers.Remove(toDelete);
            Save();
        }
    }



    /// <summary>
    /// Return the individual texture for a given index in a sprite matrix
    /// </summary>
    /// <param name="sprite">The source sprite.</param>
    /// <param name="filterMode">The desired filter mode.</param>
    public static Texture2D GetTextureFromSprite(Sprite sprite, FilterMode filterMode) {
        var rect = sprite.rect;
        Texture2D tex = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
        tex.filterMode = filterMode;
        Graphics.CopyTexture(sprite.texture, 0, 0, (int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height, tex, 0, 0, 0, 0);
        tex.Apply(true);
        return tex;
    }
    public Rect FrameToCanvasSpace(Rect rect) {
        float spriteScale = spriteUI.spriteScale;
        rect.x = (rect.x * spriteScale) + spriteUI.screenSpriteOrigin.x + spriteUI.viewOffset.x;
        rect.y = (rect.y * spriteScale) + spriteUI.screenSpriteOrigin.y + spriteUI.viewOffset.y;
        rect.width *= spriteScale;
        rect.height *= spriteScale;
        return rect;
    }

    //DRAG AND DROP FUNCTIONALITY
    public void CheckDragAndDrop() {
        switch (Event.current.type) {

            case EventType.MouseDown:
                // reset the DragAndDrop Data
                DragAndDrop.PrepareStartDrag();
                break;

            case EventType.DragUpdated:

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                break;

            case EventType.DragPerform:

                DragAndDrop.AcceptDrag();
                TryImporting(DragAndDrop.objectReferences);
                break;

            case EventType.MouseDrag:
                //DragAndDrop.StartDrag("Dragging");
                Event.current.Use();
                break;

            case EventType.MouseUp:

                // Clean up, in case MouseDrag never occurred:
                DragAndDrop.PrepareStartDrag();
                break;

            case EventType.DragExited:
                break;
        }
    }


    public void Save() {
        EditorUtility.SetDirty(myTarget);
        AssetDatabase.SaveAssets();
    }

    public void Copy() {
        if (selectedLayer != null) { //layer
            FrameData frameToCopy = new FrameData();
            frameToCopy = selectedLayer.frameDataById[selectedLayer.frames[selectedFrameIndex].dataId];
            copyData = FrameData.Clone(frameToCopy);

        } else { //frame?

        }
    }

    public void Paste() {
        if (selectedLayer != null) {//layer
            if (copyData is FrameData frameToPaste) { //if our copydata is a hitbox
                Frame currentFrame = selectedLayer.frames[selectedFrameIndex];
                Undo.RecordObject(myTarget, "pasted data");

                if (currentFrame.kind == Frame.Kind.KeyFrame) {
                    selectedLayer.frameDataById.Remove(currentFrame.dataId);
                }

                selectedLayer.frameDataById.Add(frameToPaste.guid, frameToPaste);
                selectedLayer.frames[selectedFrameIndex].kind = Frame.Kind.KeyFrame;
                selectedLayer.frames[selectedFrameIndex].dataId = frameToPaste.guid;
                selectedLayer.ResyncFrames(selectedFrameIndex);
            }
        }
    }

    private void TryImporting(Object[] objects) { //import sprites "dropped" onto the editor
        Debug.Log("trying import");
        if (myTarget != null) {
            Debug.Log("importing");

            for (int i = 0; i < objects.Length; i++) {
                if (objects[i] is Sprite s) {
                    ImportSpriteToExistingSheet(s);
                }
            }

        } else {
            for (int i = 0; i < objects.Length; i++) {
                if (objects[i] is Texture2D sprite) {
                    CreateNewSheetWithSprite(sprite);
                } else {
                    Debug.Log(objects[i].GetType());
                }
            }
        }
    }

    private void ImportSpriteToExistingSheet(Sprite s) {
        if (myTarget.spriteList == null) {
            myTarget.spriteList = new List<Sprite>();
            myTarget.propertiesList = new List<Properties>();
            myTarget.layers = new List<Layer>();
        } else {
            foreach (Layer l in myTarget.layers) {
                l.frames.Add(new Frame());
            }
        }

        myTarget.spriteList.Add((Sprite)s);
        myTarget.propertiesList.Add(new Properties());

        LoadSprites();
        InitializeRetroSheet();


    }

    private void CreateNewSheetWithSprite(Texture2D spriteTexture) {
        Sheet newSheet = CreateInstance<Sheet>();
        newSheet.spriteList = new List<Sprite> { };
        newSheet.propertiesList = new List<Properties> { new Properties() };
        newSheet.layers = new List<Layer>();

        //slice the sprite according to size and pivot

        Sprite[] sprites = RetroEditor.Utilities.SliceTexture(spriteTexture, importSize, importPivot);
        foreach (Sprite s in sprites) {
            newSheet.spriteList.Add(s);
        }

        string path = AssetDatabase.GetAssetPath(spriteTexture);
        string directory = System.IO.Path.GetDirectoryName(path);
        string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{directory}/{fileName}.asset");

        AssetDatabase.CreateAsset(newSheet, assetPath);
        EditorUtility.SetDirty(newSheet);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public void CheckNewSelection() {
        if (selectedLayer != null) {
            if (prevSelectedLayer != selectedLayer ||
                prevSelectedFrameIndex != selectedFrameIndex ||
                selectedLayer.GetFrameData(prevSelectedFrameIndex) != selectedLayer.GetFrameData(selectedFrameIndex)
            ) {
                newSelection.Invoke();
            }
        }
        prevSelectedLayer = selectedLayer;
        prevSelectedFrameIndex = selectedFrameIndex;
    }

    public void TargetPreferences() {
        EditorGUIUtility.PingObject(preferences);
        Selection.activeObject = preferences;
    }

    public void NewRetroSheet() {
        Sheet asset = CreateInstance<Sheet>();
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
          asset.GetInstanceID(),
          CreateInstance<EndRetroNameEdit>(),
          string.Format("{0}.asset", "RetroSheet"),
          AssetPreview.GetMiniThumbnail(asset),
          null);

    }

    public void UndoCallback() {
        Repaint();
    }

    public void AboutRetrobox() {
        Debug.Log("Retrobox " + myTarget.GetVersion() + ".\nCreated by Adam Younis @ Uppon Hill.\nContact: support@upponhill.com");
    }
}

internal class EndRetroNameEdit : EndNameEditAction {
    #region implemented abstract members of EndNameEditAction
    public override void Action(int instanceId, string pathName, string resourceFile) {
        AssetDatabase.CreateAsset(EditorUtility.InstanceIDToObject(instanceId), AssetDatabase.GenerateUniqueAssetPath(pathName));
    }
    #endregion
}