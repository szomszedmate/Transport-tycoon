using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Water : MonoBehaviour
{
    public List<GameObject> waters;
    private void Awake()
    {
        waters = GameObject.FindGameObjectsWithTag("Water").ToList();
    }
}
