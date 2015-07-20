using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public enum NodeType : int
{
    Temporary =-1,
    Normal =0,
    Abstract = 1,
}
public class Node : IComparable<Node>
{
    public int xVal=0;
    public float yVal = 0;
    public int zVal=0;
    public bool available = true;

    public int nodeNum;

    public List<Node> neighbors = new List<Node>();
    public float f = 0;
    public float g = 0;
    public float h = 0;
    public Node previouseNode;
    public bool visited = false;

    public Grid gridParent;
    public NodeCluster clusterParent;

    public Grid gridConnectingTo;
    public Node nodeConnectingTo;

    public NodeType level;

    private bool isTemporary = false;
    private bool isPermanent = false;


    public Node(Grid newGrid, int newX, float newY, int newZ, NodeType nodeLevel,int num)
    {
        this.gridParent = newGrid;
        this.level = nodeLevel;
        this.xVal = newX;
        this.yVal = newY;
        this.zVal = newZ;
        this.nodeNum = num;
    }
    public int CompareTo(Node newNode)
    {
        if (this.f > newNode.f)
            return 1;
        if (this.f < newNode.f)
            return -1;
        else
            return 0;
    }
    public void AddNeighbor(Node newNeighbor)
    {
        if(newNeighbor!=null&&!neighbors.Contains(newNeighbor))
        {
            if (IsAbstract())
            {
                neighbors.Add(newNeighbor);
            }
            else
            {
                //Not a ClusterEntrance, Neighbor must be within Node's Cluster
                if (newNeighbor.clusterParent == clusterParent)
                {
                    neighbors.Add(newNeighbor);
                }
            }
            
        }
    }
    
    public Vector3 GetLocation()
    {
        return new Vector3(xVal, yVal, zVal);
    }
    public void ToggleVisited(bool newVal)
    {
        visited = newVal;
    }
    public void ToggleAvailable(bool newVal)
    {
        available = newVal;
        
    }
    public void SetF()
    {
        f = g + h;
    }
    public void SetG(float newG)
    {
        g = newG;
    }
    public void SetH(Node goalNode)
    {
        
        Vector3 start = new Vector3(xVal, yVal, zVal);
        Vector3 end = new Vector3(goalNode.xVal, goalNode.yVal, goalNode.zVal);
        h = Vector3.Distance(start, end);
    }
    public void Reset()
    {
        ToggleVisited(false);
        f = 0;
        g = 0;
        h = 0;
    }
    public void AssignPreviouseNode(Node newNode)
    {
        previouseNode = newNode;
    }
    public bool IsAbstract()
    {
        return level == NodeType.Abstract;
    }
    public bool IsTemporary()
    {
        return level == NodeType.Temporary;
    }
    //Used for Nodes in the ConnectionGrid
    public void SetClusterParent(NodeCluster newNodeCluster)
    {
        clusterParent = newNodeCluster;
    }
    public void SetConnection(Node newNode)
    {
        nodeConnectingTo = newNode;
        gridConnectingTo = newNode.gridParent;
    }
    public bool IsPermanent()
    {
        return isPermanent;
    }
    public void SetPermanent(bool newVal)
    {
        isPermanent = newVal;
    }
}
