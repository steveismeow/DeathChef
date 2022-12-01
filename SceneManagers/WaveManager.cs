using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Pathfinding;

public class WaveManager : MonoBehaviour
{

    public int waveLevel;
    public int waveInterval;
    public int enemySpawnRate;
    public float enemyDensity;
    public bool bypassWaveStartUp;

    private int enemiesToSpawnThisWave;
    private int dialogueToLoadIndex;

    [SerializeField]
    private string songToLoad;


    [SerializeField]
    private GameObject waveUICanvas;
    [SerializeField]
    private TextMeshProUGUI waveLevelUI;

    public static WaveManager instance;
    public PlayerController player;
    public ObstacleManager obstacleManager;
    private RoomManager roomManager;
    private GameplayUI gameplayUI;

    [SerializeField]
    private AnimationCurve enemySpawnNumberCurve;
    [SerializeField]
    private AnimationCurve enemySpawnTierCurve;
    [SerializeField]
    private float enemyTierThresholdRange;
    
    public List<GameObject> Tier1 = new List<GameObject>();
    public List<GameObject> Tier2 = new List<GameObject>();
    public List<GameObject> Tier3 = new List<GameObject>();
    public List<GameObject> Tier4 = new List<GameObject>();


    public List<Transform> EnemySpawnPoints = new List<Transform>();
    public List<GameObject> EnemySpawnQueue = new List<GameObject>();
    public List<Enemy> EnemiesInPlay = new List<Enemy>();


    private void Awake()
    {
        waveUICanvas = transform.GetChild(0).gameObject;
        roomManager = GameObject.FindGameObjectWithTag("RoomManager").GetComponent<RoomManager>();
        obstacleManager = GameObject.FindGameObjectWithTag("ObstacleManager").GetComponent<ObstacleManager>();
        gameplayUI = GameObject.FindGameObjectWithTag("GameplayUI").GetComponent<GameplayUI>();

        spawningEnemies = false;
        dialogueInPlay = false;
        isReturningToStart = false;

        if (instance == null)
        {
            instance = this;
        }

    }

    private void Start()
    {

        waveLevel = PlayerDataManager.instance.GetPlayerWaveLevel();

        CheckDialogue();
        NewWaveSet();

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

    private void NewWaveSet()
    {
        StartCoroutine(WaveSceneStartUp());
    }

    bool startingUp;
    IEnumerator WaveSceneStartUp()
    {
        startingUp = true;

        while (startingUp)
        {
            WaveSetConfiguration();

            yield return new WaitForSeconds(1);
            
            roomManager.fadeCanvasAnimator.Play("FadeIn");

            yield return new WaitForSeconds(roomManager.fadeCanvasAnimator.GetCurrentAnimatorStateInfo(0).length);
            
            SpawnPlayer();

            yield return new WaitForSeconds(2);

            if (!bypassWaveStartUp)
            {
                Dialogue();
            }
            else
            {
                StartWave();
            }

            startingUp = false;

            break;
        
        }
    }

    private void SpawnPlayer()
    {
        //spawn player
        if (!GameObject.FindGameObjectWithTag("Player"))
        {
            print("Had to spawn Player in");
            GameObject playerObject = Instantiate(PlayerDataManager.instance.playerPrefab, roomManager.TileFromWorldPosition(roomManager.playerSpawn), Quaternion.identity);
            player = playerObject.transform.GetChild(0).gameObject.GetComponent<PlayerController>();
            player.gameObject.GetComponent<SpriteRenderer>().enabled = false;
        }
        else
        {
            print("Found player");
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
            player.transform.position = roomManager.TileFromWorldPosition(roomManager.playerSpawn);
            player.gameObject.GetComponent<SpriteRenderer>().enabled = false;
        }

        PlayerDataManager.instance.LoadPlayerData(player);

        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.player = player;
        }
        if (NovelController.instance != null)
        {
            NovelController.instance.player = player;
        }
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

        StartWave();

    }

    private void NewWave()
    {
        WaveConfiguration();

        StartWave();
    }

    public void StartWave()
    {
        waveStart = StartCoroutine(WaveStartProcess());
    }

    bool startingWave = false;
    Coroutine waveStart = null;
    IEnumerator WaveStartProcess()
    {
        startingWave = true;

        while(startingWave)
        {
            if (!bypassWaveStartUp)
            {
                AudioManager.instance.PlayMusicFromList(songToLoad);

                yield return new WaitForSeconds(1);

                WaveMessage();

                yield return new WaitForSeconds(waveUICanvas.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);

                //Spawn Enemies
                SpawnEnemies();
            }

            player.AllowInput();

            break;
        }
    }

    public void WaveMessage()
    {
        //Wave UI/Start sequence
        waveLevelUI.text = waveLevel.ToString();
        waveUICanvas.GetComponent<Animator>().Play("WaveStart");
        gameplayUI.UpdateWaveLevelText(waveLevel);
    }

    //Every 5 waves
    public void WaveSetConfiguration()
    {
        //Configure the room
        roomManager.ConfigureRoom();

        obstacleManager.PathfindingFullGraphUpdate();


        if (!bypassWaveStartUp)
        {
            DetermineEnemyQueue();
        }
    }

    //Every wave besides multiples of 5
    public void WaveConfiguration()
    {
        DetermineEnemyQueue();
    }

    private void DetermineEnemyQueue()
    {
        //Calculations need to do 2 things:
        //1) Determine the number of enemies to spawn
        //2) Determine from which lists to pull the enemies from
        enemiesToSpawnThisWave = 0;

        float difficultyModifier = enemySpawnNumberCurve.Evaluate((float)waveLevel);

        enemiesToSpawnThisWave = Mathf.RoundToInt(difficultyModifier*50);

        for(int i = 0; i < enemiesToSpawnThisWave; i++)
        {
            //based on the wave level, determine a percentage chance for each enemy type to appear
            float index = 1 - enemySpawnTierCurve.Evaluate((float)waveLevel);


            float fixedIndex = Random.Range(index - enemyTierThresholdRange, 1);

            if (fixedIndex < 0)
            {
                fixedIndex = 0;
            }
            else if (fixedIndex > 1)
            {
                fixedIndex = 1;
            }

            if(1 >= fixedIndex && fixedIndex > 0.75f)
            {
                var enemy = Tier1[Random.Range(0, Tier1.Count-1)];
                EnemySpawnQueue.Add(enemy);
            }
            else if(0.75f >= fixedIndex && fixedIndex > 0.50f)
            {
                var enemy = Tier2[Random.Range(0, Tier2.Count-1)];
                EnemySpawnQueue.Add(enemy);
            }
            else if(0.50f >= fixedIndex && fixedIndex > 0.25f)
            {
                var enemy = Tier3[Random.Range(0, Tier1.Count-1)];
                EnemySpawnQueue.Add(enemy);
            }
            else if(0.25f >= fixedIndex && fixedIndex >= 0)
            {
                var enemy = Tier4[Random.Range(0, Tier1.Count-1)];
                EnemySpawnQueue.Add(enemy);
            }

        }
    }

    public void SpawnEnemies()
    {
        //Spawn enemies
        spawnEnemies = StartCoroutine(SpawningEnemies());
    }

    bool spawningEnemies;
    Coroutine spawnEnemies = null;
    IEnumerator SpawningEnemies()
    {
        spawningEnemies = true;

        print("Enemies to spawn:" + EnemySpawnQueue.Count);

        while (spawningEnemies)
        {
            //reference enemy spawn points, randomly select one, and wait a wave-based value to spawn again
            foreach (Transform spawnPoint in EnemySpawnPoints)
            {
                if (EnemyQueueComplete())
                {
                    spawningEnemies = false;
                    break;
                }

                EnemyEntersChamber(spawnPoint);

                float spawnWaitTime = enemySpawnRate;//Random.Range(0, enemySpawnRate);

                yield return new WaitForSeconds(spawnWaitTime);
            }


            //if all enemies for the wave have been spawned, stop
        }
    }

    private bool EnemyQueueComplete()
    {
        if (EnemySpawnQueue.Count == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void EnemyEntersChamber(Transform spawnPoint)
    {
        bool foundPreviouslyInstantiatedEnemy = false;

        foreach(Transform child in transform)
        {
            if (child.gameObject.name == EnemySpawnQueue[0].name && !child.gameObject.activeInHierarchy)
            {
                child.gameObject.transform.position = spawnPoint.position;
                child.gameObject.SetActive(true);

                obstacleManager.AssignPlayerToEnemies(child.gameObject.GetComponent<AIDestinationSetter>(), player.transform);
                EnemiesInPlay.Add(child.gameObject.GetComponent<Enemy>());
                EnemySpawnQueue.RemoveAt(0);

                foundPreviouslyInstantiatedEnemy = true;
                break;
            }
            else
            {
                continue;
            }
        }

        if (foundPreviouslyInstantiatedEnemy)
        {
            return;
        }
        else
        {
            GameObject enemy = Instantiate(EnemySpawnQueue[0], spawnPoint.position, Quaternion.identity, this.transform);
            obstacleManager.AssignPlayerToEnemies(enemy.GetComponent<AIDestinationSetter>(), player.transform);
            EnemiesInPlay.Add(enemy.GetComponent<Enemy>());
            EnemySpawnQueue.RemoveAt(0);
        }
    }

    private void EndWaveSet()
    {
        Debug.Log("EndWaveSet() called");
        waveLevel++;

        //clear all references, grab player data, and send our boi to the inbetween scene for buffing
        EnemySpawnQueue.Clear();

        endingWaveSet = false;

        //DIALOGUE CHECK
        //change track here as well

        StartCoroutine(EndingWaveSet());
    }

    bool endingWaveSet;
    IEnumerator EndingWaveSet()
    {
        endingWaveSet = true;
        timeIsSlowed = false;
        shakingScreen = false;

        while(endingWaveSet)
        {
            if (!timeIsSlowed)
            {
                StartCoroutine(SlowTime());
            }
            if (!shakingScreen)
            {
                StartCoroutine(ScreenShake(1));
            }

            yield return new WaitForSeconds(1);


            player.LiftAnimation();

            PlayerDataManager.instance.SavePlayerData(player);

            AudioManager.instance.PlayMusic(null);
            
            yield return new WaitForSeconds(2);

            roomManager.fadeCanvasAnimator.Play("FadeOut");

            yield return new WaitForSeconds(roomManager.fadeCanvasAnimator.GetCurrentAnimatorStateInfo(0).length);
            
            if (PlayerDataManager.instance.waveLevel >= 51)
            {
                SceneManager.LoadScene("FinalInterviewScene");

            }
            else
            {
                SceneManager.LoadScene("InterWaveScene");
            }

        }
    }

    bool timeIsSlowed;
    IEnumerator SlowTime()
    {
        timeIsSlowed = true;

        float t = 0;
        print("Bullet time!");

        while (t < 0.75f)
        {
            t += Time.unscaledDeltaTime / 2;
            Time.timeScale = Mathf.Lerp(1, 0.2f, t);
            yield return new WaitForEndOfFrame();
        }

        StartCoroutine(SpeedTimeBackUp());

    }

    IEnumerator SpeedTimeBackUp()
    {
        float t = 0;

        while (t < 0.75f)
        {
            t += Time.unscaledDeltaTime / 2;
            Time.timeScale = Mathf.Lerp(0.3f, 1, t);
            yield return new WaitForEndOfFrame();
        }

        timeIsSlowed = false;
        print("Time should be back to normal");
    }

    bool shakingScreen;
    IEnumerator ScreenShake(float time)
    {
        shakingScreen = true;

        float timer = 0f;
        float magnitude = 0.5f;

        Vector3 cameraStartPosition = new Vector3(0,0, -10);

        while (timer < time)
        {
            float xOffset = Random.Range(-0.05f, 0.5f) * magnitude;
            float yOffset = Random.Range(-0.05f, 0.5f) * magnitude;

            Camera.main.transform.localPosition = new Vector3(xOffset, yOffset, -10);
            
            timer += Time.unscaledDeltaTime;


            yield return new WaitForEndOfFrame();
        }

        shakingScreen = false;
        Camera.main.transform.localPosition = cameraStartPosition;
        print(cameraStartPosition);
        print(Camera.main.transform.localPosition);
    }
    

    private void EndWave()
    {
        Debug.Log("EndWave() called");
        waveLevel++;

        //Clear necessary references?

        //Clear enemies
        EnemySpawnQueue.Clear();

        //Shuffle doors??


        NewWave();

    }

    public void CheckWinState()
    {
        if (EnemiesInPlay.Count <= 0)
        {
            if (waveLevel % waveInterval == 0)
            {
                if (waveLevel == 30)
                {
                    PlayerDataManager.instance.beatWave30 = true;
                }
                else if (waveLevel == 51)
                {
                    PlayerDataManager.instance.beatWave51 = true;
                }
                EndWaveSet();
            }
            else
            {
                EndWave();
            }
        }
    }

    public void ReturnToStartScene()
    {
        //PlayerDataManager.instance.ClearPlayerRunData();

        StartCoroutine(ReturningToStart());
    }

    bool isReturningToStart;
    IEnumerator ReturningToStart()
    {
        isReturningToStart = true;

        PlayerDataManager.instance.runNumber++;

        while(isReturningToStart)
        {
            player.LiftAnimation();

            AudioManager.instance.PlayMusic(null);

            yield return new WaitForSeconds(2);

            roomManager.fadeCanvasAnimator.Play("FadeOut");

            yield return new WaitForSeconds(roomManager.fadeCanvasAnimator.GetCurrentAnimatorStateInfo(0).length);

            PlayerDataManager.instance.ResetPlayerStats(player);

            waveLevel = 1;
            gameplayUI.UpdateWaveLevelText(waveLevel);

            SceneManager.LoadScene("StartScene");

        }
    }

}
