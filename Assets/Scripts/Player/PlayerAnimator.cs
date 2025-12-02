using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] PlayerCursor cursor;
    Animator animator;
    [SerializeField] float rotationSpeed = 10f;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.forward, cursor.transform.localPosition.normalized), rotationSpeed * Time.deltaTime);
    }

    public void OnStateChanged(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.IDLE:
                animator.SetBool("moving", false);
                break;
            case PlayerState.MOVING:
                animator.SetBool("moving", true);
                break;
        }
    }
}
