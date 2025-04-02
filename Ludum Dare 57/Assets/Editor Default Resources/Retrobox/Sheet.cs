using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
namespace Retro {
    public enum Shape { Box, Point }

    [System.Serializable]
    public class Sheet : ScriptableObject {
        [SerializeField]
        private string version = "1.0";
        public int count => spriteList.Count;
        public List<Sprite> spriteList;
        public List<Properties> propertiesList;
        public List<Group> groups;
        public List<Layer> layers;
        public string GetVersion() {
            return version;
        }

        public void SetVersion(System.Object o, string v) {
            if (o.GetType().Equals("RetroboxEditor")) version = v;
        }
    }


    [System.Serializable]
    public class Group {
        //group variables
        public string myBoxType;
        public Shape layerKind;
        public List<Layer> layers;

        //POINT VARIABLES

        //HITBOX VARIABLES
        public enum CollisionType { Collider, Trigger, NoCollide }
        public CollisionType collisionType;

        //editor variables
        public bool expanded;
        public bool visible;

        public Group(string s, Shape layerKind_) {
            layerKind = layerKind_;
            myBoxType = s;
            expanded = true;
            visible = true;
            collisionType = CollisionType.Trigger;
            layers = new List<Layer>();
        }
    }

    [System.Serializable]
    public class FrameDataById : SerializableDictionary<string, FrameData> { }

    [System.Serializable]
    public class Layer {
        public string myBoxType;

        public Shape kind;
        public List<Frame> frames;
        public FrameDataById frameDataById;

        //POINT VARIABLES

        //HITBOX VARIABLES
        public enum CollisionType { Collider, Trigger, NoCollide }
        public CollisionType collisionType;

        //editor variables
        public bool visible;


        public Layer(string s, Shape kind_ = Shape.Box) {
            visible = true;
            myBoxType = s;
            kind = kind_;
            frames = new List<Frame>();
            frameDataById = new FrameDataById();
        }

        public Curve CurveFromKeyFrames(int startFrame) {
            Curve c = null;
            FrameData a = null;
            FrameData b = null;

            if (frames[startFrame].IsKeyFrame()) {
                a = GetFrameData(startFrame);
                Frame n = GetNextKeyFrame(startFrame);
                if (n != null) {
                    b = GetFrameData(frames.IndexOf(n));
                    return c = new Curve(a.position, a.forwardHandle, b.backHandle, b.position);
                }
            }

            return c;
        }

        public FrameData GetFrameData(int i) {
            if (!String.IsNullOrEmpty(frames[i].dataId) &&
                frameDataById.ContainsKey(frames[i].dataId)) {

                return frameDataById[frames[i].dataId];
            } else {
                return FrameData.emptyData;
            }
        }

        public void Add(Frame f, int i = -1) {
            if (i == -1) {
                if (frames.Count > 0) {
                    i = frames.Count;
                } else {
                    i = 0;
                }
            }

            //TODO: Consider the implications of removing this
            AddKeyFrameData(f);
            frames.Insert(i, f);
            ResyncFrames(i);
        }

        public void Remove(int i) {
            Frame f = frames[i];
            RemoveKeyFrameData(f);
            frames.Remove(f);
            ResyncFrames(i);
        }

        public void ResyncFrames(int index) {

            //do we actually have a frame from which to copy?
            if (index < frames.Count - 1) {
                string dataId = index >= 0 ? frames[index].dataId : System.Guid.Empty.ToString();

                for (int i = index + 1; i < frames.Count; i++) {
                    //as long as the next frame is not a key or empty frame, 

                    if (frames[i].kind == Frame.Kind.CopyFrame) {
                        //update the frame references of all subsequence frames to the new duplicate
                        frames[i].dataId = dataId;
                    } else {
                        return;
                    }
                }
            }
            RemoveEmptyFrameData();
        }
        public void RemoveEmptyFrameData() {
            List<string> frameDataGUIDs = new List<string>();
            List<string> dataGUIDsToRemove = new List<string>();
            foreach (Frame f in frames) {
                if (!frameDataGUIDs.Contains(f.dataId)) {
                    frameDataGUIDs.Add(f.dataId);
                }
            }
            foreach (string s in frameDataById.Keys) {
                if (!frameDataGUIDs.Contains(s)) {
                    dataGUIDsToRemove.Add(s);
                }
            }
            foreach (string s in dataGUIDsToRemove) {
                frameDataById.Remove(s);
            }
        }

        public void AddKeyFrameData(Frame f) {
            //  if it's a keyframe, then create a new FrameData object, duplicating the previous frame, and tie it to the new Keyframe
            /*
              if (f.kind == Frame.Kind.KeyFrame) {
                  FrameData d;

                  if (f.data == null) {
                      d = new FrameData(new Vector2(16, 16), new Vector2(16, 16));
                  } else {
                      d = FrameData.Clone(f.data);
                  }
                  d.keyFrameId = f.guid;
                  f.dataId = d.guid;
                  f.parent.frameDataById.Add(d.guid, d);
              }
              */
        }

        public void RemoveKeyFrameData(Frame f) {
            if (f.kind == Frame.Kind.KeyFrame) {
                if (f.dataId != null && frameDataById.ContainsKey(f.dataId) && frameDataById[f.dataId].keyFrameId == f.guid) {
                    frameDataById.Remove(f.dataId);
                }
            }
        }


        public Frame GetCurrentKeyFrameOrPrevious(int startIndex) {
            if (frames[startIndex].IsKeyFrame()) {
                return frames[startIndex];
            } else return GetPreviousKeyFrame(startIndex);
        }

        public Frame GetPreviousKeyFrame(int startIndex) {
            if (startIndex > 0) {
                for (int i = startIndex - 1; i >= 0; i--) {
                    if (frames[i].kind == Frame.Kind.KeyFrame) {
                        return frames[i];
                    }
                }
            }
            return null;
        }

        public Frame GetCurrentKeyFrameOrNext(int startIndex) {
            if (frames[startIndex].IsKeyFrame()) {
                return frames[startIndex];
            } else return GetNextKeyFrame(startIndex);
        }

        public Frame GetNextKeyFrame(int startIndex) {
            for (int i = startIndex + 1; i < frames.Count; i++) {
                if (frames[i].kind == Frame.Kind.KeyFrame) {
                    return frames[i];
                }
            }
            return null;
        }

    }

    [System.Serializable]
    public class Frame {

        public string guid;

        public enum Kind { KeyFrame, CopyFrame, Empty }
        public Kind kind;
        public string dataId;
        //public FrameData data => isValid ? parent.frameDataById[dataId] : FrameData.emptyData;

        public Frame(Kind kind_ = Kind.CopyFrame, string dataId_ = default) {
            guid = Guid.NewGuid().ToString();
            dataId = dataId_;
            kind = kind_;
        }

        public bool IsKeyFrame() {
            return kind == Kind.KeyFrame;
        }

        public bool IsCopyFrame() {
            return kind == Kind.CopyFrame;
        }

        public bool IsEmpty() {
            return dataId == null ||
            dataId.Equals(System.Guid.Empty.ToString()) ||
            String.IsNullOrEmpty(dataId);
        }

        public static Frame Clone(Frame f) {
            return new Frame(f.kind, f.dataId);
        }
    }

    [Serializable]
    public class FrameData {
        public Rect rect {
            get { return new Rect(position, size); }
            set {
                position = value.position;
                size = value.size;
            }
        }

        public static FrameData emptyData;

        public readonly string guid;
        public string keyFrameId;
        public Vector2 position;
        public Vector2 size;

        public Vector2 forwardHandle;
        public Vector2 backHandle;

        public BoxProps props;

        static FrameData() {
            emptyData = new FrameData();
        }

        public FrameData() {
            guid = Guid.NewGuid().ToString();
            position = new Vector2(16, 16);
            size = new Vector2(0, 0);

            forwardHandle = position + Vector2.down * 16f;
            backHandle = position + Vector2.down * 16f;

            props = new BoxProps();
        }

        public FrameData(Vector2 position_, Vector2 size_) {
            guid = Guid.NewGuid().ToString();
            position = position_;
            size = size_;
            forwardHandle = position + Vector2.down * 16f;
            backHandle = position + Vector2.down * 16f;

            props = new BoxProps();
        }

        public static FrameData Clone(FrameData d) {
            FrameData f = new FrameData(d.position, d.size);
            f.forwardHandle = d.forwardHandle;
            f.backHandle = d.backHandle;
            f.props = new BoxProps();
            foreach (string k in d.props.Keys) {
                f.props.Add(k, BoxProperty.Clone(d.props[k]));
            }
            return f;
        }

        public static FrameData Interpolate(FrameData a, FrameData b) {
            FrameData f = new FrameData();
            f.position = Vector2.Lerp(a.position, b.position, 0.5f);
            f.size = Vector2.Lerp(a.size, b.size, 0.5f);


            Vector2 localAForward = a.forwardHandle - a.position;
            Vector2 localABack = a.backHandle - a.position;

            Vector2 localBForward = b.forwardHandle - b.position;
            Vector2 localBBack = b.backHandle - b.position;

            f.forwardHandle = Vector2.Lerp(localAForward, -localBBack, 0.5f) + f.position;
            f.backHandle = Vector2.Lerp(-localAForward, localBBack, 0.5f) + f.position;

            f.props = new BoxProps();

            foreach (string k in a.props.Keys) {
                f.props.Add(k, BoxProperty.Clone(a.props[k]));
            }
            return f;
        }

    }

    [System.Serializable]
    public class BoxProperty {
        public string name;
        public PDataType dataType;
        public int intVal;
        public float floatVal;
        public bool boolVal;
        public string stringVal;
        public Vector2 vectorVal;
        public Curve curveVal;
        // public FMODUnity.EventReference fmodEventVal;

        public BoxProperty(string name_, PDataType dataType_) {
            name = name_;
            dataType = dataType_;
            intVal = 0;
            floatVal = 0;
            boolVal = false;
            stringVal = "";
            vectorVal = new Vector2();
            curveVal = Curve.Linear;
            //  fmodEventVal = default;

        }

        public static BoxProperty Clone(BoxProperty b) {
            BoxProperty c = new BoxProperty(b.name, b.dataType);
            c.intVal = b.intVal;
            c.floatVal = b.floatVal;
            c.boolVal = b.boolVal;
            c.stringVal = b.stringVal;
            c.vectorVal = b.vectorVal;
            c.curveVal = Curve.Clone(b.curveVal);
            // c.fmodEventVal = b.fmodEventVal;
            return c;
        }

        public void SetPropertyValues(BoxProperty p) {
            dataType = p.dataType;
            intVal = p.intVal;
            boolVal = p.boolVal;
            floatVal = p.floatVal;
            stringVal = p.stringVal;
            vectorVal = p.vectorVal;
            curveVal = Curve.Clone(p.curveVal);
            //    fmodEventVal = p.fmodEventVal;
        }
    }

    [Serializable]
    public class BoxProps : SerializableDictionary<string, BoxProperty> { }

    public enum PDataType { Bool, String, Int, Float, Vector2, Curve, FMODEvent }

    [System.Serializable]
    public class Properties {

        public List<BoxProperty> frameProperties;

        public Properties(List<BoxProperty> properties) {
            frameProperties = new List<BoxProperty>();

            foreach (BoxProperty b in properties) {
                frameProperties.Add(BoxProperty.Clone(b));
            }
        }

        public Properties() {
            frameProperties = new List<BoxProperty>();
        }

    }


}