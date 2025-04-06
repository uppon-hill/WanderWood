using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour {
    public PolygonCollider2D polygonCollider;
    public BoxCollider2D boxCollider;
    // Start is called before the first frame update
    void Start() {
        Debug.Log(Physics2D.OverlapBox(boxCollider.bounds.center, boxCollider.bounds.size, 0f, new ContactFilter2D(), new List<Collider2D>()));
    }

    // Update is called once per frame
    void Update() {

    }
}
