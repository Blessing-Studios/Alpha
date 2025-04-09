using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blessing.Gameplay.Guild
{
    [Serializable]
    public struct Rank
    {
        public const int MaxStrike = 3;
        public const int MinStrike = 4;
        public const int MaxScore = 6;
        public int Score { get; private set; }
        public int Strike { get; private set; }
        private Dictionary<int, string> nameByScore;
        public Rank(int score = 0, int strikes = 0)
        {
            nameByScore = new Dictionary<int, string>()
            {
                {0, "F" },
                {1, "E"},
                {2, "D"},
                {3, "C"},
                {4, "B"},
                {5, "A"},
                {6, "S"},
            };
            Score = score;

            if (strikes >= -MinStrike && strikes <= MaxStrike)
                Strike = strikes;
            else
                Strike = 0;
        }
        public readonly string Name
        {
            get
            {
                if (Score >= 0 && Score <= 6)
                    return nameByScore[Score];
                else
                    return "F";
            }
        }
        public readonly string Label
        {
            get
            {
                string name = Name;

                if (Strike == MaxStrike)
                {
                    name += "+";
                }

                if (Strike == -MinStrike)
                {
                    name += "-";
                }

                return name;
            }
        }

        public void RankUp()
        {
            if (Score == MaxScore) return;

            Score++;
            ClearStrikes();
        }

        public void RankDown()
        {

        }

        public void AddStrikes(int strike)
        {
            if (strike <= 0) return;

            Strike += strike;
            if (Strike > MaxStrike) Strike = MaxStrike;
        }

        public void RemoveStrikes(int strike)
        {
            if (strike <= 0) return;

            Strike -= strike;
            if (Strike < -MinStrike) Strike = -MinStrike;
        }

        public void ClearStrikes()
        {
            Strike = 0;
        }

        // public void UpdateStrike(int strike)
        // {
        //     Strike += strike;

        //     if (Strike > MaxStrike) Strike = MaxStrike;
        //     if (Strike < -MinStrike) Strike = -MinStrike;
        // }
    }
}
