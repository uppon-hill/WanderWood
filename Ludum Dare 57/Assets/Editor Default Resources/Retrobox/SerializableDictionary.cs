using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver {
    [SerializeField]
    TKey[] m_keys;
    [SerializeField]
    TValue[] m_values;

    [SerializeField]
    List<TKey> o_keys;

    public SerializableDictionary() {
        o_keys = new List<TKey>();
    }

    public SerializableDictionary(IDictionary<TKey, TValue> dict) : base(dict.Count) {
        foreach (var kvp in dict) {
            this[kvp.Key] = kvp.Value;
        }
        o_keys = new List<TKey>(m_keys);

    }

    public void CopyFrom(IDictionary<TKey, TValue> dict) {
        Clear();
        foreach (var kvp in dict) {
            this[kvp.Key] = kvp.Value;
        }
        o_keys = new List<TKey>(m_keys);
    }

    public void OnAfterDeserialize() {
        if (m_keys != null && m_values != null && m_keys.Length == m_values.Length) {
            this.Clear();
            int n = m_keys.Length;
            for (int i = 0; i < n; ++i) {
                this[m_keys[i]] = m_values[i];
            }

            m_keys = null;
            m_values = null;
        }

    }

    public void OnBeforeSerialize() {
        int n = this.Count;
        m_keys = new TKey[n];
        m_values = new TValue[n];

        int i = 0;
        foreach (var kvp in this) {
            m_keys[i] = kvp.Key;
            m_values[i] = kvp.Value;
            ++i;
        }
    }

    public TKey GetKey(int i) {
        return o_keys[i];
    }

    public TValue GetValue(int i) {
        return this[o_keys[i]];
    }

    public KeyValuePair<TKey, TValue> Get(int i) {
        return new KeyValuePair<TKey, TValue>(o_keys[i], this[o_keys[i]]);
    }

    //get the index of an item from the dictionary as it appears in the o_keys list
    public int IndexOf(TKey key) {
        return o_keys.IndexOf(key);
    }

    //Add an object to the list 
    public new void Add(TKey key, TValue value) {
        base.Add(key, value);
        o_keys.Add(key);
    }


    public void InsertAt(int i, TKey key, TValue value) {
        base.Add(key, value);
        o_keys.Insert(i, key);
    }


    //Add an object to the list 
    public new void Remove(TKey key) {
        base.Remove(key);
        o_keys.Remove(key);
    }

    //remove an object from the list the the given index
    public void Remove(int i) {
        base.Remove(o_keys[i]);
        o_keys.RemoveAt(i);
    }

    //move an object up in the list
    public void MoveUp(int i) {
        if (i != 0) {
            TKey s = o_keys[i - 1];
            o_keys[i - 1] = o_keys[i];
            o_keys[i] = s;
        }
    }

    //move an object down in the list
    public void MoveDown(int i) {
        if (i != o_keys.Count - 1) {
            TKey s = o_keys[i + 1];
            o_keys[i + 1] = o_keys[i];
            o_keys[i] = s;
        }
    }

    //access using []
    public TValue this[int i] {
        get { return this[o_keys[i]]; }
        set { this[o_keys[i]] = value; }
    }

}


/*
 * License for Serializable Dictionary Class
    MIT License

    Copyright(c) 2017 Mathieu Le Ber

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files(the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.

* Thanks, Matt!
* - Adam
*/
