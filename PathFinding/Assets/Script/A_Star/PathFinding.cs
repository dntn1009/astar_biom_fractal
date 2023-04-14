using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using System.Diagnostics;

public class PathFinding : MonoBehaviour
{
    /*public Transform _seeker;
    public Transform _Target;*/

    PathRequestManager _requestManager;

    Gride _grid;

    void Awake()
    {
        _requestManager = GetComponent<PathRequestManager>();
        _grid = GetComponent<Gride>();
    }

    public void StarrFindPath(Vector3 startPos, Vector3 targetPos)
    {
        StartCoroutine(FindPath(startPos, targetPos));
    }

    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        Node startNode = _grid.NodeFromWorldPoint(startPos);
        Node targetNode = _grid.NodeFromWorldPoint(targetPos);
        startNode._parent = startNode;

        if(startNode._walkable && targetNode._walkable)
        {
            Heap<Node> openSet = new Heap<Node>(_grid._maxSize);
            HashSet<Node> closeSet = new HashSet<Node>();
            openSet.Add(startNode);

            while(openSet._count > 0)
            {
                Node currentNode = openSet.RemoveFirst();
                closeSet.Add(currentNode);
                if(currentNode == targetNode)
                {
                    sw.Stop();
                    print("Path found : " + sw.ElapsedMilliseconds + " ms");
                    pathSuccess = true;
                    break;
                }

                foreach(Node neighbour in _grid.GetNeighbours(currentNode))
                {
                    if (!neighbour._walkable || closeSet.Contains(neighbour))
                        continue;

                    int newMovementCostToneighbour = currentNode._gCost + GetDistance(currentNode, neighbour) + neighbour._movementPenalty;
                    if(newMovementCostToneighbour < neighbour._gCost || !openSet.Contains(neighbour))
                    {
                        neighbour._gCost = newMovementCostToneighbour;
                        neighbour._hCost = GetDistance(neighbour, targetNode);
                        neighbour._parent = currentNode;
                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                    }
                }
            }
        }
        yield return null;
        if (pathSuccess)
            waypoints = RetreacePath(startNode, targetNode);

        _requestManager.FinishProcessingPath(waypoints, pathSuccess);
    }

/*    void Update()
    {
        FindPath(_seeker.position, _Target.position);   
    }

    void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = _grid.NodeFromWorldPoint(startPos);
        Node targetNode = _grid.NodeFromWorldPoint(targetPos);

        //List<Node> openSet = new List<Node>();
        Heap<Node> openSet = new Heap<Node>(_grid._maxSize);
        HashSet<Node> closeSet = new HashSet<Node>();
        openSet.Add(startNode);

        while(openSet._count > 0)
        {
            *//*Node node = openSet[0];
            for(int n = 1; n < openSet.Count; n++) 
            {
                if (openSet[n]._fcost < node._fcost || openSet[n]._fcost == node._fcost) 
                {
                    if (openSet[n]._hCost < node._hCost)
                        node = openSet[n];
                }
            }
            openSet.Remove(node);*//*
            Node node = openSet.RemoveFirst();
            closeSet.Add(node);

            if (node == targetNode)
            {
                // path를 만들어 _grid의 패스에 저장.
                RetracePath(startNode, targetNode);
                return;
            }

            foreach(Node neighbour in _grid.GetNeighbours(node))
            {
                if (!neighbour._walkable || closeSet.Contains(neighbour))
                    continue;
                int newCostToNeighbour = node._gCost + GetDistance(node, neighbour);
                if(newCostToNeighbour < neighbour._gCost || !openSet.Contains(neighbour))
                {
                    neighbour._gCost = newCostToNeighbour;
                    neighbour._hCost = GetDistance(neighbour, targetNode);
                    neighbour._parent = node;
                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }
    }*/

    Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        waypoints.Add(path[0]._worldPosition);
        for(int n = 1; n < path.Count; n++)
        {
            Vector2 directionNew = new Vector2(path[n - 1]._gridX - path[n]._gridX, 
                                                path[n - 1]._gridY - path[n]._gridY);

            if(directionNew != directionOld)
            {
                waypoints.Add(path[n]._worldPosition);
            }
            directionOld = directionNew;
        }
        return waypoints.ToArray();
    }

    //void RetracePath(Node startNode, Node endNode)
    Vector3[] RetreacePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node CurrentNode = endNode;
        while(CurrentNode != startNode)
        {
            path.Add(CurrentNode);
            CurrentNode = CurrentNode._parent;
        }
        //path.Reverse();
        //_grid._path = path;
        Vector3[] waypoint = SimplifyPath(path);
        Array.Reverse(waypoint);

        return waypoint;
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA._gridX - nodeB._gridX);
        int dstY = Mathf.Abs(nodeA._gridY - nodeB._gridY);

        if(dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

}
