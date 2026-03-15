using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class MiniMapNav : MonoBehaviour, IPointerClickHandler
{

    private RawImage minmap;
    public GameObject player;
    public Camera minimapcamera;
    //IPointerClickHandler implementálva van ezert OnPointerClcik meghivodik kattintasra
    public void OnPointerClick(PointerEventData eventData)
    {


        //csak jobbklikk
        if (eventData.button==PointerEventData.InputButton.Left)
        {
            Debug.Log("Rákattintottál a minimapra");
            RectTransform minimaptransform = minmap.rectTransform;
            Vector2 localclickpoz;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                minimaptransform, // The RectTransform to find a point inside.
                eventData.position, //Screen space position.
                eventData.pressEventCamera, //The camera associated with the screen space position.
                out localclickpoz //Point in local space of the rect transform.
            );


            Debug.Log(localclickpoz);

            //minimaptransform.rect tartalmazza a raw image transformját
            float normalizedX = Mathf.InverseLerp(minimaptransform.rect.xMin, minimaptransform.rect.xMax, localclickpoz.x); //Determines where a value lies between two points. (min, max, your point)
            float normalizedY = Mathf.InverseLerp(minimaptransform.rect.yMin, minimaptransform.rect.yMax, localclickpoz.y);
            float worldX =(normalizedX - 0.5f) * (2f * minimapcamera.orthographicSize) ;
            /*
             azért kell -0.5 mert 0.0 a map közepe de normalizálásnál:
            0.0: bal
            0.5: közép
            1.0: jobb
            tehat ha kivonunk 0.5 öt akkor:
            -0.5: bal
            0.0: közép
            0.5: jobb

             
            2*minimapcamera.orthographicSize = mapsize, mert minimapcamera.orthographicSize=500, de a mapunk 1000x1000

             */

            float worldZ =(normalizedY - 0.5f) * 2f * minimapcamera.orthographicSize;
            Debug.Log(normalizedX);
            Debug.Log(normalizedY);
            player.transform.position = new Vector3(worldX, player.transform.position.y, worldZ);

        }
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        minmap = gameObject.GetComponent<RawImage>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
