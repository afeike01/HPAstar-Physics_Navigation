using UnityEngine;
using System.Collections;

public class RigidbodyControl : MonoBehaviour 
{

    private bool activated = false;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    public void ActivateRigidbody()
    {
        if (!activated)
        {
            activated = true;
            Rigidbody rB = GetComponent<Rigidbody>();
            if (rB != null)
            {
                rB.isKinematic = false;
            }
            Destroy(this.gameObject, 3f);
        }
        
    }
}
