using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;



public class Grid : MonoBehaviour 
{
    private BinaryHeap<Node> frontierHeap = new BinaryHeap<Node>();

    private Vector3 startLocation;
    private Vector3 currentLocation;

    private const float spacing = 1;
    private int nodeCounter = 0;

    private float gDist = 0;
    public float gDistInc = 1f;

    public List<Node> nodeList = new List<Node>();
    public Hashtable nodeHash = new Hashtable();

    public int gridSize = 30;
    public float[] heightMap;

    public int clusterSize = 10;
    public AbstractGrid abstractGrid;

    List<Node> mainPath = new List<Node>();
    List<GameObject> visuals = new List<GameObject>();
    int currentIndex = 0;

    public Vector2 n1;
    public Vector2 n2;

    private int visitCounter = 0;

    public GameObject nodeVisual;
    public GameObject nodeClusterVisual;
    public GameObject nodeEntranceVisual;
    public GameObject nodeUnavailableVisual;
    public GameObject debugLocationVisual;
    public GameObject connectionVisual;

    //=============================================
    //               ConnectionGrid
    //=============================================
    public ConnectionGrid connectionGrid;
    public GridConnector[] connectors;
    public List<Node> connectionNodes = new List<Node>();
    /*public Grid previousGrid;
    public bool visited = false;*/

	void Start () 
    {
        InitializeGrid();   
	}
    void Update()
    {
        
        
    }
    private void InitializeGrid()
    {
        //gridSize = (int)GetComponent<Terrain>().terrainData.size.x;
        CreateHeightMap();

        int newX = (int)transform.position.x;
        for (int i = 0; i < gridSize; ++i)
        {
            SpawnX(newX);
            newX++;
        }

        AssignAllNeighboors();

        abstractGrid = CreateAbstractGrid();

        for (int i = 0; i < connectors.Length; i++)
        {
            if(connectors[i]!=null)
                connectors[i].SetAbstractGrid(this);
        }

        connectionGrid.ManageGridList(this);


    }
    private AbstractGrid CreateAbstractGrid()
    {
        return new AbstractGrid(this, clusterSize);
    }

    public void SpawnNodeClusterVisual(params NodeCluster[] newCluster)
    {
        for(int i =0;i<newCluster.Length;i++)
        {
            GameObject newVisual = Instantiate(nodeClusterVisual, newCluster[i].GetLocation(), Quaternion.identity) as GameObject;
        }
    }
    public static void VisualizePath(List<Node> path)
    {
        if (path != null)
        {
            for (int i = 0; i < path.Count; i++)
            {
                if (i == 0 || i == path.Count - 1)
                    SpawnDebugLocationVisual(path[i]);
                else
                    SpawnNodeVisual(path[i]);
            }
        }
        else
        {
            Debug.Log("Path is NULL");
        }
    }
    
    public static void SpawnNodeVisual(params Node[] nodes)
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i] != null&&nodes[i].available)
            {
                GameObject newVisual;
                if (!nodes[i].IsAbstract())
                    newVisual = Instantiate(nodes[i].gridParent.nodeVisual, nodes[i].GetLocation(), Quaternion.identity) as GameObject;
                else
                    newVisual = Instantiate(nodes[i].gridParent.nodeEntranceVisual, nodes[i].GetLocation(), Quaternion.identity) as GameObject;
            }
            else if (nodes[i] != null && !nodes[i].available)
            {
                GameObject newVisual = Instantiate(nodes[i].gridParent.nodeUnavailableVisual, nodes[i].GetLocation(), Quaternion.identity) as GameObject;
            }
        }
    }
    public void SpawnNodeClusterVisual(Vector3 newLocation)
    {
        GameObject newVisual = Instantiate(nodeClusterVisual, newLocation, Quaternion.identity) as GameObject;
    }
    public static void SpawnDebugLocationVisual(Node newNode)
    {
        GameObject newVisual = Instantiate(newNode.gridParent.debugLocationVisual, newNode.GetLocation(), Quaternion.identity) as GameObject;
    }
    public void SpawnConnectionVisual(Node newNode)
    {
        GameObject newVisual = Instantiate(connectionVisual, newNode.GetLocation(), Quaternion.identity) as GameObject;
    }
    private void SpawnX(int newX)
    {
        int tempZ = (int)transform.position.z;
        Node newNode = new Node(this,newX, heightMap[nodeCounter], tempZ, NodeType.Normal,nodeCounter);
        nodeList.Add(newNode);
        int nodeKey = GetNodeKey(newNode);
        nodeHash.Add(nodeKey, nodeCounter);
        nodeCounter++;

        int newZ = tempZ + 1;
        for (int i = 0; i < gridSize - 1; ++i)
        {
            SpawnZ(newX, newZ);
            newZ++;
        }
    }
    private void SpawnZ(int newX, int newZ)
    {
        Node newNode = new Node(this, newX, heightMap[nodeCounter], newZ, NodeType.Normal, nodeCounter);
        nodeList.Add(newNode);
        int nodeKey = GetNodeKey(newNode);
        nodeHash.Add(nodeKey, nodeCounter);
        nodeCounter++;
    }
    public static int GetNodeKey(Node newNode)
    {
        return newNode.xVal * 1000 + newNode.zVal;
    }
    public static int GetNodeKey(int newX, int newZ)
    {
        return newX * 1000 + newZ;
    }
    public Node LookUpNode(int newX, int newZ)
    {
        int nodeKey = GetNodeKey(newX, newZ);
        if (nodeHash.Contains(nodeKey))
        {
            int nodeIndex = (int)nodeHash[nodeKey];
            return nodeList[nodeIndex];
        }
        else
        {
            return null;
        }
    }
    public Node LookUpNode(float newX, float newZ)
    {
        int nodeKey = GetNodeKey((int)newX, (int)newZ);
        if (nodeHash.Contains(nodeKey))
        {
            int nodeIndex = (int)nodeHash[nodeKey];
            return nodeList[nodeIndex];
        }
        else
        {
            return null;
        }
    }
    
    private void AssignAllNeighboors()
    {
        for (int i = 0; i < nodeList.Count; ++i)
        {
            Node newNode = nodeList[i];
            Node tempNode = null;
            int cSize = clusterSize;

            tempNode = LookUpNode(newNode.xVal + 1, newNode.zVal);
            if (AreNodesWithinSameCluster(newNode, tempNode))
                newNode.AddNeighbor(tempNode);
            //newNode.AddNeighbor(LookUpNode(newNode.xVal + 1, newNode.zVal));

            tempNode = LookUpNode(newNode.xVal - 1, newNode.zVal);
            if (AreNodesWithinSameCluster(newNode, tempNode))
                newNode.AddNeighbor(tempNode);
            //newNode.AddNeighbor(LookUpNode(newNode.xVal - 1, newNode.zVal));

            tempNode = LookUpNode(newNode.xVal, newNode.zVal + 1);
            if (AreNodesWithinSameCluster(newNode, tempNode))
                newNode.AddNeighbor(tempNode);
            //newNode.AddNeighbor(LookUpNode(newNode.xVal, newNode.zVal + 1));

            tempNode = LookUpNode(newNode.xVal, newNode.zVal - 1);
            if (AreNodesWithinSameCluster(newNode, tempNode))
                newNode.AddNeighbor(tempNode);
            //newNode.AddNeighbor(LookUpNode(newNode.xVal, newNode.zVal - 1));

            tempNode = LookUpNode(newNode.xVal + 1, newNode.zVal + 1);
            if (AreNodesWithinSameCluster(newNode, tempNode))
                newNode.AddNeighbor(tempNode);
            //newNode.AddNeighbor(LookUpNode(newNode.xVal + 1, newNode.zVal + 1));

            tempNode = LookUpNode(newNode.xVal - 1, newNode.zVal - 1);
            if (AreNodesWithinSameCluster(newNode, tempNode))
                newNode.AddNeighbor(tempNode);
            //newNode.AddNeighbor(LookUpNode(newNode.xVal - 1, newNode.zVal - 1));

            tempNode = LookUpNode(newNode.xVal - 1, newNode.zVal + 1);
            if (AreNodesWithinSameCluster(newNode, tempNode))
                newNode.AddNeighbor(tempNode);
            //newNode.AddNeighbor(LookUpNode(newNode.xVal - 1, newNode.zVal + 1));

            tempNode = LookUpNode(newNode.xVal + 1, newNode.zVal - 1);
            if (AreNodesWithinSameCluster(newNode, tempNode))
                newNode.AddNeighbor(tempNode);
            //newNode.AddNeighbor(LookUpNode(newNode.xVal + 1, newNode.zVal - 1));
        }
        
    }
    private bool AreNodesWithinSameCluster(Node newNode, Node checkNode)
    {
        
        if (newNode == null || checkNode == null)
            return false;

        int minX = (int)transform.position.x;
        int minZ = (int)transform.position.z;
        

        Vector2 currentMin = new Vector2(transform.position.x,transform.position.z);
        Vector2 currentMax = new Vector2(minX+(clusterSize-1),minZ+(clusterSize-1));

        bool clusterFound = false;
        for (int i = 0; i < gridSize / clusterSize; i++)
        {
            currentMin.x = minX + (clusterSize * i);
            currentMax.x = currentMin.x + (clusterSize - 1);
            

            for (int j = 0; j < gridSize / clusterSize; j++)
            {
                currentMin.y = minZ+(clusterSize*j);
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
            if (checkNode.xVal >= currentMin.x && checkNode.xVal <= currentMax.x && checkNode.zVal >= currentMin.y && checkNode.zVal <= currentMax.y)
            {
                return true;
            }
            else
                return false;
        }
        else
            return false;
        //This only works when Grid is at (0,0) and clusterSize ==10
        /*int nX = newNode.xVal;
        int nZ = newNode.zVal;

        int cX = checkNode.xVal;
        int cZ = checkNode.zVal;

        if (cX % clusterSize == 0 && cX > nX)
            return false;
        if (cZ % clusterSize == 0 && cZ > nZ)
            return false;
        if (nX % clusterSize == 0 && nX > cX)
            return false;
        if (nZ % clusterSize == 0 && nZ > cZ)
            return false;
        return true;*/
    }
    public List<Node> FindComplexPath(Node startNode, Node endNode)
    {
        if (startNode.available != true || endNode.available != true)
        {
            return null;
        }

        List<Node> newAbstractPath;
        if (startNode.gridParent != endNode.gridParent)
            newAbstractPath = startNode.gridParent.abstractGrid.FindMultiAbstractGridPath(startNode, endNode);
        else
            newAbstractPath = startNode.gridParent.abstractGrid.FindAbstractPath(startNode, endNode);
        //List<Node> newAbstractPath = abstractGrid.FindAbstractPath(startNode, endNode);
        

        List<Node> tempList = new List<Node>();
        List<Node> outList = new List<Node>();

        if (newAbstractPath == null)
        {
            Debug.Log("Abstract Path is Null");
            return null;
        }

        for (int i = 0; i < newAbstractPath.Count-1; i++)
        {
            Node sNode = newAbstractPath[i];
            Node eNode = newAbstractPath[i + 1];

            if (sNode.clusterParent == eNode.clusterParent)
            {
                NodeCluster newCluster = sNode.clusterParent;
                //Nodes are NOT Temporary, Use Precomputed Path
                if (!sNode.IsTemporary()&&!eNode.IsTemporary())
                {
                    
                    List<Node> storedPath = sNode.clusterParent.GetStoredPath(sNode, eNode);
                    if (storedPath==null)
                    {
                        Debug.Log("Stored Path is NULL");
                        Debug.Log("Start Node:(" + sNode.xVal + "," + sNode.zVal + ") End Node:(" + eNode.xVal + "," + eNode.zVal + ")");
                        return null;
                    }
                    
                    bool direction = (sNode.xVal == storedPath[0].xVal && sNode.zVal == storedPath[0].zVal);
                    if (!direction)
                    {
                        storedPath.Reverse();
                    }
                        
                    if (outList.Count > 0)
                    {
                        if (outList[outList.Count - 1] == sNode)
                            outList.RemoveAt(outList.Count - 1);
                    }
                    outList.AddRange(storedPath);
                }
                //At least one of the Nodes have been Inserted.  Compute a new path
                else
                {
                    Node s = sNode.gridParent.LookUpNode(sNode.xVal, sNode.zVal);
                    Node e = eNode.gridParent.LookUpNode(eNode.xVal, eNode.zVal);
                    tempList = sNode.gridParent.FindPath(s, e);
                    outList.AddRange(tempList);
                    
                    tempList.Clear();
                }
                

            }
            else
            {
                if ((i + 2) < newAbstractPath.Count)
                {
                    if (newAbstractPath[i + 2].IsAbstract())
                        outList.Add(eNode);
                }
                
            }
        }
        return outList;
        
    }
   
    public List<Node> FindPath(Node startNode, Node endNode, bool countVisitedNodes=false)
    {
        
        frontierHeap.Add(startNode);
        gDist = 0;


        while (frontierHeap.Count > 0)
        {

            Node currentNode = frontierHeap.Remove();
            
            currentNode.ToggleVisited(true);


            if (currentNode == endNode)
                break;
            gDist += gDistInc;
            for (int i = 0; i < currentNode.neighbors.Count; ++i)
            {
                if (currentNode.neighbors[i].visited != true && currentNode.neighbors[i].available)
                {
                    if (currentNode.clusterParent == currentNode.neighbors[i].clusterParent)
                    {
                        currentNode.neighbors[i].AssignPreviouseNode(currentNode);
                        currentNode.neighbors[i].SetG(gDist);
                        currentNode.neighbors[i].SetH(endNode);
                        currentNode.neighbors[i].SetF();
                        frontierHeap.Add(currentNode.neighbors[i]);

                        if (countVisitedNodes)
                        {
                            visitCounter++;
                        }
                            
                    }   

                    
                }
            }

        }
        bool pathExists = endNode.visited;
        if (pathExists)
        {
            Node curNode = endNode;
            List<Node> newPath = new List<Node>();

            while (curNode != startNode)
            {
                newPath.Add(curNode);
                curNode = curNode.previouseNode;
            }

            newPath.Add(curNode);
            newPath.Reverse();

            ResetGrid();
            return newPath;
        }
        else
        {
            ResetGrid();
            return null;
        }
        
    }
    public float DoesPathExist(Node startNode, Node endNode)
    {
        float currentPathCost = 0;

        frontierHeap.Add(startNode);
        gDist = 0;

        while (frontierHeap.Count > 0)
        {
            Node currentNode = frontierHeap.Remove();
            
            currentNode.ToggleVisited(true);
            if (currentNode == endNode)
            {
                break;
            }
                
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

        bool pathExists = endNode.visited;


        if (pathExists)
        {
            Node curNode = endNode;

            while (curNode != startNode)
            {
                currentPathCost += curNode.f;
                curNode = curNode.previouseNode;
            }
        }

        ResetGrid();
        return (pathExists) ? currentPathCost : -1;
    }
    private void ResetGrid()
    {
        for (int i = 0; i < nodeList.Count; ++i)
        {
            nodeList[i].Reset();
        }
        frontierHeap.Clear();
        
    }
    private void CreateHeightMap()
    {
        heightMap = new float[gridSize * gridSize];
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                heightMap[(i * gridSize) + j] = transform.position.y;
            }
        }
    }

    public bool ToggleNodeAvailability(Node newNode, bool availability = false)
    {
        if (newNode != null)
        {
            newNode.ToggleAvailable(availability);
            return true;
        }
        else
            return false;
            
    }
    public bool ToggleNodeAvailability(int xVal, int zVal, bool availability = false)
    {
        Node newNode = LookUpNode(xVal, zVal);
        if (newNode != null)
        {
            newNode.ToggleAvailable(availability);
            return true;
        }
        else
            return false;
    }
    /*
     * Connection Nodes in the Grid Class are Abstract Nodes
     */ 
    public void ManageConnectionNodes(Node newNode)
    {
        if (!connectionNodes.Contains(newNode) && newNode != null)
        {
            connectionNodes.Add(newNode);
        }
    }
}
