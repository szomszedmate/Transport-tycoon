using UnityEngine;
using UnityEngine.AI;
public class BusAiAgent : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public NavMeshAgent ai;
    public Transform targetpos;
    
    void Start()
    {
        ai = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (targetpos!=null)
        {
            ai.SetDestination(targetpos.position);
            
        }
    }
}
