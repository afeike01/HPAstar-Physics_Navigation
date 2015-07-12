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
            MoveUnit();
            //Execute();
        }
	}
    void Execute()
    {
        
    }
    void SpawnUnit()
    {

    }
    void MoveUnit()
    {
        


        

        if (myAgent == null)
        {
            Grid startGrid = connectionGrid.gridList[1];
            int index = 461;

            startNode = startGrid.nodeList[index];
            endNode = connectionGrid.gridList[0].nodeList[index];

            GameObject newUnit = Instantiate(unitPrefab, startNode.GetLocation(), Quaternion.identity) as GameObject;
            myAgent = newUnit.GetComponent<GridAgent>();
            myAgent.SetNavigationGrid(startNode.gridParent);
            myAgent.SetCurrentNode(startNode);
        }

        myPath = connectionGrid.FindMultiGridPath(startNode, endNode);

        Grid.VisualizePath(myPath);
        myAgent.SetPath(myPath);


        Grid newEndGrid = startNode.gridParent;
        startNode = endNode;
        int newIndex = Random.Range(0, newEndGrid.nodeList.Count - 1);
        endNode = newEndGrid.nodeList[newIndex];
        
        
        direction = !direction;

        

    }
}
