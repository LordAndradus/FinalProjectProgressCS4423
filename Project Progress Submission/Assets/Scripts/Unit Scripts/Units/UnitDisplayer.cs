using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UnitDisplayer : MonoBehaviour
{
    public Unit Display = null;

    [Header("Miscellaneous Information")]
    [SerializeField] GameObject SpriteView;
    [SerializeField] TextMeshProUGUI HPValDisplay;
    [SerializeField] TextMeshProUGUI Name;
    [SerializeField] TextMeshProUGUI LevelAndClass;
    [SerializeField] TextMeshProUGUI FieldCost;
    

    [Header("Bars")]
    [SerializeField] StatusBar HPBar;
    [SerializeField] StatusBar XPBar;
    [SerializeField] StatusBar PPBar;

    [Header("Attribute Pairs")]
    [SerializeField] TextMeshProUGUI Threat;
    [SerializeField] TextMeshProUGUI Armor;
    [SerializeField] TextMeshProUGUI Weapon;
    [SerializeField] TextMeshProUGUI Strength;
    [SerializeField] TextMeshProUGUI Agility;
    [SerializeField] TextMeshProUGUI Magic;
    [SerializeField] TextMeshProUGUI Leadership;

    [Header("Resource Management")]
    [SerializeField] GameObject IronCost;
    [SerializeField] GameObject MagicGemCost;
    [SerializeField] GameObject HorseCost;
    [SerializeField] GameObject HolyTearsCost;
    [SerializeField] GameObject AdamantiumCost;

    [Header("Traits")]
    [SerializeField] GameObject TraitViewParent;
    [SerializeField] GameObject TraitPopupView;
    [Header("Trait Info Box")]
    [SerializeField] Image TraitImage;
    [SerializeField] TextMeshProUGUI TraitName;
    [SerializeField] TextMeshProUGUI TraitFlavor;
    [SerializeField] TextMeshProUGUI TraitEffect;

    public void AssignUnit(Unit unit)
    {
        Display = unit;
        if(Display != null) UpdateEverything();
        else throw new System.Exception("NullReferenceException: Unit Displayer was passed a null Unit reference!");
    }

    private void UpdateEverything()
    {
        TraitPopupView.SetActive(false);
        TraitPopupView.transform.SetParent(transform, false);

        //Update Attributes
        UpdateAttributes();
        //Update Bars
        UpdateBars();
        //Update Traits
        UpdateTraits();
        //Update ResourceCosts
        UpdateResourceCosts();
        //Update Miscellaneous
        UpdateMiscellaneous();
    }

    private void UpdateAttributes()
    {
        Threat.text = Display.GetThreat().ToString();
        Armor.text = Display.GetArmor().ToString();
        Weapon.text = Display.GetWeapon().ToString();
        Strength.text = Display.GetStrength().ToString();
        Agility.text = Display.GetAgility().ToString();
        Magic.text = Display.GetMagic().ToString();
        Leadership.text = Display.GetLeadership().ToString();
    }

    private void UpdateBars()
    {
        HPBar.SetToMax(Display.GetHealth());

        XPBar.SetMaxValue(Display.GetXPCap());
        XPBar.SetValue(Display.GetXP());

        PPBar.SetMaxValue(Display.GetPPCap());
        PPBar.SetValue(Display.GetPP());
    }

    private void UpdateTraits()
    {
        TraitPopupView.SetActive(false);

        foreach(Transform child in TraitViewParent.transform)
        {
            Destroy(child.gameObject);    
        }

        int elementNum = 0;
        foreach(Trait trait in Display.traits)
        {
            GameObject TraitBox = UtilityClass.CreatePrefabObject("Assets/PreFabs/TransitionMenu/Marketplace/TraitView.prefab", TraitViewParent.transform, "Trait_" + elementNum);

            if(trait.icon != null) TraitBox.transform.GetChild(1).GetComponent<Image>().sprite = trait.icon;

            TraitBox.GetComponentInChildren<TextMeshProUGUI>().text = trait.UIFriendlyClassName;

            TraitBox.GetComponentInChildren<InteractableObject>().OnEnter += () => {
                Debug.Log("Trait Entered");
                TraitPopupView.SetActive(true);

                //Debug.Log("Mouse position = " + UtilityClass.GetScreenMouseToWorld());

                //UtilityClass.SetWorldPosition(TraitPopupView, UtilityClass.GetScreenMouseToWorld() + new Vector3(0f, 1.5f, 0f));
                TraitPopupView.transform.localPosition = transform.InverseTransformPoint(UtilityClass.GetScreenMouseToWorld() + new Vector3(0f, 1.5f, 0f));
                TraitPopupView.transform.localPosition = new Vector3(TraitPopupView.transform.localPosition.x, TraitPopupView.transform.localPosition.y, 0f);

                //Debug.Log("Popup target position = " + TraitPopupView.transform.position);

                if(trait.icon != null) TraitImage.sprite = trait.icon;
                TraitName.text = trait.UIFriendlyClassName;
                TraitFlavor.text = trait.FlavorText;
                TraitEffect.text = trait.EffectText;
            };

            TraitBox.GetComponentInChildren<InteractableObject>().OnExit += () => {
                Debug.Log("Exitted component");
                TraitPopupView.SetActive(false);
            };
        }
    }

    private void UpdateResourceCosts()
    {
        CheckCost(IronCost, Display.IronCost);
        CheckCost(MagicGemCost, Display.MagicGemCost);
        CheckCost(HorseCost, Display.HorseCost);
        CheckCost(HolyTearsCost, Display.HolyTearCost);
        CheckCost(AdamantiumCost, Display.AdamntiumCost);
    }

    private void CheckCost(GameObject pair, int cost)
    {
        pair.SetActive(cost > 0 ? true : false);
        pair.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = cost.ToString();
    }

    private void UpdateMiscellaneous()
    {
        UpdateSpriteView();

        HPValDisplay.text = Display.GetHealth().ToString() + "/" + Display.GetMaxHealth().ToString();
        Name.text = Display.Name;
        LevelAndClass.text = "Lvl " + Display.GetLevel() + " " + Display.UIFriendlyClassName;
        FieldCost.text = Display.GetFieldCost().ToString();
    }

    private void UpdateSpriteView()
    {
        Image DisplaySprite = SpriteView.GetComponent<Image>();

        if(Display.spriteView == null) DisplaySprite.sprite = Resources.Load<Sprite>(GlobalSettings.DefaultUnitSpriteView);
        else DisplaySprite.sprite = Display.spriteView;
    }
}
