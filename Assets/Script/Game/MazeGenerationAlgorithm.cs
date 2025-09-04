using System;
using System.Collections.Generic;
using AsyncGameObjectsDependency.Core;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = System.Numerics.Vector2;

public class MazeGenerationAlgorithm 
{
    private int _mazeWidth; 
    private int _mazeHeight;
    private MazeCell[,] _cellGrids;
    private List<MazeWall> _walls = new List<MazeWall>();
    private Stack<MazeCell> _cellStack = new Stack<MazeCell>();
    
    private bool _isDone = false;

    public MazeGenerationAlgorithm()
    {
        
    }

    public (MazeCell[,], List<MazeWall>) GenerateNewMaze(int mazeWidth, int mazeHeight)
    {
        _mazeWidth = mazeWidth;
        _mazeHeight = mazeHeight;
        InitCellGrids();
        InitMazeAlgo();
        
        return (_cellGrids, _walls);
    }

    private void InitMazeAlgo()
    {
        int randRow = UnityEngine.Random.Range(0, _cellGrids.GetLength(0));
        int randCol = UnityEngine.Random.Range(0, _cellGrids.GetLength(1));

        var randomElement = _cellGrids[randRow, randCol];
        randomElement.IsVisitedOnGeneration = true;
        _cellStack.Push(randomElement);
        while (_cellStack.Count > 0)
        {
            var currentCell = _cellStack.Pop();
            var unvisitedNeighbors = new List<KeyValuePair<Direction, Vector2Int>>();
            foreach (var adjacentCell in currentCell.AdjacentCells)
            {
                var coordinates = adjacentCell.Value;
                if (!_cellGrids[coordinates.x, coordinates.y].IsVisitedOnGeneration)
                {
                    unvisitedNeighbors.Add(adjacentCell);
                }
            }
            if (unvisitedNeighbors.Count > 0)
            {
                _cellStack.Push(currentCell);
                var randomNeighbor = unvisitedNeighbors[UnityEngine.Random.Range(0, unvisitedNeighbors.Count)];
                var direction = randomNeighbor.Key;
                var coordinates = randomNeighbor.Value;
                var chosenCell = _cellGrids[coordinates.x, coordinates.y];
                RemoveWall(currentCell, direction);
                RemoveWall(chosenCell, DirectionHelper.GetReverseDirection(direction));
                chosenCell.IsVisitedOnGeneration = true;
                _cellStack.Push(chosenCell);
            }
          
        }

        RemoveStartAndEndWall();
        _isDone = true;

    }
    
    private void InitCellGrids()
    {
        _cellGrids = new MazeCell[_mazeWidth, _mazeHeight];
        for (int i = 0; i < _mazeWidth; i++)
        {
            for (int j = 0; j < _mazeHeight; j++)
            {
                _cellGrids[i, j] = new MazeCell(i, _mazeWidth, j ,_mazeHeight);
                var cell = _cellGrids[i, j];
                foreach (var wall in cell.AdjacentWalls)
                {
                    var w = wall.Value;
                    if (!_walls.Contains(w)) _walls.Add(w);
                }
            }
        }
    }

    private void RemoveStartAndEndWall()
    {
        var startCell = _cellGrids[0, 0];
        var endCell = _cellGrids[_mazeWidth - 1, _mazeHeight - 1];
        RemoveWall(startCell, Direction.South);
        RemoveWall(endCell, Direction.North);

    }

    private void RemoveWall(MazeCell cell, Direction wallDirection)
    {
        var w = cell.AdjacentWalls[wallDirection];
        if(_walls.Contains(w)) _walls.Remove(w);
        cell.AdjacentWalls.Remove(wallDirection);
    }
    
    
}

public enum Direction
{
    North,
    South,
    East,
    West
}

public static class DirectionHelper
{
    public static readonly Dictionary<Direction, Vector2Int> Offsets =
        new Dictionary<Direction, Vector2Int>
        {
            { Direction.West,  new Vector2Int(-1, 0) },
            { Direction.South, new Vector2Int(0, -1) },
            { Direction.East,  new Vector2Int(1, 0) },
            { Direction.North, new Vector2Int(0, 1) },
        };

    public static Direction GetReverseDirection(Direction dir)
    {
        var offset = Offsets[dir];
        var reversedOffset = -offset;
        foreach (var kvp in Offsets)
        {
            if (kvp.Value.Equals(reversedOffset))
            {
                return kvp.Key;
            }
        }
        
        return Direction.North;
    }
}

public class MazeCell
{
    public Vector2Int Position;
    public bool IsVisitedOnGeneration;
    
    public Dictionary<Direction,Vector2Int> AdjacentCells;
    public Dictionary<Direction, MazeWall> AdjacentWalls;

    public MazeCell(int x, int mazeWidth, int y, int mazeHeight)
    {
        Position = new Vector2Int(x, y);
        IsVisitedOnGeneration = false;
        AdjacentCells = new Dictionary<Direction, Vector2Int>(4);
        AdjacentWalls = new Dictionary<Direction, MazeWall>(4);

        foreach (var (dir, offset) in DirectionHelper.Offsets)
        {
            var adjacentCellPos = Position + offset;
            bool insideBounds =
                adjacentCellPos.x >= 0 && adjacentCellPos.x < mazeWidth &&
                adjacentCellPos.y >= 0 && adjacentCellPos.y < mazeHeight;

            if (insideBounds)
            {
                AdjacentCells.Add(dir, adjacentCellPos);
                AdjacentWalls.Add(dir, new MazeWall(Position, adjacentCellPos));
            }
            else
            {
                AdjacentWalls.Add(dir, new MazeWall(Position, adjacentCellPos));
            }
        }
        
    }

    public List<Vector2Int> GetAccessableNeighbors()
    {
        List<Vector2Int> neighbours = new List<Vector2Int>();
        foreach (var stuff in AdjacentCells)
        {
            if (!AdjacentWalls.TryGetValue(stuff.Key, out MazeWall adjacentWall))
            {
                neighbours.Add(stuff.Value);
            }
        }

        if (neighbours.Count == 0)
        {
            Debug.Log("walkable neighbours = zero "+ Position);

        }

        return neighbours;
    }
}

public struct MazeWall : IEquatable<MazeWall>
{
    public Vector2Int StartCell;
    public Vector2Int EndCell;

    public MazeWall(Vector2Int startCell, Vector2Int endCell)
    {
        if (startCell.GetHashCode() < endCell.GetHashCode())
        {
            StartCell = startCell;
            EndCell = endCell;
        }
        else
        {
            StartCell = endCell;
            EndCell = startCell;
        }
    }
    
    public bool Equals(MazeWall other)
    {
        return StartCell.Equals(other.StartCell) && EndCell.Equals(other.EndCell);
    }

    public override bool Equals(object obj)
    {
        return obj is MazeWall other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(StartCell, EndCell);
    }

    public Vector2Int GetWallPosition()
    {
        return (StartCell + EndCell) / 2;
    }
}


