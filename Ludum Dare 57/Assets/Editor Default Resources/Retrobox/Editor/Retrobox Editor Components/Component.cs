using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Retro;
namespace RetroEditor {
    public class Component {
        protected RetroboxEditor e;
        protected Sheet sheet => e.myTarget;
    }
}