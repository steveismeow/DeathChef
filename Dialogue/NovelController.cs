using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


/// <summary>
/// Manages the txt file and interprets actions and inputs
/// </summary>
public class NovelController : MonoBehaviour
{
    public static NovelController instance;

    List<string> data = new List<string>();

    public List<TextAsset> textAssets = new List<TextAsset>();

    public PlayerController player;

    public string clickSFXName;

    public bool buildSettings;


    void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        }
        else
        {
            return;
        }
    }


    string activeChapterFile = "";


    // Update is called once per frame
    void Update()
    {
        if (player != null && DialogueManager.instance.isSpeaking)
        {
            if (player.Input.SelectInput || player.Input.AttackInput)
            {
                Next();
                player.Input.UseAttackInput();
                player.Input.UseSelectInput();
            }
        }
    }

    public void LoadChapterFile(string file)
    {
        activeChapterFile = file;

        if (buildSettings)
        {
            data = FileManager.LoadFileInBuild("Resources/Story/" + file);
        }
        else
        {
            data = FileManager.LoadFile("Resources/Story/" + file);
        }

        cachedLastSpeaker = "";

        if (handlingChapterFile != null)
        {
            StopCoroutine(handlingChapterFile);
        }

        handlingChapterFile = StartCoroutine(HandlingChapterFile());

        Next();
    }

    //public void LoadChapterFile(string fileName)
    //{
    //    activeChapterFile = fileName;

    //    if (buildSettings)
    //    {
    //        data = FileManager.LoadFileInBuild("Resources/Story/" + fileName);
    //    }
    //    else
    //    {
    //        data = FileManager.LoadFile("Resources/Story/" + fileName);
    //    }

    //    cachedLastSpeaker = "";

    //    if (handlingChapterFile != null)
    //    {
    //        StopCoroutine(handlingChapterFile);
    //    }

    //    handlingChapterFile = StartCoroutine(HandlingChapterFile());

    //    Next();
    //}

    bool _next = false;
    public void Next()
    {
        _next = true;
    }

    public bool isHandlingChapterFile { get { return handlingChapterFile != null; } }
    Coroutine handlingChapterFile = null;
    private int chapterProgress = 0;
    IEnumerator HandlingChapterFile()
    {
        print("Handling text file!");
        chapterProgress = 0;

        while (chapterProgress < data.Count)
        {
            if (_next)
            {
                string line = data[chapterProgress];

                //if (line.StartsWith("choice"))
                //{
                //    yield return HandlingChoiceLine(line);
                //    chapterProgress++;
                //}
                //else
                //{
                    HandleLine(data[chapterProgress]);
                    chapterProgress++;
                    while (isHandlingLine)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                //}

            }

            yield return new WaitForEndOfFrame();

        }

        handlingChapterFile = null;
    }
    //IEnumerator HandlingChoiceLine(string line)
    //{
    //    string title = line.Split('"')[1];
    //    List<string> choices = new List<string>();
    //    List<string> actions = new List<string>();
    //    print("Handle Choice line:" + title);

    //    while (true)
    //    {
    //        chapterProgress++;
    //        line = data[chapterProgress];
    //        if (line == "{")
    //        {
    //            continue;
    //        }

    //        if (line != "}")
    //        {
    //            choices.Add(line.Split('"')[1]);
    //            actions.Add(data[chapterProgress + 1]);
    //            chapterProgress++;

    //        }
    //        else
    //        {
    //            break;
    //        }

    //    }

    //    if (choices.Count > 0)
    //    {
    //        ChoiceScreen.Show(title, choices.ToArray());
    //        yield return new WaitForEndOfFrame();

    //        while (ChoiceScreen.isWaitingForChoiceToBeMade)
    //        {
    //            yield return new WaitForEndOfFrame();
    //        }

    //        string action = actions[ChoiceScreen.lastChoiceMade.index];
    //        HandleLine(action);

    //        while (isHandlingLine)
    //        {
    //            yield return new WaitForEndOfFrame();
    //        }
    //    }
    //    else
    //    {
    //        Debug.Log("List[Choices] is empty. No choices were found");
    //    }

    //}

    void HandleLine(string rawLine)
    {
        ChapterLineManager.Line line = ChapterLineManager.Interpret(rawLine);

        StopHandlingLine();
        handlingLine = StartCoroutine(HandlingLine(line));
    }

    void StopHandlingLine()
    {
        if (isHandlingLine)
        {
            StopCoroutine(handlingLine);
        }
        handlingLine = null;
    }

    public string cachedLastSpeaker = "";

    public bool isHandlingLine { get { return handlingLine != null; } }
    Coroutine handlingLine = null;
    IEnumerator HandlingLine(ChapterLineManager.Line line)
    {
        _next = false;
        int lineProgress = 0;

        while (lineProgress < line.segments.Count)
        {
            _next = false;
            ChapterLineManager.Line.Segment segment = line.segments[lineProgress];

            if (lineProgress > 0)
            {
                if (segment.trigger == ChapterLineManager.Line.Segment.Trigger.autoDelay)
                {
                    for (float timer = segment.autoDelay; timer >= 0; timer -= Time.deltaTime)
                    {
                        yield return new WaitForEndOfFrame();
                        if (_next)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    while (!_next)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                }
            }
            _next = false;

            segment.Run();

            while (segment.isRunning)
            {
                yield return new WaitForEndOfFrame();

                if (_next)
                {
                    if (!segment.architect.skip)
                    {
                        segment.architect.skip = true;
                    }
                    else
                    {
                        segment.ForceFinish();
                    }

                    _next = false;
                }
            }

            lineProgress++;

            yield return new WaitForEndOfFrame();
        }

        for (int i = 0; i < line.actions.Count; i++)
        {
            HandleAction(line.actions[i]);
        }
        handlingLine = null;
    }

    //ACTIONS
    //////////////////////////////////////////////////////////////////////
    public void HandleAction(string action)
    {
        //print("Handle action [" + action + "]");
        string[] data = action.Split('(', ')');
        switch (data[0])
        {
            case "wait":
                Command_Wait(data[1]);
                break;
            //case "setBackground":
            //    Command_SetLayerImage(data[1], LayerController.instance.background);
            //    break;
            //case "setForeground":
            //    Command_SetLayerImage(data[1], LayerController.instance.foreground);
            //    break;
            //case "setCinematic":
            //    //Command_SetLayerImage(data[1], LayerController.instance.cinematic);
            //    Command_SetVideo(data[1], LayerController.instance.cinematic);
            //    break;
            //case "clearFG":
            //    Command_ClearFGLayer(data[1], LayerController.instance.foreground);
            //    break;
            case "Reset":
                Command_OpenResetMenu();
                break;
            case "playAnim":
                Command_PlayerAnimation(data[1]);
                break;
            case "loadScene":
                Command_LoadScene(data[1]);
                break;
            case "changeFacing":
                Command_ChangeFacing(data[1]);
                break;
            case "playMusic":
                Command_PlayMusic(data[1]);
                break;
            case "playSound":
                Command_PlaySound(data[1]);
                break;
            case "Load":
                Command_Load(data[1]);
                break;
            case "next":
                Next();
                break;
            case "exitDialogue":
                ExitDialogue();
                break;

        }
    }


    void Command_Wait(string data)
    {
        float waitTime = float.Parse(data);
        Coroutine waiting = StartCoroutine(Waiting(waitTime));
    }

    IEnumerator Waiting(float waitTIme)
    {
        DialogueManager.instance.Close();

        yield return new WaitForSeconds(waitTIme);

        Next();
    }

    void ExitDialogue()
    {
        DialogueManager.instance.Close();
    }

    void Command_Load(string chapterName)
    {
        //DialogueManager.instance.Close();
        NovelController.instance.LoadChapterFile(chapterName);

    }

    void Command_ChangeFacing(string facing)
    {
        switch (facing)
        {
            case "SE":
                player.facing = new Vector2Int(1, -1);
                break;
            case "SW":
                player.facing = new Vector2Int(-1, -1);
                break;
            case "NW":
                player.facing = new Vector2Int(-1, 1);
                break;
            case "NE":
                player.facing = new Vector2Int(1, 1);
                break;
        }
    }

    void Command_PlayerAnimation(string animation)
    {
        player.anim.Play(animation);
    }

    void Command_LoadScene(string sceneToLoad)
    {

    }

    void Command_OpenResetMenu()
    {

    }

    //void Command_SetVideo(string data, LayerController.Layer layer)
    //{
    //    string videoName = data.Contains(",") ? data.Split(',')[0] : data;
    //    VideoClip video = videoName == "null" ? null : Resources.Load("Video/" + videoName) as VideoClip;

    //    //May want to come back here and edit in the ability to modify transition speed
    //    layer.SetVideo(video, 1, false);

    //}

    //void Command_SetLayerImage(string data, LayerController.Layer layer)
    //{
    //    string textureName = data.Contains(",") ? data.Split(',')[0] : data;
    //    Texture2D texture = textureName == "null" ? null : Resources.Load("Images/" + textureName) as Texture2D;
    //    float speed = 2f;
    //    bool smooth = true;

    //    //CURRENT CHECK IS ONLY FOR TRANSITION SPEED, ADDITIONAL BOOLS AND VARIABLES MAY BE INTEGRATED AS NECESSARY
    //    if (data.Contains(","))
    //    {
    //        //PARSING MECHANISM SHOWN BELOW, HOLD ONTO IN CASE OF EMERGENCY
    //        string[] parameters = data.Split(',');
    //        foreach (string p in parameters)
    //        {
    //            float fVal = 0;
    //            bool bVal = true;

    //            if (float.TryParse(p, out fVal))
    //            {
    //                speed = fVal;
    //                continue;
    //            }
    //            if (bool.TryParse(p, out bVal))
    //            {
    //                smooth = bVal;
    //                continue;
    //            }
    //        }
    //    }
    //    layer.SetTexture(texture, speed);
    //}

    //void Command_ClearFGLayer(string data, LayerController.Layer layer)
    //{
    //    float transitionSpeed = float.Parse(data);
    //    layer.ClearLayer(transitionSpeed);
    //}

    void Command_PlayMusic(string songToLoad)
    {
        AudioManager.instance.PlayMusicFromList(songToLoad);
    }

    void Command_PlaySound(string data)
    {
        AudioManager.instance.PlaySoundFromList(data);
    }



}
