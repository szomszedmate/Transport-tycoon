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
        if (route == null) return;
        Route = route;
        startpoz = route[0];
        maxprogress = Route.Count - 1;
        currprog = 0;
        //movingtostart = true;
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
        Vector3 forward = Vector3.zero;
        int progress = currprog;

        // Menetirány meghatározása a jelenlegi és a következő út alapján
        if (progress < maxprogress)
        {
            // Előre nézünk a következő elemre
            forward = (Route[progress + 1].transform.position - Route[progress].transform.position).normalized;
        }
        else if (progress > 0)
        {
            // Ha az utolsó elemnél vagyunk (bár Reverse után ez ritka), az előzőtől nézzük az irányt
            forward = (Route[progress].transform.position - Route[progress - 1].transform.position).normalized;
        }

        if (forward == Vector3.zero) forward = transform.forward; // Biztonsági tartalék

        // A jobbra mutató vektor kiszámítása
        Vector3 right = Vector3.Cross(Vector3.up, forward);

        Road currentRoad = Route[progress];
        Debug.Log($"[SELECT] currprog={progress} forward={forward} right={Vector3.Cross(Vector3.up, forward)}");


        // Kereszteződés kezelése
        if (currentRoad.transform.GetChild(0).tag != "straight_road" && currentRoad.transform.GetChild(0).tag != "turn_road")
        {
            // Minden sáv irányvektora a középponthoz képest
            Vector3 toLeft = (currentRoad.leftLane.position - currentRoad.transform.position).normalized;
            Vector3 toRight = (currentRoad.rightLane.position - currentRoad.transform.position).normalized;
            Vector3 toTop = (currentRoad.topLane.position - currentRoad.transform.position).normalized;
            Vector3 toBottom = (currentRoad.bottomLane.position - currentRoad.transform.position).normalized;

            float dotL = Vector3.Dot(right, toLeft);
            float dotR = Vector3.Dot(right, toRight);
            float dotT = Vector3.Dot(right, toTop);
            float dotB = Vector3.Dot(right, toBottom);

            float maxDot = Mathf.Max(dotL, dotR, dotT, dotB);

            if (maxDot == dotL) return currentRoad.leftLane.gameObject;
            if (maxDot == dotR) return currentRoad.rightLane.gameObject;
            if (maxDot == dotB) return currentRoad.bottomLane.gameObject;
            return currentRoad.topLane.gameObject;
        }

        // Sima út kezelése
        Vector3 tL = (currentRoad.leftLane.position - currentRoad.transform.position).normalized;
        Vector3 tR = (currentRoad.rightLane.position - currentRoad.transform.position).normalized;

        Debug.Log($"[SELECT] rightLane pos={currentRoad.rightLane.position} leftLane pos={currentRoad.leftLane.position} road pos={currentRoad.transform.position}");
        Debug.Log($"[SELECT] dotR={Vector3.Dot(right, tR):F3} dotL={Vector3.Dot(right, tL):F3} → {(Vector3.Dot(right, tR) > Vector3.Dot(right, tL) ? "JOBB" : "BAL")}");

        if (Vector3.Dot(right, tR) > Vector3.Dot(right, tL))
        {
            righlane = true;
            return currentRoad.rightLane.gameObject;
        }
        else
        {
            righlane = false;
            return currentRoad.leftLane.gameObject;
        }
    }
    public void MoveToStart()
    {
      
        transform.position = startpoz.transform.position;
        ontrack = true;
        movingtostart = false;
        MoveToNewDest();

    }

    public void stopbusz()
    {
        ai.isStopped = true;
    }

    public void startbusz()
    {
        ai.isStopped = false;
    }

    //megnezi hogy szabad e
    public bool isFree()
    {
        Road nextRoad = next.GetComponentInParent<Road>();
        if (nextRoad.data.Description == "Straight Road" || nextRoad.data.Description == "Right Turn")
        {
            if (next == nextRoad.rightLane.gameObject)
                return nextRoad.rightlanefree;
            else
                return nextRoad.leftlanefree;
        }
        else
        {
            return (nextRoad.buszok.Count == 0 || nextRoad.buszok[nextRoad.buszok.Count - 1] == this);
        }
    }


    //lefoglalja azt a savot ahova éppen tart
    public void SetNotFree()
    {
        if (next.GetComponentInParent<Road>().data.Description == "Straight Road" || next.GetComponentInParent<Road>().data.Description == "Right Turn")
        {

            //Ha egyenes vagy kanyar 
            Road nextRoadComp = next.GetComponentInParent<Road>();
            if (next == nextRoadComp.rightLane.gameObject)
                nextRoadComp.rightlanefree = false;
            else
                nextRoadComp.leftlanefree = false;
        }
        else
        {
            //ha kereszteződés
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
            Road lastRoad = last.GetComponentInParent<Road>();
            if (last == lastRoad.rightLane.gameObject)
                lastRoad.rightlanefree = true;
            else
                lastRoad.leftlanefree = true;

        }
        else
        {
            //ha kereszteződés volt
            last.GetComponentInParent<Road>().RemBusz();
        }
        last.GetComponentInParent<Road>().OnFreeSetted();
        Debug.Log($"[SETFREE] last={last.name} tag={last.tag}");

    }


    //next ben van akövetkezö sáv gameobjectje, lastban pedig amit majd fel kell szabaditani ha eltudott indulni a következöre
    public GameObject next=null;
    public GameObject last=null;

    [SerializeField]
    private bool lastLaneWasRight;

    //kivalasztja a következö célt és megnezi szabad e
    public void MoveToNewDest()
    {
        Debug.Log("next: " + next);
        if (next != null)
        {
            last = next;
            //lastLaneWasRight = righlane;
        }
        next = SelectLane();
        Debug.Log("nextagain: " + next);
        if (next == null)
        {
            Debug.LogError($"[BusAi] Hiba: A SelectLane nem talált célpontot a(z) {currprog}. indexnél!");
            return;
        }
        TryFree();
        if (ai.enabled)
        {
            ai.isStopped = false;
            ai.SetDestination(next.transform.position);
        }
    }



    //megnezi hogy szabad e a cella ahova menni akar, ha igen beallitja az ai nak, hanem feliratkozik a cella esemenyere ami akkor hivodik meg ha egy másik jármű felszabaditja azt
    public void TryFree()
    {
        if (next == null)
        {
            isMoving = false;
            return;
        }
        Debug.Log($"[TRYFREE] next={next.name} righlane={righlane} isFree={isFree()}");
        BusAiAgent waitingFor = GetBusIAmWaitingFor();
        bool isDeadlock = (waitingFor != null && waitingFor.GetBusIAmWaitingFor() == this);

        Road nextRoad = next.GetComponentInParent<Road>();
        if (isFree() || isDeadlock) // ha free vagy egymásra várnak
        {
            nextRoad.OnSetFree -= CheckifFreeAgain;
            SetNotFree();
            if (last != null)
            {
                SetFree();
            }
            Debug.Log("Setting new dest: " + next.transform.position + " current pos: " + transform.position );
            ai.SetDestination(next.transform.position);
            ai.speed =speed * nextRoad.speedmodifier;
            isMoving = true;

        }
        else
        {
            //Debug.Log("Event triggered");
            nextRoad.OnSetFree -= CheckifFreeAgain;
            nextRoad.OnSetFree += CheckifFreeAgain;
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

        // Ha ez egy kereszteződés és vannak benne mások
        if (targetRoad.buszok != null && targetRoad.buszok.Count > 0)
        {
            // Ha mi is benne vagyunk a listában, akkor az előttünk lévőt nézzük
            int myIndex = targetRoad.buszok.IndexOf(this);

            if (myIndex > 0)
            {
                // A listában közvetlenül előttünk álló busz
                return targetRoad.buszok[myIndex - 1];
            }
            else if (myIndex == -1)
            {
                // Ha még nem vagyunk a listában (csak a TryFree-nél várunk), 
                // akkor az aktuálisan bent lévő utolsó buszra várunk
                return targetRoad.buszok[targetRoad.buszok.Count - 1];
            }
        }

        return null; // Senkire nem vár
    }

    void Update()
    {
        if (ontrack && !movingtostart &&!ai.isStopped )
        {
            float distance = Vector3.Distance(transform.position, ai.destination);
            Debug.Log($"[UPDATE] currprog={currprog} dist={distance:F3} isMoving={isMoving} dest={ai.destination}");
            //Debug.Log("distance: " + distance + "pos: " + transform.position);
            if (distance < 0.2f&&isMoving )
            {
                isMoving = false;

                if (currprog == 0 && !firstdone)
                {
                    firstdone = true;
                    currprog = 1;
                    MoveToNewDest();
                    return;
                }

                bool hasBusStop = Route[currprog].Road_HasBusStop();
                StopType roadStopType = Route[currprog].GetStopType();
                
                Debug.Log("Type: " + type + " needed: " + roadStopType);
                if (hasBusStop && (roadStopType == type || roadStopType == StopType.Universal || type == StopType.Bus)) // truck csak az egyezo megalloknal es varosoknal, a busz mindenhol megall
                {
                    Debug.Log($"Megállóhoz ért: {currprog}");
                    Arrived?.Invoke(this, new ArrivedEventArgs { Stop = Route[currprog].BusStop });
                } else
                {
                    ProcessNextPoint();
                }                
            }
        }
    }


    public void ProcessNextPoint()
    {
        if (currprog >= maxprogress)
        {
            Debug.Log("--- FORDULÓ ---");

            if (next != null) { last = next; }

            Debug.Log($"[FORDULO] currprog={currprog} max={maxprogress} righlane={righlane}");
            Route.Reverse();
            currprog = 1;
            Debug.Log($"[FORDULO UTAN] Route[0]={Route[0].name} Route[1]={Route[1].name}");

            // Hagyjuk a SelectLane-t eldönteni a sávot az új irány alapján
            // NE állítsuk be manuálisan a righlane-t vagy a next-et
            MoveToNewDest(); // ez meghívja a SelectLane-t, ami az új forward vektorral dolgozik
            return;
        }

        currprog++;
        MoveToNewDest();
    }
}
