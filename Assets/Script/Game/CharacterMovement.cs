using System;
using System.Collections.Generic;
using System.Threading;
using AsyncGameObjectsDependency.Core;
using Cysharp.Threading.Tasks;
using R3;
using Script;
using UnityEngine;
using ZeroMessenger;

public class CharacterMovement : ADMonoBehaviour
{
    [ADDependencyInject] private GameManager _gameManager;
    [ADDependencyInject] private MazeBuilder _mazeBuilder;
    
    private AStarPathFindingAlgorithm _pathFindingAlgorithm;
    
    private Vector2Int _currentCellPosition;
    public float moveSpeed = 2f;
    public float reachThreshold = 0.1f;
    
    public bool IsMoving { get; private set; }
    public bool IsReady { get; private set; }
    public List<MazeCell> CurrentPathList { get; private set; }
    
    private CancellationTokenSource cts = new CancellationTokenSource();

    protected override void ADOnFilledDependencies()
    {
        base.ADOnFilledDependencies();
        
        MessageBroker<OnFinishedBuildingMaze>.Default.Subscribe(x =>
        {
            IsReady = true;
        }).AddTo(this);
    }

    public void InitSpawningPosition(MazeCell startCell)
    {
        _pathFindingAlgorithm = new AStarPathFindingAlgorithm(_mazeBuilder.ConverToNodeGrid());
        _currentCellPosition =  startCell.Position;
    }
 
    

    public async UniTask MoveTo(MazeCell cell)
    {
        try
        {
            CurrentPathList = new List<MazeCell>();
            CurrentPathList = FindPath(_currentCellPosition, cell.Position);
            var cellIndex = 0;
            cts = new CancellationTokenSource();
            var token = cts.Token;
            while (CurrentPathList.Count > 0)
            {
                IsMoving = true;
                MazeCell targetCell = CurrentPathList[cellIndex];
                Vector3 targetPosition = _mazeBuilder.ConvertWorldPosToPlayerPos(targetCell);
                if (token.IsCancellationRequested)
                {
                    IsMoving = false;
                    break;
                    
                }

                await MoveToPositionAsync(targetPosition, token);
                if (_mazeBuilder.CheckIfPostionIsInsideThisCell(targetCell, gameObject.transform.position))
                {
                    _currentCellPosition = targetCell.Position;
                }

                CurrentPathList.RemoveAt(0);
                    
            }            
            IsMoving = false;

        }
        catch (OperationCanceledException e)
        {
            IsMoving = false;
        }
        
    }
    
    
    private async UniTask MoveToPositionAsync(Vector3 targetPosition, CancellationToken token)
    {       
        while (Vector3.Distance(transform.position, targetPosition) >= reachThreshold)
        {
            if (token.IsCancellationRequested)
            {
                break; 
            }
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            await UniTask.Yield();
        }

    }
    
    public void CancelMovement()
    {
        if (IsMoving)
        {   
            cts?.Cancel();  
            IsMoving = false;
        }
    }

    public void SetCharacterCellPosition(MazeCell cell)
    {
        _currentCellPosition = cell.Position;
    }
    
    private List<MazeCell> FindPath(Vector2Int startPosition, Vector2Int targetPosition)
    {
        List<MazeCell> path = new List<MazeCell>();
        foreach (var node in _pathFindingAlgorithm.FindPath(startPosition, targetPosition))
        {
            path.Add(node.Cell);
        }

        return path;
    }
    
}
