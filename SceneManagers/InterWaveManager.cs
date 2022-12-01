using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InterWaveManager : MonoBehaviour
{
    public static InterWaveManager instance;

    public Animator fadeCanvasAnimator;
    private RoomManager roomManager;

    public List<GameObject> buffs = new List<GameObject>();
    private List<GameObject> buffsInPlay = new List<GameObject>();

    public int numOfBuffsToSpawn;
    
    private PlayerController player;

    [SerializeField]
    private string songToLoad;

    private int dialogueToLoadIndex;

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
        exitingInterWaveScene = false;

        CheckDialogue();
        StartCoroutine(InterWaveSceneStartUp());
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
    IEnumerator InterWaveSceneStartUp()
    {
        startingUp = true;

        while(startingUp)
        {
            SpawnPickups();

            yield return new WaitForSeconds(1);

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

        player.AllowInput();
    }

    private void SpawnPickups()
    {
        var roomManager = GameObject.FindGameObjectWithTag("RoomManager").GetComponent<RoomManager>();

        List<Vector2Int> buffPlacements = roomManager.GetBuffSpawnPositions();

        foreach (Vector2Int position in buffPlacements)
        {
            GameObject buff = Instantiate(buffs[Random.Range(0, buffs.Count - 1)], roomManager.TileFromWorldPosition(position), Quaternion.identity);
            buffsInPlay.Add(buff);
        }
    }

    public void RemoveBuffsFromField()
    {
        foreach(GameObject buff in buffsInPlay)
        {
            Destroy(buff);
        }
    }

    public void LoadGameplayScene()
    {
        StartCoroutine(InterWaveSceneExit());
    }

    bool exitingInterWaveScene;
    IEnumerator InterWaveSceneExit()
    {
        exitingInterWaveScene = true;

        while (exitingInterWaveScene)
        {
            player.ProhibitInput();

            PlayerDataManager.instance.SavePlayerData(player.GetComponent<PlayerController>());

            player.LiftAnimation();

            yield return new WaitForSeconds(2);
            
            fadeCanvasAnimator.Play("FadeOut");

            yield return new WaitForSeconds(fadeCanvasAnimator.GetCurrentAnimatorStateInfo(0).length);

            if (PlayerDataManager.instance.waveLevel >= 30)
            {
                SceneManager.LoadScene("WaveBlock2Scene");

            }
            else if (PlayerDataManager.instance.waveLevel >= 50)
            {
                SceneManager.LoadScene("FinalInterviewScene");
            }
            else
            {
                SceneManager.LoadScene("WaveBlock1Scene");

            }


            break;

        }
    }
}
