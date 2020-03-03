using UnityEngine;
using System.Collections;

using MeshMelter;

public class Rotate : MonoBehaviour {

	private float startTime;
	public float revolutionTime = 10f;
	public float movementTime = 20f;

	void Start() {
		startTime = Time.realtimeSinceStartup;
	}
	
	void Update () {
		if ((Time.realtimeSinceStartup - startTime) < movementTime) {
			transform.Rotate (Vector3.up * Time.deltaTime * (360f / revolutionTime), Space.World);
		}
	}
}
