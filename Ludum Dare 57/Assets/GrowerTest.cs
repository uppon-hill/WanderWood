using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrowerTest : MonoBehaviour {
    public GameObject growerPrefab;
    public Vector2Int gridSize = new Vector2Int(10, 10);
    public float cellsize = 0.32f;
    // Start is called before the first frame update
    void Start() {
        //spawn a grid of grower prefabs
        for (int x = 0; x < gridSize.x; x++) {
            for (int y = 0; y < gridSize.y; y++) {
                Vector3 position = new Vector3(x * cellsize, y * cellsize, 0);
                position += (Vector3)Helpers.PixelPerfect((Vector3.right * Random.value * 0.08f));
                if (Random.value > 0.5f) {
                    position += Vector3.up * 0.02f;
                }
                GameObject grower = Instantiate(growerPrefab, transform.position + position, Quaternion.identity);
                grower.transform.parent = transform;
            }
        }
    }

    // Update is called once per frame
    void Update() {

    }


}
