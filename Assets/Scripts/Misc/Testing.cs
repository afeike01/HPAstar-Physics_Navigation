using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Testing : MonoBehaviour 
{

    public ConnectionGrid connectionGrid;

    public Vector2 moveToLocation;
    public GameObject unitPrefab;

    private int agentCount = 25;
    private List<GridAgent> myAgents = new List<GridAgent>();
    private List<Node> myPath = new List<Node>();

    private Node startNode;
    private Node endNode;
   

    private bool direction = true;

    public GameObject player;

    public Transform targetTransform;
    private Vector3 target;
    private float x = 0.0f;
    private float y = 0.0f;
    public float xSpeed = 250.0f;
    public float ySpeed = 120.0f;
    public float yMinLimit = -20;
    public float yMaxLimit = 80;
    public float distance = 10.0f;

    public float XSensitivity = 2f;
    public float YSensitivity = 2f;
    private Quaternion m_CharacterTargetRot;
    private Quaternion m_CameraTargetRot;
    public bool clampVerticalRotation = true;
    public float MinimumX = -90F;
    public float MaximumX = 90F;
    public bool smooth;
    public float smoothTime = 5f;

    public GameObject physicsExplosionPrefab;

	// Use this for initialization
	void Start () 
    {
	    
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            SpawnUnit();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.LoadLevel(0);
        }
        /*if (Input.GetMouseButtonDown(1))
        {
            Vector3 playerLocation = player.gameObject.transform.position;
            Node newNode = connectionGrid.GetNodeFromLocation(playerLocation);
            if (newNode != null)
                MoveUnits(newNode);
            else
            {
                newNode = connectionGrid.GetClosestConnectionNode(playerLocation);
                if (newNode != null)
                    MoveUnits(newNode);
            }
        }*/
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Rigidbody newRB = hit.collider.gameObject.GetComponent<Rigidbody>();
                RigidbodyControl rBC = hit.collider.gameObject.GetComponent<RigidbodyControl>();
                if (newRB)
                {

                    newRB.AddForceAtPosition(-hit.normal*250, hit.point);
                }
                if (rBC)
                {
                    if (connectionGrid.DisableNode(hit.collider.gameObject.transform.position))
                    {
                        rBC.ActivateRigidbody();
                    }
                }
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            GameObject newExplosion = Instantiate(physicsExplosionPrefab, player.transform.position, Quaternion.identity) as GameObject;
            PhysicsExplosion newPExplosion = newExplosion.GetComponent<PhysicsExplosion>();
            newPExplosion.Initialize(connectionGrid);
        }
	}
    
    public Node GetPlayerLocation()
    {
        Vector3 playerLocation = player.gameObject.transform.position;
        Node newNode = connectionGrid.GetNodeFromLocation(playerLocation);
        return newNode;
    }
    void SpawnUnit()
    {
        Grid startGrid = connectionGrid.gridList[1];
        startNode = startGrid.LookUpNode(startGrid.transform.position.x + (startGrid.gridSize / 2), startGrid.transform.position.z + (startGrid.gridSize / 2));

        float range = 10f;

        for (int i = 0; i < agentCount; i++)
        {
            Vector3 initialLocation = startNode.GetLocation();
            Vector3 offset = new Vector3(Random.Range(-range, range), 1, Random.Range(-range, range));
            Vector3 spawnLocation = initialLocation + offset;
            GameObject newUnit = Instantiate(unitPrefab, spawnLocation, Quaternion.identity) as GameObject;
            GridAgent newAgent = newUnit.GetComponent<GridAgent>();
            newAgent.SetNavigationGrid(connectionGrid);
            myAgents.Add(newAgent);
        }
        

    }
    void MoveUnits(Node newEndNode)
    {
        for (int i = 0; i < myAgents.Count; i++)
        {
            if (myAgents[i] != null)
            {
                myAgents[i].GetPath(newEndNode);
            }
        }
    }
}
