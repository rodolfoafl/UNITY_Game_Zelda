﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZeldaTutorial.Enemies;
using ZeldaTutorial.Player;

namespace ZeldaTutorial.Game{
public class Knockback : MonoBehaviour {

    [SerializeField] float _thrust;
    [SerializeField] float _knockTime;
    [SerializeField] float _damage;

    Enemy _enemy;
    PlayerMovement _player;

    void OnTriggerEnter2D(Collider2D other)
    {
        _player = other.GetComponent<PlayerMovement>();        

        if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Player"))
        {
            Rigidbody2D otherRB = other.GetComponent<Rigidbody2D>();

            if (otherRB != null)
            {
                Vector2 difference = otherRB.transform.position - transform.position;
                difference = difference.normalized * _thrust;
                otherRB.AddForce(difference, ForceMode2D.Impulse);

                if (other.gameObject.CompareTag("Enemy") && other.isTrigger)
                {
                    _enemy = other.GetComponent<Enemy>();
                    _enemy.ChangeState(CharacterState.STAGGER);
                    _enemy.CallKnock(otherRB, _knockTime, _damage);
                    return;
                }

                if (other.gameObject.CompareTag("Player"))
                {
                    if (_player.CurrentState != CharacterState.STAGGER)
                    {
                        _player.ChangeState(CharacterState.STAGGER);
                        _player.CallKnock(otherRB, _knockTime, _damage);
                    }
                    return;
                }                
            }
        }                                                           
    }
}
}
