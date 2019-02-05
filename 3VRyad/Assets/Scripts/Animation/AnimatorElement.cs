using UnityEngine;
using System.Collections;

public class AnimatorElement : MonoBehaviour
{
    //public bool destroyThisObject = false;
    //private Animator thisAnimator;
    private Animation thisAnimation;
    private float idleAnimationTime;

    void Awake()
    {
        //ссылка на аниматор
        //thisAnimator = GetComponent<Animator>();
        thisAnimation = GetComponent<Animation>();
        //определяем когда в следующий раз проиграть анимацию
        SetidleAnimationTime();

        thisAnimation["Destroy"].layer = 1;
    }

    // Update is called once per frame
    void Update()
    {
        //if (idleAnimationTime < Time.time)
        //{
        //    //запускаем анимацию
        //    PlayIdleAnimation();
        //    //определяем когда в следующий раз проиграть анимацию
        //    SetidleAnimationTime();
        //}
    }

    private void SetidleAnimationTime() {
        //определяем когда в следующий раз проиграть анимацию
        int random = UnityEngine.Random.Range(1, 30);
        idleAnimationTime = Time.time + random;
    }

    //Если закончилась анимация смерти, то удаляем этот элемент
    public void DestroyObject() {
        Destroy(this.gameObject);
    }

    public void PlayDestroyAnimation()
    {
        //thisAnimator.SetBool("Destroy", true);
        thisAnimation.CrossFade("Destroy");
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
