using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CircleCollider2D))]
public class SpiderAggroRange: MonoBehaviour
{
    public UnityEvent<Player> whenPlayerEnteredAggroZone;
    public UnityEvent whenPlayerExitedAggroZone;
    void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        if (player == null)
            return;
        whenPlayerEnteredAggroZone?.Invoke(player);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        if (player == null)
            return;
        whenPlayerExitedAggroZone?.Invoke();
    }
}