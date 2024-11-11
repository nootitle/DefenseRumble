using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Script_Weapon_M4 : Script_Weapon
{
    [Header("Basic Setting")]
    [SerializeField] Transform _originalParentTranform = null;
    [SerializeField] Vector3 _offsetOnHand = Vector3.zero;
    [SerializeField] Vector3 _offsetOnEuler = Vector3.zero;
    [SerializeField] Rigidbody _rigidBody = null;

    [Header("Fire")]
    [SerializeField] float _fireDamage = 25f;
    [SerializeField] float _fireRange = 20f;
    [SerializeField] float _bulletRadius = 0.1f;
    [SerializeField] float _attenAlongRange = 1f;
    [SerializeField] float _reactionTimeWhenFire = 1f;
    [SerializeField] Vector3 _localPositionWhenFire = Vector3.zero;
    [SerializeField] Vector3 _localEulerWhenFire = Vector3.zero;
    [SerializeField] Transform _muzzle = null;

    Vector3 _localPositionGoal = Vector3.zero;
    Vector3 _localEulerGoal = Vector3.zero;
    float _reactionTimeCount = 0f;
    bool _reactioning = false;

    [Header("Reload")]
    [SerializeField] int _maxAmmo = 30;
    [SerializeField] float _reloadTime = 1f;

    int _currentAmmo = 30;
    bool _reloading = false;
    Coroutine _reloadCo = null;

    [Header("SE")]
    [SerializeField] AudioSource _audioSource = null;
    [SerializeField] AudioClip _fireSE = null;
    [SerializeField] AudioClip _reloadSE = null;

    public override void Get()
    {
        _rigidBody.isKinematic = true;

        this.transform.SetParent(Script_ReferenceHub.instacne.GetPlayer(Script_ReferenceHub.Player.local).RightHand());
        this.transform.localPosition = _offsetOnHand;
        this.transform.localEulerAngles = _offsetOnEuler;

        if(_currentAmmo > 0)
            Script_ReferenceHub.instacne.GetUI().SetWeaponSprite(Script_UIManager.WeaponType.M4, Script_UIManager.WeaponImage.yellow);
        else
            Script_ReferenceHub.instacne.GetUI().SetWeaponSprite(Script_UIManager.WeaponType.M4, Script_UIManager.WeaponImage.black);
    }

    public override void Discard()
    {
        CancelReload();

        this.transform.SetParent(_originalParentTranform);

        _rigidBody.isKinematic = false;
    }

    public override void Use()
    {
        if (_reactionTimeCount < _reactionTimeWhenFire || _currentAmmo <= 0) return;

        _reactionTimeCount = 0f;

        _reactioning = true;

        Fire();

        Script_ReferenceHub.instacne.PlayEffect(Script_ReferenceHub.Fx.muzzleEffect, _muzzle);

        _audioSource.clip = _fireSE;
        _audioSource.Play();

        --_currentAmmo;

        if(_currentAmmo <= 0)
            Script_ReferenceHub.instacne.GetUI().SetWeaponSprite(Script_UIManager.WeaponType.M4, Script_UIManager.WeaponImage.black);
    }

    void Fire()
    {
        if (Physics.Raycast(Script_ReferenceHub.instacne.GetAimPoint(), Script_ReferenceHub.instacne.GetAimForward(), out RaycastHit hit, _fireRange))
        {
            if (hit.collider.gameObject.layer == Script_ReferenceHub.instacne.GetLayer(Script_ReferenceHub.Layer.enemy).layerInt)
            {
                Script_Enemy SE = hit.transform.GetComponent<Script_Enemy>();
     
                if (SE != null && SE.spawned)
                {
                    SE.Hit();
                }
            }
            else if (hit.collider.gameObject.layer == Script_ReferenceHub.instacne.GetLayer(Script_ReferenceHub.Layer.player).layerInt)
            {

            }
            else if (hit.collider.gameObject.layer == Script_ReferenceHub.instacne.GetLayer(Script_ReferenceHub.Layer.staticObject).layerInt)
            {
                Script_ReferenceHub.instacne.PlayEffect(Script_ReferenceHub.Fx.bulletHoleSmoke, hit.point, hit.normal);
            }
            else if (hit.collider.gameObject.layer == Script_ReferenceHub.instacne.GetLayer(Script_ReferenceHub.Layer.weapon).layerInt)
            {

            }
        }
        else if (Physics.SphereCast(Script_ReferenceHub.instacne.GetAimPoint(), _bulletRadius, Script_ReferenceHub.instacne.GetAimForward(), out RaycastHit hit2, _fireRange))
        {
            if (hit2.collider.gameObject.layer == Script_ReferenceHub.instacne.GetLayer(Script_ReferenceHub.Layer.enemy).layerInt)
            {
                Script_Enemy SE = hit2.transform.GetComponent<Script_Enemy>();
   
                if (SE != null && SE.spawned)
                {
                    SE.Hit();
                }
            }
            else if (hit2.collider.gameObject.layer == Script_ReferenceHub.instacne.GetLayer(Script_ReferenceHub.Layer.player).layerInt)
            {

            }
            else if (hit2.collider.gameObject.layer == Script_ReferenceHub.instacne.GetLayer(Script_ReferenceHub.Layer.staticObject).layerInt)
            {
                Script_ReferenceHub.instacne.PlayEffect(Script_ReferenceHub.Fx.bulletHoleSmoke, hit2.point, hit2.normal);
            }
            else if (hit2.collider.gameObject.layer == Script_ReferenceHub.instacne.GetLayer(Script_ReferenceHub.Layer.weapon).layerInt)
            {

            }
        }
    }

    public override bool CanReload()
    {
        return _currentAmmo < _maxAmmo && !_reloading;
    }

    public override void Reload()
    {
        if (_reloading) return;

        _reloading = true;
        _currentAmmo = 0;

        _audioSource.clip = _reloadSE;
        _audioSource.Play();

        if (_reloadCo != null) StopCoroutine(_reloadCo);
        _reloadCo = StartCoroutine(ReloadCorutine());
    }

    public override void CancelReload()
    {
        if (_reloadCo != null) StopCoroutine(_reloadCo);
        _reloading = false;
    }

    IEnumerator ReloadCorutine()
    {
        float timeCount = 0f;

        while (timeCount < _reloadTime)
        {
            yield return null;

            timeCount += Time.deltaTime;
        }

        Script_ReferenceHub.instacne.GetUI().SetWeaponSprite(Script_UIManager.WeaponType.M4, Script_UIManager.WeaponImage.yellow);

        _currentAmmo = _maxAmmo;
        _reloading = false;
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
