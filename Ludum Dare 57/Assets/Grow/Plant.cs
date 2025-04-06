using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Retro;
using UnityEngine.UI;
public class Plant : MonoBehaviour, IContainable {

    public Color activeColour;
    public LevelContainer container { get; set; }
    public float responsiveness = 2f;
    public RetroAnimator animator;
    public SpriteRenderer sprite;
    public List<Sheet> sheets;

    float avgLum = 0;
    public float avgSamples = 30;
    int cap;

    int maxFrame => animator.mySheet.count - 1 - cap;

    public SpriteMask fogMask;

    // Start is called before the first frame update
    void Start() {
        animator.Play(sheets[Random.Range(0, sheets.Count)]);
        animator.Stop();
        sprite.flipX = Random.value > 0.5f;
        cap = Random.Range(0, 3);
        responsiveness += (Random.value - 0.5f) * 0.1f;

        container.plants.Add(this);
        animator.spriteRenderer.color = activeColour;
    }

    // Update is called once per frame
    void Update() {
        // if (!container.IsCurrentIndex()) return;

        fogMask.frontSortingOrder = container.sortingOrder + 1;
        fogMask.backSortingOrder = container.sortingOrder - 4;
        float lum = GetLuminance();

        //decay average by one nth
        avgLum *= (avgSamples - 1) / avgSamples;
        //add one nth of the current lum
        avgLum += lum * (1 / avgSamples);
        float clampedAvgLum = Mathf.Clamp(avgLum, 0, responsiveness);
        animator.frame = (int)Helpers.Map(clampedAvgLum, 0, responsiveness, 0, maxFrame);

        fogMask.transform.localScale = Vector2.one * avgLum * 4f;

    }

    public bool IsInLuminanceRadius(Vector2 position) {
        return Vector2.Distance(transform.position, position) < fogMask.transform.localScale.x / 2f;
    }

    public float GetLuminance() {
        float luminance = 0;
        foreach (Transform light in GameManager.i.lights) {
            if (light.GetComponent<IContainable>() is Orb o) {
                if (o.activeColour == activeColour) {
                    bool playerCarryingOrb = o == GameManager.i.selectedOrb;
                    bool onThisLevel = GameManager.i.zoomer.currentLevel == container;
                    //if the light is not on this level
                    if (o.container == container || playerCarryingOrb && onThisLevel) {
                        float dist = Vector2.Distance(light.position, transform.position);
                        float min = Mathf.Min(responsiveness, dist);
                        float lum = responsiveness - min;
                        luminance += lum;
                    }
                }
            }
        }
        return luminance;
    }
}
