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
    public bool ontrack = false;
    [SerializeField]
    private bool righlane = false;
    [SerializeField]
    private bool laneslected = false;
    [SerializeField]
    private bool movingtostart = false;
    private Road startpoz;
    void Start()
    {
        ai = GetComponent<NavMeshAgent>();
        ai.enabled = true;
    }

    public void RemoveRoute()
    {
        transform.position=startpoz.transform.position;
        ai.SetDestination(startpoz.transform.position);
        Route.Clear();
        oda = true;
        righlane = false;
        laneslected = false;
        movingtostart = false;
        isMoving = false;
        maxprogress = 0;
         currprog = 0;
        ontrack = false;
}

    public void GiveRoute(List<Road> route)
    {

        Route = route;
        startpoz = route[0];
        maxprogress = Route.Count - 1;
        MoveToStart();
    }
    private bool IsBetween(float value, float min, float max)
    {
        return value >= min && value <= max;
    }


    public void RotateToNext()
    {
        Vector3 dir;



        //TODO buszt ne lehessen forgatva letenni
        //TODO buszt csak bustopra lehessen letenni
        Debug.Log(currprog);
            dir = Route[currprog].transform.position - transform.position;
            dir.y = 0f;

        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                120f * Time.deltaTime
            );
        }

          
    }

    public Transform SelectLane()
    {
        /*
         
        right a menetirany szeriont jobb oldal
        a to...lane hogy az adott pont az ut kozepetol melyik iranyba van
       vector.dot eldonti h melyik pont van legjobbrabb
         
         */


        RotateToNext();
        Vector3 forward=new Vector3();
        if (!movingtostart)
        {
            forward = (Route[currprog].transform.position - Route[currprog - 1].transform.position).normalized;
        }
            
        
       

        Debug.Log("haladasi irany: "  + forward);
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        /*
        https://docs.unity3d.com/6000.4/Documentation/ScriptReference/Vector3.Cross.html
        alculates the cross product of two three-dimensional vectors.

        The cross product of two three-dimensional vectors results in a third vector which is perpendicular to the two input vectors. 
        The result's magnitude is equal to the magnitudes of the two inputs multiplied together and then multiplied by the sine of the angle between the inputs. 
        You can determine the direction of the result vector from the two input vectors using the "left hand rule".
         
         */



        Vector3 toLeftLane = new Vector3();
            Vector3 toRightLane = new Vector3();
        Vector3 toToptLane = new Vector3();
        Vector3 toBottomLane = new Vector3();
        //currprog itt a kovetkezo tile mindig. nem az amin éppen van hanem amire menni akar majd.
        if (Route[currprog].transform.GetChild(0).tag == "straight_road" || Route[currprog].transform.GetChild(0).tag == "turn_road" || Route[currprog].transform.GetChild(0).tag == "T_road"  )
        {
             toLeftLane = Route[currprog].leftLane.position - Route[currprog].transform.position;

            toRightLane = Route[currprog].rightLane.position - Route[currprog].transform.position;
        }
        else if (Route[currprog].transform.GetChild(0).tag == "cross_road")
        {
           
            toLeftLane = Route[currprog].leftLane.position - Route[currprog].transform.position;

            toRightLane = Route[currprog].rightLane.position - Route[currprog].transform.position;

            toToptLane = Route[currprog].topLane.position - Route[currprog].transform.position;

            toBottomLane = Route[currprog].bottomLane.position - Route[currprog].transform.position;

            float bal = Vector3.Dot(right, toLeftLane); 
            float jobb = Vector3.Dot(right, toRightLane);    
            float felso = Vector3.Dot(right, toToptLane);    
            float also = Vector3.Dot(right, toBottomLane);    

            float max = Mathf.Max(bal, jobb, felso, also);

            

            if (max == bal)
            {
                laneslected = true;
                
                return Route[currprog].leftLane;
            }
            else if (max == jobb)
            {
                laneslected = true;
                
                return Route[currprog].rightLane;
            }
            else if (max == also)
            {
                laneslected = true;
               
                return Route[currprog].bottomLane;
            }
            else
            {
                laneslected = true;
               
                return Route[currprog].topLane;
            }

         
        }
        else // csak city roadnal
        {
             toLeftLane = Route[currprog].transform.position - Route[currprog].transform.position;

             toRightLane = Route[currprog].transform.position - Route[currprog].transform.position;
        }
            

        /*
         https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Vector3.Dot.html
        Calculates the dot product of two three-dimensional vectors defined in the same coordinate space.

        The dot product is a float value equal to the product of the magnitudes of the lhs and rhs vectors and the cosine of the angle between them.
         */


        /*
        right valtozoban levan tarolva h a menetirannyal meröleges jobbirányu vektor,
        a dot megmondja melyik lane mutat inkabb a right vektor iranyaba

         */



        /*
          huzunk egy egyenes vonalat a haladas iranyaba majd ebböl szamolunk egy corsst
        ami megmutatja merre van jobb irany ezután emgnezzuk a ket lane kooridnata közül hogymelyik mutat a jobbra mutato vektor iranyahoz
         */
        float leftDot = Vector3.Dot(toLeftLane, right);
        float rightDot = Vector3.Dot(toRightLane, right);
        if (rightDot > leftDot)
        {
            laneslected = true;
            return Route[currprog].rightLane;


        }
        else
        {
            laneslected = true;
            return Route[currprog].leftLane;
        }
        
    }
    public void MoveToStart()
    {
        movingtostart = true;
        //SelectLane();
        ai.SetDestination(Route[0].transform.position);


    }

    // Update is called once per frame
    void Update()
    {
        if (ontrack)
        {
            //if (!laneslected)
            //{
           //    SelectLane();
            //}
            
            //if (laneslected)
            //{
                //if (oda)
                //{
                    if (!isMoving && currprog <= maxprogress)
                    {
                        isMoving = true;
                        if (Route[currprog].transform.GetChild(0).tag=="straight_road" || Route[currprog].transform.GetChild(0).tag == "turn_road" || Route[currprog].transform.GetChild(0).tag == "T_road" || Route[currprog].transform.GetChild(0).tag == "cross_road")
                        {
                           // if (righlane)
                           // {
                                ai.SetDestination(SelectLane().position);
                           // }
                           // else
                            //{
                               // ai.SetDestination(Route[currprog].leftLane.position);
                            //}

                        }
                        else // city roads nal jut csak ide
                        {
                            ai.SetDestination(Route[currprog].transform.position);
                        }

                    currprog++;
                    if (currprog > maxprogress)
                    {
                        currprog = 0;
                        Route.Reverse();
                        //currprog = maxprogress;
                        //oda = false;
                    }
                    
                    SelectLane();
                    
                }
                //}
                /*else
                {
                    if (!isMoving && currprog >= 0)
                    {
                        isMoving = true;
                        if (Route[currprog].transform.GetChild(0).tag == "straight_road" || Route[currprog].transform.GetChild(0).tag == "turn_road")
                        {
                            if (righlane)
                            {
                                ai.SetDestination(Route[currprog].leftLane.position);

                            }
                            else
                            {
                                ai.SetDestination(Route[currprog].rightLane.position);
                            }
                        }
                        else
                        {
                            ai.SetDestination(Route[currprog].transform.position);
                        }

                        currprog--;
                        SelectLane();
                    }
                }*/
            //}
            
            float distance = Vector3.Distance(transform.position, ai.destination);
            if (distance < 0.05f)
            {
                isMoving = false;
                
            }
        }else if (movingtostart)
        {
            float distance = Vector3.Distance(transform.position, ai.destination);
            Debug.Log(distance);
            if (distance < 0.05f)
            {
                if (Route.Count>0)
                {
                    ontrack = true;
                }
                
                movingtostart = false;
                currprog++;
            }

        }
    }
}
