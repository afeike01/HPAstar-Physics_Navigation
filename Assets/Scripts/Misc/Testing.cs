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
        if (Input.GetMouseButtonDown(1))
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
        }
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Rigidbody newRB = hit.collider.gameObject.GetComponent<Rigidbody>();
                if (newRB)
                {

                    newRB.AddForceAtPosition(-hit.normal*250, hit.point);
                }
            }
        }
	}
    void Execute()
    {
        
    }
    void SpawnUnit()
    {
        Grid startGrid = connectionGrid.gridList[1];
        int index = 461;
        startNode = startGrid.nodeList[index];
        endNode = connectionGrid.gridList[0].nodeList[index];

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
