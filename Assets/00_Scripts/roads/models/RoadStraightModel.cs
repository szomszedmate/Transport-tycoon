using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadStraightModel : RoadModel
{
    public override Direction[] Inputs { get; set; } = { Direction.W, Direction.E }; // vehicles going E and W can enter
    public override Direction[] Outputs { get; set; } = { Direction.W, Direction.E }; // vehicles going E and W can leave
    
    private readonly RoadType roadType = RoadType.STRAIGHT;
    public override RoadType RoadType { get { return roadType; } }

}
