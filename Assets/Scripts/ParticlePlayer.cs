using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

public class ParticlePlayer : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] allParticles;
    [SerializeField] private float lifetime = 1.0f;
    

    void Start()
    {
        allParticles = GetComponentsInChildren<ParticleSystem>();

        Destroy(gameObject, lifetime);
    }

    public void Play()
    {
        foreach (ParticleSystem particle in allParticles)
        {
            particle.Stop();
            particle.Play();
        }
    }
}
