using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace Retro {
    [System.Serializable]
    public class RetroboxPrefs : ScriptableObject {
        List<BoxData> boxTypes; //persistent list of hitbox types (set by user) (TO BE DELETED)

        public BoxDataDictionary boxDictionary;//persistent list of hitbox types (set by user)
        public BoxDataDictionary pointDictionary;//persistent list of hitbox types (set by user)

        public BoxPropertiesDictionary propsDictionary;
        public BoxPropertiesDictionary framePropsDictionary;

        public int cachedZoomSetting;  //persistent zoom setting for the editor
        public bool cachedGridSetting; //persistent grid on/off setting for the editor

        public BoxData Get(string s) {//return any boxtypes with a matching name
            try {
                return boxDictionary[s];
            } catch (System.Exception e) {
                Debug.Log(e);
                return null;
            }

        }
        public BoxDataDictionary GetShapeDictionary(Retro.Shape s) {
            switch (s) {
                case Retro.Shape.Box:
                    return boxDictionary;
                case Retro.Shape.Point:
                    return pointDictionary;
            }
            return null;
        }
    }

    [System.Serializable]
    public class BoxData {
        public Color colour; //colour
        public string boxTypeName; //name //TODO: Rename GroupIdentifier
        public Shape shape;

        //For Points

        //For Hitboxes
        public int physicsLayer; //layer
        public bool isRounded; //rounded collider
        public bool isAggressor; //rounded collider

        public PhysicsMaterial2D material; //physics material

        public BoxData() {
            colour = new Color(0, 0, 0, 1);
        }

        public BoxData(Color c, string n, int l, bool r, Shape shape_) {
            shape = shape_;
            colour = c;
            boxTypeName = n;
            physicsLayer = l;
            isRounded = r;
        }
    }

}
[Serializable]
public class BoxDataDictionary : SerializableDictionary<string, Retro.BoxData> { }

[Serializable]
public class BoxPropertiesDictionary : SerializableDictionary<string, Retro.PDataType> { }


