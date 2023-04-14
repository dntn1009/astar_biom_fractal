using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PathRequestManager : MonoBehaviour
{
    struct PathRequest
    {
        public Vector3 _pathStart;
        public Vector3 _pathEnd;
        public Action<Vector3[], bool> _callback;

        public PathRequest(Vector3 start, Vector3 end, Action<Vector3[], bool> callback)
        {
            _pathStart = start;
            _pathEnd = end;
            _callback = callback;
        }
    }

    static PathRequestManager _instance;

    Queue<PathRequest> _pathRquestQueue = new Queue<PathRequest>();
    PathRequest _currentpathFinding;
    PathFinding _pathFinding;
    bool _isProcessingPath;

    private void Awake()
    {
        _instance = this;
        _pathFinding = GetComponent<PathFinding>();

    }

    void TryProcessNext()
    {
        if(!_isProcessingPath && _pathRquestQueue.Count > 0)
        {
            _currentpathFinding = _pathRquestQueue.Dequeue();
            _isProcessingPath = true;
            // pathFinding에 위치 추가...
            _pathFinding.StarrFindPath(_currentpathFinding._pathStart, _currentpathFinding._pathEnd);
        }
    }

    public void FinishProcessingPath(Vector3[] path, bool success)
    {
        _currentpathFinding._callback(path, success);
        _isProcessingPath = false;
        TryProcessNext();
    }

    static public void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback)
    {
        PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback);
        _instance._pathRquestQueue.Enqueue(newRequest);
        _instance.TryProcessNext();
    }

}
