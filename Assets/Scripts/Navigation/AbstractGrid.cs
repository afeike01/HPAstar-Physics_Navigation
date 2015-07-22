using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class AbstractGrid 
{
    public Grid mainGrid;
    public int abstractGridSize;
    public int clusterSize;

    //public List<NodeCluster> nodeClusterList = new List<NodeCluster>();
    //public Hashtable nodeClusterHash = new Hashtable();
    public Dictionary<int, NodeCluster> nodeClusterDictionary = new Dictionary<int, NodeCluster>();

    public Dictionary<int, Node> nodeDictionary = new Dictionary<int, Node>();
    //public List<Node> nodeList = new List<Node>();
    //public Hashtable nodeHash = new Hashtable();
    private BinaryHeap<Node> frontierHeap = new BinaryHeap<Node>();
    private int nodeClusterCounter = 0;

    private float gDist = 0;
    public float gDistInc = 1f;


    public AbstractGrid(Grid newGrid, int cSize)
    {
        mainGrid = newGrid;
        clusterSize = cSize;
        abstractGridSize = mainGrid.gridSize / clusterSize;

        
        int newX = (int)mainGrid.transform.position.x;
        for (int i = 0; i < abstractGridSize; ++i)
        {
            SpawnX(newX);
            newX+=clusterSize;
        }

        SetClusterParents(mainGrid.nodeDictionary);
        CreateNodeClusterEntrances();
        SetAllClusterEntranceConnections();
        AssignAbstractNodeNeighbors();

        
    }
    private void SpawnX(int newX)
    {
        int tempZ = (int)mainGrid.transform.position.z;//int tempZ = 0;
        NodeCluster newNodeCluster = new NodeCluster(this, clusterSize, newX, tempZ);
        //nodeClusterList.Add(newNodeCluster);
        int nodeClusterKey = GetNodeClusterKey(newNodeCluster.xVal, newNodeCluster.zVal);
        //nodeClusterHash.Add(nodeClusterKey, nodeClusterCounter);
        nodeClusterDictionary.Add(nodeClusterKey, newNodeCluster);
        nodeClusterCounter++;

        int newZ = tempZ + clusterSize;//int newZ = 1;
        for (int i = 0; i < abstractGridSize - 1; ++i)
        {
            SpawnZ(newX, newZ);
            newZ += clusterSize;
        }
    }
    private void SpawnZ(int newX, int newZ)
    {
        NodeCluster newNodeCluster = new NodeCluster(this, clusterSize, newX, newZ);
        //nodeClusterList.Add(newNodeCluster);
        int nodeClusterKey = GetNodeClusterKey(newNodeCluster.xVal, newNodeCluster.zVal);
        //nodeClusterHash.Add(nodeClusterKey, nodeClusterCounter);
        nodeClusterDictionary.Add(nodeClusterKey, newNodeCluster);
        nodeClusterCounter++;
    }
    public void SetClusterParents(Dictionary<int,Node> nodeDictionary/*List<Node> nodeList*/)
    {
        foreach(Node newNode in nodeDictionary.Values)//for (int h = 0; h < nodeList.Count; h++)
        {
            //Node newNode = nodeList[h];

            int minX = (int)mainGrid.transform.position.x;
            int minZ = (int)mainGrid.transform.position.z;

            Vector2 currentMin = new Vector2(minX, minZ);
            Vector2 currentMax = new Vector2(minX + (clusterSize - 1), minZ + (clusterSize - 1));

            bool clusterFound = false;
            for (int i = 0; i < mainGrid.gridSize / clusterSize; i++)
            {
                currentMin.x = minX + (clusterSize * i);
                currentMax.x = currentMin.x + (clusterSize - 1);

                for (int j = 0; j < mainGrid.gridSize / clusterSize; j++)
                {
                    currentMin.y = minZ + (clusterSize * j);
                    currentMax.y = currentMin.y + (clusterSize - 1);

                    if (newNode.xVal >= currentMin.x && newNode.xVal <= currentMax.x && newNode.zVal >= currentMin.y && newNode.zVal <= currentMax.y)
                    {
                        clusterFound = true;
                        break;
                    }

                }
                if (clusterFound)
                    break;
            }
            if (clusterFound)
            {
                NodeCluster newCluster = LookUpNodeCluster((int)currentMin.x, (int)currentMin.y);
                newNode.SetClusterParent(newCluster);
            }
        }
    }
    public void CreateNodeClusterEntrances()
    {
        //Cluster Parents must be assigned!!!
        //This will need REVAMPED when some nodes are unavailable, currently does not account for obstacles
        
        foreach(NodeCluster newCluster in nodeClusterDictionary.Values)//for (int i = 0; i < nodeClusterList.Count; i++)
        {
            //NodeCluster newCluster = nodeClusterList[i];

            int minX = newCluster.xVal;//newCluster.xVal * clusterSize;
            int minZ = newCluster.zVal;// newCluster.zVal* clusterSize;
            int maxX = minX + clusterSize-1;//newCluster.xVal*clusterSize + clusterSize - 1;
            int maxZ = minZ+clusterSize-1;//newCluster.zVal *clusterSize + clusterSize -1;

            newCluster.ManageAbstractNodes(mainGrid.LookUpNode(minX, minZ));
            newCluster.ManageAbstractNodes(mainGrid.LookUpNode(maxX, maxZ));
            newCluster.ManageAbstractNodes(mainGrid.LookUpNode(minX, maxZ));
            newCluster.ManageAbstractNodes(mainGrid.LookUpNode(maxX, minZ));
        }

    }
    
    public void SetAllClusterEntranceConnections()
    {
        foreach (NodeCluster newCluster in nodeClusterDictionary.Values)
        {
            newCluster.SetAbstractConnections();
        }
        /*for (int i = 0; i < nodeClusterList.Count; i++)
        {
            nodeClusterList[i].SetAbstractConnections();
        }*/
    }
    public void AssignAbstractNodeNeighbors()
    {

        foreach(Node newNode in nodeDictionary.Values)//for (int i = 0; i < nodeList.Count; i++)
        {
            //Node newNode = nodeList[i];
            Node tempNode = null;

            tempNode = LookUpAbstractNode(newNode.xVal + 1, newNode.zVal);
            newNode.AddNeighbor(tempNode);

            tempNode = LookUpAbstractNode(newNode.xVal - 1, newNode.zVal);
            newNode.AddNeighbor(tempNode);

            tempNode = LookUpAbstractNode(newNode.xVal, newNode.zVal + 1);
            newNode.AddNeighbor(tempNode);

            tempNode = LookUpAbstractNode(newNode.xVal, newNode.zVal - 1);
            newNode.AddNeighbor(tempNode);

            tempNode = LookUpAbstractNode(newNode.xVal + 1, newNode.zVal + 1);
            newNode.AddNeighbor(tempNode);

            tempNode = LookUpAbstractNode(newNode.xVal - 1, newNode.zVal - 1);
            newNode.AddNeighbor(tempNode);

            tempNode = LookUpAbstractNode(newNode.xVal - 1, newNode.zVal + 1);
            newNode.AddNeighbor(tempNode);

            tempNode = LookUpAbstractNode(newNode.xVal + 1, newNode.zVal - 1);
            newNode.AddNeighbor(tempNode);
            
        }
    }
    public static int GetNodeClusterKey(int newX, int newZ)
    {
        return newX * 1000 + newZ;
    }
    public NodeCluster LookUpNodeCluster(int newX, int newZ)
    {
        int nodeClusterKey = GetNodeClusterKey(newX, newZ);
        /*if (nodeClusterHash.Contains(nodeKey))
        {
            int nodeClusterIndex = (int)nodeClusterHash[nodeKey];
            return nodeClusterList[nodeClusterIndex];
        }*/
        if (nodeClusterDictionary.ContainsKey(nodeClusterKey))
        {
            return nodeClusterDictionary[nodeClusterKey];
        }
        else
        {
            return null;
        }
    }
    public int GetAbstractNodeKey(Node newNode)
    {
        return newNode.xVal * 1000 + newNode.zVal;
    }
    public Node LookUpAbstractNode(int newX, int newZ)
    {
        int nodeKey = newX * 1000 + newZ;
        /*if (nodeHash.Contains(nodeKey))
        {
            return nodeHash[nodeKey] as Node;
        }*/
        if (nodeDictionary.ContainsKey(nodeKey))
        {
            return nodeDictionary[nodeKey];
        }
        else
        {
            return null;
        }
    }
    public void ManageAbstractNodeList(Node newNode,bool add=true)
    {
        int nodeKey = GetAbstractNodeKey(newNode);
        if (add && newNode != null && !nodeDictionary.ContainsKey(nodeKey))
        {
            //nodeList.Add(newNode);
            //int nodeKey = GetAbstractNodeKey(newNode);
            //nodeHash.Add(nodeKey,newNode);
            nodeDictionary.Add(nodeKey, newNode);
        }
        else
        {
            //nodeList.Remove(newNode);
            //nodeHash.Remove(GetAbstractNodeKey(newNode));
            if (nodeDictionary.ContainsKey(nodeKey))
            {
                nodeDictionary.Remove(nodeKey);
            }
        }
    }
    public List<Node> FindAbstractPath(Node sNode, Node eNode)
    {
        

        bool startNodeInserted = (LookUpAbstractNode(sNode.xVal, sNode.zVal) == null);
        bool endNodeInserted = (LookUpAbstractNode(eNode.xVal, eNode.zVal) == null);

        Node startNode = startNodeInserted ? InsertNode(sNode) : LookUpAbstractNode(sNode.xVal,sNode.zVal);
        Node endNode = endNodeInserted ? InsertNode(eNode) : LookUpAbstractNode(eNode.xVal,eNode.zVal);

        /*Debug.Log("Start Node is null: " + sNode==null);
        Debug.Log("End Node is null: " + eNode==null);*/

        frontierHeap.Add(startNode);
        gDist = 0;
        //HOLD ON
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
            {
                
                RemoveNode(startNode);
            }

            if (endNodeInserted)
            {
                
                RemoveNode(endNode);
            }
            ResetAbstractGrid();

            return newPath;
        }
        else
        {
            
            if (startNodeInserted)
            {
                
                RemoveNode(startNode);
            }

            if (endNodeInserted)
            {
                
                RemoveNode(endNode);
            }
                
            ResetAbstractGrid();

            return null;
            
        }
    }
    public List<Node> FindMultiAbstractGridPath(Node sNode, Node eNode)
    {

        Grid startNodeGrid = sNode.gridParent;
        Grid endNodeGrid = eNode.gridParent;

        bool startNodeInserted = (startNodeGrid.abstractGrid.LookUpAbstractNode(sNode.xVal, sNode.zVal) == null);
        bool endNodeInserted = (endNodeGrid.abstractGrid.LookUpAbstractNode(eNode.xVal, eNode.zVal) == null);

        Node startNode = startNodeInserted ? startNodeGrid.abstractGrid.InsertNode(sNode) : startNodeGrid.abstractGrid.LookUpAbstractNode(sNode.xVal, sNode.zVal);
        Node endNode = endNodeInserted ? endNodeGrid.abstractGrid.InsertNode(eNode) : endNodeGrid.abstractGrid.LookUpAbstractNode(eNode.xVal, eNode.zVal);

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
                startNodeGrid.abstractGrid.RemoveNode(startNode);
            if (endNodeInserted)
                endNodeGrid.abstractGrid.RemoveNode(endNode);
            startNodeGrid.abstractGrid.ResetAbstractGrid();
            endNodeGrid.abstractGrid.ResetAbstractGrid();

            return newPath;
        }
        else
        {
            Debug.Log("Abstract Path is NULL");

            if (startNodeInserted)
                startNodeGrid.abstractGrid.RemoveNode(startNode);
            if (endNodeInserted)
                endNodeGrid.abstractGrid.RemoveNode(endNode);
            startNodeGrid.abstractGrid.ResetAbstractGrid();
            endNodeGrid.abstractGrid.ResetAbstractGrid();

            return null;

        }
    }
    private void ResetAbstractGrid()
    {
        /*for (int i = 0; i < nodeList.Count; i++)
        {
            nodeList[i].Reset();
        }*/
        foreach (Node newNode in nodeDictionary.Values)
        {
            newNode.Reset();
        }
        /*for (int i = 0; i < nodeClusterList.Count; i++)
        {
            for (int j = 0; j < nodeClusterList[i].nodeList.Count; j++)
            {
                nodeClusterList[i].nodeList[j].Reset();
            }
        }*/
        frontierHeap.Clear();

    }
    public NodeCluster GetNodeClusterFromLocation(int xVal, int zVal)
    {
        foreach(NodeCluster newCluster in nodeClusterDictionary.Values)//for (int i = 0; i < nodeClusterList.Count; i++)
        {
            //NodeCluster newCluster = nodeClusterList[i];
            int minX = newCluster.xVal;
            int minZ = newCluster.zVal;
            int maxX = newCluster.xVal + clusterSize;
            int maxZ = newCluster.zVal + clusterSize;

            if (xVal >= minX && xVal <= maxX && zVal >= minZ && zVal <= maxZ)
                return newCluster;
        }
        return null;
    }
    /*
    * =============================================================
    *                      Manages Inserted Nodes
    * =============================================================
    */
    public Node InsertNode(Node refNode)
    {
        
        NodeCluster newCluster = refNode.clusterParent!=null ? refNode.clusterParent : GetNodeClusterFromLocation(refNode.xVal,refNode.zVal);
        Node newNode = new Node(this.mainGrid,refNode.xVal, refNode.yVal, refNode.zVal, NodeType.Temporary,-1);
        newNode.SetClusterParent(newCluster);

        ManageAbstractNodeList(newNode);
        newCluster.ManageAbstractNodes(newNode);
        newCluster.SetAbstractConnections();
        
        return newNode;  
    }
    public void RemoveNode(Node newNode)
    {
        //Only Remove if Node is NOT ABSTRACT
        if (newNode.IsTemporary())
        {
            NodeCluster newCluster = newNode.clusterParent;
            ManageAbstractNodeList(newNode, false);
            
            for (int i = 0; i < newCluster.nodeList.Count; i++)
            {
                Node removeNode=null;
                for (int j = 0; j < newCluster.nodeList[i].neighbors.Count; j++)
                {
                    if (newCluster.nodeList[i].neighbors[j] == newNode)
                        removeNode = newNode;
                }
                if (removeNode != null)
                    newCluster.nodeList[i].neighbors.Remove(removeNode);
            }
            newCluster.ManageAbstractNodes(newNode, false);
            newCluster.SetAbstractConnections();
        }
        
    }

}
