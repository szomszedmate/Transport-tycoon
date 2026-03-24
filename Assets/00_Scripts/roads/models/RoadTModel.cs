using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadTModel : RoadModel
{
    private Direction[] inputs = { Direction.W, Direction.N, Direction.E }; // vehicles going N, W, E can enter
    public override Direction[] Inputs => inputs;
    private Direction[] outputs = { Direction.W, Direction.S, Direction.E }; // vehicles going S, W, E can leave
    public override Direction[] Outputs => outputs;
}
