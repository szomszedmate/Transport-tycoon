using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadStraight : RoadModel
{
    private Direction[] inputs = { Direction.W, Direction.S }; // vehicles going N and W can enter
    public override Direction[] Inputs => inputs;
    private Direction[] outputs = { Direction.W, Direction.S }; // vehicles going N and W can leave
    public override Direction[] Outputs => outputs;
}
