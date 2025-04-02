using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
namespace Retro {
    public class HitboxManager {
        Dictionary<Collider2D, Frame> activeFrameObjects; //a list of hitboxes active this frame
        Dictionary<Collider2D, Frame> oldFrames;//a list of hitboxes active last frame
        Dictionary<Layer, ColliderInfo> layerColInfo; //collider info for all active hitboxes
        Dictionary<Collider2D, List<RetroAnimator>> bannedColliders;
        public Dictionary<Layer, CurveInstance> curveInstancesByLayer = new Dictionary<Layer, CurveInstance>();
        public UnityEvent<Layer> onCurveEnded = new UnityEvent<Layer>();


        public BoxCollisionEvent collisionEvent;
        public HitConfirmEvent hitConfirmEvent;
        public RetroAnimator animator;
        PropertyModifier modifier;

        Sheet mySheet => animator.mySheet;
        Transform transform => animator.spriteRenderer != null ? animator.spriteRenderer.transform : animator.transform;

        public UnityEvent<CurveInstance, Vector2> effectCurveEvent = new UnityEvent<CurveInstance, Vector2>();

        public HitboxManager(RetroAnimator animator_) {
            animator = animator_;
            activeFrameObjects = new Dictionary<Collider2D, Frame>();
            layerColInfo = new Dictionary<Layer, ColliderInfo>();
            bannedColliders = new Dictionary<Collider2D, List<RetroAnimator>>();
            collisionEvent = new BoxCollisionEvent();
            hitConfirmEvent = new HitConfirmEvent();
            curveInstancesByLayer = new Dictionary<Layer, CurveInstance>();
        }

        public void Initialise() { //Setup a new animation to be played.


            oldFrames = new Dictionary<Collider2D, Frame>();
            if (activeFrameObjects != null) foreach (KeyValuePair<Collider2D, Frame> b in activeFrameObjects) oldFrames.Add(b.Key, b.Value); //make a copy of the list of colliders..
            activeFrameObjects = new Dictionary<Collider2D, Frame>();
            layerColInfo = new Dictionary<Layer, ColliderInfo>();

            if (curveInstancesByLayer != null) {
                foreach (Layer layer in curveInstancesByLayer.Keys.ToList()) {
                    onCurveEnded.Invoke(layer);
                    curveInstancesByLayer.Remove(layer);
                }
            }
            curveInstancesByLayer = new Dictionary<Layer, CurveInstance>();

            //Setup the dictionary
            Dictionary<string, List<Layer>> layersByType = new Dictionary<string, List<Layer>>();

            foreach (Layer l in mySheet.layers) {
                if (!layersByType.ContainsKey(l.myBoxType)) {
                    layersByType.Add(l.myBoxType, new List<Layer>() { l });
                } else {
                    layersByType[l.myBoxType].Add(l);
                }
            }

            //go through the dictionary filling in the layer kinds
            foreach (string s in layersByType.Keys) {
                List<Layer> layers = layersByType[s];

                Layer layer = layers[0];
                if (layer.kind == Shape.Box && layer.collisionType != Layer.CollisionType.NoCollide) {

                    GameObject layerObject = FindLayerGameObject(layer);

                    if (layerObject != null) {//Refresh every boxcollider in our existing gameobject with each of our new hitboxes.

                        int layersInstantiated = 0;
                        int total = layerObject.transform.childCount;//the number of children before we do any modifications to the collections

                        for (int childIndex = 0; childIndex < total; childIndex++) {

                            ColliderInfo info = layerObject.transform.GetChild(childIndex).GetComponent<ColliderInfo>();

                            if (layersInstantiated < layers.Count) {//if we still have more to instantiate
                                layer = layers[layersInstantiated];

                                if (info == null) {
                                    AddNewHitbox(layerObject, layer, layerObject.transform.GetChild(layersInstantiated).gameObject);

                                } else {
                                    InstantiateHitbox(info, layer);
                                    oldFrames.Remove(info.col);
                                }
                                layersInstantiated++;

                            } else { //otherwise, if we had some leftover, disable the remaining old ones.
                                info.col.enabled = false;
                            }
                        }

                        //if we have any new hitboxes left...
                        if (layersInstantiated < layers.Count) {
                            for (int n = layersInstantiated; n < layers.Count; n++) { //add new colliders on the object.
                                AddNewHitbox(layerObject, layers[n]);
                            }
                        }

                    } else { //we don't have anything of this kind in the scene yet...
                        GameObject newObject = CreateHitboxGroupInScene(layer);
                        for (int j = 0; j < layers.Count; j++) {
                            AddNewHitbox(newObject, layers[j]);
                        }
                    }
                }

            }

            //disable every collider from the previous frame that wasn't involved in anything we just did.
            foreach (KeyValuePair<Collider2D, Frame> b in oldFrames) {
                b.Key.enabled = false;
                ClearBannedAnimators(b.Key);
            }
        }


        GameObject FindLayerGameObject(Retro.Layer layer) {
            for (int k = 0; k < transform.childCount; k++) {
                Transform child = transform.GetChild(k);
                if (child.name.Equals(layer.myBoxType)) {//if i find one...
                    return child.gameObject;
                }
            }
            return null;
        }

        GameObject CreateHitboxGroupInScene(Layer layer) {
            //setup a new group object child under the gameobject
            GameObject newObject = new GameObject();
            newObject.name = layer.myBoxType;
            newObject.transform.position = new Vector3(0, 0, 0);
            newObject.transform.SetParent(transform, false);
            newObject.layer = animator.preferences.boxDictionary[layer.myBoxType].physicsLayer;
            return newObject;
        }


        GameObject NewContainer(GameObject parent, Layer layer) {
            GameObject newObject = new GameObject();
            newObject.name = layer.myBoxType;
            newObject.transform.position = new Vector3(0, 0, 0);
            newObject.transform.SetParent(parent.transform, false);
            newObject.layer = animator.preferences.boxDictionary[layer.myBoxType].physicsLayer;
            return newObject;
        }


        ColliderInfo AddNewHitbox(GameObject parent, Layer layer, GameObject container = null) {
            if (container == null) {
                container = NewContainer(parent, layer);
            }

            //declare new objects
            ColliderInfo info = container.AddComponent<ColliderInfo>();
            BoxCollider2D c = container.AddComponent<BoxCollider2D>();
            if (container.GetComponent<PlatformEffector2D>() != null) {
                c.usedByEffector = true;
            }
            Frame f = layer.frames[0];
            FrameData data = layer.GetFrameData(0);
            info.Setup(mySheet, mySheet.layers.IndexOf(layer), GetModifiedProperties(data.props), c, animator); //add the new collider to our ColliderInfo component
            InstantiateHitbox(info, layer);
            c.enabled = f.IsKeyFrame();
            return info;
        }

        void InstantiateHitbox(ColliderInfo info, Layer layer) {
            //declare new objects
            FrameData frameData = layer.GetFrameData(0);
            Frame frame = layer.frames[0];
            //don't need to setup a new Collider2D because there's one in the existingCollider...

            bool rounded = animator.preferences.boxDictionary[layer.myBoxType].isRounded;

            if (frameData.props.ContainsKey("forceRounded")) {
                rounded = frameData.props["forceRounded"].boolVal;
            }

            info.Setup(mySheet, mySheet.layers.IndexOf(layer));
            //setup collider properties
            SetColliderProperties(info.col,
                frameData.rect,
                mySheet.spriteList[0],
                rounded,
                (layer.collisionType == Layer.CollisionType.Trigger),
                animator.preferences.boxDictionary[layer.myBoxType].material
                );

            activeFrameObjects.Add(info.col, frame);
            layerColInfo.Add(layer, info);
        }


        public void SetFrame(int newFrame) {
            //we actually need to process point groups first if they're going to offset the box positions
            List<Layer> pointGroups = mySheet.layers.Where(x => x.kind == Shape.Point).ToList();
            List<Layer> boxGroups = mySheet.layers.Where(x => x.kind == Shape.Box).ToList();
            List<Layer> sortedGroups = new List<Layer>(pointGroups);
            sortedGroups.AddRange(boxGroups);

            RemoveOldCurveOffsets();

            for (int i = 0; i < sortedGroups.Count; i++) {
                Layer layer = sortedGroups[i];

                Frame frame = layer.frames[newFrame];

                switch (layer.kind) {
                    case Shape.Box:
                        if (layer.collisionType != Layer.CollisionType.NoCollide) {
                            SetBoxFrame(layer, newFrame);
                        } else {
                            //do something with this, because clearly, we want to use it for something other than being a collider.
                        }
                        break;

                    case Shape.Point:
                        SetPointFrame(layer, newFrame);
                        break;

                }
            }
        }

        public void RemoveOldCurveOffsets() {
            //remove all of the old curves that might be hanging around
            foreach (Layer l in curveInstancesByLayer.Keys) {
                CurveInstance c = curveInstancesByLayer[l];

                if (l.kind == Shape.Point) {
                    if (l.frames[animator.frame].IsEmpty()) {
                        animator.localOffsetHash.RemoveEffector(c.GetID());
                    }
                }
            }
        }

        public void UpdateFixedCurves() {
            UpdateCurves(true);
        }

        public void UpdateCurves(bool fixedUpdate = false) {
            if (curveInstancesByLayer.Count > 0 && animator.isPlaying) {
                foreach (Layer layer in curveInstancesByLayer.Keys.ToList()) {

                    CurveInstance c = curveInstancesByLayer[layer];

                    c.Evaluate();

                    //are we sampling between the start and end keyframes of the curve?
                    bool validTime = c.t >= 0 && c.t <= 1;

                    float clampedTime = Mathf.Clamp01(c.t);
                    bool lastFrame = c.t > 1 || c.endIndex == animator.frame;

                    if (validTime) {
                        //get the time of the first frame, and the time of the last frame
                        curveInstancesByLayer[layer].Process(layer, c.t, Time.deltaTime);
                    }
                }

                FramePropertyHandler.ApplyCurveProperties(animator, fixedUpdate);
            }
        }


        void SetPointFrame(Layer layer, int index) {
            Frame startFrame = layer.GetCurrentKeyFrameOrPrevious(index);
            Frame nextKeyFrame = layer.GetNextKeyFrame(index);


            //TODO: Redefine a curve in Retrobox, these tests are weird
            bool hasTwoKeyframes = (startFrame != null && nextKeyFrame != null);
            bool isLastFrame = (startFrame == layer.frames[layer.frames.Count - 1]);
            bool isCurve = hasTwoKeyframes /*|| isLastFrame*/;

            bool hasCurveAlready = curveInstancesByLayer.ContainsKey(layer);

            bool newCurveStartsNow = (startFrame == layer.frames[index]);

            if (isCurve) { //there is a curve

                if (newCurveStartsNow) { // it starts on this frame
                    Vector2 oldStartOffset = Vector2.zero;
                    if (hasCurveAlready) {//if there is an old curve that's expired, let's remove it.
                        onCurveEnded.Invoke(layer);

                        curveInstancesByLayer.Remove(layer);
                    }

                    int startIndex = layer.frames.IndexOf(startFrame);
                    int endindex = layer.frames.IndexOf(nextKeyFrame);

                    if (endindex != -1) {
                        CurveInstance c = new CurveInstance(animator, animator.mySheet, layer, startIndex, endindex, animator.transform.position);
                        curveInstancesByLayer.Add(layer, c);
                    }
                }

                //curveInstancesByLayer[layer].ProcessByFrame(layer, index);
                //FramePropertyHandler.ApplyCurveProperties(animator, true);
                //FramePropertyHandler.ApplyCurveProperties(animator, false);
            } else {
                onCurveEnded.Invoke(layer);
                curveInstancesByLayer.Remove(layer);
            }
        }


        void SetBoxFrame(Layer layer, int frameIndex) {
            ColliderInfo info = layerColInfo[layer];
            Frame frame = layer.frames[frameIndex];
            FrameData data = layer.GetFrameData(frameIndex);

            bool rounded = false;

            if (info.col is BoxCollider2D box) {
                rounded = box.edgeRadius > 0;
                if (data.props.ContainsKey("forceRounded")) {
                    rounded = data.props["forceRounded"].boolVal;
                }
            }
            //update our collider in the scene (feed "istrigger" back to itself, no change)
            SetColliderProperties(
                info.col,
                data.rect,
                mySheet.spriteList[layer.frames.IndexOf(frame)],
                rounded,
                info.col.isTrigger,
                animator.preferences.boxDictionary[layer.myBoxType].material
            );

            //update the current "hitbox" object in the dictionary
            activeFrameObjects[info.col] = frame;

            //unban any keyframes, enable them
            if (frame.IsKeyFrame()) {
                info.SetFrame(animator.frame, GetModifiedProperties(data.props));//update our "ColliderInfo" object in the scene
                info.col.enabled = true;
                ClearBannedAnimators(info.col);
            } else {
                info.SetFrame(animator.frame);
            }

            //disable empty frames
            if (frame.kind == Frame.Kind.Empty) {
                layerColInfo[layer].col.enabled = false;
            }
        }

        public void ClearFrame() { //update the colliders / triggers to the given frame
            foreach (KeyValuePair<Collider2D, Frame> b in activeFrameObjects) {
                b.Key.enabled = false;
                ClearBannedAnimators(b.Key);
            }
        }

        public void SetColliderProperties(Collider2D col, Rect r, Sprite s, bool rounded, bool trigger, PhysicsMaterial2D material) {
            float radius = 0.02f;

            if (col is BoxCollider2D box) {
                if (rounded) {
                    box.edgeRadius = radius;
                    r = RoundHitbox(r, radius);
                } else {
                    box.edgeRadius = 0;
                }
            }

            Rect mappedRect = MapBoxRectToTransform(r, s);

            if (col is BoxCollider2D box2) {
                box2.size = mappedRect.size;
            }

            col.offset = mappedRect.min;
            col.isTrigger = trigger;
            col.sharedMaterial = material;
        }
        public void SetModifier(PropertyModifier p) {
            modifier = p;
        }

        public BoxProps GetModifiedProperties(BoxProps props) { //take some boxProperties, apply modifier functions to them

            BoxProps boxProps = new BoxProps();

            foreach (string s in props.Keys) {
                boxProps.Add(s, BoxProperty.Clone(props[s]));
            }
            if (modifier != null) {
                if (boxProps.ContainsKey(modifier.modName)) {
                    if (boxProps[modifier.modName].stringVal.Equals(modifier.modValue)) {
                        foreach (string s in modifier.modifiers.Keys) {
                            BoxProperty p;
                            if (boxProps.ContainsKey(s)) { //if we already have a property with that name
                                p = boxProps[s];
                                p.SetPropertyValues(modifier.modifiers[p.name](p));

                            } else { //otherwise make a new property
                                p = new BoxProperty(s, PDataType.Bool);
                                p.SetPropertyValues(modifier.modifiers[p.name](p));
                                boxProps.Add(s, p); //add it to props

                            }
                        }
                    }
                }
            }
            return boxProps;

        }
        public void BanAnimator(Collider2D collider, RetroAnimator otherAnimator) {
            if (bannedColliders.ContainsKey(collider)) { //if the hitbox has been banned before
                if (!bannedColliders[collider].Contains(otherAnimator)) { //if we haven't already got the record of the animator(double checking)
                    bannedColliders[collider].Add(otherAnimator); //add the animator to the banned list for this hitbox
                }
            } else { //otherwise, add a new record for this hitbox, and put the other animator in as the first item in the list.
                bannedColliders.Add(collider, new List<RetroAnimator>() { otherAnimator });
            }
        }

        public void ClearBannedAnimators(Collider2D collider) {
            if (bannedColliders.ContainsKey(collider)) {
                bannedColliders.Remove(collider);
            }
        }

        public bool IsBanned(Collider2D collider, RetroAnimator animator) {
            if (bannedColliders.ContainsKey(collider)) { //if the hitbox has been banned before
                if (bannedColliders[collider].Contains(animator)) { //if we haven't already got the record of the animator(double checking)
                    return true;
                }
            }
            return false;
        }
        public Dictionary<Collider2D, List<RetroAnimator>> GetBannedColliders() {
            return bannedColliders;
        }

        public void SetLocalOffset(Vector2 offset) {

            foreach (ColliderInfo c in layerColInfo.Values) {
                Sheet s = c.GetSheet();
                Layer layer = c.GetRetroLayer();
                int frame = c.GetFrame();

                FrameData data = layer.GetFrameData(frame);
                SetColliderProperties(
                   c.col,
                   data.rect,
                   mySheet.spriteList[frame],
                   c.col is BoxCollider2D box && box.edgeRadius > 0,
                   c.col.isTrigger,
                   animator.preferences.boxDictionary[layer.myBoxType].material
                   );

            }
        }

        public static bool HasColliderOfType(string name, Sheet s, int frame) {
            foreach (Layer l in s.layers) {
                if (l.myBoxType == name) {
                    bool followingHitbox = false;
                    for (int i = 0; i < l.frames.Count; i++) {
                        if (l.frames[i].kind == Frame.Kind.KeyFrame) {
                            followingHitbox = true;
                        } else if (l.frames[i].kind == Frame.Kind.Empty) {
                            followingHitbox = false;
                        }

                        if (i == frame && followingHitbox) {
                            return true;
                        }

                    }

                }
            }
            return false;
        }

        Rect RoundHitbox(Rect r, float radius) {
            radius *= 100;
            r.x += radius;
            r.y += radius;
            r.width -= radius * 2;
            r.height -= radius * 2;
            return r;
        }

        public static Rect MapBoxRectToTransform(Rect r, Sprite s) {
            Vector2 offset = new Vector2((r.x + r.width * 0.5f - s.pivot.x) / s.pixelsPerUnit, (s.rect.height - r.y - r.height * 0.5f - s.pivot.y) / s.pixelsPerUnit);
            Vector2 size = new Vector2(r.width / s.pixelsPerUnit, r.height / s.pixelsPerUnit);
            return new Rect(offset, size);
        }

        public Frame.Kind GetColliderType(Collider2D col) {
            return activeFrameObjects[(Collider2D)col].kind;
        }

        public static BoxProps GetColliderProperties(Collider2D col) {
            foreach (ColliderInfo info in col.GetComponents<ColliderInfo>()) {
                if (info.col == col) {
                    return info.GetProperties();
                }
            }
            return null;
        }

        public static string GetColliderBoxType(Collider2D col) {
            foreach (ColliderInfo info in col.GetComponents<ColliderInfo>()) {
                if (info.col == col) {
                    return info.GetBoxType();
                }
            }
            return null;
        }

    }
}
