using UnityEngine;
using System.Collections;
using Spine.Unity;

namespace Amslot_SW
{
    public class BigwinSpine : MonoBehaviour
    {
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

        public SkeletonAnimation light;

        SkeletonAnimation skeletonAnimation;

        public Spine.AnimationState spineAnimationState;
        public Spine.Skeleton skeleton;

        void Awake()
        {
            skeletonAnimation = GetComponent<SkeletonAnimation>();
            spineAnimationState = skeletonAnimation.state;
            skeleton = skeletonAnimation.skeleton;
        }
       
        public void BigInAnim()
        {
            skeletonAnimation.AnimationName = bigIn;
            light.gameObject.SetActive(true);
            light.AnimationName = "LigntIn";
        }
        public void BigLoopAnim() {
            skeletonAnimation.AnimationName = bigLoop;
            light.AnimationName = "LightStanby";
        }
        public void SuperInAnim() {
            skeletonAnimation.AnimationName = superIn;
        }
        public void SuperLoopAnim() {
            skeletonAnimation.AnimationName = superLoop; 
        }
        public void MegaInAnim() {
            skeletonAnimation.AnimationName = megaIn;
        }
        public void MegaLoopAnim() {
            skeletonAnimation.AnimationName = megaLoop;
        }
        public void NothingAnim()
        {
            skeletonAnimation.AnimationName = nothing;
            light.gameObject.SetActive(false);
        }
    }
}
