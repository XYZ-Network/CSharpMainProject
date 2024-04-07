using System;
using UnityEngine;

namespace Utilities
{
    [RequireComponent(typeof(Camera))]
    public class CameraPositioning : MonoBehaviour
    {
        [SerializeField] private float _posLerpSpeed = 0.25f;
        [SerializeField] private float _distanceLerpSpeed = 0.25f;
        [SerializeField] private float _expandTargetBy = 8f;
        [SerializeField] private float _startMovingSqrDistance = 16f;
        [SerializeField] private float _stopMovingSqrDistance = 0.001f;
        
        private Camera _camera;
        private Bounds _target;
        private bool _isMoving = false;

        public void SetTarget(Bounds target)
        {
            _target = target;
        }
        
        private void Start()
        {
            ServiceLocator.Register(this);
            _camera = GetComponent<Camera>();
        }

        private void Update()
        {
            var rayToCornerMin = _camera.ScreenPointToRay(Vector3.zero);
            var rayToCornerMax = _camera.ScreenPointToRay(new Vector3(Screen.width, Screen.height));
            var dirToCornerMin = rayToCornerMin.direction;

            var visiblePointMin = rayToCornerMin.GetPoint(rayToCornerMin.origin.y / Mathf.Abs(dirToCornerMin.y));
            var visiblePointMax = rayToCornerMax.GetPoint(rayToCornerMax.origin.y / Mathf.Abs(rayToCornerMax.direction.y));
            
            // to guarantee zero plane inclusion
            visiblePointMin.y = -0.5f;
            visiblePointMax.y = 0.5f;
            
            var visibleBounds = new Bounds();
            visibleBounds.SetMinMax(Vector3.Min(visiblePointMin, visiblePointMax), Vector3.Max(visiblePointMin, visiblePointMax));
            
            var expTarget = _target;
            expTarget.Expand(_expandTargetBy);
            
            var xToY = Mathf.Abs(dirToCornerMin.x / dirToCornerMin.y);
            var zToY = Mathf.Abs(dirToCornerMin.z / dirToCornerMin.y);
            var heightByX = expTarget.extents.x / xToY;
            var heightByZ = expTarget.extents.z / zToY;

            var targetHeight = Mathf.Max(heightByX, heightByZ);

            var oldPos = transform.position;
            var targetPos = new Vector3(expTarget.center.x, targetHeight, expTarget.center.z);
            if ((targetPos - oldPos).sqrMagnitude > _startMovingSqrDistance)
                _isMoving = true;
            
            var x = Mathf.Lerp(oldPos.x, targetPos.x, _posLerpSpeed);
            var y = Mathf.Lerp(oldPos.y, targetPos.y, _distanceLerpSpeed);
            var z = Mathf.Lerp(oldPos.z, targetPos.z, _posLerpSpeed);

            var newPos = new Vector3(x, y, z);
            if (!_isMoving && visibleBounds.Contains(_target.min) && visibleBounds.Contains(_target.max))
                return;

            transform.position = newPos;
            if ((newPos - targetPos).sqrMagnitude < _stopMovingSqrDistance)
                _isMoving = false;
        }

        private void OnDrawGizmosSelected()
        {
            if (_target.extents == Vector3.zero)
                return;
            
            Gizmos.color = Color.green;

            var size = _target.size;
            size.y += 3f;   // to make it more visible
            Gizmos.DrawCube(_target.center, size);
            Gizmos.DrawSphere(_target.center + Vector3.up * 2f, 0.5f);
        }
    }
}