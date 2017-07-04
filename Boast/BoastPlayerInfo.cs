using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Spine.Unity;

public class BoastPlayerInfo : MonoBehaviour {
    
    //玩家資訊
    public UILabel PlayerName;
    public UILabel Balance;
    public UILabel AddBalance;
    public UILabel SubBalance;
    public GameObject Double;
    public GameObject RoomOwner;
    public HeadPhotoManager Head;

    public int PlayerID;
    public int GameCount;
    public int WinCount;
    public int OpenCount;

    
    //玩家骰盅
    [Space(10)]
    public GameObject OpenCup;
    public GameObject PlayersDiceCup;
    public SkeletonAnimation DiceCupSpine;
    public GameObject CupsDice;
    public GameObject CupsBotton;
    public GameObject Cups;

    //提示框框
    [Space(10)]
    public GameObject Timeup;
    public GameObject DiceTip;
    public GameObject DiceTipMask;
    public UILabel DiceTipNumLabel;
    public UISprite DiceTipPoint;
    public TweenColor ColorTween;
    public TweenFill FillTween;
    public GameObject Chating;

    //玩家名字
    public void Set_Name(string name)
    {
        PlayerName.text = name;
    }

    //玩家金錢
    public void Set_Balance(long balance)
    {
        Utilities.SetNumOnLabelInNGUI(Balance, balance);
    }

    //玩家加錢
    public void Set_AddBalance(long balance)
    {
        AddBalance.text = "+" + balance;
        Invoke("Set_NotAdd",2.0f);
        //AddBalance.transform.localScale = new Vector3(1, 1, 1);
        //AddBalance.transform.localPosition = new Vector3(AddBalance.transform.localPosition.x, 0, 0);
        //AddBalance.transform.DOLocalMoveY(105, 1.0f)
        //                .OnComplete(() => AddBalance.transform.DOScale(Vector3.zero, 0.5f).SetDelay(1.0f).SetEase(Ease.InBack));
    }
    private void Set_NotAdd()
    {
        AddBalance.text = "";
    }

    //玩家扣錢
    public void Set_SubBalance(long balance)
    {
        SubBalance.text = "-" + balance;
        Invoke("Set_NotSub", 2.0f);
        //SubBalance.transform.localScale = new Vector3(1, 1, 1);
        //SubBalance.transform.localPosition = new Vector3(SubBalance.transform.localPosition.x, 0, 0);
        //SubBalance.transform.DOLocalMoveY(105, 1.0f)
        //                .OnComplete(() => SubBalance.transform.DOScale(Vector3.zero, 0.5f).SetDelay(1.0f).SetEase(Ease.InBack));
    }
    private void Set_NotSub()
    {
        SubBalance.text = "";
    }

    //玩家加倍
    public void Set_Double()
    {
        Double.transform.localScale = new Vector3(1, 1, 1);
        Double.transform.localPosition = new Vector3(Double.transform.localPosition.x, 0, 0);
        Double.transform.DOLocalMoveY(105, 1.0f)
                        .OnComplete(() => Double.transform.DOScale(Vector3.zero, 0.5f).SetDelay(1.0f).SetEase(Ease.InBack));
    }

    //玩家是房主
    public void Set_RoomOwner()
    {
        RoomOwner.transform.localScale = Vector3.zero;
        RoomOwner.SetActive(true);
        RoomOwner.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
    }

    //玩家頭像
    public void Set_Head(int pid)
    {
        Head.DownLoadPhotoAndSave(pid.ToString());
    }

    //倒數時間
    public void Set_CD(float time)
    {
        Timeup.GetComponent<Animator>().speed = 1 / time;
        ColorTween.duration = time;
        FillTween.duration = time;
    }

    //玩家抓人框框開啟
    public void Open_Cup()
    {
        OpenCup.SetActive(true);
        OpenCup.transform.GetChild(0).transform.DOScale(new Vector3(1.3f, 1.3f, 1.3f), 0.2f).SetLoops(2, LoopType.Yoyo);
        Invoke("Close_Cup", 1.5f);
    }

    //玩家抓人框框關閉
    public void Close_Cup()
    {
        OpenCup.SetActive(false);
    }

    //倒數框開啟
    public void Open_Timeup()
    {
        Timeup.SetActive(true);
    }

    //倒數框關閉
    public void Close_Timeup()
    {
        Timeup.SetActive(false);
    }

    //講話框開啟
    public void Open_Chating()
    {
        Chating.SetActive(true);
    }
    //講話框關閉
    public void Close_Chating()
    {
        Chating.SetActive(false);
    }
}
