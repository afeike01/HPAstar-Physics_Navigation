using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConnectionGrid : MonoBehaviour 
{
    public List<Grid> gridList = new List<Grid>();
    public Dictionary<int, Node> nodeDictionary = new Dictionary<int, Node>();
    public Dictionary<int, List<Node>> storedPathsDictionary = new Dictionary<int, List<Node>>();
    public BinaryHeap<Node> frontierHeap = new BinaryHeap<Node>();

    private float gDist = 0;
    public float gDistInc = 1f;

    int nodeCounter = 0;
    public List<Grid> gridsInScene = new List<Grid>();

    public Testing temporaryController;

    private void Initialize()
    {
        SetConnectionNodeNeighbors();
        SetConnections();

        
    }
    public Node GetPlayerLocation()
    {
        return temporaryController.GetPlayerNodeLocation();
    }
    public bool DisableNode(Vector3 nodeLocation)
    {
        Node newNode = GetNodeFromLocation(nodeLocation);
        if (newNode != null && !newNode.IsPermanent())
        {
            newNode.ToggleAvailable(false);
            newNode.clusterParent.RefreshPaths(newNode);
            return true;
        }
        return false;
    }
    public Node LookUpNode(int newX, int newZ)
    {
        int nodeKey = Grid.GetNodeKey(newX, newZ);
        if (nodeDictionary.ContainsKey(nodeKey))
        {
            return nodeDictionary[nodeKey];
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
                    Node newNode = new Node(refNode.gridParent, refNode.xVal, refNode.yVal, refNode.zVal, NodeType.Abstract, nodeCounter);
                    newNode.nodeConnectingTo = refNode.nodeConnectingTo;
                    nodeCounter++;
                    int newKey = Grid.GetNodeKey(newNode);
                    nodeDictionary.Add(newKey, newNode);
                }

            }
            else if (add && refNode.IsTemporary())
            {
                //If an Inserted Node
                if (!nodeDictionary.ContainsValue(refNode))
                {
                    int newKey = Grid.GetNodeKey(refNode);
                    nodeDictionary.Add(newKey, refNode);
                }
            }
            else
            {
                //If Remove
                int newKey = Grid.GetNodeKey(refNode);
                if (nodeDictionary.ContainsKey(newKey))
                    nodeDictionary.Remove(newKey);
                if (refNode.IsTemporary())
                    refNode = null;
            }
        }
        
    }
    public void SetConnectionNodeNeighbors()
    {
        foreach(Node newNode in nodeDictionary.Values)
        {
            newNode.AddNeighbor(this.LookUpNode(newNode.nodeConnectingTo.xVal, newNode.nodeConnectingTo.zVal));
            foreach(Node newNode2 in nodeDictionary.Values)
            {
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
        if (storedPathsDictionary.ContainsKey(connectionKey))
        {
            List<Node> newPath = storedPathsDictionary[connectionKey];
            return newPath;
        }
        else
            return null;
    }
    public void StorePath(Node startNode, Node endNode)
    {
        if (startNode.gridParent != endNode.gridParent)
            return;

        List<Node> newPath = startNode.gridParent.abstractGrid.FindAbstractPath(startNode, endNode);
        int connectionKey = GetConnectionKey(startNode, endNode);
        if (!storedPathsDictionary.ContainsKey(connectionKey) && newPath != null)
        {
            storedPathsDictionary.Add(connectionKey, newPath);
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
        List<Node> newPath = new List<Node>();

        if (startNode.gridParent != endNode.gridParent)
        {
            List<Node> newConnectionPath = FindConnectionPath(startNode, endNode);


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
                }
            }
        }
        else
        {
            Grid newGrid = startNode.gridParent;
            newPath = newGrid.FindComplexPath(startNode, endNode);
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
                newPath.Add(curNode);
                curNode = curNode.previouseNode;
            }
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
        foreach (Node newNode in nodeDictionary.Values)
        {
            newNode.Reset();
        }
        frontierHeap.Clear();
    }
    public Node InsertNode(Node refNode)
    {
        Node newNode = new Node(refNode.gridParent, refNode.xVal, refNode.yVal, refNode.zVal, NodeType.Temporary, -1);
        for (int i = 0; i < newNode.gridParent.connectionNodes.Count; i++)
        {
            Node newNeighbor = LookUpNode(newNode.gridParent.connectionNodes[i].xVal, newNode.gridParent.connectionNodes[i].zVal);
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
            foreach(Node n in nodeDictionary.Values)
            {
                Node removeNode = null;
                for (int j = 0; j < n.neighbors.Count; j++)
                {
                    if (n.neighbors[j] == newNode)
                        removeNode = newNode;
                }
                if (removeNode != null)
                    n.neighbors.Remove(removeNode);
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
    public Node GetNodeFromLocation(Vector3 location)
    {
        Node newNode = null;
        int xVal = (int)location.x;
        int zVal = (int)location.z;

        for (int i = 0; i < gridList.Count; i++)
        {
            Node tempNode = gridList[i].LookUpNode(xVal, zVal);
            if (tempNode != null)
                {
                    newNode = tempNode;
                    break;
                }
        }
        if (newNode != null && !newNode.available)
        {
            for (int i = 0; i < newNode.neighbors.Count; i++)
            {
                if (newNode.neighbors[i].available)
                {
                    newNode = newNode.neighbors[i];
                }
            }
        }
        if (newNode == null)
        {
            newNode = GetClosestConnectionNode(location);
        }
        return newNode;
    }
    public Node GetClosestConnectionNode(Vector3 location)
    {
        Node closestNode = null;
        float closestDist = 1000000;
        foreach(Node newNode in nodeDictionary.Values)
        {
            float tempDist = Vector3.Distance(location, newNode.GetLocation());
            if (tempDist < closestDist)
            {
                closestDist = tempDist;
                closestNode = newNode;
            }
        }
        return closestNode;
    }
}
