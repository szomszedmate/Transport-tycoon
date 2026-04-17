using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

using System;
using System.Collections.Generic;
using System.Linq;



public class BusAiAgent : MonoBehaviour
{
  
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
   
    private bool lastwasrighlane = false;
    [SerializeField]
    private bool laneslected = false;
    [SerializeField]
    private bool movingtostart = false;
    [SerializeField]
    private Road startpoz;
    public StopType type = StopType.None;

    private bool firstdone = false;

    void Start()
    {
        ai = GetComponent<NavMeshAgent>();
        ai.enabled = true;
    }

    public void RemoveRoute()
    {
        ai.ResetPath();
        Route.Clear();
        
        
        
        oda = true;
        righlane = false;
        laneslected = false;
        movingtostart = false;
        isMoving = false;
        maxprogress = 0;
         currprog = 0;
        ontrack = false;
        firstdone = false;
        ai.isStopped = true;
        transform.position = startpoz.transform.position;
        
        ai.Warp(startpoz.transform.position);
        transform.rotation = Quaternion.identity;
    }

    public void GiveRoute(List<Road> route)
    {

        Route = route;
        startpoz = route[0];
        maxprogress = Route.Count - 1;
        // SelectLane();
        // SelectLane();
       // Route.Reverse();
        currprog = 0;
        //Debug.Log("righlane: "+righlane);
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
    //TODO celbaerest lekezelni
    public GameObject SelectLane()
    {

        Debug.Log("SelectLaneStarted");
        /*
         
        right a menetirany szeriont jobb oldal
        a to...lane hogy az adott pont az ut kozepetol melyik iranyba van
       vector.dot eldonti h melyik pont van legjobbrabb
         
         */


        //RotateToNext();
        Vector3 forward=new Vector3();

        int progress=0;

        if (!movingtostart)
        {
            if (last == null)
            {
                progress = currprog;
                forward = (Route[currprog+1 ].transform.position - Route[currprog].transform.position).normalized;
            }
            else if (currprog==0)
            {
                //Debug.Log("currkisebbmintmax");
                progress = currprog + 1;
                forward = (Route[currprog + 1].transform.position - Route[currprog].transform.position).normalized;
            }
            else
            {
                progress = currprog;
                forward = (Route[currprog ].transform.position - Route[currprog-1].transform.position).normalized;
            }
            
        }
            
        
       

       // Debug.Log("haladasi irany: "  + forward);
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
        if (Route[progress].transform.GetChild(0).tag == "straight_road" || Route[progress].transform.GetChild(0).tag == "turn_road"  )
        {
            //Debug.Log("nemkereszt on " + currprog);
             toLeftLane = Route[progress].leftLane.position - Route[progress].transform.position;

            toRightLane = Route[progress].rightLane.position - Route[progress].transform.position;
        }
        else 
        {
           
            toLeftLane = Route[progress].leftLane.position - Route[progress].transform.position;

            toRightLane = Route[progress].rightLane.position - Route[progress].transform.position;

            toToptLane = Route[progress].topLane.position - Route[progress].transform.position;

            toBottomLane = Route[progress].bottomLane.position - Route[progress].transform.position;

            float bal = Vector3.Dot(right, toLeftLane); 
            float jobb = Vector3.Dot(right, toRightLane);    
            float felso = Vector3.Dot(right, toToptLane);    
            float also = Vector3.Dot(right, toBottomLane);    

            float max = Mathf.Max(bal, jobb, felso, also);

            

            if (max == bal)
            {
                laneslected = true;
            /*    if (righlane)
            {
                lastwasrighlane = true;
            }
            else
            {
                lastwasrighlane = false;
            }*/
            //Debug.Log("Rightlane ste to falsee");
           /* Debug.Log("rightlane set to FALSE");
            righlane = false;*/
                return Route[progress].leftLane.gameObject;
            }
            else if (max == jobb)
            {
                /*laneslected = true;
                if (righlane)
                {
                    lastwasrighlane = true;
                }
                else
                {
                    lastwasrighlane = false;
                }
                // Debug.Log("Rightlane ste to truee");
                Debug.Log("rightlane set to TRUE");
                righlane = true;*/
                return Route[progress].rightLane.gameObject;
            }
            else if (max == also)
            {
                laneslected = true;
               
                return Route[progress].bottomLane.gameObject;
            }
            else
            {
                laneslected = true;
               
                return Route[progress].topLane.gameObject;
            }

         
        }
        /*else // csak city roadnal
        {
             toLeftLane = Route[currprog].transform.position - Route[currprog].transform.position;

             toRightLane = Route[currprog].transform.position - Route[currprog].transform.position;
        }*/
            

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
            
                //ai.isStopped = false;
             /*   laneslected = true;
            if (righlane)
            {
                lastwasrighlane = true;
            }
            else
            {
                lastwasrighlane = false;
            }
            // Debug.Log("Rightlane ste to truee");
            Debug.Log("rightlane set to TRUE");*/
            righlane = true;
           
                return Route[progress].rightLane.gameObject;
            
           


        }
        else
        {
            
                //ai.isStopped = false;
             /*   laneslected = true;
            if (righlane)
            {
                lastwasrighlane = true;
            }
            else
            {
                lastwasrighlane = false;
            }
            //Debug.Log("Rightlane ste to falsee");
            Debug.Log("rightlane set to FALSE");*/
            righlane = false;
            
            return Route[progress].leftLane.gameObject;
            
           
        }
        //ai.isStopped = true;
        //return null;
    }
    public void MoveToStart()
    {
       // movingtostart = true;
        transform.position = startpoz.transform.position;
        ontrack = true;
        MoveToNewDest();

        //SelectLane();


    }

    // Update is called once per frame
    public void stopbusz()
    {
        //ai.isStopped = true;
    }

    public void startbusz()
    {
        //Debug.Log("started;");
        ai.isStopped = false;
    }

    /*public void setFree()
    {



        if (righlane && Route[currprog + 1].rightlanefree || !righlane && Route[currprog + 1].leftlanefree)

        {
            Debug.Log("SetFree started;");
          //  Debug.Log("lance checked for: " + (currprog + 1));


            //if nem keresztezodes

            if (Route[currprog + 1].transform.GetChild(0).tag == "straight_road" || Route[currprog + 1].transform.GetChild(0).tag == "turn_road")

            {

                if (righlane)

                {
                    Debug.Log("In setfree rightlane was true");
                    Route[currprog + 1].rightlanefree = false;

                }

                else

                {
                    //Debug.Log("leftlanefalse");
                   /* if (currprog == 0 && firstdone == false)
                    {
                        Route[currprog + 1].rightlanefree = false;
                        firstdone = true;
                       
                    //}*/
                    //else
                    //{
                       /* Debug.Log("In setfree rightlane was false");
                        Route[currprog + 1].leftlanefree = false;*/
                    //}
              

               /* }
            }
            else
            {
                //Debug.Log("buszaddedtokeresztezodes");
                Route[currprog + 1].AddBusz(this);
            }



            //ha nem keresztezides
            if (Route[currprog].transform.GetChild(0).tag == "straight_road" || Route[currprog].transform.GetChild(0).tag == "turn_road")

            {
                
                if (lastwasrighlane)
                {
                    Route[currprog].rightlanefree = true;

                }
                else
                {
                    Route[currprog].leftlanefree = true;
                }
            }
            else
            {
                Route[currprog].RemBusz();

            }
        
        }
    }*/
    /*public bool CheckIfFree()
    {
        Debug.Log("checking if " + (currprog+1) +"is free");

        if (Route[currprog+1].transform.GetChild(0).tag == "straight_road" || Route[currprog+1].transform.GetChild(0).tag == "turn_road")
        {
            if (righlane && Route[currprog + 1].rightlanefree || !righlane && Route[currprog + 1].leftlanefree)
            {
                // Debug.Log("lance checked for: " + (currprog + 1));

               // Debug.Log("if free checked");
                return true;
            }
            else
            {
                return false;
            }
        }
        if (Route[currprog + 1].buszok.Count==0|| Route[currprog + 1].buszok[Route[currprog + 1].buszok.Count-1]==this)
        {
            return true;
        }
        else
        {
            
            return false;
        }

        

        
        
    }*/

    public bool isFree() {
        if (next.GetComponentInParent<Road>().data.Description == "Straight Road" || next.GetComponentInParent<Road>().data.Description == "Right Turn")
        {
            if (righlane)
            {
                return next.GetComponentInParent<Road>().rightlanefree;
            }
            else
            {
                return next.GetComponentInParent<Road>().leftlanefree;
            }
        }
        else
        {
            return (next.GetComponentInParent<Road>().buszok.Count == 0 || next.GetComponentInParent<Road>().buszok[next.GetComponentInParent<Road>().buszok.Count - 1] == this);
        }
    }

    public void SetNotFree()
    {
        if (next.GetComponentInParent<Road>().data.Description == "Straight Road" || next.GetComponentInParent<Road>().data.Description == "Right Turn")
        {
           
        
            if (righlane)
         {
             next.GetComponentInParent<Road>().rightlanefree = false;
                lastLaneWasRight = true;
            }
            else
            {
                next.GetComponentInParent<Road>().leftlanefree = false;
                lastLaneWasRight = false;
         }
        }
        else
        {
            next.GetComponentInParent<Road>().AddBusz(this);
        }
    }

    public void SetFree()
    {
        if (last.GetComponentInParent<Road>().data.Description == "Straight Road" || last.GetComponentInParent<Road>().data.Description == "Right Turn")
        {
            if (last.gameObject.tag=="right")
            {
                last.GetComponentInParent<Road>().rightlanefree = true;
            }
            else if (last.gameObject.tag=="left")
            {
                last.GetComponentInParent<Road>().leftlanefree = true;
            }
            
        }
        else
        {
            last.GetComponentInParent<Road>().RemBusz();
        }
        last.GetComponentInParent<Road>().OnFreeSetted();
    }
    public GameObject next=null;
    public GameObject last=null;

    [SerializeField]
    private bool lastLaneWasRight;
    public void MoveToNewDest()
    {
        
        if (next != null)
        {
            last = next;
            lastLaneWasRight = righlane;
        }
        next = null;
        if (next == null)
        {
            next = SelectLane();
        }
        TryFree();
    }

    public void TryFree()
    {
        if (isFree())
        {
            next.GetComponentInParent<Road>().OnSetFree -= CheckifFreeAgain;
            SetNotFree();
            if (last != null)
            {
                SetFree();
                //currprog++;
            }

            ai.SetDestination(next.transform.position);
            isMoving = true;

        }
        else
        {
            Debug.Log("Event triggered");
            next.GetComponentInParent<Road>().OnSetFree -= CheckifFreeAgain;
            next.GetComponentInParent<Road>().OnSetFree += CheckifFreeAgain;
        }
    }

    private void CheckifFreeAgain(object sender, EventArgs e)
    {
        TryFree();
    }

    void Update()
    {
        if (ontrack && !movingtostart )
        {
      
     
            

            float distance = Vector3.Distance(transform.position, ai.destination);
            if (distance < 0.15f&&isMoving )
            {
                //firstdone = true;
                //check if stop
                isMoving = false;
                    if (Route[currprog ].Road_HasBusStop() && Route[currprog ].GetStopType() == type || Route[currprog ].Road_HasBusStop() && Route[currprog].GetStopType() == StopType.Universal)
                    {
                        Debug.Log("stop");
                        if (currprog == maxprogress)
                        {
                            
                            currprog = 0;
                            Route.Reverse();
                            Debug.Log("reversed");
                           // righlane=!righlane;
                              //righlane = false;

                              //lastwasrighlane = false;
                        }
                        //SelectLane();
                     /*   if (CheckIfFree())
                        {
                            Debug.Log("STARTEDFROMSTOP");
                            isMoving = false;
                            //SelectLane();
                            //setFree();
                            currprog++;
                        }*/
                        
                        
                    }
                   // else
                    //{
                        //checkiffree

                        
                        //if (CheckIfFree())
                       // {
                        //Debug.Log("Startiiiiiiing");
                        //SelectLane();
                        //setFree();
                        
                    //setFree();
                    //SelectLane();
                    
                            currprog++;
                    
                    MoveToNewDest();
                    
                       // }
                      
                       

                    //}
                    
                    
                    
                
                
               
                
            }
        }/*else if (movingtostart)
        {
            float distance = Vector3.Distance(transform.position, ai.destination);
            //Debug.Log(distance);
            if (distance < 0.05f)
            {
                if (Route.Count>0)
                {
                    ontrack = true;
                }

                //
                
                currprog=1;
                SelectLane();
                SelectLane();
                
                //Debug.Log(currprog);
                //SelectLane();
                if (CheckIfFree())
                {
                   // Debug.Log("sdadsadasd" );
                    movingtostart = false;
                    //SelectLane();
                }
                //
            }

        }*/
    }
}
