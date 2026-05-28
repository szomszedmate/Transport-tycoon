using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadTModel : RoadModel
{
    public override Direction[] Inputs { get; set; } = { Direction.W, Direction.N, Direction.E }; // vehicles going N, W, E can enter
    public override Direction[] Outputs { get; set; } = { Direction.W, Direction.S, Direction.E }; // vehicles going S, W, E can leave

    private readonly RoadType roadType = RoadType.T;
    public override RoadType RoadType { get { return roadType; } }
}
