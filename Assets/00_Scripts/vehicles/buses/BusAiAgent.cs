using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

using System;
using System.Linq;



public class BusAiAgent : MonoBehaviour
{
    public System.EventHandler<EventArgs> Reversed;
    public delegate void ArrivedAtStop(object sender, ArrivedEventArgs e);
    public event ArrivedAtStop Arrived;
    private NavMeshAgent ai;
    public Transform targetpos;
    public List<Road> Route = null;
    public bool nonStop;
    public bool oda = true;
    public bool isMoving = false;
    public int maxprogress = 0;
    public int currprog = 0;
    public bool ontrack = false;
    [SerializeField]
    private bool righlane = false;

    //private bool lastwasrighlane = false;
    //[SerializeField]
    //private bool laneslected = false;
    [SerializeField]
    private bool movingtostart = false;
    [SerializeField]
    private Road startpoz;
    public List<StopType> types = new List<StopType>();
    public StopType mainType;
    [SerializeField]
    public float speed;

    private bool firstdone = false;
    private bool isProcessingFree = false;

    void Start()
    {
        ai = GetComponent<NavMeshAgent>();
        ai.speed = speed;
        ai.enabled = true;
    }

    public void RemoveRoute()
    {
        if (ai != null && ai.enabled)
        {
            ai.ResetPath();
            ai.isStopped = true;
        }
        if (Route != null)
        {
            Route.Clear();
        }



        oda = true;
        righlane = false;
        //laneslected = false;
        movingtostart = false;
        isMoving = false;
        maxprogress = 0;
        currprog = 0;
        ontrack = false;
        firstdone = false;
        ai.isStopped = true;

        if (startpoz != null)
        {
            transform.position = startpoz.transform.position;
            if (ai != null && ai.enabled)
            {
                ai.Warp(startpoz.transform.position);
            }
        }
        transform.rotation = Quaternion.identity;
    }

    public void GiveRoute(List<Road> route, bool isNonStop)
    {

        Route = route;
        nonStop = isNonStop;
        startpoz = route[0];
        maxprogress = Route.Count - 1;
        currprog = 0;
        Debug.Log("Moving to start");
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


    //ez dnti el hogy melyik svot vlassza a kvetkez clnak. egyenes s kanyar eseten csak jobb s bal van keresztezdsben 4 lehetsg van azrt hogy vizulisan is jot vlasszon s ne szembesvba menjen.
    //Nha mg mindig a szembesvot vlasztja de csak vizulisan, ezt a keresztezdsekben lv 4 pont mozgatsval lehetne majd megoldani taln, mert a legrvidebb utat valasztja a kvetkez pontig s van hogyugy jn ki neki hogy a masik savban gyorsabb.
    //de ez csak vizulis bug, logikailag jo svot vlaszt
    public GameObject SelectLane()
    {

        //Debug.Log("SelectLaneStarted");
        /*        
        right a menetirany szeriont jobb oldal
        a to...lane hogy az adott pont az ut kozepetol melyik iranyba van
       vector.dot eldonti h melyik pont van legjobbrabb 
        */

        Vector3 forward = new Vector3();
        int progress = currprog;

        if (progress < maxprogress)
        {
            forward = (Route[progress + 1].transform.position - Route[progress].transform.position).normalized;
        }
        else if (nonStop && progress == maxprogress)
        {
            forward = (Route[0].transform.position - Route[progress].transform.position).normalized;
        }
        else // Minden más esetben (pl. megállás a végén)
        {
            forward = transform.forward;
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
        //currprog itt a kovetkezo tile mindig. nem az amin ppen van hanem amire menni akar majd.
        if (Route[progress].transform.GetChild(0).tag == "straight_road" || Route[progress].transform.GetChild(0).tag == "turn_road")
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
                //laneslected = true;
                return Route[progress].leftLane.gameObject;
            }
            else if (max == jobb)
            {

                return Route[progress].rightLane.gameObject;
            }
            else if (max == also)
            {
                //laneslected = true;

                return Route[progress].bottomLane.gameObject;
            }
            else
            {
                //laneslected = true;

                return Route[progress].topLane.gameObject;
            }


        }

        /*
         https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Vector3.Dot.html
        Calculates the dot product of two three-dimensional vectors defined in the same coordinate space.

        The dot product is a float value equal to the product of the magnitudes of the lhs and rhs vectors and the cosine of the angle between them.
         */


        /*
        right valtozoban levan tarolva h a menetirannyal merleges jobbirnyu vektor,
        a dot megmondja melyik lane mutat inkabb a right vektor iranyaba

         */



        /*
          huzunk egy egyenes vonalat a haladas iranyaba majd ebbl szamolunk egy corsst
        ami megmutatja merre van jobb irany ezutn emgnezzuk a ket lane kooridnata kzl hogymelyik mutat a jobbra mutato vektor iranyahoz
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
        //transform.position = startpoz.transform.position;
        //ontrack = true;
        //ai.enabled = true;
        //MoveToNewDest();

        // 1. Először kikapcsoljuk az ágenst, hogy ne vitatkozzon a pozícióváltással
        ai.enabled = false;

        // 2. Beállítjuk a pozíciót
        transform.position = startpoz.transform.position;

        // 3. Visszakapcsoljuk
        ai.enabled = true;

        // 4. KRITIKUS: A Warp kényszeríti az ágenst a hálóra!
        // Ha ezt nem hívod meg, a SetDestination "not placed on NavMesh" hibát dob.
        if (ai.Warp(startpoz.transform.position))
        {
            ontrack = true;
            MoveToNewDest();
        }
        else
        {
            Debug.LogError($"Súlyos hiba: A buszt ({gameObject.name}) nem sikerült a NavMesh-re rakni a startponton: {startpoz.transform.position}");
        }
    }

    // Update is called once per frame
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
        {//ha keresztezds
            return (next.GetComponentInParent<Road>().buszok.Count == 0 || next.GetComponentInParent<Road>().buszok[next.GetComponentInParent<Road>().buszok.Count - 1] == this);
        }
    }


    //lefoglalja azt a savot ahova ppen tart
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
            //ha keresztezds
            //ilyenkor hozzadja magt a keresztezdsben egy listhoz.
            //egyszerre csak egy lehet a keresztezdsben jelenleg s ezt knmegoldani , hogy lehessen tbb is csak ne keresztezzek egymast, ha keresztezik akkor erkezesi sorrend szerint. (ez a feladat lers)
            next.GetComponentInParent<Road>().AddBusz(this);
        }
    }


    //felszabaditja a last gameobjectet, azt a svot amit elhagyott
    public void SetFree()
    {
        if (last == null) return;

        Road lastRoad = last.GetComponentInParent<Road>();
        if (lastRoad == null) return;

        isProcessingFree = true;

        if (lastRoad.data.Description == "Straight Road" || lastRoad.data.Description == "Right Turn")
        {

            //ha egyenes vagy sima kanyar volt
            if (last.gameObject.tag == "right")
            {
                lastRoad.rightlanefree = true;
            }
            else if (last.gameObject.tag == "left")
            {
                lastRoad.leftlanefree = true;
            }

        }
        else
        {
            //ha keresztezds volt
            lastRoad.RemBusz();
        }
        lastRoad.OnFreeSetted();
        
    }


    //next ben van akvetkez sv gameobjectje, lastban pedig amit majd fel kell szabaditani ha eltudott indulni a kvetkezre
    public GameObject next = null;
    public GameObject last = null;

    [SerializeField]
    private bool lastLaneWasRight;

    //kivalasztja a kvetkez clt s megnezi szabad e
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



    //megnezi hogy szabad e a cella ahova menni akar, ha igen beallitja az ai nak, hanem feliratkozik a cella esemenyere ami akkor hivodik meg ha egy msik jrm felszabaditja azt
    //public void TryFree()
    //{
    //    if (next == null)
    //    {
    //        Debug.LogWarning($"{gameObject.name}: Next is null in TryFree!");
    //        return;
    //    }
    //    bool free = isFree();
    //    if (free || GetBusIAmWaitingFor().GetBusIAmWaitingFor() == this) // ha free vagy egymsra vrnak
    //    {
    //        next.GetComponentInParent<Road>().OnSetFree -= CheckifFreeAgain;
    //        SetNotFree();
    //        if (last != null)
    //        {
    //            SetFree();
    //        }
    //        ai.SetDestination(next.transform.position);
    //        ai.speed = speed * next.GetComponentInParent<Road>().speedmodifier;
    //        isMoving = true;
    //    }
    //    else
    //    {
    //        next.GetComponentInParent<Road>().OnSetFree -= CheckifFreeAgain;
    //        next.GetComponentInParent<Road>().OnSetFree += CheckifFreeAgain;
    //    }
    //}


    public void TryFree()
    {
        if (isProcessingFree) return;
        if (next == null) return;

        Road nextRoad = next.GetComponentInParent<Road>();
        if (nextRoad == null) return; // Ha nincs Road script a szülőn, megállunk.

        // Szétbontjuk a láncolt hívást, mert ha az első null-t ad, a második elszáll
        bool deadlock = false;
        var waitingFor = GetBusIAmWaitingFor();
        if (waitingFor != null)
        {
            var waitingForOther = waitingFor.GetBusIAmWaitingFor();
            if (waitingForOther == this) deadlock = true;
        }

        if (isFree() || deadlock)
        {
            nextRoad.OnSetFree -= CheckifFreeAgain; // Most már biztonságos
            SetNotFree();
            if (last != null) SetFree();

            ai.SetDestination(next.transform.position);
            ai.speed = speed * nextRoad.speedmodifier;
            isMoving = true;
        }
        else
        {
            nextRoad.OnSetFree -= CheckifFreeAgain;
            nextRoad.OnSetFree += CheckifFreeAgain;
        }
        isProcessingFree = false;
    }


    private void CheckifFreeAgain(object sender, EventArgs e)
    {
        TryFree();
    }

    public BusAiAgent GetBusIAmWaitingFor()
    {
        // Ha nem vrakozunk semmire, null-t adunk vissza
        if (next == null) return null;

        Road targetRoad = next.GetComponentInParent<Road>();
        if (targetRoad == null || targetRoad.buszok == null || targetRoad.buszok.Count == 0) return null;

        // Ha mi is benne vagyunk a listban, akkor az elttnk lvt nzzk
        int myIndex = targetRoad.buszok.IndexOf(this);
        
        if (myIndex > 0)
        {
            // A listban kzvetlenl elttnk ll busz
            return targetRoad.buszok[myIndex - 1];
        }
        else if (myIndex == -1)
        {
            // Ha mg nem vagyunk a listban (csak a TryFree-nl vrunk), 
            // akkor az aktulisan bent lv utols buszra vrunk
            return targetRoad.buszok[targetRoad.buszok.Count - 1];
        }

        return null; // Senkire nem vr
    }

    void Update()
    {
        if (ontrack && !movingtostart && !ai.isStopped)
        {
            float distance = Vector3.Distance(transform.position, ai.destination);

            if (distance < 0.2f && isMoving)
            {
                //isMoving = false;

                // Csak a legelső indulásnál (amikor lerakod a buszt) kell ez a sávváltó logika
                // Ha már úton van (nem null a last), akkor kezeljük rendes megállóként
                if (currprog == 0 && !firstdone && last == null)
                {
                    //Debug.Log($"Start megállóhoz ért (Index: {currprog})");
                    Arrived?.Invoke(this, new ArrivedEventArgs { Stop = Route[currprog].BusStop });
                    firstdone = true;
                    currprog = 1;
                    MoveToNewDest();
                    return;
                }

                // Megálló ellenőrzése
                bool hasBusStop = Route[currprog].Road_HasBusStop();
                StopType roadStopType = Route[currprog].GetStopType();


                if (hasBusStop && (types.Contains(roadStopType) || roadStopType == StopType.Universal || mainType == StopType.Bus))
                {
                    Arrived?.Invoke(this, new ArrivedEventArgs { Stop = Route[currprog].BusStop });
                }
                else
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
            if (!nonStop)
            {
                if (next != null) { last = next; }
                Route.Reverse();
                currprog = 0;

                if (Route[currprog].Road_HasBusStop())
                {
                    Arrived?.Invoke(this, new ArrivedEventArgs { Stop = Route[currprog].BusStop }); // bug? kamion utvonal vegen 2x hivja az arrivedot
                    return;
                }
            } else
            {
                if (next != null) { last = next; }
                currprog = 0;
                firstdone = false;
            }
            Reversed?.Invoke(this, EventArgs.Empty);
            MoveToNewDest();
            return;
        }

        currprog++;
        MoveToNewDest();
    }
}
