using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : IHeapItem<Node>
{

    public bool _walkable;
    public Vector3 _worldPosition;

    public int _gridX;
    public int _gridY;
    public int _movementPenalty;



    public int _gCost;
    public int _hCost;
    public Node _parent;
    int _heapIdx;


    public int _fcost
    {
        get { return _gCost + _hCost; }
    }

    public int _heapIndex
    {
        get { return _heapIdx; }
        set { _heapIdx = value; }
    }


    public Node(bool walkable, Vector3 pos, int gX, int gY, int penalty)
    {
        _walkable = walkable;
        _worldPosition = pos;
        _gridX = gX;
        _gridY = gY;
        _movementPenalty = penalty;
    }

    public int CompareTo(Node other)
    {
        int compare = _fcost.CompareTo(other._fcost);
        if(compare == 0)
        {
            compare = _hCost.CompareTo(other._hCost);
        }
        return -compare;
    }
}