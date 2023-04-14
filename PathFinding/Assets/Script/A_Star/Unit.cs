using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    const float _minPathUpdateTime = 0.2f;
    const float _pathUpdateMoveThreshould = 0.5f;

    public Transform _target;
    public float _turnDest = 5;
    public float _speed = 10;
    public float _turnSpped = 3;
    public float _stoppingDst = 10;


    Path _path;
    //Vector3[] _path;
    int _targetIndex;

    void Start()
    {
        StartCoroutine(UpdatePath());
    }

    IEnumerator UpdatePath()
    {
        if (Time.timeSinceLevelLoad < 0.3f)
            yield return new WaitForSeconds(0.3f);
        PathRequestManager.RequestPath(transform.position, _target.position, OnPathFound);

        float sqrMoveThresould = _pathUpdateMoveThreshould * _pathUpdateMoveThreshould;
        Vector3 targetPosOld = _target.position;

        while(true)
        {
            yield return new WaitForSeconds(_minPathUpdateTime);
            if((_target.position - targetPosOld).sqrMagnitude > sqrMoveThresould)
            {
                PathRequestManager.RequestPath(transform.position, _target.position, OnPathFound);
                targetPosOld = _target.position;
            }
        }
    }

    IEnumerator FollowPath()
    {
        bool followingPath = true;
        int pathIndex = 0;
        float speedPercent = 1;
        transform.LookAt(_path._lookPoints[0]);

        while (followingPath)
        {
            Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
            while(_path._turnBoundaries[pathIndex].HasCorssedLine(pos2D))
            {
                if (pathIndex == _path._finishLineIndex)
                {
                    followingPath = false;
                    break;
                }
                else
                    pathIndex++;
            }
            if(followingPath)
            {
                if(pathIndex >= _path._slowDownIndex && _stoppingDst > 0)
                {
                    speedPercent = Mathf.Clamp01(_path._turnBoundaries[_path._finishLineIndex].DistanceFromPoint(pos2D) / _stoppingDst);
                    if (speedPercent < 0.01f)
                        followingPath = false;
                }
                Quaternion targetRotation = Quaternion.LookRotation(_path._lookPoints[pathIndex] - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * _turnSpped);
                transform.Translate(Vector3.forward * Time.deltaTime * _speed * speedPercent, Space.Self);
            }
            yield return null;
        }
    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if(pathSuccessful)
        {
            //_path = newPath;
            _path = new Path(newPath, transform.position, _turnDest, _stoppingDst);
            //_targetIndex = 0;

            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    void OnDrawGizmos()
    {
        if(_path != null)
        {
            _path.DrawWithGizmos();
           /* for(int n = _targetIndex; n < _path.Length; n++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(_path[n], Vector3.one);

                if (n == _targetIndex)
                    Gizmos.DrawLine(transform.position, _path[n]);
                else
                    Gizmos.DrawLine(_path[n - 1], _path[n]);
            }*/
        }
    }
}
