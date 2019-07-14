using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class AnimatorElement : MonoBehaviour
{
    //public bool destroyThisObject = false;
    //private Animator thisAnimator;
    private Animation thisAnimation;
    private float idleAnimationTime;
    public bool playIdleAnimationRandomTime;
    private bool returnToPool;

    void Awake()
    {
        //ссылка на аниматор
        thisAnimation = GetComponent<Animation>();
        playIdleAnimationRandomTime = false;
        returnToPool = false;
        //определяем когда в следующий раз проиграть анимацию
        SetidleAnimationTime();

        thisAnimation["Destroy"].layer = 1;
        thisAnimation["creature"].layer = 2;
    }

    // Update is called once per frame
    void Update()
    {
        if (playIdleAnimationRandomTime && idleAnimationTime < Time.time)
        {
            //запускаем анимацию
            PlayIdleAnimation();
            //определяем когда в следующий раз проиграть анимацию
            SetidleAnimationTime();
        }
    }

    private void SetidleAnimationTime() {
        //определяем когда в следующий раз проиграть анимацию
        int random = UnityEngine.Random.Range(5, 10);
        idleAnimationTime = Time.time + random;
    }

    public void StopAllAnimation()
    {
        thisAnimation.Stop();
    }

    //Если закончилась анимация смерти, то удаляем этот элемент
    public void DestroyObject() {
        if (returnToPool)
        {
            PoolManager.Instance.ReturnObjectToPool(gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void PlayDestroyAnimation(bool returnToPool)
    {
        this.returnToPool = returnToPool;
        thisAnimation.CrossFade("Destroy");
    }

    public void StopDestroyAnimation()
    {
        thisAnimation.Stop("Destroy");
    }

    public void PlayFallAnimation()
    {
        //thisAnimator.SetBool("Destroy", true);
        thisAnimation.Play("fall");
    }

    public void StopFallAnimation()
    {
        thisAnimation.Stop("fall");
    }

    public void PlayCreatureAnimation()
    {
        thisAnimation.Play("creature");
    }

    public void StopCreatureAnimation()
    {
        thisAnimation.Stop("creature");
    }

    public void PlayIdleAnimation()
    {
        //thisAnimator.SetBool("Idle", true);
        thisAnimation.CrossFade("Idle");
    }

    public void StopIdleAnimation()
    {
        thisAnimation.Stop("Idle");
    }

    public void PlayHintAnimation()
    {
        //thisAnimator.SetBool("hint", true);
        thisAnimation.CrossFade("hint");
    }

    public void StopHintAnimation()
    {
        thisAnimation.Stop("hint");
    }
}
