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
        if (Input.GetKeyDown(KeyCode.Space))
        {

        }
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.GetComponent<MeshCollider>())
                {
                    Vector3 newLocation = hit.point;
                    Node newNode = connectionGrid.GetNodeFromLocation(newLocation);
                    if(newNode!=null)
                        MoveUnits(newNode);
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
