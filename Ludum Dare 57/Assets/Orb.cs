using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : MonoBehaviour, IContainable {


    public SpriteRenderer fireflies;
    public Color activeColour;
    public float speed;
    public float offset;
    float randomTimeStart;
    float startTime = 0;
    float t => randomTimeStart + Time.time - startTime;

    public Retro.RetroAnimator animator;
    public Retro.Sheet world;
    public Retro.Sheet held;

    public LevelContainer container {
        get { return c; }
        set {
            c = value;
            if (value != null) {
                sprite.sortingOrder = 0;
            } else {
                sprite.sortingOrder = 0;
            }
        }
    }

    LevelContainer c;
    public SpriteRenderer sprite;

    public float clickRadius = 0.08f;

    public bool createdPortal;
    // Start is called before the first frame update
    void Start() {
        GameManager.i.onSelect.AddListener(TryClick);
        randomTimeStart = Random.value * 100;
        GameManager.i.lights.Add(transform);
        animator.spriteRenderer.color = activeColour;
        fireflies.color = activeColour;

    }

    // Update is called once per frame
    void Update() {

        if (GameManager.i.selectedOrb == this) {
            animator.Play(held);
            transform.localPosition = Vector2.zero;
        } else {
            animator.Play(world);
            Breathe();
        }


    }

    void LateUpdate() {
        fireflies.gameObject.SetActive(createdPortal);
        if (container == null) {
            fireflies.gameObject.SetActive(false);
            return;
        }
        sprite.sortingOrder = container.sortingOrder + 1;

        fireflies.sortingOrder = sprite.sortingOrder - 10;
        LevelContainer nextContainer = null;
        int myContainerIndex = GameManager.i.zoomer.levels.IndexOf(container);

        if (fireflies.transform.parent != null) {
            if (fireflies.transform.parent.GetComponent<LevelContainer>() != null) {
                fireflies.transform.parent.GetComponent<LevelContainer>().RemoveRenderer(fireflies);
            }
            fireflies.transform.SetParent(container.transform);
        }

        if (container != null && myContainerIndex < GameManager.i.zoomer.levels.Count - 1) {
            nextContainer = GameManager.i.zoomer.levels[myContainerIndex + 1];

            if (!nextContainer.HasRenderer(fireflies)) {
                nextContainer.AddRenderer(fireflies);
                fireflies.sortingOrder = nextContainer.sortingOrder + 1;
                fireflies.transform.SetParent(nextContainer.transform);
            }
            fireflies.transform.localPosition = transform.parent.localPosition;
            fireflies.transform.localScale = Vector3.one;

        }

        createdPortal = false;
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
