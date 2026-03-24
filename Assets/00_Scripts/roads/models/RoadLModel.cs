using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadLModel : RoadModel
{
    private Direction[] inputs = { Direction.W, Direction.N }; // vehicles going N and E can enter
    public override Direction[] Inputs => inputs;
    private Direction[] outputs = { Direction.E, Direction.S }; // vehicles going S and W can leave
    public override Direction[] Outputs => outputs;
}
