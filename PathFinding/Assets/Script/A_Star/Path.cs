using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path
{
    public readonly Vector3[] _lookPoints;
    public readonly Line[] _turnBoundaries;
    public readonly int _finishLineIndex;
    public readonly int _slowDownIndex;

    public Path(Vector3[] wayPoints, Vector3 startPoint, float turnDst, float stoppingDst)
    {
        float dstFromEndPoint = 0;
        _lookPoints = wayPoints;
        _turnBoundaries = new Line[_lookPoints.Length];
        _finishLineIndex = _turnBoundaries.Length - 1;
        Vector2 previousPoint = V3ToV2(startPoint);

        for(int n = 0; n < _lookPoints.Length; n++)
        {
            Vector2 currentPoint = V3ToV2(_lookPoints[n]);
            Vector2 dirToCurrentPoint = (currentPoint - previousPoint).normalized;
            Vector2 turnBoundaryPoint = (n == _finishLineIndex) ? currentPoint : currentPoint - dirToCurrentPoint * turnDst;
            _turnBoundaries[n] = new Line(turnBoundaryPoint, previousPoint - dirToCurrentPoint * turnDst);
            previousPoint = turnBoundaryPoint;
        }

        for(int n = _lookPoints.Length -1; n > 0; n--)
        {
            dstFromEndPoint += Vector3.Distance(_lookPoints[n], _lookPoints[n - 1]);
            if(dstFromEndPoint > stoppingDst)
            {
                _slowDownIndex = n;
                break;
            }
        }
    }

    Vector2 V3ToV2(Vector3 v3)
    {
        return new Vector2(v3.x, v3.z);
    }

    public void DrawWithGizmos()
    {
        Gizmos.color = Color.black;
        foreach(Vector3 pos in _lookPoints)
            Gizmos.DrawCube(pos + Vector3.up, Vector3.one);

        Gizmos.color = Color.white;
        foreach (Line l in _turnBoundaries)
            l.DrawWithGizmos(10);
    }
}
