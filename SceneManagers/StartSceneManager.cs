using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneManager : MonoBehaviour
{
    public static StartSceneManager instance;

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

        //if (!GameplayUI.instance.gameObject.activeInHierarchy)
        //{
        //    GameplayUI.instance.gameObject.SetActive(true);

        //}

    }

    // Start is called before the first frame update
    void Start()
    {
        fadeCanvasAnimator = GameObject.FindGameObjectWithTag("FadeCanvas").GetComponent<Animator>();
        roomManager = GameObject.FindGameObjectWithTag("RoomManager").GetComponent<RoomManager>();
        startingUp = false;
        exitingScene = false;

        GameplayUI.instance.UpdateWaveLevelText(PlayerDataManager.instance.waveLevel);

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
            print(dialogueToLoad);
            print("Music is playing");
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

        PlayerDataManager.instance.GetPreviousWeapon(player);

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
            print(NovelController.instance.textAssets[dialogueToLoadIndex].name);
            dialogueInPlay = true;

            while (dialogueInPlay)
            {
                yield return new WaitForEndOfFrame();
            }

        }

        player.AllowInput();
    }


    public void LoadGameplayScene()
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

            player.LiftAnimation();

            yield return new WaitForSeconds(2);

            fadeCanvasAnimator.Play("FadeOut");

            yield return new WaitForSeconds(fadeCanvasAnimator.GetCurrentAnimatorStateInfo(0).length);

            SceneManager.LoadScene("WaveBlock1Scene");

            break;

        }
    }
}
