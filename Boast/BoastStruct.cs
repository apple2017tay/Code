using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Boast_SW
{
    //55 坐下
    public class SitDown
    {
        public Player_info player_info;
        public int pos;
    }

    public class Player_info
    {
        public double all_balance;
        public double balance;
        public int card_balance;
        public int fake_balance;
        public int game_balance;
        public int game_count;
        public int id;
        public string name;
        public int open_card_count;
        public int win_count;
    }

    //21 開始遊戲

    public class GameInfo
    {
        public int base_risk_money;
        public int bet_num;
        public int bet_point;
        public int bet_pos;
        public int count_down_sec;
        public List<int> cups;
        public GameResult game_result;
        public int master;
        public int play_mode;
        public int player_per_desk;
        public int render_index;
        public List<Seat> seats;
        public int status;
    }

    public class SelfPai
    {
        public bool is_cover;
        public int point;
        public int type;
    }

    public class Seat
    {
        public Ctx ctx;
        public Player_info player_info;
        public int pos;
        public int status;
    }

    public class GameResult
    {
        public bool is_double;
        public int bet_id;
        public int lost_id;
        public int win_id;
        public Dictionary<int, List<SelfPai>> player_pais;
        public int result;
    }

    public class Ctx
    {

    }

    //48 喊點
    public class Called
    {
        public int num;
        public int point;
        public int pos;
    }

    public class Cups
    {
        public int pid;
        public int pos;
    }
}