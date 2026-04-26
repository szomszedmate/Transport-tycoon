using UnityEngine;

public interface IBuildable
{
    BuildCategory BuildCategory { get; }

}

public enum BuildCategory
{
    ROAD,
    BUS,
    BUSSTOP,
    TRUCK,
    UNKNOWN
}