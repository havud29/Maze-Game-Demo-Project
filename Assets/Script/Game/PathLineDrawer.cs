using System.Collections.Generic;
using AsyncGameObjectsDependency.Core;
using R3;
using UnityEditor;
using UnityEngine;
using ZeroMessenger;

namespace Script
{
    public class PathLineDrawer : ADMonoBehaviour
    {
        [ADDependencyInject] private MazeBuilder _mazeBuilder;

        [SerializeField] private CharacterMovement _characterMovement;
        
        [SerializeField] private LineRenderer lineRenderer;
        
        private WorldCell _currentWorldCell;

        protected override void ADOnFilledDependencies()
        {
            base.ADOnFilledDependencies();

            MessageBroker<OnProcessMovementToCell>.Default.Subscribe(x =>
            {
                if (_currentWorldCell != null) _currentWorldCell.GetComponent<MeshRenderer>().enabled = false;
                _currentWorldCell= _mazeBuilder.GetWorldCell(x.Cell);
                _currentWorldCell.GetComponent<MeshRenderer>().enabled = true;
            }).AddTo(this);
        }

        protected override void ADUpdate()
        {
            base.ADUpdate();

            if (_characterMovement.IsMoving && _characterMovement.CurrentPathList.Count > 0)
            {
                lineRenderer.enabled = true;
                DrawPathAlongCells(_characterMovement.CurrentPathList);
            }
            else
            {
                lineRenderer.enabled = false; 
                if (_currentWorldCell != null) _currentWorldCell.GetComponent<MeshRenderer>().enabled = false;

            }
        }

        private void DrawPathAlongCells(List<MazeCell> cells)
        {        
            lineRenderer.positionCount = (cells.Count + 1);
            lineRenderer.SetPosition(0, new Vector3(transform.position.x, 0.1f, transform.position.z));
            for (int i = 0; i < cells.Count; i++)
            {
                var f = _mazeBuilder.ConvertWorldPosToPlayerPos(cells[i]);
                lineRenderer.SetPosition(i+1, new Vector3(f.x, 0.1f, f.z)); 
                
            }
        }
    }
}