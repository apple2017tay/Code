using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Loger_System;
using BaseProtocol;
using DG.Tweening;
using NGUI_ExtendByLoger;
using Spine.Unity;
using System.Threading;
using System;
using VoiceChat;

//按鈕狀態
enum ButtonState { start, play, notPlay, };

namespace Boast_SW
{
    public class BoastUIManager : MonoBehaviour
    {
        #region 變數

        public static BoastUIManager Instance;
        //彈幕
        public BarrageManager Barrage;
        //完家資訊
        public PlayerFormManager PlayerForm;
        //儲值頁面
        public DepositFormManager DepositForm;
        //設定頁面
        public SettingFormManager SettingForm;
        //FB上傳
        public FBFormManager FBForm;
        //綁定頁面
        public AccBindManager AccBindForm;
        //離開
        public GameObject ExitForm;
        //遮罩
        public GameObject ExitMask;
        //麥克風
        public VoiceChatManager voiceChatManager;

        public BoastCoinManager Coin;

        //玩家資訊
        [Space(10)]
        public BoastPlayerInfo[] PlayerSit;

        //底池物件、骰子數量、點數及顯示金額
        [Space(10)]
        public GameObject PoolDiceInfo;
        public UILabel PoolDiceNum;
        public UISprite PoolDice;
        public UILabel PoolMoney;

        //遊玩按鈕
        [Space(10)]
        public GameObject[] PlayButton;

        //已喊、必須大於
        [Space(10)]
        public GameObject CalledTip;
        public UISprite CalledDice;
        public GameObject NeedBig;

        //提示動畫
        [Space(10)]
        public GameObject AnimTip;
        public GameObject AnimWait;
        public GameObject AnimShake;
        public GameObject AnimTurn;
        public GameObject AnimStart;
        public GameObject AnimTap;
        public GameObject AnimTapTip;

        //繼續遊戲視窗
        [Space(10)]
        public GameObject ConPanel;
        public GameObject ConForm;

        //九宮格
        [Space(10)]
        public GameObject NineCupFormPanel;
        public GameObject NineCupForm;
        public UISprite[] NineCupWaterForm;
        public GameObject NineCupTable;
        public UISprite[] NineCupWaterTable;

        //勝利、失敗、平手
        [Space(10)]
        public GameObject Win;
        public GameObject Lose;
        public GameObject[] Drew;

        //跳抓
        [Space(10)]
        public GameObject JumpDouble;

        //天牌
        [Space(10)]
        public GameObject[] GodCard;

        //表情
        [Space(10)]
        public UISprite[] face;

        private object locker = new object();

        #endregion

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            AccBindForm = GlobalFormManager.Instance.GetAccForm();
            FBForm = GlobalFormManager.Instance.GetFBForm();
            DepositForm = GlobalFormManager.Instance.GetDepositForm();
            SettingForm = GlobalFormManager.Instance.GetSettingForm();

            voiceChatManager.OnSend += OnSendMsgToGame;
            voiceChatManager.OnStopRecord += recoverAudios;
            voiceChatManager.OnDespawnVoice += _OnDespawnVoice;

            SettingForm.isBackToLobby = true;
            SettingForm.leaveMsg = "您確定要回到大廳嗎?";
        }

        private void OnDestroy()
        {
            if (Instance != null)
            {
                Instance = null;
            }

            DepositForm.OnSendPrice -= DepositForm_OnSendPrice;
            AccBindForm.OnSendBinding -= AccBind_SendBinding;
            AccBindForm.OnSendGetSms -= AccBind_SendGetSms;
        }

        void Start()
        {
            AB_SaveAndLoad.Instance.UnLoadAssetBundle(false);
            //訂閱
            DepositForm.OnSendPrice += DepositForm_OnSendPrice;
            AccBindForm.OnSendBinding += AccBind_SendBinding;
            AccBindForm.OnSendGetSms += AccBind_SendGetSms;
            SettingForm.OnBackToLobby = SettingForm_OnBackToLobby;

            //播放音樂
            AudioManager.Instance.PlayBgm("BGM3", true, true);

            //請求快照
            send_GetSnapShot();
        }

        #region 訂閱

        //訂閱儲值
        private void DepositForm_OnSendPrice(OperateInfo op)
        {
            BoastMsgManager.Instance.SendLobby(op);
        }
        //訂閱送出簡訊
        private void AccBind_SendBinding(OperateInfo op)
        {
            BoastMsgManager.Instance.SendLobby(op);
        }
        //訂閱確認
        private void AccBind_SendGetSms(OperateInfo op)
        {
            BoastMsgManager.Instance.SendLobby(op);
        }
        public void BackToLobby()
        {
            if (PlayerSit[0].PlayersDiceCup.activeSelf)
            {
                ErrorFormManager.Instance.Show("您確定要離開? 本局投注金額將不會返還。", new EventDelegate(() =>
                {
                    BoastMsgManager.Instance.Leave();
                }), new EventDelegate(() =>
                {
                    ErrorFormManager.Instance.Close();
                }));
            }
            else
            {
                SettingForm.BackToLobby();
            }
        }
        //訂閱返回大廳
        private void SettingForm_OnBackToLobby(object sender, System.EventArgs e)
        {
            BoastMsgManager.Instance.Leave();
        }
        //訂閱麥克風
        private void OnSendMsgToGame(OperateInfo op)
        {
            BoastMsgManager.Instance.SendGame(op);
        }
        //訂閱麥克風
        private void OnSendMsgToChat(OperateInfo op)
        {
            BoastMsgManager.Instance.SendChat(op);
        }
        //訂閱麥克風
        private void _OnDespawnVoice(object obj)
        {
            int tempID = Convert.ToInt32(obj);

            int playerPos = PlayerPos(PlayerIDPos(tempID));

            PlayerSit[playerPos].Close_Chating();
        }

        #endregion

        #region 按鈕

        //拍照
        public void Button_TakePicture()
        {
            if (Monitor.TryEnter(locker, 0))
            {
                try
                {
                    UM_Camera.Instance.SaveScreenshotToGallery();
                }
                catch
                {
                }
                finally
                {
                    Monitor.Exit(locker);
                }
            }
        }

        //準備開始，每局開始(房主)
        public void ReadyStart()
        {
            ChangeButton(ButtonState.start);
        }
        public void Button_Start()
        {
            AudioManager.Instance.PlaySound("Button1", 1.0f);

            //如果玩家人數不足，就return
            //if (BoastDataManager.Instance.snapShot.players.Count <= 1) return;

            int playerNum = 0;

            for (int i = 0; i < 6; i++)
            {
                if (PlayerSit[i].gameObject.activeSelf) playerNum++;
            }

            if (playerNum <= 1) return; 

            send_Start();
        }

        //準備喊點，每當自己回合
        public void ReadyPlay()
        {
            ChangeButton(ButtonState.play);

            if (AnimStart.activeSelf)
            {
                Invoke("Anim_YourTurn", 1.0f);
            }
            else
            {
                Anim_YourTurn();
            }
        }
        public void Button_Play()
        {
            AudioManager.Instance.PlaySound("Button1", 1.0f);

            //關閉輪到你回合提示
            if (AnimTurn.activeSelf) Anim_Close(AnimTurn);

            BoastDataManager.Instance.ChoseDiceNumber = int.Parse(BoastChoseForm.Instance.ChoseNumLabel.text);
            BoastDataManager.Instance.ChoseDicePoint = int.Parse(BoastChoseForm.Instance.ChoseDiceSprite.spriteName.Substring(1, 1));

            //如果喊的數量跟點數超出規定就跳出
            if (BoastDataManager.Instance.ChoseDiceNumber < BoastChoseForm.Instance.minNum || BoastDataManager.Instance.ChoseDiceNumber > BoastChoseForm.Instance.maxNum)
            {
                Utilities.Log("數量錯誤: " + BoastDataManager.Instance.ChoseDiceNumber);
                return;
            }
            if (BoastDataManager.Instance.ChoseDicePoint <= 0 || BoastDataManager.Instance.ChoseDicePoint > 6)
            {
                Utilities.Log("點數錯誤: " + BoastDataManager.Instance.ChoseDicePoint);
                return;
            }

            //如果喊的數量跟點數小於最後一次喊的就跳出
            if (BoastDataManager.Instance.ChoseDiceNumber < BoastDataManager.Instance.called.num)
            {
                UI_NeedToBig();
                Utilities.Log("數量太小");
                return;
            }
            //如果喊的數量是一樣，那點數就必須不一樣
            else if (BoastDataManager.Instance.ChoseDiceNumber == BoastDataManager.Instance.called.num)
            {
                if (BoastDataManager.Instance.called.point == 1)
                {
                    UI_NeedToBig();
                    Utilities.Log("數量一樣，但是點數太小");
                    return;
                }
                else if (BoastDataManager.Instance.ChoseDicePoint <= BoastDataManager.Instance.called.point && BoastDataManager.Instance.ChoseDicePoint != 1)
                {
                    UI_NeedToBig();
                    Utilities.Log("數量一樣，點數也一樣");
                    return;
                }
            }

            send_DicePointAndNum();
        }

        //不是自己回合無法喊點或已喊完點
        public void NotPlay()
        {
            ChangeButton(ButtonState.notPlay);
        }

        //我不要繼續
        public void Button_ConNo()
        {
            send_NoCon();
            Form_CloseCon();
        }
        //我要繼續
        public void Button_ConYes()
        {
            send_Con();
            Form_CloseCon();
        }

        //抓人開牌
        public void Button_CatchAndOpen()
        {
            if (BoastDataManager.Instance.IsCatch) send_Catch();
        }

        //選擇酒杯
        public void Button_ChoseNineCup(GameObject cup)
        {
            //可以選擇酒杯時
            if (!BoastDataManager.Instance.IsChoseCup)
            {
                if (NineCupWaterForm[int.Parse(cup.name)].gameObject.activeSelf) return;
                AudioManager.Instance.PlaySound("Pouring2", 1.0f);
                BoastDataManager.Instance.IsChoseCup = true;
                send_ChoseNineCup(int.Parse(cup.name));
            }
        }

        #endregion

        #region 麥克風

        public void Button_RecordVoice()
        {
            muteAudios();
            voiceChatManager.StartRecord();
        }

        public void Button_BreakRecordVoice()
        {
            voiceChatManager.BreakRecord();
        }

        bool isBgmMuteBeforeRecord;
        bool isVocalMuteBeforeRecord;
        bool isSoundMuteBeforeRecord;
        private void muteAudios()
        {
            isBgmMuteBeforeRecord = AudioManager.Instance.IsBgmMute;
            isVocalMuteBeforeRecord = AudioManager.Instance.IsVocalMute;
            isSoundMuteBeforeRecord = AudioManager.Instance.IsSoundMute;

            AudioManager.Instance.ToggleVol(AudioClipType.Bgm, false);
            AudioManager.Instance.ToggleVol(AudioClipType.Vocal, false);
            AudioManager.Instance.ToggleVol(AudioClipType.Sound, false);
        }

        private void recoverAudios()
        {
            if (isBgmMuteBeforeRecord == false)
            {
                AudioManager.Instance.ToggleVol(AudioClipType.Bgm, true);
            }

            if (isVocalMuteBeforeRecord == false)
            {
                AudioManager.Instance.ToggleVol(AudioClipType.Vocal, true);
            }

            if (isSoundMuteBeforeRecord == false)
            {
                AudioManager.Instance.ToggleVol(AudioClipType.Sound, true);
            }
        }

        public void ReceiveVoiceChat(string key, VoiceChatPacket data, int id)
        {
            //確定在遊戲中才可以撥放聲音
            if (BoastDataManager.Instance.IsReady)
            {
                voiceChatManager.EnqueueVoice(key, data, id);
                PlayerSit[PlayerPos(PlayerIDPos(id))].Open_Chating();
            }
        }

        #endregion

        #region 動畫

        //等待動畫
        public void Anim_Wait()
        {
            Sequence sq = DOTween.Sequence();
            AnimTip.SetActive(true);
            AnimWait.SetActive(true);
            sq.Append(AnimTip.transform.DOScaleY(1, 0.2f));

            //如果是房主就打開準備開始遊戲按鈕，若不是則關閉
        }
        //開始遊戲動畫
        public void Anim_Start()
        {
            Sequence sq = DOTween.Sequence();
            AnimTip.SetActive(true);
            AnimStart.SetActive(true);
            sq.Append(AnimTip.transform.DOScaleY(1, 0.2f)).InsertCallback(1.0f, () => Anim_Close(AnimStart));
            AudioManager.Instance.PlaySound("Ding_CardStrat", 1.0f);
        }
        //搖動提示動畫
        bool isFirst = false;
        public void Anim_ShakeAndTapTip()
        {
            Sequence sq = DOTween.Sequence();
            AnimTip.SetActive(true);
            AnimShake.SetActive(true);

            if (PlayerSit[0].GameCount == 0)
            {
                if (!isFirst)
                {
                    isFirst = true;
                    AnimTapTip.SetActive(true);
                }
            }
            else AnimTapTip.SetActive(false);

            sq.Append(AnimTip.transform.DOScaleY(1, 0.2f));
        }
        //輪到你回合動畫
        public void Anim_YourTurn()
        {
            Sequence sq = DOTween.Sequence();
            AnimTip.SetActive(true);
            AnimTurn.SetActive(true);
            AnimTurn.transform.localScale = Vector3.zero;
            AnimTurn.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
            sq.Append(AnimTip.transform.DOScaleY(1, 0.2f)).InsertCallback(2.0f, () => Anim_Close(AnimTurn));
            AudioManager.Instance.PlaySound("Ding_CardStrat", 1.0f);
            PlayerSit[0].Open_Timeup();
        }
        //上滑提示
        public void Anim_Tap()
        {
            AnimTap.SetActive(true);
        }
        //關閉提示動畫
        public void Anim_Close(GameObject anim)
        {
            AnimTip.SetActive(false);
            anim.SetActive(false);
        }
        public void Anim_AllClose()
        {
            AnimWait.SetActive(false);
            AnimStart.SetActive(false);
            AnimShake.SetActive(false);
            AnimTurn.SetActive(false);
            AnimTip.SetActive(false);
        }

        //搖骰盅
        public void Anim_ShakeDiceCup()
        {
            if (!BoastDataManager.Instance.IsShaking)
            {
                Anim_Close(AnimShake);
                AudioManager.Instance.PlaySound("Shake", 1.0f);
                BoastDataManager.Instance.IsShaking = true;
                PlayerSit[0].CupsBotton.SetActive(false);
                PlayerSit[0].Cups.SetActive(false);
                PlayerSit[0].CupsDice.SetActive(false);
                PlayerSit[0].DiceCupSpine.gameObject.SetActive(true);
                PlayerSit[0].Close_Timeup();
                PlayerSit[0].DiceCupSpine.AnimationName = "BWag";
                Invoke("Anim_ShakingEnd", 1.4f);
            }
        }
        //搖其他玩家的骰盅
        public void Anim_ShakeOtherDiceCup()
        {
            for (int i = 1; i < 6; i++)
            {
                if (PlayerSit[i].CupsBotton.activeSelf)
                {
                    PlayerSit[i].CupsBotton.SetActive(false);
                    PlayerSit[i].Cups.SetActive(false);
                    PlayerSit[i].CupsDice.SetActive(false);
                    PlayerSit[i].DiceCupSpine.gameObject.SetActive(true);
                    PlayerSit[i].DiceCupSpine.AnimationName = "BWag";
                    AudioManager.Instance.PlaySound("Shake", 1.0f);
                }
            }
        }
        private void Anim_ShakingEnd()
        {
            //BoastDataManager.Instance.IsShaking = false;
            Anim_DiceCupSpineInit();
            PlayerSit[0].CupsBotton.SetActive(true);
            PlayerSit[0].Cups.SetActive(true);
            PlayerSit[0].DiceCupSpine.gameObject.SetActive(false);
            send_Vote(1);
        }
        //搖完等待開牌中
        public void Anim_StandDiceCup()
        {
            BoastDataManager.Instance.IsShaking = true;
            RollDice.Instance.Roll();
            PlayerSit[0].CupsBotton.SetActive(true);
            PlayerSit[0].Cups.SetActive(true);
            PlayerSit[0].CupsDice.SetActive(true);
            PlayerSit[0].DiceCupSpine.gameObject.SetActive(false);
            Anim_IsGodCard(0);
        }
        //開骰盅
        public void Anim_OpenDiceCup()
        {
            if (PlayerSit[0].CupsBotton.activeSelf)
            {
                PlayerSit[0].CupsBotton.SetActive(true);
                PlayerSit[0].Cups.SetActive(false);
                PlayerSit[0].DiceCupSpine.gameObject.SetActive(true);
                PlayerSit[0].DiceCupSpine.AnimationName = "BOpen";
            }

            for (int i = 1; i < 6; i++)
            {
                if (PlayerSit[i].PlayersDiceCup.activeSelf)
                {
                    PlayerSit[i].CupsBotton.SetActive(true);
                    PlayerSit[i].CupsDice.SetActive(true);
                    PlayerSit[i].Cups.SetActive(false);
                    PlayerSit[i].DiceCupSpine.gameObject.SetActive(true);
                    PlayerSit[i].DiceCupSpine.AnimationName = "BOpen";
                }
            }

            RollDice.Instance.DiceDark();
        }
        //骰盅動畫重置
        private void Anim_DiceCupSpineInit()
        {
            for (int i = 0; i < 6; i++)
            {
                PlayerSit[i].DiceCupSpine.AnimationName = "BStop";
            }
        }

        //跳抓
        public void Anim_JumpDoubleAnim()
        {
            Sequence seq = DOTween.Sequence();
            JumpDouble.SetActive(true);
            JumpDouble.transform.localScale = Vector3.zero;
            JumpDouble.transform.localPosition = Vector3.zero;
            seq.Append(JumpDouble.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack)).OnComplete(() => Invoke("Anim_JumpDoubleClose", 2.0f));
               //.Insert(0.7f, JumpDouble.transform.DOLocalMove(new Vector3(PlayerSit[index].transform.localPosition.x, PlayerSit[index].transform.localPosition.y - 180, 0), 0.5f))
               //.Insert(0.7f, JumpDouble.transform.DOScale(new Vector3(0.6f, 0.6f, 0.6f), 0.5f))
               //.OnComplete(() => Invoke("Anim_JumpDoubleClose", 3.0f));
        }
        private void Anim_JumpDoubleClose()
        {
            JumpDouble.SetActive(false);
        }

        //獲利兩倍
        public void Anim_DoubleMoney(int index)
        {
            PlayerSit[index].Set_Double();
        }

        //是否有天牌
        public void Anim_IsGodCard(int index)
        {
            //記錄六種點數哪些是有拿到的
            List<bool> diceHave = new List<bool> {false, false, false, false, false, false };

            if (index == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    diceHave[int.Parse(RollDice.Instance.MyDice[i].spriteName) -1] = true;
                }
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    diceHave[int.Parse(RollDice.Instance.OtherDice[index - 1][i].spriteName) -1] = true;
                }
            }

            //從哪一個點數開始判別
            int iStart = 0;
            int diceHaveNum = 0;

            //如果1已經被喊，就必須判斷1點否有拿
            if (BoastDataManager.Instance.IsOne) iStart = 0;
            else iStart = 1;

            for (int i = iStart; i < 6; i++)
            {
                if (diceHave[i] == true) diceHaveNum++;
                if (diceHaveNum > 1) return;
            }

            //如果拿到五顆同樣點數骰子
            AudioManager.Instance.PlaySound("Sky2", 1.0f);
            GodCard[index].SetActive(true);
            GodCard[index].transform.localScale = Vector3.zero;
            GodCard[index].transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 1.0f).SetEase(Ease.OutBack).OnComplete(() => Anim_GodCardClose(index));
        }
        private void Anim_GodCardClose(int index)
        {
            GodCard[index].transform.DOScale(Vector3.zero, 1.0f).SetEase(Ease.InBack).SetDelay(2.0f).OnComplete(() => GodCard[index].SetActive(false));
        }

        //勝利
        public void Anim_Winer()
        {
            int index = BoastDataManager.Instance.winIndex;
            Win.SetActive(true);
            Win.transform.localPosition = new Vector3(PlayerSit[index].transform.localPosition.x, PlayerSit[index].transform.localPosition.y + 150, 0);
            Win.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            Win.transform.DOScale(Vector3.one, 0.1f);
            Win.transform.DOLocalMoveY(PlayerSit[index].transform.localPosition.y -50 , 0.2f).SetDelay(0.5f);
            AudioManager.Instance.PlaySound("Win2", 1.0f);

            if (BoastDataManager.Instance.gameInfo.game_result.is_double)
            {
                PlayerSit[index].Set_Double();
                PlayerSit[index].Set_AddBalance(BoastDataManager.Instance.game_result.result);
            }
            else PlayerSit[index].Set_AddBalance(BoastDataManager.Instance.game_result.result);

            Coin.MoveDuration = 1.5f;
            Coin.StartPos = Coin.Pos[BoastDataManager.Instance.loseIndex];
            Coin.EndPos = Coin.Pos[BoastDataManager.Instance.winIndex];
            Coin.DoCreatCoin(BoastDataManager.Instance.game_result.result);


            Invoke("Anim_CloseWinLoseDrew", 4.0f);
            Utilities.Log("贏家座位為" + index);
        }
        //失敗
        public void Anim_Loser()
        {
            int index = BoastDataManager.Instance.loseIndex;
            UISprite LoseSprite = Lose.transform.GetChild(0).GetComponent<UISprite>();
            LoseSprite.spriteName = "Lose";
            Lose.SetActive(true);
            Lose.transform.localPosition = new Vector3(PlayerSit[index].transform.localPosition.x, PlayerSit[index].transform.localPosition.y + 150, 0);
            Lose.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            Lose.transform.DOScale(Vector3.one, 0.1f);
            Lose.transform.DOLocalMoveY(PlayerSit[index].transform.localPosition.y -50 , 0.2f).SetDelay(0.5f).OnComplete(() => LoseSprite.spriteName = "Lose2");
            AudioManager.Instance.PlaySound("Lose3", 1.0f);

            PlayerSit[index].Set_SubBalance(BoastDataManager.Instance.game_result.result);

            Utilities.Log("輸家座位為" + index);
        }
        //平手
        public void Anim_Drewer()
        {
            Drew[0].SetActive(true);
            Drew[0].transform.localScale = Vector3.zero;
            Drew[0].transform.DOScale(new Vector3(3, 3, 3), 0.3f).SetEase(Ease.OutBack);
            Drew[1].SetActive(true);
            Drew[1].transform.localScale = Vector3.zero;
            Drew[1].transform.DOScale(new Vector3(1.8f, 1.8f, 1.8f), 0.3f).SetEase(Ease.OutBack);
            AudioManager.Instance.PlaySound("Drew3", 1.0f);
            Invoke("Anim_CloseWinLoseDrew", 4.0f);
        }
        //關閉勝利失敗平手特效
        public void Anim_CloseWinLoseDrew()
        {
            if (Win.activeSelf) Win.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() => Win.SetActive(false));
            if (Lose.activeSelf) Lose.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() => Lose.SetActive(false));
            if (Drew[0].activeSelf) Drew[0].transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() => Drew[0].SetActive(false));
            if (Drew[1].activeSelf) Drew[1].transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() => Drew[1].SetActive(false));

            if (BoastDataManager.Instance.gameInfo.play_mode == 1 && BoastDataManager.Instance.game_result.is_double)
            {
                send_Vote(3);
            }
            else if (BoastDataManager.Instance.gameInfo.play_mode != 1)
            {
                send_Vote(3);
            }
        }

        #endregion

        #region 介面

        //清空桌子
        public void UI_Clean()
        {
            Utilities.Log("清空");

            //重整動畫
            Anim_DiceCupSpineInit();
            Anim_AllClose();

            //重整UI
            UI_ClosePoolDicePointNum();
            UI_CatchDiceCupsClose();
            UI_PlayerTipClose();
            UI_OneIsCalledClose();

            for (int i = 0; i < 6; i++)
            {
                PlayerSit[i].gameObject.SetActive(false);
                PlayerSit[i].Close_Chating();
                PlayerSit[i].Close_Timeup();
            }

            //重整骰子
            RollDice.Instance.DiceWhite();

            //選擇骰子數量重整
            BoastChoseForm.Instance.SwitchCalledChoseNumber(0, 30);

            BoastChoseForm.Instance.Close_DiceNum();
            BoastChoseForm.Instance.Close_DicePoint();

            BoastDataManager.Instance.IsShaking = false;
            BoastDataManager.Instance.IsCatch = false;

            //請求快照
            send_GetSnapShot();

            GlobalEventManager.Instance.SwitchWaitingLoading(true);
        }

        //更新UI
        public void UI_Update(GameInfo startGame)
        {
            //更新座位資訊
            for (int i = 0; i < startGame.seats.Count; i++)
            {
                if (startGame.seats[i].status != 0)
                {
                    UI_SeatPos(i, PlayerPos(startGame.seats[i].pos));
                }
            }


            if (startGame.play_mode == 1) UI_TableNineCup();

            //目前遊戲狀態
            switch (startGame.status)
            {
                    //等待
                case 0:
                    Utilities.Log("等待狀態");
                    Anim_Wait();
                    break;

                    //準備遊戲
                case 1:
                    UI_PreparGame(startGame);
                    break;

                    //開始遊戲
                case 2:
                    UI_StartGame(startGame);
                    break;

                //結果
                case 3:
                    //UI_Result(startGame.game_result);
                    break;

                //下一局
                case 4:
                    Utilities.Log("下一局");
                    break;

            }
        }

        //準備遊戲
        public void UI_PreparGame(GameInfo startGame)
        {
            Utilities.Log("準備狀態");

            //重整動畫
            Anim_DiceCupSpineInit();
            Anim_ShakeOtherDiceCup();
            

            //重整UI
            UI_ClosePoolDicePointNum();
            UI_CatchDiceCupsClose();
            UI_PlayerTipClose();
            UI_OneIsCalledClose();

            //重整骰子
            RollDice.Instance.DiceWhite();

            Anim_Close(AnimWait);
            Anim_ShakeAndTapTip();
            PlayerSit[0].Open_Timeup();

            //int playNum = 0;
            BoastChoseForm.Instance.minNum = 0;
            BoastChoseForm.Instance.maxNum = 0;
            //如果此時玩家在這位子上，重整骰盅
            for (int i = 0; i < 6; i++)
            {
                if (BoastDataManager.Instance.gameInfo.seats[i].status == 2)
                {
                    BoastChoseForm.Instance.minNum += 1;
                    BoastChoseForm.Instance.maxNum += 5;
                    PlayerSit[PlayerPos(BoastDataManager.Instance.gameInfo.seats[i].pos)].PlayersDiceCup.SetActive(true);
                    PlayerSit[PlayerPos(BoastDataManager.Instance.gameInfo.seats[i].pos)].Cups.SetActive(true);
                }
            }

            //選擇骰子數量重整
            BoastChoseForm.Instance.SwitchCalledChoseNumber(BoastChoseForm.Instance.minNum, BoastChoseForm.Instance.maxNum);
            BoastChoseForm.Instance.SwitchCalledChosePoint(new List<bool> { true, true, true, true, true, true,});
            BoastChoseForm.Instance.Close_DiceNum();
            BoastChoseForm.Instance.Close_DicePoint();
            BoastChoseForm.Instance.ChoseNumLabel.text = BoastChoseForm.Instance.minNum.ToString();
            BoastChoseForm.Instance.ChoseDiceSprite.spriteName = "N2";
            BoastDataManager.Instance.ChoseDiceNumber = BoastChoseForm.Instance.minNum;
            BoastDataManager.Instance.ChoseDicePoint = 2;

            //我的骰子
            List<int> myDice = new List<int>();
            for (int i = 0; i < 5; i++)
            {
                if (BoastDataManager.Instance.gameInfo.seats[BoastDataManager.Instance.MyPos].status == 2)
                {
                    if (BoastDataManager.Instance.self_pais[i].point >= 1 && BoastDataManager.Instance.self_pais[i].point <= 6)
                    {
                        myDice.Add(BoastDataManager.Instance.self_pais[i].point);
                    }
                }
            }
            RollDice.Instance.ResultNumber = myDice;

            BoastDataManager.Instance.IsStart = false;
            BoastDataManager.Instance.IsShaking = false;
            BoastDataManager.Instance.IsCatch = false;
        }

        //開始遊戲
        public void UI_StartGame(GameInfo startGame)
        {
            Utilities.Log("開始狀態");

            BoastDataManager.Instance.IsStart = true;
            BoastDataManager.Instance.IsShaking = true;
            BoastDataManager.Instance.IsCatch = false;

            //喊點最小最大數量
            BoastChoseForm.Instance.minNum = 0;
            BoastChoseForm.Instance.maxNum = 0;
            for (int i = 0; i < 6; i++)
            {
                if (BoastDataManager.Instance.gameInfo.seats[i].status == 2)
                {
                    BoastChoseForm.Instance.minNum += 1;
                    BoastChoseForm.Instance.maxNum += 5;
                }
            }

            BoastChoseForm.Instance.SwitchCalledChoseNumber(BoastChoseForm.Instance.minNum, BoastChoseForm.Instance.maxNum);
            BoastChoseForm.Instance.ChoseNumLabel.text = BoastChoseForm.Instance.minNum.ToString();

            //更新底池及頭像旁的喊點提式框
            if (startGame.bet_point > 0 && startGame.bet_point <= 6)
            {
                //更新底池顯示以及頭像旁顯示已喊數量
                UI_PoolDicePointNum(startGame.bet_num, startGame.bet_point);
                UI_PlayerTipOpen(PlayerPos(startGame.bet_pos), startGame.bet_num, startGame.bet_point);

                //選擇視窗更新
                BoastChoseForm.Instance.ChoseNumLabel.text = startGame.bet_num.ToString();
                BoastDataManager.Instance.ChoseDiceNumber = int.Parse(BoastChoseForm.Instance.ChoseNumLabel.text);
                ChangeDicePoint(BoastChoseForm.Instance.ChoseDiceSprite, "N" + startGame.bet_point);
                BoastDataManager.Instance.ChoseDicePoint = int.Parse(BoastChoseForm.Instance.ChoseDiceSprite.spriteName.Substring(1, 1));
            }
            else
            {
                //關閉底池顯示目前已喊數量
                UI_ClosePoolDicePointNum();
            }
        }

        //結算
        public void UI_Result(GameResult gameResult)
        {
            Utilities.Log("結算狀態");

            BoastDataManager.Instance.IsCatch = false;

            BoastDataManager.Instance.gameInfo.status = 3;

            BoastDataManager.Instance.winIndex = PlayerPos(PlayerIDPos(gameResult.win_id));
            BoastDataManager.Instance.loseIndex = PlayerPos(PlayerIDPos(gameResult.lost_id));

            NotPlay();

            //更新其他玩骰子
            List<int> otherDice;
            for (int i = 0; i < 6; i++)
            {
                //如果這位玩家是遊玩的狀態
                if (BoastDataManager.Instance.gameInfo.seats[i].status == 2)
                {
                    //而且不是我自己
                    if (BoastDataManager.Instance.gameInfo.seats[i].player_info.id != DataManager.PlayerInfo.id)
                    {
                        otherDice = new List<int>();
                        for (int d = 0; d < 5; d++)
                        {
                            otherDice.Add(gameResult.player_pais[BoastDataManager.Instance.gameInfo.seats[i].player_info.id][d].point);
                        }

                        RollDice.Instance.Updata_OtherDice(PlayerPos(BoastDataManager.Instance.gameInfo.seats[i].pos), otherDice);
                    }
                }
            }

            //誰抓
            UI_CatchDiceCups(PlayerPos(PlayerIDPos(gameResult.bet_id)));
            UI_PlayerTipClose();
            Anim_Close(AnimTap);

            PlayerSit[PlayerPos(BoastDataManager.Instance.called.pos)].Close_Timeup();

            //跳抓，如果不是在單挑模式下
            float delay = 1.5f;
            if (BoastDataManager.Instance.gameInfo.play_mode != 1 && gameResult.is_double)
            {
                delay = 3.0f;
                Anim_JumpDoubleAnim();
            }
            
            Invoke("UI_CatchDiceCupsClose", delay);
            Invoke("Anim_OpenDiceCup", delay);
            Invoke("Anim_Winer", delay + 2.0f);
            Invoke("Anim_Loser", delay + 2.0f);

            //如果單挑模式下，且尚未聽牌連線
            if (BoastDataManager.Instance.gameInfo.play_mode == 1 && !gameResult.is_double)
            {
                if (gameResult.win_id == DataManager.PlayerInfo.id)
                {
                    Invoke("Form_OpenNineCup", delay + 4.0f);
                }
            }
        }

        //進入座位
        public void UI_SeatPos(int snapPlayerIndex, int index)
        {
            //座位頭像
            PlayerSit[index].gameObject.SetActive(true);
            //名字
            PlayerSit[index].Set_Name(BoastDataManager.Instance.gameInfo.seats[snapPlayerIndex].player_info.name);
            //玩家大頭貼
            PlayerSit[index].Set_Head(BoastDataManager.Instance.gameInfo.seats[snapPlayerIndex].player_info.id);
            //玩家金錢
            PlayerSit[index].Set_Balance(Convert.ToInt64(BoastDataManager.Instance.gameInfo.seats[snapPlayerIndex].player_info.balance));
            //ID
            PlayerSit[index].PlayerID = BoastDataManager.Instance.gameInfo.seats[snapPlayerIndex].player_info.id;
            //遊玩次數
            PlayerSit[index].GameCount = BoastDataManager.Instance.gameInfo.seats[snapPlayerIndex].player_info.game_count;
            //贏得次數
            PlayerSit[index].WinCount = BoastDataManager.Instance.gameInfo.seats[snapPlayerIndex].player_info.win_count;
            //開牌次數
            PlayerSit[index].OpenCount = BoastDataManager.Instance.gameInfo.seats[snapPlayerIndex].player_info.open_card_count;

            if (BoastDataManager.Instance.gameInfo.seats[snapPlayerIndex].status == 2)
            {
                PlayerSit[index].PlayersDiceCup.SetActive(true);
                PlayerSit[index].Cups.SetActive(true);
            }
        }

        //離開座位test
        public void UI_SeatPosExit(int index)
        {
            PlayerSit[index].gameObject.SetActive(false);
            PlayerSit[index].PlayersDiceCup.SetActive(false);

            //有玩家離開後，如果遊戲沒有開始，就進入等待
            if (BoastDataManager.Instance.gameInfo.status != 1 || BoastDataManager.Instance.gameInfo.status != 2)
            {
                if (!AnimWait.activeSelf)
                {
                    Anim_Wait();

                    if (NineCupFormPanel.activeSelf) Form_CloseNineCup();
                }
            }
        }

        //房主圖示
        public void UI_RoomOwner(int index)
        {
            for (int i = 0; i < 6; i++)
            {
                if (i == index)
                {
                    PlayerSit[index].Set_RoomOwner();
                }
                else PlayerSit[index].RoomOwner.SetActive(false);
            }
        }

        //更新玩家金錢
        public void UI_PlayerBalance(int index, long balance)
        {
            PlayerSit[index].Set_Balance(balance);
        }

        //增加金錢
        public void UI_AddBalaceNum(int index, long balance) 
        {
            PlayerSit[index].Set_AddBalance(balance);
        }       

        //扣除金錢
        public void UI_SubBalaceNum(int index, long balance)
        {
            PlayerSit[index].Set_SubBalance(balance);
        }

        //目前底池金額
        public void UI_PoolMoneyInfo(int balance)
        {
            PoolMoney.text = "底池:" + balance.ToString();
        }

        //目前骰子點數及數量
        public void UI_PoolDicePointNum(int diceNum, int dicePoint)
        {
            PoolDiceInfo.SetActive(true);
            //骰子點數
            ChangeDicePoint(PoolDice, "N" + dicePoint);
            //骰子數量
            PoolDiceNum.text = diceNum + " 個";
        }
        public void UI_ClosePoolDicePointNum()
        {
            PoolDiceInfo.SetActive(false);
        }

        //各個玩家喊的點數
        public void UI_PlayerTipOpen(int index, int diceNum, int dicePoint)
        {
            //坐位上如果人已離開
            if (!PlayerSit[index].gameObject.activeSelf) return;
            //開啟提示
            if (!PlayerSit[index].DiceTip.activeSelf) PlayerSit[index].DiceTip.SetActive(true);
            //喊幾顆
            PlayerSit[index].DiceTipNumLabel.text = diceNum.ToString();
            //喊多少點
            ChangeDicePoint(PlayerSit[index].DiceTipPoint, "N" + dicePoint);

            //開關其他玩家提示的遮罩
            for (int i = 0; i < 6; i++)
            {
                if (i == index) PlayerSit[i].DiceTipMask.SetActive(false);
                else PlayerSit[i].DiceTipMask.SetActive(true);
            }

            //提示框動畫
            PlayerSit[index].DiceTip.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.2f).SetLoops(2, LoopType.Yoyo);

        }
        public void UI_PlayerTipClose()
        {
            for (int i = 0; i < 6; i++)
            {
                if (PlayerSit[i].DiceTip.activeSelf) PlayerSit[i].DiceTip.SetActive(false);
            }
        }

        //已喊
        public void UI_OneIsCalled()
        {
            BoastDataManager.Instance.IsOne = true;
            CalledTip.SetActive(true);
            CalledTip.transform.localScale = Vector3.zero;
            CalledTip.transform.DOScale(new Vector3(1.0f, 1.0f, 1.0f), 0.2f).SetEase(Ease.OutBack);
        }
        public void UI_OneIsCalledClose()
        {
            BoastDataManager.Instance.IsOne = false;
            CalledTip.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() => CalledTip.SetActive(false));
        }

        //必須大於上家所喊
        public void UI_NeedToBig()
        {
            NeedBig.SetActive(true);
            NeedBig.transform.DOScaleX(1, 0.2f).OnComplete(() => Invoke("UI_NeedTobigClose", 2.5f));
        }
        private void UI_NeedTobigClose()
        {
            NeedBig.transform.DOScaleX(0, 0.2f).OnComplete(() => NeedBig.SetActive(false));
        }

        //喊開
        public void UI_CatchDiceCups(int index)
        {
            PlayerSit[index].Open_Cup();
        }
        public void UI_CatchDiceCupsClose()
        {
            for (int i = 0; i < 6; i++)
            {
                PlayerSit[i].Close_Cup();
            }
        }

        //更新桌面的九宮格
        public void UI_TableNineCup()
        {
            List<int> nine = BoastDataManager.Instance.gameInfo.cups;

            if (!NineCupTable.activeSelf) NineCupTable.SetActive(true);
            for (int i = 0; i < 9; i++)
            {
                if (nine[i] == 0) NineCupWaterTable[i].gameObject.SetActive(false);
                else if (nine[i] == DataManager.PlayerInfo.id)
                {
                    NineCupWaterTable[i].gameObject.SetActive(true);
                    NineCupWaterTable[i].spriteName = "Red";
                }
                else
                {
                    NineCupWaterTable[i].gameObject.SetActive(true);
                    NineCupWaterTable[i].spriteName = "Blue";
                }
            }
        }
        public void UI_TableNineCupClose()
        {
            if (NineCupTable.activeSelf) NineCupTable.SetActive(false);
        }

        //表情
        public void UI_DoFace(int index, int faceNumber)
        {
            switch (faceNumber)
            {
                case 1:
                    face[index].spriteName = "Face01";
                    break;
                case 2:
                    face[index].spriteName = "Face02";
                    break;
                case 3:
                    face[index].spriteName = "Face03";
                    break;
                case 4:
                    face[index].spriteName = "Face04";
                    break;
                case 5:
                    face[index].spriteName = "Face05";
                    break;
                case 6:
                    face[index].spriteName = "Face06";
                    break;
            }

            Sequence seq = DOTween.Sequence();
            //如果該位置的人正在做表情，刪除原先表情的DOTween
            if (face[index].gameObject.activeSelf)
            {
                DOTween.Kill(index);
            }
            face[index].gameObject.transform.localScale = Vector3.zero;
            face[index].gameObject.SetActive(true);
            seq.SetId(index)
               .Append(face[index].gameObject.transform.DOScale(new Vector3(1, 1, 1), 0.2f).SetEase(Ease.OutBack))
               .AppendInterval(2)
               .Append(face[index].gameObject.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack))
               .OnComplete(() => face[index].gameObject.SetActive(false));
        }

        #endregion

        #region 各種視窗

        //儲值視窗
        public void Form_OpenDeposit()
        {
            AudioManager.Instance.PlaySound("Button1", 1.0f);
            DepositForm.UpdateBonusSell(DataManager.SysSetting.IAPDisCount);
            DepositForm.Show();
        } 
        public void Form_CloseDeposit()
        {
            AudioManager.Instance.PlaySound("Button1", 16);
            DepositForm.Close();
        }

        //聊視窗
        public void Form_OpenChat()
        {
            AudioManager.Instance.PlaySound("Button1", 1.0f);
        }

        //設定視窗
        public void Form_OpenSetting()
        {
            AudioManager.Instance.PlaySound("Button1", 1.0f);
            SettingForm.Show();
        }
        public void Form_CloseSetting()
        {
            AudioManager.Instance.PlaySound("Button1", 16);
            SettingForm.Close();
        }

        //離開視窗
        public void Form_OpenExit()
        {
            AudioManager.Instance.PlaySound("Button1", 1.0f);
            ExitMask.SetActive(true);
            ExitForm.SetActive(true);
            ExitForm.transform.DOScale(new Vector3(1, 1, 1), 0.2f).SetEase(Ease.OutBack);
        }
        public void Form_CloseExit()
        {
            AudioManager.Instance.PlaySound("Button1", 16);
            ExitMask.SetActive(false);
            ExitForm.SetActive(false);
            ExitForm.transform.localScale = new Vector3(0, 0, 0);
        }

        //FB視窗
        public void Form_OpenFB()
        {
            AudioManager.Instance.PlaySound("Button1", 1.0f);
            FBForm.Show();
        }
        public void Form_CloseFB()
        {
            AudioManager.Instance.PlaySound("Button1", 16);
            FBForm.Close();
        }

        //玩家資訊視窗
        public void Form_OpenPlayer()
        {
            AudioManager.Instance.PlaySound("Button1", 1.0f);
            PlayerForm.Show();
        }
        public void Form_ClosePlayer()
        {
            AudioManager.Instance.PlaySound("Button1", 16);
            PlayerForm.Close();
        }

        //繼續遊戲視窗
        public void Form_OpenCon()
        {
            ConPanel.SetActive(true);
            ConForm.transform.DOScale(new Vector3(1, 1, 1), 0.2f);
        }
        public void Form_CloseCon()
        {
            ConForm.transform.DOScale(new Vector3(0, 0, 0), 0.2f).OnComplete(() => ConPanel.SetActive(false));
        }

        //開啟九宮格視窗
        public void Form_OpenNineCup()
        {
            List<int> nine = BoastDataManager.Instance.gameInfo.cups;

            BoastDataManager.Instance.IsChoseCup = false;
            NineCupFormPanel.SetActive(true);
            NineCupForm.transform.DOScale(new Vector3(1, 1, 1), 0.2f);
            AudioManager.Instance.PlaySound("NineCupStart", 1.0f);

            for (int i = 0; i < 9; i++)
            {
                if (nine[i] == 0)
                {
                    NineCupWaterForm[i].gameObject.SetActive(false);
                }
                else if (nine[i] == DataManager.PlayerInfo.id)
                {
                    NineCupWaterForm[i].gameObject.SetActive(true);
                    NineCupWaterForm[i].spriteName = "Red";
                }
                else
                {
                    NineCupWaterForm[i].gameObject.SetActive(true);
                    NineCupWaterForm[i].spriteName = "Blue";
                }
            }

            //Invoke("Auto_ChoseReady", 5.8f);
        }
        public void Form_CloseNineCup()
        {
            BoastDataManager.Instance.IsChoseCup = true;
            NineCupForm.transform.DOScale(new Vector3(0, 0, 0), 0.2f).OnComplete(() => NineCupFormPanel.SetActive(false));
        }

        #endregion

        #region 常用功能

        //實際位置轉換自己視角的相對位置(pos)
        public int PlayerPos(int realPos)
        {
            int pos;

            //以自己的位置去算出其他玩家的相對位置
            if (realPos - BoastDataManager.Instance.MyPos >= 0) pos = realPos - BoastDataManager.Instance.MyPos;
            else pos = realPos - BoastDataManager.Instance.MyPos + 6;

            //如果不是自己且又是1V1，將玩家位置擺放置對面位子
            if (pos != 0 && BoastDataManager.Instance.gameInfo.play_mode == 1) pos = 3;

            return pos;
        }

        //以ID尋找位置
        public int PlayerIDPos(int pid)
        {
            int pos = -1;

            //以pid的方式搜尋該玩家的順序在哪
            for (int i = 0; i < BoastDataManager.Instance.gameInfo.seats.Count; i++)
            {
                if (BoastDataManager.Instance.gameInfo.seats[i].player_info.id == pid)
                {
                    pos = i;
                    break;
                }
            }

            return pos;
        }

        //更換骰子點數
        public void ChangeDicePoint(UISprite dice, string spriteName)
        {
            dice.spriteName = spriteName;
        }

        //切換按鈕
        private void ChangeButton(ButtonState state)
        {
            switch (state)
            {
                case ButtonState.start:
                    PlayButton[0].SetActive(true);
                    PlayButton[1].SetActive(false);
                    PlayButton[2].SetActive(false);
                    break;

                case ButtonState.notPlay:
                    PlayButton[0].SetActive(false);
                    PlayButton[1].SetActive(true);
                    PlayButton[2].SetActive(false);
                    break;

                case ButtonState.play:
                    PlayButton[0].SetActive(false);
                    PlayButton[1].SetActive(false);
                    PlayButton[2].SetActive(true);
                    break;
            }
        }

        //自動選擇酒杯啟動
        private void Auto_ChoseReady()
        {
            BoastDataManager.Instance.IsChoseCup = true;
            Invoke("Auto_ChoseNineCup", 0.2f);
        }

        //自動選擇酒杯開始
        private void Auto_ChoseNineCup()
        {
            /*int autoIndex;
            while (true)
            {
                autoIndex = System.Random.(0, 9);
                if (!NineCupWaterForm[autoIndex].gameObject.activeSelf)
                {
                    send_ChoseNineCup(autoIndex);
                    break;
                }
            }*/
        }

        #endregion

        #region 送出訊息

        //請求快照
        public void send_GetSnapShot()
        {
            lock (locker)
            {
                Utilities.Log("請求快照");
                OperateInfo op = new OperateInfo();
                op.event_id = (int)BoastProtocol.EventCodes.EVENT_GET_SNAPSHOT;
                BoastMsgManager.Instance.SendGame(op);
            }
        }

        //送出搖完骰
        public void send_Vote(int statusInt)
        {
            OperateInfo op = new OperateInfo();
            op.event_id = (int)BoastProtocol.EventCodes.EVENT_VOTE;
            op.data.Add("is_agree", true);
            op.data.Add("status", statusInt);
            BoastMsgManager.Instance.SendGame(op);
        }

        //送出開始訊息
        private void send_Start()
        {
            lock (locker)
            {
                NotPlay();
            }
        }

        //送出喊點訊息
        private void send_DicePointAndNum()
        {
            //送出數量和點數
            lock (locker)
            {
                OperateInfo op = new OperateInfo();
                op.event_id = (int)BoastProtocol.EventCodes.EVENT_BET;
                op.data.Add("num", BoastDataManager.Instance.ChoseDiceNumber);
                op.data.Add("point", BoastDataManager.Instance.ChoseDicePoint);
                BoastMsgManager.Instance.SendGame(op);

                CancelInvoke("Anim_YourTurn");

                NotPlay();
                PlayerSit[0].Close_Timeup();
                BoastChoseForm.Instance.Close_DiceNum();
                BoastChoseForm.Instance.Close_DicePoint();
            }
        }

        //送出抓人訊息
        private void send_Catch()
        {
            lock (locker)
            {
                OperateInfo op = new OperateInfo();
                op.event_id = (int)BoastProtocol.EventCodes.EVENT_HU_PAI;
                BoastMsgManager.Instance.SendGame(op);
                BoastDataManager.Instance.IsCatch = false;
            }
        }

        //送出繼續訊息
        private void send_Con()
        {
            lock (locker)
            {
               
            }
        }

        //送出不繼續訊息
        private void send_NoCon()
        {
            lock (locker)
            {

            }
        }

        //送出選擇酒杯訊息
        private void send_ChoseNineCup(int index)
        {
            lock (locker)
            {
                OperateInfo op = new OperateInfo();
                op.event_id = (int)BoastProtocol.EventCodes.EVENT_PENG_PAI;
                op.data.Add("pos", index);
                BoastMsgManager.Instance.SendGame(op);

                NineCupWaterForm[index].gameObject.SetActive(true);

                NineCupWaterForm[index].spriteName = "Red";
                NineCupWaterTable[index].spriteName = "Red";
            }
        }

        //送出表情符號
        public void send_ChoseFace(GameObject faceName)
        {
            int faceNumber;
            faceNumber = int.Parse(faceName.name);

            /*lock (locker)
            {
                OperateInfo op = new OperateInfo();
                op.event_id = (int)EventCodes.EVENT_EMOJI;
                op.data.Add(ParamsCodes.KEY_CODE.ToString("D"), faceNumber);
                BoastMsgManager.Instance.SendGame(op);
            }*/

            UI_DoFace(0, faceNumber);
        }

        #endregion
    }
}