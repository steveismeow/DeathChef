using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameplayUI : MonoBehaviour
{
    public static GameplayUI instance;

    public Image equippedWeaponSprite;

    public Transform buffUIParent;
    public GameObject buffUIPrefab;
    public GameObject buffStackPrefab;

    public TextMeshProUGUI waveNumberText;

    [SerializeField]
    private List<GameObject> buffStacks = new List<GameObject>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    public void CheckBuffTableForBuff(Sprite buffSprite)
    {
        //check which buff we've recevied, and check to see if we already have a stack of them going. If so, add them to stack, otherwise create a new stack
        foreach (GameObject buffstack in buffStacks)
        {
            if (buffstack.transform.GetChild(0).GetComponent<Image>().sprite == buffSprite)
            {
                GameObject newDuplicateBuffSprite = Instantiate(buffUIPrefab, buffstack.transform);
                newDuplicateBuffSprite.GetComponent<Image>().sprite = buffSprite;
                return;
            }
        }

        GameObject newBuffStack = Instantiate(buffStackPrefab, buffUIParent.transform);
        buffStacks.Add(newBuffStack);
        GameObject newBuffSprite = Instantiate(buffUIPrefab, newBuffStack.transform);
        newBuffSprite.GetComponent<Image>().sprite = buffSprite;


    }

    public void UpdateWaveLevelText(int waveLevel)
    {
        waveNumberText.text = waveLevel.ToString("00");
    }

    public void ClearStacks()
    {
        foreach (GameObject buffStack in buffStacks)
        {
            DestroyImmediate(buffStack);
        }

    }
}
