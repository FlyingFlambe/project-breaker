using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionController : MonoBehaviour {

    public PlayerController player;
    public EnemyController enemy;

    public Transform explosion;

	// Use this for initialization
	void Start () {
        player = GetComponent<PlayerController>();
        enemy = GetComponent<EnemyController>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Animate()
    {
        Instantiate(explosion, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
    }
}
