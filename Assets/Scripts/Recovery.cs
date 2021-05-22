using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recovery : MonoBehaviour
{
    public float HealthRecovered;
    public float CooldownTime; // 0: no respawn

    private float _currentTime = 0;

    private void Update()
    {
        _currentTime += Time.deltaTime;
        if (_currentTime < CooldownTime) GetComponent<SpriteRenderer>().color = new Color(75 / 255, 75 / 255, 75 / 255, 1);
        else GetComponent<SpriteRenderer>().color = new Color(1,1,1,1);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && _currentTime >= CooldownTime)
        {
            GameManager_.Instance.Player.GetComponent<CharacterController2D>().Recover(HealthRecovered);
            
            _currentTime = 0;
        }
    }

}
