using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
public class InteractableObject : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Important Options")]
    [SerializeField] bool OutlineOrHighlight;

    [SerializeField] Image TargetImage;

    [Header("Interactable Colors")]
    Color Current;
    [SerializeField] Color NormalColor = new Color(0.4f, 0.4f, 0.4f, 0.4f);
    [SerializeField] Color HoveredColor = new Color(0.4f, 0.4f, 0.4f, 0.4f);
    [SerializeField] Color ClickedColor = new Color(0.4f, 0.4f, 0.4f, 0.4f);
    [SerializeField] Color SelectedColor = new Color(0.4f, 0.4f, 0.4f, 0.4f);
    [SerializeField] Color DisabledColor = new Color(0.4f, 0.4f, 0.4f, 0.4f);

    //Using Raycasting
    public event Action LeftClickEvent;
    public event Action RightClickEvent;
    public event Action MiddleClickEvent;

    public event Action OnEnter;
    public event Action OnExit;

    void Awake()
    {
        Current = NormalColor;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left) LeftClickEvent?.Invoke();
        else if(eventData.button == PointerEventData.InputButton.Right) RightClickEvent?.Invoke();
        else if(eventData.button == PointerEventData.InputButton.Middle) MiddleClickEvent?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Current = HoveredColor;
        OnEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Current = NormalColor;
        OnExit?.Invoke();
    }

    //Using colliders
    
}