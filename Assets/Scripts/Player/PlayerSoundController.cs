using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundController : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip soundRun;
    public AudioClip soundRomper;
    public AudioClip soundPut;
    public AudioClip soundDaño;
    public AudioClip soundDead;
    public AudioClip soundVictory;

    public void RunAnt()
    {
        if(!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(soundRun);
        }
    }

    public void DañoAnt()
    {
        audioSource.PlayOneShot(soundDaño);
    }

    public void DeadAnt()
    {
        audioSource.PlayOneShot(soundDead);
    }

    public void RomperBloque()
    {
        audioSource.PlayOneShot(soundRomper);
    }

    public void ColocarBloque()
    {
        audioSource.PlayOneShot(soundPut);
    }
    public void VictoryAnt()
    {
        audioSource.PlayOneShot(soundVictory);
    }

}
