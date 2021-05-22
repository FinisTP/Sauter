using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class WhiteGhost : Enemy
{
    protected override void Attack()
    {
        StopAllCoroutines();
        GetComponent<AIDestinationSetter>().target = GameManager_.Instance.Player.transform;
        
    }

    protected override IEnumerator Patrol()
    {
        while (true)
        {
            while (Vector2.Distance(transform.position, PatrolWaypoints[_currentWaypointIndex].position) > 0.5f)
            {
                GetComponent<AIDestinationSetter>().target = PatrolWaypoints[_currentWaypointIndex];
                yield return new WaitForSeconds(1f);
            }
            _currentWaypointIndex = (_currentWaypointIndex + 1) % PatrolWaypoints.Count;
            yield return null;

        }
    }

    protected override void StopAttack()
    {
        
        StopAllCoroutines();
        StartCoroutine(Patrol());
    }
}
