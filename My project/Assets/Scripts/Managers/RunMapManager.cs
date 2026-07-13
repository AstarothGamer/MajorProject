using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(MapGenerator))]
public class RunMapManager : MonoBehaviour
{
    public static RunMapManager Instance { get; private set; }

    [SerializeField] private string mapSceneName = "MapScene";
    
    [Header("Encounter Scenes Names")]
    [SerializeField] private string enemySceneName = "FightingScene";
    [SerializeField] private string restSceneName = "RestScene";
    [SerializeField] private string unknownSceneName = "MapScene";
    [SerializeField] private string treasureSceneName = "TreasuresScene";
    [SerializeField] private string bossSceneName = "TallManScene";
    
    private readonly List<int> visitedNodeIds = new List<int>();

    public IReadOnlyList<int> VisitedNodeIds => visitedNodeIds;

    public GeneratedMap CurrentMap { get; private set; }

    public int CurrentNodeId { get; private set; } = -1;
    public int CurrentRow { get; private set; } = -1;

    private MapGenerator mapGenerator;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        mapGenerator = GetComponent<MapGenerator>();
    }
    
    public void StartNewRun()
    {
        int seed = unchecked(System.Environment.TickCount * 31 + System.Guid.NewGuid().GetHashCode());

        CurrentMap = mapGenerator.Generate(seed);

        CurrentNodeId = -1;
        CurrentRow = -1;

        visitedNodeIds.Clear();

        SceneManager.LoadScene(mapSceneName);
    }

    public List<MapNodeData> GetAvailableNodes()
    {
        if (CurrentMap == null)
            return new List<MapNodeData>();
        
        if (CurrentNodeId < 0)
        {
            return CurrentMap.GetRow(0);
        }

        MapNodeData currentNode = CurrentMap.GetNode(CurrentNodeId);

        if (currentNode == null)
            return new List<MapNodeData>();

        return currentNode.nextNodeIds.Select(CurrentMap.GetNode).Where(node => node != null).ToList();
    }

    public bool CanMoveTo(int nodeId)
    {
        return GetAvailableNodes().Any(node => node.id == nodeId);
    }

    public bool TryMoveTo(int nodeId)
    {
        if (!CanMoveTo(nodeId))
        {
            Debug.LogWarning($"Cannot move to node {nodeId}.");

            return false;
        }

        MapNodeData targetNode = CurrentMap.GetNode(nodeId);

        CurrentNodeId = targetNode.id;
        CurrentRow = targetNode.row;

        if (!visitedNodeIds.Contains(targetNode.id))
            visitedNodeIds.Add(targetNode.id);

        Debug.Log($"Moved to row {CurrentRow + 1}, " +$"column {targetNode.column + 1}, " + $"type {targetNode.type}.");

        StartEncounter(targetNode);

        return true;
    }
    
    public bool IsConnectionVisited(int fromNodeId, int toNodeId)
    {
        for (int i = 0; i < visitedNodeIds.Count - 1; i++)
        {
            if (visitedNodeIds[i] == fromNodeId && visitedNodeIds[i + 1] == toNodeId)
            {
                return true;
            }
        }

        return false;
    }

    private void StartEncounter(MapNodeData node)
    {
        string targetScene = null;

        switch (node.type)
        {
            case MapNodeType.Enemy:
                targetScene = enemySceneName;
                break;

            case MapNodeType.Rest:
                targetScene = restSceneName;
                break;

            case MapNodeType.Unknown:
                targetScene = unknownSceneName;
                break;

            case MapNodeType.Treasure: 
                targetScene = treasureSceneName;
                break;

            case MapNodeType.Boss:
                targetScene = bossSceneName;
                break;
        }

        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogWarning($"Scene is not configured for {node.type}.");

            return;
        }

        SceneManager.LoadScene(targetScene);
    }

    public int CurrentLevel
    {
        get
        {
            return CurrentRow + 1;
        }
    }
    
    public int CurrentAvailableLevel
    {
        get
        {
            if (CurrentMap == null)
                return 0;
            
            if (CurrentNodeId < 0)
                return 1;

            MapNodeData currentNode = CurrentMap.GetNode(CurrentNodeId);

            if (currentNode == null)
                return 1;

            if (currentNode.nextNodeIds == null || currentNode.nextNodeIds.Count == 0)
                return currentNode.row + 1;

            return currentNode.row + 2;
        }
    }
}
