using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapView : MonoBehaviour
{
    [Header("Containers")]
    [SerializeField] private RectTransform mapContent;
    [SerializeField] private RectTransform linesContainer;
    [SerializeField] private RectTransform nodesContainer;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Prefabs")]
    [SerializeField] private MapNodeView nodePrefab;
    [SerializeField] private MapConnectionView connectionPrefab;

    [Header("Map Layout")]
    [SerializeField] private float columnSpacing = 180f;
    [SerializeField] private float rowSpacing = 220f;
    [SerializeField] private float horizontalPadding = 150f;
    [SerializeField] private float bottomPadding = 120f;
    [SerializeField] private float topPadding = 180f;

    [Header("Room Icons")]
    [SerializeField] private Sprite enemyIcon;
    [SerializeField] private Sprite restIcon;
    [SerializeField] private Sprite unknownIcon;
    [SerializeField] private Sprite treasureIcon;
    [SerializeField] private Sprite bossIcon;

    private readonly Dictionary<int, MapNodeView> nodeViews = new Dictionary<int, MapNodeView>();

    private readonly List<ConnectionRecord> connectionViews = new List<ConnectionRecord>();

    private class ConnectionRecord
    {
        public int fromNodeId;
        public int toNodeId;
        public MapConnectionView view;
    }

    private IEnumerator Start()
    {
        yield return null;

        BuildMap();

        yield return null;

        Canvas.ForceUpdateCanvases();

        ScrollToPlayerPosition();
    }

    public void BuildMap()
    {
        if (RunMapManager.Instance == null)
        {
            Debug.LogError("RunMapManager was not found.");
            return;
        }

        GeneratedMap map = RunMapManager.Instance.CurrentMap;

        if (map == null)
        {
            Debug.LogError("The generated map does not exist. " + "Start the run from the main menu.");

            return;
        }

        ClearMap();
        ConfigureContentSize(map);
        ConfigureContainers();

        CreateNodes(map);
        CreateConnections(map);

        RefreshMapState();
    }

    private void ConfigureContentSize(GeneratedMap map)
    {
        float width = (map.columns - 1) * columnSpacing + horizontalPadding * 2f;

        float height = (map.rows - 1) * rowSpacing + bottomPadding + topPadding;

        mapContent.sizeDelta = new Vector2(width, height);
    }

    private void ConfigureContainers()
    {
        StretchContainer(linesContainer);
        StretchContainer(nodesContainer);

        linesContainer.SetAsLastSibling();
        nodesContainer.SetAsLastSibling();
    }

    private void StretchContainer(RectTransform container)
    {
        container.anchorMin = Vector2.zero;
        container.anchorMax = Vector2.one;
        container.pivot = new Vector2(0.5f, 0.5f);

        container.offsetMin = Vector2.zero;
        container.offsetMax = Vector2.zero;
    }

    private void CreateNodes(GeneratedMap map)
    {
        foreach (MapNodeData node in map.nodes)
        {
            MapNodeView nodeView = Instantiate(nodePrefab, nodesContainer);

            RectTransform rect = nodeView.GetComponent<RectTransform>();

            rect.anchorMin = new Vector2(0.5f, 0f);

            rect.anchorMax = new Vector2(0.5f, 0f);

            rect.pivot = new Vector2(0.5f, 0.5f);

            rect.anchoredPosition = GetNodePosition(node, map.columns);

            nodeView.Initialize(node, GetIcon(node.type), OnNodeClicked);

            nodeViews.Add(node.id, nodeView);
        }
    }

    private void CreateConnections(GeneratedMap map)
    {
        foreach (MapNodeData fromNode in map.nodes)
        {
            foreach (int toNodeId in fromNode.nextNodeIds)
            {
                if (!nodeViews.TryGetValue(fromNode.id, out MapNodeView fromView))
                {
                    continue;
                }

                if (!nodeViews.TryGetValue(toNodeId, out MapNodeView toView))
                {
                    continue;
                }

                MapConnectionView connection =
                    Instantiate(connectionPrefab, linesContainer);

                connection.Initialize(fromView.RectTransform.anchoredPosition, toView.RectTransform.anchoredPosition);

                connectionViews.Add(new ConnectionRecord { fromNodeId = fromNode.id, toNodeId = toNodeId, view = connection}
                );
            }
        }
    }

    private Vector2 GetNodePosition(MapNodeData node, int columnCount)
    {
        float centerColumn = (columnCount - 1) / 2f;

        float x = (node.column - centerColumn) * columnSpacing;

        float y = bottomPadding + node.row * rowSpacing;

        return new Vector2(x, y);
    }

    private Sprite GetIcon(MapNodeType type)
    {
        switch (type)
        {
            case MapNodeType.Enemy:
                return enemyIcon;

            case MapNodeType.Rest:
                return restIcon;

            case MapNodeType.Unknown:
                return unknownIcon;

            case MapNodeType.Treasure:
                return treasureIcon;

            case MapNodeType.Boss:
                return bossIcon;

            default:
                return null;
        }
    }

    private void OnNodeClicked(int nodeId)
    {
        bool moved = RunMapManager.Instance.TryMoveTo(nodeId);

        if (!moved)
            return;

        RefreshMapState();
    }

    public void RefreshMapState()
    {
        RunMapManager manager = RunMapManager.Instance;

        HashSet<int> availableIds = new HashSet<int>();

        foreach (MapNodeData availableNode in manager.GetAvailableNodes())
        {
            availableIds.Add(availableNode.id);
        }

        HashSet<int> visitedIds = new HashSet<int>(manager.VisitedNodeIds);

        foreach (KeyValuePair<int, MapNodeView> pair in nodeViews)
        {
            int nodeId = pair.Key;

            bool isAvailable = availableIds.Contains(nodeId);

            bool isVisited = visitedIds.Contains(nodeId);

            bool isCurrent = manager.CurrentNodeId == nodeId;

            pair.Value.SetState(isAvailable, isVisited, isCurrent);
        }

        foreach (ConnectionRecord connection in connectionViews)
        {
            bool isAvailable = manager.CurrentNodeId == connection.fromNodeId && availableIds.Contains(connection.toNodeId);

            bool isTravelled = manager.IsConnectionVisited(connection.fromNodeId, connection.toNodeId);

            connection.view.SetState(isAvailable, isTravelled);
        }
    }
    
    private void ScrollToPlayerPosition()
    {
        if (scrollRect == null)
            return;

        if (mapContent == null)
            return;

        if (RunMapManager.Instance == null)
            return;

        int currentNodeId = RunMapManager.Instance.CurrentNodeId;
        
        if (currentNodeId < 0)
        {
            scrollRect.verticalNormalizedPosition = 0f;
            return;
        }

        if (!nodeViews.TryGetValue(currentNodeId, out MapNodeView currentNodeView))
        {
            scrollRect.verticalNormalizedPosition = 0f;
            return;
        }

        RectTransform viewport = scrollRect.viewport;

        if (viewport == null)
            viewport = scrollRect.GetComponent<RectTransform>();

        float contentHeight = mapContent.rect.height;
        float viewportHeight = viewport.rect.height;

        float scrollableHeight = contentHeight - viewportHeight;

        if (scrollableHeight <= 0f)
            return;

        float targetY = currentNodeView.RectTransform.anchoredPosition.y;

        float normalizedPosition = Mathf.Clamp01((targetY - viewportHeight * 0.5f) / scrollableHeight);
        
        scrollRect.verticalNormalizedPosition = normalizedPosition;
    }

    private void ClearMap()
    {
        nodeViews.Clear();
        connectionViews.Clear();

        DestroyChildren(nodesContainer);
        DestroyChildren(linesContainer);
    }

    private void DestroyChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }
}
