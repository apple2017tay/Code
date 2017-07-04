using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using PathologicalGames;

public class BoastCoinManager : MonoBehaviour {

    public GameObject[] Coin;

    public Vector3[] Pos;
    public Vector3 StartPos;
    public Vector3 EndPos;

    public int CoinIndex;
    public long Num;
    public float CreatSpeed;
    public float MoveDuration;

    private void Start()
    {
        StartPos = new Vector3();
        EndPos = new Vector3();
    }

    public void DoCreatCoin(long money)
    {
        if (money < 1000)
        {
            CoinIndex = 0;
            Num = money / 100;
        }
        else if (money < 10000)
        {
            CoinIndex = 1;
            Num = money / 1000;
        }
        else if (money < 100000)
        {
            CoinIndex = 2;
            Num = money / 10000;
        }
        else if (money < 1000000)
        {
            CoinIndex = 3;
            Num = money / 100000;
        }
        else if (money < 10000000)
        {
            CoinIndex = 4;
            Num = money / 1000000;
        }


        StartCoroutine("CreatCoinDelay");
    }

    IEnumerator CreatCoinDelay()
    {
        for (int i = 0; i < Num; i++)
        {
            GameObject temp = Instantiate(Coin[CoinIndex]);
            temp.SetActive(true);
            temp.transform.parent = transform;
            temp.transform.localScale = Vector3.one;
            temp.transform.localPosition = StartPos;
            temp.transform.DOLocalMove(EndPos, MoveDuration).OnComplete(() => Destroy(temp));
            yield return new WaitForSeconds(CreatSpeed);
        }
    }
}
