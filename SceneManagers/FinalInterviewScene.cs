using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalInterviewScene : MonoBehaviour
{
    public static FinalInterviewScene instance;

    private RoomManager roomManager;

    public Animator fadeCanvasAnimator;

    private PlayerController player;

    [SerializeField]
    private int dialogueToLoadIndex;

    [SerializeField]
    private string songToLoad;
    private string dialogueToLoad;


    private void Awake()
    {

        if (instance == null)
        {
            instance = this;
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        fadeCanvasAnimator = GameObject.FindGameObjectWithTag("FadeCanvas").GetComponent<Animator>();
        roomManager = GameObject.FindGameObjectWithTag("RoomManager").GetComponent<RoomManager>();
        startingUp = false;
        exitingScene = false;

        CheckDialogue();
        StartCoroutine(SceneStartUp());
    }

    private void CheckDialogue()
    {
        dialogueToLoadIndex = PlayerDataManager.instance.CheckForDialogue();

        if (dialogueToLoadIndex != 999)
        {

        }
        else
        {
            AudioManager.instance.PlayMusicFromList(songToLoad);
        }
    }



    bool startingUp;
    IEnumerator SceneStartUp()
    {
        startingUp = true;

        while (startingUp)
        {
            fadeCanvasAnimator.Play("FadeIn");

            AudioManager.instance.PlayMusicFromList(songToLoad);

            yield return new WaitForSeconds(fadeCanvasAnimator.GetCurrentAnimatorStateInfo(0).length);

            SpawnPlayer();

            yield return new WaitForSeconds(2);

            Dialogue();

            break;
        }
    }

    private void SpawnPlayer()
    {
        GameObject playerRoot = Instantiate(PlayerDataManager.instance.playerPrefab, roomManager.TileFromWorldPosition(roomManager.playerSpawn), Quaternion.identity);

        GameObject playerObject = playerRoot.transform.GetChild(0).gameObject;

        playerObject.GetComponent<SpriteRenderer>().enabled = false;

        player = playerObject.GetComponent<PlayerController>();

        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.player = player;
        }
        if (NovelController.instance != null)
        {
            NovelController.instance.player = player;
        }

        PlayerDataManager.instance.LoadPlayerData(player);


        player.ProhibitInput();

        player.DropAnimation();
    }

    private void Dialogue()
    {
        StartCoroutine(DialogueOccurring());
    }

    public bool dialogueInPlay;
    IEnumerator DialogueOccurring()
    {

        if (dialogueToLoadIndex == 999)
        {

        }
        else
        {
            NovelController.instance.LoadChapterFile(NovelController.instance.textAssets[dialogueToLoadIndex].name);

            dialogueInPlay = true;

            while (dialogueInPlay)
            {
                yield return new WaitForEndOfFrame();
            }



        }

        ExitScene();


    }


    public void ExitScene()
    {
        StartCoroutine(SceneExit());
    }

    bool exitingScene;
    IEnumerator SceneExit()
    {
        exitingScene = true;

        while (exitingScene)
        {
            player.ProhibitInput();

            PlayerDataManager.instance.SavePlayerData(player.GetComponent<PlayerController>());

            fadeCanvasAnimator.Play("FadeOut");

            yield return new WaitForSeconds(fadeCanvasAnimator.GetCurrentAnimatorStateInfo(0).length);

            SceneManager.LoadScene("Credits");

            break;

        }
    }

}
