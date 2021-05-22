using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenGhost : Enemy
{
    public float ShootSpeed;
    public float ShootDelay;
    public float TeleportDelay;
    public GameObject Bullet;

    [SerializeField] private bool _canAttack = true;
    [SerializeField] private bool _canPatrol = true;
    protected override void Attack()
    {
        if (_canAttack) {
            // StopAllCoroutines();

            StartCoroutine(Shoot());
        } 
    }

    IEnumerator Shoot()
    {
        _canAttack = false;
        GameObject go = Instantiate(Bullet, transform.position, transform.rotation);
        go.GetComponent<Rigidbody2D>().velocity = (GameManager_.Instance.Player.transform.position - transform.position).normalized * ShootSpeed;
        yield return new WaitForSeconds(ShootDelay);
        _canAttack = true;
        
    }

    protected override IEnumerator Patrol()
    {
        _isStatic = true;
        _canPatrol = false;
        if (WaypointObject != null)
        {
            int index = Random.Range(0, PatrolWaypoints.Count);
            
            while (index == _currentWaypointIndex) index = Random.Range(0, PatrolWaypoints.Count);
            _currentWaypointIndex = index;
            Vector2 newDest = PatrolWaypoints[_currentWaypointIndex].transform.position;
            transform.position = newDest;
        }
        yield return new WaitForSeconds(TeleportDelay);
        _canPatrol = true;
    }

    protected override void StopAttack()
    {
        if (_canPatrol)
        {
            // StopAllCoroutines();
            StartCoroutine(Patrol());
        }   
    }

}
