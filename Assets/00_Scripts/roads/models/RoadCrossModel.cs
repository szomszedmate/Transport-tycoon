using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadCrossModel : RoadModel
{
    private Direction[] inputs = { Direction.W, Direction.S, Direction.E, Direction.N }; // vehicles going N, W, E, S can enter
    public override Direction[] Inputs => inputs;
    private Direction[] outputs = { Direction.W, Direction.S, Direction.E, Direction.N }; // vehicles going N, W, E, S can leave
    public override Direction[] Outputs => outputs;
}
