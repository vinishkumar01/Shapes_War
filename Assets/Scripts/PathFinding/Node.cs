using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Node : MonoBehaviour 
{
    public Node cameFrom;
    public ConnectionType connectionType;

    public enum ConnectionType
    {
        walkable,
        jump,
        fall
    }

    public class NodeConnection
    {
        public Node targetNode;
        public ConnectionType type;

        public NodeConnection(Node node, ConnectionType connectiontype)
        {
            targetNode = node;
            type = connectiontype;
        }
    }

    public List<NodeConnection> connections = new List<NodeConnection>();


    //A* Calculations
    public float gScore;
    public float hScore;

    public float fScore()
    {
        return gScore + hScore; 
    }
}


