using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodStainSpawnManager : MonoBehaviour 
{
    private int _stainCount = 0;
    private const int _maxStains = 10000;

    private GameObject _BloodStainPrefab;

    public static BloodStainSpawnManager instance;

    public CharacterType _characterType;

    [SerializeField] private BloodsSO _bloodSO;

    public enum CharacterType
    {
        Player,
        Enemy
    }

    private void Awake()
    {
        if(instance != null && instance != this )
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public void Spawn(Vector2 point, Vector2 normal, CharacterType characterType)
    {
        if(_stainCount >= _maxStains)
        {
            return;
        }

        //Assign the blood Stain prefab
        if(characterType == BloodStainSpawnManager.CharacterType.Enemy)
        {
            _BloodStainPrefab = _bloodSO.enemyStainPrefab;
        }
        else if(characterType == BloodStainSpawnManager.CharacterType.Player)
        {
            _BloodStainPrefab = _bloodSO.playerStainPrefab;
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

            Vector2 offset = tangent * Random.Range(-0.2f, 0.2f) + normal * Random.Range(-0.2f, -1.9f);

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

            SpawnStain(_BloodStainPrefab, pos, normal, characterType);
            _stainCount++;
        }
    }

    private void SpawnStain(GameObject BloodStain,Vector2 pos, Vector2 normal, CharacterType characterType)
    {
        var dot = PoolManager.SpawnObject(BloodStain, pos, Quaternion.identity, PoolManager.PoolType.BloodStains);

        dot.transform.position = pos;

        bool isFloor = Mathf.Abs(normal.y) > 0.7f;

        float XScale;
        float YScale;

        if(isFloor)
        {
            //Width of the stain prefab is greater than the height as the blood is on the floor
            XScale = Random.Range(0.25f, 0.4f);
            YScale = Random.Range(0.05f, 0.14f);
        }
        else
        {
            //Height of the stain prefab is greater than the Width as the blood is on the Wall
            XScale = Random.Range(0.1f, 0.2f);
            YScale = Random.Range(0.3f, 0.55f);
        }

        dot.transform.localScale = new Vector3(XScale, YScale, dot.transform.localScale.z);

        var spriteRenderer = dot.GetComponent<SpriteRenderer>();

        switch(characterType)
        {
            case CharacterType.Player:
                spriteRenderer.color = new Color(Random.Range(0.5f, 0.7f), 0f, 0f, Random.Range(0.6f, 0.9f));
                break;

            case CharacterType.Enemy:
                spriteRenderer.color = new Color(0f, 0f, Random.Range(0.5f, 0.7f), Random.Range(0.6f, 0.9f));
                break;
        }
    }

    public void OnStainReturned()
    {
        _stainCount = Mathf.Max(0, _stainCount - 1);
    }
}
