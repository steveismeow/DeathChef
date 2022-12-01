using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class SplashSceneManager : MonoBehaviour
{
    public VideoPlayer splashVideo;

    public Animator fadeCanvasAnimator;

    private void Start()
    {
        StartCoroutine(SplashSequence());
    }

    IEnumerator SplashSequence()
    {
        fadeCanvasAnimator.Play("FadeIn");

        yield return new WaitForSeconds(fadeCanvasAnimator.GetCurrentAnimatorStateInfo(0).length);

        print("Pre-Load Complete, Starting Video");

        splashVideo.Play();
        AudioManager.instance.PlaySoundFromList("CrowAppear");

        yield return new WaitForSeconds((float)splashVideo.clip.length);

        print("Video complete, loading next scene");

        fadeCanvasAnimator.Play("FadeOut");

        yield return new WaitForSeconds(fadeCanvasAnimator.GetCurrentAnimatorStateInfo(0).length);

        SceneManager.LoadScene("MainMenuScene");
    }
}
