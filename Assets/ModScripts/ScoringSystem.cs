using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public static class ScoringSystem
{
    public static int[] baseScores(string serialNumber) => serialNumber.Select(x => ("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(x)) % 6 + 1).ToArray();

    public static int[] finalScores(int[] baseScore)
    {

        return baseScore;
    }
}
