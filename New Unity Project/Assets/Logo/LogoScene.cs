using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LogoScene : MonoBehaviour
{
    public Image fadePanel;
    public float fadeDuration = 0.5f;
    public float delayTime = 3;

    private IEnumerator Start()
    {
        fadePanel.gameObject.SetActive(false);

        yield return new WaitForSeconds(delayTime);
        fadePanel.gameObject.SetActive(true);
        FadeOut();

        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Main");
    }

    private void FadeOut()
    {
        fadePanel.canvasRenderer.SetAlpha(0.0f);
        fadePanel.CrossFadeAlpha(1.0f, fadeDuration, false);
    }
}
