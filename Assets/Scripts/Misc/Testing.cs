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

    public GameObject physicsExplosionPrefab;

    public int score = 0;
    private int spawnRate = 1000;
    private int spawnRateCounter = 1000;

    private bool gameOver = false;

	// Use this for initialization
	void Start () 
    {
	    
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            SpawnMultiUnits();
        }
        if (!gameOver)
        {
            spawnRateCounter++;
            if (spawnRateCounter >= spawnRate)
            {
                spawnRateCounter = 0;
                SpawnMultiUnits();
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.LoadLevel(0);
        }
        
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
    
    public Node GetPlayerNodeLocation()
    {
        Vector3 playerLocation = player.gameObject.transform.position;
        Node newNode = connectionGrid.GetNodeFromLocation(playerLocation);
        return newNode;
    }
    public Vector3 GetPlayerLocation()
    {
        return player.transform.position;
    }
    public void SpawnMultiUnits()
    {
        Grid startGrid = connectionGrid.gridList[1];
        Vector3 offset = new Vector3(0,5,0);

        for (int i = 0; i < agentCount; i++)
        {
            int randomIndex = Random.Range(0, startGrid.permanentNodes.Count);
            Vector3 spawnLocation = startGrid.permanentNodes[randomIndex].GetLocation() + offset;

            GameObject newUnit = Instantiate(unitPrefab, spawnLocation, Quaternion.identity) as GameObject;
            GridAgent newAgent = newUnit.GetComponent<GridAgent>();
            newAgent.SetNavigationGrid(connectionGrid);
            newAgent.Initialize(this);
            //myAgents.Add(newAgent);
        }
        

    }
    public void SpawnSingleUnit()
    {
        Grid startGrid = connectionGrid.gridList[1];
        Vector3 offset = new Vector3(0, 5, 0);

        int randomIndex = Random.Range(0, startGrid.permanentNodes.Count);
        Vector3 spawnLocation = startGrid.permanentNodes[randomIndex].GetLocation() + offset;

        GameObject newUnit = Instantiate(unitPrefab, spawnLocation, Quaternion.identity) as GameObject;
        GridAgent newAgent = newUnit.GetComponent<GridAgent>();
        newAgent.SetNavigationGrid(connectionGrid);
        newAgent.Initialize(this);
        //myAgents.Add(newAgent);
    }
    public void AddScore()
    {
        score++;
        //Debug.Log("Score: " + score);
    }
    public void EndGame()
    {
        gameOver = true;
        Debug.Log("GameOver");
    }
}
