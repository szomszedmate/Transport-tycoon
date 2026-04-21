using UnityEngine;

public class Shift : MonoBehaviour
{
    private int workers;
    private float startTime;
    private float endTime;
    private bool working;

    public float StartTime { get => startTime; set => startTime = value; }
    public float EndTime { get => endTime; set => endTime = value; }

    public void StartShift()
    {
        working = true;
    }

    public void StopShift()
    {
        working = false;
    }
}
