using Ancient_Awakenings_SoulNail_charm.Auxiliar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Ancient_Awakenings_SoulNail_charm.Monobehaviors
{
    public class AAPlayerAnimator:MonoBehaviour
    {

        public tk2dSpriteAnimator heroAnimator;
        public UnityEvent _OnStarted = new UnityEvent(), _OnFinished = new UnityEvent();

        private Coroutine animRoutine;
        private bool doingAnimation;

        public void ChangeAnimation(string animationName,float maxTime=-1,bool relinquishControl=true, float delay = 0f)
        {
            if (heroAnimator == null)
            {
                heroAnimator = HeroController.instance.GetComponent<tk2dSpriteAnimator>();
            }

            if (!doingAnimation)
            {
                animRoutine = StartCoroutine(DoAnimation(animationName, maxTime,relinquishControl,delay));
            }

        }

        public IEnumerator DoAnimation(string animName, float maxTime = -1,bool relinquishControl= true,float delay=0f)
        {

            yield return new WaitForSeconds(delay);

            AncientAwakeningsMod.Instance.Log("Started doing animation " + animName);
            doingAnimation = true;

            if (relinquishControl){
                HeroController.instance.RelinquishControl();
            }

            HeroController.instance.StopAnimationControl();

            heroAnimator.Play(animName);
            _OnStarted.Invoke();

            if (maxTime > 0){
                yield return new WaitForSeconds(maxTime);
            }else{
                yield return new WaitWhile(() => heroAnimator.IsPlaying(animName));
            }

            if (relinquishControl){
                HeroController.instance.RegainControl();
            }

            HeroController.instance.StartAnimationControl();

            _OnFinished.Invoke();

            doingAnimation = false;
            AncientAwakeningsMod.Instance.Log("Finished doing animation " + animName);

            _OnStarted.RemoveAllListeners();
            _OnFinished.RemoveAllListeners();

        }
    }
}
