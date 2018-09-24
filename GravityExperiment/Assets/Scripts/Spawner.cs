using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

    public GameObject gravityObjectPrefab;
    public float radius = 1;
    public int numGroups = 1;

    public int groupParticles = 100;
    public float groupRadius = 100f;
    public float spawnVelocity = 0f;

    public float negativeRatio = 1;
    public float neutralRatio = 1;
    public float positiveRatio = 1;

    private List<GameObject> groups = new List<GameObject>();

    // Use this for initialization
    void Start() {
        Respawn();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            Respawn();
        }
    }

    public void Respawn() {
        //Recycle or create groups
        for (int i = 0; i < numGroups; i++) {
            GameObject group;
            GravityObjectSpawner spawner;
            if (groups.Count > i) {
                group = groups[i];
                spawner = group.GetComponent<GravityObjectSpawner>();
            } else {
                group = new GameObject("Group_" + i);
                groups.Add(group);
                spawner = group.AddComponent<GravityObjectSpawner>();
            }
            
            //position group
            group.transform.position = (numGroups > 1) ? transform.position + Random.insideUnitSphere * radius : transform.position;
            group.transform.parent = transform;
            
            //group settings
            spawner.prefab = gravityObjectPrefab;
            spawner.n = groupParticles;
            spawner.radius = groupRadius;
            spawner.particleInitialVelocity = spawnVelocity;
            spawner.negativeRatio = negativeRatio;
            spawner.neutralRatio = neutralRatio;
            spawner.positiveRatio = positiveRatio;
            spawner.Respawn();
        }

        //Dispose of extra groups
        if (numGroups < groups.Count) {
            for (int i = groups.Count - 1; i >= numGroups; i--) {
                Destroy(groups[i].gameObject);
            }
            groups.RemoveRange(numGroups, groups.Count - numGroups);
        }
        
    }

    
}
