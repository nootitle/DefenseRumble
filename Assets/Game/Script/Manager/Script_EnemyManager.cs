using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEngine;

public class Script_EnemyManager : MonoBehaviour
{
    [SerializeField] List<Script_Enemy> _enemies = null;
    [SerializeField] float _spawnTime = 1f;
    [SerializeField] bool _spawn = false;

    float _timeCount = 0f;

    [Header("Box")]
    [SerializeField] List<Script_Enemy> _boxes = null;
    [SerializeField] List<Transform> _spawnPoints = null;

    public Script_Enemy GetEnemy(int index)
    {
        return _enemies[index];
    }

    void EnemySpawn()
    {
        if (!_spawn) return;

        if(_timeCount > _spawnTime)
        {
            ShootBoxes();

            _timeCount = 0f;
        }
        else
        {
            _timeCount += Time.deltaTime;
        }
    }

    public void ShootBoxes()
    {
        Transform point = _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Count)];

        for(int i = 0; i < _boxes.Count; i++)
        {
            if (!_boxes[i].spawned)
            {
                _boxes[i].gameObject.SetActive(true);
                _boxes[i].StartMove(point.position, point.forward);
                break;
            }
        }
    }

    private void Update()
    {
        EnemySpawn();
    }
}
