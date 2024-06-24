using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySelector : MonoBehaviour
{
    [SerializeField] private ScriptableEnemies _enemies;
    private SpriteRenderer _spriteRenderer;
    private Coroutine _jumpCoroutine;
    private Enemy _selectedEnemy;
    
    private void Start()
    {
        _enemies.AddListener(EnemySelected);
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void EnemySelected()
    {
        if (_jumpCoroutine != null)
        {
            StopCoroutine(_jumpCoroutine);
            _jumpCoroutine = null;
        }

        _selectedEnemy = _enemies.GetSelectedEnemy();

        if (_selectedEnemy == null)
        {
            _spriteRenderer.enabled = false;
        }
        else
        {
            _spriteRenderer.enabled = true;
            transform.position = _selectedEnemy.transform.position;
            _jumpCoroutine = StartCoroutine(JumpUpAndDown());
        }
    }

    private void Update()
    {
        if (_selectedEnemy != null)
        {
            transform.LookAt(Camera.main.transform);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        }
    }

    IEnumerator JumpUpAndDown()
    {
        float jumpHeight = 0.5f;
        float jumpSpeed = 2.0f;

        float enemyHeight = _selectedEnemy.GetComponent<Collider>().bounds.size.y;

        while (true)
        {
            if (_selectedEnemy == null)
            {
                _jumpCoroutine = null;
                _spriteRenderer.enabled = false;
                yield break;
            }
            
            Vector3 startPosition =_selectedEnemy.transform.position + new Vector3(0, enemyHeight, 0);
            Vector3 endPosition = startPosition + new Vector3(0, jumpHeight, 0);
            
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
            
            for (float t = 0; t < 1; t += Time.deltaTime * jumpSpeed)
            {
                transform.position = Vector3.Lerp(startPosition, endPosition, t);
                yield return null;
            }

            for (float t = 0; t < 1; t += Time.deltaTime * jumpSpeed)
            {
                transform.position = Vector3.Lerp(endPosition, startPosition, t);
                yield return null;
            }
        }
    }
}
