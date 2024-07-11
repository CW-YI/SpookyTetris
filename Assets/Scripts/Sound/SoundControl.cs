using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundControl : MonoBehaviour
{
    public Slider bgmSlider;
    public Slider sfxSlider;
    void Start()
    {
        if (!PlayerPrefs.HasKey("BGMSound")) PlayerPrefs.SetFloat("BGMSound", 0.5f);
        if (!PlayerPrefs.HasKey("SFXSound")) PlayerPrefs.SetFloat("SFXSound", 0.5f);

        // �����̴��� ���� PlayerPrefs���� �ҷ��� ������ �����մϴ�.
        bgmSlider.value = PlayerPrefs.GetFloat("BGMSound");
        sfxSlider.value = PlayerPrefs.GetFloat("SFXSound");

        // �����̴� �� ���� �� �̺�Ʈ ����
        bgmSlider.onValueChanged.AddListener(BGMSlider);
        sfxSlider.onValueChanged.AddListener(SFXSlider);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnEnable()
    {
        bgmSlider.value = PlayerPrefs.GetFloat("BGMSound");
        sfxSlider.value = PlayerPrefs.GetFloat("SFXSound");
    }

    public void BGMSlider(float value)
    {
        PlayerPrefs.SetFloat("BGMSound", value);
    }

    public void SFXSlider(float value2)
    {
        PlayerPrefs.SetFloat("SFXSound", value2);
    }
}
