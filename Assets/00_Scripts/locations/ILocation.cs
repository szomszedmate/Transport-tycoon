using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public interface ILocation
{
    Vector3 Position { get;}
    StopType Type { get; }
    List<Vector3> GetAllBuildingPositions();
}
