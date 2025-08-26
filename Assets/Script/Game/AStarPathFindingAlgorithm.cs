using System.Collections.Generic;
using UnityEngine;

namespace Script
{
    public class AStarPathFindingAlgorithm
    {
        private Node[,] _nodeGrids;

        public AStarPathFindingAlgorithm(Node[,] grid)
        {
            _nodeGrids = grid;
        }

        public List<Node> FindPath(Vector2Int startPosition, Vector2Int targetPosition)
        {
            Node startNode = _nodeGrids[startPosition.x, startPosition.y];
            Node targetNode = _nodeGrids[targetPosition.x, targetPosition.y];

            List<Node> openList = new List<Node>();
            HashSet<Node> closedList = new HashSet<Node>(); 

            openList.Add(startNode);

            while (openList.Count > 0)
            {
                Node currentNode = GetNodeWithLowestFCost(openList);
                openList.Remove(currentNode);
                closedList.Add(currentNode);

                if (currentNode == targetNode)
                {
                    return RetracePath(startNode, targetNode);
                }
                foreach (var neighborCoor in currentNode.WalkableNeighbours)
                {
                    var neighborNode = _nodeGrids[neighborCoor.x, neighborCoor.y];
                    if (closedList.Contains(neighborNode))
                        continue;

                    float tentativeGCost = currentNode.GCost + GetDistance(currentNode, neighborNode);
                    if (tentativeGCost < neighborNode.GCost || !openList.Contains(neighborNode))
                    {
                        neighborNode.GCost = tentativeGCost;
                        neighborNode.HCost = GetDistance(neighborNode, targetNode);
                        neighborNode.Parent = currentNode;

                        if (!openList.Contains(neighborNode))
                            openList.Add(neighborNode);
                    }
                }
            }

            return new List<Node>(); 
        }

        private Node GetNodeWithLowestFCost(List<Node> openList)
        {
            Node lowestFCostNode = openList[0];
            foreach (Node node in openList)
            {
                if (node.FCost < lowestFCostNode.FCost ||
                    (Mathf.Approximately(node.FCost, lowestFCostNode.FCost) && node.HCost < lowestFCostNode.HCost))
                {
                    lowestFCostNode = node;
                }
            }

            return lowestFCostNode;
        }

        private List<Node> RetracePath(Node startNode, Node endNode)
        {
            List<Node> path = new List<Node>();
            Node currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;
            }

            path.Reverse();
            return path;
        }

        private float GetDistance(Node nodeA, Node nodeB)
        {
            return Mathf.Abs(nodeA.Position.x - nodeB.Position.x) + Mathf.Abs(nodeA.Position.y - nodeB.Position.y);
        }
        
    }
    
    public class Node
    {
        public MazeCell Cell;
        public List<Vector2Int> WalkableNeighbours;
        public Vector2Int Position => Cell.Position;

        public float GCost = 0;
        public float HCost = 0;      
        public Node Parent;    
        
        public float FCost => GCost + HCost;

        public Node(MazeCell cell)
        {
            Cell = cell;
            WalkableNeighbours = cell.GetAccessableNeighbors();
        }
        
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Node otherNode = (Node)obj;
            return Position.Equals(otherNode.Position); 
        }

        public static bool operator ==(Node node1, Node node2)
        {
            if (ReferenceEquals(node1, node2)) return true;
            if (node1 is null || node2 is null) return false;
            return node1.Position == node2.Position;
        }

        public static bool operator !=(Node node1, Node node2)
        {
            return !(node1 == node2);
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }
    }
}