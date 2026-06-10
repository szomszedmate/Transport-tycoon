using UnityEngine;

[CreateAssetMenu(menuName = "Data/Cat")]
public class CatData : ScriptableObject, IData
{
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public GameObject Prefab { get; private set; }
    [field: SerializeField] public AudioClip[] Sounds { get; private set; }
    [field: SerializeField] public int Cost { get; private set; }
}
