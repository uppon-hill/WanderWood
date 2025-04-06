using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceSensor : MonoBehaviour {

    // Start is called before the first frame update
    void Start() {
        GameManager.i.zoomer.anim.finished.AddListener(RespondToFinished);
    }

    // Update is called once per frame
    void Update() {

    }
    public void OnTriggerEnter2D(Collider2D other) {
        bool isGround = other.gameObject.layer == LayerMask.NameToLayer("Ground");
        bool notAnimating = !GameManager.i.zoomer.anim.animating;
        bool sameLevelLayer = GameManager.i.zoomer.currentLevel.geometry == other;

        if (isGround && notAnimating && sameLevelLayer) {
            //GameManager.i.Bounce();
            //  GetComponent<Collider2D>().enabled = false;
        }

        GameManager.i.character.body.velocity = Vector2.zero;

    }
    public void RespondToFinished(SimpleAnimation a) {
        GetComponent<Collider2D>().enabled = true;

    }
}
