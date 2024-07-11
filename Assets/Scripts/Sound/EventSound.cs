using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSound : MonoBehaviour
{
    public AudioClip sound;  // ��ư Ŭ�� ����
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayClickSound()
    {
        audioSource.PlayOneShot(sound);
    }
}
