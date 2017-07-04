using UnityEngine;
using System.Collections;
using Spine.Unity;
using DG.Tweening;

public class JackPotSpine : MonoBehaviour {

    [SpineAnimation]
    public string JackpotIn;
    [SpineAnimation]
    public string JackpotLoop;

    SkeletonAnimation skeletonAnimation;

    public Spine.AnimationState spineAnimationState;
    public Spine.Skeleton skeleton;

    public GameObject moneyFont;

    void Awake()
    {
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        spineAnimationState = skeletonAnimation.state;
        skeleton = skeletonAnimation.skeleton;
    }

    public void JackpotInAnim()
    {
        skeletonAnimation.AnimationName = JackpotIn;
        moneyFont.transform.localScale = Vector3.zero;
        Invoke("JackpotLoopAnim",0.667f);
    }
    public void JackpotLoopAnim()
    {
        skeletonAnimation.AnimationName = JackpotLoop;
        moneyFont.transform.DOScale(1, 0.3f);
    }
}
