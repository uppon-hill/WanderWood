using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;


public static class Helpers {

    public static int Mod(int a, int b) {
        return (a % b + b) % b;
    }


    public static GameObject GetChildComponent(GameObject fromGameObject, string withName) {
        Transform[] ts = fromGameObject.transform.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in ts) {
            if (t.gameObject.name == withName) {
                return t.gameObject;
            }
        }
        return null;
    }


    /// <summary>
    /// Adjust a vector to the nearest pixel (PPU 100)
    /// </summary>
    public static Vector2 PixelPerfect(Vector2 target) {

        //int x = (int)(target.x * 100f)+1;
        //int y = (int)(target.y * 100f)+2;
        //return new Vector2(x * 0.01f, y * 0.01f);
        return new Vector2(Mathf.Round(target.x * 100) / 100, Mathf.Round(target.y * 100) / 100);

    }

    public static float RoundTo2DP(float num) {
        return num = Mathf.Round(num * 100f) / 100f;
    }

    /// <summary>
    /// Return the individual texture for a given index in a sprite matrix
    /// </summary>
    /// <param name="sprite">The source sprite.</param>
    /// <param name="filtermode">the required filter mode</param>
    public static Texture2D GetTextureFromSprite(Sprite sprite, FilterMode filterMode) {
        var rect = sprite.rect;
        Texture2D tex = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
        Graphics.CopyTexture(sprite.texture, 0, 0, (int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height, tex, 0, 0, 0, 0);
        tex.filterMode = filterMode;
        tex.Apply(true);

        return tex;
    }

    /// <summary>
    /// Return the individual texture for a given index in a sprite matrix
    /// </summary>
    /// <param name="sprite">The source sprite.</param>
    public static Texture2D GetTextureFromSprite(Sprite sprite) {
        return GetTextureFromSprite(sprite, FilterMode.Point);
    }


    //flip a Color[] along the X / Y axis.
    public static Color[] FlipTextureDataHorizontal(Color[] input, int width, int height, bool vertical, bool horizontal) {
        Color[] flipped_data = new Color[input.Length];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                int index = 0;
                if (horizontal && vertical)
                    index = width - 1 - x + (height - 1 - y) * width;
                else if (horizontal && !vertical)
                    index = width - 1 - x + y * width;
                else if (!horizontal && vertical)
                    index = x + (height - 1 - y) * width;
                else if (!horizontal && !vertical)
                    index = x + y * width;

                flipped_data[x + y * width] = input[index];
            }
        }


        return flipped_data;
    }



    public static Vector2 RotateVector(Vector2 v, float angle) {
        float radian = angle * Mathf.Deg2Rad;
        float _x = v.x * Mathf.Cos(radian) - v.y * Mathf.Sin(radian);
        float _y = v.x * Mathf.Sin(radian) + v.y * Mathf.Cos(radian);
        return new Vector2(_x, _y);
    }


    public static float DistanceFromPointToPlane(Vector2 point, Vector2 p1, Vector2 p2) {
        return Mathf.Abs((p2.x - p1.x) * (p1.y - point.y) - (p1.x - point.x) * (p2.y - p1.y)) /
               Mathf.Sqrt(Mathf.Pow(p2.x - p1.x, 2) + Mathf.Pow(p2.y - p1.y, 2));
    }

    public static float PointToSegment(Vector2 point, Vector2 p1, Vector2 p2) {
        float t = 0;
        return PointToSegment(point, p1, p2, ref t);
    }

    // Calculate the distance between
    // point pt and the segment p1 --> p2.
    public static float PointToSegment(Vector2 point, Vector2 p1, Vector2 p2, ref float t) {

        Vector2 closest;
        float dx = p2.x - p1.x;
        float dy = p2.y - p1.y;
        t = 0;

        if ((dx == 0) && (dy == 0)) {// It's a point not a line segment.
            closest = p1;
            dx = point.x - p1.x;
            dy = point.y - p1.y;

            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        // Calculate the t that minimizes the distance.
        t = ((point.x - p1.x) * dx + (point.y - p1.y) * dy) / (dx * dx + dy * dy);
        // See if this represents one of the segment's end points or a point in the middle.
        if (t < 0) {
            closest = new Vector2(p1.x, p1.y);
            dx = point.x - p1.x;
            dy = point.y - p1.y;
        } else if (t > 1) {
            closest = new Vector2(p2.x, p2.y);
            dx = point.x - p2.x;
            dy = point.y - p2.y;
        } else {
            closest = new Vector2(p1.x + t * dx, p1.y + t * dy);
            dx = point.x - closest.x;
            dy = point.y - closest.y;
        }

        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    // For infinite lines:
    public static Vector3 PointToLine(Vector3 point, Vector3 start, Vector3 end) {
        return start + Vector3.Project(point - start, end - start);
    }

    public static Vector2 PointToSegmentPoint(Vector2 point, Vector2 p1, Vector2 p2) {
        Vector2 closest;
        float dx = p2.x - p1.x;
        float dy = p2.y - p1.y;

        if ((dx == 0) && (dy == 0)) {// It's a point not a line segment.
            closest = p1;
            return closest;
        }

        // Calculate the t that minimizes the distance.
        float t = ((point.x - p1.x) * dx + (point.y - p1.y) * dy) / (dx * dx + dy * dy);

        // See if this represents one of the segment's end points or a point in the middle.
        if (t < 0) {
            closest = new Vector2(p1.x, p1.y);
        } else if (t > 1) {
            closest = new Vector2(p2.x, p2.y);
        } else {
            closest = new Vector2(p1.x + t * dx, p1.y + t * dy);
        }

        return closest;
    }

    public static float PointToEdgeChain(Vector2 point, List<Vector2> edgeChain) {
        float t;
        int closest;
        return PointToEdgeChain(point, edgeChain, out closest, out t);
    }

    public static float PointToEdgeChain(Vector2 point, List<Vector2> edgeChain, out int closestIndex, out float t) {
        //return the shortest distance to any of the points in the shape
        float shortest = float.MaxValue;
        closestIndex = 0;
        t = 0;
        for (int i = 0; i < edgeChain.Count - 1; i++) {
            Vector2 current = edgeChain[i];
            Vector2 next = edgeChain[i + 1];
            float d = PointToSegment(point, current, next, ref t);
            if (d < shortest) {
                shortest = d;
                closestIndex = i;
            }
        }
        return shortest;
    }


    public static int SolveBallisticArc(Vector3 proj_pos, float speed, Vector3 target, float gravity, out Vector3 s0, out Vector3 s1) {

        // Handling these cases is up to your project's coding standards
        Debug.Assert(proj_pos != target && speed > 0 && gravity > 0, "fts.solve_ballistic_arc called with invalid data");

        // C# requires out variables be set
        s0 = Vector3.zero;
        s1 = Vector3.zero;

        // Derivation
        //   (1) x = v*t*cos O
        //   (2) y = v*t*sin O - .5*g*t^2
        // 
        //   (3) t = x/(cos O*v)                                        [solve t from (1)]
        //   (4) y = v*x*sin O/(cos O * v) - .5*g*x^2/(cos^2 O*v^2)     [plug t into y=...]
        //   (5) y = x*tan O - g*x^2/(2*v^2*cos^2 O)                    [reduce; cos/sin = tan]
        //   (6) y = x*tan O - (g*x^2/(2*v^2))*(1+tan^2 O)              [reduce; 1+tan O = 1/cos^2 O]
        //   (7) 0 = ((-g*x^2)/(2*v^2))*tan^2 O + x*tan O - (g*x^2)/(2*v^2) - y    [re-arrange]
        //   Quadratic! a*p^2 + b*p + c where p = tan O
        //
        //   (8) let gxv = -g*x*x/(2*v*v)
        //   (9) p = (-x +- sqrt(x*x - 4gxv*(gxv - y)))/2*gxv           [quadratic formula]
        //   (10) p = (v^2 +- sqrt(v^4 - g(g*x^2 + 2*y*v^2)))/gx        [multiply top/bottom by -2*v*v/x; move 4*v^4/x^2 into root]
        //   (11) O = atan(p)

        Vector3 diff = target - proj_pos;
        Vector3 diffXZ = new Vector3(diff.x, 0f, diff.z);
        float groundDist = diffXZ.magnitude;
        float speed2 = speed * speed;
        float speed4 = speed * speed * speed * speed;
        float y = diff.y;
        float x = groundDist;
        float gx = gravity * x;

        float root = speed4 - gravity * (gravity * x * x + 2 * y * speed2);

        // No solution
        if (root < 0) {
            s0 = (target.x > proj_pos.x) ? new Vector3(0.5f * speed, 0.5f * speed) : new Vector3(-0.5f * speed, 0.5f * speed);
            s1 = (target.x > proj_pos.x) ? new Vector3(0.5f * speed, 0.5f * speed) : new Vector3(-0.5f * speed, 0.5f * speed);

            return 0;
        }
        root = Mathf.Sqrt(root);

        float lowAng = Mathf.Atan2(speed2 - root, gx);
        float highAng = Mathf.Atan2(speed2 + root, gx);
        int numSolutions = lowAng != highAng ? 2 : 1;

        Vector3 groundDir = diffXZ.normalized;
        s0 = groundDir * Mathf.Cos(lowAng) * speed + Vector3.up * Mathf.Sin(lowAng) * speed;
        if (numSolutions > 1) {
            s1 = groundDir * Mathf.Cos(highAng) * speed + Vector3.up * Mathf.Sin(highAng) * speed;
        }
        return numSolutions;
    }

    public static float ballistic_range(float speed, float gravity, float initial_height) {

        // Handling these cases is up to your project's coding standards
        Debug.Assert(speed > 0 && gravity > 0 && initial_height >= 0, "fts.ballistic_range called with invalid data");

        // Derivation
        //   (1) x = speed * time * cos O
        //   (2) y = initial_height + (speed * time * sin O) - (.5 * gravity*time*time)
        //   (3) via quadratic: t = (speed*sin O)/gravity + sqrt(speed*speed*sin O + 2*gravity*initial_height)/gravity    [ignore smaller root]
        //   (4) solution: range = x = (speed*cos O)/gravity * sqrt(speed*speed*sin O + 2*gravity*initial_height)    [plug t back into x=speed*time*cos O]
        float angle = 45 * Mathf.Deg2Rad; // no air resistence, so 45 degrees provides maximum range
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);

        float range = (speed * cos / gravity) * (speed * sin + Mathf.Sqrt(speed * speed * sin * sin + 2 * gravity * initial_height));
        return range;
    }

    public static Vector2[] DetermineTrajectoryPoints(Vector3 pStartPosition, Vector3 pVelocity, int pointCount, float timeStep) {
        float speed = Mathf.Sqrt((pVelocity.x * pVelocity.x) + (pVelocity.y * pVelocity.y));
        float angle = Mathf.Rad2Deg * (Mathf.Atan2(pVelocity.y, pVelocity.x));
        float cos = Mathf.Cos(angle * Mathf.Deg2Rad);
        float sin = Mathf.Sin(angle * Mathf.Deg2Rad);

        float fTime = 0;

        Vector2[] points = new Vector2[pointCount];
        points[0] = pStartPosition;
        float gravity = Physics2D.gravity.magnitude;
        fTime += timeStep;
        for (int i = 1; i < pointCount; i++) {

            float dx = speed * fTime * cos;
            float dy = speed * fTime * sin - (gravity * fTime * fTime / 2.0f);
            Vector3 pos = new Vector3(pStartPosition.x + dx, pStartPosition.y + dy, 2);
            points[i] = pos;
            fTime += timeStep;
        }

        return points;
    }

    public static List<Vector2> DetermineTrajectoryVelocities(List<Vector2> points, float step) {
        List<Vector2> newList = new List<Vector2>();

        for (int i = 0; i < points.Count - 1; i++) {
            newList.Add(points[i + 1] - points[i] / step);
        }

        //add the last one again, because why not
        newList.Add(points[points.Count - 1] - points[points.Count - 2] / step);

        return newList;
    }


    public static Vector3 GetCollisionVector(Collider2D from, Collider2D to) {
        if (from && to) {
            return (to.bounds.center - from.bounds.center).normalized;
        }
        return Vector3.zero;
    }

    public static float Map(float value, float min1, float max1, float min2, float max2, bool clamp = false) { //map value from one range to another
        float val = min2 + (max2 - min2) * ((value - min1) / (max1 - min1));
        return clamp ? Mathf.Clamp(val, Mathf.Min(min2, max2), Mathf.Max(min2, max2)) : val; //second min + delta2 * (distance between value and first / delta2)
    }


    public static bool Intersects2DBounds(Bounds a, Bounds b) {

        a.center = new Vector3(a.center.x, a.center.y, 0);
        b.center = new Vector3(b.center.x, b.center.y, 0);
        return b.Intersects(a);
    }



    public static bool IntersectsRect(Rect a, Rect b) {
        return a.x < b.x + b.width && a.x + a.width > b.x && a.y < b.y + b.height && a.y + a.height > b.y;
    }

    public static float PointRectDistance(Vector2 point, Rect rect) {
        var dx = Mathf.Max(rect.min.x - point.x, 0, point.x - rect.max.x);
        var dy = Mathf.Max(rect.min.y - point.y, 0, point.y - rect.max.y);
        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    public static void DrawRect(Rect r) {

        Vector2 a = new Vector2(r.xMax, r.yMin);
        Vector2 b = new Vector2(r.xMax, r.yMax);
        Vector2 c = new Vector2(r.xMin, r.yMax);
        Vector2 d = new Vector2(r.xMin, r.yMin);

        Gizmos.DrawLine(a, b);
        Gizmos.DrawLine(b, c);
        Gizmos.DrawLine(c, d);
        Gizmos.DrawLine(d, a);
    }

    public static void DrawPoint(Vector2 point) {

        Gizmos.DrawLine(point + Vector2.up * 0.1f, point + Vector2.down * 0.1f);
        Gizmos.DrawLine(point + Vector2.left * 0.1f, point + Vector2.right * 0.1f);
    }


    public static void DrawBounds(Bounds bounds) {

        Vector2 a = new Vector2(bounds.max.x, bounds.min.y);
        Vector2 b = new Vector2(bounds.max.x, bounds.max.y);
        Vector2 c = new Vector2(bounds.min.x, bounds.max.y);
        Vector2 d = new Vector2(bounds.min.x, bounds.min.y);

        Gizmos.DrawLine(a, b);
        Gizmos.DrawLine(b, c);
        Gizmos.DrawLine(c, d);
        Gizmos.DrawLine(d, a);
    }

    public static float GetCurveDuration(AnimationCurve curve) {
        return curve.keys[curve.keys.Length - 1].time;
    }

    public static bool IsChildOf(GameObject a, GameObject b) {

        foreach (Transform t in b.GetComponentsInChildren<Transform>()) {
            if (t == a.transform) {
                return true;
            }
        }

        return false;
    }

    public static bool ShapeContainsPoint(List<Vector2> shape, Vector2 point) {

        int i, j = shape.Count - 1;
        bool oddNodes = false;
        float y = point.y;
        float x = point.x;
        for (i = 0; i < shape.Count; i++) {
            if (shape[i].y < y && shape[j].y >= y
            || shape[j].y < y && shape[i].y >= y) {
                if (shape[i].x + (y - shape[i].y) / (shape[j].y - shape[i].y) * (shape[j].x - shape[i].x) < x) {
                    oddNodes = !oddNodes;
                }
            }
            j = i;
        }

        return oddNodes;
    }

    public static Vector3Int V3Int(Vector3 v) {
        return new Vector3Int(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y), Mathf.FloorToInt(v.z));
    }

    public static Vector3 V3(Vector3Int v) {
        return new Vector3(v.x, v.y, v.z);
    }
    public static Vector3 V3(Vector2Int v) {
        return new Vector3(v.x, v.y);
    }
    public static Vector3Int V3Int(Vector2Int v) {
        return new Vector3Int(v.x, v.y, 0);
    }

    public static Vector2Int V2Int(Vector3Int v) {
        return new Vector2Int(v.x, v.y);
    }

    public static Vector2Int V2Int(Vector3 v) {
        return new Vector2Int((int)v.x, (int)v.y);
    }

    public static Transform GetParentWithTag(Transform trans, string tag) {
        if (trans.tag.Equals(tag)) {
            return trans;
        } else {
            while (trans.parent != null) {
                trans = trans.parent;
                if (trans.tag.Equals(tag)) {
                    return trans;
                }
            }
        }
        return null;
    }

    public static Rect BoundsToRect(Bounds b) {
        return new Rect(b.min.x, b.min.y, b.size.x, b.size.y);
    }


    public static Bounds PointsToBounds(ICollection<Vector2> points) {
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        foreach (Vector2 p in points) {
            if (p.x < minX) {
                minX = p.x;
            }
            if (p.y < minY) {
                minY = p.y;
            }
            if (p.x > maxX) {
                maxX = p.x;
            }
            if (p.y > maxY) {
                maxY = p.y;
            }
        }

        return new Bounds(new Vector3((maxX + minX) / 2f, (maxY + minY) / 2f, 0), new Vector3(maxX - minX, maxY - minY, 0));
    }

    // Helper method to get full GameObject path
    public static string GetGameObjectPath(GameObject obj) {
        string path = obj.name;
        while (obj.transform.parent != null) {
            obj = obj.transform.parent.gameObject;
            path = obj.name + "/" + path;
        }
        return path;
    }

    public static string GetGameObjectNameFromPath(string path) {
        int lastSlashIndex = path.LastIndexOf('/');
        return lastSlashIndex >= 0 ? path.Substring(lastSlashIndex + 1) : path;
    }

    public static GameObject FindGameObjectByPath(string path) {
        string[] names = path.Split('/');
        GameObject root = FindRootObject(names[0]);

        if (root == null) return null;

        Transform currentTransform = root.transform;

        for (int i = 1; i < names.Length; i++) {
            currentTransform = FindChildByName(currentTransform, names[i]);
            if (currentTransform == null) return null; // Path is invalid
        }

        return currentTransform.gameObject;
    }

    // Finds a root object, even if inactive
    private static GameObject FindRootObject(string rootName) {
        GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject obj in rootObjects) {
            if (obj.name == rootName) return obj;
        }
        return null;
    }

    // Recursively searches for a child (including inactive ones)
    private static Transform FindChildByName(Transform parent, string childName) {
        foreach (Transform child in parent) {
            if (child.name == childName) return child;
        }
        return null;
    }


    public static Color AssignAlpha(Color col, float alpha) {
        return new Color(col.r, col.g, col.b, alpha);
    }
}

public static class RectExtension {
    public static Rect PadRect(this Rect rect, float amount) {
        return new Rect(rect.x - amount, rect.y - amount, rect.width + (amount * 2), rect.height + (amount * 2));
    }

}