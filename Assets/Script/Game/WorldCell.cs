using System;
using AsyncGameObjectsDependency.Core;
using UnityEngine;

namespace Script
{
    public class WorldCell : ADMonoBehaviour
    {
        [ADDependencyInject] private GameManager _gameManager;
        [ADDependencyInject] private MazeBuilder _mazeBuilder;

        public MazeCell Cell;

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player") && _gameManager.IsGameStarted)
            {
                var endCell = _mazeBuilder.GetEndWorldCell();
                if (this == endCell)
                {  
                    var pos = _mazeBuilder.GetCellWorldPosition(endCell.Cell);
                    other.transform.position = new Vector3(pos.x, other.transform.position.y, pos.z);
                    
                    _gameManager.Win();
                  
                }
            }
        }
    }
}