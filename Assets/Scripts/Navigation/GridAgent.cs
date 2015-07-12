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
    public Grid mainGrid;

    public float distFromNextNode;
    public float switchNodeDist = .5f;

    private Rigidbody rB;

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
        if (okToMove && agentPath.Count > 0)
        {
            currentNode = agentPath[0];
            distFromNextNode = Vector3.Distance(transform.position, currentNode.GetLocation());

            if (rB.isKinematic)
            {
                transform.Translate(Vector3.forward * movementSpeed * Time.deltaTime);

                Vector3 newPosition = currentNode.GetLocation() - transform.position;
                Quaternion newRotation = Quaternion.LookRotation(newPosition);
                transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * rotationSpeed);
            }
            

            if (distFromNextNode < switchNodeDist)
            {
                if (!rB.isKinematic)
                    rB.isKinematic = true;
                lastNode = agentPath[0];
                if (agentPath.Count>1&&lastNode.gridParent != agentPath[1].gridParent)
                    Launch(agentPath[1].GetLocation());
                agentPath.RemoveAt(0);
                if (agentPath.Count < 1)
                {
                    ToggleOkToMove(false);
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
        if (currentNode == null)
        {
            SetCurrentNode();
        }
        Node startNode = mainGrid.LookUpNode(transform.position.x, transform.position.z);
        agentPath = mainGrid.FindComplexPath(startNode, endNode);
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
        agentPath = newPath;
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
            int xLocation = (int)transform.position.x;
            int zLocation = (int)transform.position.z;
            
            currentNode = mainGrid.LookUpNode(xLocation, zLocation);
        }
    }
    public void SetNavigationGrid(Grid newGrid)
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
    private void Launch(Vector3 destination)
    {
        Vector3 launchVelocity = GetLaunchVelocity(transform.position, destination, 2);
        Rigidbody rB = GetComponent<Rigidbody>();
        rB.isKinematic = false;
        GetComponent<Rigidbody>().AddForce(launchVelocity, ForceMode.VelocityChange);
    }
}
