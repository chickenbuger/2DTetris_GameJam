using UnityEngine;
using UnityEngine.UI;

public class FadeInOut : MonoBehaviour
{
    public Image fadePanel;
    public float fadeDuration = 0.5f;

    private void Start()
    {
        // 시작 시 페이드 인
        FadeIn();
    }

    private void FadeIn()
    {
        fadePanel.canvasRenderer.SetAlpha(1.0f);
        fadePanel.CrossFadeAlpha(0.0f, fadeDuration, false);
    }

    private void FadeOut()
    {
        fadePanel.canvasRenderer.SetAlpha(0.0f);
        fadePanel.CrossFadeAlpha(1.0f, fadeDuration, false);
    }

    
}
