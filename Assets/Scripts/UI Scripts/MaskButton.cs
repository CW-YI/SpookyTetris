using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaskButton : MonoBehaviour
{
    public RectTransform buttonTransform; // ��ư�� RectTransform
    public Image characterImage; // �ڸ� �̹���

    void Start()
    {
        // �̹����� �θ� ��ư���� �����Ͽ� ����ũ ����
        characterImage.transform.SetParent(buttonTransform, false);

        // �̹��� ũ��� ��ġ ����
        //characterImage.rectTransform.anchoredPosition = Vector2.zero;
        //characterImage.rectTransform.sizeDelta = buttonTransform.sizeDelta;

        // �̹����� ��ư ���� ���� ������ ����
        Mask mask = buttonTransform.gameObject.AddComponent<Mask>();
        //mask.showMaskGraphic = false; // ����ũ �̹����� ǥ������ ����
    }
}
