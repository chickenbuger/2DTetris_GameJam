using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverSystem : MonoBehaviour
{
    public GameObject stage;
    public Image fadePanel;
    public float fadeDuration = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        Stage stageScript = FindObjectOfType<Stage>();
        if (stageScript != null)
        {
            // Score 값을 가져와서 문자열로 변환하여 설정
            stageScript.GameOverScoreText.text = stageScript.Score.ToString();
        }
        stage.SetActive(false);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Btn()
    {
        StartCoroutine(Home());
    }

    public IEnumerator Home()
    {
        fadePanel.gameObject.SetActive(true);
        FadeOut();

        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("Main");
    }

    private void FadeOut()
    {
        fadePanel.canvasRenderer.SetAlpha(0.0f);
        fadePanel.CrossFadeAlpha(1.0f, fadeDuration, false);
    }
}
