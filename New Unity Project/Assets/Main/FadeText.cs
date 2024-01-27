using UnityEngine;

public class FadeText : MonoBehaviour
{
    public float fadeDuration = 1.0f;
    private float timer = 0.0f;
    private CanvasGroup canvasGroup;
    private bool fadingOut = true;

    void Start()
    {
        // CanvasGroup 컴포넌트가 없다면 추가
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // 초기 투명도 설정
        canvasGroup.alpha = 1.0f;
    }

    void Update()
    {
        // 타이머 업데이트
        timer += Time.deltaTime;

        // 페이드 인/아웃 로직
        float normalizedTime = Mathf.Clamp01(timer / fadeDuration);
        canvasGroup.alpha = fadingOut ? 1.0f - normalizedTime : normalizedTime;

        // 3초가 지나면 타이머 초기화 및 페이드 방향 전환
        if (timer >= fadeDuration)
        {
            timer = 0.0f;
            fadingOut = !fadingOut;
        }
    }
}
