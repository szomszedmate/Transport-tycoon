using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadLModel : RoadModel
{
    public override Direction[] Inputs { get; set; } = { Direction.N, Direction.E }; // vehicles going N and W can enter
    public override Direction[] Outputs { get; set; } = { Direction.S, Direction.W }; // vehicles going S and E can leave
}
