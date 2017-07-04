using UnityEngine;
using System.Collections.Generic;

namespace Amslot_SW
{
    public class snapShot
    {
        public string name;
        public double jackpot;
        public playResult play;
        public int dragonBulbs;
        public int tigerBulbs;
        public Wins wins;
    }

    public struct playResult
    {
        public float prize;
        public int bets;
        public int minimumBet;
        public List<int> matrix;
        public bool beginTimes;
        public bool endTimes;
        public int freeTimes;
        public float freePrize;
    }

    public struct Wins
    {
        public int big;
        public int super;
        public int mega;
    }

    public struct chiaJin
    {
        public double money;
    }

    public struct gameMode
    {
        public int game;
    }

    public struct playerBalance
    {
        public double money;
    }

    public class balls
    {
        public Tiger tiger;
        public Dragon dragon;
    }

    public struct Tiger
    {
        public int count;
        public bool mine;
    }

    public struct Dragon
    {
        public int count;
        public bool mine;
    }

    public struct machineInfo
    {
        public int id;
        public string name;
        public float sum;
        public int bonusTimes;
        public int bigReturns;
        public int status;
    }

    public struct getChiaJin
    {
        public float dragon;
        public float tiger;
    }

    public struct freeEndSum
    {
        public float prize;
        public int times;
    }
    
    public class myDragon
    {
        public bool mine;
        public int count;
    }
    public class myTiger
    {
        public bool mine;
        public int count;
    }


}