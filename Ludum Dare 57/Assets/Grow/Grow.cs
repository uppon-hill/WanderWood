using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Retro;
public class Grow : MonoBehaviour {
    public float responsiveness = 2f;
    public RetroAnimator animator;
    public SpriteRenderer sprite;
    public List<Sheet> sheets;

    float avgLum = 0;
    public float avgSamples = 30;
    int cap;
    // Start is called before the first frame update
    void Start() {
        animator.Play(sheets[Random.Range(0, sheets.Count)]);
        animator.Stop();
        sprite.flipX = Random.value > 0.5f;
        cap = Random.Range(0, 3);
        responsiveness += (Random.value - 0.5f) * 0.1f;
    }

    // Update is called once per frame
    void Update() {

        float lum = GetLuminance();

        //decay average by one nth
        avgLum *= (avgSamples - 1) / avgSamples;
        //add one nth of the current lum
        avgLum += lum * (1 / avgSamples);
        float clampedAvgLum = Mathf.Clamp(avgLum, 0, responsiveness);
        animator.frame = (int)Helpers.Map(clampedAvgLum, 0, responsiveness, 0, animator.mySheet.count - 1 - cap);
    }

    public float GetLuminance() {
        float luminance = 0;
        foreach (Transform light in GameManager.instance.lights) {
            float dist = Vector2.Distance(light.position, transform.position);
            float min = Mathf.Min(responsiveness, dist);
            float lum = responsiveness - min;
            luminance += lum;
        }
        return luminance;
    }
}
