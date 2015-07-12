using UnityEngine;
using System.Collections;

public class DelayDestroy : MonoBehaviour 
{
    public float lifeTime = 3f;

	// Use this for initialization
	void Start () 
    {
        Destroy(this.gameObject, lifeTime);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
