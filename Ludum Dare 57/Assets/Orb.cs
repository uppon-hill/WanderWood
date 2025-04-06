using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : MonoBehaviour, IContainable {
    public float speed;
    public float offset;
    float randomTimeStart;
    float startTime = 0;
    float t => randomTimeStart + Time.time - startTime;
    public LevelContainer container {
        get { return c; }
        set {
            c = value;
            if (value != null) {
                sprite.sortingOrder = value.sortingOrder;
            } else {
                sprite.sortingOrder = 0;
            }
        }
    }
    LevelContainer c;
    public SpriteRenderer sprite;

    public float clickRadius = 0.08f;

    // Start is called before the first frame update
    void Start() {
        GameManager.i.onSelect.AddListener(TryClick);
        randomTimeStart = Random.value * 100;
        GameManager.i.lights.Add(transform);

    }

    // Update is called once per frame
    void Update() {

        if (GameManager.i.selectedOrb == this) {
            transform.localPosition = Vector2.zero;
        } else {
            Breathe();
        }
    }


    public void SetPosition(Vector2 pos) {
        transform.parent.position = pos;
    }

    void Breathe() {
        transform.localPosition = Helpers.PixelPerfect(Vector2.up * Mathf.Sin(t * speed) * offset);
    }

    void TryClick(Transform selector) {
        bool onLayer = container == null || container.IsCurrentIndex();

        if (Vector2.Distance(selector.transform.position, transform.position) < clickRadius && onLayer) {
            GameManager.i.SelectOrb(this);
        }
    }
    public void Reparent(Transform parent) {
        transform.parent.SetParent(parent);
        if (parent.GetComponent<LevelContainer>() != null) {
            container = parent.GetComponent<LevelContainer>();
            sprite.sortingOrder = container.sortingOrder;
        } else {
            container = null;
        }
        randomTimeStart = 0;
        startTime = Time.time;
    }

}
