using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class CloudLayer : MonoBehaviour, IContainable {

    public LevelContainer container { get; set; }
    public SpriteRenderer cloudSprite;

    List<SpriteRenderer> sprites = new List<SpriteRenderer>();
    List<Transform> transforms = new List<Transform>();
    public Vector3 size = new Vector3(4.8f, 2.7f, 0);
    public float cloudsPerUnit = 5f;
    public float noiseScale = 0.1f;
    public float noiseSpeed = 1f;
    public float baseSize = 0.4f;
    // Start is called before the first frame update
    void Start() {
        //instantiate a cluster of clouds in an area
        for (int x = 0; x < size.x * cloudsPerUnit; x++) {
            for (int y = 0; y < size.y * cloudsPerUnit; y++) {
                Vector3 position = new Vector3(x / cloudsPerUnit, y / cloudsPerUnit, 0);
                SpriteRenderer cloud = Instantiate(cloudSprite, transform.position - (size / 2f) + position, Quaternion.identity);
                cloud.gameObject.SetActive(true);
                cloud.transform.parent = transform;
                sprites.Add(cloud);
                transforms.Add(cloud.transform);
                cloud.color = container.layerColor;
                cloud.transform.localScale = Vector2.one * Random.Range(baseSize * 0.5f, baseSize * 2);
            }
        }
    }

    // Update is called once per frame
    void Update() {
        //move the clouds in a random direction based on noise and using time.deltatime
        for (int i = 0; i < transforms.Count; i++) {
            Vector3 pos = transforms[i].position;
            float tOffset = i * 100;
            float t = Time.time + tOffset * noiseSpeed;
            float x = (Mathf.PerlinNoise(pos.x, t) - 0.5f) * noiseScale;
            float y = (Mathf.PerlinNoise(pos.y, t) - 0.5f) * noiseScale;
            transforms[i].position += new Vector3(x, y, 0) * Time.deltaTime * 0.01f;

        }
    }
}
