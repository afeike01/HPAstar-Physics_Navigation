using UnityEngine;
using System.Collections;


public class GridConnector : MonoBehaviour 
{
    public ConnectionGrid connectionGrid;

    public Transform connection_01;
    public Grid grid_01;
    public AbstractGrid abstractGrid_01;
    public Node node_01;

    public Transform connection_02;
    public Grid grid_02;
    public AbstractGrid abstractGrid_02;
    public Node node_02;

	// Use this for initialization
	void Start () 
    {
        
	}

    private void Initialize()
    {

        int x_01 = (int)connection_01.position.x;
        int z_01 = (int)connection_01.position.z;

        Node tempNode = abstractGrid_01.mainGrid.LookUpNode(x_01, z_01);
        NodeCluster tempCluster = tempNode.clusterParent;
        Node closestNode = tempCluster.nodeList[0];
        float closestDist = 1000000;
        for (int i = 0; i < tempCluster.nodeList.Count; i++)
        {
            float tempDist = Vector3.Distance(tempNode.GetLocation(), tempCluster.nodeList[i].GetLocation());
            if (tempDist < closestDist)
            {
                closestDist = tempDist;
                closestNode = tempCluster.nodeList[i];
            }
        }
        node_01 = closestNode;

        /*NodeCluster nc_01 = abstractGrid_01.GetNodeClusterFromLocation(x_01, z_01);
        Node closestNode = nc_01.nodeList[0];
        float closestDistance = 100000000;
        for (int i = 0; i < nc_01.nodeList.Count; i++)
        {
            float tempDist = Vector3.Distance(connection_01.position, nc_01.nodeList[i].GetLocation());
            if (tempDist < closestDistance)
            {
                closestDistance = tempDist;
                closestNode = nc_01.nodeList[i];
            }
        }
        node_01 = closestNode;*/

        int x_02 = (int)connection_02.position.x;
        int z_02 = (int)connection_02.position.z;

        Node tempNode2 = abstractGrid_02.mainGrid.LookUpNode(x_02, z_02);
        NodeCluster tempCluster2 = tempNode2.clusterParent;
        Node closestNode2 = tempCluster2.nodeList[0];
        float closestDist2 = 1000000;
        for (int i = 0; i < tempCluster2.nodeList.Count; i++)
        {
            float tempDist = Vector3.Distance(tempNode2.GetLocation(), tempCluster2.nodeList[i].GetLocation());
            if (tempDist < closestDist2)
            {
                closestDist2 = tempDist;
                closestNode2 = tempCluster2.nodeList[i];
            }
        }
        node_02 = closestNode2;

        /*NodeCluster nc_02 = abstractGrid_02.GetNodeClusterFromLocation(x_02, z_02);
        closestNode = nc_02.nodeList[0];
        closestDistance = 100000000;
        for (int i = 0; i < nc_02.nodeList.Count; i++)
        {
            float tempDist = Vector3.Distance(connection_02.position, nc_02.nodeList[i].GetLocation());
            if (tempDist < closestDistance)
            {
                closestDistance = tempDist;
                closestNode = nc_02.nodeList[i];
            }
        }
        node_02 = closestNode;*/

        node_01.SetConnection(node_02);
        node_02.SetConnection(node_01);

        node_01.gridParent.ManageConnectionNodes(node_01);
        node_02.gridParent.ManageConnectionNodes(node_02);

        connectionGrid.ManageNodeList(node_01);
        connectionGrid.ManageNodeList(node_02);

        
    }
    /*
     * Set AbstractGrid variables for this Class
     * When both variables are set, Initialize is called
     */ 
    public void SetAbstractGrid(Grid newGrid)
    {
        if (newGrid == grid_01)
        {
            abstractGrid_01 = newGrid.abstractGrid;
        }
        if (newGrid == grid_02)
        {
            abstractGrid_02 = grid_02.abstractGrid;
        }
        if (abstractGrid_01 != null && abstractGrid_02 != null)
        {
            Initialize();
        }
    }
}
