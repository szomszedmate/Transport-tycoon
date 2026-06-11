using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Roads/Joker Road")]
public class JokerRoadData : ScriptableObject, IData
{
    [field: SerializeField] public int Cost { get; private set; }
    [field: SerializeField] public string Description { get; private set; }

    [SerializeField] public RoadData StraightRoad;
    [SerializeField] public RoadData TurnRoad;
    [SerializeField] public RoadData TRoad;
    [SerializeField] public RoadData CrossRoad;

    // Returns which PHYSICAL SIDES of the joker tile need to be open,
    // based on neighboring roads that can send vehicles toward this tile.
    private List<Direction> GetNeededConnections(BuildingGrid grid, Vector3 worldPos)
    {
        var connections = new List<Direction>();
        float cell = BuildingSystem.CellSize;

        // (neighbor offset, direction vehicle travels toward joker, physical side of joker that opens)
        // North neighbor sends vehicles south (Output=S) → joker opens N side
        // South neighbor sends vehicles north (Output=N) → joker opens S side
        // East  neighbor sends vehicles west  (Output=W) → joker opens E side
        // West  neighbor sends vehicles east  (Output=E) → joker opens W side

        if (grid.GetRoad(worldPos + new Vector3(0, 0,  cell)) != null) connections.Add(Direction.N);
        if (grid.GetRoad(worldPos + new Vector3(0, 0, -cell)) != null) connections.Add(Direction.S);
        if (grid.GetRoad(worldPos + new Vector3( cell, 0, 0)) != null) connections.Add(Direction.E);
        if (grid.GetRoad(worldPos + new Vector3(-cell, 0, 0)) != null) connections.Add(Direction.W);

        return connections;
    }

    // Physical opening of a road model after r CW rotations.
    // Opening = opposite(Input direction) because:
    //   Input=N means vehicle travels northward → enters from SOUTH side → SOUTH opening.
    // After r CW rotations, direction d maps to (d + r) % 4 in the N=0,E=1,S=2,W=3 CW enum.
    // But Unity enum is N=0,W=1,S=2,E=3 so one CW step: N→E→S→W→N
    // One CW step in enum: (d + 3) % 4 for N,W,S,E ordering.
    private HashSet<Direction> GetOpenings(RoadData data, int rotations)
    {
        var openings = new HashSet<Direction>();
        foreach (Direction input in data.Model.Inputs)
        {
            // rotate direction r times CW: each step applies (d+3)%4
            int d = (int)input;
            for (int i = 0; i < rotations; i++) d = (d + 3) % 4;
            // opening = opposite of rotated input
            Direction rotated = (Direction)d;
            Direction opening = (Direction)(((int)rotated + 2) % 4);
            openings.Add(opening);
        }
        return openings;
    }

    public (RoadData data, int rotation) Resolve(BuildingGrid grid, Vector3 worldPos)
    {
        var connections = GetNeededConnections(grid, worldPos);
        var needed = new HashSet<Direction>(connections);

        bool hasN = needed.Contains(Direction.N);
        bool hasS = needed.Contains(Direction.S);
        bool hasE = needed.Contains(Direction.E);
        bool hasW = needed.Contains(Direction.W);

        // Pick road type
        RoadData data;
        if (connections.Count >= 4)
            return (CrossRoad, 0);
        else if (connections.Count == 3)
            data = TRoad;
        else if (connections.Count == 2 && ((hasN && hasS) || (hasE && hasW)))
            data = StraightRoad;
        else if (connections.Count == 2)
            data = TurnRoad;
        else
            data = StraightRoad;

        // Find rotation: try all 4 and pick the one whose openings contain all needed sides
        for (int r = 0; r < 4; r++)
        {
            HashSet<Direction> openings = GetOpenings(data, r);
            if (needed.IsSubsetOf(openings))
                return (data, r * 90);
        }

        return (data, 0);
    }
}
