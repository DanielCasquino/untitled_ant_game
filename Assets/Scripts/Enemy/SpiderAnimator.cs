using UnityEngine;

public class SpiderAnimator : MonoBehaviour
{
    Animator animator;
    void Awake()
    {
        animator = GetComponent<Animator>();
    }


    public void OnStateChanged(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.IDLE:
                animator.SetBool("moving", false);
                break;
            case EnemyState.MOVING:
                animator.SetBool("moving", true);
                break;
        }
    }
}
