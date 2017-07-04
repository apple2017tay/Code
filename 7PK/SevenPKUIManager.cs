using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NGUI_ExtendByLoger;
using Spine.Unity;

public class SevenPKUIManager : MonoBehaviour {

    #region 變數

    public static SevenPKUIManager Instance;

    public NGUILabelNumberAnim jackpotLabel;
    public NGUILabelNumberAnim balanceLabel;
    public NGUILabelNumberAnim winLabel;
    public UILabel betLabel;

    [Space(10)]
    public GameObject[] SilverLight;
    public GameObject[] GoldenLight;
    public GameObject[] JokeLight;

    private object locker = new object();

    #endregion

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

    #region 按鈕

    bool canClickBet = true;
    public void Btn_Bet()
    {
        if (SevenPkDataManager.Instance.AutoPlay)
        {
            CancelInvoke("Do_Play");
            SevenPkDataManager.Instance.AutoPlay = false;
        }

        if (!canClickBet) return;
        canClickBet = false;

        Do_Play();
    }

    bool canClickAll = false;
    public void Btn_All()
    {
        if (!canClickAll) return;
        canClickAll = false;
        canClickBet = false;

        SevenPkDataManager.Instance.BetIndex = 4;

        Do_TakeAllCard();
    }

    public void Btn_Auto()
    {
        SevenPkDataManager.Instance.AutoPlay = true;
        if (!canClickBet) return;
        canClickBet = false;
        Do_Play();
    }

    bool canTake = false;
    public void Btn_TakeScore()
    {
        if (!canTake) return;
        canTake = false;

        CancelInvoke("Btn_TakeScore");
        SevenPKCards.Instance.Init_Cards();
        Do_BigWin();
    }

    bool canGuess = false;
    public void Btn_Guess()
    {
        if (!canGuess) return;
        canGuess = false;

        send_Guess();
    }

    #endregion

    #region 功能

    //遊玩
    public void Do_Play()
    {
        send_PlayOne();
    }

    //單開
    public void Do_TakeCard(int BetIndex)
    {
        switch (BetIndex)
        {
            case 1:
                StartCoroutine("Delay_Bet1");
                break;
            case 2:
                StartCoroutine("Delay_Bet2");
                break;
            case 3:
                StartCoroutine("Delay_Bet3");
                break;
            case 4:
                StartCoroutine("Delay_Bet4");
                break;
        }
    }

    //全開
    public void Do_TakeAllCard()
    {
        send_PlayAll();
    }

    //是否中獎
    private void Do_IsWin()
    {
        //沒有中獎就直接開啟下一輪
        if (SevenPkDataManager.Instance.Win <= 0)
        {
            Invoke("Do_NextPlay", 1.0f);
            return;
        }

        Do_GoldenJackpot();
    }

    //有沒有金JP亮燈或中獎
    private void Do_GoldenJackpot()
    {
        //是否亮燈

        //亮燈完是否中獎

        Do_SilverJackpot();
    }

    //拿金彩金
    private void Do_TakeGoldenJackpot(long balance)
    {
        //拿彩金
    }

    //有沒有銀JP亮燈或中獎
    private void Do_SilverJackpot()
    {
        //是否亮燈

        //亮燈完是否中獎

        Do_JokerTurn();
    }

    //拿銀彩金
    private void Do_TakeSilverJackpot(long balance)
    {
        //拿彩金
    }

    //有沒有鬼牌亮燈
    private void Do_JokerTurn()
    {
        //是否亮燈

        //亮燈是否進入轉盤
        if (JokeLight[2].activeSelf) SevenPKTruntable.Instance.Show();
        else Do_TakeOrGuess();
    }

    //開啟比倍
    public void Do_TakeOrGuess()
    {
        //開啟領取按鈕
        canTake = true;

        //非自動下開啟比倍功能
        if (SevenPkDataManager.Instance.AutoPlay)
        {
            canGuess = false;
            Invoke("Btn_TakeScore", 1.0f);
        }
        else
        {
            canGuess = true;
        }
    }

    //大獎
    private void Do_BigWin()
    {
        if (SevenPkDataManager.Instance.Win >= (SevenPkDataManager.Instance.Bet * SevenPkDataManager.Instance.BigWin))
        {
            SevenPKBigWin.Instance.Show();
            SevenPKBigWin.Instance.Result = SevenPkDataManager.Instance.Win;
        }

        else
        {
            UI_Win(SevenPkDataManager.Instance.Win);
            Invoke("Do_NextPlay", 1.5f);
        }
    }

    //可進行下一輪
    public void Do_NextPlay()
    {
        if (SevenPkDataManager.Instance.AutoPlay)
        {
            canClickAll = false;
            canClickBet = false;

            Do_Play();
        }
        else
        {
            canClickBet = true;
        }
    } 

    #endregion

    #region 送出訊息

    void send_PlayOne()
    {
        lock (locker)
        {
            SevenPkDataManager.Instance.BetIndex += 1;
            if (SevenPkDataManager.Instance.BetIndex >= 5) SevenPkDataManager.Instance.BetIndex = 1;

            //之後改為傳資料給後端後，後端回應在進行Do_TakeCard
            Do_TakeCard(SevenPkDataManager.Instance.BetIndex);
        }
    }

    void send_PlayAll()
    {
        lock (locker)
        {
            
        }

        StartCoroutine("Delay_TakeAll");
    }

    void send_Guess()
    {
        lock (locker)
        {

        }
    }

    void send_Jackpot()
    {
        lock (locker)
        {

        }
    }

    void send_Balance()
    {
        lock (locker)
        {

        }
    }

    #endregion

    #region UI

    public void UI_CardType()
    {
        Do_IsWin();

        //紀錄牌型
        string CardType;

        for(int i = 0; i < 10; i++)
        {
            CardType = SevenPKCards.Instance.CheckCardType(i);
            if (CardType != null)
            {
                Utilities.Log(CardType);
                return;
            }
        }

        for (int i = 0; i < 7; i++)
        {
            SevenPKCards.Instance.Mask[i].SetActive(true);
        }

        Utilities.Log("沒有中獎");
    }

    public void UI_Balance(long balance)
    {
        if (balanceLabel.CurrentNum > balance) balanceLabel.DoAddNumAnim(balanceLabel.CurrentNum,balance);
        else if (balanceLabel.CurrentNum < balance) balanceLabel.DoSubNumAnim(balanceLabel.CurrentNum, balance);
    }

    public void UI_Bet(long balance)
    {
        betLabel.text = balance.ToString();
    }

    public void UI_Win(long balance)
    {
        winLabel.DoAddNumAnim(0, balance);
    }

    public void UI_Jackpot(long balance)
    {
        if (jackpotLabel.CurrentNum > balance) jackpotLabel.DoAddNumAnim(jackpotLabel.CurrentNum, balance);
        else if (jackpotLabel.CurrentNum < balance) jackpotLabel.DoSubNumAnim(jackpotLabel.CurrentNum, balance);
    }

    public void UI_SilverLight(int index)
    {
        if (index == 0)
        {
            for (int i = 0; i < 3; i++)
            {
                SilverLight[i].SetActive(false);
            }
        }

        else
        {
            for (int i = 0; i < 3; i++)
            {
                if ((index-1) >= i) SilverLight[i].SetActive(true);
                else SilverLight[i].SetActive(false);
            }
        }
    }

    public void UI_GoldenLight(int index)
    {
        if (index == 0)
        {
            for (int i = 0; i < 3; i++)
            {
                GoldenLight[i].SetActive(false);
            }
        }

        else
        {
            for (int i = 0; i < 3; i++)
            {
                if ((index - 1) >= i) GoldenLight[i].SetActive(true);
                else GoldenLight[i].SetActive(false);
            }
        }
    }

    public void UI_JokerLight(int index)
    {
        if (index == 0)
        {
            for (int i = 0; i < 3; i++)
            {
                JokeLight[i].SetActive(false);
            }
        }

        else
        {
            for (int i = 0; i < 3; i++)
            {
                if ((index - 1) >= i) JokeLight[i].SetActive(true);
                else JokeLight[i].SetActive(false);
            }
        }
    }

    #endregion

    #region 延遲

    //第一次投注
    IEnumerator Delay_Bet1()
    {
        canGuess = false;
        canTake = false;

        SevenPKCards.Instance.Init_Cards();
        SevenPKCards.Instance.Init_Deck();
        yield return new WaitForSeconds(0.5f);

        SevenPKCards.Instance.TakeCards(0);
        yield return new WaitForSeconds(0.2f);
        SevenPKCards.Instance.TakeCards(1);
        yield return new WaitForSeconds(0.2f);
        SevenPKCards.Instance.TakeCards(2);
        yield return new WaitForSeconds(0.2f);

        //後端傳牌型過來用開牌SevenPKCards.Instance.OpenCards(第幾張,數字,花色);
        SevenPKCards.Instance.RandomCard(0);
        yield return new WaitForSeconds(0.2f);
        SevenPKCards.Instance.RandomCard(2);
        yield return new WaitForSeconds(0.2f);

        if (SevenPkDataManager.Instance.AutoPlay)
        {
            canClickAll = false;
            canClickBet = false;
            Do_Play();
        }
        else
        {
            canClickAll = true;
            canClickBet = true;
        }
    }

    //第二次投注
    IEnumerator Delay_Bet2()
    {
        SevenPKCards.Instance.TakeCards(3);
        yield return new WaitForSeconds(0.2f);
        SevenPKCards.Instance.TakeCards(4);
        yield return new WaitForSeconds(0.2f);

        SevenPKCards.Instance.RandomCard(4);
        yield return new WaitForSeconds(0.2f);

        if (SevenPkDataManager.Instance.AutoPlay)
        {
            canClickAll = false;
            canClickBet = false;
            Do_Play();
        }
        else
        {
            canClickBet = true;
        }
    }

    //第三次投注
    IEnumerator Delay_Bet3()
    {
        SevenPKCards.Instance.TakeCards(5);
        yield return new WaitForSeconds(0.2f);

        SevenPKCards.Instance.RandomCard(5);
        yield return new WaitForSeconds(0.2f);

        if (SevenPkDataManager.Instance.AutoPlay)
        {
            canClickAll = false;
            canClickBet = false;
            Do_Play();
        }
        else
        {
            canClickBet = true;
        }

    }

    //第四次投注
    IEnumerator Delay_Bet4()
    {
        canClickAll = false;

        SevenPKCards.Instance.TakeCards(6);
        yield return new WaitForSeconds(0.2f);

        SevenPKCards.Instance.RandomCard(6);
        yield return new WaitForSeconds(0.2f);
        SevenPKCards.Instance.RandomCard(1);
        yield return new WaitForSeconds(0.2f);
        SevenPKCards.Instance.RandomCard(3);
        yield return new WaitForSeconds(0.5f);

        UI_CardType();
    }

    //全開
    IEnumerator Delay_TakeAll()
    {
        canClickAll = false;
        canGuess = false;

        //拿牌
        for (int i = 0; i < 7; i++)
        {
            if (!SevenPKCards.Instance.Cards[i].gameObject.activeSelf)
            {
                SevenPKCards.Instance.TakeCards(i);
                yield return new WaitForSeconds(0.05f);
            }
        }

        //開牌
        for (int i = 0; i < 7; i++)
        {
            if (SevenPKCards.Instance.Cards[i].spriteName == "Card_Back")
            {
                //後端SevenPKCards.Instance.OpenCards(i,x,y);
                SevenPKCards.Instance.RandomCard(i);
                yield return new WaitForSeconds(0.05f);
            }
        }

        yield return new WaitForSeconds(0.5f);

        UI_CardType();
    }

    #endregion
}
