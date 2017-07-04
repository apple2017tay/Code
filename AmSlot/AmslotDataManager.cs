using UnityEngine;
using System.Collections.Generic;

namespace Amslot_SW
{
    public class AmslotDataManager : MonoBehaviour
    {
        public static AmslotDataManager Instance;

        #region 儲存後端資訊
        //機台表現
        public machineInfo MachineInfo = new machineInfo();
        //玩家金額
        public playerBalance PlayerBalance = new playerBalance();
        //快照
        public snapShot SnapShot = new snapShot();
        //遊玩資訊
        public playResult PlayResult = new playResult();
        //彩金
        public chiaJin ChiaJin = new chiaJin();
        //免費遊戲模式
        public gameMode GameMode = new gameMode();
        //免費遊戲總結
        public freeEndSum FreeEndSum = new freeEndSum();
        //龍虎珠
        public balls Balls = new balls();
        //得到彩金
        public getChiaJin GetChiaJin = new getChiaJin();
        #endregion

        #region 判斷後端資訊後開關
        //每條線金額
        public int PerLineCost = 5;
        //龍珠中獎
        public bool isGetDragon = false;
        //虎珠中獎
        public bool isGetTiger = false;
        //免費遊戲結束
        public bool isFreeEnd = false;
        #endregion

        #region 前端操控屬性

        //停止轉動延遲
        public float stopDelay = 0.5f;
        //自動轉動延遲
        public float autoDelay = 0.5f;
        //自動轉動開關
        public bool autoBool = false;
        //可遊玩開關
        public bool isStart = true;

        //古錢計算數
        [HideInInspector]public int oldMoneyNum = 0;
        //已停止的reel
        public int stopReel = 0;
        //免費遊戲預設值
        public int FreeGameModeIndex;
        //免費遊戲預設
        public List<GameObject> FreeGameMode;

        //BigWin門檻
        [Space(10)]
        public int bigMoney;
        //SuperWin門檻
        public int superMoney;
        //MegaWin門檻
        public int megaMoney;

        //龍珠
        public myDragon MyDragon = new myDragon();
        public myTiger MyTiger = new myTiger();

        //紀錄回合中是否有以下SYMBOL中獎以播放相對應的音效
        [HideInInspector]
        public bool dragon = false;
        [HideInInspector]
        public bool tiger = false;
        [HideInInspector]
        public bool fog = false;
        [HideInInspector]
        public bool money = false;
        [HideInInspector]
        public bool other = false;
        [HideInInspector]
        public bool isHit = false;
        //儲存中獎物件
        public List<GameObject> HitObject;
        //儲存中獎動畫
        public List<SymbolSpine> symbolSpine;
        //儲存中獎暫存
        public List<GameObject> tempHit;

        #endregion

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

    }
}
