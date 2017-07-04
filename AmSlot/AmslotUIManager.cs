using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SlotSystem_Loger;
using System;
using System.Collections.Generic;
using CSFramework;
using NGUI_ExtendByLoger;
using Spine.Unity;
using CoinEffect_SongWei;
using DG.Tweening;
using Loger_System;
using BaseProtocol;

namespace Amslot_SW
{ 
    public class AmslotUIManager : ISlotUIManager
    {
        #region 變數
        public static AmslotUIManager Instance;
        //CustomSlot遊戲腳本
        public CustomSlot slot;

        public BarrageManager Barrage;

        //神獸簡介
        public IntroFormManager StartPage;
        //說明頁面
        public ExplainFormManager Explain;
        //Bet腳本
        public BetBoxManager bbm;
        //儲值頁面
        public DepositFormManager DepositForm;
        //設定頁面
        public SettingFormManager SettingForm;
        //FB上傳
        public FBFormManager FBForm;
        //綁定頁面
        public AccBindManager AccBindForm;
        //玩家資訊
        public PlayerFormManager PlayerForm;
        public InformationFormManager InfomationForm;

        //背景光文字
        [Space(10)]
        [SerializeField]private UIPanel mask;
        //Mask初始位置
        float offsetY = -600;

        //FreeGame頁面
        [Space(10)]
        public GameObject[] freeGameForm;
        //FreeGameFlag
        public FlagSpine[] flagSpine;
        //顯示多少次免費遊戲
        [SerializeField]private UILabel FGNumLabel;
        //免費遊戲時的遮罩
        public GameObject[] FreeGameMask;
        //免費遊戲結束UI
        public GameObject FreeResult;
        //免費遊戲結束遮罩
        public GameObject FreeGameOverMask;
        //免費遊戲贏得總額
        public NGUILabelNumberAnim FreeGameWinTotal;
        //免費遊戲總共次數
        public NGUILabelNumberAnim FreeGameSpinNum;
        //免費遊戲總額金幣特效
        public CreatCoinRain CoinRain;

        //bigwin特效
        [Space(10)]
        public GameObject BigWin;
        public BigwinSpine BigSpine;
        //bigwin金額
        public NGUILabelNumberAnim bigWinMoney;
        //BigWin遮罩
        public GameObject BigWinMask;
        //BIGWIN時噴錢
        public ParticleSystem Coinshot;
        //領取按鈕
        public GameObject TakeButton;

        //龍珠
        [Space(10)]
        public GameObject[] dragonBall;
        //虎珠
        public GameObject[] tigerBall;
        //龍珠錢幣噴發
        public CreatCoin dragonCoinShot;
        //虎珠錢幣噴發
        public CreatCoin tigerCoinShot;
        //彩金Spine
        public JackPotSpine jackpot;
        //彩金金額
        public NGUILabelNumberAnim jackpotLabel;

        //期待框
        [Space(10)]
        public GameObject fireReel;
        //錢幣動畫
        public GameObject moneyAnim;
        public List<GameObject> tempMoneyAnim;
        //期待框位置
        public int[] rireReelPos;
        //兩旁火焰特效
        public ParticleSystem[] fireSide;

        //開始按鈕開始字樣
        [Space(10)]
        public GameObject startFont;
        //開始按鈕停止字樣
        public GameObject stopFont;

        //玩家金額
        [Space(15)]
        [SerializeField]private NGUILabelNumberAnim balanceLabel;
        //贏得金額
        public NGUILabelNumberAnim winLabel;
        public NGUILabelNumberAnim winLabel2;
        //彩金金額
        public NGUILabelNumberAnim ChiaJinLabel;
        //FreeGame次數
        public UILabel freeLabel;
        //機台號碼
        public UILabel slotID;
        public CreatCoin winCoin;

        //上排資訊
        public UILabel playerName;
        public UITexture PlayerHead;

        [Space(10)]
        //離開
        public GameObject ExitForm;
        //遮罩
        public GameObject ExitMask;
        //頭像
        public HeadPhotoManager head;
        //中獎Symbol
        public GameObject Hit;
        List<GameObject> hitList = new List<GameObject>();

        //重置圖案位置
        List<float> PosList= new List<float> { -2475, -2700, -225, -450, -675, -900, -1125, -1350, -1575, -1800, -2025, -2250 };

        private object locker = new object();

        #endregion

        private void Awake()
        {
            Instance = this;

            AccBindForm = GlobalFormManager.Instance.GetAccForm();
            FBForm = GlobalFormManager.Instance.GetFBForm();
            DepositForm = GlobalFormManager.Instance.GetDepositForm();
            SettingForm = GlobalFormManager.Instance.GetSettingForm();

            SettingForm.isBackToLobby = true;
            SettingForm.leaveMsg = "您確定要回到大廳嗎?";
        }

        private void OnDestroy()
        {
            DepositForm.OnSendPrice -= DepositForm_OnSendPrice;
            AccBindForm.OnSendBinding -= AccBind_SendBinding;
            AccBindForm.OnSendGetSms -= AccBind_SendGetSms;
            InfomationForm.OnBindingFB -= AccBind_SendGetSms;

            if (Instance != null)
            {
                Instance = null;
            }
        }

        private void Start()
        {
            AB_SaveAndLoad.Instance.UnLoadAssetBundle(false);
            //訂閱
            DepositForm.OnSendPrice += DepositForm_OnSendPrice;
            AccBindForm.OnSendBinding += AccBind_SendBinding;
            AccBindForm.OnSendGetSms += AccBind_SendGetSms;
            InfomationForm.OnBindingFB += AccBind_SendGetSms;

            SettingForm.OnBackToLobby = SettingForm_OnBackToLobby;

            //請求快照
            SendGetSnapShot();

            //中獎圖案會跳到其他父物件的開關關掉，以便控制一起亮
            slot.effects.changeParentOfHitSymbol = false;

            //開啟神獸簡介
            OpenIntroGameForm();

            //背景音樂放大
            BGMSevVol(true);

            //每X秒要玩家金額、龍虎珠資料開啟
            StartCoroutine("PlayerBalanceTime");
            StartCoroutine("DragonBallTime");
            StartCoroutine("TigerBallTime");

            Barrage.Show();
        }
        //訂閱儲值
        private void DepositForm_OnSendPrice(OperateInfo op)
        {
            AmslotMsgManager.Instance.SendLobby(op);
        }
        //訂閱送出簡訊
        private void AccBind_SendBinding(OperateInfo op)
        {
            AmslotMsgManager.Instance.SendLobby(op);
        }
        //訂閱確認
        private void AccBind_SendGetSms(OperateInfo op)
        {
            AmslotMsgManager.Instance.SendLobby(op);
        }
        //訂閱返回大廳
        private void SettingForm_OnBackToLobby(object sender, System.EventArgs e)
        {
            OpenExitForm();
        }

        //請求快照
        public void SendGetSnapShot()
        {
            lock (locker)
            {
                OperateInfo op = new OperateInfo();
                op.event_id = (int)AMSlotProtocol.EventCodes.EVENT_SNAPSHOT;
                AmslotMsgManager.Instance.SendGame(op);
            }
        }
        //檢查是否有免費遊戲
        public void StartCalFreeGame()
        {
            //偵測玩家是否有免費遊戲，且未選擇
            if (AmslotDataManager.Instance.SnapShot.play.beginTimes && !AmslotDataManager.Instance.SnapShot.play.endTimes)
            {
                OpenFreeGameForm();
            }
            //玩家有免費遊戲，且已選擇模式，直接進入免費遊戲轉動
            else if(AmslotDataManager.Instance.SnapShot.play.freeTimes > 0)
            {
                FGNumLabel.text = AmslotDataManager.Instance.SnapShot.play.freeTimes.ToString();
                delayOpenFreeGameNum();
            }
            //開啟遊玩狀態
            SwitchStart();
        }

        //背景光文字
        private void Update()
        {
            mask.clipOffset = new Vector2(0, offsetY += 150 * Time.deltaTime);
            if (offsetY > 900) offsetY = -600;
        }

        #region 遊玩操控
        //開始轉動
        public override void Play()
        {
            //你沒錢就別玩
            if (AmslotDataManager.Instance.PlayerBalance.money < AmslotDataManager.Instance.PlayResult.bets)
            {
                if (AmslotDataManager.Instance.autoBool) AutoPlay();
                AutoErrorFormManager.Instance.Show("","餘額不足");
                return;
            }

            //開始按鈕是否開啟
            if (!AmslotDataManager.Instance.isStart) return;
            //Slot狀態是否閒置中
            if (!slot.isIdle) return;

            //開始按鈕關閉
            AmslotDataManager.Instance.isStart = false;
            //送出遊玩資訊
            sendPlay();
        }

        //送出遊玩資訊
        private void sendPlay()
        {
            //開始轉動後，暫時關閉要玩家金額
            StopCoroutine("PlayerBalanceTime");
            lock (locker)
             {              
                OperateInfo op = new OperateInfo();
                op.event_id = (int)AMSlotProtocol.EventCodes.EVENT_PLAY;
                op.data.Add("bet", AmslotDataManager.Instance.PlayResult.bets);
                AmslotMsgManager.Instance.SendGame(op);
            }
        }

        //開始開關
        public void SwitchStart()
        {
            AmslotDataManager.Instance.isStart = true;
        }

        //關閉自動
        public void StopAutoPlay()
        {
            AudioManager.Instance.PlaySound("ButtonSound", 16);
            if (AmslotDataManager.Instance.autoBool) AutoPlay();
        }
        //自動轉動切換
        public override void AutoPlay()
        {
            AmslotDataManager.Instance.autoBool = !AmslotDataManager.Instance.autoBool;

            //開啟自動遊玩後，開始按鈕顯示停止
            if (AmslotDataManager.Instance.autoBool)
            {
                startFont.SetActive(false);
                stopFont.SetActive(true);

                //開啟自動轉動時，若錯過偵測自動轉動，則在1.0秒後自動轉動
                if (slot.isIdle) Invoke("Play",1.0f);
            }
            else
            {
                startFont.SetActive(true);
                stopFont.SetActive(false);
            }
        }

        //自動停止
        public override void StopAuto() { slot.StopReel(); }

        //開始轉動
        public override void DoScroll(List<int> resultNumbers)
        {
            //儲存後端傳過來的結果Index
            symbolIndex = resultNumbers;
            //開始排列
            RandomSymbol();

            //前端操控參數
            //刪除所有Symbol動畫
            if (AmslotDataManager.Instance.symbolSpine.Count>0)
            {
                for(int i=0;i< AmslotDataManager.Instance.symbolSpine.Count;i++)
                {
                    if (AmslotDataManager.Instance.symbolSpine[i] != null) AmslotDataManager.Instance.symbolSpine[i].destroyParent();
                }
            }
            //有關中獎暫存List初始
            hitList = new List<GameObject>();
            AmslotDataManager.Instance.HitObject = new List<GameObject>();
            AmslotDataManager.Instance.symbolSpine = new List<SymbolSpine>();

            //自動開始延遲為0.5秒
            AmslotDataManager.Instance.autoDelay = 0.5f;
            //重置ReelIndex及儲存古錢數量
            AmslotDataManager.Instance.stopReel = 0;
            //重製儲存古錢數量
            AmslotDataManager.Instance.oldMoneyNum = 0;
            //重置中獎
            AmslotDataManager.Instance.isHit = false;
            //重置中獎分數
            winLabel.SetNumNoAnim(0);
            winLabel2.SetNumNoAnim(0);
            winLabel2.Label.text = "";
            //更新免費遊戲次數
            UpdateFreeGame(AmslotDataManager.Instance.PlayResult.freeTimes);

            //開始轉動
            slot.Play();

            //本地端先扣除本次遊玩扣除後的錢(非免費遊戲狀況)
            if (!FreeGameMask[0].activeSelf)
            {
                balanceLabel.DoSubNumAnim(balanceLabel.CurrentNum, Convert.ToInt64((AmslotDataManager.Instance.PlayerBalance.money) - AmslotDataManager.Instance.PlayResult.bets));
            }

            //開始轉動X秒後自動停止
            Invoke("StopAuto", AmslotDataManager.Instance.stopDelay * 3);
        }

        //回合結束
        public override void OnStopScroll()
        {
            //重置圖案位置
            restSymbolPos();

            //刪除所有中獎暫存
            if (AmslotDataManager.Instance.tempHit.Count>0)
            {
                for(int i =0;i< AmslotDataManager.Instance.tempHit.Count;i++)
                {
                    Destroy(AmslotDataManager.Instance.tempHit[i]);
                }
                AmslotDataManager.Instance.tempHit = new List<GameObject>();
            }

            //中獎圖案動畫在中獎Symbol底下建立並開啟
            if(AmslotDataManager.Instance.HitObject.Count>0)
            {
                for(int i = 0;i< AmslotDataManager.Instance.HitObject.Count;i++)
                {
                       GameObject temp;
                       temp = Instantiate(Hit);
                       hitList.Add(temp);
                       hitList[i].transform.parent = AmslotDataManager.Instance.HitObject[i].transform;
                       RectTransform rect = hitList[i].GetComponent<RectTransform>();
                       rect.localPosition = new Vector3(0,0,0);
                       rect.localScale = new Vector3(1, 1, 1);
                }
                //播放Symbol音效
                Invoke("symbolSound",0.1f);
            }
     
            //偵測該局的symbol中獎，是否有bigwin
            calBigWin();
        }
        //判斷是否中獎有達成bigwin
        private void calBigWin()
        {
            //如果該回合中獎超過押注金X倍，出現bigWin
            if (Convert.ToInt32(AmslotDataManager.Instance.PlayResult.prize) > (AmslotDataManager.Instance.PlayResult.bets * AmslotDataManager.Instance.bigMoney))
            {
                //跑BIGWIN動畫
                Invoke("BigWinAnim",2.0f);
                return;
            }
            //如果沒有bigwin但有中獎
            else if(AmslotDataManager.Instance.PlayResult.prize>0)
            {
                //更新贏得金額
                UpdateWinBalance(Convert.ToInt32(AmslotDataManager.Instance.PlayResult.prize));
                //延遲更新玩家金額
                Invoke("DelayUpdatePlayerBalance", 1.5f);
                Invoke("calFreeGameOver", 1.8f);
                return;
            }
            UpdateWinBalance(Convert.ToInt32(AmslotDataManager.Instance.PlayResult.prize));
            //每秒更新玩家金額啟動
            StartCoroutine("PlayerBalanceTime");
            //偵測是否為免費遊戲結束
            calFreeGameOver();
        }
        //中獎延遲更新金額
        private void DelayUpdatePlayerBalance()
        {
            StartCoroutine("PlayerBalanceTime");
        }                                                                                             
        //判斷是否為免費遊戲結束，若不是則判斷龍虎珠
        private void calFreeGameOver()
        {
            //如果為免費遊戲結束，就跑結算動畫
            if (AmslotDataManager.Instance.isFreeEnd)
            {
                freeGameWinTotal();
                return;
            }
            //否則偵測龍珠彩金
            DragonChiaJin();
        }
        //判斷是否有中免費遊戲
        private void calFreeGame()
        {
            //偵測是否要進行FreeGame
            if (AmslotDataManager.Instance.PlayResult.beginTimes) Invoke("OpenFreeGameForm",1.5f);
            //準備開始遊戲
            else gameStart();
        }
        //準備開始
        private void gameStart()
        {
            //如果本回合有中獎，延遲1.5秒，若無則0.5秒
            if (AmslotDataManager.Instance.isHit) AmslotDataManager.Instance.autoDelay = 0.3f;
            else AmslotDataManager.Instance.autoDelay = 0.25f;

            //啟動開始按鈕
            Invoke("SwitchStart", AmslotDataManager.Instance.autoDelay - 0.4f);

            AmslotDataManager.Instance.MyDragon.mine = false;
            AmslotDataManager.Instance.MyTiger.mine = false;

            //如果本次為免費遊戲或自動轉動為開啟時，就自動轉動
            if (FreeGameMask[0].activeSelf || AmslotDataManager.Instance.autoBool)
            {
                //自動轉動
                Invoke("Play", AmslotDataManager.Instance.autoDelay);
            }
        }

        //離開遊戲不保留座位
        public void UnretainerSeat()
        {
            lock (locker)
            {
                OperateInfo op = new OperateInfo();
                op.event_id = (int)AMSlotProtocol.EventCodes.EVENT_EXITGAME;
                AmslotMsgManager.Instance.SendGame(op);
            }

            Leave();
        }
        //離開遊戲
        public void Leave()
        {
            AmslotMsgManager.Instance.Leave();
        }
        
        #endregion

        #region 更新UI

        //更新彩金
        public override void UpdateChiaJin(long money)
        {
            ChiaJinLabel.DoAddNumAnim(ChiaJinLabel.CurrentNum,money);
        }

        //玩家金錢
        public override void UpdatePlayerBalance(long balance)
        {
            balanceLabel.DoAddNumAnim(balanceLabel.CurrentNum,balance);
        }

        //更新UI
        public override void UpdateUI<T>(T data)
        {
            snapShot snap = data as snapShot;
            playerName.text = DataManager.PlayerInfo.name;
            //大頭貼更新
            if (head.LoadPhotoFromLocal() == false)
            {
                head.DownLoadPhotoAndSave(DataManager.PlayerInfo.id.ToString());
            }
            //上局贏得金額
            winLabel.Label.text = Convert.ToInt32(snap.play.prize).ToString();
            bbm.SetUI(snap.play.minimumBet, snap.play.minimumBet*10, snap.play.minimumBet);
            //上局投注金額
            if (snap.play.bets == 0) snap.play.bets = snap.play.minimumBet;
            bbm.SpecifyBet(snap.play.bets);
            AmslotDataManager.Instance.PlayResult.bets = snap.play.bets;
            //更新彩金及龍虎珠資訊
            ChiaJinLabel.SetNumNoAnim(Convert.ToInt64(snap.jackpot));
            AmslotDataManager.Instance.Balls.dragon.count = snap.dragonBulbs;
            AmslotDataManager.Instance.Balls.tiger.count = snap.tigerBulbs;
            //更新bigwin倍數
            AmslotDataManager.Instance.bigMoney = snap.wins.big;
            AmslotDataManager.Instance.superMoney = snap.wins.super;
            AmslotDataManager.Instance.megaMoney = snap.wins.mega;
            //機台編號
            slotID.text = "NO." + snap.name;
            //排列圖案，若有不完整，將以預設圖案排列
            if (snap.play.matrix != null && snap.play.matrix.Count == 15) SymbolInit(snap.play.matrix);
            else SymbolInit(new List<int> {2,2,2,2,2,3,3,3,3,3,4,4,4,4,4 });
        }

        //更新symbol
        public void SymbolInit(List<int> lastTime)
        {
            int rowIndex = 0;
            List<int> symbolIndexTemp = lastTime;
                for (int EndReelIndex = 0; EndReelIndex<slot.reels.Length; EndReelIndex++)
                {
                    for (int symbolNum = 4; symbolNum > 1; symbolNum--)
                    {
                        slot.reels[EndReelIndex].symbols[symbolNum] = symbols[symbolIndexTemp[EndReelIndex + rowIndex]];
                        rowIndex += 5;
                        if (rowIndex > 10) rowIndex = 0;
                    }
                    slot.reels[EndReelIndex].RefreshHolders();
                }
        }

        //贏得獎金
        public void UpdateWinBalance(int winMoney)
        {
            if (winMoney == 0) winLabel.SetNumNoAnim(winMoney);
            else winLabel.DoAddNumAnim(0, winMoney);

            //如果有bigwin、彩金就不執行中間噴錢動畫
            if (Convert.ToInt32(AmslotDataManager.Instance.PlayResult.prize) > (AmslotDataManager.Instance.PlayResult.bets * AmslotDataManager.Instance.bigMoney)) return;
            if (AmslotDataManager.Instance.isGetDragon) return;
            if (AmslotDataManager.Instance.isGetTiger) return;
            if (winMoney <= 0) return;
            //贏得金額噴錢特效及數字跳動
            winLabel2.DoAddNumAnim(0, winMoney);
            //依照中獎金額控制跳出金幣的數量
            int coinValue = 1 + (winMoney / AmslotDataManager.Instance.PlayResult.bets);
            if (coinValue >= 30) coinValue = 30;
            winCoin.Value = coinValue;
            winCoin.DoCreatCoin();
            AudioManager.Instance.PlaySound("BonusMoney",16f);
        }

        //FreeGame次數
        public void UpdateFreeGame(int freeNum)
        {
            freeLabel.text = freeNum.ToString();
        }

        //龍珠
        public void UpdateDragonBall(int num)
        {
            //如果沒有，就關閉所有龍珠
            if(num == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    dragonBall[i].SetActive(false);
                }
            }
            //如果目前有龍珠，開啟相同數量的龍珠
            else if (num>0 && num<6)
            {
                for (int i =0; i < num; i++)
                {
                    dragonBall[i].SetActive(true);                 
                }
                for (int i = num; i < 5; i++)
                {
                    dragonBall[i].SetActive(false);
                }
            }
        }

        //虎珠
        public void UpdateTigerBall(int num)
        {
            //如果沒有，就關閉所有虎珠
            if(num == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    tigerBall[i].SetActive(false);
                }
            }
            //如果目前有虎珠，開啟相同數量的虎珠
            else if (num>0 && num < 6)
            {
                for (int i = 0; i < num; i++)
                {
                    tigerBall[i].SetActive(true);
                }
                for (int i = num; i < 5; i++)
                {
                    tigerBall[i].SetActive(false);
                }
            }
        }

        #endregion

        #region 說明頁面

        //神獸簡介
        public override void OpenIntroGameForm()
        {
            StartPage.Show();
        }

        //說明頁面
        public override void OpenExplainForm()
        {
            Explain.Show();
            AudioManager.Instance.PlaySound("ButtonSound", 16);
        }

        #endregion

        #region FreeGame頁面
        //打開選擇免費模式頁面1
        public void OpenFreeGameForm()
        {
            //關閉所有免費遊戲遮罩
            for (int i = 0; i < 3; i++) FreeGameMask[i].SetActive(false);
            //關閉兩旁火焰
            for (int i = 0; i < 2; i++) { fireSide[i].loop = false; }
            //關閉一般模式音樂，播放進入免費遊戲音樂
            AudioManager.Instance.Stop(AudioClipType.Bgm,"NormalGameBGM");
            AudioManager.Instance.PlayBgm("EnterFreeGameBGM",true,true);
            //打開選擇模式的頁面，關閉其他頁面
            freeGameForm[0].SetActive(true);
            freeGameForm[1].SetActive(true);
            freeGameForm[2].SetActive(false);
            //旗子入場動畫
            for (int i = 0; i < 4; i++) { flagSpine[i].flagInAnim(); }
            //0.667秒後旗子動畫改變
            Invoke("changeFlagAnim", 0.667f);
            //9.5秒後關閉手動點擊
            Invoke("preventClickAndDefault", 9.5f);
            //10秒後未選擇，則系統預設選擇
            Invoke("defaultMode", 10f);

        }
        //預設選擇
        private void defaultMode()
        {
            OpenFreeGameNumber(AmslotDataManager.Instance.FreeGameMode[AmslotDataManager.Instance.FreeGameModeIndex]
                ,AmslotDataManager.Instance.FreeGameMode[AmslotDataManager.Instance.FreeGameModeIndex].GetComponent<FlagSpine>());
        }
        //防止同時觸發玩家點擊及預設
        private void preventClickAndDefault()
        {
            //關閉點擊
            for (int i = 0; i < 4; i++)
            {
                flagSpine[i].transform.parent.transform.GetComponent<BoxCollider>().enabled = false;
            }
        }
        //旗子動畫改變2
        private void changeFlagAnim()
        {
            //旗子變成停留狀態
            for(int i=0;i<4;i++) { flagSpine[i].flagStandybyAnim(); }
        }
        //選擇到某個模式後，打開顯示免費遊玩幾次的頁面3
        public void OpenFreeGameNumber(GameObject flag,FlagSpine flagAnim)
        {
            //停止預設及防止同時點擊倒數
            CancelInvoke("defaultMode");
            CancelInvoke("preventClickAndDefault");
            //免費遊戲次數
            int FreeGameNum = 0;
            //選擇旗子
            switch (flag.name)
            {
                case "Flag5":
                    FreeGameNum = 5;
                    sendChoseMode(0);
                    break;
                case "Flag10":
                    FreeGameNum = 10;
                    sendChoseMode(1);
                    break;
                case "Flag15":
                    FreeGameNum = 15;
                    sendChoseMode(2);
                    break;
                case "Flag20":
                    FreeGameNum = 20;
                    sendChoseMode(3);
                    break;
            }
            //點擊動畫
            flagAnim.flagClickAnim();
            //更新免費遊玩次數UI
            FGNumLabel.text = FreeGameNum.ToString();
            UpdateFreeGame(FreeGameNum);
            //選擇0.5秒後打開頁面(讓旗子播放點擊動畫)
            Invoke("delayOpenFreeGameNum", 0.5f);
        }
        private void sendChoseMode(int m)
        {
            OperateInfo op = new OperateInfo();
            op.event_id = (int)AMSlotProtocol.EventCodes.EVENT_TIMES;
            op.data.Add("game", m);
            AmslotMsgManager.Instance.SendGame(op);
        }
        //選擇模式後旗子動畫改變
        private void delayOpenFreeGameNum()
        {
            //打開顯示次數的介面，其餘的頁面關閉
            freeGameForm[0].SetActive(true);
            freeGameForm[1].SetActive(false);
            freeGameForm[2].SetActive(true);
            //顯示2秒後自動關閉(或手動按開始)
            Invoke("CloseFreeGameForm", 2f);
        }
        //關閉所有免費遊戲提示頁面5
        public void CloseFreeGameForm()
        {
            //開啟點擊
            for (int i = 0; i < 4; i++)
            {
                flagSpine[i].transform.parent.transform.GetComponent<BoxCollider>().enabled = true;
            }
            //關閉進入免費遊戲音樂，播放免費遊戲音樂
            AudioManager.Instance.Stop(AudioClipType.Bgm,"EnterFreeGameBGM");
            AudioManager.Instance.PlayBgm("FreeGameBGM",true,true);
            //關閉所有免費遊戲提示頁面
            for (int i = 0; i < 3; i++) freeGameForm[i].SetActive(false);
            //開啟免費遊戲遮罩
            for (int i = 0; i < 3; i++) FreeGameMask[i].SetActive(true);
            //開啟兩旁火焰
            for (int i = 0; i < 2; i++)
            {
                fireSide[i].Play();
                fireSide[i].loop = true;
            }
            //結束關閉頁面的計時器(手動開始)
            if (IsInvoking("CloseFreeGameForm")) CancelInvoke("CloseFreeGameForm");
            //FreeGame轉動次數加快
            //slot.modes.defaultMode.reelMaxSpeed = 3000;
            //UpdateFreeGame(AmslotDataManager.Instance.PlayResult.freeTimes);
            gameStart();
        }

        #endregion

        #region 押注按鈕
        //加注減注最大注，基本投注金額為每一條線金額*總共有25條線*投注數
        public override void AddBet()
        {
            //必須在idle時才可以加減注
            if (slot.isIdle && AmslotDataManager.Instance.isStart) bbm.AddBet();
            AmslotDataManager.Instance.PlayResult.bets = bbm.CurrentBetMoney;
            AudioManager.Instance.PlaySound("ButtonSound",16);
        }

        public override void SubBet()
        {
            if (slot.isIdle && AmslotDataManager.Instance.isStart) bbm.SubBet();
            AmslotDataManager.Instance.PlayResult.bets = bbm.CurrentBetMoney;
            AudioManager.Instance.PlaySound("ButtonSound", 16);
        }

        public override void AddToMaxBet()
        {
            if (slot.isIdle && AmslotDataManager.Instance.isStart) bbm.SpecifyBet(bbm.GetMaxBetValue());
            AmslotDataManager.Instance.PlayResult.bets = bbm.CurrentBetMoney;
            AudioManager.Instance.PlaySound("ButtonSound", 16);
        }
        #endregion

        #region 儲值聊天設定按鈕及開場動畫
        //儲值按鈕
        public override void OpenDepositForm()
        {
            AudioManager.Instance.PlaySound("Btn_Sound", 16);
            DepositForm.UpdateBonusSell(DataManager.SysSetting.IAPDisCount);
            DepositForm.Show();
        }
        public void CloseDepositForm()
        {
            AudioManager.Instance.PlaySound("Btn_Sound", 16);
            DepositForm.Close();
        }

        //聊天按鈕
        public override void OpenChatForm()
        {
            AudioManager.Instance.PlaySound("Btn_Sound", 16);
        }

        //設定按鈕
        public override void ToggleSettingForm()
        {
            AudioManager.Instance.PlaySound("Btn_Sound", 16);
            SettingForm.Show();
        }
        public void CloseSettingForm()
        {
            AudioManager.Instance.PlaySound("Btn_Sound", 16);
            SettingForm.Close();
        }

        //開場動畫
        public override void PlayOpeningAnim()
        {

        }
        
        //離開
        public void OpenExitForm()
        {
            AudioManager.Instance.PlaySound("Btn_Sound", 16);
            ExitMask.SetActive(true);
            ExitForm.SetActive(true);
            ExitForm.transform.DOScale(new Vector3(1, 1, 1), 0.2f).SetEase(Ease.OutBack);
        }
        //離開視窗關閉
        public void CloseExitForm()
        {
            AudioManager.Instance.PlaySound("Btn_Sound", 16);
            ExitMask.SetActive(false);
            ExitForm.SetActive(false);
            ExitForm.transform.localScale = new Vector3(0,0,0);
        }

        //FB
        public void OpenFBForm()
        {
            AudioManager.Instance.PlaySound("Btn_Sound", 16);
            FBForm.Show();
        }
        public void CloseFBForm()
        {
            AudioManager.Instance.PlaySound("Btn_Sound", 16);
            FBForm.Close();
        }

        //玩家資訊
        public void OpenPlayeForm()
        {
            AudioManager.Instance.PlaySound("Btn_Sound", 16);
            PlayerForm.Show();

        }
        public void ClosePlayerForm()
        {
            AudioManager.Instance.PlaySound("Btn_Sound", 16);
            PlayerForm.Close();
        }

        #endregion

        #region 每秒要資料

        IEnumerator PlayerBalanceTime()
        {
            while (true)
            {
                OperateInfo op = new OperateInfo();
                op.event_id = (int)AMSlotProtocol.EventCodes.EVENT_BALANCE;
                AmslotMsgManager.Instance.SendGame(op);
                yield return new WaitForSeconds(2.0f);
            }
        }

        int dragonFive = 0;
        IEnumerator DragonBallTime()
        {
            while (true)
            {
                //如果有轉到龍珠
                if (AmslotDataManager.Instance.MyDragon.mine)
                {   
                    //就等到跑完自己的龍珠後在更新新的龍珠
                    yield return new WaitUntil(() => AmslotDataManager.Instance.MyDragon.mine == false);
                }
                //如果目前為五顆龍珠，檢查三次後仍然不變，就歸零
                if (AmslotDataManager.Instance.Balls.dragon.count == 5)
                {
                    //偵測三次仍然是五顆，就歸零
                    dragonFive++;
                    if (dragonFive >= 3)
                    {
                        AmslotDataManager.Instance.Balls.dragon.count = 0;
                        dragonFive = 0;
                    }
                }
                else dragonFive = 0;

                UpdateDragonBall(AmslotDataManager.Instance.Balls.dragon.count);
                //等待一秒後更新
                yield return new WaitForSeconds(1.5f);
            }
        }
        int tigerFive = 0;
        IEnumerator TigerBallTime()
        {
            while (true)
            {   
                //如果有轉到虎珠
                if (AmslotDataManager.Instance.MyTiger.mine)
                {
                    //就等到跑完自己的虎珠後在更新新的虎珠
                    yield return new WaitUntil(() => AmslotDataManager.Instance.MyTiger.mine == false);
                }
                //如果目前為五顆虎珠，檢查三次仍然不變，就歸零
                if (AmslotDataManager.Instance.Balls.tiger.count == 5)
                {
                    //偵測三次後虎珠仍然是五顆，就歸零
                    tigerFive++;
                    if (tigerFive >= 3)
                    {
                        AmslotDataManager.Instance.Balls.tiger.count = 0;
                        tigerFive = 0;
                    }
                }
                else tigerFive = 0;

                UpdateTigerBall(AmslotDataManager.Instance.Balls.tiger.count);
                //等待一秒後更新
                yield return new WaitForSeconds(1.5f);
            }
        }

        #endregion

        #region 龍虎珠五顆拿彩金

        //得到龍珠彩金
        public void DragonChiaJin()
        {
            //更新龍珠
            if (AmslotDataManager.Instance.MyDragon.mine) UpdateDragonBall(AmslotDataManager.Instance.MyDragon.count);
            //如果第五顆龍珠為自己轉到，就開啟龍珠彩金動畫
            if (AmslotDataManager.Instance.MyDragon.mine && AmslotDataManager.Instance.MyDragon.count == 5) { AmslotDataManager.Instance.isGetDragon = true; }
            //如果沒有中彩金，則跳出並判斷虎珠彩金
            if (!AmslotDataManager.Instance.isGetDragon)
            {
                TigerChiaJin();
                AmslotDataManager.Instance.MyDragon.mine = false;
                AmslotDataManager.Instance.MyDragon.count = 0;
                return;
            }
            //播放彩金、噴錢音效
            AudioManager.Instance.PlaySound("jackpot", 16f);
            AudioManager.Instance.PlaySound("BigConnectMoney", 16f);
            AudioManager.Instance.PlaySound("BigWinMoneyLoop", 16f);
            //更新贏得及玩家金錢
            UpdateWinBalance(Mathf.RoundToInt(AmslotDataManager.Instance.GetChiaJin.dragon));
            UpdatePlayerBalance(Convert.ToInt64(AmslotDataManager.Instance.PlayerBalance.money));
            //彩金動畫跳動金額
            jackpotSpine(Mathf.RoundToInt(AmslotDataManager.Instance.GetChiaJin.dragon));
            
            Invoke("closeJackpotSpine", 8f);
            Invoke("stopCoinSound", 8f);
            Invoke("TigerChiaJin", 9f);
            Invoke("dragonZero", 8f);
        }
        //龍珠資訊歸零
        private void dragonZero()
        {
            AmslotDataManager.Instance.isGetDragon = false;
            AmslotDataManager.Instance.MyDragon.mine = false;
            AmslotDataManager.Instance.MyDragon.count = 0;
            if(AmslotDataManager.Instance.Balls.dragon.count == 5 ) AmslotDataManager.Instance.Balls.dragon.count = 0;
            UpdateDragonBall(0);
        }

        //得到虎珠彩金
        public void TigerChiaJin()
        {
            //更新虎珠
            if (AmslotDataManager.Instance.MyTiger.mine) UpdateTigerBall(AmslotDataManager.Instance.MyTiger.count);
            //如果第五顆龍珠為自己轉到，就開啟虎珠彩金動畫
            if (AmslotDataManager.Instance.MyTiger.mine && AmslotDataManager.Instance.MyTiger.count == 5) { AmslotDataManager.Instance.isGetTiger = true; }
            //如果沒有中彩金，則跳出並判斷免費遊戲
            if (!AmslotDataManager.Instance.isGetTiger)
            {
                calFreeGame();
                AmslotDataManager.Instance.MyTiger.mine = false;
                AmslotDataManager.Instance.MyTiger.count = 0;
                return;
            }
            //播放噴錢音效
            AudioManager.Instance.PlaySound("jackpot", 16f);
            AudioManager.Instance.PlaySound("LittleConnectMoney", 16f);
            AudioManager.Instance.PlaySound("BigWinMoneyLoop", 16f);
            //更新贏得及玩家金錢
            UpdateWinBalance(Mathf.RoundToInt(AmslotDataManager.Instance.GetChiaJin.tiger));
            UpdatePlayerBalance(Convert.ToInt64(AmslotDataManager.Instance.PlayerBalance.money));
            //彩金動畫跳動金額
            jackpotSpine(Mathf.RoundToInt(AmslotDataManager.Instance.GetChiaJin.tiger));

            Invoke("closeJackpotSpine", 8f);
            Invoke("stopCoinSound", 8f);
            Invoke("calFreeGame", 9f);
            Invoke("tigerZero", 8f);
        }
        //虎珠資訊歸零
        private void tigerZero()
        {
            AmslotDataManager.Instance.isGetTiger = false;
            AmslotDataManager.Instance.MyTiger.mine = false;
            AmslotDataManager.Instance.MyTiger.count = 0;
            if(AmslotDataManager.Instance.Balls.tiger.count == 5) AmslotDataManager.Instance.Balls.tiger.count = 0;
            UpdateTigerBall(0);
        }

        //彩金動畫播放
        private void jackpotSpine(int money)
        {
            jackpot.transform.parent.gameObject.SetActive(true);
            jackpotLabel.DoAddNumAnim(0, money);
            jackpot.JackpotInAnim();
            AudioManager.Instance.PlaySound("firework",16f);
            BGMSevVol(false);
        }
        //彩金動畫結束
        private void closeJackpotSpine()
        {
            jackpot.transform.parent.gameObject.SetActive(false);
            AudioManager.Instance.Stop(AudioClipType.Sound, "firework");
            BGMSevVol(true);
        }

        #endregion

        #region 免費遊戲總結
        private void freeGameWinTotal()
        {
            //關閉所有免費遊戲遮罩
            for (int i = 0; i < 3; i++) { FreeGameMask[i].SetActive(false); }
            //關閉兩旁火焰
            for (int i = 0; i < 2; i++) { fireSide[i].loop = false; }

            //免費遊戲結束判斷為false
            AmslotDataManager.Instance.isFreeEnd = false;
            //開啟免費遊戲總結遮罩
            FreeGameOverMask.SetActive(true);
            FreeResult.transform.DOScale(new Vector3(1,1,1),0.2f).SetEase(Ease.OutBack);
            //免費遊戲次數及總額
            FreeGameWinTotal.DoAddNumAnim(0, Convert.ToInt32(AmslotDataManager.Instance.FreeEndSum.prize));
            FreeGameSpinNum.Label.text = AmslotDataManager.Instance.FreeEndSum.times.ToString();
            CoinRain.DoCreatCoin();
            //開啟金錢音效
            AudioManager.Instance.PlaySound("BigWinMoneyLoop", 16f);
            //5秒後關閉噴錢音效及總結動畫
            Invoke("stopCoinSound", 5f);
            Invoke("totalEnd", 5f);
        }
        private void totalEnd()
        {
            //關閉免費遊戲音樂，播放一般模式音樂
            AudioManager.Instance.Stop(AudioClipType.Bgm, "FreeGameBGM");
            AudioManager.Instance.PlayBgm("NormalGameBGM", true, true);
            AmslotDataManager.Instance.PlayResult.freeTimes = -1;
            //轉動速度恢復正常
            //slot.modes.defaultMode.reelMaxSpeed = 3000;
            //免費遊戲遮罩關閉
            FreeGameOverMask.SetActive(false);
            FreeResult.transform.DOScale(new Vector3(0,0,0), 0.2f).SetEase(Ease.InBack);
            //偵測有沒有龍珠彩金
            DragonChiaJin();
        }

        #endregion

        #region BigWin

        //點擊後跳到最後動畫
        bool isSkip = true;
        bool isTake = true;
        int resultPrize;
        //BigWin入場動畫
        public void BigWinAnim()
        {
            isSkip = true;
            isTake = false;
            resultPrize = Convert.ToInt32(AmslotDataManager.Instance.PlayResult.prize);
            //播放bigwin音效，並減少背景音樂的音量
            AudioManager.Instance.PlaySound("BigWin",16f);
            BGMSevVol(false);
            //打開BIGWIN並播放入場動畫
            BigSpine.BigInAnim();
            //BigWin父物件控制SCALE動畫
            BigWin.gameObject.transform.parent.DOScale(1, 0.2f);
            //打開噴錢特效
            Coinshot.Play();
            //播放噴錢音效
            AudioManager.Instance.PlaySound("BigWinMoneyLoop", 16f);
            //打開BIGWIN遮罩
            BigWinMask.SetActive(true);
            //0.5秒後BIGWIN變停留動畫
            StartCoroutine("BigWinTime");
        }
        //BigWin停留動畫
        IEnumerator BigWinTime()
        {
            //打開BIGWIN的金錢UI
            bigWinMoney.gameObject.SetActive(true);
            //如果總額大於一定數值則會跳SuperWin
            if (resultPrize >= (AmslotDataManager.Instance.PlayResult.bets * AmslotDataManager.Instance.superMoney))
            {
                Invoke("superWin", 2.2f);
                bigWinMoney.SetTime(2.2f);
                //跳錢動畫，金額為大獎金額
                bigWinMoney.DoAddNumAnim(0, AmslotDataManager.Instance.PlayResult.bets * AmslotDataManager.Instance.bigMoney);
                yield return new WaitForSeconds(0.8f);
                BigSpine.BigLoopAnim();
                isSkip = true;
            }
            else
            {
                bigWinMoney.SetTime(3.2f);
                //跳錢動畫，金額為大獎金額
                bigWinMoney.DoAddNumAnim(0, resultPrize);
                yield return new WaitForSeconds(0.8f);
                BigSpine.BigLoopAnim();
                yield return new WaitUntil(() => bigWinMoney.CurrentAnimNumFloat == resultPrize);
                TakeButton.SetActive(true);
                isTake = true;
                isSkip = false;
            }
            //開始BIGWIN的5秒後自動關閉動畫
            Invoke("ReadToCloseBigAnim", 5.0f);
        }

        //如果超過一定金額，變成SUPER入場動畫
        private void superWin()
        {
            //播放SuperWin音效
            AudioManager.Instance.PlaySound("SuperWin", 16f);
            //關閉動畫計時重新計算
            CancelInvoke("ReadToCloseBigAnim");

            BigSpine.SuperInAnim();
            StartCoroutine("SuperWinTime");
        }
        //Super停留動畫
        IEnumerator SuperWinTime()
        {
            //如果總額大於一定數值會跳MEGAWin
            if (resultPrize >= (AmslotDataManager.Instance.PlayResult.bets * AmslotDataManager.Instance.megaMoney))
            {
                Invoke("megaWin", 1.7f);
                bigWinMoney.SetTime(2.5f);
                //跳錢動畫，金額為大獎金額
                bigWinMoney.DoAddNumAnim(AmslotDataManager.Instance.PlayResult.bets * AmslotDataManager.Instance.bigMoney, AmslotDataManager.Instance.PlayResult.bets * AmslotDataManager.Instance.superMoney);
                yield return new WaitForSeconds(0.8f);
                BigSpine.SuperLoopAnim();
            }
            else
            {
                bigWinMoney.SetTime(2.5f);
                //跳錢動畫，金額為大獎金額
                bigWinMoney.DoAddNumAnim(AmslotDataManager.Instance.PlayResult.bets * AmslotDataManager.Instance.bigMoney, resultPrize);
                yield return new WaitForSeconds(0.8f);
                BigSpine.SuperLoopAnim();
                yield return new WaitUntil(() => bigWinMoney.CurrentAnimNumFloat == resultPrize);
                TakeButton.SetActive(true);
                isTake = true;
                isSkip = false;
            }
            //開始BIGWIN的5秒後自動關閉動畫
            Invoke("ReadToCloseBigAnim", 5.0f);
        }

        //如果超過一定金額，變成MEGA入場動畫
        private void megaWin()
        {
            //播放MegaWinWin音效
            AudioManager.Instance.PlaySound("MegaWin", 16f);
            //關閉動畫計時重新計算
            CancelInvoke("ReadToCloseBigAnim");

            BigSpine.MegaInAnim();
            StartCoroutine("MegaWinTime");
        }
        //Mega停留動畫
        IEnumerator MegaWinTime()
        {
            bigWinMoney.SetTime(2.0f);
            //跳錢動畫，金額為大獎金額
            bigWinMoney.DoAddNumAnim(AmslotDataManager.Instance.PlayResult.bets * AmslotDataManager.Instance.superMoney, resultPrize);
            yield return new WaitForSeconds(0.8f);
            BigSpine.MegaLoopAnim();
            yield return new WaitUntil(() => bigWinMoney.CurrentAnimNumFloat == resultPrize);
            TakeButton.SetActive(true);
            isTake = true;
            isSkip = false;
            //開始BIGWIN的5秒後自動關閉動畫
            Invoke("ReadToCloseBigAnim", 5.0f);
        }
        public void SkipBigWinAnim()
        {
            //防止手動及自動結束同時觸發
            if (!isSkip) return;
            isSkip = false;

            //取消有關bigwin的invoke
            CancelInvoke("ReadToCloseBigAnim");
            CancelInvoke("superWin");
            CancelInvoke("megaWin");
            StopCoroutine("BigWinTime");
            StopCoroutine("SuperWinTime");
            StopCoroutine("MegaWinTime");

            if (resultPrize > (AmslotDataManager.Instance.PlayResult.bets * AmslotDataManager.Instance.megaMoney)) BigSpine.MegaLoopAnim();
            else if (resultPrize > (AmslotDataManager.Instance.PlayResult.bets * AmslotDataManager.Instance.superMoney)) BigSpine.SuperLoopAnim();
            else BigSpine.BigLoopAnim();

            bigWinMoney.SetNumNoAnim(resultPrize);

            TakeButton.SetActive(true);
            isTake = true;
            Coinshot.Stop();

            Invoke("TakeBigWinMoney", 2.0f);
        }
        //領取
        public void TakeBigWinMoney()
        {
            if (!isTake) return;
            isTake = false;
            CancelInvoke("TakeBigWinMoney");
            CancelInvoke("ReadToCloseBigAnim");
            ReadToCloseBigAnim();
        }

        //準備關閉bigwin動畫
        private void ReadToCloseBigAnim()
        {
            Coinshot.Stop();
            isTake = false;
            //BigWin父物件控制SCALE動畫
            BigWin.gameObject.transform.parent.DOScale(0, 0.5f).SetEase(Ease.InBack).OnComplete(() => CloseBigWin());
        }

        //關閉所有BigWin物件
        private void CloseBigWin()
        {
            //每秒更新玩家金額啟動
            StartCoroutine("PlayerBalanceTime");
            //更新贏得金額
            UpdateWinBalance(resultPrize);
            //關閉噴錢音效，並增加背景音樂的音量
            AudioManager.Instance.Stop(AudioClipType.Sound, "BigWinMoneyLoop");
            BGMSevVol(true);
            //動畫狀態切換無任何東西
            BigSpine.NothingAnim();
            //關閉所有bigwin物件
            bigWinMoney.gameObject.SetActive(false);
            BigWinMask.SetActive(false);
            TakeButton.SetActive(false);
            //偵測有沒有中龍珠
            Invoke("calFreeGameOver",1.0f);
            //calFreeGameOver();
            isSkip = true;
        }

        #endregion

        #region 音效控制

        //Symbol音效
        private void symbolSound()
        {
            //開啟相對應的Symbol音效
            if (AmslotDataManager.Instance.dragon) AudioManager.Instance.PlaySound("Dragon", 16f);
            if (AmslotDataManager.Instance.tiger) AudioManager.Instance.PlaySound("Tiger", 16f);
            if (AmslotDataManager.Instance.fog) AudioManager.Instance.PlaySound("Wild", 16f);
            if (AmslotDataManager.Instance.money) AudioManager.Instance.PlaySound("Scatter", 16f);
            if (AmslotDataManager.Instance.other) AudioManager.Instance.PlaySound("Win", 16f);

            //重置所有中獎Symbol判斷
            AmslotDataManager.Instance.dragon = false;
            AmslotDataManager.Instance.tiger = false;
            AmslotDataManager.Instance.fog = false;
            AmslotDataManager.Instance.money = false;
            AmslotDataManager.Instance.other = false;

        }  

        //關閉噴錢音效
        private void stopCoinSound()
        {
            AudioManager.Instance.Stop(AudioClipType.Sound, "BigWinMoneyLoop");
        }

        //音樂變大變小
        private void BGMSevVol(bool isAmplify)
        {
            if (AudioManager.Instance.IsBgmMute) return;
            if (isAmplify) AudioManager.Instance.SetVol(AudioClipType.Bgm, 16);
            else AudioManager.Instance.SetVol(AudioClipType.Bgm, 14);
        }

        #endregion

        #region 期待框
        //期待框
        public void  HopeForm()
        {
            float delayTime = AmslotDataManager.Instance.stopDelay;
            AudioManager.Instance.PlaySound("stopReel", 16f);
            //每停止一個Reel就偵測該Reel是否有古錢
            for (int i=0;i<3;i++)
            {
                //如果有古錢
                if (slot.reels[AmslotDataManager.Instance.stopReel].symbols[i].name == "Money")
                {
                    //紀錄+1
                    AmslotDataManager.Instance.oldMoneyNum++;
                    //新增古錢動畫，若超過3個就播放
                    GameObject temp;
                    temp = Instantiate(moneyAnim);
                    tempMoneyAnim.Add(temp);
                    tempMoneyAnim[tempMoneyAnim.Count - 1].transform.parent = slot.reels[AmslotDataManager.Instance.stopReel].transform.GetChild(11-i).transform;
                    tempMoneyAnim[tempMoneyAnim.Count - 1].transform.localScale = new Vector3(2.2f,2.2f,1);
                    tempMoneyAnim[tempMoneyAnim.Count - 1].transform.localPosition = new Vector3(0,0,0);
                    tempMoneyAnim[tempMoneyAnim.Count - 1].SetActive(false);
                }
            }
            //如果古錢有2個以上，且不是最後一個Reel時
            if(AmslotDataManager.Instance.oldMoneyNum >=2 && AmslotDataManager.Instance.stopReel < 4)
            {
                //播放期待框音效
                AudioManager.Instance.PlaySound("ThirdSpeedUp", 16f);
                //重開期待框
                fireReel.SetActive(false);
                fireReel.SetActive(true);
                //非免費遊戲，兩旁火焰開啟
                if (AmslotDataManager.Instance.PlayResult.freeTimes < 0)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        fireSide[i].Play();
                        fireSide[i].loop = true;
                    }
                }
                //建立期待框的位置
                fireReel.transform.localPosition = new Vector3(rireReelPos[AmslotDataManager.Instance.stopReel +1],0,0);
                //Reel轉動時間延長為X秒
                delayTime = AmslotDataManager.Instance.stopDelay * 6;

                //開起每個金幣動畫
                for(int i=0;i<tempMoneyAnim.Count;i++)
                {
                    tempMoneyAnim[i].SetActive(true);
                }
            }
            //當最後一個Reel停止時
            if (AmslotDataManager.Instance.stopReel >= 4)
            {
                //關閉期待框
                fireReel.SetActive(false);
                //非免費遊戲，兩旁火焰關閉
                if (AmslotDataManager.Instance.PlayResult.freeTimes < 0)
                {
                    for (int i = 0; i < 2; i++) { fireSide[i].loop = false; }
                }
                //轉動時間恢復預設值
                delayTime = AmslotDataManager.Instance.stopDelay;
                //刪除所有金幣動畫物件
                if (tempMoneyAnim.Count>0)
                {
                    for (int i = 0; i < tempMoneyAnim.Count; i++)
                    {
                        Destroy(tempMoneyAnim[i]);
                    }
                    //清空list
                    tempMoneyAnim.Clear();
                }
            }
            //每當停止一個Reel時+1，指向下一個Reel
            AmslotDataManager.Instance.stopReel++;
            //每當停止一個Reel時，依據當前轉動延遲時間，停止下一個Reel的轉動
            if (AmslotDataManager.Instance.stopReel > 4) return;
            Invoke("StopAuto", delayTime);
        }
        #endregion

        #region 隨機排列並決定結果

        //各個Reel的Index
        int ReelIndex = 0;
        //各個Reel裡的Row的Index
        int RowIndex = 0;
        //儲存目前擁有的Symbol
        [Space(15)]
        public Symbol[] symbols;
        //排列結果 以index讀取symbol 0~4上排五個 5~9中間五個 10~14下排五個
        [HideInInspector]
        public List<int> symbolIndex;
        //儲存結果的Symbol
        Symbol[] EndSymbol = new Symbol[15];

        private void RandomSymbol()
        {
            //隨機排列該reel的所有symbol
            for (int ReelIndex =0; ReelIndex<5; ReelIndex++)
            {
                for (int symbolNum = 0; symbolNum < slot.reels[ReelIndex].symbols.Length; symbolNum++)
                {
                    slot.reels[ReelIndex].symbols[symbolNum] = symbols[UnityEngine.Random.Range(0, symbols.Length)];
                }

                if(ReelIndex==4) Arrangement();
            }
        }

        private void Arrangement()
        {
            //開始儲存決定的圖案及連線
            for (int si = 0; si < EndSymbol.Length; si++)
            {
                EndSymbol[si] = symbols[symbolIndex[si]];
            }
         
            for (int EndReelIndex = 0; EndReelIndex < slot.reels.Length; EndReelIndex++)
            {
                for (int symbolNum = 2; symbolNum > -1; symbolNum--)
                {
                    slot.reels[EndReelIndex].symbols[symbolNum] = EndSymbol[EndReelIndex + RowIndex];
                    RowIndex += 5;
                    if (RowIndex > 10) RowIndex = 0;
                }
            }

            //每一個reel都設定停留點
            for (int reelIndex = 0; reelIndex < slot.reels.Length; reelIndex++)
            { slot.reels[reelIndex].SetManipulation(null, 2, 0); }
        }

        #endregion

        #region 重置位置

        private void restSymbolPos()
        {
            //重置SYMBOL位置
            for (int reelIndex = 0; reelIndex < 5; reelIndex++)
            {
                for (int symbolNum = 0; symbolNum < 12; symbolNum++)
                {
                    GameObject symbolChild = slot.reels[reelIndex].transform.GetChild(symbolNum).gameObject;

                    if (slot.reels[reelIndex].holders[symbolNum].y != PosList[symbolNum])
                    {
                        symbolChild.transform.localPosition = new Vector3(0, PosList[symbolNum], 0);
                        slot.reels[reelIndex].holders[symbolNum].image.sprite = slot.reels[reelIndex].symbols[11 - symbolNum].sprite;
                    }

                    if (symbolChild.transform.localScale.x < 1)
                    {
                        symbolChild.transform.localScale = Vector3.one;
                    }

                    if (!symbolChild.GetComponent<Image>().enabled)
                    {
                        symbolChild.GetComponent<Image>().enabled = true;
                    }
                }
            }
        }

        #endregion
    }
}
