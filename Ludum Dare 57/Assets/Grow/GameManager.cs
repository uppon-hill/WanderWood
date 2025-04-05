using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour {
    public List<Transform> lights;
    public static GameManager instance;

    public Pointer pointer;
    public Orb selectedOrb;

    public void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    public void Update() {

        if (Input.mousePosition.x >= 0 && Input.mousePosition.x <= Screen.width &&
            Input.mousePosition.y >= 0 && Input.mousePosition.y <= Screen.height) {
            UnityEngine.Cursor.visible = false; // Hide the cursor
        } else {
            UnityEngine.Cursor.visible = true; // Show the cursor
        }

        if (selectedOrb != null) {
            selectedOrb.SetPosition(pointer.transform.position);
        }
    }

    public void DeselectOrb() {
        selectedOrb = null;
    }

    public void SelectOrb(Orb orb) {
        selectedOrb = orb;
    }
}