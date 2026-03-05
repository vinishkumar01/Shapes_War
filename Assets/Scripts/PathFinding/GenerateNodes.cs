using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class GenerateNodes : MonoBehaviour
{
    public static event Action OnNodesGenerated;

    [Header("References")]
    [SerializeField] Tilemap platformTM;
    [SerializeField] LayerMask platformLayer;
    [SerializeField] Node NodePrefab;
    public static GenerateNodes instance;

    [Header("Nodes")]
    [SerializeField] public List<Node> nodeList;
    [SerializeField] public List<Node> EdgeNodes;
    [SerializeField] public List<Node> CombinedNodes;
    bool canDrawGizmos;

    [Header("Node Types")]
    [SerializeField] float maxJumpHeight = 6.0f;
    [SerializeField] float maxJumpDistance = 5.0f;
    [SerializeField] float maxFallHeight = 10.0f;
    [SerializeField] float maxFallDistance = 7.0f;

    [Header("Node Count Var")]
    int walkableNodeCount = 0;
    int REdgeNodeCount = 0;
    int LEdgeNodeCount = 0;
    
    private void Awake()
    {
        instance = this;

        generatingNodes();
        DebugLinesForNodeToNodesWeHaveGenerated();

        //Passing Event
        OnNodesGenerated?.Invoke();
        
    }

    void generatingNodes()
    {
        BoundsInt bounds = platformTM.cellBounds;

        // This loop will iterate through every tile (cell) in the tileMap
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int CurrentcellPos = new Vector3Int(x, y, 0);

                if (platformTM.HasTile(CurrentcellPos))
                {
                    Vector3Int aboveCell = new Vector3Int(x, y + 1, 0);


                    if (!platformTM.HasTile(aboveCell))
                    {
                        Vector3 worldPos = platformTM.GetCellCenterWorld(aboveCell);
                        Node node = PoolManager.SpawnObject(NodePrefab, worldPos, Quaternion.identity, PoolManager.PoolType.Nodes);
                        node.name = "WalkableNode " + "_ " + walkableNodeCount.ToString();
                        walkableNodeCount++;
                        //Adding Nodes to the list so that the connections can be done automatically
                        nodeList.Add(node);

                        Vector3Int rightCell = new Vector3Int(x + 1, y, 0);
                        Vector3Int leftCell = new Vector3Int(x - 1, y, 0);

                        if (!platformTM.HasTile(rightCell))
                        {
                            Vector3 rightPos = platformTM.GetCellCenterWorld(aboveCell) + new Vector3(1f, 0, 0);
                            Node RightNode = PoolManager.SpawnObject(NodePrefab, rightPos, Quaternion.identity, PoolManager.PoolType.Nodes);
                            RightNode.name = "RightEdgeNode " + "_ " + REdgeNodeCount.ToString();
                            REdgeNodeCount++;
                            EdgeNodes.Add(RightNode);
                        }

                        if (!platformTM.HasTile(leftCell))
                        {
                            Vector3 leftPos = platformTM.GetCellCenterWorld(aboveCell) + new Vector3(-1f, 0, 0);
                            Node LeftNode = PoolManager.SpawnObject(NodePrefab, leftPos, Quaternion.identity, PoolManager.PoolType.Nodes);
                            LeftNode.name = "LeftEdgeNode " + "_ " + LEdgeNodeCount.ToString();
                            LEdgeNodeCount++;
                            EdgeNodes.Add(LeftNode);
                        }
                    }
                }
            }
        }

        CreateWalkableConnections();
        ConnectJumpAndFallNode();

        
    }

    void CreateWalkableConnections()
    {
        for(int i =0; i < nodeList.Count; i++)
        {
            Node nodeA = nodeList[i];

            for(int j = i + 1; j < nodeList.Count; j++)
            {
                if(i == j) continue;

                Node nodeB = nodeList[j];
                Vector3 PosA = nodeA.transform.position;
                Vector3 PosB = nodeB.transform.position;

                float horizontalDistance = Mathf.Abs(PosA.x - PosB.x);
                float verticalDistance = PosB.y - PosA.y;

                // Walkable Nodes: Checking the height and the horizontal nodes are within the distance that is required
                if (horizontalDistance <= 1.0f && Mathf.Abs(verticalDistance) < 0.1f)
                {
                    nodeA.connections.Add(new Node.NodeConnection(nodeB, Node.ConnectionType.walkable));
                    nodeB.connections.Add(new Node.NodeConnection(nodeA, Node.ConnectionType.walkable));
                }
            }
        }

        foreach (Node WalkNode in nodeList)
        {
            foreach (Node edgeNode in EdgeNodes)
            {
                TryConnectWalkableToEdgeNode(WalkNode, edgeNode);
            }
        }

        canDrawGizmos = true;
    }

    void TryConnectWalkableToEdgeNode(Node WalkNode, Node EdgeNode)
    {
        Vector3 PosA = WalkNode.transform.position;
        Vector3 PosB = EdgeNode.transform.position;

        float horizontalDistance = Mathf.Abs(PosA.x - PosB.x);
        float VerticalDistance = PosA.y - PosB.y;

        if(horizontalDistance <= 1.0f && Mathf.Abs(VerticalDistance) < 0.1f)
        {
            WalkNode.connections.Add(new Node.NodeConnection(EdgeNode, Node.ConnectionType.walkable));
            EdgeNode.connections.Add(new Node.NodeConnection(WalkNode, Node.ConnectionType.walkable));
        }
    }

    void ConnectJumpAndFallNode()
    {

        foreach (Node edgeNode in EdgeNodes)
        {

            // In this list each candidate is a tuple holding three values
            List<(Node node, float dist, Node.ConnectionType type)> Connectioncandidate = new();

            foreach (Node OtherEdgeNodes in EdgeNodes)
            {
                if (OtherEdgeNodes == edgeNode) continue;

                TryAddConnectionCandidate(edgeNode, OtherEdgeNodes, ref Connectioncandidate);
            }

            foreach (Node OtherNodes in nodeList)
            {
                if (OtherNodes == edgeNode) continue;

                TryAddConnectionCandidate(edgeNode, OtherNodes, ref Connectioncandidate);
            }

            //Sort by distance
            Connectioncandidate.Sort((a, b) => a.dist.CompareTo(b.dist));

            int maxConnections = Mathf.Min(Connectioncandidate.Count, 3);
            for(int i = 0; i < maxConnections; i++)
            {
                Node other = Connectioncandidate[i].node;
                Node.ConnectionType type = Connectioncandidate[i].type;


                //avoid duplicate connection
                if(edgeNode.connections.Exists(c=>c.targetNode == other)) continue;

                edgeNode.connections.Add(new Node.NodeConnection(other, type));
                other.connections.Add(new Node.NodeConnection(edgeNode, type));
            }

        }    

        canDrawGizmos = true;
    }

    void TryAddConnectionCandidate(Node from, Node to, ref List<(Node node, float dist, Node.ConnectionType type)> Connectioncandidate)
    {
        float distX = Mathf.Abs(from.transform.position.x - to.transform.position.x);
        float distY = from.transform.position.y - to.transform.position.y;

        float distance = Vector2.Distance(from.transform.position, to.transform.position);

        // Check if otherEdgeNode is reachable by jump or fall
        bool canJump = distY > 0 && distY <= maxJumpHeight && distance <= maxJumpDistance;
        bool canFall = distY < 0 && Mathf.Abs(distY) <= maxFallHeight && distance <= maxFallDistance;

        if (!canJump && !canFall) return;

        if (Physics2D.Linecast(from.transform.position, to.transform.position, platformLayer)) return;

        Connectioncandidate.Add((to, distance, canJump ? Node.ConnectionType.jump : Node.ConnectionType.fall));
    }

    void DrawNodeConnections(Node node)
    {
        foreach(var connection in node.connections)
        {
            switch (connection.type)
            {
                case Node.ConnectionType.walkable:
                    Gizmos.color = Color.green;
                    break;

                case Node.ConnectionType.jump:
                    Gizmos.color = Color.blue;
                    break;

                case Node.ConnectionType.fall:
                    Gizmos.color = Color.red;
                    break;
            }

            Gizmos.DrawLine(node.transform.position, connection.targetNode.transform.position);
        }
    }

    private void OnDrawGizmos()
    {
        if (canDrawGizmos)
        {
            //Walkable Node Connections
            foreach(Node node in nodeList)
            {
                DrawNodeConnections(node);
            }

            foreach (Node edgeNode in EdgeNodes)
            {
                DrawNodeConnections(edgeNode);
            }

            //Drawing Edge Node and there connections
            foreach (Node node in EdgeNodes)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawSphere(node.transform.position, 0.25f);
            }
        }
    }

    private void DebugLinesForNodeToNodesWeHaveGenerated()
    {

        for (int i = 0; i < EdgeNodes.Count; i++)
        {
            Node nearestNode = null;
            float nearestNodeDistance = float.MaxValue;

            Node edgeNode = EdgeNodes[i];

            for(int j =  i + 1; j < EdgeNodes.Count; j++)
            {
                Node OtherEdgeNode = EdgeNodes[j];

                if (OtherEdgeNode == edgeNode) continue;

                float distanceX = Mathf.Abs(edgeNode.transform.position.x - OtherEdgeNode.transform.position.x);
                float distanceY = OtherEdgeNode.transform.position.y - edgeNode.transform.position.y;

                float distance = Vector2.Distance(edgeNode.transform.position, OtherEdgeNode.transform.position);

                if (distance < nearestNodeDistance)
                {
                    nearestNodeDistance = distance;
                    nearestNode = OtherEdgeNode;
                }

                //Determine connection type 
                float vertDiff = nearestNode.transform.position.y - edgeNode.transform.position.y;
                Node.ConnectionType connectionType = (vertDiff > 0) ? Node.ConnectionType.jump : Node.ConnectionType.fall;

                //Debug.Log(string.Format("EdgeNode: {0} to OtherEdgeNode: {1} and the distance between them is: {2}, So the XDistance is {3} and the YDistance is: {4} and the maxJump Height is :{5}, maxJump Distance:{6}, and the maxfallHeight is {7}, and the maxfallDistance: {8}, and the connection Type is:{9}",i,j, distance,distanceX,distanceY, maxJumpHeight, maxJumpDistance, maxFallHeight, maxFallDistance, connectionType));
            }
        }
    }

}
