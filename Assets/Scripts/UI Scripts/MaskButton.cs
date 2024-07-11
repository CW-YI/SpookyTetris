using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaskButton : MonoBehaviour
{
    public RectTransform buttonTransform; // 버튼의 RectTransform
    public Image characterImage; // 자를 이미지

    void Start()
    {
        // 이미지의 부모를 버튼으로 설정하여 마스크 적용
        characterImage.transform.SetParent(buttonTransform, false);

        // 이미지 크기와 위치 조정
        //characterImage.rectTransform.anchoredPosition = Vector2.zero;
        //characterImage.rectTransform.sizeDelta = buttonTransform.sizeDelta;

        // 이미지가 버튼 영역 내에 들어가도록 설정
        Mask mask = buttonTransform.gameObject.AddComponent<Mask>();
        //mask.showMaskGraphic = false; // 마스크 이미지는 표시하지 않음
    }
}
