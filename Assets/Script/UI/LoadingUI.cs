using UnityEngine;
using TMPro;
using System.Collections;
using System.Text;

public class LoadingUI : MonoBehaviour
{
    public TextMeshProUGUI loadingText;
    private string baseText = "LOADING";
    private float typeDelay = 0.2f;  // 각 글자가 나타나는 간격
    private float dotDelay = 0.5f;   // 점(.)이 나타나는 간격
    private Coroutine loadingCoroutine;

    // 무지개 색상 배열 (밝은 파스텔톤)
    private Color32[] rainbowColors = new Color32[]
    {
        new Color32(255, 105, 105, 255),  // 밝은 빨강 (파스텔)
        new Color32(255, 175, 105, 255),  // 밝은 주황 (파스텔)
        new Color32(255, 255, 130, 255),  // 밝은 노랑 (파스텔)
        new Color32(130, 255, 130, 255),  // 밝은 초록 (파스텔)
        new Color32(130, 200, 255, 255),  // 밝은 파랑 (파스텔)
        new Color32(147, 162, 255, 255),  // 밝은 남색 (파스텔)
        new Color32(200, 140, 255, 255)   // 밝은 보라 (파스텔)
    };

    public void StartLoading()
    {
        gameObject.SetActive(true);
        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
        }
        loadingCoroutine = StartCoroutine(AnimateLoadingText());
    }

    public void StopLoading()
    {
        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
            loadingCoroutine = null;
        }
        gameObject.SetActive(false);
    }

    private string GetColoredText(string text)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < text.Length; i++)
        {
            Color32 color = rainbowColors[i % rainbowColors.Length];
            sb.Append($"<color=#{color.r:X2}{color.g:X2}{color.b:X2}>{text[i]}</color>");
        }
        return sb.ToString();
    }

    private IEnumerator AnimateLoadingText()
    {
        while (true)
        {
            // LOADING 글자를 하나씩 표시
            for (int i = 1; i <= baseText.Length; i++)
            {
                string partialText = baseText.Substring(0, i);
                loadingText.text = GetColoredText(partialText);
                yield return new WaitForSeconds(typeDelay);
            }

            // 점(...) 하나씩 추가
            string fullText = baseText;
            for (int i = 1; i <= 3; i++)
            {
                string dots = new string('.', i);
                string coloredDots = GetColoredText(dots);
                loadingText.text = GetColoredText(fullText) + coloredDots;
                yield return new WaitForSeconds(dotDelay);
            }

            // 잠시 대기 후 다시 시작
            yield return new WaitForSeconds(0.7f);
            loadingText.text = "";
            yield return new WaitForSeconds(0.3f);
        }
    }
}