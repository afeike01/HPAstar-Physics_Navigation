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

    //private Unit unitComponent;

    //private int surroundingNodeIndex = 0;
	// Use this for initialization
	void Start () 
    {
        Initialize();
	}
	
	
	void Update () 
    {
        UpdateMovement();
            
	}
    
    private void Initialize()
    {
        rB = GetComponent<Rigidbody>();
        
    }
    private void UpdateMovement()
    {
        if (transform.position.y < -10)
        {
            Destroy(this.gameObject);
        }
            

        if (rB != null)
        {
            currentVelocity = rB.velocity;
            currentMagnitude = currentVelocity.magnitude;
            if (currentMagnitude == 0 && !isGrounded)
            {
                ToggleIsGrounded(true);
            }
        }
            
        if (okToMove && agentPath.Count > 0)
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
                    /*if (!rB.useGravity)
                        ToggleGravity(true);*/
                    ResetVelocity();
                    Launch(agentPath[1].GetLocation(), 2);
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
            if (isGrounded)
            {
                if (currentNode != null)
                {
                    Vector3 newDirection = ((currentNode.GetLocation() - transform.position).normalized);
                    rB.MovePosition(transform.position + newDirection * movementSpeed * Time.deltaTime);
                    Quaternion newRotation = Quaternion.LookRotation(newDirection);
                    Quaternion newSmoothRotation = Quaternion.Slerp(transform.rotation, newRotation, rotationSpeed * Time.deltaTime);
                    rB.MoveRotation(newSmoothRotation);
                }
                
            }
            
        }
    }
    private void ExecuteMove()
    {
        ToggleOkToMove(true);
        
    }
    public void GetPath(Node endNode)
    {
        agentPath.Clear();
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
                Debug.Log("Location was Out of Bounds.  Found Closest Entrance");
            }
                
            else
            {
                Debug.Log("StartNode is Null");
                return;
            }
        }
        Grid.VisualizePath(agentPath);
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
}
