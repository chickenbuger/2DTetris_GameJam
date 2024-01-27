using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ImageChanger : MonoBehaviour
{
    public Sprite newSprite; // ���ο� �̹���
    private Sprite originalSprite; // ���� �̹���
    private Image imageComponent;

    void Start()
    {
        imageComponent = GetComponent<Image>();
        originalSprite = imageComponent.sprite;

        // 1�ʸ��� �̹��� ������ �����մϴ�.
        StartCoroutine(ChangeImageWithDelay());
    }

    IEnumerator ChangeImageWithDelay()
    {
        while (true)
        {
            // 1�� ��� �� ���ο� �̹����� �����մϴ�.
            yield return new WaitForSeconds(0.1f);
            imageComponent.sprite = newSprite;

            // 1�� ��� �� ���� �̹����� �ǵ����ϴ�.
            yield return new WaitForSeconds(0.1f);
            imageComponent.sprite = originalSprite;
        }
    }
}
