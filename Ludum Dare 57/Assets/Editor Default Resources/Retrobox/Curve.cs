using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

namespace Retro {
    [System.Serializable]
    public class Curve {
        double EPSILON = 1E-10;
        public const double TAU = 6.2831853071795862;

        const int res = 30;
        const int controlDims = 8;

        public Vector2 a { //start anchor
            get { return points[0]; }
            set { points[0] = value; }
        }
        public Vector2 a1 { //start handle
            get { return points[1]; }
            set { points[1] = value; }
        }

        public Vector2 b1 {//end handle
            get { return points[2]; }
            set { points[2] = value; }
        }

        public Vector2 b {//end anchor 
            get { return points[3]; }
            set { points[3] = value; }
        }

        [SerializeField]
        public Vector2[] points;

        static float controlWeight = 3;


        public static Curve Linear => new Curve(Vector2.zero, new Vector2(0.25f, 0.25f), new Vector2(0.75f, 0.75f), Vector2.one);
        public static Curve EaseInOut => new Curve(Vector2.zero, new Vector2(0.25f, 0), new Vector2(0.75f, 1), Vector2.one);
        public static Curve Accelerate => new Curve(Vector2.zero, new Vector2(0.25f, 0), new Vector2(0.75f, 1), Vector2.one);
        public static Curve Decelerate => new Curve(Vector2.zero, new Vector2(0.25f, 0.25f), new Vector2(0.75f, 0.75f), Vector2.one);
        public static Curve Clone(Curve c) {
            if (c != null && c.points != null && c.points.Length > 0) {
                return new Curve(c.a, c.a1, c.b1, c.b);
            } else {
                return Linear;
            }
        }

        public Curve(Vector2 a_, Vector2 b_) {
            points = new Vector2[] { a_, a_ + (b_ - a_) * 0.25f, b_ + (a_ - b_) * 0.25f, b_ };
        }

        public Curve(Vector2 a_, Vector2 a1_, Vector2 b1_, Vector2 b_) {
            points = new Vector2[] { a_, a1_, b1_, b_ };
        }

        public Vector2 Evaluate(float t) {
            //do some maths
            Vector2 one = Vector2.Lerp(a, a1, t);
            Vector2 two = Vector2.Lerp(a1, b1, t);
            Vector2 three = Vector2.Lerp(b1, b, t);
            Vector2 four = Vector2.Lerp(one, two, t);
            Vector2 five = Vector2.Lerp(two, three, t);
            Vector2 six = Vector2.Lerp(four, five, t);

            //return the lerped lerp of the lerps of the points
            return six;
        }

        public void DrawCurve(Rect bounds, bool drawHandles = true, bool drawAnchors = true) {
            DrawCurve(new Rect(Vector2.zero, Vector2.one), bounds, drawHandles, drawAnchors);
        }

        public void DrawCurve(Rect startBounds, Rect endBounds, bool drawHandles = true, bool drawAnchors = true) {
#if UNITY_EDITOR

            Vector2 sa = Vector2Utils.Map(a, startBounds.min, startBounds.max, endBounds.min, endBounds.max);
            Vector2 sa1 = Vector2Utils.Map(a1, startBounds.min, startBounds.max, endBounds.min, endBounds.max);
            Vector2 sb = Vector2Utils.Map(b, startBounds.min, startBounds.max, endBounds.min, endBounds.max);
            Vector2 sb1 = Vector2Utils.Map(b1, startBounds.min, startBounds.max, endBounds.min, endBounds.max);

            Vector3[] points = new Vector3[res + 1];

            for (int i = 0; i <= res; i++) {
                points[i] = Vector2Utils.Map(Evaluate((float)i / (float)res), startBounds.min, startBounds.max, endBounds.min, endBounds.max);
            }

            //draw the curve
            Handles.DrawAAPolyLine(controlWeight, points.Length, points);//top
            Vector2 away = (Vector2.one * controlDims * 0.5f);

            //draw the lines between points and their handles
            if (drawHandles) {
                Color c = Handles.color;
                Handles.color = (Handles.color + Color.white) * 0.5f;
                Handles.DrawAAPolyLine(controlWeight, sa, sa1);
                Handles.DrawAAPolyLine(controlWeight, sb, sb1);
                Handles.color = c;

                Rect aHandle = new Rect(sa1 - away, Vector2.one * controlDims);
                Handles.DrawSolidRectangleWithOutline(aHandle, Handles.color, Color.clear);
                Rect bHandle = new Rect(sb1 - away, Vector2.one * controlDims);
                Handles.DrawSolidRectangleWithOutline(bHandle, Handles.color, Color.clear);
            }

            //draw the control points
            if (drawAnchors) {
                Rect aHandle = new Rect(sa - away, Vector2.one * controlDims);
                Handles.DrawSolidRectangleWithOutline(aHandle, Color.white, Color.clear);
                Rect bHandle = new Rect(sb - away, Vector2.one * controlDims);
                Handles.DrawSolidRectangleWithOutline(bHandle, Color.black, Color.clear);
            }
#endif
        }

        public Vector2 EvaluateDrawnCurve(Rect startBounds, Rect endBounds, float t) {
            return Vector2Utils.Map(Evaluate(t), startBounds.min, startBounds.max, endBounds.min, endBounds.max);
        }


        public int GetEditingHandle(Vector2 mousePos, Rect endBounds, bool handles = true, bool anchors = true) {
            return GetEditingHandle(mousePos, new Rect(Vector2.zero, Vector2.one), endBounds, handles, anchors);
        }

        public int GetEditingHandle(Vector2 mousePos, Rect startBounds, Rect endBounds, bool handles = true, bool anchors = true) {

            int value = -1;
            Vector2 localMousePos = Vector2Utils.Map(mousePos, endBounds.min, endBounds.max, startBounds.min, startBounds.max);
            for (int i = 0; i < points.Length; i++) {

                if (((i == 0 || i == 3) && anchors) || ((i == 1 || i == 2) && handles)) {

                    Vector2 startBoundsSize = new Vector2(Mathf.Abs(startBounds.size.x), Mathf.Abs(startBounds.size.y));
                    Vector2 endBoundsSize = new Vector2(Mathf.Abs(endBounds.size.x), Mathf.Abs(endBounds.size.y));
                    //scaled handle size
                    Vector2 shSize = controlDims * (startBoundsSize / endBoundsSize);
                    Rect handle = new Rect(points[i] - (shSize * 0.5f), shSize);

                    if (handle.Contains(localMousePos)) {
                        return i;
                    }
                }
            }

            return value;
        }


        public void DebugMousePos(Vector2 mousePos, Rect startBounds, Rect endBounds) {
#if UNITY_EDITOR
            Vector2 localMousePos = Vector2Utils.Map(mousePos, endBounds.min, endBounds.max, startBounds.min, startBounds.max);
            for (int i = 0; i < points.Length; i++) {
                Vector2 endBoundsSize = new Vector2(Mathf.Abs(endBounds.size.x), Mathf.Abs(endBounds.size.y));
                Vector2 startBoundsSize = new Vector2(Mathf.Abs(startBounds.size.x), Mathf.Abs(startBounds.size.y));

                //scaled handle size
                Vector2 shSize = controlDims * (startBoundsSize / endBoundsSize);
                Rect handle = new Rect(points[i] - (shSize * 0.5f), shSize);

                Vector2 worldHandlePos = Vector2Utils.Map(handle.position, startBounds.min, startBounds.max, endBounds.min, endBounds.max);

                Handles.color = Color.red;
                Handles.DrawWireCube(handle.position, shSize);
                Handles.color = Color.blue;

                Handles.DrawWireCube(mousePos, Vector3.one * 10);
                Handles.color = Color.cyan;

                Handles.DrawWireCube(localMousePos, Vector3.one * 10);

            }
#endif
        }


        public void EditCurve(int pointIndex, Vector2 mousePos, Rect endBounds, bool clamp = true) {
            EditCurve(pointIndex, mousePos, new Rect(Vector2.zero, Vector2.one), endBounds, clamp);
        }
        public void EditCurve(int pointIndex, Vector2 mousePos, Rect startBounds, Rect endBounds, bool clamp = true) {
            points[pointIndex] = Vector2Utils.Map(mousePos, endBounds.min, endBounds.max, startBounds.min, startBounds.max, clamp);
        }

        public Vector2 EvaluateForX(float x) {

            float t = FindTForX(x);
            return Evaluate(t);
        }

        float FindTForX(double x) {
            double t = 0;
            double[] roots = FindRoots(x);
            if (roots.Length > 0) {
                foreach (double _t in roots) {
                    if (_t < 0 || _t > 1) continue;
                    t = _t;
                    break;
                }
            }

            return (float)t;
        }

        // Find the roots for a cubic polynomial with bernstein coefficients
        // {pa, pb, pc, pd}. The function will first convert those to the
        // standard polynomial coefficients, and then run through Cardano's
        // formula for finding the roots of a depressed cubic curve.
        double[] FindRoots(double x) {
            double pa = (double)this.a.x;
            double pb = (double)this.a1.x;
            double pc = (double)this.b1.x;
            double pd = (double)this.b.x;

            double pa3 = 3 * pa,
                    pb3 = 3 * pb,
                    pc3 = 3 * pc,
                    a = -pa + pb3 - pc3 + pd,
                    b = pa3 - 2 * pb3 + pc3,
                    c = -pa3 + pb3,
                    d = pa - x;

            // Fun fact: any Bezier curve may (accidentally or on purpose)
            // perfectly model any lower order curve, so we want to test 
            // for that: lower order curves are much easier to root-find.
            if (Approximately(a, 0)) {
                // this is not a cubic curve.
                if (Approximately(b, 0)) {
                    // in fact, this is not a quadratic curve either.
                    if (Approximately(c, 0)) {
                        // in fact in fact, there are no solutions.
                        return new double[] { };
                    }

                    // linear solution:
                    return new double[] { -d / c };
                }
                // quadratic solution:
                double qu = Math.Sqrt(c * c - 4 * b * d),
                        b2 = 2 * b;
                return new double[]{
                    (qu - c) / b2,
                    (-c - qu) / b2
                };
            }

            // At this point, we know we need a cubic solution,
            // and the above a/b/c/d values were technically
            // a pre-optimized set because a might be zero and
            // that would cause the following divisions to error.

            b /= a;
            c /= a;
            d /= a;

            double b3 = b / 3,
              p = (3 * c - b * b) / 3,
              p3 = p / 3,
              q = (2 * b * b * b - 9 * b * c + 27 * d) / 27,
              q2 = q / 2,
              discriminant = q2 * q2 + p3 * p3 * p3,
              u1, v1;

            // case 1: three real roots, but finding them involves complex
            // maths. Since we don't have a complex data type, we use trig
            // instead, because complex numbers have nice geometric properties.
            if (discriminant < 0) {
                double mp3 = -p / 3,
                  r = Math.Sqrt(mp3 * mp3 * mp3),
                  t = -q / (2 * r),
                  cosphi = t < -1 ? -1 : t > 1 ? 1 : t,
                  phi = Math.Acos(cosphi),
                  crtr = Cbrt(r),
                  t1 = 2 * crtr;
                return new double[]{
                            t1 * Math.Cos(phi / 3) - b3,
                            t1 * Math.Cos((phi + TAU) / 3) - b3,
                            t1 * Math.Cos((phi + 2 * TAU) / 3) - b3
                        };
            }

            // case 2: three real roots, but two form a "double root",
            // and so will have the same resultant value. We only need
            // to return two values in this case.
            else if (discriminant == 0) {
                u1 = q2 < 0 ? Cbrt(-q2) : -Cbrt(q2);
                return new double[]{
                2 * u1 - b3,
                -u1 - b3
                };
            }

            // case 3: one real root, 2 complex roots. We don't care about
            // complex results so we just ignore those and directly compute
            // that single real root.
            else {
                double sd = Math.Sqrt(discriminant);

                u1 = Cbrt(-q2 + sd);
                v1 = Cbrt(q2 + sd);


                return new double[] { u1 - v1 - b3 };
            }
        }

        bool Approximately(double n0, double n1) {
            return Math.Abs(n1 - n0) <= EPSILON;
        }

        double Cbrt(double d) {
            if (d < 0.0f) {
                return -Math.Pow(-d, 1f / 3f);
            } else {
                return Math.Pow(d, 1f / 3f);
            }
        }


        public static void Subdivide(Vector3 a0, Vector3 a1, Vector3 a2, Vector3 a3, float t, out Vector2[] firstPart, out Vector2[] secondPart) {
            var b0 = Vector3.Lerp(a0, a1, t); // Same as evaluating a Bezier
            var b1 = Vector3.Lerp(a1, a2, t);
            var b2 = Vector3.Lerp(a2, a3, t);

            var c0 = Vector3.Lerp(b0, b1, t);
            var c1 = Vector3.Lerp(b1, b2, t);

            var d0 = Vector3.Lerp(c0, c1, t); // This would be the interpolated point

            firstPart = new Vector2[] { a0, b0, c0, d0 }; // first point of each step
            secondPart = new Vector2[] { a3, b2, c1, d0 }; // last point of each step
        }

        public static void Subdivide(Curve curve, float t, out Curve firstPart, out Curve secondPart) {
            var b0 = Vector3.Lerp(curve.a, curve.a1, t); // Same as evaluating a Bezier
            var b1 = Vector3.Lerp(curve.a1, curve.b1, t);
            var b2 = Vector3.Lerp(curve.b1, curve.b, t);

            var c0 = Vector3.Lerp(b0, b1, t);
            var c1 = Vector3.Lerp(b1, b2, t);

            var d0 = Vector3.Lerp(c0, c1, t); // This would be the interpolated point

            firstPart = new Curve(curve.a, b0, c0, d0); // first point of each step
            secondPart = new Curve(curve.b, b2, c1, d0); // last point of each step
        }
    }


}

public static class Vector2Utils {
    public static Vector2 Map(Vector2 value, Vector2 min1, Vector2 max1, Vector2 min2, Vector2 max2, bool clamp = false) {
        Vector2 mapped = min2 + (max2 - min2) * ((value - min1) / (max1 - min1));
        if (clamp) {
            mapped.x = Mathf.Clamp(mapped.x, min2.x, max2.x);
            mapped.y = Mathf.Clamp(mapped.y, min2.y, max2.y);
        }
        return mapped;
    }
}
