using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InfoDispatcher : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI nametext;
    public TextMeshProUGUI descriptiontext;

    public string itemName;
    public string itemDescription;

    private string startName;
    private string startDescription;

    private void Awake()
    {
        startName = nametext.text;
        startDescription = descriptiontext.text;
    }

    //private void OnMouseOver()
    //{
    //    print("Attempting to read info");
    //    SetInfo(GetComponent<Image>().sprite);
    //}

    public void OnPointerEnter(PointerEventData eventData)
    {
        print("Attempting to read info");
        SetInfo(GetComponent<Image>().sprite);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        print("No longer hovering");
        nametext.text = startName;
        descriptiontext.text = startDescription;

    }

    void SetInfo(Sprite sprite)
    {
        print("Attempting to read info from string");

        switch (sprite.name)
        {
            case "Carrot":
                itemName = "Carrot Knives";
                itemDescription = "Carrots sharpened to a point. Used for quick, piercing firepower.";
                break;
            case "Bananarang":
                itemName = "Bananarang";
                itemDescription = "Bananas shaped perfectly for playing catch with yourself.";
                break;
            case "PizzaDisc":
                itemName = "Pizza Discuses";
                itemDescription = "Pizza with a nasty, rubbery crust. Terrible for eating, good for bouncing off walls and your enemies.";
                break;
            case "Lid":
                itemName = "Pot Lid";
                itemDescription = "Not sure where the pot went, but seems to work great as a riot shield. Only works if you charge your glutes by holding the attack button and dash around like a madman with it.";
                break;
        }




        nametext.text = itemName;
        descriptiontext.text = itemDescription;
    }
}
