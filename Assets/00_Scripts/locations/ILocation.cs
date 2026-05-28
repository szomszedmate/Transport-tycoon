using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public interface ILocation
{
    //event System.EventHandler<GetTimeEventArgs> GetTime;
    Vector3 Position { get;}
    StopType Type { get; }
    List<Vector3> GetAllBuildingPositions();
    void AdjustVisualToGround(Vector3 surfacePoint);
    GameObject Visual {  get; }
}
