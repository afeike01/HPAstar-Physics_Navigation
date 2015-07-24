using UnityEngine;
using System.Collections;

public class Ground : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    void OnCollisionEnter(Collision other)
    {
        /*GridAgent newAgent = other.gameObject.GetComponent<GridAgent>();
        if (newAgent)
        {
            newAgent.DeactivateAgent();
        }*/
        Testing newManager = other.gameObject.GetComponentInChildren<Testing>();
        if (newManager)
        {
            newManager.EndGame();
        }
    }
}
