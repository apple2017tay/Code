using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Boast_SW
{
    public class BoastDataManager : MonoBehaviour
    {
        public static BoastDataManager Instance;

        #region 後端紀錄

        public GameInfo gameInfo;

        public GameResult game_result = new GameResult();

        public List<SelfPai> self_pais = new List<SelfPai>();

        public SitDown sitDown = new SitDown();

        public Called called = new Called();

        public Cups cups = new Cups();

        #endregion

        #region 前端紀錄

        //遊戲是否已準備好
        public bool IsReady = false;

        //目前遊戲是否在遊玩
        public bool IsStart = false;

        //玩家選擇骰子點數
        public int ChoseDicePoint;

        //玩家選擇骰子數量
        public int ChoseDiceNumber;

        //1是否被喊
        public bool IsOne;

        //是否可以開
        public bool IsCatch = false;

        //是否正在搖骰
        public bool IsShaking = false;

        //是否可以選擇酒杯
        public bool IsChoseCup = false;

        //我的座位
        public int MyPos = 0;

        //被喊的點數實際上大家擁有的數量
        public int DiceNum = 0;

        public int winIndex = 0;

        public int loseIndex = 0;

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