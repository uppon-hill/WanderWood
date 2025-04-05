using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelContainer : MonoBehaviour {
    public SpriteRenderer geometryRenderer;
    public SpriteRenderer backgroundRenderer;
    public SpriteMask mask;
    public PolygonCollider2D collider2D;
    // Start is called before the first frame update
    void Start() {
        backgroundRenderer.sortingOrder = geometryRenderer.sortingOrder;
        mask.frontSortingOrder = geometryRenderer.sortingOrder;
        mask.backSortingOrder = geometryRenderer.sortingOrder - 1;
    }

    // Update is called once per frame
    void Update() {

    }
}
