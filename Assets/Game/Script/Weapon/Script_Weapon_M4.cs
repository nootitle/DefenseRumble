using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Script_Weapon_M4 : Script_Weapon
{
    [Header("Basic Setting")]
    [SerializeField] protected Transform _originalParentTranform = null;
    [SerializeField] protected Vector3 _offsetOnHand = Vector3.zero;
    [SerializeField] protected Vector3 _offsetOnEuler = Vector3.zero;
    [SerializeField] Rigidbody _rigidBody = null;

    [Header("Fire")]
    [SerializeField] float _fireDamage = 25f;
    [SerializeField] float _fireRange = 20f;
    [SerializeField] float _attenAlongRange = 1f;
    [SerializeField] float _reactionTimeWhenFire = 1f;
    [SerializeField] Vector3 _localPositionWhenFire = Vector3.zero;
    [SerializeField] Vector3 _localEulerWhenFire = Vector3.zero;
    [SerializeField] Transform _muzzle = null;

    Vector3 _localPositionGoal = Vector3.zero;
    Vector3 _localEulerGoal = Vector3.zero;
    float _reactionTimeCount = 0f;
    bool _reactioning = false;

    [Header("SE")]
    [SerializeField] AudioSource _audioSource = null;

    public override void Get()
    {
        _rigidBody.isKinematic = true;

        this.transform.SetParent(ReferenceManager.instacne.GetPlayer(ReferenceManager.Player.local).RightHand());
        this.transform.localPosition = _offsetOnHand;
        this.transform.localEulerAngles = _offsetOnEuler;
    }

    public override void Discard()
    {
        this.transform.SetParent(_originalParentTranform);

        _rigidBody.isKinematic = false;
    }

    public override void Use()
    {
        if (_reactionTimeCount < _reactionTimeWhenFire) return;

        _reactionTimeCount = 0f;

        _reactioning = true;

        ReferenceManager.instacne.PlayMuzzleEffect(ReferenceManager.Fx.muzzleEffect, _muzzle);
        _audioSource.Play();
    }

    void ReactionWhenFire()
    {
        if (!_reactioning) return;

        if (_reactionTimeCount < _reactionTimeWhenFire)
        {
            this.transform.localPosition = Vector3.Lerp(_offsetOnHand, _localPositionWhenFire, _reactionTimeCount / _reactionTimeWhenFire);
            this.transform.localEulerAngles = Vector3.Lerp(_offsetOnEuler, _localEulerWhenFire, _reactionTimeCount / _reactionTimeWhenFire);

            _reactionTimeCount += Time.deltaTime;
        }
        else
        {
            this.transform.localPosition = _offsetOnHand;
            this.transform.localEulerAngles = _offsetOnEuler;

            _reactioning = false;
        }
    }

    void CoolDown()
    {
        if (_reactionTimeCount < _reactionTimeWhenFire)
        {
            _reactionTimeCount += Time.deltaTime;
        }
    }

    private void Update()
    {
        ReactionWhenFire();
        CoolDown();
    }
}
