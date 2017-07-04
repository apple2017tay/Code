using UnityEngine;
using System.Collections;
using PathologicalGames;

namespace CoinEffect_SongWei {

    public class CreatCoin : MonoBehaviour
    {

        //噴發物件
        public GameObject coin;
        //噴發數量
        public int Value = 100;
        //噴射位置範圍
        public Vector2 creatPosRange = new Vector2(0, 0);
        //生成速度
        public float creatSpeed = 0.01f;
        //噴發力道
        public int force = 300;
        public int shotRange = 180;
        //金幣大小
        public float coinSize = 20f;
        //到目的地的移動速度
        public float gotoTargetSpeed = 20;
        //掉落在多低
        public float floorY = -200;
        //是否錢往目的地
        public bool gotoTargetBool = true;
        //目的地
        public Vector3 gotoTarget = new Vector3(550, 300, 0);
        public float duration = 5f;
        //是否噴發金幣
        public bool creatCoin { get; private set; }
        //儲存子物件
        Transform coinChild;
        //物件池
        public SpawnPool pool;

        private void OnEnable()
        {
           while (transform.childCount > 0)
            {
                pool.Despawn(transform.GetChild(0), pool.transform);
            }

            creatCoin = false;
        }

        public void DoCreatCoin()
        {
            if (creatCoin == false)
            {
                creatCoin = true;
                StopCoroutine(CreatCoinDelay());
                StartCoroutine(CreatCoinDelay());
            }
        }

        IEnumerator CreatCoinDelay()
        {
            for (int i = 0; i < Value; i++)
            {
                coinChild = pool.Spawn("Coin", transform);
                coinChild.localScale = new Vector3(coinSize, coinSize, coinSize);
                coinChild.localPosition = new Vector3(Random.Range(-creatPosRange.x, creatPosRange.x), Random.Range(-creatPosRange.y, creatPosRange.y), 0);
                coinChild.gameObject.SetActive(true);
                Coin coin = coinChild.GetComponent<Coin>();
                coin.floorY = floorY;
                coin.gotoTargetBool = gotoTargetBool;
                coin.targetPos = gotoTarget;
                coin.speed = gotoTargetSpeed * 100;
                coin.force = force;
                coin.shotRange = 180 / shotRange;
                coin.init();
                yield return new WaitForSeconds(creatSpeed);
            }

            yield return new WaitForSeconds(duration);
            while (transform.childCount > 0)
            {
                pool.Despawn(transform.GetChild(0), pool.transform);
            }

            creatCoin = false;
        }
    }

}
