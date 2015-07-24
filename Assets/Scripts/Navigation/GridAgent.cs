using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridAgent : MonoBehaviour 
{

    public float movementSpeed = 5f;
    public float rotationSpeed = 25f;
    public bool okToMove = true;
    public List<Node> agentPath=new List<Node>();
    public Node currentNode;
    public Node lastNode;
    public ConnectionGrid mainGrid;

    public float distFromNextNode;

    public float switchNodeDist = .5f;
    public float moveSwitchDist = 4f;
    public float jumpSwitchDist = .25f;
    public float resetPathDist = 10f;

    private Rigidbody rB;

    public Vector3 currentVelocity;
    public float currentMagnitude;
    public bool isGrounded = true;

    private int navUpdateCounter;
    private int navUpdateTimer = 0;

    private float maxAirTime = 2.5f;
    private float minAirTime = 1.5f;

    private Testing gameManager;
    public float distFromPlayer = 0;

    private int waitTime = 100;
    private int waitCounter = 0;
    private float minDistFromPlayer = 5f;

	void Start () 
    {
        //Initialize();
        
	}
	
	
	void Update () 
    {
        UpdateMovement();
	}
    
    public void Initialize(Testing newGameManager)
    {
        rB = GetComponent<Rigidbody>();
        navUpdateCounter = Random.Range(50, 150);
        gameManager = newGameManager;
    }
    private void UpdateMovement()
    {
        navUpdateTimer++;
           
            
        if (navUpdateTimer >= navUpdateCounter)
        {
            navUpdateTimer = 0;
            Node newDestination = mainGrid.GetPlayerLocation();
            GetPath(newDestination);
        }
        if (transform.position.y < -10)
        {
            DeactivateAgent();
        }
            
        if (rB != null)
        {
            currentVelocity = rB.velocity;
            currentMagnitude = currentVelocity.magnitude;
            if (currentMagnitude <.1f && !isGrounded)
            {
                ToggleIsGrounded(true);
            }
        }
            
        if (okToMove &&agentPath!=null&& agentPath.Count > 0)
        {
            currentNode = agentPath[0];
            distFromNextNode = Vector3.Distance(transform.position, currentNode.GetLocation());

            switchNodeDist = (rB.useGravity&&!isGrounded) ? jumpSwitchDist : moveSwitchDist;

            if (distFromNextNode < switchNodeDist)
            {
                lastNode = agentPath[0];

                //Make GridAgent Jump to another Grid
                if (agentPath.Count > 1 && lastNode.gridParent != agentPath[1].gridParent)
                {
                    waitCounter = 0;
                    float airTime = 2f;// Random.Range(minAirTime, maxAirTime);
                    Launch(agentPath[1].GetLocation(), airTime);
                }
                
                agentPath.RemoveAt(0);
                //GridAgent has reached Destination
                if (agentPath.Count < 1)
                {
                    ToggleOkToMove(false);
                    ResetVelocity();
                    ToggleGravity(true);
                }
            }
            else if (distFromNextNode >= resetPathDist && isGrounded&&currentMagnitude==0)
            {
                //Debug.Log("Need to Find a new Path");
                Node newEndNode = agentPath[agentPath.Count - 1];
                Node startNode = mainGrid.GetNodeFromLocation(transform.position);
                if(startNode!=null&&startNode.gridParent==newEndNode.gridParent)
                    GetPath(newEndNode);
            }
            
            //HandleRotation
            if (isGrounded)
            {
                if (currentNode != null)
                {
                    Vector3 newDirection = ((currentNode.GetLocation() - transform.position).normalized);
                    rB.MovePosition(transform.position + newDirection * movementSpeed * Time.deltaTime);
                    Quaternion newRotation = Quaternion.LookRotation(newDirection);
                    Quaternion newSmoothRotation = Quaternion.Slerp(transform.rotation, newRotation, rotationSpeed * Time.deltaTime);
                    rB.MoveRotation(newSmoothRotation);

                    waitCounter = 0;
                }

            }
        }
        if (gameManager)
        {
            distFromPlayer = Vector3.Distance(transform.position, gameManager.GetPlayerLocation());
            if (distFromPlayer > minDistFromPlayer && currentMagnitude < .05f)
            {
                waitCounter++;
                if (waitCounter >= waitTime)
                {
                    waitCounter = 0;
                    Launch(gameManager.GetPlayerLocation(), 2);
                }
            }
        }
    }
    private void ExecuteMove()
    {
        if(agentPath!=null)
            ToggleOkToMove(true);
        
    }
    public void GetPath(Node endNode)
    {
        if(agentPath!=null)
            agentPath.Clear();
        if (endNode == null)
            return;
        SetCurrentNode();
        Node startNode = mainGrid.GetNodeFromLocation(transform.position);
        if (startNode != null)
            agentPath = mainGrid.FindMultiGridPath(startNode, endNode);
        else
        {
            startNode = mainGrid.GetClosestConnectionNode(transform.position);
            if (startNode != null)
            {
                agentPath = mainGrid.FindMultiGridPath(startNode, endNode);
                //Debug.Log("Location was Out of Bounds.  Found Closest Entrance");
            }
                
            else
            {
                Debug.Log("StartNode is Null");
                return;
            }
        }
        //Grid.VisualizePath(agentPath);
        ExecuteMove();
    }
    public void SetPath(List<Node> newPath)
    {
        agentPath.Clear();
        if (currentNode == null)
        {
            SetCurrentNode();
        }
        agentPath = new List<Node>(newPath);
        ExecuteMove();
    }
    public void SetCurrentNode(Node newNode=null)
    {
        if (newNode!=null)
        {
            currentNode = newNode;
        }
        else
        {
            currentNode = mainGrid.GetNodeFromLocation(transform.position);
        }
    }
    public void SetNavigationGrid(ConnectionGrid newGrid)
    {
        mainGrid = newGrid;
    }
    public bool GetPathStatus()
    {
        if (agentPath.Count > 0)
            return true;
        else
            return false;
    }
    public void ToggleOkToMove(bool newVal)
    {
        okToMove = newVal;
    }
    private Vector3 GetLaunchVelocity(Vector3 origin, Vector3 target, float timeToTarget)
    {

        Vector3 toTarget = target - origin;
        Vector3 toTargetXZ = toTarget;
        toTargetXZ.y = 0;

        float y = toTarget.y;
        float xz = toTargetXZ.magnitude;
        float t = timeToTarget;
        float v0y = y / t + 0.5f * Physics.gravity.magnitude * t;
        float v0xz = xz / t;
        Vector3 result = toTargetXZ.normalized;
        result *= v0xz;
        result.y = v0y;
        return result;
    }
    private void Launch(Vector3 destination, float airTime)
    {
        ResetVelocity();
        Vector3 launchVelocity = GetLaunchVelocity(transform.position, destination, airTime);
        rB.AddForce(launchVelocity, ForceMode.VelocityChange);
        ToggleIsGrounded(false);
    }
    private void ResetVelocity()
    {
        rB.isKinematic = true;
        rB.isKinematic = false;
    }
    private void ToggleGravity(bool newVal)
    {
        rB.useGravity = newVal;
    }
    private void ToggleIsGrounded(bool newVal)
    {
        isGrounded = newVal;
    }
    public void DeactivateAgent(bool addScore = true)
    {
        if (addScore)
        {
            gameManager.AddScore();
            gameManager.SpawnSingleUnit();
        }
        Destroy(this.gameObject);
    }
}
