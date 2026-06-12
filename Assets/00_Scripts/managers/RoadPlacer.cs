using System;
using System.Collections.Generic;
using UnityEngine;

public class RoadPlacer : MonoBehaviour
{
    [Header("Roads")]
    [SerializeField] private RoadData straightRoad;
    [SerializeField] private RoadData lRoad;
    [SerializeField] private RoadData tRoad;
    [SerializeField] private RoadData crossRoad;
    [SerializeField] private JokerRoadData jokerRoad;

    [Header("Bridges")]
    [SerializeField] private RoadData bridgeRoad1;
    [SerializeField] private RoadData bridgeRoad2;
    [SerializeField] private RoadData bridgeRoad3;

    [Header("Prefabs")]
    [SerializeField] private Road roadPrefab;

    public JokerRoadData ActiveJoker { get; set; } = null;
    public RoadType LastJokerType { get; set; } = RoadType.UNKNOWN;
    public Dictionary<Vector3, JokerRoadData> JokerPositions { get; private set; } = new();

    public Road RoadPrefab { get => roadPrefab; set => roadPrefab = value; }

    // Hover state for destroy mode
    private Road hovered;
    private Road lastHovered;

    // Shared references retrieved from BuildingSystem in Awake
    private BuildingSystem bs;
    private BuildingGrid grid;
    private TerrainPainter painter;
    private int builtLayerIndex;
    private int positiveIndex;
    private int negativeIndex;
    private int destroyHoverIndex;
    private int selectedIndex;
    private int confirmedIndex;
    private List<int> mainLayers;

    private void Awake()
    {
        if (bs == null) bs = GetComponentInParent<BuildingSystem>();
    }

    // Called by BuildingSystem after all components are ready
    public void Init(BuildingGrid grid, TerrainPainter painter,
        int builtLayerIndex, int positiveIndex, int negativeIndex,
        int destroyHoverIndex, int selectedIndex, int confirmedIndex,
        List<int> mainLayers)
    {
        if (bs == null) bs = GetComponentInParent<BuildingSystem>();
        this.grid = grid;
        this.painter = painter;
        this.builtLayerIndex = builtLayerIndex;
        this.positiveIndex = positiveIndex;
        this.negativeIndex = negativeIndex;
        this.destroyHoverIndex = destroyHoverIndex;
        this.selectedIndex = selectedIndex;
        this.confirmedIndex = confirmedIndex;
        this.mainLayers = mainLayers;
    }

    public void HandleDestroyMode(bool shouldIDestroy, Vector2 mousePosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, bs.TerrainLayer))
        {
            Vector3 snappedPos = bs.GetSnappedCenterPosition(hit.point);
            hovered = grid.GetRoad(snappedPos);

            if (hovered is not null && hovered != lastHovered)
            {
                if (lastHovered != null)
                    lastHovered.ChangeState(RoadState.BUILT);

                if (hovered != null)
                    hovered.ChangeState(RoadState.DESTROYHOVER);

                lastHovered = hovered;
            }
            else if (hovered != lastHovered)
            {
                if (lastHovered != null)
                {
                    lastHovered.ChangeState(RoadState.BUILT);
                    lastHovered = null;
                }
            }
            if (shouldIDestroy && hovered is not null) DestroyHovered();
        }
    }

    public void ResetLastHovered()
    {
        if (lastHovered != null)
        {
            lastHovered.ChangeState(RoadState.BUILT);
            lastHovered = null;
        }
    }

    public void DestroyHovered()
    {
        if (bs.Preview != null) return;
        if (hovered is null) return;
        if (grid.IsCityRoad(hovered.transform.position)) return;

        Vector3 pos = hovered.transform.position;

        painter.ResetLayer(pos, mainLayers, hovered.Type, hovered.Model.Rotation);

        JokerPositions.Remove(pos);
        grid.RemRoad(hovered.transform.position);
        Destroy(hovered.gameObject);
        lastHovered = null;
        UpdateNeighborJokers(pos);
    }

    public void PlaceRoad(Vector3 roadPosition)
    {
        if (bs.Preview is not RoadPreview) return;
        Vector3 snappedPos = bs.GetSnappedCenterPosition(roadPosition);

        if (grid.HasTrees(snappedPos))
        {
            grid.ClearTrees(snappedPos);
        }
        Road road = Instantiate(RoadPrefab, snappedPos, Quaternion.identity);
        road.RoadStateChangedEventHandler += Road_RoadStateChangedEventHandler;

        road.Setup(((RoadPreview)bs.Preview).Data, ((RoadPreview)bs.Preview).RoadModel.Rotation);
        if (road.data.Kind == RoadKind.Bridge)
        {
            float bridgeY = grid.CalculateBridgeHeight(snappedPos, road.Model.Rotation, road.data);
            Vector3 finalPos = snappedPos;
            finalPos.y = bridgeY;
            road.Model.bridgeEffect.transform.position = finalPos;
        }
        grid.SetRoad(road, snappedPos);

        if (ActiveJoker != null)
            JokerPositions[snappedPos] = ActiveJoker;

        UpdateNeighborJokers(snappedPos);
    }

    private void UpdateNeighborJokers(Vector3 pos)
    {
        Vector3[] offsets = {
            new Vector3(0, 0,  BuildingSystem.CellSize),
            new Vector3(0, 0, -BuildingSystem.CellSize),
            new Vector3( BuildingSystem.CellSize, 0, 0),
            new Vector3(-BuildingSystem.CellSize, 0, 0),
        };

        foreach (var offset in offsets)
        {
            Vector3 neighborPos = pos + offset;
            if (JokerPositions.TryGetValue(neighborPos, out JokerRoadData jokerData))
            {
                ReplaceJokerRoad(neighborPos, jokerData);
            }
        }
    }

    private void ReplaceJokerRoad(Vector3 worldPos, JokerRoadData jokerData)
    {
        Road existing = grid.GetRoad(worldPos);
        if (existing == null) return;

        var (resolvedData, resolvedRotation) = jokerData.Resolve(grid, worldPos);

        painter.ResetLayer(worldPos, mainLayers, existing.Type, existing.Model.Rotation);
        grid.RemRoad(worldPos);
        Destroy(existing.gameObject);

        Road newRoad = Instantiate(RoadPrefab, worldPos, Quaternion.identity);
        newRoad.RoadStateChangedEventHandler += Road_RoadStateChangedEventHandler;
        newRoad.Setup(resolvedData, resolvedRotation);
        grid.SetRoad(newRoad, worldPos);
        JokerPositions[worldPos] = jokerData;

        painter.PaintRoadAt(worldPos, newRoad.Type, newRoad.Model.Rotation, builtLayerIndex, mainLayers, true, 0);
    }

    public void Road_RoadStateChangedEventHandler(object sender, RoadStateChangedEventArgs e)
    {
        int lIndex;
        int prevIndex = 0;
        switch (e.NewState)
        {
            case RoadState.BUILT:
                lIndex = builtLayerIndex;
                break;
            case RoadState.DESTROYHOVER:
                lIndex = destroyHoverIndex;
                break;
            case RoadState.SELECTED:
                lIndex = selectedIndex;
                break;
            case RoadState.CONFIRMED:
                lIndex = confirmedIndex;
                break;
            default:
                return;
        }
        switch (e.PrevState)
        {
            case RoadState.BUILT:
                if (e.NewState == RoadState.BUILT)
                {
                    prevIndex = positiveIndex;
                    break;
                }
                prevIndex = builtLayerIndex;
                break;
            case RoadState.DESTROYHOVER:
                prevIndex = destroyHoverIndex;
                break;
            case RoadState.SELECTED:
                prevIndex = selectedIndex;
                break;
            case RoadState.CONFIRMED:
                prevIndex = confirmedIndex;
                break;
            default:
                return;
        }
        painter.PaintRoadAt(bs.GetSnappedCenterPosition(e.SnappedPosition), e.RoadType, e.Rotation, lIndex, mainLayers, false, prevIndex);
    }

    public void PaintRegisteredRoads(List<Road> roads)
    {
        foreach (Road road in roads)
        {
            painter.PaintRoadAt(road.transform.position, road.Type, road.Model.Rotation, builtLayerIndex, mainLayers, true, 0);
        }
    }

    public void RegisterCityRoadsAsJokers(List<Road> roads)
    {
        foreach (Road road in roads)
        {
            if (!road.IsCityRoad) continue;
            JokerPositions[road.transform.position] = jokerRoad;
        }
        foreach (Road road in roads)
        {
            if (!road.IsCityRoad) continue;
            UpdateNeighborJokers(road.transform.position);
        }
    }

    private void RemoveRoad(Vector3 mouseWorldPosition)
    {
        Vector3 snappedPos = bs.GetSnappedCenterPosition(mouseWorldPosition);
        grid.RemRoad(snappedPos);
    }
}
