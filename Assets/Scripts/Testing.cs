using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Testing : MonoBehaviour 
{

    public ConnectionGrid connectionGrid;

    public Vector2 moveToLocation;
    public GameObject unitPrefab;

    private GridAgent myAgent;
    private List<Node> myPath = new List<Node>();

    private Node startNode;
    private Node endNode;

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
            MoveUnit();
            //Execute();
        }
	}
    void Execute()
    {
        
    }
    void SpawnUnit()
    {
        

        startNode = connectionGrid.gridList[1].nodeList[556];
        endNode = connectionGrid.gridList[2].nodeList[546];

        GameObject newUnit = Instantiate(unitPrefab, startNode.GetLocation(), Quaternion.identity) as GameObject;
        myAgent = newUnit.GetComponent<GridAgent>();
        myAgent.SetNavigationGrid(startNode.gridParent);
    }
    void MoveUnit()
    {
        if (myAgent == null)
            return;

        List<Node> newPath = connectionGrid.FindMultiGridPath(startNode, endNode);
        Grid.VisualizePath(newPath);

        myAgent.SetPath(newPath);

        

    }
}
