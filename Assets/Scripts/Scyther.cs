using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scyther : MonoBehaviour
{
    public float MaxThrowDuration = 2f;
    public float AttackRadius = 1f;
    public float AttackDamage = 1f;
    public bool CanBeThrown = true;
    public bool CanBeRetrieved = false;

    private float _currentTime = 0f;
    private bool _goBack = false;

    public LayerMask EnemyLayer;
    public ParticleSystem ScytherParticle;

    [SerializeField] private float _throwDelay = 0.05f;

    private void Start()
    {
        ScytherParticle.gameObject.SetActive(false);
    }
    public void GoToTarget(Vector2 target)
    {
        transform.parent = null;
        if (!CanBeThrown) return;
        CanBeThrown = false;
        CanBeRetrieved = false;
        GetComponent<Animator>().enabled = true;
        GetComponent<Animator>().Play("Weapon");
        
        StartCoroutine(Shoot(target));
    }

    IEnumerator Shoot(Vector2 target)
    {
        ScytherParticle.gameObject.SetActive(true);
        float time = 0;
        Vector2 startPosition = transform.position;

        while (time < _throwDelay)
        {
            transform.position = Vector2.Lerp(startPosition, target, time / _throwDelay);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
        CanBeRetrieved = true;

        while (time < MaxThrowDuration && !_goBack)
        {
            time += Time.deltaTime;
            yield return null;
        }

        time = 0;
        startPosition = transform.position;
        while (Vector2.Distance(transform.position, GameManager_.Instance.Player.transform.position) > 0.5f)
        {
            transform.position = Vector2.Lerp(startPosition, GameManager_.Instance.Player.transform.position, time / _throwDelay);
            time += Time.deltaTime;
            yield return null;
        }

        ScytherParticle.gameObject.SetActive(false);
        CanBeThrown = true;
        CanBeRetrieved = false;
        GetComponent<Animator>().enabled = false;
        transform.parent = GameManager_.Instance.Player.transform.Find("WeaponHolder");
        _goBack = false;   
    }


    public void GoBackToOwner()
    {
        _goBack = true;
        CanBeRetrieved = false;
    }

    private void Update()
    {
        ScytherParticle.gameObject.transform.position = transform.position;
        _currentTime += Time.deltaTime;
        if (CanBeThrown)
        {
            transform.localPosition = new Vector2(0, 0);
            transform.rotation = Quaternion.identity;
            transform.localScale = new Vector3(1, 1, 1);
        } else
        {
            //Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, AttackRadius, EnemyLayer);
            //foreach (Collider2D hit in hitEnemies)
            //{
            //    hit.gameObject.GetComponent<Rigidbody2D>().AddForce()
            //}
        }
    }
}
