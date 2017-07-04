using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SevenPKDescription : MonoBehaviour {

    public GameObject Form;
    public GameObject[] Page;
    public GameObject NextBtn;
    public GameObject PreviousBtn;

    int pageIndex = 0;

    //打開
    public void Show()
    {
        pageIndex = 0;
        Form.SetActive(true);
        NextBtn.SetActive(true);
        PreviousBtn.SetActive(false);
        Page[pageIndex].SetActive(true);
        for (int i = 1; i < Page.Length; i++) Page[i].SetActive(false);
    }

    //關閉
    public void Close()
    {
        Form.SetActive(false);
    }

    //下一頁
    public void NextPage()
    {
        Page[pageIndex].SetActive(false);
        pageIndex += 1;

        if (pageIndex >= Page.Length - 1)
        {
            pageIndex = Page.Length - 1;
            NextBtn.SetActive(false);
        }

        Page[pageIndex].SetActive(true);
    }

    //上一頁
    public void PreviousPage()
    {
        Page[pageIndex].SetActive(false);
        pageIndex -= 1;

        if (pageIndex <= 0)
        {
            pageIndex = 0;
            PreviousBtn.SetActive(false);
        }

        Page[pageIndex].SetActive(true);
    }
}
