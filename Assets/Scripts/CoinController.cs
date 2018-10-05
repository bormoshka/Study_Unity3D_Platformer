using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinController : MonoBehaviour {

	public float rotationSpeed = 100;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		float angle = rotationSpeed * Time.deltaTime;
		transform.Rotate(new Vector3(0,0,angle));
	}
}
