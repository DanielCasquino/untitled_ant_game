using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpiderSoundController : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip soundRun;
    public AudioClip soundAttack;

    public void RunSpider()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(soundRun);
        }
    }


}
