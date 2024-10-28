using UnityEngine;
using UnityEngine.EventSystems;

public class UIDescriptionBox : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RectTransform MovingObject;
    public Vector3 offset = new(0.1f, -0.1f, 0);
    public Camera cam;

    GameObject OnHovering;

    string Description;
    SettingKey setting;

    Ray ray;
    RaycastHit hit;

    Timer appearanceTimer = new(0.5f);

    public void Awake()
    {
        appearanceTimer.TimerFunction += showDescription;
    }

    public void Update()
    {
        MoveObject();

        ray = Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));

        if(Physics.Raycast(ray, out hit))
        {
            GameObject hoveredObject = hit.collider.gameObject;

            Debug.Log(hoveredObject.name);
        }
    }

    public void MoveObject()
    {
        MovingObject.transform.position = new(cam.ScreenToWorldPoint(Input.mousePosition).x, cam.ScreenToWorldPoint(Input.mousePosition).y, 0);
        MovingObject.transform.position += new Vector3(1f, -1f, 0f);
    }

    public void showDescription()
    {

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Hovered: " + gameObject.name);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Stopped hovering over: " + gameObject.name);
    }
}