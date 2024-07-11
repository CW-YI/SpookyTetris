using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TextMove : MonoBehaviour
{
    #region move
    [SerializeField] private float amplitude = 5f;
    [SerializeField] private float frequency = 3f;
    private RectTransform rectTransform;
    private Vector2 originPosition;
    #endregion

    #region blink
    [SerializeField] private float blinkSpeed = 1.5f;
    private TextMeshProUGUI textMeshProUGUI;
    private Color originalColor;
    #endregion

    #region pulse
    [SerializeField] private float pulseDuration = 0.5f;
    [SerializeField] private float maxScaleFactor = 1.1f;
    private bool isPulsing = false;
    #endregion

    [SerializeField] private Image Fadeout;
    bool isStart = false;
    void Start()
    {
        //rectTransform = GetComponent<RectTransform>();
        //originPosition = rectTransform.anchoredPosition;

        textMeshProUGUI = GetComponent<TextMeshProUGUI>();
        originalColor = textMeshProUGUI.color;
    }

    void Update()
    {
        if (!isStart)
        {
            BlinkText();
            
            if (Input.GetKey(KeyCode.Return))
            {
                isStart = true;
                textMeshProUGUI.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
                StartCoroutine(PulseCoroutine());
            }
        }
    }

    void MoveScene()
    {
        SceneManager.LoadScene("OnlineLobby");
    }

    void MoveText()
    {
        float newY = originPosition.y + Mathf.Sin(Time.time * frequency) * amplitude;
        rectTransform.anchoredPosition = new Vector2(originPosition.x, newY);
    }

    void BlinkText()
    {
        float alpha = Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed));
        textMeshProUGUI.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
    }

    IEnumerator PulseCoroutine()
    {
        Vector3 originScale = textMeshProUGUI.rectTransform.localScale;
        Vector3 targetScale = originScale * maxScaleFactor;
        float elapsedTime = 0f;

        while (elapsedTime < pulseDuration / 2)
        {
            textMeshProUGUI.rectTransform.localScale = Vector3.Lerp(originScale, targetScale, elapsedTime / (pulseDuration / 2));
            elapsedTime += Time.deltaTime * 4f;
            yield return null;
        }

        elapsedTime = 0f;

        while (elapsedTime < pulseDuration / 2)
        {
            textMeshProUGUI.rectTransform.localScale = Vector3.Lerp(targetScale, originScale, elapsedTime / (pulseDuration / 2));
            elapsedTime += Time.deltaTime * 4f;
            yield return null;
        }

        elapsedTime = 0f;

        StartCoroutine(FadeOutCorutine());
    }

    IEnumerator FadeOutCorutine()
    {
        Color startColor = Fadeout.color;
        float duration = 100f;
        float startAlpha = startColor.a;
        float alphaDecreaseRate = startAlpha / duration;
        while (Fadeout.color.a > 0)
        {
            float newAlpha = Fadeout.color.a - (alphaDecreaseRate * Time.deltaTime); // 알파 값을 감소시킵니다.
            Fadeout.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);
            yield return null;
        }
        Fadeout.color = new Color(startColor.r, startColor.g, 0f);
        //Invoke(nameof(MoveScene), 1f);
        MoveScene();
    }
}
