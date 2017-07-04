using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace Boast_SW
{
    public class RollDice : MonoBehaviour
    {
        public static RollDice Instance;

        //我的骰子
        public List<UISprite> MyDice;
        //其他人的骰子
        public List<List<UISprite>> OtherDice;
        public List<UISprite> OtherDice1;
        public List<UISprite> OtherDice2;
        public List<UISprite> OtherDice3;
        public List<UISprite> OtherDice4;
        public List<UISprite> OtherDice5;
        //需不需要骰子跳動動畫
        public bool NeedAnim = false;

        public List<int> ResultNumber = new List<int>();

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

        private void Start()
        {
            OtherDice = new List<List<UISprite>> { OtherDice1, OtherDice2, OtherDice3, OtherDice4, OtherDice5 };
        }

        //手機搖動
        private void Update()
        {
            if (Mathf.Abs(Input.acceleration.y) > 1.5f)
            {
                if (BoastDataManager.Instance.IsShaking) return;
                BoastUIManager.Instance.Anim_ShakeDiceCup();
            }
        }

        //轉動骰子
        public void Roll()
        {
            Utilities.Log("你的骰子點數:" + ResultNumber[0] + " " + ResultNumber[1] + " " + ResultNumber[2] + " " + ResultNumber[3] + " " + ResultNumber[4]);

            //如果不做動畫就直接換骰子
            if (!NeedAnim)
            {
                diceResult();
                return;
            }
            
            //開始作動畫
            for (int i = 0; i < MyDice.Count; i++)
            {
                Transform tf = MyDice[i].transform;
                Vector3 originPos = MyDice[i].transform.localPosition;
                Vector3 pos1 = new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(0.35f, 0.7f), 0);
                Vector3 pos2 = new Vector3(Random.Range(-0.15f, 0.15f), Random.Range(0.2f, 0.5f), 0);
                Vector3 pos3 = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(0.1f, 0.2f), 0);

                Sequence seq = DOTween.Sequence();
                seq.Append(tf.DORotate(new Vector3(0, 0, 360 * 20), 2.5f, RotateMode.FastBeyond360).SetEase(Ease.OutSine))//旋轉骰子
                   .Join(tf.DOLocalMove(originPos + pos1, 0.5f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad))//同時跳動骰子
                   .Insert(1.0f, tf.DOLocalMove(originPos + pos2, 0.3f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad))//反彈骰子
                   .Insert(1.6f, tf.DOLocalMove(originPos + pos3, 0.2f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad))//反彈骰子
                   .Insert(2.0f, tf.DOLocalMove(originPos, 0.1f).SetEase(Ease.Linear));//為避免位置跑掉，將骰子恢復原本位子
            }

            StartCoroutine("randomDice");
        }
        
        //每X秒隨機更新骰子數字
        IEnumerator randomDice()
        {
            for (int time = 0; time < 10; time++)
            {
                for (int i = 0; i < MyDice.Count; i++)
                {
                    int point = Random.Range(1, 7);
                    BoastUIManager.Instance.ChangeDicePoint(MyDice[i], point.ToString());
                }
                yield return new WaitForSeconds(0.2f);
            }

            diceResult();
        }
        
        //骰子結果
        private void diceResult()
        {
            for (int i = 0; i < 5; i++)
            {
                BoastUIManager.Instance.ChangeDicePoint(MyDice[i], ResultNumber[i].ToString());
            }
        }

        //更新其他骰子
        public void Updata_OtherDice(int index, List<int> dices)
        {
            index = index - 1;

            for (int d = 0; d < dices.Count; d++)
            {
                BoastUIManager.Instance.ChangeDicePoint(OtherDice[index][d], dices[d].ToString());
            }

            BoastUIManager.Instance.Anim_IsGodCard(index + 1);

            Utilities.Log("玩家" + (index+1) + "骰子為_" + OtherDice[index][0].spriteName + "," + OtherDice[index][1].spriteName + ","
                + OtherDice[index][2].spriteName + "," + OtherDice[index][3].spriteName + "," + OtherDice[index][4].spriteName);
        }

        //骰子變亮
        public void DiceWhite()
        {
            Color32 white = new Color32(255, 255, 255, 255);

            for (int i = 0; i < 5; i++)
            {
                MyDice[i].color = white;
            }

            for (int p = 0; p < 5; p++)
            {
                if (BoastUIManager.Instance.PlayerSit[p + 1].gameObject.activeSelf)
                {
                    for (int d = 0; d < 5; d++)
                    {
                        OtherDice[p][d].color = white;
                    }
                }
            }
        }

        //骰子變暗
        public void DiceDark()
        {
            string PoolDice = BoastUIManager.Instance.PoolDice.spriteName.Substring(1, 1);

            Color32 black = new Color32(80, 80, 80, 255);

            BoastDataManager.Instance.DiceNum = 0;
            //目前喊到的點數
            string PoolDiceSpriteName = PoolDice;

            for (int i = 0; i < 5; i++)
            {
                if (BoastDataManager.Instance.IsOne)
                {
                    if (MyDice[i].spriteName != PoolDiceSpriteName)
                    {
                        MyDice[i].color = black;
                    }
                    else BoastDataManager.Instance.DiceNum += 1;
                }
                else
                {
                    if (MyDice[i].spriteName != PoolDiceSpriteName && MyDice[i].spriteName != "1")
                    {
                        MyDice[i].color = black;
                    }
                    else BoastDataManager.Instance.DiceNum += 1;
                }
            }

            for (int p = 0; p < 5; p++)
            {
                if (BoastUIManager.Instance.PlayerSit[p + 1].gameObject.activeSelf)
                {
                    for (int d = 0; d < 5; d++)
                    {
                        if (BoastDataManager.Instance.IsOne)
                        {
                            if (OtherDice[p][d].spriteName != PoolDiceSpriteName)
                            {
                                OtherDice[p][d].color = black;
                            }
                            else BoastDataManager.Instance.DiceNum += 1;
                        }
                        else
                        {
                            if (OtherDice[p][d].spriteName != PoolDiceSpriteName && OtherDice[p][d].spriteName != "1")
                            {
                                OtherDice[p][d].color = black;
                            }
                            else BoastDataManager.Instance.DiceNum += 1;
                        }
                    }
                }
            }
        }
    }
}