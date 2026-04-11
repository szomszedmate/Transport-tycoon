using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
public class BusAiAgent : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private NavMeshAgent ai;
    public Transform targetpos;
    public List<Road> Route=null;
    public bool oda = true;
    public bool isMoving = false;
    public int maxprogress = 0;
    public int currprog = 0;
    void Start()
    {
        ai = GetComponent<NavMeshAgent>();
        ai.enabled = true;
    }

    public void GiveRoute(List<Road> route)
    {

        Route = route;
        maxprogress = Route.Count - 1;
    }
    // Update is called once per frame
    void Update()
    {
        if (Route.Count>0)
        {

            if (oda)
            {
                if (!isMoving&&currprog<=maxprogress)
                {
                    isMoving = true;
                    if (Route[currprog].data!=null)
                    {
                        ai.SetDestination(Route[currprog].rightLane.position);
                    }
                    else
                    {
                        ai.SetDestination(Route[currprog].transform.position);
                    }
                   
                    currprog++;
                }




            }
            else
            {
                if (!isMoving && currprog >= 0)
                {
                    isMoving = true;
                    if (Route[currprog].data!=null)
                    {
                        ai.SetDestination(Route[currprog].leftLane.position);
                    }
                    else
                    {
                        ai.SetDestination(Route[currprog].transform.position);
                    }
                       
                    currprog--;
                }
            }
            float distance = Vector3.Distance(transform.position, ai.destination);
            if (distance < 0.05f)
            {
                isMoving = false;
                if (currprog > maxprogress)
                {
                    currprog = maxprogress;
                    oda = false;
                }else if (currprog < 0)
                {
                    currprog = 0;
                    oda = true;
                }
            }
        }
      /*  if (targetpos!=null)
        {
            ai.SetDestination(targetpos.position);
            
        }*/
    }
}
