using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SquadView : MonoBehaviour
{
    [SerializeField] List<Image> icons;

    public void UpdateViewport(Squad squad)
    {
        icons = transform.GetComponentsInChildren<Image>().ToList();
        icons.RemoveAt(0);

        List<Pair<Unit, Pair<int, int>>> UnitPairs = squad.RetrieveUnitPairs();

        foreach(Image icon in icons) 
            icon.color = new Color(0f, 0f, 0f, 0f);

        foreach(Pair<Unit, Pair<int, int>> UnitPair in UnitPairs)
        {
            int UnitPairIndex = (UnitPair.Second.First * 3) + UnitPair.Second.Second;

            if(UnitPair.First.Icon != null) icons[UnitPairIndex].sprite = UnitPair.First.Icon;
            else icons[UnitPairIndex].sprite = Resources.Load<Sprite>(GlobalSettings.DefaultUnitSpriteIcon);

            icons[UnitPairIndex].color = Color.white;
        }
    }
}
