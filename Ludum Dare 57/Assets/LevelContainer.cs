using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
public class LevelContainer : MonoBehaviour {
    public Color layerColor;
    public SpriteRenderer geometryRenderer;
    public SpriteRenderer backgroundRenderer;
    public SpriteMask mask;
    public PolygonCollider2D geometry;
    Collider2D[] allColliders;
    List<SpriteRenderer> allRenderers;
    List<SpriteRenderer> allMessRenderers;
    CharacterShadow shadow;
    public int sortingOrder { get { return geometryRenderer.sortingOrder; } }

    void Awake() {
        shadow = GetComponentInChildren<CharacterShadow>();
        IContainable[] containables = GetComponentsInChildren<IContainable>();
        foreach (IContainable c in containables) {
            c.container = this;
        }

    }
    // Start is called before the first frame update
    void Start() {
        //backgroundRenderer.sortingOrder = geometryRenderer.sortingOrder;
        //mask.frontSortingOrder = geometryRenderer.sortingOrder;
        //mask.backSortingOrder = geometryRenderer.sortingOrder - 1;

        allColliders = GetComponentsInChildren<Collider2D>();

        geometry = GetComponent<PolygonCollider2D>();


        geometryRenderer.material = new Material(geometryRenderer.material);
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

    public void SetShadowAlpha(float a) {
        geometryRenderer.material.SetFloat("_ShadowAmount", a);
    }

    public void AddRenderer(SpriteRenderer r) {
        allRenderers.Add(r);
    }
    public void RemoveRenderer(SpriteRenderer r) {
        allRenderers.Remove(r);
    }

    public void SetSorting(int order) {
        //assigns sorting order
        geometryRenderer.sortingOrder = order;
        backgroundRenderer.sortingOrder = order - 1;
        mask.frontSortingOrder = order;
        mask.backSortingOrder = order - 9;

        allRenderers = GetComponentsInChildren<SpriteRenderer>().ToList();
        allMessRenderers = allRenderers.ToList();

        allMessRenderers.Remove(geometryRenderer);
        allMessRenderers.Remove(backgroundRenderer);
        allMessRenderers.Remove(mask.GetComponent<SpriteRenderer>());

        foreach (SpriteRenderer r in allMessRenderers) {
            r.sortingOrder += order;
        }

    }
}
