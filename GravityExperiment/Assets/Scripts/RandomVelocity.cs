using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RandomVelocity : MonoBehaviour {

    public float speed = 10f;

	// Use this for initialization
	void Start () {
        Apply();
	}

    public void Apply() {
        GetComponent<Rigidbody>().velocity = Random.insideUnitSphere.normalized * speed;
    }
}
