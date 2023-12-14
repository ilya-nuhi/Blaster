using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("PopSound")]
    [SerializeField] AudioClip popSoundFX;

    [Range(0.0f, 1.0f)]
    [SerializeField] float popSoundVolume = .5f;
    [Header("SuccessSound")]
    [SerializeField] AudioClip successSoundSFX;
    [Range(0.0f, 1.0f)]
    [SerializeField] float successSoundVolume = .5f;
    public void PopSound(){
        AudioSource.PlayClipAtPoint(popSoundFX, Camera.main.transform.position, popSoundVolume);
    }

    public void SuccessSound()
    {
        AudioSource.PlayClipAtPoint(successSoundSFX, Camera.main.transform.position, successSoundVolume);
    }
}
