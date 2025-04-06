using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : MonoBehaviour, IContainable {
    public float speed;
    public float offset;
    float randomTimeStart;
    float t => randomTimeStart + Time.time;
    public LevelContainer container { get; set; }

    float clickRadius = 0.08f;
    // Start is called before the first frame update
    void Start() {
        GameManager.i.pointer.onSelect.AddListener(TryClick);
        randomTimeStart = Random.value * 100;
        GameManager.i.lights.Add(transform);
    }

    // Update is called once per frame
    void Update() {
        Breathe();
    }


    public void SetPosition(Vector2 pos) {
        transform.parent.position = pos;
    }

    void Breathe() {
        transform.localPosition = Helpers.PixelPerfect(Vector2.up * Mathf.Sin(t * speed) * offset);
    }

    void TryClick(Pointer p) {
        bool onLayer = container == null || container.IsCurrentIndex();

        if (Vector2.Distance(p.transform.position, transform.position) < clickRadius && onLayer) {
            GameManager.i.SelectOrb(this);
        }
    }

}
