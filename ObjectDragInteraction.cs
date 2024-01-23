using System;
using CandyCoded;
using UnityEngine;
using UnityEngine.Events;

namespace ScottDoxey.Interactions
{

    [Serializable]
    public struct ObjectDragInteractionConstraints
    {

        public bool x;

        public bool y;

        public bool z;

    }

    [Serializable]
    public class ObjectInteractionGrabEvent : UnityEvent<Vector3>
    {

    }

    [Serializable]
    public class ObjectInteractionReleaseEvent : UnityEvent<Vector3, Vector3, Vector3, Vector3>
    {

    }

    public class ObjectDragInteraction : MonoBehaviour
    {

        private const float DAMPEN_INPUT_POSITION_SPEED = 0.01f;

        public ObjectDragInteractionConstraints Constraints;

        public ObjectInteractionGrabEvent Grabbed;

        public ObjectInteractionReleaseEvent Released;

        private int? _currentFingerId;

        private Vector3? _dampenedInputPosition;

        private float? _dragStartDistance;

        private Vector3? _dragStartOffset;

        private Vector3? _dragStartPosition;

        private Vector3? _lastInputPosition;

        private Camera _mainCamera;

        private void Awake()
        {

            _mainCamera = Camera.main;

        }

        private void Update()
        {

            if (gameObject.GetInputDown(_mainCamera, ref _currentFingerId, out RaycastHit hit))
            {

                _dampenedInputPosition = InputManager.GetInputPosition(_currentFingerId);

                _dragStartPosition = gameObject.transform.position;

                _dragStartDistance = hit.distance;

                _dragStartOffset = _dragStartPosition.Value - hit.point;

                Grabbed?.Invoke(_dragStartPosition.Value);

            }

            if (_dragStartPosition.HasValue)
            {

                _lastInputPosition = InputManager.GetInputPosition(_currentFingerId);

                if (_lastInputPosition.HasValue && _dragStartDistance.HasValue && _dragStartOffset.HasValue)
                {

                    var newPosition = _mainCamera.ScreenPointToRay(_lastInputPosition.Value)
                                          .GetPoint(_dragStartDistance.Value) +
                                      _dragStartOffset.Value;

                    if (Constraints.x) newPosition.x = _dragStartPosition.Value.x;
                    if (Constraints.y) newPosition.y = _dragStartPosition.Value.y;
                    if (Constraints.z) newPosition.z = _dragStartPosition.Value.z;

                    gameObject.transform.position = newPosition;

                }

                if (_lastInputPosition.HasValue && _dampenedInputPosition.HasValue)
                {

                    _dampenedInputPosition = Vector3.Lerp(_dampenedInputPosition.Value, _lastInputPosition.Value,
                        DAMPEN_INPUT_POSITION_SPEED);

                }

            }

            if (InputManager.GetInputUp(ref _currentFingerId))
            {

                if (_lastInputPosition.HasValue && _dampenedInputPosition.HasValue && _dragStartPosition.HasValue &&
                    _dragStartDistance.HasValue)
                {

                    var lastInputPositionWorld = _mainCamera.ScreenToWorldPoint(new Vector3(_lastInputPosition.Value.x,
                        _lastInputPosition.Value.y, _dragStartDistance.Value));

                    var dampenedInputPositionWorld = _mainCamera.ScreenToWorldPoint(
                        new Vector3(_dampenedInputPosition.Value.x, _dampenedInputPosition.Value.y,
                            _dragStartDistance.Value));

                    var velocity = lastInputPositionWorld - dampenedInputPositionWorld;

                    Released?.Invoke(_dragStartPosition.Value, gameObject.transform.position, velocity, Vector3.zero);

                }

                _dragStartPosition = null;

                _dragStartDistance = null;

                _dragStartOffset = null;

            }

        }

    }

}
