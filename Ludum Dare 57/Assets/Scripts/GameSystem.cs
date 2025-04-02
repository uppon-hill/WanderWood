using System.Collections;
using System.Collections.Generic;
using Retro;
using UnityEngine;

public class GameSystem : MonoBehaviour {

    public static GameSystem system;
    public RetroboxPrefs boxPrefs;
    // Start is called before the first frame update
    void Start() {
        if (system == null) {
            system = this;
        } else {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update() {

    }
}
