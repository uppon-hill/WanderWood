using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using System.Linq;
public class ControlHash<T, U> {
    protected Dictionary<T, U> effectors;
    private List<T> keys;

    public int Count { get { return effectors.Count; } }

    // Use this for initialization
    public ControlHash() {
        effectors = new Dictionary<T, U>();
        keys = new List<T>();
    }

    public bool HasControl() {
        return (effectors.Count == 0);
    }

    public void LogEffectors() {
        Debug.Log("Effectors:");
        foreach (KeyValuePair<T, U> key in effectors.ToList()) {
            Debug.Log(key.Key + " : " + key.Value);
        }
    }

    public void AddEffector(T key, U value = default) {
        if (!effectors.ContainsKey(key)) {
            effectors.Add(key, value);
            keys.Add(key);
        } else {
            effectors[key] = value;
        }
    }

    public void RemoveEffector(T t) {
        effectors.Remove(t);
        keys.Remove(t);
    }

    public void RemoveEffectorsOfSameType(T t) {
        foreach (T k in effectors.Keys) {
            if (k.GetType() == t.GetType()) {
                effectors.Remove(k);
                keys.Remove(k);
            }
        }
    }

    public bool HasEffector(T t) {
        return effectors.ContainsKey(t);
    }

    public Dictionary<T, U> GetEffectors() {
        return effectors;
    }

    public virtual U GetValue() {
        if (effectors.Count == 0) {
            return default;
        }
        return effectors[keys[keys.Count - 1]];
    }


    public T GetKey() {
        if (effectors.Count == 0) {
            return default;
        }
        return keys[keys.Count - 1];
    }

    public void Clear() {
        effectors.Clear();
        keys.Clear();
    }

}

public class FloatControlHash<T> : ControlHash<T, float> {
    float defaultValue = 0;
    public FloatControlHash(float defaultValue = 0) {
        this.defaultValue = defaultValue;
    }

    public override float GetValue() {
        if (effectors.Count == 0) {
            return defaultValue;
        }
        return base.GetValue();
    }

    public float GetTotalValue() {
        float total = 0;
        foreach (float f in effectors.Values) {
            total += f;
        }
        return total;
    }
}


public class VectorControlHash<T> : ControlHash<T, Vector3> {
    Vector3 defaultValue = default;
    public VectorControlHash(Vector3 defaultValue = default) {
        this.defaultValue = defaultValue;
    }

    public override Vector3 GetValue() {
        if (effectors.Count == 0) {
            return defaultValue;
        }
        return base.GetValue();
    }

    public Vector3 GetTotalValue() {
        Vector3 total = default;
        foreach (Vector3 f in effectors.Values) {
            total += f;
        }

        return total;
    }


}