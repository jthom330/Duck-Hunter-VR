using UnityEngine;
using System.Collections;

public static class Globals {    

    public static bool canFireGun = true;
    public static float duckSpeed = 20f;
    public static float doubleDuckSpawnChance = .05f;
    public static int shotsLeft = 3;
    public static int round = 1;
    public static int ducksSpawnedThisRound = 0;
    public static int roundKills = 0;
    public static int roundKillRequirment = 6;
    public static int aliveDucks = 0;
    public static int score = 0;



    public static int IncreaseScore()
    {
        score += 1000 * (shotsLeft+1);

        return (1000 * (shotsLeft + 1));
    }

    private static float speedRoot = 1;
    public static void IncrementSpeed()
    {
        duckSpeed = (Mathf.Atan(speedRoot) / Mathf.PI) * 100;
        speedRoot += 0.2f;
    }

    public static void ResetShots()
    {
        shotsLeft = 3;
    }

    public static void UpdateKillRequirment()
    {
        if(round >= 20)
        {
            roundKillRequirment = 10;
        }
        else if(round >= 15)
        {
            roundKillRequirment = 9;
        }
        else if(round >= 13)
        {
            roundKillRequirment = 8;
        }
        else if(round >= 11)
        {
            roundKillRequirment = 7;
        }
    }

    public static void IncreaseDoubleSpawnRate()
    {
        doubleDuckSpawnChance += .01f;
    }

    public static void ResetAll()
    {
        canFireGun = true;
        duckSpeed = 20f;
        ducksSpawnedThisRound = 0;
        doubleDuckSpawnChance = .05f;
        shotsLeft = 3;
        round = 1;
        roundKills = 0;
        roundKillRequirment = 6;
        aliveDucks = 0;
        score = 0;
    }
}
