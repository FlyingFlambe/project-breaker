using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    [SerializeField]
    private GameObject[] BorderParent;
    public GameObject[] enemy;

    //
    void Awake()
    {
        foreach (GameObject border in BorderParent)
            Instantiate(border);   
    }

	// Use this for initialization
	void Start () {
    }
	
	// Update is called once per frame
	void Update () {
        Scene loadedLevel = SceneManager.GetActiveScene();

        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(loadedLevel.buildIndex);
    }
}
