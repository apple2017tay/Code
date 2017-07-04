using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Boast_SW;
using Loger_System;

public class BoastChoseForm : MonoBehaviour {

    public static BoastChoseForm Instance;

    public GameObject ChoseDicePointForm;
    public GameObject ChoseDiceNumForm;

    public UISprite ChoseDiceSprite;

    public UILabel ChoseNumLabel;

    public UIScrollView ChoseNumberScroll;
    public UIScrollView ChoseDiceScroll;

    public UIGrid ChoseNumberGrid;
    public UIGrid ChosePointGrid;

    public int minNum = 0;
    public int maxNum = 0;

    bool isFirst = false;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance != null)
        {
            Instance = null;
        }
    }

    //喊骰子數量
    public void Button_ChoseDiceNumber(GameObject DiceNumber)
    {
        AudioManager.Instance.PlaySound("Button1", 1.0f);
        DiceNumber.transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.1f).OnComplete(() => DiceNumber.transform.DOScale(Vector3.one, 0.1f));
        ChoseNumLabel.text = DiceNumber.name;
        BoastDataManager.Instance.ChoseDiceNumber = int.Parse(DiceNumber.name);

        if (BoastDataManager.Instance.called.num == 0 || BoastDataManager.Instance.called.point == 0) return;

        //選擇數量大於目前喊的數量，全開點數
        if (BoastDataManager.Instance.ChoseDiceNumber > BoastDataManager.Instance.called.num)
        {
            SwitchCalledChosePoint(new List<bool> {true, true, true, true, true, true });
        }
        else
        {
            if (BoastDataManager.Instance.called.point == 6)
            {
                SwitchCalledChosePoint(new List<bool> { true, false, false, false, false, false });
            }
            else if (BoastDataManager.Instance.ChoseDicePoint != 6)
            {
                List<bool> isOpen = new List<bool> { false, false, false, false, false, false };

                for (int i = 0; i < 6; i++)
                {
                    if (i == 0) isOpen[i] = true;
                    else if (i >= (BoastDataManager.Instance.ChoseDicePoint - 1)) isOpen[i] = true;
                    else isOpen[i] = false;
                }

                SwitchCalledChosePoint(isOpen);
            }
        }
    }
    //喊骰子
    public void Button_ChoseDicePoint(GameObject Dice)
    {
        AudioManager.Instance.PlaySound("Button1", 1.0f);
        Dice.transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.1f).OnComplete(() => Dice.transform.DOScale(Vector3.one, 0.1f));
        ChoseDiceSprite.spriteName = Dice.name;
        BoastDataManager.Instance.ChoseDicePoint = int.Parse(Dice.name.Substring(1, 1));
    }

    //選擇骰子數量視窗
    public void Show_DiceNum()
    {
        if (BoastUIManager.Instance.AnimShake) return;

        if (ChoseDiceNumForm.activeSelf)
        {
            Close_DiceNum();
            return;
        }

        AudioManager.Instance.PlaySound("Button1", 1.0f);
        ChoseDiceNumForm.SetActive(true);
        ChoseDiceNumForm.transform.DOScaleY(1, 0.1f).OnComplete(() => ChoseNumberScroll.ResetPosition());

        SwitchCalledChoseNumber(minNum,maxNum);

        //關閉選擇骰子點數視窗
        if (ChoseDicePointForm.activeSelf)
        {
            Close_DicePoint();
        }

        //關閉輪到你回合提示
        if (BoastUIManager.Instance.AnimTurn.activeSelf)
        {
            BoastUIManager.Instance.Anim_Close(BoastUIManager.Instance.AnimTurn);
        }
    }
    public void Close_DiceNum()
    {
        if (!ChoseDiceNumForm.activeSelf) return;
        ChoseDiceNumForm.transform.DOScaleY(0, 0.1f).OnComplete(() => ChoseDiceNumForm.SetActive(false));
    }

    //選擇骰子點數視窗
    public void Show_DicePoint()
    {
        if (BoastUIManager.Instance.AnimShake) return;

        if (ChoseDicePointForm.activeSelf)
        {
            Close_DicePoint();
            return;
        }

        AudioManager.Instance.PlaySound("Button1", 1.0f);
        ChoseDicePointForm.SetActive(true);
        ChoseDicePointForm.transform.DOScaleY(1, 0.1f).OnComplete(() => ChoseDiceScroll.ResetPosition()); ;

        //關閉選擇骰子數量視窗
        if (ChoseDiceNumForm.activeSelf)
        {
            Close_DiceNum();
        }

        //關閉輪到你回合提示
        if (BoastUIManager.Instance.AnimTurn.activeSelf)
        {
            BoastUIManager.Instance.Anim_Close(BoastUIManager.Instance.AnimTurn);
        }
    }
    public void Close_DicePoint()
    {
        if (!ChoseDicePointForm.activeSelf) return;
        ChoseDicePointForm.transform.DOScaleY(0, 0.1f).OnComplete(() => ChoseDicePointForm.SetActive(false));
    }

    //面板更新
    public void UI_Chose(int calledNum, int calledPoint)
    {
        if (calledNum == 0 || calledPoint == 0) return;

        minNum = calledNum;
        int minPoint = 0;

        //1被喊
        if (calledPoint == 1) BoastUIManager.Instance.UI_OneIsCalled();

        //如果不是我喊的
        if (BoastDataManager.Instance.called.pos != BoastDataManager.Instance.MyPos)
        {
            BoastDataManager.Instance.IsCatch = true;

            if (BoastUIManager.Instance.PlayerSit[0].OpenCount == 0)
            {
                if (!isFirst)
                {
                    isFirst = true;
                    BoastUIManager.Instance.Anim_Tap();
                }
            }
        }
        else BoastUIManager.Instance.Anim_Close(BoastUIManager.Instance.AnimTap);

        //喊到最大時
        if (calledNum == maxNum && calledPoint == 1)
        {
            BoastDataManager.Instance.ChoseDiceNumber = calledNum;
            BoastDataManager.Instance.ChoseDicePoint = calledPoint;

            ChoseNumLabel.text = calledNum.ToString();
            ChoseDiceSprite.spriteName = "N1";

            return;
        }

        //如果當前選擇數量小於等於目前已喊數量
        if (BoastDataManager.Instance.ChoseDiceNumber <= calledNum)
        {
            Utilities.Log("選擇數量小於等於已喊數量");
            //點數為1 把數量+1 點數從2開始
            if (calledPoint == 1)
            {
                Utilities.Log("已喊點數為1");
                BoastDataManager.Instance.ChoseDiceNumber = calledNum + 1;
                BoastDataManager.Instance.ChoseDicePoint = 2;

                minNum += 1;
                if (minNum >= maxNum) minNum = maxNum;
                minPoint = 2;

                ChoseNumLabel.text = minNum.ToString();
                ChoseDiceSprite.spriteName = "N2";
            }
            //點數非為1
            else
            {
                Utilities.Log("已喊點數為2 3 4 5 6");
                if (BoastDataManager.Instance.ChoseDicePoint <= calledPoint && BoastDataManager.Instance.ChoseDicePoint != 1)
                {
                    Utilities.Log("選擇點數小於等於已喊點數");
                    BoastDataManager.Instance.ChoseDicePoint += 1;
                    if (BoastDataManager.Instance.ChoseDicePoint >= 7) BoastDataManager.Instance.ChoseDicePoint = 1;

                    minPoint = BoastDataManager.Instance.ChoseDicePoint;
                    if (minPoint == 1) minPoint = 2;

                    ChoseDiceSprite.spriteName = "N" + BoastDataManager.Instance.ChoseDicePoint;
                }
                else
                {
                    Utilities.Log("選擇點數大於已喊點數");
                    minPoint = calledPoint +1;
                    if (minPoint >= 7) minPoint = 2;
                }

                minNum = calledNum;
                BoastDataManager.Instance.ChoseDiceNumber = calledNum;
                ChoseNumLabel.text = minNum.ToString();

            }
        }
        //如果當前選擇數量已大於目前已喊數量
        else
        {
            Utilities.Log("選擇數量大於已喊數量");
            minPoint = 2;

            //點數為1時
            if (calledPoint == 1)
            {
                Utilities.Log("點數為1");
                minNum += 1;
                if (minNum >= maxNum) minNum = maxNum;
            }
            else
            {
                Utilities.Log("點數為2 3 4 5 6");
                minNum = calledNum;
            }
        }

        SwitchCalledChoseNumber(minNum, maxNum);

        //改變可選點數
        if (minPoint == 1)
        {
            SwitchCalledChosePoint(new List<bool> { true, false, false, false, false, false });
        }
        else if (minPoint != 1)
        {
            List<bool> isOpen = new List<bool> { false, false, false, false, false, false };

            for (int i = 0; i < 6; i++)
            {
                if (i == 0) isOpen[i] = true;
                else if (i >= (minPoint - 1)) isOpen[i] = true;
                else isOpen[i] = false;
            }

            SwitchCalledChosePoint(isOpen);
        }

        #region 已註解
        //如果被喊的點數是1，而且目前選擇數量又低於或等於時
        /*if (calledPoint == 1)
        {  
            //數量+1
            minNum = calledNum + 1;
            if (minNum >= maxNum) minNum = maxNum;

            //如果玩家預選的數量小於等於目前以喊數量，就幫他改變最小喊點數量
            if (BoastDataManager.Instance.ChoseDiceNumber <= calledNum)
            {
                //數量從已喊數量+1開始
                BoastDataManager.Instance.ChoseDiceNumber = minNum;
                ChoseNumLabel.text = BoastDataManager.Instance.ChoseDiceNumber.ToString();

                //點數從2開始
                BoastDataManager.Instance.ChoseDicePoint = 2;
                ChoseDiceSprite.spriteName = "N2";
            }

            SwitchCalledChoseNumber(minNum, maxNum);
            SwitchCalledChosePoint(new List<bool> { true, true, true, true, true, true });
        }
        //如果點數不等於1
        else if (calledPoint != 1)
        {
            //數量不變
            minNum = calledNum;

            //玩家預選數量小於以喊數量時
            if (BoastDataManager.Instance.ChoseDiceNumber <= calledNum)
            {
                //選擇最小數量
                BoastDataManager.Instance.ChoseDiceNumber = minNum;
                ChoseNumLabel.text = BoastDataManager.Instance.ChoseDiceNumber.ToString();

                //選擇最小點數
                if (calledPoint >= 2 && calledPoint <= 5)
                {
                    minPoint = calledPoint + 1;
                    if (BoastDataManager.Instance.ChoseDicePoint != 1 && BoastDataManager.Instance.ChoseDicePoint <= calledPoint)
                    {
                        BoastDataManager.Instance.ChoseDicePoint = minPoint;
                        ChoseDiceSprite.spriteName = "N" + BoastDataManager.Instance.ChoseDicePoint;
                    }
                }
                else if (calledPoint == 6)
                {
                    BoastDataManager.Instance.ChoseDicePoint = 1;
                    minPoint = 1;
                    ChoseDiceSprite.spriteName = "N" + BoastDataManager.Instance.ChoseDicePoint;
                }

            }
            else
            {
                //最小可喊點數
                if (calledPoint >= 2 && calledPoint <= 5)
                {
                    minPoint = calledPoint + 1;
                }
                else if (calledPoint == 6)
                {
                    minPoint = 1;
                }
            }

            SwitchCalledChoseNumber(minNum, maxNum);


            //改變可選點數
            if (minPoint == 1)
            {
                SwitchCalledChosePoint(new List<bool> { true, false, false, false, false, false });
            }
            else if(minPoint != 1)
            {
                List<bool> isOpen = new List<bool> { false, false, false, false, false, false };

                for (int i = 0; i < 6; i++)
                {
                    if (i == 0) isOpen[i] = true;
                    else if (i >= (BoastDataManager.Instance.ChoseDicePoint-1)) isOpen[i] = true;
                    else isOpen[i] = false;
                }

                SwitchCalledChosePoint(isOpen);
            }

        }*/
        #endregion
    }

    //開關已喊的數量按鈕
    public void SwitchCalledChoseNumber(int start, int end)
    {
        if (start >= end) return;

        start -= 1;

        //關閉前面區段
        for (int i = 0; i < start; i++)
        {
            if (i == start) break;
            ChoseNumberGrid.transform.GetChild(i).gameObject.SetActive(false);
        }
        //開啟可按區段
        for (int i = start; i < end; i++)
        {
            ChoseNumberGrid.transform.GetChild(i).gameObject.SetActive(true);
        }
        //關閉後面區段
        for (int i = end; i < 30; i++)
        {
            if (end >= 30) break;
            ChoseNumberGrid.transform.GetChild(i).gameObject.SetActive(false);
        }
        ChoseNumberScroll.ResetPosition();
    }

    public void SwitchCalledChosePoint(List<bool> isOpen)
    {
        for (int i = 0; i < 6; i++)
        {
            ChosePointGrid.transform.GetChild(i).gameObject.SetActive(isOpen[i]);
        }
        ChosePointGrid.Reposition();
        ChoseDiceScroll.ResetPosition();
    }
}
