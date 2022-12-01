using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Manages the dialogue UI elements and the TextArchitect
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public List<string> speakerNames = new List<string>();
    public List<Sprite> speakerPortraits = new List<Sprite>();

    public static DialogueManager instance;
    public ELEMENTS elements;

    [SerializeField]
    private GameObject continueObject;

    public PlayerController player;


    void Awake()
    {
        instance = this;
    }

    private void Start()
    {
    }

    public void Speak(string dialogue, string speaker = "", bool additive = false)
    {

        StopSpeaking();

        if (additive)
        {
            dialogueText.text = targetDialogue;
        }

        continueObject.SetActive(false);

        speaking = StartCoroutine(Speaking(dialogue, additive, speaker));

    }

    public void StopSpeaking()
    {
        if (isSpeaking)
        {
            StopCoroutine(speaking);
        }
        if (textArchitect != null && textArchitect.isConstructing)
        {
            textArchitect.Stop();
        }
        speaking = null;


    }

    public bool isSpeaking { get { return speaking != null; } }
    /*[HideInInspector] */
    public bool isWaitingForUserInput = false;

    public string targetDialogue = "";
    Coroutine speaking = null;
    public TextArchitect textArchitect = null;
    public TextArchitect currentArchitect { get { return textArchitect; } }

    IEnumerator Speaking(string dialogue, bool additive, string speaker = "")
    {
        ////Initiate game-wide pauses where relevant
        //if (GameplayPause.instance != null)
        //{
        //    GameplayPause.instance.StopGameplay();
        //}

        dialoguePanel.SetActive(true);


        string additiveDialogue = additive ? dialogueText.text : "";
        targetDialogue = additiveDialogue + dialogue;

        if (textArchitect == null)
        {
            textArchitect = new TextArchitect(dialogueText, dialogue, additiveDialogue);
        }
        else
        {
            textArchitect.Renew(dialogue, additiveDialogue);
        }

        elements.speakerName = DetermineSpeaker(speaker);
        elements.speakerPortrait = DetermineSpeakerPortrait(speaker);
        elements.speakerText.text = speaker;
        elements.speakerImageContainer.sprite = elements.speakerPortrait;

        speakerPanel.SetActive(speaker != "");


        isWaitingForUserInput = false;

        if (isClosed)
        {
            OpenAllRequirementsForDialogueSystemVisibility(true);
        }

        //while (textArchitect.isConstructing)
        //{
        //    if (Input.GetKeyDown(KeyCode.RightArrow))
        //    {
        //        textArchitect.skip = true;
        //    }

        //    yield return new WaitForEndOfFrame();
        //}

        while (textArchitect.isConstructing)
        {
            //if (Gamepad.current.enabled)
            //{
            //    if (Gamepad.current.aButton.wasPressedThisFrame)
            //    {
            //        textArchitect.skip = true;
            //    }

            //}
            //else
            //{
                if (Mouse.current.leftButton.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    textArchitect.skip = true;

                    AudioManager.instance.PlaySoundFromList("Click1");

                }

            //}

            yield return new WaitForEndOfFrame();
        }


        //text has finished
        isWaitingForUserInput = true;
        while (isWaitingForUserInput)
        {
            continueObject.SetActive(true);
            yield return new WaitForEndOfFrame();
        }

        continueObject.SetActive(false);

        StopSpeaking();
    }

    string DetermineSpeaker(string s)
    {
        string returnValue = speakerName;//default is the current name


        if (s != speakerName && s != "")
        {
            returnValue = (s.ToLower().Contains("player")) ? "" : s;
        }

        return returnValue;

    }

    Sprite DetermineSpeakerPortrait(string s)
    {
        //Check the dictionary for the string s and return the corresponding image
        if (speakerNames.Contains(s))
        {
            //Debug.Log(s);

            int i = speakerNames.IndexOf(s);
            
            
            return speakerPortraits[i];
        }
        else
        {
            return null;
        }

    }

    public void Close()
    {
        ////re-enable paused entities
        //GameplayPause.instance.EnableGameplay();

        AudioManager.instance.PlaySoundFromList("Click1");

        if (WaveManager.instance != null)
        {
            WaveManager.instance.dialogueInPlay = false;
        }
        if(InterWaveManager.instance != null)
        {
            InterWaveManager.instance.dialogueInPlay = false;
        }
        if(StartSceneManager.instance != null)
        {
            StartSceneManager.instance.dialogueInPlay = false;
        }
        if(FinalInterviewScene.instance != null)
        {
            FinalInterviewScene.instance.dialogueInPlay = false;
        }

        StopSpeaking();

        for (int i = 0; i < DialoguePanelRequirements.Length; i++)
        {
            DialoguePanelRequirements[i].SetActive(false);
        }
    }

    public void OpenAllRequirementsForDialogueSystemVisibility(bool v)
    {
        ////Initiate game-wide pauses where relevant
        //GameplayPause.instance.StopGameplay();

        for (int i = 0; i < DialoguePanelRequirements.Length; i++)
        {
            DialoguePanelRequirements[i].SetActive(v);
        }
    }

    public void Open(string speakerName = "", string dialogue = "")
    {
        if (speakerName == "" && dialogue == "")
        {
            OpenAllRequirementsForDialogueSystemVisibility(false);
            return;
        }

        OpenAllRequirementsForDialogueSystemVisibility(true);

        elements.speakerName = speakerName;
        speakerPanel.SetActive(speakerName != "");
        dialogueText.text = dialogue;
    }

    public bool isClosed { get { return !dialoguePanel.activeInHierarchy; } }

    [System.Serializable]
    public class ELEMENTS
    {
        public GameObject dialogueBox;
        public GameObject speakerBox;
        public TextMeshProUGUI dialogueText;
        public string speakerName;
        public Sprite speakerPortrait;
        public TextMeshProUGUI speakerText;
        public Image speakerImageContainer;
    }
    public GameObject dialoguePanel { get { return elements.dialogueBox; } }
    public GameObject speakerPanel { get { return elements.speakerBox; } }
    public Sprite speakerPortrait { get { return elements.speakerPortrait; } }

    public Image speakerImageContainer { get { return elements.speakerImageContainer; } }

    public string speakerName { get { return elements.speakerName; } }
    public TextMeshProUGUI dialogueText { get { return elements.dialogueText; } }
    public TextMeshProUGUI speakerText { get { return elements.speakerText; } }

    public GameObject[] DialoguePanelRequirements;
    //public GameObject dialoguePanel;

}
