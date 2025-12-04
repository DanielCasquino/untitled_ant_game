using UnityEngine;

public class SpiderTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        if (player == null)
            return;
        player.Damage();
    }
}