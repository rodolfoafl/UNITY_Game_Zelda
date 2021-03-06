﻿using System.Collections;
using UnityEngine;

namespace ZeldaTutorial.Player
{
    public class PlayerMovement : MonoBehaviour {

        [Header("Weapons")]
        [SerializeField] GameObject _arrow;
        [SerializeField] Item _bow;

        [Header("Movement")]
        [SerializeField] float _speed;

        [Header("Stamina")]
        [SerializeField] float _maxStamina;

        [Header("Signals")]
        //[SerializeField] Signal _playerHealthSignal;
        [SerializeField] Signal _screeKickSignal;
        [SerializeField] Signal _secondaryAttackSignal;

        [Header("ScriptableObjects")]
        //[SerializeField] FloatValue _currentHealth;
        [SerializeField] Vector2Value _transitionStartingPosition;

        [Header("Inventory")]
        [SerializeField] Inventory _inventory;
        [SerializeField] SpriteRenderer _collectedItemSprite;

        [Header("IFrame")]
        [SerializeField] Color _flashColor;
        [SerializeField] Color _regularColor;
        [SerializeField] float _flashDuration;
        [SerializeField] int _numberOfFlashes;
        [SerializeField] Collider2D _triggerCollider;

        SpriteRenderer _spriteRenderer;

        Rigidbody2D _rigidbody;

        Vector3 _change;

        Animator _animator;

        CharacterState _currentState;

        float _currentStamina;

        #region Properties
        public CharacterState CurrentState
        {
            get
            {
                return _currentState;
            }

            protected set
            {
                _currentState = value;
            }
        }

        public float MaxStamina
        {
            get
            {
                return _maxStamina;
            }
        }

        public float CurrentStamina
        {
            get
            {
                return _currentStamina;
            }

            set
            {
                _currentStamina += value;
                if(_currentStamina > MaxStamina)
                {
                    _currentStamina = MaxStamina;
                }
            }
        }
        #endregion

        void Start () {
            _currentState = CharacterState.WALK;
            _rigidbody = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _animator.SetFloat("moveX", 0);
            _animator.SetFloat("moveY", -1);
            transform.position = _transitionStartingPosition.InitialValue;

            _currentStamina = MaxStamina;
        }
	
	    void Update () {
            if(CurrentState == CharacterState.INTERACT)
            {
                return;
            }

            _change = Vector2.zero;
            _change.x = Input.GetAxisRaw("Horizontal");
            _change.y = Input.GetAxisRaw("Vertical");

            if(Input.GetButtonDown("Attack") && _currentState != CharacterState.ATTACK && _currentState != CharacterState.STAGGER)
            {
                StartCoroutine(Attack());
            }
            else if(Input.GetButtonDown("Secondary Attack") && _currentState != CharacterState.ATTACK && _currentState != CharacterState.STAGGER)
            {
                if (_inventory.CheckForItem(_bow))
                {
                    if (CurrentStamina > 0)
                    {
                        CurrentStamina = -1;
                        StartCoroutine(SecondaryAttack());
                    }
                }
            }
            else if (_currentState == CharacterState.WALK || _currentState == CharacterState.IDLE)
            {
                UpdateAnimationAndMove();
            }
	    }

        public void ChangeState(CharacterState newState)
        {
            if (CurrentState != newState)
            {
                CurrentState = newState;
            }
        }

        IEnumerator Attack()
        {
            _animator.SetBool("attacking", true);
            ChangeState(CharacterState.ATTACK);
            yield return null;
            _animator.SetBool("attacking", false);
            yield return new WaitForSeconds(0.3f);
            if(CurrentState != CharacterState.INTERACT)
            {
                ChangeState(CharacterState.WALK);
            }
        }

        IEnumerator SecondaryAttack()
        {
            _secondaryAttackSignal.Raise();
            ChangeState(CharacterState.ATTACK);
            yield return null;
            CreateArrow();
            yield return new WaitForSeconds(0.3f);
            if (CurrentState != CharacterState.INTERACT)
            {
                ChangeState(CharacterState.WALK);
            }
        }

        void CreateArrow()
        {
            Vector2 velocity = new Vector2(_animator.GetFloat("moveX"), _animator.GetFloat("moveY"));

            GameObject arrow = Instantiate(_arrow, transform.position, Quaternion.identity);
            if(arrow.GetComponent<Arrow>() != null)
            {
                arrow.GetComponent<Arrow>().Setup(velocity, SetArrowDirection());
            }
        }

        Vector3 SetArrowDirection()
        {
            float degree = Mathf.Atan2(_animator.GetFloat("moveY"), _animator.GetFloat("moveX")) * Mathf.Rad2Deg;
            return new Vector3(0, 0, degree);
        }

        public void CollectItem()
        {
            if(_inventory.CurrentItem != null){
                if(CurrentState != CharacterState.INTERACT)
                {
                    _animator.SetBool("getItem", true);
                    ChangeState(CharacterState.INTERACT);
                    _collectedItemSprite.sprite = _inventory.CurrentItem.ItemSprite;
                }
                else
                {
                    _animator.SetBool("getItem", false);
                    ChangeState(CharacterState.IDLE);
                    _collectedItemSprite.sprite = null;
                    _inventory.CurrentItem = null;
                }
            }
        }

        void UpdateAnimationAndMove()
        {
            if (_change != Vector3.zero)
            {
                MoveCharacter();
                _change.x = Mathf.Round(_change.x);
                _change.y = Mathf.Round(_change.y);
                ChangeState(CharacterState.WALK);
            }
            else
            {
                _animator.SetBool("moving", false);
                ChangeState(CharacterState.IDLE);
            }
        }

        void MoveCharacter()
        {
            _rigidbody.MovePosition(transform.position + _change.normalized * _speed * Time.deltaTime);
            _animator.SetFloat("moveX", _change.x);
            _animator.SetFloat("moveY", _change.y);
            _animator.SetBool("moving", true);
        }

        //NOTE: These methods are duplicated on Enemy script.
        //In the future, it would be better to centralize this logic in just one place!
        public void CallKnock(Rigidbody2D knockedRB, float knockTime)
        {
            StartCoroutine(Knock(knockedRB, knockTime));
        }

        IEnumerator Knock(Rigidbody2D knockedRB, float knockTime)
        {
            _screeKickSignal.Raise();
            if (knockedRB != null)
            {
                StartCoroutine(Flash());
                yield return new WaitForSeconds(knockTime);
                knockedRB.velocity = Vector2.zero;
                _currentState = CharacterState.IDLE;
            }
        }

        IEnumerator Flash()
        {
            int count = 0;
            _triggerCollider.enabled = false;
            while(count < _numberOfFlashes)
            {
                _spriteRenderer.color = _flashColor;
                yield return new WaitForSeconds(_flashDuration);
                _spriteRenderer.color = _regularColor;
                yield return new WaitForSeconds(_flashDuration);
                count++;
            }
            _triggerCollider.enabled = true;
        }
    }
}
