using UnityEngine;
using System.Collections;

public class HideGameObject : MonoBehaviour 
{

	// Use this for initialization
	void Start () 
    {
        Renderer newRenderer = GetComponent<Renderer>();
        if(newRenderer!=null)
        {
            newRenderer.enabled = false;
        }
	}
	
	
    
}
