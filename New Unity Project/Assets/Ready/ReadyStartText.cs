using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReadyStartText : MonoBehaviour
{
    public Text textObject; // Unity Inspector���� �ؽ�Ʈ ������Ʈ�� �Ҵ��մϴ�.
    public GameObject stage;


    IEnumerator Start()
    {
        stage.gameObject.SetActive(false);

        yield return  AnimateText();

        stage.gameObject.SetActive(true);
    }

    IEnumerator AnimateText()
    {
        // Ready �ؽ�Ʈ
        yield return new WaitForSeconds(0.5f); // ���
        textObject.CrossFadeAlpha(1f, 0.4f, false); // ���̵���
        yield return new WaitForSeconds(0.5f); // ����
        textObject.CrossFadeAlpha(0f, 0.4f, false); // ���̵�ƿ�
        yield return new WaitForSeconds(0.3f); // ���

        // Start �ؽ�Ʈ
        textObject.text = "Start"; // �ؽ�Ʈ ����
        textObject.CrossFadeAlpha(1f, 0.4f, false); // ���̵���
        yield return new WaitForSeconds(0.5f); // ����
        textObject.CrossFadeAlpha(0f, 0.4f, false); // ���̵�ƿ�
        yield return new WaitForSeconds(1f); // ���
    }
}
