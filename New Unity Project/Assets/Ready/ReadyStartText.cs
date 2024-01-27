using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReadyStartText : MonoBehaviour
{
    public Text textObject; // Unity Inspector에서 텍스트 오브젝트를 할당합니다.
    public GameObject stage;


    IEnumerator Start()
    {
        stage.gameObject.SetActive(false);

        yield return  AnimateText();

        stage.gameObject.SetActive(true);
    }

    IEnumerator AnimateText()
    {
        // Ready 텍스트
        yield return new WaitForSeconds(0.5f); // 대기
        textObject.CrossFadeAlpha(1f, 0.4f, false); // 페이드인
        yield return new WaitForSeconds(0.5f); // 고정
        textObject.CrossFadeAlpha(0f, 0.4f, false); // 페이드아웃
        yield return new WaitForSeconds(0.3f); // 대기

        // Start 텍스트
        textObject.text = "Start"; // 텍스트 변경
        textObject.CrossFadeAlpha(1f, 0.4f, false); // 페이드인
        yield return new WaitForSeconds(0.5f); // 고정
        textObject.CrossFadeAlpha(0f, 0.4f, false); // 페이드아웃
        yield return new WaitForSeconds(1f); // 대기
    }
}
