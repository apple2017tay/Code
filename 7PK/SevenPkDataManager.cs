using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SevenPkDataManager : MonoBehaviour {

    public static SevenPkDataManager Instance;

    //每次投注
    public int Bet = 200;
    //投注次數
    public int BetIndex = 0;

    public bool AutoPlay = false;

    public long Win = 0;

    public int BigWin = 5;
    public int SuperWin = 5;
    public int MegaWin = 5;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void OnDestroy()
    {
        if (Instance != null)
        {
            Instance = null;
        }
    }
}
