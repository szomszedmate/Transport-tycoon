using UnityEngine;

public class CreditsScroll : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 30f;
    private Vector3 basePos;

    private void Awake()
    {
        basePos = transform.position;
        Time.timeScale = 1f;
    }

    void Update()
    {
        transform.Translate(Vector3.up * scrollSpeed * Time.deltaTime);
    }

    public void Reset()
    {
        transform.position = basePos;
    }
}
