using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

//花色
public enum CardTypes
{
    spade = 5, heart = 6, diamond = 7, club = 8, jokeBlack = 9, jokeRed = 10,
}

public class SevenPKCards : MonoBehaviour {

    public static SevenPKCards Instance;

    //牌
    public UISprite[] Cards;
    public UISprite[] Num;
    public UISprite[] Icon;
    public UISprite[] Icon2;
    public GameObject[] Mask;

    //隨機測試用牌組
    [Space(10)]
    public List<int> Deck;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    private void OnDestroy()
    {
        if(Instance != null)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        Init_Cards();
    }

    //重置卡片
    public void Init_Cards()
    {
        for (int i = 0; i < 7; i++)
        {
            //回上面
            /*Cards[i].gameObject.SetActive(false);
            Cards[i].transform.localPosition = new Vector3(Cards[i].transform.localPosition.x, 500, 0);
            Cards[i].transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            Cards[i].transform.DOKill();
            Mask[i].SetActive(false);*/

            //直接翻面
            Cards[i].transform.DORotate(new Vector3(0, 90, 45), 0.2f, RotateMode.FastBeyond360)
                    .OnComplete(() =>
                    {
                        Cards[i].transform.localRotation = Quaternion.Euler(0, 0, 0);

                    }
                    );

        }
    }

    //重置牌組(單機測試用)
    public void Init_Deck()
    {
        Deck = new List<int>();

        for (int n = 1; n <= 54; n++)
        {
            Deck.Add(n);
        }
    }

    //隨機抽牌(單機測試用)
    public void RandomCard(int index)
    {
        bool isRandom = true;
        int randomIndex = 0;
        int num = 0;
        int icon = 0;

        while (isRandom)
        {
            randomIndex = Random.Range(1, 54);
            //Debug.Log("第" + index+1 + "張牌是" + randomIndex);
            if (Deck[randomIndex] != -1)
            {
                Deck[randomIndex] = -1;

                //黑桃A~K
                if (randomIndex >= 1 && randomIndex <= 13)
                {
                    num = randomIndex;
                    icon = 5;
                }
                //愛心A~K
                else if (randomIndex >= 14 && randomIndex <= 26)
                {
                    num = randomIndex - 13;
                    icon = 6;
                }
                //方塊A~K
                else if (randomIndex >= 27 && randomIndex <= 39)
                {
                    num = randomIndex - 26;
                    icon = 7;
                }
                //梅花A~K
                else if (randomIndex >= 40 && randomIndex <= 52)
                {
                    num = randomIndex - 39;
                    icon = 8;
                }
                //黑鬼
                else if (randomIndex == 53)
                {
                    num = 14;
                    icon = 9;
                }
                //紅鬼
                else if (randomIndex == 54)
                {
                    num = 14;
                    icon = 10;
                }

                isRandom = false;
            }
        }
        OpenCards(index, num, icon);
    }

    //拿到卡片
    public void TakeCards(int index)
    {
        Cards[index].spriteName = "Card_Back";
        Cards[index].gameObject.SetActive(true);
        Num[index].gameObject.SetActive(false);
        Icon[index].gameObject.SetActive(false);
        Icon2[index].gameObject.SetActive(false);

        //動畫終點位置
        Cards[index].transform.DOLocalMoveY(-70 * (index % 2),0.2f).SetEase(Ease.OutBack);
        Cards[index].transform.DOScale(Vector3.one, 0.2f);
    }

    //翻開卡片
    public void OpenCards(int index, int num, int Inticon)
    {
        CardTypes icon = 0;

        switch (Inticon)
        {
            case 5:
                icon = CardTypes.spade;
                break;
            case 6:
                icon = CardTypes.heart;
                break;
            case 7:
                icon = CardTypes.diamond;
                break;
            case 8:
                icon = CardTypes.club;
                break;
            case 9:
                icon = CardTypes.jokeBlack;
                break;
            case 10:
                icon = CardTypes.jokeRed;
                break;
        }


        Sequence sq = DOTween.Sequence();
        sq.Append(Cards[index].transform.DORotate(new Vector3(0, -90, 45), 0.2f, RotateMode.FastBeyond360))
          .Join(Cards[index].transform.DOLocalMoveY(Cards[index].transform.localPosition.y - 20, 0.1f))
          .Join(Cards[index].transform.DOScale(new Vector3(1.6f, 1.6f, 1.6f), 0.2f))
          .AppendCallback(()=> cardsNumAndIcon(index, num, icon))
          .Join(Cards[index].transform.DOLocalMoveY(Cards[index].transform.localPosition.y + 20, 0.1f))
          .Append(Cards[index].transform.DOScale(Vector3.one, 0.2f));
    }

    //牌型判別
    public string CheckCardType(int type)
    {
        string cardType;

        switch(type)
        {
            case 0:
                cardType = royalStraightFlush();
                if (cardType == null) return null;
                else return cardType;

            case 1:
                cardType = fiveOfKind();
                if (cardType == null) return null;
                else return cardType;

            case 2:
                cardType = straightFlush(1);
                if (cardType == null) return null;
                else return cardType;

            case 3:
                cardType = fourOfKind();
                if (cardType == null) return null;
                else return cardType;

            case 4:
                cardType = fullHouse();
                if (cardType == null) return null;
                else return cardType;

            case 5:
                cardType = flush();
                if (cardType == null) return null;
                else return cardType;

            case 6:
                cardType = straight(1);
                if (cardType == null) return null;
                else return cardType;

            case 7:
                cardType = threeOfKind();
                if (cardType == null) return null;
                else return cardType;

            case 8:
                cardType = twoPair();
                if (cardType == null) return null;
                else return cardType;

            case 9:
                cardType = onePair(11);
                if (cardType == null) return null;
                else return cardType;

            default:
                return null;
        }
    }

    //顯示牌型
    void showCard(List<int> index)
    {
        List<bool> show = new List<bool> {false, false, false, false, false, false, false };

        //檢查有哪幾張是需要顯示的
        for (int i = 0; i < index.Count; i++)
        {
            show[index[i]] = true;
        }

        //把其他沒有要顯示的都蓋上遮罩
        for (int i = 0; i < 7; i++)
        {
            if (show[i]) Cards[i].transform.DOScale(new Vector3(1.1f, 1.1f, 1.1f), 0.5f).SetLoops(-1, LoopType.Yoyo);
            else Mask[i].SetActive(true);
        }
    }

    //換牌花色
    void cardsNumAndIcon(int index, int num, CardTypes icon)
    {
        bool isJoke = false;

        Cards[index].spriteName = "Card_Front";
        Cards[index].transform.localRotation = Quaternion.Euler(0, 0, 0);

        //數字
        Num[index].spriteName = "P" + num.ToString();

        //圖案
        switch (icon)
        {
            case CardTypes.spade:
                Icon[index].spriteName = "Spade";
                Icon2[index].spriteName = "Spade";
                Num[index].color = new Color32(0, 0, 0, 255);
                break;
            case CardTypes.heart:
                Icon[index].spriteName = "Heart";
                Icon2[index].spriteName = "Heart";
                Num[index].color = new Color32(255, 255, 255, 255);
                break;
            case CardTypes.diamond:
                Icon[index].spriteName = "Diamond";
                Icon2[index].spriteName = "Diamond";
                Num[index].color = new Color32(255, 255, 255, 255);
                break;
            case CardTypes.club:
                Icon[index].spriteName = "Club";
                Icon2[index].spriteName = "Club";
                Num[index].color = new Color32(0, 0, 0, 255);
                break;
            case CardTypes.jokeBlack:
                Icon[index].spriteName = "JokeB";
                isJoke = true;
                Num[index].color = new Color32(0, 0, 0, 255);
                break;
            case CardTypes.jokeRed:
                Icon[index].spriteName = "JokeR";
                isJoke = true;
                Num[index].color = new Color32(255, 255, 255, 255);
                break;
        }

        Icon[index].gameObject.SetActive(true);
        Num[index].gameObject.SetActive(true);
        if (!isJoke) Icon2[index].gameObject.SetActive(true); 
    }

    #region 判斷牌型

    //皇家同花順
    string royalStraightFlush()
    {
        //紀錄花色
        List<UISprite> spade = new List<UISprite>();
        List<UISprite> heart = new List<UISprite>();
        List<UISprite> diamond = new List<UISprite>();
        List<UISprite> club = new List<UISprite>();
        List<List<UISprite>> allIcon = new List<List<UISprite>> { spade, heart, diamond, club };

                                           //A      10     J      Q      K
        List<bool> tenToA = new List<bool> { false, false, false, false, false };

        //紀錄第幾張
        List<int> spadeInt = new List<int>();
        List<int> heartInt = new List<int>();
        List<int> diamondInt = new List<int>();
        List<int> clubInt = new List<int>();
        List<List<int>> showList = new List<List<int>> {spadeInt,heartInt,diamondInt,clubInt };

        //依照花色分類
        for (int i = 0; i < 7; i++)
        {
            if (Icon[i].spriteName == "Spade") { allIcon[0].Add(Num[i]); showList[0].Add(i); }
            else if (Icon[i].spriteName == "Heart") { allIcon[1].Add(Num[i]); showList[1].Add(i); }
            else if (Icon[i].spriteName == "Diamond") { allIcon[2].Add(Num[i]); showList[2].Add(i); }
            else if (Icon[i].spriteName == "Club") { allIcon[3].Add(Num[i]); showList[3].Add(i); }
        }

        //如果每個花色超過五張，則判斷是否有10 J Q K A
        for (int i =0; i < 4; i++)
        {
            //如果同花色超過五個
            if (allIcon[i].Count >= 5)
            {
                //檢查是否有10~A各一張
                for (int n = 0; n < allIcon[i].Count; n++)
                {
                    if (allIcon[i][n].spriteName == "P1") tenToA[0] = true;
                    else if (allIcon[i][n].spriteName == "P10") tenToA[1] = true;
                    else if (allIcon[i][n].spriteName == "P11") tenToA[2] = true;
                    else if (allIcon[i][n].spriteName == "P12") tenToA[3] = true;
                    else if (allIcon[i][n].spriteName == "P13") tenToA[4] = true;
                }

                for (int t = 0; t< tenToA.Count; t++)
                {
                    if (!tenToA[t]) break;

                    if (t == tenToA.Count-1)
                    {
                        showCard(showList[i]);
                        return "皇家同花順";
                    }
                }

                for (int t = 0; t < tenToA.Count; t++)
                {
                    tenToA[t] = false;
                }

                showList = new List<List<int>> { spadeInt, heartInt, diamondInt, clubInt };
            }
        }

        return null;
    }

    //五枚
    string fiveOfKind()
    {
        List<bool> JokeAndfour = new List<bool> { false, false, false, false, false };
        int JAFIndex = 0;

        List<int> showList = new List<int>();

        for (int i = 0; i < 7; i++)
        {
            //檢查是否有鬼牌
            if (Icon[i].spriteName == "JokeR" || Icon[i].spriteName == "JokeB")
            {
                JokeAndfour[JAFIndex] = true;
                JAFIndex += 1;

                //如果有鬼牌，則檢查其他牌是否有4張A~K
                for (int n = 1; n <= 13; n++)
                {
                    for (int cardIndex = 0; cardIndex < 7; cardIndex++)
                    {
                        if (Num[cardIndex].spriteName == "P" + n.ToString())
                        {
                            JokeAndfour[JAFIndex] = true;
                            JAFIndex += 1;
                            showList.Add(cardIndex);
                        }
                    }

                    for (int t = 0; t < JokeAndfour.Count; t++)
                    {
                        if (!JokeAndfour[t]) break;

                        if (t == JokeAndfour.Count - 1)
                        {
                            showList.Add(i);
                            showCard(showList);
                            return "五枚";
                        }
                    }

                    for (int j = 0; j < 5; j++)
                    {
                        JokeAndfour[j] = false;
                    }

                    JAFIndex = 0;
                    showList = new List<int>();
                }
            }
        }

        return null;
    }

    //同花順(可設定最小點數為多少)
    string straightFlush(int min = 1)
    {
        List<UISprite> spade = new List<UISprite>();
        List<UISprite> heart = new List<UISprite>();
        List<UISprite> diamond = new List<UISprite>();
        List<UISprite> club = new List<UISprite>();
        List<List<UISprite>> allIcon = new List<List<UISprite>> { spade, heart, diamond, club };

        List<bool> order = new List<bool> { false, false, false, false, false };

        List<int> spadeInt = new List<int>();
        List<int> heartInt = new List<int>();
        List<int> diamondInt = new List<int>();
        List<int> clubInt = new List<int>();
        List<List<int>> showList = new List<List<int>> { spadeInt, heartInt, diamondInt, clubInt };

        //依照花色分類
        for (int i = 0; i < 7; i++)
        {
            if (Icon[i].spriteName == "Spade") { allIcon[0].Add(Num[i]); showList[0].Add(i); }
            else if (Icon[i].spriteName == "Heart") { allIcon[1].Add(Num[i]); showList[1].Add(i); }
            else if (Icon[i].spriteName == "Diamond") { allIcon[2].Add(Num[i]); showList[2].Add(i); }
            else if (Icon[i].spriteName == "Club") { allIcon[3].Add(Num[i]); showList[3].Add(i); }
        }

        //如果每個花色超過五張，則判斷是否有順序
        for (int i = 0; i < 4; i++)
        {
            //如果同花色超過五個
            if (allIcon[i].Count >= 5)
            {
                //檢查是否有連續點數
                for (int m = 10; m >= min; m--)
                {
                    for (int n = 0; n < allIcon[i].Count; n++)
                    {
                        if (m == 10)
                        {
                            if (allIcon[i][n].spriteName == "P" + m.ToString()) order[0] = true;
                            else if (allIcon[i][n].spriteName == "P" + (m + 1).ToString()) order[1] = true;
                            else if (allIcon[i][n].spriteName == "P" + (m + 2).ToString()) order[2] = true;
                            else if (allIcon[i][n].spriteName == "P" + (m + 3).ToString()) order[3] = true;
                            else if (allIcon[i][n].spriteName == "P" + 1.ToString()) order[4] = true;
                        }
                        else
                        {
                            if (allIcon[i][n].spriteName == "P" + m.ToString()) order[0] = true;
                            else if (allIcon[i][n].spriteName == "P" + (m + 1).ToString()) order[1] = true;
                            else if (allIcon[i][n].spriteName == "P" + (m + 2).ToString()) order[2] = true;
                            else if (allIcon[i][n].spriteName == "P" + (m + 3).ToString()) order[3] = true;
                            else if (allIcon[i][n].spriteName == "P" + (m + 4).ToString()) order[4] = true;
                        }
                    }

                    for (int t = 0; t < order.Count; t++)
                    {
                        if (!order[t]) break;

                        if (t == order.Count - 1)
                        {
                            showCard(showList[i]);
                            return "同花順";
                        }
                    }

                    for (int orderIndex = 0; orderIndex < 5; orderIndex++)
                    {
                        order[orderIndex] = false;
                    }
                    showList = new List<List<int>> { spadeInt, heartInt, diamondInt, clubInt };
                }
            }
        }

        return null;
    }

    //四條
    string fourOfKind()
    {
        List<bool> fourKind = new List<bool> { false, false, false, false };
        int FKIndex = 0;

        List<int> showList = new List<int>();

        //檢查是否有4張A~K
        for (int n = 1; n <= 13; n++)
        {
            for (int cardIndex = 0; cardIndex < 7; cardIndex++)
            {
                if (Num[cardIndex].spriteName == "P" + n.ToString())
                {
                    fourKind[FKIndex] = true;
                    FKIndex += 1;
                    showList.Add(cardIndex);
                }
            }

            for (int t = 0; t < fourKind.Count; t++)
            {
                if (!fourKind[t]) break;

                if (t == fourKind.Count - 1)
                {
                    showCard(showList);
                    return "四條";
                }
            }

            for (int j = 0; j < 4; j++)
            {
                fourKind[j] = false;
            }
            showList = new List<int>();
            FKIndex = 0;
        }

        return null;
    }

    //葫蘆
    string fullHouse()
    {
        List<bool> threeKind = new List<bool> { false, false, false };
        int threeIndex = 0;
        List<bool> pair = new List<bool> { false, false, false };
        int pairIndex = 0;

        List<int> showList = new List<int>();
        List<int> pairList = new List<int>();

        //檢查是否有3張A~K
        for (int n = 1; n <= 13; n++)
        {
            for (int cardIndex = 0; cardIndex < 7; cardIndex++)
            {
                if (Num[cardIndex].spriteName == "P" + n.ToString())
                {
                    threeKind[threeIndex] = true;
                    threeIndex += 1;
                    showList.Add(cardIndex);
                }
            }

            if (threeKind[0] && threeKind[1] && threeKind[2])
            {
                //檢查是否有2張A~K，除了已被檢查有3張的A~K
                for (int m = 1; m <= 13; m++)
                {
                    if (m != n)
                    {
                        for (int cardIndex = 0; cardIndex < 7; cardIndex++)
                        {
                            if (Num[cardIndex].spriteName == "P" + m.ToString())
                            {
                                pair[pairIndex] = true;
                                pairIndex += 1;
                                pairList.Add(cardIndex);
                            }
                        }

                        if (pair[0] && pair[1])
                        {
                            for (int p = 0; p < pairList.Count; p++)
                            {
                                showList.Add(pairList[p]);
                            }
                            showCard(showList);
                            return "葫蘆";
                        }
                        pair[0] = false;
                        pair[1] = false;
                        pairIndex = 0;
                        pairList = new List<int>();
                    }
                }
            }
            showList = new List<int>();
            for (int t = 0; t < 3; t++)
            {
                threeKind[t] = false;
            }
            threeIndex = 0;
        }

            return null;
    }

    //同花
    string flush()
    {
        List<UISprite> spade = new List<UISprite>();
        List<UISprite> heart = new List<UISprite>();
        List<UISprite> diamond = new List<UISprite>();
        List<UISprite> club = new List<UISprite>();
        List<List<UISprite>> allIcon = new List<List<UISprite>> { spade, heart, diamond, club };

        List<int> spadeInt = new List<int>();
        List<int> heartInt = new List<int>();
        List<int> diamondInt = new List<int>();
        List<int> clubInt = new List<int>();
        List<List<int>> showList = new List<List<int>> { spadeInt, heartInt, diamondInt, clubInt };

        //依照花色分類
        for (int i = 0; i < 7; i++)
        {
            if (Icon[i].spriteName == "Spade") { allIcon[0].Add(Num[i]); showList[0].Add(i); } 
            else if (Icon[i].spriteName == "Heart") { allIcon[1].Add(Num[i]); showList[1].Add(i); }
            else if (Icon[i].spriteName == "Diamond") { allIcon[2].Add(Num[i]); showList[2].Add(i); }
            else if (Icon[i].spriteName == "Club") { allIcon[3].Add(Num[i]); showList[3].Add(i); }
        }

        for (int i = 0; i < 4; i++)
        {
            //如果同花色超過五個
            if (allIcon[i].Count >= 5)
            {
                showCard(showList[i]);
                return "同花";
            }
            showList = new List<List<int>> { spadeInt, heartInt, diamondInt, clubInt };
        }

                return null;
    }

    //順子(可設定最小點數為多少)
    string straight(int min = 1)
    {
        List<bool> order = new List<bool> { false, false, false, false, false };

        List<int> showList = new List<int>();

        //檢查是否有連續點數
        for (int m = 10; m >= min; m--)
        {
            for (int n = 0; n < 7; n++)
            {
                if (m == 10)
                {
                    if (Num[n].spriteName == "P" + m.ToString()) { order[0] = true; showList.Add(n); }
                    else if (Num[n].spriteName == "P" + (m + 1).ToString()) { order[1] = true; showList.Add(n); }
                    else if (Num[n].spriteName == "P" + (m + 2).ToString()) { order[2] = true; showList.Add(n); }
                    else if (Num[n].spriteName == "P" + (m + 3).ToString()) { order[3] = true; showList.Add(n); }
                    else if (Num[n].spriteName == "P" + 1.ToString()) { order[4] = true; showList.Add(n); }
                }
                else
                {
                    if (Num[n].spriteName == "P" + m.ToString()) { order[0] = true; showList.Add(n); }
                    else if (Num[n].spriteName == "P" + (m + 1).ToString()) { order[1] = true; showList.Add(n); }
                    else if (Num[n].spriteName == "P" + (m + 2).ToString()) { order[2] = true; showList.Add(n); }
                    else if (Num[n].spriteName == "P" + (m + 3).ToString()) { order[3] = true; showList.Add(n); }
                    else if (Num[n].spriteName == "P" + (m + 4).ToString()) { order[4] = true; showList.Add(n); }
                }
            }

            for (int t = 0; t < order.Count; t++)
            {
                if (!order[t]) break;

                if (t == order.Count - 1)
                {
                    showCard(showList);
                    return "順子";
                }
            }

            showList = new List<int>();
            for (int orderIndex = 0; orderIndex < 5; orderIndex++)
            {
               order[orderIndex] = false;
            }

        }

        return null;
    }

    //三條
    string threeOfKind()
    {
        List<bool> threeKind = new List<bool> { false, false, false };
        int threeIndex = 0;
        List<int> showList = new List<int>();

        //檢查是否有3張A~K
        for (int n = 1; n <= 13; n++)
        {
            for (int cardIndex = 0; cardIndex < 7; cardIndex++)
            {
                if (Num[cardIndex].spriteName == "P" + n.ToString())
                {
                    threeKind[threeIndex] = true;
                    threeIndex += 1;
                    showList.Add(cardIndex);
                }
            }

            for (int t = 0; t < threeKind.Count; t++)
            {
                if (!threeKind[t]) break;

                if (t == threeKind.Count - 1)
                {
                    showCard(showList);
                    return "三條";
                }
            }

            for (int t = 0; t < 3; t++)
            {
                threeKind[t] = false;
            }
            threeIndex = 0;
            showList = new List<int>();
        }
        return null;
    }

    //兩對
    string twoPair()
    {
        List<bool> pairA = new List<bool> { false, false };
        int pairAIndex = 0;
        List<bool> pairB = new List<bool> { false, false };
        int pairBIndex = 0;

        List<int> showList = new List<int>();
        List<int> pairList = new List<int>();

        //檢查是否有2張A~K
        for (int n = 1; n <= 13; n++)
        {
            for (int cardIndex = 0; cardIndex < 7; cardIndex++)
            {
                if (Num[cardIndex].spriteName == "P" + n.ToString())
                {
                    pairA[pairAIndex] = true;
                    pairAIndex += 1;
                    showList.Add(cardIndex);
                }
            }

            if (pairA[0] && pairA[1])
            {
                //檢查是否有2張A~K，除了已被檢查過的A~K
                for (int m = 1; m <= 13; m++)
                {
                    if (m != n)
                    {
                        for (int cardIndex = 0; cardIndex < 7; cardIndex++)
                        {
                            if (Num[cardIndex].spriteName == "P" + m.ToString())
                            {
                                pairB[pairBIndex] = true;
                                pairBIndex += 1;
                                pairList.Add(cardIndex);
                            }
                        }

                        if (pairB[0] && pairB[1])
                        {
                            for(int p = 0; p < pairList.Count; p++)
                            {
                                showList.Add(pairList[p]);
                            }
                            showCard(showList);
                            return "兩對";
                        }
                        pairB[0] = false;
                        pairB[1] = false;
                        pairBIndex = 0;
                        pairList = new List<int>();
                    }
                }
            }
            pairA[0] = false;
            pairA[1] = false;
            pairAIndex = 0;
            showList = new List<int>();
        }

        return null;
    }

    //一對(可設定最小點數為多少)
    string onePair(int min = 2)
    {
        List<bool> pair = new List<bool> { false, false };
        int pairIndex = 0;
        List<int> showList = new List<int>();

        if (min <= 1) min = 2;

        //先檢查是否有一對A
        for (int cardIndex = 0; cardIndex < 7; cardIndex++)
        {
            if (Num[cardIndex].spriteName == "P" + 1.ToString())
            {
                pair[pairIndex] = true;
                pairIndex += 1;
                showList.Add(cardIndex);
            }
        }

        if (pair[0] && pair[1])
        {
            showCard(showList);
            return "一對";
        }
        pair[0] = false;
        pair[1] = false;
        pairIndex = 0;
        showList = new List<int>();

        //檢查是否有一對2~K
        for (int n = 13; n >= min; n--)
        {
            for (int cardIndex = 0; cardIndex < 7; cardIndex++)
            {
                if (Num[cardIndex].spriteName == "P" + n.ToString())
                {
                    pair[pairIndex] = true;
                    pairIndex += 1;
                    showList.Add(cardIndex);
                }
            }

            if (pair[0] && pair[1])
            {
                showCard(showList);
                return "一對";
            }
            pair[0] = false;
            pair[1] = false;
            pairIndex = 0;
            showList = new List<int>();
        }

        return null;
    }

    #endregion

}
