using UnityEngine;
using System.Collections;

namespace CoinEffect_SongWei
{
    public class Coin : MonoBehaviour
    {

        Rigidbody2D rb;
        //目標位置
        [HideInInspector]
        public Vector3 targetPos = new Vector3(800, 600, 0);
        //是否往目標位置
        [HideInInspector]
        public bool gotoTargetBool = true;
        //是否往目標移動
        bool gotoTargetPos = false;
        //往目標移動速度
        [HideInInspector]
        public float speed = 2000;
        //最小Y值
        [HideInInspector]
        public float floorY = -200;
        //噴發力道
        [HideInInspector]
        public int force = 70;
        //噴發力道向上
        int forceUp;
        //噴發力道向左右
        int forceRL;
        [HideInInspector]
        public int shotRange = 1;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void OnDisable()
        {
            gotoTargetPos = false;
            rb.gravityScale = 0.3f;
        }

        void OnSpawned()
        {
            this.gameObject.SetActive(false);
            this.gameObject.SetActive(true);
        }

        public void init()
        {
            forceUp = Random.Range(force, force);
            forceRL = Random.Range(-force / shotRange, force / shotRange);
            rb.AddForce(transform.up * forceUp);
            rb.AddForce(transform.right * forceRL);
        }

        void Update()
        {

            //當錢幣小於一定的Y值
            if (transform.localPosition.y < floorY)
            {
                rb.gravityScale = 0;
                gotoTargetPos = true;
            }
            //移動到目標
            if (gotoTargetPos && gotoTargetBool)
            {
                speed += 100;
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPos, Time.deltaTime * speed);
            }
            //到目標後消失
            if (transform.localPosition == targetPos) gameObject.SetActive(false);
        }
    }
}