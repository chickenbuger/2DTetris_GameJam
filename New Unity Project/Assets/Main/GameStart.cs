using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStart : MonoBehaviour
{
    public Image fadePanel;
    public float fadeDuration = 0.5f;
    void Update()
    {
        // 아무 키나 눌렸을 때
        if (Input.anyKeyDown)
        {
            StartCoroutine(Next());
        }
    }

    private IEnumerator Next()
    {
        fadePanel.gameObject.SetActive(true);
        FadeOut();

        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("SampleScene");
    }

    private void FadeOut()
    {
        fadePanel.canvasRenderer.SetAlpha(0.0f);
        fadePanel.CrossFadeAlpha(1.0f, fadeDuration, false);
    }
}
