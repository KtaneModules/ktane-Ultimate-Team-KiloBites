using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public static class ScoringSystem
{
    public static int[] baseScores(string serialNumber) => serialNumber.Select(x => ("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(x)) % 6 + 1).ToArray();

    public static int[] finalScores(string[] difficulty, int[] baseScore)
    {

        for (int i = 0; i < 6; i++)
        {
            var modifier = 0;
            switch (difficulty[i])
            {
                case "Easy":
                    modifier = 1;
                    break;
                case "Medium":
                    modifier = 2;
                    break;
                case "Hard":
                    modifier = 3;
                    break;
                case "VeryHard":
                    modifier = 4;
                    break;
            }

            baseScore[i] += modifier;
        }

        return baseScore;
    }
}
