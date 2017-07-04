using UnityEngine;
using System.Collections;
using Spine.Unity;
using NGUI_ExtendByLoger;
using DG.Tweening;

public class SevenPKBigWin : MonoBehaviour {

    public static SevenPKBigWin Instance;

    [SpineAnimation]
    public string bigIn;
    [SpineAnimation]
    public string bigLoop;
    [SpineAnimation]
    public string superIn;
    [SpineAnimation]
    public string superLoop;
    [SpineAnimation]
    public string megaIn;
    [SpineAnimation]
    public string megaLoop;
    [SpineAnimation]
    public string nothing;

    public long Result = 0;
    public GameObject BigWinForm;
    public SkeletonAnimation light;
    public NGUILabelNumberAnim LabelMoney;
    public GameObject ButtonTake;
    public GameObject Mask;
    public ParticleSystem CoinShot;

    SkeletonAnimation skeletonAnimation;

    public Spine.AnimationState spineAnimationState;
    public Spine.Skeleton skeleton;

    void Awake()
    {
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        spineAnimationState = skeletonAnimation.state;
        skeleton = skeletonAnimation.skeleton;

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance != null)
        {
            Instance = null;
        }
    }

    //進入
    public void Show()
    {
        isTake = true;
        isSkip = true;

        ButtonTake.SetActive(false);
        Mask.SetActive(true);
        BigWinForm.transform.localScale = Vector3.zero;
        BigWinForm.SetActive(true);
        BigWinForm.transform.DOScale(Vector3.one, 0.2f).OnComplete(() => BigInAnim());
    }

    //Big
    public void BigInAnim()
    {
        skeletonAnimation.AnimationName = bigIn;
        light.gameObject.SetActive(true);
        light.AnimationName = "LigntIn";
        Invoke("BigLoopAnim", 0.8f);
    }
    public void BigLoopAnim()
    {
        isSkip = false;

        LabelMoney.gameObject.SetActive(true);
        skeletonAnimation.AnimationName = bigLoop;
        light.AnimationName = "LightStanby";
        CoinShot.Play();
        if (Result >= (SevenPkDataManager.Instance.Bet * SevenPkDataManager.Instance.SuperWin))
        {
            LabelMoney.SetTime(2.2f);
            Invoke("SuperInAnim", 1.4f);
            LabelMoney.DoAddNumAnim(0, SevenPkDataManager.Instance.Bet * SevenPkDataManager.Instance.SuperWin);
        }
        else
        {
            LabelMoney.SetTime(2.2f);
            LabelMoney.DoAddNumAnim(0, Result);
            Invoke("OpenTakeButton", 2.2f);
        }
    }

    //Super
    public void SuperInAnim()
    {
        skeletonAnimation.AnimationName = superIn;
        Invoke("SuperLoopAnim", 0.8f);
    }
    public void SuperLoopAnim()
    {
        skeletonAnimation.AnimationName = superLoop;

        if (Result >= (SevenPkDataManager.Instance.Bet * SevenPkDataManager.Instance.MegaWin))
        {
            LabelMoney.SetTime(2.2f);
            Invoke("MegaInAnim", 1.4f);
            LabelMoney.DoAddNumAnim(LabelMoney.CurrentNum, SevenPkDataManager.Instance.Bet * SevenPkDataManager.Instance.MegaWin);
        }
        else
        {
            LabelMoney.SetTime(2.2f);
            LabelMoney.DoAddNumAnim(LabelMoney.CurrentNum, Result);
            Invoke("OpenTakeButton", 2.2f);
        }
    }

    //Mega
    public void MegaInAnim()
    {
        skeletonAnimation.AnimationName = megaIn;
        Invoke("MegaLoopAnim", 0.8f);
    }
    public void MegaLoopAnim()
    {
        skeletonAnimation.AnimationName = megaLoop;
        LabelMoney.SetTime(2.2f);
        LabelMoney.DoAddNumAnim(LabelMoney.CurrentNum, Result);
        Invoke("OpenTakeButton", 2.2f);
    }

    //Nothing
    public void NothingAnim()
    {
        skeletonAnimation.AnimationName = nothing;
        light.gameObject.SetActive(false);

        LabelMoney.SetNumNoAnim(0);
        LabelMoney.gameObject.SetActive(false);
    }

    //關閉
    public void Close()
    {
        isTake = true;
        isSkip = true;
        Mask.SetActive(false);

        CoinShot.Stop();

        BigWinForm.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).OnComplete(() => 
        {
            NothingAnim();
            SevenPKUIManager.Instance.Do_NextPlay();
            BigWinForm.SetActive(false);
        });
    }

    //打開領取按鈕
    private void OpenTakeButton()
    {
        isTake = false;
        isSkip = true;
        ButtonTake.SetActive(true);
        Invoke("Btn_Take", 5.0f);
    }

    //領取
    bool isTake = false;
    public void Btn_Take()
    {
        if (isTake) return;
        isTake = true;

        Close();
    }

    //跳過
    bool isSkip = false;
    public void Skip()
    {
        if (isSkip) return;

        CancelInvoke("OpenTakeButton");
        CancelInvoke("SuperInAnim");
        CancelInvoke("SuperLoopAnim");
        CancelInvoke("MegaInAnim");
        CancelInvoke("MegaLoopAnim");

        CoinShot.Stop();

        if (Result >= (SevenPkDataManager.Instance.Bet * SevenPkDataManager.Instance.MegaWin))
        {
            skeletonAnimation.AnimationName = megaLoop;
        }
        else if (Result >= (SevenPkDataManager.Instance.Bet * SevenPkDataManager.Instance.SuperWin))
        {
            skeletonAnimation.AnimationName = superLoop;
        }
        else
        {
            skeletonAnimation.AnimationName = bigLoop;
        }

        OpenTakeButton();
        LabelMoney.SetNumNoAnim(Result);
    }
}
