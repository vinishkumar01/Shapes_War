using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayPunchForBackGrounds : MonoBehaviour
{
    Vector3 _basePos;

    public static GamePlayPunchForBackGrounds instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        _basePos = transform.localPosition;
    }

    public void Punch(Vector2 dir, float strength)
    {
        StopAllCoroutines();
        StartCoroutine(PunchRoutine(dir * strength));
    }

    private IEnumerator PunchRoutine(Vector2 offset)
    {
        float t = 0f;
        float duration = 0.06f;

        while (t < duration)
        {
            t += Time.deltaTime;
            //Normalize time into range 1 -> 0
            float k = 1f - (t / duration);
            transform.localPosition = _basePos + (Vector3)(offset * k);

            yield return null;
        }

        transform.localPosition = _basePos;
    }
}
