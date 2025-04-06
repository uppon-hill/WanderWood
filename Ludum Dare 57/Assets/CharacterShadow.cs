using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterShadow : MonoBehaviour, IContainable {
    public LevelContainer container { get; set; }
    public SpriteRenderer sprite;
    public BoxCollider2D trigger;
    // Start is called before the first frame update
    void Start() {
    }

    // Update is called once per frame
    void Update() {
        sprite.sortingOrder = container.sortingOrder;
        transform.localPosition = GameManager.i.character.transform.localPosition;
    }

    void LateUpdate() {
        sprite.enabled = GameManager.i.zoomer.levels.IndexOf(container) == GameManager.i.zoomer.current + 1;
        sprite.color = Helpers.AssignAlpha(sprite.color, 0.5f);
    }

    public bool OverlapsCollider() {
        List<Collider2D> colliders = new List<Collider2D>();
        Physics2D.OverlapArea(trigger.bounds.min, trigger.bounds.max, new ContactFilter2D(), colliders);
        return colliders.Contains(container.geometry);
    }

}
