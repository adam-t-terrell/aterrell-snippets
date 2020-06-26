using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {
    [Header ("SPAWN")]
    public GameObject reference;

    [Header("SPAWNING")]
    public bool infinite;
    public float ratePerSecond = 3.0f;
    public int spawnNumber = 50;

    [Header("VELOCITY")]
    [Range(0f, 10f)] public float minStrength = 1f;
    [Range(0f, 10f)] public float maxStrength = 10f;

    private int remainingToSpawn;

    // Use this for initialization
    void Start () {
        remainingToSpawn = spawnNumber;
        StartCoroutine(Spawn());
    }

    private IEnumerator Spawn()
    {
        while(infinite || remainingToSpawn > 0)
        {
            float scale = Mathf.Round(((spawnNumber - remainingToSpawn) * 0.02f) * 10.0f)/10.0f;

            Debug.Log("Scale=" + scale.ToString());
            float pos = Random.Range(-5.0f, 5.0f);
            Vector3 newPosition = transform.position + new Vector3(pos,0,0);
            GameObject newObject = Instantiate(reference, newPosition, transform.rotation);
            newObject.transform.localScale += new Vector3(scale, scale, 0);
            Rigidbody rb = newObject.GetComponent<Rigidbody>();
            if (rb)
            {
                Vector3 direction = new Vector3(
                    -pos/10,
                    -1.0f,
                    0);
                direction *= Random.Range(minStrength, maxStrength);
                Forces forces = newObject.GetComponent<Forces>();
                forces.force = direction;
            }
            remainingToSpawn--;

            yield return new WaitForSeconds(ratePerSecond);
        }
    }
}
