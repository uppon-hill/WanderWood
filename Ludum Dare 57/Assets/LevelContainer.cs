using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelContainer : MonoBehaviour {
    public SpriteRenderer geometryRenderer;
    public SpriteRenderer backgroundRenderer;
    public SpriteMask mask;
    public PolygonCollider2D geometry;
    Collider2D[] allColliders;
    SpriteRenderer[] allRenderers;
    CharacterShadow shadow;
    public int sortingOrder { get { return geometryRenderer.sortingOrder; } }

    void Awake() {
        shadow = GetComponentInChildren<CharacterShadow>();
    }
    // Start is called before the first frame update
    void Start() {
        backgroundRenderer.sortingOrder = geometryRenderer.sortingOrder;
        mask.frontSortingOrder = geometryRenderer.sortingOrder;
        mask.backSortingOrder = geometryRenderer.sortingOrder - 1;

        allRenderers = GetComponentsInChildren<SpriteRenderer>();
        allColliders = GetComponentsInChildren<Collider2D>();

        geometry = GetComponent<PolygonCollider2D>();

        IContainable[] containables = GetComponentsInChildren<IContainable>();
        foreach (IContainable c in containables) {
            c.container = this;
        }

        SetGeometry(IsCurrentIndex());
    }

    // Update is called once per frame
    void Update() {

    }

    public void SetAlpha(float a) {
        foreach (SpriteRenderer r in allRenderers) {
            r.color = new Color(r.color.r, r.color.g, r.color.b, a);
        }
    }

    public void SetGeometry(bool enabled) {
        shadow.gameObject.SetActive(!enabled);

        foreach (Collider2D c in allColliders) {
            string layer = enabled ? "Ground" : "Shadow";
            c.gameObject.layer = LayerMask.NameToLayer(layer);
        }
    }

    public bool IsCurrentIndex() {
        LevelZoomer z = GameManager.i.zoomer;
        return z.current == z.levels.IndexOf(this);
    }

    public bool Navigable() {
        return !shadow.OverlapsCollider();
    }
}
