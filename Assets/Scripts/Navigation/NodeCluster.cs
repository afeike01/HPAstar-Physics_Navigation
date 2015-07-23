using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class NodeCluster
{
    public int xVal=0;
    public int yVal = 0;
    public int zVal=0;
    private int clusterSize;
    private AbstractGrid mainAbstractGrid;

    public List<Node> nodeList = new List<Node>();
    public Dictionary<int,List<Node>> storedPathsDictionary = new Dictionary<int,List<Node>>();

    private int nodeCounter = 0;

    public NodeCluster(AbstractGrid newAbstractGrid, int newSize, int newX, int newZ)
    {
        xVal = newX;
        zVal = newZ;
        clusterSize = newSize;
        mainAbstractGrid = newAbstractGrid;

    }
    
    public void ManageAbstractNodes(Node refNode,bool add=true)
    {
       
        if (refNode != null)
        {
            if (add&&!refNode.IsTemporary())
            {

                bool isValid = true;

                int minX = ((int)mainAbstractGrid.mainGrid.transform.position.x);
                int minZ = ((int)mainAbstractGrid.mainGrid.transform.position.z);
                int maxX = (mainAbstractGrid.mainGrid.gridSize-1)+((int)mainAbstractGrid.mainGrid.transform.position.x);
                int maxZ = (mainAbstractGrid.mainGrid.gridSize - 1) + ((int)mainAbstractGrid.mainGrid.transform.position.z);

                if (refNode.xVal == minX && refNode.zVal == minZ)
                    isValid = false;
                if (refNode.xVal == maxX && refNode.zVal == maxZ)
                    isValid = false;
                if (refNode.xVal == minX && refNode.zVal == maxZ)
                    isValid = false;
                if (refNode.xVal == maxX && refNode.zVal == minZ)
                    isValid = false;

                if (isValid)
                {
                    //Create a new Node
                    //Add to NodeList
                    Node newNode = new Node(this.mainAbstractGrid.mainGrid,refNode.xVal, refNode.yVal, refNode.zVal, NodeType.Abstract,nodeCounter);
                    nodeCounter++;
                    newNode.SetClusterParent(this);
                    nodeList.Add(newNode);
                    mainAbstractGrid.ManageAbstractNodeList(newNode);
                }
                
            }
            else if (add && refNode.IsTemporary())
            {
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
                if(refNode.IsTemporary())
                    refNode = null;
            }
        }
        
    }
    public Vector3 GetLocation()
    {
        return new Vector3(xVal,yVal,zVal);
    }
    public int GetConnectionKey(Node startNode, Node endNode)
    {
        return startNode.nodeNum + (endNode.nodeNum * 1000);
    }
    public List<Node> GetStoredPath(Node startNode, Node endNode)
    {
        int connectionKey = GetConnectionKey(startNode,endNode);
        if (storedPathsDictionary.ContainsKey(connectionKey))
        {
            List<Node> newPath = storedPathsDictionary[connectionKey];
            return newPath;
        }
        else
            return null;
        
    }
    public void StoreConnectionValue(Node startNode, Node endNode)
    {
        

        Node refStartNode = mainAbstractGrid.mainGrid.LookUpNode(startNode.xVal, startNode.zVal);
        Node refEndNode = mainAbstractGrid.mainGrid.LookUpNode(endNode.xVal, endNode.zVal);

        float newVal = mainAbstractGrid.mainGrid.DoesPathExist(refStartNode, refEndNode);
        int connectionKey = GetConnectionKey(startNode, endNode);
        if (!storedPathsDictionary.ContainsKey(connectionKey))
        {
            if (newVal > 0)
            {
                startNode.AddNeighbor(endNode);
                endNode.AddNeighbor(startNode);
                StorePath(startNode, endNode);
            }
        }
        
    }
    public void StorePath(Node startNode, Node endNode)
    {
        if (!startNode.IsTemporary() && !endNode.IsTemporary())
        {
            Node refStartNode = mainAbstractGrid.mainGrid.LookUpNode(startNode.xVal, startNode.zVal);
            Node refEndNode = mainAbstractGrid.mainGrid.LookUpNode(endNode.xVal, endNode.zVal);

            List<Node> newPath = mainAbstractGrid.mainGrid.FindPath(refStartNode, refEndNode);
            int connectionKey = GetConnectionKey(startNode, endNode);
            if (!storedPathsDictionary.ContainsKey(connectionKey) && newPath != null)
            {
                storedPathsDictionary.Add(connectionKey, newPath); 
            }
        }
    }
    public void StoreRefreshedPath(Node startNode, Node endNode)
    {
        if (!startNode.IsTemporary() && !endNode.IsTemporary())
        {
            Node refStartNode = mainAbstractGrid.mainGrid.LookUpNode(startNode.xVal, startNode.zVal);
            Node refEndNode = mainAbstractGrid.mainGrid.LookUpNode(endNode.xVal, endNode.zVal);

            List<Node> newPath = mainAbstractGrid.mainGrid.FindPath(refStartNode, refEndNode);
            int connectionKey = GetConnectionKey(startNode, endNode);
            if (newPath != null&&storedPathsDictionary.ContainsKey(connectionKey))
            {
                storedPathsDictionary[connectionKey] = newPath;
            }
        }
    }
    public void RefreshPaths(Node newNode)
    {
        List<int> pathsToRefresh = new List<int>();
        foreach (int connectionKey in storedPathsDictionary.Keys)
        {
            List<Node> path = storedPathsDictionary[connectionKey] as List<Node>;
            for (int i = 0; i < path.Count; i++)
            {
                if (path[i].xVal == newNode.xVal && path[i].zVal == newNode.zVal)
                {
                    pathsToRefresh.Add(connectionKey);
                }
            }
        }
        for (int i = 0; i < pathsToRefresh.Count; i++)
        {
            List<Node> path = storedPathsDictionary[pathsToRefresh[i]];
            StoreRefreshedPath(path[0], path[path.Count - 1]);
        }
    }
    public void SetAbstractConnections()
    {
        for (int i = 0; i < nodeList.Count; i++)
        {
            Node startNode = nodeList[i];
            for (int j = 0; j < nodeList.Count; j++)
            {
                Node endNode = nodeList[j];
                StoreConnectionValue(startNode, endNode);
                
            }
        }
    }
}
