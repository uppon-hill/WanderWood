using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Pointer : MonoBehaviour {

    public UnityEvent<Pointer> onSelect;
    // Start is called before the first frame update
    void Awake() {
        GameManager.i.pointer = this;
    }

    void Start() {
        GameManager.i.lights.Add(transform);
    }

    // Update is called once per frame
    void Update() {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Mathf.Abs(Camera.main.transform.position.z);

        // Convert the screen point to a world point
        transform.position = Camera.main.ScreenToWorldPoint(mousePosition);

        // Ensure the pointer stays on the desired plane (e.g., z = 0)
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);

        if (Input.GetMouseButtonDown(0)) {
            if (GameManager.i.selectedOrb == null) {
                Select();
            } else {
                Deselect();
            }
        }
    }

    void Select() {
        onSelect.Invoke(this);
    }

    void Deselect() {
        GameManager.i.DeselectOrb();
    }
}
