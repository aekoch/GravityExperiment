using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityObjectSpawner : MonoBehaviour {

    public GameObject prefab;
    public float radius = 20f;
    public int n = 50;
    public float particleInitialVelocity = 0f;

    public float negativeRatio = 1;
    public float neutralRatio = 1;
    public float positiveRatio = 1;
    
    private List<GravityObject> gravityObjects = new List<GravityObject>();

    private void Awake() {
        Respawn();
    }

    private GravityObject.Charge RandomCharge() {

        float ratioDivisor = (negativeRatio + neutralRatio + positiveRatio);
        float negativeNeutralPoint = (float)negativeRatio / ratioDivisor;
        float neutralPositivePoint = negativeNeutralPoint + (float)neutralRatio / ratioDivisor;

        float rand = Random.value;
        if (rand < negativeNeutralPoint) {
            return GravityObject.Charge.Negative;
        } else if(rand < neutralPositivePoint) {
            return GravityObject.Charge.Neutral;
        } else {
            return GravityObject.Charge.Positive;
        }
    }

    public void Respawn() {
        if(gravityObjects == null) {
            gravityObjects = new List<GravityObject>();
        }
        for (int i = 0; i < n; i++) {
            GameObject obj;
            GravityObject settings;

            if(gravityObjects.Count > i) {
                obj = gravityObjects[i].gameObject;
                settings = gravityObjects[i];
            } else {
                obj = Instantiate(prefab, transform);
                settings = obj.GetComponent<GravityObject>();
                gravityObjects.Add(settings);
            }
            //obj = Instantiate(prefab, transform);
            settings = obj.GetComponent<GravityObject>();
            obj.transform.position = transform.position + Random.insideUnitSphere * radius;

            settings.charge = RandomCharge();
            //settings.mass = Mathf.Pow(2, (5 * Random.value + 5));
            settings.mass = 100f;
            //settings.density = Random.value * 5 + 1;
            settings.density = 3;
            settings.GetComponent<RandomVelocity>().speed = particleInitialVelocity;
            settings.GetComponent<RandomVelocity>().Apply();
        }

        for(int i = gravityObjects.Count - 1; i >= n; i--) {
            Destroy(gravityObjects[i].gameObject);
            gravityObjects.RemoveAt(i);
        }
    }
}
