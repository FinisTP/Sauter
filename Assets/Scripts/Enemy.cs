using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEngine.UI;

public abstract class Enemy : MonoBehaviour
{
    public float EnemyDamage;
    public float EnemyChaseSpeed;
    public float EnemyPatrolSpeed;
    public float EnemyMaxHealth;
    public float EnemyDetectionRange;
    public float DamagedCooldown;

    // Enemy HP display
    public GameObject BarHolder;
    public Transform WaypointObject;
    protected List<Transform> PatrolWaypoints;
    private SpriteRenderer _hpBar;
    protected bool _isStatic = false;

    public LayerMask Player;
    [SerializeField] private bool _isAttacking = false;
    protected bool _isPatrolling = true;
    protected int _currentWaypointIndex = 0;
    [SerializeField] private float _currentHealth;
    [SerializeField] private bool _canBeAttacked = true;

    private void Start()
    {
        _currentHealth = EnemyMaxHealth;
        PatrolWaypoints = new List<Transform>();
        if (WaypointObject != null)
        {
            foreach (Transform child in WaypointObject.transform)
            {
                PatrolWaypoints.Add(child);
            }
            StartCoroutine(Patrol());
        }
        _hpBar = BarHolder.GetComponentInChildren<SpriteRenderer>();
    }


    protected abstract IEnumerator Patrol();

    public virtual IEnumerator TakeDamage(float damage, Transform weapon)
    {
        _canBeAttacked = false;
        GetComponent<AIDestinationSetter>().target = null;
        _currentHealth -= damage;
        if (_currentHealth <= 0)
        {
            // die animation
            Destroy(gameObject);
        }
        
        GetComponent<Animator>().enabled = true;
        GetComponent<Animator>().Play("EnemyAttacked");
        yield return new WaitForSeconds(DamagedCooldown);
        GetComponent<Animator>().enabled = false;
        _canBeAttacked = true; 
    }

    protected abstract void Attack();
    protected abstract void StopAttack();

    private void Update()
    {
        BarHolder.transform.localScale = new Vector3(_currentHealth / EnemyMaxHealth, 1, 1);
        if (_currentHealth / EnemyMaxHealth >= 0.5f)
            _hpBar.color = new Color((1 - _currentHealth / EnemyMaxHealth) * 2, 1, 0, 1);
        else _hpBar.color = new Color(1, _currentHealth / EnemyMaxHealth * 2, 0, 1);
    }


    private void FixedUpdate()
    {
        if (!_canBeAttacked) return;
        _isAttacking = Physics2D.OverlapCircle(transform.position, EnemyDetectionRange, Player);
        if (_isAttacking)
        {
            Attack();
            _isPatrolling = false;
        }
        else if (!_isPatrolling)
        {
            StopAttack();
            _isPatrolling = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Weapon") && _canBeAttacked && !collision.GetComponent<Scyther>().CanBeThrown)
        {
            
            if (!_isStatic) StopAllCoroutines();
            EnemyDetectionRange = float.MaxValue;
            StartCoroutine(TakeDamage(collision.gameObject.GetComponent<Scyther>().AttackDamage, collision.gameObject.transform));
        }
    }

}
