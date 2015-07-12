using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConnectionGrid : MonoBehaviour 
{
    public List<Node> nodeList = new List<Node>();
    public List<Grid> gridList = new List<Grid>();
    public Hashtable storedPaths = new Hashtable();
    public BinaryHeap<Node> frontierHeap = new BinaryHeap<Node>();
    public Hashtable nodeHash = new Hashtable();

    private float gDist = 0;
    public float gDistInc = 1f;

    int nodeCounter = 0;
    public List<Grid> gridsInScene = new List<Grid>();

	// Use this for initialization
	void Start () 
    {
	
	}
    private void Initialize()
    {
        SetConnectionNodeNeighbors();
        SetConnections();

        //Setting startNode to: gridList[2].nodeList[250] caused as Error
        //CHECK THIS OUT

        /*Node startNode = gridList[2].nodeList[401];
        Node endNode = gridList[1].nodeList[131];
        List<Node> newPath = FindMultiGridPath(startNode, endNode);
        Grid.VisualizePath(newPath);*/
    }
    
    public Node LookUpNode(int newX, int newZ)
    {
        int nodeKey = Grid.GetNodeKey(newX, newZ);
        if (nodeHash.Contains(nodeKey))
        {
            return nodeHash[nodeKey] as Node;
        }
        else
        {
            return null;
        }
    }
    public void ManageNodeList(Node refNode, bool add =true)
    {
        if (refNode != null)
        {
            if (add && !refNode.IsTemporary())
            {

                bool isValid = true;
                Node tempNode = LookUpNode(refNode.xVal, refNode.zVal);
                if (tempNode != null)
                    isValid = false;

                if (isValid)
                {
                    //Create a new Node
                    //Add to NodeList
                    Node newNode = new Node(refNode.gridParent, refNode.xVal, refNode.yVal, refNode.zVal, NodeType.Abstract, nodeCounter);
                    newNode.nodeConnectingTo = refNode.nodeConnectingTo;
                    nodeCounter++;
                    nodeList.Add(newNode);
                    int newKey = Grid.GetNodeKey(newNode);
                    nodeHash.Add(newKey, newNode);

                    
                }

            }
            else if (add && refNode.IsTemporary())
            {
                //Debug.Log("Temporary Node Inserted");
                //If an Inserted Node
                if (!nodeList.Contains(refNode))
                {
                    nodeList.Add(refNode);
                }
            }
            else
            {
                //If Remove
                nodeList.Remove(refNode);
                if (refNode.IsTemporary())
                    refNode = null;
            }
        }
        
    }
    public void SetConnectionNodeNeighbors()
    {
        for (int i = 0; i < nodeList.Count; i++)
        {
            Node newNode = nodeList[i];
            newNode.AddNeighbor(this.LookUpNode(newNode.nodeConnectingTo.xVal, newNode.nodeConnectingTo.zVal));
            for (int j = 0; j < nodeList.Count; j++)
            {
                Node newNode2 = nodeList[j];
                if (newNode != newNode2&&newNode.gridParent == newNode2.gridParent)
                {
                    newNode.AddNeighbor(newNode2);
                    
                }
            }
        }
    }
    public int GetConnectionKey(Node startNode, Node endNode)
    {
        return startNode.nodeNum + (endNode.nodeNum * 1000);
    }
    public List<Node> GetStoredPath(Node startNode, Node endNode)
    {
        int connectionKey = GetConnectionKey(startNode, endNode);
        List<Node> newPath = storedPaths[connectionKey] as List<Node>;
        return newPath;
    }
    public void StorePath(Node startNode, Node endNode)
    {
        if (startNode.gridParent != endNode.gridParent)
            return;

        List<Node> newPath = startNode.gridParent.abstractGrid.FindAbstractPath(startNode, endNode);
        int connectionKey = GetConnectionKey(startNode, endNode);
        if (!storedPaths.Contains(connectionKey) && newPath != null)
        {
            storedPaths.Add(connectionKey, newPath);
        }
    }
    private void SetConnections()
    {
        for (int i = 0; i < gridList.Count; i++)
        {
            for (int j = 0; j < gridList[i].connectionNodes.Count; j++)
            {
                Node startNode = gridList[i].connectionNodes[j];
                for (int k = 0; k < gridList[i].connectionNodes.Count; k++)
                {
                    Node endNode = gridList[i].connectionNodes[k];
                    if (startNode != endNode)
                    {
                        StorePath(startNode, endNode);
                    }
                }
            }
        }
    }
    public List<Node> FindMultiGridPath(Node startNode, Node endNode)
    {
        //This won't work correctly if startNode and endNode have the same gridParent
        List<Node> newConnectionPath = FindConnectionPath(startNode, endNode);
        
        List<Node> newPath = new List<Node>();
        for (int i = 0; i < newConnectionPath.Count - 1; i++)
        {
            Node sNode = newConnectionPath[i];
            Node eNode = newConnectionPath[i + 1];
            if (sNode.gridParent == eNode.gridParent)
            {
                Grid newGrid = sNode.gridParent;

                sNode = newGrid.LookUpNode(sNode.xVal, sNode.zVal);
                eNode = newGrid.LookUpNode(eNode.xVal, eNode.zVal);

                List<Node> addedPath = newGrid.FindComplexPath(sNode, eNode);
                if (addedPath != null)
                    newPath.AddRange(addedPath);
                else
                    Debug.Log("Did not add Null Path");
            }
        }
        return newPath;
    }
    public List<Node> FindConnectionPath(Node sNode, Node eNode)
    {


        bool startNodeInserted = (this.LookUpNode(sNode.xVal, sNode.zVal) == null);
        bool endNodeInserted = (this.LookUpNode(eNode.xVal, eNode.zVal) == null);

        Node startNode = startNodeInserted ? this.InsertNode(sNode) : this.LookUpNode(sNode.xVal, sNode.zVal);
        Node endNode = endNodeInserted ? this.InsertNode(eNode) : this.LookUpNode(eNode.xVal, eNode.zVal);

        frontierHeap.Add(startNode);
        gDist = 0;

        while (frontierHeap.Count > 0)
        {

            Node currentNode = frontierHeap.Remove();
            if (currentNode.visited)
                break;
            currentNode.ToggleVisited(true);
            if (currentNode == endNode)
                break;
            gDist += gDistInc;
            for (int i = 0; i < currentNode.neighbors.Count; ++i)
            {
                if (currentNode.neighbors[i].visited != true && currentNode.neighbors[i].available)
                {
                    currentNode.neighbors[i].AssignPreviouseNode(currentNode);
                    currentNode.neighbors[i].SetG(gDist);
                    currentNode.neighbors[i].SetH(endNode);
                    currentNode.neighbors[i].SetF();
                    frontierHeap.Add(currentNode.neighbors[i]);
                }
            }

        }
        //===================================================
        //Back track through nodes
        //===================================================
        bool pathExists = endNode.visited;
        if (pathExists)
        {
            Node curNode = endNode;
            List<Node> newPath = new List<Node>();
            //Debug.Log("PathSucceeded: " + endNode.visited);

            while (curNode != startNode)
            {
                //pathCost += curNode.f;
                //Node newNode = mainGrid.LookUpNode(curNode.xVal, curNode.zVal);
                newPath.Add(curNode);
                curNode = curNode.previouseNode;
            }

            //Node lastNode = mainGrid.LookUpNode(curNode.xVal, curNode.zVal);
            newPath.Add(curNode);
            newPath.Reverse();

            if (startNodeInserted)
                RemoveNode(startNode);
            if (endNodeInserted)
                RemoveNode(endNode);
            ResetConnectionGrid();

            return newPath;
        }
        else
        {
            Debug.Log("Connection Path is NULL");

            if (startNodeInserted)
            {
                RemoveNode(startNode);
            }

            if (endNodeInserted)
            {
                RemoveNode(endNode);
            }
                
            ResetConnectionGrid();

            return null;

        }
    }
    public void ResetConnectionGrid()
    {
        for (int i = 0; i < nodeList.Count; i++)
        {
            nodeList[i].Reset();
        }
        frontierHeap.Clear();
    }
    public Node InsertNode(Node refNode)
    {
        Node newNode = new Node(refNode.gridParent, refNode.xVal, refNode.yVal, refNode.zVal, NodeType.Temporary, -1);
        for (int i = 0; i < newNode.gridParent.connectionNodes.Count; i++)
        {
            Node newNeighbor = LookUpNode(newNode.gridParent.connectionNodes[i].xVal, newNode.gridParent.connectionNodes[i].zVal);//newNode.gridParent.connectionNodes[i];
            newNode.AddNeighbor(newNeighbor);
            newNeighbor.AddNeighbor(newNode);
        }
        ManageNodeList(newNode);
        
        return newNode;
    }
    public void RemoveNode(Node newNode)
    {
        if (newNode.IsTemporary())
        {
            for (int i = 0; i < nodeList.Count; i++)
            {
                Node removeNode = null;
                for (int j = 0; j < nodeList[i].neighbors.Count; j++)
                {
                    if (nodeList[i].neighbors[j] == newNode)
                        removeNode = newNode;
                }
                if (removeNode != null)
                    nodeList[i].neighbors.Remove(removeNode);
            }
            ManageNodeList(newNode, false);
        }

    }
    public void ManageGridList(Grid newGrid)
    {
        gridList.Add(newGrid);
        if (gridList.Count == gridsInScene.Count)
        {
            Initialize();
        }
    }
}
