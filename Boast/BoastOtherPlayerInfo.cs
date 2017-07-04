using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using ChatSystem;
using BaseProtocol;

public class BoastOtherPlayerInfo : MonoBehaviour {

    public GameObject Panel;
    public GameObject Form;
    public UILabel NameLabel;
    public UILabel MoneyLabel;
    public UILabel TotalLabel;
    public UILabel WinLabel;
    public UILabel OpenLabel;
    public HeadPhotoManager head;
    public GameObject Btn_Add;
    public GameObject Btn_Black;

    public void Open(BoastPlayerInfo playerInfo)
    {
        float gameCount = playerInfo.GameCount;
        float win = playerInfo.WinCount;
        float open = playerInfo.OpenCount;

        win = ((win / gameCount) * 100);
        open = ((open / gameCount) * 100);

        NameLabel.text = playerInfo.PlayerName.text;
        MoneyLabel.text = playerInfo.Balance.text;
        TotalLabel.text = "總局數: " + playerInfo.GameCount;
        WinLabel.text = "勝率: " + win.ToString("0.00") + "%";
        OpenLabel.text = "開牌率: " + open.ToString("0.00") + "%";

        head.DownLoadPhoto(playerInfo.PlayerID.ToString());

        Panel.SetActive(true);
        Form.transform.DOScale(Vector3.one, 0.2f);
    }

    public void Close()
    {
        Form.transform.DOScale(Vector3.zero, 0.2f).OnComplete(() => Panel.SetActive(false));
    }
    
    public void Btn_AddFriend()
    {

    }

    public void Btn_BlackFriend()
    {
 
    }

}
