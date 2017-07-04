using UnityEngine;
using System.Collections;
using Spine.Unity;

namespace Amslot_SW
{
    public class FlagSpine : MonoBehaviour
    {
        [SpineAnimation]
        public string flagIn;
        [SpineAnimation]
        public string flagStandby;
        [SpineAnimation]
        public string flagClick;

        SkeletonAnimation skeletonAnimation;

        public Spine.AnimationState spineAnimationState;
        public Spine.Skeleton skeleton;

        void Awake()
        {
            skeletonAnimation = GetComponent<SkeletonAnimation>();
            spineAnimationState = skeletonAnimation.state;
            skeleton = skeletonAnimation.skeleton;
        }

        public void flagInAnim() { skeletonAnimation.AnimationName = flagIn; }
        public void flagStandybyAnim() { skeletonAnimation.AnimationName = flagStandby; }
        public void flagClickAnim() { skeletonAnimation.AnimationName = flagClick; }

    }
}
