using AsyncGameObjectsDependency.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using R3;
using ZeroMessenger;

namespace Script
{
    public class PointAndClickMoveHandler : ADMonoBehaviour
    {
        [SerializeField] private Camera _camera;
        private LayerMask _cellLayer;
        private Mouse _mouse;

        private bool IsLockedInput = true;

        protected override void ADOnFilledDependencies()
        {
            base.ADOnFilledDependencies();
            _cellLayer = LayerMask.GetMask("Cell");
            _mouse = Mouse.current;
            MessageBroker<OnStartedGame>.Default.Subscribe(x =>
            {
                IsLockedInput = true;
            }).AddTo(this);
            
            MessageBroker<OnFinishedAnimationSequence>.Default.Subscribe(x =>
            {
                IsLockedInput = false;
            }).AddTo(this);

            MessageBroker<IsWinGame>.Default.Subscribe(x =>
            {
                IsLockedInput = true;
            }).AddTo(this);
            
            MessageBroker<EnableClickInput>.Default.Subscribe(x =>
            {
                IsLockedInput = !x.IsEnabled;
            }).AddTo(this);

        }

        protected override void ADUpdate()
        {
            base.ADUpdate();
            if (_mouse != null && _mouse.leftButton.wasPressedThisFrame && !IsLockedInput)
            {
                HandleWorldClick();
            }
        }
        
        void HandleWorldClick()
        {
            DetectObjectOnClick();
        }

        void DetectObjectOnClick()
        {
            Ray ray = _camera.ScreenPointToRay(_mouse.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _cellLayer))
            {
                GameObject clickedObject = hit.collider.gameObject; 
                MessageBroker<OnClickedOnAWorldCell>.Default.Publish(new OnClickedOnAWorldCell()
                {
                    Cell = clickedObject.GetComponent<WorldCell>().Cell
                });
                
                Debug.Log(clickedObject.GetComponent<WorldCell>().Cell.Position);
            }
        }
        
        Vector3 GetWorldPositionFromClick(Vector3 screenPosition)
        {
            Ray ray = _camera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _cellLayer))
            {
                return hit.point;
            }

            return Vector3.negativeInfinity;
        }
    }
    
}

public struct OnClickedOnAWorldCell
{
    public MazeCell Cell;
}

public struct EnableClickInput
{
    public bool IsEnabled;
}
