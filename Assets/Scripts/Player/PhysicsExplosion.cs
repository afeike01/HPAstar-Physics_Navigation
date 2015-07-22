using UnityEngine;
using System.Collections;

public class PhysicsExplosion : MonoBehaviour 
{
    public float explosionForce = 2000f;
    public float explosionRadius = 20f;
    public float lifeTime = 1.25f;

    private ConnectionGrid mainGrid;

	// Use this for initialization
	void Start () 
    {
        
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    public void Initialize(ConnectionGrid newGrid)
    {
        mainGrid = newGrid;
        Destroy(this.gameObject, lifeTime);
    }
    void OnTriggerEnter(Collider other)
    {
        Rigidbody rB = other.gameObject.GetComponent<Rigidbody>();
        GridAgent newAgent = other.gameObject.GetComponent<GridAgent>();

        if (rB && newAgent)
        {
            rB.AddExplosionForce(explosionForce, transform.position, explosionRadius);
        }
        RigidbodyControl rbControl = other.gameObject.GetComponent<RigidbodyControl>();
        if (rbControl)
        {
            if (mainGrid.DisableNode(rbControl.gameObject.transform.position))
            {
                rbControl.ActivateRigidbody();
            }
        }
    }
}
