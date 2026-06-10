using UnityEngine;

public class Cat : MonoBehaviour, IPreview
{
    private CatData catData;
    public GameObject visual;
    private int currIndex;
    private PreviewState state;

    // IPreview
    public IData Data => catData;
    public PreviewState State => state;
    public void ChangeState(PreviewState newState) { state = newState; }
    public void Rotate(int degrees) { RotateCat(); }

    public void Setup(CatData data)
    {
        catData = data;
        currIndex = 0;
        gameObject.name = data.Name;
    }

    public void RotateCat()
    {
        transform.Rotate(0, 90, 0);
        AudioSource.PlayClipAtPoint(catData.Sounds[currIndex], visual.transform.position, 3f);
        currIndex = (currIndex + 1) % catData.Sounds.Length;
    }
}
