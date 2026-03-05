using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DownFacingSpikesBehaviour : MonoBehaviour
{
    [Header("spikes Attributes")]
    [SerializeField] private float _replaceTime;
    [SerializeField] private float _stayTime;
    [SerializeField] private Vector2 _targetPosition;
    private Vector2 _initialPosition;
    [SerializeField] private int _damageGives = 30;
    [SerializeField] private LayerMask _layerMask;

    private void Start()
    {
        //Set the initial position
        _initialPosition = transform.position;
        _targetPosition = new Vector2(transform.position.x, transform.position.y + 2f);

        StartCoroutine(ReplaceSpikes());
    }

    private IEnumerator ReplaceSpikes()
    {
        while (true)
        {
            //Move Down
            yield return StartCoroutine(MoveTo(_targetPosition));

            yield return new WaitForSeconds(_replaceTime);

            //Move back up
            yield return StartCoroutine(MoveTo(_initialPosition));

            yield return new WaitForSeconds(_stayTime);
        }
    }

    private IEnumerator MoveTo(Vector2 target)
    {
        float speed = 2f;

        while (Vector2.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector2.Lerp(transform.position, target, speed * Time.deltaTime);

            yield return null;
        }

        transform.position = target;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Hurts the Player
        if (collision.gameObject.TryGetComponent(out IPlayerDamageable playerDamageable))
        {
            GameManager._instance._playerGotInSpikesZone = true;

            Vector2 hitPoint = collision.ClosestPoint(transform.position);
            Vector2 hitNormal = (collision.transform.position - transform.position).normalized;

            Vector2 hitDirection = (collision.transform.position - transform.position).normalized;
            playerDamageable.Damage(_damageGives, hitDirection, hitPoint, hitNormal);
        }

        //Hurts the Enemy
        if (collision.gameObject.TryGetComponent(out IDamageable enemyDamageable))
        {
            Vector2 hitDirection = (collision.transform.position - transform.position).normalized;

            float rayDistance = 0.5f;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, hitDirection, rayDistance, _layerMask);

            
                enemyDamageable.RecieveHit(hit, hitDirection);
            
        }
    }
}
