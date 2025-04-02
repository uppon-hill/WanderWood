using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Retro {
    public class PropertyModifier {
        public string modName;
        public string modValue;
        public delegate BoxProperty Modify(BoxProperty property);
        public Dictionary<string, Modify> modifiers;
        public PropertyModifier(string modName_, string modValue_, Dictionary<string, Modify> modifiers_) {
            modName = modName_;
            modValue = modValue_;
            modifiers = modifiers_;

        }

    }
}