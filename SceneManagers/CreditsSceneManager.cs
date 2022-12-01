using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsSceneManager : MonoBehaviour
{
    public static CreditsSceneManager instance;

    private RoomManager roomManager;

    public Animator fadeCanvasAnimator;

    private PlayerController player;


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
        startingUp = false;
        exitingScene = false;

        StartCoroutine(SceneStartUp());
    }


    bool startingUp;
    IEnumerator SceneStartUp()
    {
        startingUp = true;

        while (startingUp)
        {
            fadeCanvasAnimator.Play("FadeIn");

            yield return new WaitForSeconds(fadeCanvasAnimator.GetCurrentAnimatorStateInfo(0).length);

            yield return new WaitForSeconds(2);

            break;
        }

        BackToMainMenu();
    }

    public void BackToMainMenu()
    {
        StartCoroutine(SceneExit());
    }

    bool exitingScene;
    IEnumerator SceneExit()
    {
        exitingScene = true;

        while (exitingScene)
        {

            fadeCanvasAnimator.Play("FadeOut");

            yield return new WaitForSeconds(fadeCanvasAnimator.GetCurrentAnimatorStateInfo(0).length);

            GameplayUI.instance.gameObject.SetActive(false);

            AudioManager.instance.PlayMusic(null);

            SceneManager.LoadScene("MainMenuScene");

            break;

        }
    }
}
