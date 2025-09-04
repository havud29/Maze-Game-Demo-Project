using System;
using System.Collections.Generic;
using AsyncGameObjectsDependency.Core;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using ZeroMessenger;
using Cysharp;
namespace Script
{
    public class PlayerCharacterController : ADMonoBehaviour
    {
        [ADDependencyInject] private GameManager _gameManager;
        [ADDependencyInject] private MazeBuilder _mazeBuilder;
        
        [SerializeField] private CharacterMovement characterMovement;

        private Queue<OnClickedOnAWorldCell> _messageQueue = new Queue<OnClickedOnAWorldCell>();
        private bool _isQueueLocked = false;
        
        [SerializeField] private Material _defaultMaterial;
        [SerializeField] private Material _hurtMaterial;
        protected override async void ADOnFilledDependencies()
        {
            base.ADOnFilledDependencies();
            
            GetComponent<MeshRenderer>().material = _defaultMaterial;
            _gameManager.RegisterPlayerCharacter(this);
            
            MessageBroker<OnStartedGame>.Default.Subscribe(x =>
            {
                gameObject.transform.position = new Vector3(0, transform.position.y, 0);
            }).AddTo(this);
            
            MessageBroker<OnFinishedBuildingMaze>.Default.Subscribe(x =>
            {
                InitPlayerPosition();  

            }).AddTo(this);
          
            MessageBroker<OnClickedOnAWorldCell>.Default.Subscribe(async x =>
            {
                _messageQueue.Enqueue(x);
            }).AddTo(this);
            
        }

        private async void InitPlayerPosition()
        {
            await UniTask.WaitUntil(() => characterMovement.IsReady);
            characterMovement.InitSpawningPosition(_mazeBuilder.CellGrids[0,0]);

        }
        protected override async void ADUpdate()
        {
            base.ADUpdate();
            if (_messageQueue.Count > 0 && !_isQueueLocked)
            {
                _isQueueLocked = true;
                characterMovement.CancelMovement();
                await UniTask.WaitUntil(() => !characterMovement.IsMoving);
                var cell = _messageQueue.Dequeue().Cell;
                MessageBroker<OnProcessMovementToCell>.Default.Publish(new OnProcessMovementToCell()
                {
                    Cell = cell
                });
                characterMovement.MoveTo(cell).Forget();
                _isQueueLocked = false;
                
            }
        }

        public async void PlayerHittedTrapBehaviour()
        {
            characterMovement.CancelMovement();
            GetComponent<MeshRenderer>().material = _hurtMaterial;
            MessageBroker<EnableClickInput>.Default.Publish(new EnableClickInput(){ IsEnabled = false});
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            
            characterMovement.SetCharacterCellPosition(_mazeBuilder.CellGrids[0,0]);
            GetComponent<MeshRenderer>().material = _defaultMaterial;
            var worldPos = _mazeBuilder.GetCellWorldPosition(_mazeBuilder.CellGrids[0, 0]);
            this.gameObject.transform.position = new Vector3(worldPos.x, transform.position.y, worldPos.z);            MessageBroker<EnableClickInput>.Default.Publish(new EnableClickInput(){ IsEnabled = false});
            MessageBroker<EnableClickInput>.Default.Publish(new EnableClickInput(){ IsEnabled = true});

        }

        public void CancelMovement()
        {
            characterMovement.CancelMovement();
        }
        
    }
    
}

public struct OnProcessMovementToCell
{
    public MazeCell Cell;
}