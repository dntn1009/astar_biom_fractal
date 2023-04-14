using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line
{
    const float _verticalLineGradient = 1e5f;

    float _gradient;
    float _interceptY;
    Vector2 _pointOnLine1;
    Vector2 _pointOnLine2;
    bool _approachSide;
    float _gradientPerpendicular;

    public Line(Vector2 pol, Vector2 pointPerpendicularToLine)
    {
        float dx = pol.x - pointPerpendicularToLine.x;
        float dy = pol.y - pointPerpendicularToLine.y ;
        if (dy == 0)
            _gradientPerpendicular = _verticalLineGradient;
        else
            _gradientPerpendicular = dy / dx;
        if (_gradientPerpendicular == 0)
            _gradient = _verticalLineGradient;
        else
            _gradient = -1 / _gradientPerpendicular;

        _interceptY = pol.y - _gradient * pol.x;
        _pointOnLine1 = pol;
        _pointOnLine2 = pol + new Vector2(1, _gradient);
        _approachSide = false;
        _approachSide = GetSide(pointPerpendicularToLine);
    }
    bool GetSide(Vector2 p)
    {
        return (p.x - _pointOnLine1.x) * (_pointOnLine2.y - _pointOnLine1.y) > (p.y - _pointOnLine1.y) * (_pointOnLine2.x - _pointOnLine1.x);
    }

    public bool HasCorssedLine(Vector2 p)
    {
        return GetSide(p) != _approachSide;
    }

    public float DistanceFromPoint(Vector2 p)
    {
        float yInterceptPerpendicular = p.y - _gradientPerpendicular * p.x;
        float intersectX = (yInterceptPerpendicular - _interceptY) / (_gradient - _gradientPerpendicular);
        float intersectY = _gradient * intersectX + _interceptY;
        return Vector2.Distance(p, new Vector2(intersectX, intersectY));
    }

    public void DrawWithGizmos(float length)
    {
        Vector3 lineDir = new Vector3(1, 0, _gradient).normalized;
        Vector3 lineCenter = new Vector3(_pointOnLine1.x, 0, _pointOnLine1.y) + Vector3.up;
        Gizmos.DrawLine(lineCenter - lineDir * length / 2f, lineCenter + lineDir * length/ 2f);
    }
}
