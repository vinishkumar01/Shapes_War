using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodStainSpawnManager : MonoBehaviour 
{
    private int _stainCount = 0;
    private const int _maxStains = 10000;

    [SerializeField] private GameObject _bloodStainPrefab;

    public static BloodStainSpawnManager instance;

    private void Awake()
    {
        if(instance != null && instance != this )
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public void Spawn(Vector2 point, Vector2 normal)
    {
        if(_stainCount >= _maxStains)
        {
            return;
        }

        //tangent direction along the surface
        Vector2 tangent = new Vector2(-normal.y, normal.x);

        int dotCount = Random.Range(3, 8);

        //Corner amplification
        if (Physics2D.OverlapCircle(point, 0.08f))
        {
            dotCount += 2;
        }

        for (int i = 0; i < dotCount; i++) 
        {
            if(_stainCount >= _maxStains)
            {
                return ;
            }

            Vector2 offset = tangent * Random.Range(-0.2f, 0.2f) + normal * Random.Range(-0.2f, -0.9f);

            //Directional Streaking
            Vector2 streak = -normal * Random.Range(0.01f, 0.03f);
            offset += streak;

            //platform ground and platform wall bias
            if(Mathf.Abs(normal.y) > 0.7f)
            {
                //floor -> restrict vertical Spread
                offset.y *= 0.2f;
            }
            else
            {
                //wall -> restrict horizontal spread
                offset.x *= 0.2f;
            }
            
            //Per hit scatter offset
            Vector2 clusterScatter = tangent * Random.Range(-0.2f, 0.2f);

            Vector2 pos = point + clusterScatter +offset;

            SpawnDot(pos, normal);
            _stainCount++;
        }
    }

    private void SpawnDot(Vector2 pos, Vector2 normal)
    {
        var dot = PoolManager.SpawnObject(_bloodStainPrefab, pos, Quaternion.identity, PoolManager.PoolType.BloodStains);

        dot.transform.position = pos;

        bool isFloor = Mathf.Abs(normal.y) > 0.7f;

        float XScale;
        float YScale;

        if(isFloor)
        {
            XScale = Random.Range(0.25f, 0.5f);
            YScale = Random.Range(0.05f, 0.14f);
        }
        else
        {
            XScale = Random.Range(0.1f, 0.2f);
            YScale = Random.Range(0.8f, 0.15f);
        }
        
        dot.transform.localScale = new Vector3(XScale, YScale, dot.transform.localScale.z);

        var spriteRenderer = dot.GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(Random.Range(0.5f, 0.7f), 0f, 0f, Random.Range(0.6f, 0.9f));
    }
}
