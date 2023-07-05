using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : GamePiece
{
    public bool clearedByBomb;
    public bool clearedAtBottom;

    void Start()
    {
        //cannot match any game pieces or bombs with collectibles
        matchValue = MatchValue.None;
    }

    void Update()
    {
        
    }
}
