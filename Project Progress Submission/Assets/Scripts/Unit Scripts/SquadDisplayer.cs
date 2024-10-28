using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class SquadDisplayer : MonoBehaviour
{
    [SerializeField] SquadView sv;
    [SerializeField] Image[] Equipment;
    [SerializeField] TextMeshProUGUI[] EquipmentSprites;

    public void AssignSquad(Squad reference)
    {
        sv.UpdateViewport(reference);

        //Update each equipment slot here
        Equipment[0].sprite = Resources.Load<Sprite>(GlobalSettings.UnknownQuestionMarkIcon);
        Equipment[1].sprite = Resources.Load<Sprite>(GlobalSettings.UnknownQuestionMarkIcon);
        Equipment[2].sprite = Resources.Load<Sprite>(GlobalSettings.UnknownQuestionMarkIcon);

        if(EquipmentSprites.Length == 0) return;
        
        var ResourceCost = reference.GetResourceCost();

        if(ResourceCost.Item1 != 0) EquipmentSprites[0].text = ResourceCost.Item1.ToString();
        else EquipmentSprites[0].transform.parent.gameObject.SetActive(false);

        if(ResourceCost.Item2 != 0) EquipmentSprites[1].text = ResourceCost.Item2.ToString();
        else EquipmentSprites[1].transform.parent.gameObject.SetActive(false);

        if(ResourceCost.Item3 != 0) EquipmentSprites[2].text = ResourceCost.Item3.ToString();
        else EquipmentSprites[2].transform.parent.gameObject.SetActive(false);
        
        if(ResourceCost.Item4 != 0) EquipmentSprites[3].text = ResourceCost.Item4.ToString();
        else EquipmentSprites[3].transform.parent.gameObject.SetActive(false);
        
        if(ResourceCost.Item5 != 0) EquipmentSprites[4].text = ResourceCost.Item5.ToString();
        else EquipmentSprites[4].transform.parent.gameObject.SetActive(false);

        if(ResourceCost.Item6 != 0) EquipmentSprites[5].text = ResourceCost.Item6.ToString();
        else EquipmentSprites[5].transform.parent.gameObject.SetActive(false);
    }
}
