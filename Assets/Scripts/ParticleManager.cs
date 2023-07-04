using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    [SerializeField] private GameObject clearFXPrefab;
    [SerializeField] private GameObject breakFXPrefab;
    [SerializeField] private GameObject doubleBreakFXPrefab;

    public void ClearPieceFXAt(int xCoordinate, int yCoordinate, int zCoordinate = 0)
    {
        GameObject clearFX = Instantiate(clearFXPrefab, new Vector3(xCoordinate, yCoordinate, zCoordinate), Quaternion.identity);

        ParticlePlayer particlePlayer = clearFX.GetComponent<ParticlePlayer>();

        if (particlePlayer != null )
        {
            particlePlayer.Play();
        }
    }

    public void BreakTileFXAt(int breakableValue, int xCoordinate, int yCoordinate, int zCoordinate = 0)
    {
        GameObject breakFX = null;
        ParticlePlayer particlePlayer = null;

        if (breakableValue > 1)
        {
            if (doubleBreakFXPrefab != null)
            {
                breakFX = Instantiate(doubleBreakFXPrefab, new Vector3(xCoordinate, yCoordinate, zCoordinate), Quaternion.identity);
            }
            
        }
        else
        {
            if (breakFXPrefab != null)
            {
                breakFX = Instantiate(breakFXPrefab, new Vector3(xCoordinate, yCoordinate, zCoordinate), Quaternion.identity);
            }
        }

        if (breakFX != null)
        {
            particlePlayer = breakFX.GetComponent<ParticlePlayer>();

            if (particlePlayer != null)
            {
                particlePlayer.Play();
            }
        }
    }
}
