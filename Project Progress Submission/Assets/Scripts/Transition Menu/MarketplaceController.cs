using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MarketplaceController : MonoBehaviour
{
    [Header("External Systems")]
    [SerializeField] MainController mc;
    [SerializeField] EventSystem eventSystem;
    [SerializeField] GameObject MainMenu;

    [Header("Button Tabs")]
    [SerializeField] Button BackButton;
    [SerializeField] Button Enlistment;
    [SerializeField] Button Artificer;
    [SerializeField] Button Pawnshop;
    [SerializeField] Button MarketExchange;

    [Header("Views")]
    [SerializeField] GameObject EnlistView;
    [SerializeField] GameObject ArtificerView;
    [SerializeField] GameObject PawnshopView;
    [SerializeField] GameObject MarketExchangeView;

    [Header("Enlist View")]
    [SerializeField] TextMeshProUGUI AdditionalInfo;
    [SerializeField] UnitDisplayer DisplayUnit;

    [Header("Merchant Inventory")]
    [SerializeField] List<Pair<Unit, int>> UnitsToEnlist;

    GameObject ActiveObject; //There will always be an active object

    //State Machine

    void Awake()
    {
        //Add Button Listeners
        BackButton.onClick.AddListener(() => {
            transform.gameObject.SetActive(false);
            MainMenu.SetActive(true);
        });
        Enlistment.onClick.AddListener(() => {
            DisableAllViews();
            EnlistView.SetActive(true);
        });
        Artificer.onClick.AddListener(() => {
            DisableAllViews();
            ArtificerView.SetActive(true);
        });
        Pawnshop.onClick.AddListener(() => {
            DisableAllViews();
            PawnshopView.SetActive(true);
        });
        MarketExchange.onClick.AddListener(() => {
            DisableAllViews();
            MarketExchangeView.SetActive(true);
        });
    
        //Generate random list of units
        GenerateUnitPurchaseList();
        GenerateEquipmentPurchaseList(); //Need to implement
    }

    void OnEnable()
    {
        UpdateUnitRoster();
    }

    void OnDisable() 
    {
        
    }

    void Update()
    {
        
    }

//*****************************************************************************************************************************//
//General Purpose
void DisableAllViews()
{
    EnlistView.SetActive(false);
    ArtificerView.SetActive(false);
    PawnshopView.SetActive(false);
    MarketExchangeView.SetActive(false);
}

void GenerateUnitPurchaseList()
{
    UnitsToEnlist = new();

    //Add base class units
    UnitsToEnlist.Add(new(GenerateUnit(0), Random.Range(100, 300)));
    UnitsToEnlist.Add(new(GenerateUnit(1), Random.Range(100, 300)));
    UnitsToEnlist.Add(new(GenerateUnit(2), Random.Range(100, 300)));
    UnitsToEnlist.Add(new(GenerateUnit(3), Random.Range(100, 300)));
    UnitsToEnlist.Add(new(GenerateUnit(4), Random.Range(100, 300)));
    UnitsToEnlist.Add(new(GenerateUnit(5), Random.Range(100, 300)));

    //Add 4 mercenaries
    for(int i = 0; i < 4; i++) UnitsToEnlist.Add(new(UnitGenerator.generateMerc(), Random.Range(500, 1500)));
}

void GenerateEquipmentPurchaseList()
{

}

//*****************************************************************************************************************************//
//Enlistment View

public void UpdateUnitRoster()
{
    GameObject ParentContent = EnlistView.transform.GetChild(0).transform.GetChild(2).transform.GetChild(0).gameObject;

    //Destroy each element in the ParentContent
    UtilityClass.DeleteListContent(ParentContent);

    int ElementTracker = 0;
    foreach(Pair<Unit, int> UnitCost in UnitsToEnlist.ToArray())
    {
        GameObject element = UtilityClass.CreatePrefabObject("PreFabs/TransitionMenu/Marketplace/SampleUnitPurchase", ParentContent.transform, "Element_" + ElementTracker);

        InteractableObject ClickHandler = element.GetComponent<InteractableObject>();

        TextMeshProUGUI[] text = element.GetComponentsInChildren<TextMeshProUGUI>();
        Image spriteIcon = element.transform.GetChild(1).GetComponent<Image>();

        ClickHandler.LeftClickEvent += () => {
            Debug.Log("Unit to purcahse: " + UnitCost.First.displayQuickInfo());

            PurchaseAction(UnitCost);
        };

        ClickHandler.RightClickEvent += () => {
            Debug.Log("Right click detected");
        };

        ClickHandler.OnEnter += () => {
            Debug.Log("Set this to be the active object");
            EventSystem.current.SetSelectedGameObject(element);
            DisplayUnit.AssignUnit(UnitCost.First);
            AdditionalInfo.text = UnitCost.First.Description;
        };

        text[0].text = UnitCost.First.Name;
        text[1].text = UnitCost.Second.ToString();

        if(UnitCost.First.Icon == null) spriteIcon.sprite = Resources.Load<Sprite>(GlobalSettings.DefaultUnitSpriteIcon);
        else spriteIcon.sprite = UnitCost.First.Icon;
        
        ElementTracker++;
    }

    DisplayUnit.AssignUnit(UnitsToEnlist[0].First);
    AdditionalInfo.text = UnitsToEnlist[0].First.Description;
}

void PurchaseAction(Pair<Unit, int> pair)
{
    if(UnitResourceManager.Gold - pair.Second < 0)
    {
        Debug.LogError("More gold is required!");
        return;
    }

    //Deduct gold amount
    UnitResourceManager.Gold -= pair.Second;

    int UnitPairIndex = UnitsToEnlist.IndexOf(pair);

    //Add unit to MainController unit list
    mc.UnitList.Add(pair.First);

    UnitsToEnlist.Remove(pair);

    UnitsToEnlist.Insert(UnitPairIndex, new(GenerateUnit(UnitPairIndex), UnitPairIndex <= 5 ? Random.Range(100, 300) : Random.Range(500, 1500)));

    UpdateUnitRoster();
}

private Unit GenerateUnit(int index)
{
    Unit u;

    switch(index)
    {
        case 0:
            u = UnitGenerator.generate<MilitiaMale>();
            break;
        case 1:
            u = UnitGenerator.generate<MilitiaFemale>();
            break;
        case 2:
            u = UnitGenerator.generate<MilitiaBowman>();
            break;
        case 3:
            u = UnitGenerator.generate<Apothecary>();
            break;
        case 4:
            u = UnitGenerator.generate<NeophyteMagus>();
            break;
        case 5:
            u = UnitGenerator.generate<Squire>();
            break;
        default:
            u = UnitGenerator.generateMerc();
            break;
    }

    return u;
}

//*****************************************************************************************************************************//
//Artificer View

//*****************************************************************************************************************************//
//Pawnshop View

//*****************************************************************************************************************************//
//Marketplace View

//*****************************************************************************************************************************//
//State Machine

}

public enum MarketplaceView
{

}
