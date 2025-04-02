using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class FramePropertyEventCollection {
    Dictionary<string, UnityEvent> events;

    public FramePropertyEventCollection() {
        events = new Dictionary<string, UnityEvent>();
    }

    public void AddListener(UnityAction a) {
        string methodName = a.Method.Name;
        if (!events.ContainsKey(methodName)) {
            events.Add(methodName, new UnityEvent());
        }
        events[methodName].AddListener(a);
    }

    public void RemoveListener(UnityAction a) {
        string methodName = a.Method.Name;
        events[methodName].RemoveListener(a);
    }

    public void RemoveAllListeners(string s) {
        events.Remove(s);
    }

    public bool ContainsEvent(string s) {
        return events.ContainsKey(s);
    }

    public void Invoke(string s) {
        events[s].Invoke();
    }
}
