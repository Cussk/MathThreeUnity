using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    [SerializeField] private int xIndex;
    [SerializeField] private int yIndex;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void SetCoordinates(int xCoordinate, int yCoordinate)
    {
        xIndex = xCoordinate;
        yIndex = yCoordinate;
    }
}
