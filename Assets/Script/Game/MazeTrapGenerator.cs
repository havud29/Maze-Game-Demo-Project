using System.Collections.Generic;
using AsyncGameObjectsDependency.Core;
using R3;
using UnityEngine;
using ZeroMessenger;

namespace Script
{
    public class MazeTrapGenerator : ADMonoBehaviour
    {
        
        [ADDependencyInject] private MazeBuilder _mazeBuilder;
        
        [SerializeField] private Transform _mapObjectRoot;
        [SerializeField] private ThornTrap _thornTrap;
        public float SpawnRate = 0.125f;
        
        private List<GameObject> _trappedObjects;
        protected override void ADOnFilledDependencies()
        {
            base.ADOnFilledDependencies();

            MessageBroker<OnFinishedBuildingMaze>.Default.Subscribe(x =>
            {
                CreateTraps();
            }).AddTo(this);
            
            MessageBroker<OnResetingGame>.Default.Subscribe(x =>
            {
                CleanUpObjects();
            }).AddTo(this);
        }

        public void CreateTraps()
        {
            var currentGrid = _mazeBuilder.CellGrids;
            _trappedObjects = new List<GameObject>();
            foreach (var cell in currentGrid)
            {
                //skip the first cell
                if(cell.Position.Equals(new Vector2Int(0,0))) continue;
                if (cell.Equals(currentGrid[currentGrid.GetLength(0) - 1, currentGrid.GetLength(1) - 1]))
                    continue;

                if (Random.value < SpawnRate)
                {
                    var pos = _mazeBuilder.GetCellWorldPosition(cell);
                    var obj = PoolManager.SpawnObject(_thornTrap.gameObject, pos, Quaternion.identity);
                    _trappedObjects.Add(obj);
                    obj.transform.SetParent(_mapObjectRoot);
                    obj.transform.localScale = Vector3.one * _mazeBuilder.ScaleVariable;
                }

            }
        }


        private void CleanUpObjects()
        {
            foreach (var obj in _trappedObjects)
            {
                PoolManager.ReleaseObject(obj);
            }
        }
    }
}