using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ImageChanger : MonoBehaviour
{
    public Sprite newSprite; // 새로운 이미지
    private Sprite originalSprite; // 원본 이미지
    private Image imageComponent;

    void Start()
    {
        imageComponent = GetComponent<Image>();
        originalSprite = imageComponent.sprite;

        // 1초마다 이미지 변경을 시작합니다.
        StartCoroutine(ChangeImageWithDelay());
    }

    IEnumerator ChangeImageWithDelay()
    {
        while (true)
        {
            // 1초 대기 후 새로운 이미지로 변경합니다.
            yield return new WaitForSeconds(0.1f);
            imageComponent.sprite = newSprite;

            // 1초 대기 후 원본 이미지로 되돌립니다.
            yield return new WaitForSeconds(0.1f);
            imageComponent.sprite = originalSprite;
        }
    }
}
