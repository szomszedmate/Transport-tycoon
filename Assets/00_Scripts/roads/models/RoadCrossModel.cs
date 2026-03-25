using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadCrossModel : RoadModel
{
    public override Direction[] Inputs { get; set; } = { Direction.W, Direction.S, Direction.E, Direction.N }; // vehicles going N, W, E, S can enter
    public override Direction[] Outputs { get; set; } = { Direction.W, Direction.S, Direction.E, Direction.N }; // vehicles going N, W, E, S can leave
}
