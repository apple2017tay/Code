using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;
using Amslot_SW;
using System;
using Spine.Unity;

public class SymbolSpine : MonoBehaviour {

    //儲存Symbol的動畫
    public GameObject[] spine;
    int spineIndex = 3;
    string spineName;
    //儲存父物件的父物件的圖片
    Image image;
    //儲存父物件
    GameObject parents;
    //儲存父物件的父物件的物件
    GameObject SymbolParents;
    //動畫放大的倍數
    public float scale = 1.1f;
    //持續時間(需/2)
    public float duration = 0.5f;

    void Start ()
    {
        //儲存父物件
        parents = transform.parent.gameObject;
        //儲存父物件的父物件的物件
        SymbolParents = parents.transform.parent.gameObject;

        //儲存父物件的父物件的IMAGE
        image = parents.transform.parent.GetComponent<Image>();

        AmslotDataManager.Instance.isHit = true;
        AmslotDataManager.Instance.symbolSpine.Add(this);
        //若IMAGE名稱為以下，則開啟對應的SPINE並且暫時關閉image以免影響spine
        switch (image.sprite.name)
        {
            //蟾蜍
            case "fog":
                spineIndex = 0;
                spineName = "BingoToad";
                spine[spineIndex].SetActive(true);
                image.enabled = false;
                AmslotDataManager.Instance.fog = true;
                break;
            //老虎
            case "tiger":
                spineIndex = 1;
                spineName = "BingoTiger";
                spine[spineIndex].SetActive(true);
                image.enabled = false;
                AmslotDataManager.Instance.tiger = true;
                break;
            //龍
            case "dragon":
                spineIndex = 2;
                spineName = "BingoDrango";
                spine[spineIndex].SetActive(true);
                image.enabled = false;
                AmslotDataManager.Instance.dragon = true;
                break;
            //古錢
            case "oldMoney":
                spine[spineIndex].SetActive(true);
                image.enabled = false;
                AmslotDataManager.Instance.money = true;
                break;
            default:
                spineIndex = 5;
                AmslotDataManager.Instance.other = true;
                break;
        }
        //播放SCALE放大動畫，並在播完後刪除父物件
        if (spineIndex != 5) SymbolParents.transform.DOScale(scale, duration).SetLoops(-1, LoopType.Yoyo);
        if (spineIndex < 3) SpineTime();
    }

    void SpineTime()
    {
        spine[spineIndex].transform.GetChild(0).GetComponent<SkeletonAnimation>().AnimationName = spineName;
    }
    //移除
    public void destroyParent()
    {
        //如果image為關閉時就打開
        if (image.enabled == false) image.enabled = true;
        //刪除Symbol的DoTween動畫
        SymbolParents.transform.DOKill();
        //避免大小因動畫跑掉，這邊做個手動重置
        SymbolParents.transform.localScale = new Vector3(1, 1, 1);
        //如果父物件仍存在，就移除
        if (parents!=null) Destroy(parents);
    }
}
