using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

using System;
using System.Linq;



public class BusAiAgent : MonoBehaviour
{
    public delegate void ArrivedAtStop(object sender, ArrivedEventArgs e);
    public event ArrivedAtStop Arrived;
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
    [SerializeField]
    public float speed;
   
    private bool firstdone = false;

    void Start()
    {
        ai = GetComponent<NavMeshAgent>();
        ai.speed = speed;
        ai.enabled = true;
    }

    public void RemoveRoute()
    {
        //Not working yet
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
        currprog = 0;    
        MoveToStart();
    }
    private bool IsBetween(float value, float min, float max)
    {
        return value >= min && value <= max;
    }


    public void RotateToNext()
    {
        
        Vector3 dir;
      
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
   

    //ez dönti el hogy melyik sávot válassza a következö célnak. egyenes és kanyar eseten csak jobb és bal van keresztezödésben 4 lehetöség van azért hogy vizuálisan is jot válasszon és ne szembesávba menjen.
    //Néha még mindig a szembesávot választja de csak vizuálisan, ezt a keresztezödésekben lévö 4 pont mozgatásával lehetne majd megoldani talán, mert a legrövidebb utat valasztja a következö pontig és van hogyugy jön ki neki hogy a masik savban gyorsabb.
    //de ez csak vizuális bug, logikailag jo sávot választ
    public GameObject SelectLane()
    {

        //Debug.Log("SelectLaneStarted");
        /*        
        right a menetirany szeriont jobb oldal
        a to...lane hogy az adott pont az ut kozepetol melyik iranyba van
       vector.dot eldonti h melyik pont van legjobbrabb 
        */

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
                progress = currprog + 1;
                forward = (Route[currprog + 1].transform.position - Route[currprog].transform.position).normalized;
            }
            else
            {
                progress = currprog;
                forward = (Route[currprog ].transform.position - Route[currprog-1].transform.position).normalized;
            }
            
        }
            
        
       

       
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
                return Route[progress].leftLane.gameObject;
            }
            else if (max == jobb)
            {
               
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
                          
            righlane = true;           
            return Route[progress].rightLane.gameObject;

        }
        else
        {
                       
            righlane = false;
            
            return Route[progress].leftLane.gameObject;
            
           
        }
        
    }
    public void MoveToStart()
    {
      
        transform.position = startpoz.transform.position;
        ontrack = true;
        MoveToNewDest();

    }

    // Update is called once per frame
    public void stopbusz()
    {
 
    }

    public void startbusz()
    {
       
        ai.isStopped = false;
    }

    //megnezi hogy szabad e
    public bool isFree() {
        if (next.GetComponentInParent<Road>().data.Description == "Straight Road" || next.GetComponentInParent<Road>().data.Description == "Right Turn")
        { //Ha egyenes vagy kanyar 
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
        {//ha keresztezõdés
            return (next.GetComponentInParent<Road>().buszok.Count == 0 || next.GetComponentInParent<Road>().buszok[next.GetComponentInParent<Road>().buszok.Count - 1] == this);
        }
    }


    //lefoglalja azt a savot ahova éppen tart
    public void SetNotFree()
    {
        if (next.GetComponentInParent<Road>().data.Description == "Straight Road" || next.GetComponentInParent<Road>().data.Description == "Right Turn")
        {
           
            //Ha egyenes vagy kanyar 
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
            //ha keresztezõdés
            //ilyenkor hozzáadja magát a keresztezödésben egy listához.
            //egyszerre csak egy lehet a keresztezödésben jelenleg és ezt kénmegoldani , hogy lehessen több is csak ne keresztezzek egymast, ha keresztezik akkor erkezesi sorrend szerint. (ez a feladat leírás)
            next.GetComponentInParent<Road>().AddBusz(this);
        }
    }


    //felszabaditja a last gameobjectet, azt a sávot amit elhagyott
    public void SetFree()
    {

        if (last.GetComponentInParent<Road>().data.Description == "Straight Road" || last.GetComponentInParent<Road>().data.Description == "Right Turn")
        {

            //ha egyenes vagy sima kanyar volt
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
            //ha keresztezõdés volt
            last.GetComponentInParent<Road>().RemBusz();
        }
        last.GetComponentInParent<Road>().OnFreeSetted();
    }


    //next ben van akövetkezö sáv gameobjectje, lastban pedig amit majd fel kell szabaditani ha eltudott indulni a következöre
    public GameObject next=null;
    public GameObject last=null;

    [SerializeField]
    private bool lastLaneWasRight;

    //kivalasztja a következö célt és megnezi szabad e
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



    //megnezi hogy szabad e a cella ahova menni akar, ha igen beallitja az ai nak, hanem feliratkozik a cella esemenyere ami akkor hivodik meg ha egy másik jármû felszabaditja azt
    public void TryFree()
    {
        if (isFree() || GetBusIAmWaitingFor().GetBusIAmWaitingFor() == this) // ha free vagy egymásra várnak
        {
            next.GetComponentInParent<Road>().OnSetFree -= CheckifFreeAgain;
            SetNotFree();
            if (last != null)
            {
                SetFree();
            }

            ai.SetDestination(next.transform.position);
            ai.speed =speed * next.GetComponentInParent<Road>().speedmodifier;
            isMoving = true;

        }
        else
        {
            //Debug.Log("Event triggered");
            next.GetComponentInParent<Road>().OnSetFree -= CheckifFreeAgain;
            next.GetComponentInParent<Road>().OnSetFree += CheckifFreeAgain;
        }
        
    }


   
    private void CheckifFreeAgain(object sender, EventArgs e)
    {
        TryFree();
    }

    public BusAiAgent GetBusIAmWaitingFor()
    {
        // Ha nem várakozunk semmire, null-t adunk vissza
        if (next == null) return null;

        Road targetRoad = next.GetComponentInParent<Road>();

        // Ha ez egy keresztezõdés és vannak benne mások
        if (targetRoad.buszok != null && targetRoad.buszok.Count > 0)
        {
            // Ha mi is benne vagyunk a listában, akkor az elõttünk lévõt nézzük
            int myIndex = targetRoad.buszok.IndexOf(this);

            if (myIndex > 0)
            {
                // A listában közvetlenül elõttünk álló busz
                return targetRoad.buszok[myIndex - 1];
            }
            else if (myIndex == -1)
            {
                // Ha még nem vagyunk a listában (csak a TryFree-nél várunk), 
                // akkor az aktuálisan bent lévõ utolsó buszra várunk
                return targetRoad.buszok[targetRoad.buszok.Count - 1];
            }
        }

        return null; // Senkire nem vár
    }

    void Update()
    {
        if (ontrack && !movingtostart )
        {
      
     
            

            float distance = Vector3.Distance(transform.position, ai.destination);
            if (distance < 0.15f&&isMoving )
            {
   
                isMoving = false;
                    
                if (Route[currprog ].Road_HasBusStop() && Route[currprog ].GetStopType() == type || Route[currprog ].Road_HasBusStop() && Route[currprog].GetStopType() == StopType.Universal)
                    
                {
                        
                    Debug.Log("stop");
                        
                    //TODO itt érkezik meg, megallok lerakasanal automatikusan olyan megallot tesz le amilyen letesítmény mellé van leteve
                    //itt kell lekezelni, hogy mi történik megerkezéskor.
                    
                    Arrived?.Invoke(this, new ArrivedEventArgs { Stop = Route[currprog].BusStop });

                    //ha ez a routban az utolso megallo akkor a listat megforditja és kezdi elöröl, de msot visszafele
                    
                       
                        
                    
                }


                if (currprog == maxprogress)
                {
                    //TODO if !linear akkor ne forduljon meg
                    
                    currprog = 0;
                    Route.Reverse();
                    Debug.Log("reversed");
                }
                currprog++;
                    
                    
                MoveToNewDest();
                    
                    
                
                
               
                
            }
        }
    }
}
