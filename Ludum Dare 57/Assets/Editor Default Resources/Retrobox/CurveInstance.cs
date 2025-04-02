using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
namespace Retro {
    public class CurveInstance {

        //Editor Time Variables
        public RetroAnimator animator;
        public Sheet sheet;
        public Layer layer;
        public FrameData startFrame;
        public int startIndex;
        public int endIndex;
        public int durationInFrames => endIndex - startIndex;
        public float duration => ((float)durationInFrames) / animator.frameRate;
        public Vector3 startWorldPos;

        float startTime;

        //Runtime variables
        public float t { get; private set; }
        //dictionary of <string, vector2> for each of the curve Properties to be evaluated...?
        public Dictionary<string, Vector2> properties;

        public CurveInstance(RetroAnimator animator_, Sheet sheet_, Layer layer_, int startIndex_, int endIndex_, Vector3 startWorldPos_) {
            animator = animator_;
            startTime = animator.time;
            sheet = sheet_;
            layer = layer_;
            startIndex = startIndex_;
            startFrame = layer.GetFrameData(startIndex_);
            endIndex = endIndex_;
            startWorldPos = startWorldPos_;
            properties = new Dictionary<string, Vector2>();

            foreach (string s in startFrame.props.Keys) {
                if (startFrame.props[s].curveVal != null) {
                    properties.Add(s, Vector2.zero);
                }
            }
        }
        /*
            public float GetNormalTime(bool clampOutput = false) {
                //we assume that modtime is always > starttime, practically speaking
                float modTime = animator.looping ? animator.time % animator.duration : animator.time;
                float startTime = (float)(startIndex) / animator.frameRate;
                float elapsedTime = modTime - startTime;
                float t = elapsedTime / duration;
                return clampOutput ? Mathf.Clamp(t, 0, 0.99f) : t;
            }
    */
        public float GetNormalTime(bool clampOutput = false) {
            float t = (animator.time - startTime) / duration;
            return clampOutput ? Mathf.Clamp(t, 0, 0.99f) : t;
        }

        public void ProcessByFrame(Layer l, int frameIndex) {
            float t1 = (float)(frameIndex + 1 - startIndex) / (float)durationInFrames;
            float t0 = (float)(frameIndex - startIndex) / (float)durationInFrames;

            Process(l, t1, t1 - t0);
        }


        public void Evaluate() {
            float prevT = t;
            t = GetNormalTime();

            if (prevT < 0.99f && t >= 1) {
                t = 0.99f;
            }
        }

        public void Process(Layer l, float t, float tDelta) {
            Sprite sprite = sheet.spriteList[0];

            foreach (string s in startFrame.props.Keys) {

                Curve tCurve = startFrame.props[s].curveVal;
                float propTime1 = tCurve.EvaluateForX(t).y;
                float propTime0 = tCurve.EvaluateForX(t - tDelta / duration).y;

                Curve sCurve = l.CurveFromKeyFrames(startIndex);

                if (sCurve != null) { //TODO: instead of checking for null, make it so sCurve can't be null...

                    //this is in sprite pixel space, not centred or anything
                    Vector2 spatialVal1 = sCurve.Evaluate(propTime1);
                    Vector2 spatialVal0 = sCurve.Evaluate(propTime0);

                    //pixel delta between samples
                    Vector2 spaceDelta = spatialVal1 - spatialVal0;

                    //make it face the way the animator is facing...
                    float facingDirection = animator.transform.localScale.x > 0 ? 1 : -1;
                    spaceDelta = new Vector2(spaceDelta.x * facingDirection, -spaceDelta.y);

                    if (tDelta != 0) {
                        //divide by PPU to scale into world spa
                        properties[s] = spaceDelta / sprite.pixelsPerUnit / tDelta;
                    }
                }

            }
        }

        public string GetID() {
            return animator.GetSheet().layers.IndexOf(layer) + " - " + startIndex;
        }
    }
}