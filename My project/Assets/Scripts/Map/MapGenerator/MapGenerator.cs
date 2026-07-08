using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MapNodeType
{
    Enemy,
    Rest,
    Unknown,
    Treasure,
    Boss
}

[Serializable]
public class MapNodeData
{
    public int id;
    
    public int row;
    public int column;

    public MapNodeType type;

    public List<int> nextNodeIds = new List<int>();
    public List<int> previousNodeIds = new List<int>();

    public int consecutiveEnemyCount;
}

[Serializable]
public class GeneratedMap
{
    public int seed;
    public int rows;
    public int columns;

    public List<MapNodeData> nodes = new List<MapNodeData>();

    public MapNodeData GetNode(int id)
    {
        return nodes.FirstOrDefault(node => node.id == id);
    }

    public List<MapNodeData> GetRow(int row)
    {
        return nodes
            .Where(node => node.row == row)
            .OrderBy(node => node.column)
            .ToList();
    }
}

public class MapGenerator : MonoBehaviour
{
    private const int ColumnCount = 5;
    private const int RowCount = 10;

    private const int FirstRow = 0;
    private const int TreasureRow = 4;
    private const int NoRestRow = 7;
    private const int PreBossRow = 8;
    private const int BossRow = 9;

    private const int BossColumn = 2;

    [Header("Nodes per row")]
    [SerializeField, Range(2, 5)]
    private int minNodesPerRow = 2;

    [SerializeField, Range(2, 5)]
    private int maxNodesPerRow = 5;

    [Header("Random room weights")]
    [SerializeField, Min(0f)]
    private float enemyWeight = 0.55f;

    [SerializeField, Min(0f)]
    private float restWeight = 0.18f;

    [SerializeField, Min(0f)]
    private float unknownWeight = 0.27f;

    private System.Random random;

    public GeneratedMap Generate(int seed)
    {
        if (minNodesPerRow > maxNodesPerRow)
        {
            throw new InvalidOperationException("minNodesPerRow cannot be greater than maxNodesPerRow.");
        }

        random = new System.Random(seed);

        GeneratedMap map = new GeneratedMap
        {
            seed = seed,
            rows = RowCount,
            columns = ColumnCount
        };

        List<List<MapNodeData>> nodesByRow = new List<List<MapNodeData>>();

        int nextNodeId = 0;

        List<int> firstColumns = ChooseFirstRowColumns();

        nodesByRow.Add(CreateRow(FirstRow, firstColumns, map, ref nextNodeId));

        for (int row = 1; row <= PreBossRow; row++)
        {
            List<int> previousColumns = nodesByRow[row - 1].Select(node => node.column).ToList();

            List<int> columns = ChooseNextRowColumns(previousColumns, row);

            nodesByRow.Add(CreateRow(row, columns, map, ref nextNodeId));
        }

        nodesByRow.Add(
            CreateRow(
                BossRow,
                new List<int> { BossColumn },
                map,
                ref nextNodeId
            )
        );

        for (int row = 0; row < BossRow; row++)
        {
            ConnectRows(
                nodesByRow[row],
                nodesByRow[row + 1]
            );
        }

        AssignNodeTypes(map, nodesByRow);

        ValidateMap(map);

        Debug.Log($"Map generated. Seed: {seed}, nodes: {map.nodes.Count}");

        return map;
    }

    private List<MapNodeData> CreateRow(int row, List<int> columns, GeneratedMap map, ref int nextNodeId)
    {
        List<MapNodeData> result = new List<MapNodeData>();

        columns.Sort();

        foreach (int column in columns)
        {
            MapNodeData node = new MapNodeData
            {
                id = nextNodeId++,
                row = row,
                column = column
            };

            map.nodes.Add(node);
            result.Add(node);
        }

        return result;
    }

    private List<int> ChooseFirstRowColumns()
    {
        List<int> availableColumns = Enumerable.Range(0, ColumnCount).ToList();

        Shuffle(availableColumns);

        int count = random.Next(minNodesPerRow, maxNodesPerRow + 1);

        return availableColumns.Take(count).OrderBy(column => column).ToList();
    }

    private List<int> ChooseNextRowColumns(List<int> previousColumns, int targetRow)
    {
        List<int> allowedColumns;

        if (targetRow == PreBossRow)
        {
            allowedColumns = new List<int>
            {
                BossColumn - 1,
                BossColumn,
                BossColumn + 1
            };
        }
        else
        {
            allowedColumns = Enumerable.Range(0, ColumnCount).ToList();
        }

        int maximumCount = Mathf.Min(maxNodesPerRow, allowedColumns.Count);

        List<List<int>> validVariants = new List<List<int>>();

        int variantCount = 1 << allowedColumns.Count;

        for (int mask = 0; mask < variantCount; mask++)
        {
            List<int> candidate = new List<int>();

            for (int index = 0; index < allowedColumns.Count; index++)
            {
                bool selected = (mask & (1 << index)) != 0;

                if (selected) candidate.Add(allowedColumns[index]);
            }

            if (candidate.Count < minNodesPerRow || candidate.Count > maximumCount)
            {
                continue;
            }

            if (IsValidTransition(previousColumns, candidate))
            {
                validVariants.Add(candidate);
            }
        }

        if (validVariants.Count == 0)
        {
            throw new InvalidOperationException($"Cannot generate row {targetRow + 1}.");
        }

        return validVariants[random.Next(validVariants.Count)];
    }

    private bool IsValidTransition(List<int> previousColumns, List<int> nextColumns)
    {
        foreach (int previousColumn in previousColumns)
        {
            bool hasNext = nextColumns.Any(nextColumn => Mathf.Abs(nextColumn - previousColumn) <= 1);

            if (!hasNext)
                return false;
        }
        
        foreach (int nextColumn in nextColumns)
        {
            bool hasPrevious = previousColumns.Any(previousColumn => Mathf.Abs(nextColumn - previousColumn) <= 1);

            if (!hasPrevious)
                return false;
        }

        return true;
    }

    private void ConnectRows(List<MapNodeData> currentRow, List<MapNodeData> nextRow)
    {
        foreach (MapNodeData currentNode in currentRow)
        {
            List<MapNodeData> availableTargets = nextRow.Where(nextNode => Mathf.Abs(nextNode.column - currentNode.column) <= 1).ToList();

            Shuffle(availableTargets);

            int maximumConnections = Mathf.Min(3, availableTargets.Count);

            int connectionCount = random.Next(1, maximumConnections + 1);

            for (int i = 0; i < connectionCount; i++)
            {
                AddConnection(currentNode, availableTargets[i]);
            }
        }

        foreach (MapNodeData nextNode in nextRow)
        {
            if (nextNode.previousNodeIds.Count > 0)
                continue;

            List<MapNodeData> possibleSources = currentRow.Where(currentNode => Mathf.Abs(nextNode.column - currentNode.column) <= 1
                    && !currentNode.nextNodeIds.Contains(nextNode.id)).ToList();

            if (possibleSources.Count == 0)
            {
                throw new InvalidOperationException($"Node {nextNode.id} has no possible parent.");
            }

            MapNodeData source = possibleSources[random.Next(possibleSources.Count)];

            AddConnection(source, nextNode);
        }
    }

    private void AddConnection(MapNodeData from, MapNodeData to)
    {
        if (!from.nextNodeIds.Contains(to.id))
            from.nextNodeIds.Add(to.id);

        if (!to.previousNodeIds.Contains(from.id))
            to.previousNodeIds.Add(from.id);
    }

    private void AssignNodeTypes(GeneratedMap map, List<List<MapNodeData>> nodesByRow)
    {
        Dictionary<int, MapNodeData> nodeLookup = map.nodes.ToDictionary(node => node.id, node => node);

        for (int row = 0; row < RowCount; row++)
        {
            foreach (MapNodeData node in nodesByRow[row])
            {
                List<MapNodeData> parents = node.previousNodeIds.Select(id => nodeLookup[id]).ToList();

                if (row == FirstRow)
                {
                    node.type = MapNodeType.Enemy;
                }
                else if (row == TreasureRow)
                {
                    node.type = MapNodeType.Treasure;
                }
                else if (row == PreBossRow)
                {
                    node.type = MapNodeType.Rest;
                }
                else if (row == BossRow)
                {
                    node.type = MapNodeType.Boss;
                }
                else
                {
                    node.type = ChooseRandomNodeType(row, parents);
                }

                int maximumParentEnemyCount = parents.Count == 0 ? 0 : parents.Max(parent => parent.consecutiveEnemyCount);

                node.consecutiveEnemyCount = node.type == MapNodeType.Enemy ? maximumParentEnemyCount + 1 : 0;
            }
        }
    }

    private MapNodeType ChooseRandomNodeType(int row, List<MapNodeData> parents)
    {
        int maximumParentEnemyCount = parents.Count == 0 ? 0 : parents.Max(parent => parent.consecutiveEnemyCount);
        
        bool canBeEnemy = maximumParentEnemyCount < 3;
        
        bool canBeRest = row != NoRestRow && parents.All(parent => parent.type != MapNodeType.Rest);

        float availableEnemyWeight = canBeEnemy ? enemyWeight : 0f;

        float availableRestWeight = canBeRest ? restWeight : 0f;

        float availableUnknownWeight = Mathf.Max(unknownWeight, 0.001f);

        float totalWeight = availableEnemyWeight + availableRestWeight + availableUnknownWeight;

        double randomValue = random.NextDouble() * totalWeight;

        if (randomValue < availableEnemyWeight) 
            return MapNodeType.Enemy;

        randomValue -= availableEnemyWeight;

        if (randomValue < availableRestWeight)
            return MapNodeType.Rest;

        return MapNodeType.Unknown;
    }

    private void ValidateMap(GeneratedMap map)
    {
        if (map.GetRow(BossRow).Count != 1)
        {
            throw new InvalidOperationException("The boss row must contain exactly one node.");
        }

        foreach (MapNodeData node in map.nodes)
        {
            if (node.row < BossRow)
            {
                if (node.nextNodeIds.Count < 1 || node.nextNodeIds.Count > 3)
                {
                    throw new InvalidOperationException($"Node {node.id} has an invalid number of exits.");
                }
            }
            else if (node.nextNodeIds.Count != 0)
            {
                throw new InvalidOperationException("Boss node cannot have an exit.");
            }

            if (node.row > FirstRow && node.previousNodeIds.Count == 0)
            {
                throw new InvalidOperationException($"Node {node.id} is unreachable.");
            }

            if (node.consecutiveEnemyCount > 3)
            {
                throw new InvalidOperationException($"More than three enemies in a row at node {node.id}.");
            }

            foreach (int nextNodeId in node.nextNodeIds)
            {
                MapNodeData nextNode = map.GetNode(nextNodeId);

                if (nextNode == null)
                {
                    throw new InvalidOperationException($"Connection from {node.id} points to a missing node.");
                }

                if (nextNode.row != node.row + 1)
                {
                    throw new InvalidOperationException("A connection must lead to the next row.");
                }

                if (Mathf.Abs(nextNode.column - node.column) > 1)
                {
                    throw new InvalidOperationException("A connection jumps more than one column.");
                }

                if (node.type == MapNodeType.Rest && nextNode.type == MapNodeType.Rest)
                {
                    throw new InvalidOperationException("Two rest nodes cannot follow each other.");
                }
            }
        }
    }

    private void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = random.Next(i + 1);

            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }
}
