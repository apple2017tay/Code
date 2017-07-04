using UnityEngine;
using System.Collections;
using PathologicalGames;

namespace CoinEffect_SongWei
{
    public class CreatCoinRain : MonoBehaviour
    {

        //噴發物件
        public GameObject coin;
        //噴發數量
        public int value = 100;
        //生成速度
        public float creatSpeed = 0.5f;
        //生成X位置範圍
        public float creatPosRange = 600;
        //掉落速度
        public float DownSpeed = 0.2f;
        //金幣大小範圍
        public Vector2 CoinSizeRange = new Vector2(5, 10);
        //儲存金幣大小
        float CoinSize;
        //是否開始生成金幣
        public bool creatCoinRain { get; private set; }
        //金幣儲存陣列
        [HideInInspector]
        public GameObject[] CoinArray;
        public float duration = 5f;

        Transform childCoin;

        public SpawnPool pool;

        private void OnEnable()
        {
            while (transform.childCount > 0)
            {
                pool.Despawn(transform.GetChild(0), pool.transform);
            }
            creatCoinRain = false;
        }

        public void DoCreatCoin()
        {
            //是否開始生產
            if (creatCoinRain == false)
            {
                creatCoinRain = true;
                CoinArray = new GameObject[value];
                StopCoroutine(CreatRainCoinDelay());
                StartCoroutine(CreatRainCoinDelay());
            }
        }

        IEnumerator CreatRainCoinDelay()
        {

            for (int i = 0; i < value; i++)
            {

                childCoin = pool.Spawn("Coin2", transform);
                childCoin.gameObject.SetActive(true);
                CoinSize = Random.Range(CoinSizeRange.x, CoinSizeRange.y);
                childCoin.transform.localScale = new Vector3(CoinSize, CoinSize, CoinSize);
                childCoin.transform.localPosition = new Vector3(Random.Range(-creatPosRange, creatPosRange), 400, 0);
                Rigidbody2D rb = childCoin.GetComponent<Rigidbody2D>();
                rb.gravityScale = DownSpeed * (CoinSize / 20);
                yield return new WaitForSeconds(creatSpeed);
            }
            yield return new WaitForSeconds(duration);
            while (transform.childCount > 0)
            {
                pool.Despawn(transform.GetChild(0), pool.transform);
            }
            creatCoinRain = false;
        }
    }
}
