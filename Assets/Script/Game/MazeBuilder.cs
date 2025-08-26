using System;
using System.Collections.Generic;
using System.Numerics;
using AsyncGameObjectsDependency.Core;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using ZeroMessenger;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Script
{
    public class MazeBuilder : ADMonoBehaviour
    {
        public MazeGenerationAlgorithm _mazeGenerationAlgorithm = new MazeGenerationAlgorithm();
        
        [SerializeField] private int _mazeWidth = 25;
        [SerializeField] private int _mazeHeight = 25;    
        
        public float StepsSize;
        public float ScaleVariable => StepsSize / 2;
        
        public Vector3 WallObjectRatio = new Vector3(5, 1, 0.5f); // preferred size
        
        [SerializeField] private Transform _mazeRoot;
        [SerializeField] private GameObject _wallPrefab;
        [SerializeField] private Transform _gridRoot;
        [SerializeField] private GameObject _gridCubePrefab;
        
        private MazeCell[,] _currentCellGrids;
        public MazeCell[,] CellGrids => _currentCellGrids;

        private List<MazeWall> _currentWalls;

        private List<GameObject> _currentWorldWalls;
        private List<WorldCell> _currentWorldCells;
        protected override void ADOnFilledDependencies()
        {
            base.ADOnFilledDependencies(); 
        
            DiContainer.Register(this);

            MessageBroker<OnResetingGame>.Default.Subscribe(x =>
            {
                CleanUpObjects();
            }).AddTo(this);
            
            MessageBroker<OnStartedGame>.Default.Subscribe(async x =>
            {
                await UniTask.Yield();
                BuildMaze();
            }).AddTo(this);
        }
        
        
        public async void BuildMaze()
        {
            var result = _mazeGenerationAlgorithm.GenerateNewMaze(_mazeWidth, _mazeWidth);
            _currentCellGrids = result.Item1;
            _currentWalls = result.Item2;
            
            BuildWalls();
            BuildGrid();
            await UniTask.Yield();
            MessageBroker<OnFinishedBuildingMaze>.Default.Publish(new OnFinishedBuildingMaze());
        }

        private void BuildWalls()
        {
            var w = _currentWalls;
            var q = StepsSize / WallObjectRatio.x;
            var wallScale = new Vector3(StepsSize, 1, q * WallObjectRatio.y);
            _currentWorldWalls = new List<GameObject>();
            foreach (var wall in w)
            {
                Vector3 start = new Vector3(wall.StartCell.x, 0, wall.StartCell.y);
                Vector3 end   = new Vector3(wall.EndCell.x,   0, wall.EndCell.y);

                Vector3 mid = ((start + end) / 2f) * StepsSize;
                Vector3 dir = (end - start).normalized;
                Vector3 perp = new Vector3(-dir.z, 0, dir.x);
                
                var obj = PoolManager.SpawnObject(_wallPrefab, mid, Quaternion.identity);
                obj.transform.localScale = wallScale;
                _currentWorldWalls.Add(obj);
                obj.transform.parent = _mazeRoot.transform;
                if (Mathf.Abs(dir.x) > 0)
                {
                    obj.transform.rotation = Quaternion.Euler(0, 90, 0);
                }
            }
        }


        private void BuildGrid()
        {
            var g = _currentCellGrids;
            var q = StepsSize;
            _currentWorldCells = new List<WorldCell>();
            foreach (var cell in g)
            {
                var coor = cell.Position;
                var worldPos = new Vector3(coor.x * q, 0, coor.y * q);

                var obj = PoolManager.SpawnObject(_gridCubePrefab, worldPos, Quaternion.identity);
                obj.GetComponent<WorldCell>().Cell = cell;
                _currentWorldCells.Add(obj.GetComponent<WorldCell>());
                obj.transform.parent = _gridRoot.transform;
                obj.transform.localScale = Vector3.one * ScaleVariable;
            }
        }

        private void CleanUpObjects()
        {
            foreach (var c in _currentWorldCells)
            {
                PoolManager.ReleaseObject(c.gameObject);
            }
            
            foreach (var w in _currentWorldWalls)
            {
                PoolManager.ReleaseObject(w);
            }

            _currentWorldWalls = new List<GameObject>();
            _currentWorldCells = new List<WorldCell>();
            
        }
        
        public Vector3 GetCellWorldPosition(MazeCell cell)
        {
            return new Vector3(cell.Position.x, 0, cell.Position.y) * StepsSize;
        }

        public WorldCell GetStartWorldCell()
        {
            return GetWorldCell(CellGrids[0, 0]);
        }
        
        public WorldCell GetEndWorldCell()
        {
            return GetWorldCell(CellGrids[_mazeWidth-1, _mazeHeight-1]);
        }
        public Vector3 ConvertWorldPosToPlayerPos(MazeCell cell)
        {
            var worldPos = GetCellWorldPosition(cell);
            return new Vector3(worldPos.x, 1, worldPos.z);
        }

        public bool CheckIfPostionIsInsideThisCell(MazeCell cell, Vector3 pos)
        {
            var t = GetCellWorldPosition(cell);
            var distance = Vector3.Distance(new Vector3(pos.x, 0, pos.z), 
                new Vector3(t.x, 0, t.z));
            var result = (distance <= StepsSize / 2);
            Debug.Log("Check if postion is inside this cell = "+ result);
            return result;
        }
        public Node[,] ConverToNodeGrid()
        {
            Node[,] nodeGrids = new Node[_mazeWidth, _mazeHeight];
            foreach (var c in _currentCellGrids)
            {
                nodeGrids[c.Position.x, c.Position.y] = new Node(c);
            }

            return nodeGrids;
        }

        public WorldCell GetWorldCell(MazeCell cell)
        {
           return _currentWorldCells.Find(x => x.Cell == cell);
        }
    }
}

public struct OnFinishedBuildingMaze
{
    
}