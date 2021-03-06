﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZeldaTutorial.Objects;

public enum DoorType{
	KEY,
	ENEMY,
	BUTTON
}

public class Door : Interactable {

    [Header("Attributes")]
	[SerializeField] DoorType _doorType;

    [Header("PlayerInventory")]
	[SerializeField] Inventory _playerInventory;
	
	SpriteRenderer _doorSprite;
	BoxCollider2D[] _doorColliders;
	bool _isOpen = false;

	void Start(){
		_doorSprite = GetComponentInParent<SpriteRenderer>();
		_doorColliders = GetComponentsInParent<BoxCollider2D>();
	}

	void Update()
	{
		if(Input.GetButtonDown("Attack") && !_isOpen)
		{
			if(PlayerInRange && _doorType == DoorType.KEY)			
			{
				if(_playerInventory.NumberOfKeys > 0)
				{
					_playerInventory.NumberOfKeys--;
					Open();
				}
			}
		}
	}

	public void Open()
	{
		_isOpen = true;
		_doorSprite.enabled = false;
		foreach(BoxCollider2D col in _doorColliders){
			col.enabled = false;
		}
	}

	public void Close()
	{
        _isOpen = false;
        _doorSprite.enabled = true;
        foreach (BoxCollider2D col in _doorColliders)
        {
            col.enabled = true;
        }
    }
}
