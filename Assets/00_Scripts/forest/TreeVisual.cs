using UnityEngine;

public class TreeVisual : MonoBehaviour
{
    [SerializeField] private GameObject tree1;
    [SerializeField] private GameObject tree2;
    [SerializeField] private GameObject tree3;
    [SerializeField] private GameObject tree4;
    void Start()
    {
        
    }

    
    void Update()
    {
        
    }
    public void SetTreeCount(int treeCount)
    {
        tree1.SetActive(treeCount >= 1);
        tree2.SetActive(treeCount >= 2);
        tree3.SetActive(treeCount >= 3);
        tree4.SetActive(treeCount >= 4);
    }
}
