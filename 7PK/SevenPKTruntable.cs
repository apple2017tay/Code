using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SevenPKTruntable : MonoBehaviour {

    public static SevenPKTruntable Instance;

    //Panel
    public GameObject TurnPanel;
    //轉盤
    public GameObject TurnTable;
    //箭頭
    public GameObject TurnArrow;
    //開始按鈕
    public GameObject StartButton;
    //獎勵數目
    public int rewardNum = 6;
    //箭頭角度
    public float ArrowAngle = 0.0f;
    //轉幾圈
    public int TrunNum = 10;
    //轉多久
    public float TrunDuration = 5.0f;

    //記錄本次停留角度
    float rewardAngle = 0;
    //記錄每個獎項的角度
    float angle = 360.0f;
    
    //防止連點
    bool isClick;
    bool isSkip;

    object locker = new object();

    private void Awake()
    {
        Instance = this;

        angle = 360 / rewardNum;
    }

    private void OnDestroy()
    {
        if (Instance != null)
        {
            Instance = null;
        }
    }

    //按下轉動按鈕
    public void Btn_Start()
    {
        if (isClick) return;
        isClick = true;

        lock (locker)
        {
            int temp = Random.Range(0, rewardNum + 1);
            Debug.Log(temp);
            Turn(temp);
        }
    }

    //跳過動畫
    public void Btn_Skip()
    {
        if (isSkip) return;
        isSkip = true;

        //刪除動畫
        TurnTable.transform.DOKill();
        //直接停在獎勵點
        TurnTable.transform.rotation = Quaternion.Euler(0f, 0f, rewardAngle);

        CancelInvoke("TurnEnd");
        Invoke("TurnEnd", 1.5f);
    }

    //顯示轉盤
    public void Show()
    {
        isClick = true;
        isSkip = true;
        isTurn = false;

        TurnTable.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        TurnPanel.transform.localScale = Vector3.zero;
        TurnPanel.SetActive(true);
        TurnPanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).OnComplete(()=> { isClick = false; });
    }

    //開始轉動
    bool isTurn = false;
    public void Turn(int reward)
    {
        if (isTurn) return;
        isTurn = true;

        //讓停留的角度活一點XD
        float randomAngle = 0.0f;
        randomAngle = Random.Range(-((angle / 2) - 5), (angle / 2) - 5);
        //記錄本次要停留的角度
        rewardAngle = (reward * angle) + randomAngle + ArrowAngle;
        //開始轉動
        TurnTable.transform.DORotate(new Vector3(0, 0, -(360 * TrunNum) + rewardAngle), TrunDuration, RotateMode.FastBeyond360)
                 .SetEase(Ease.InOutQuart).OnComplete(()=> { isSkip = true; Invoke("TurnEnd", 3.0f); });

        isSkip = false;
    }

    //轉動完成後
    void TurnEnd()
    {
        TurnPanel.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack)
                 .OnComplete(() => { TurnPanel.SetActive(false); SevenPKUIManager.Instance.Do_TakeOrGuess(); });

        //要做什麼?
    }
}
