using UnityEngine;

public interface IPreview
{
    IData Data { get; }
    PreviewState State { get; }

    void ChangeState(PreviewState state);
    void Rotate(int degrees);
}
