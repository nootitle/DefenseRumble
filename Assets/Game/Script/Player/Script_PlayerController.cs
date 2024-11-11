using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Script_PlayerController : MonoBehaviour
{
    [Header("General")]
    [SerializeField] Rigidbody _rigidBody = null;
    [SerializeField] Animator _animator = null;
    [SerializeField] Script_CameraController _cameraController = null;
    [SerializeField] Transform _cameraTransform = null;

    state _state = state.normal;

    public enum state { normal, run, fire, reload }

    Coroutine _stateChangeCo = null;

    [Header("Movement")]
    [SerializeField] float _cameraSpeed = 5f;
    [SerializeField] float _landCheckRadius = 0.25f;
    [SerializeField] float _moveSpeed = 1f;
    [SerializeField] float _maxSpeed = 2f;
    [SerializeField] float _jumpSpeed = 5f;
    [SerializeField] float _jumpCoolDown = 0.25f;
    [SerializeField] float _rotateSpeed = 1f;
    [SerializeField] Transform _center = null;

    float mouseX = 0f;
    float mouseY = 0f;

    Vector3 _velocity = Vector3.zero;
    Vector3 _velocityBuffer = Vector3.zero;
    Vector3 _eulerBuffer = Vector3.zero;
    Vector2 _axis = Vector2.zero;

    float _jumpCoolDownCount = 0f;

    bool _isGround = false;

    [Header("Weapon")]
    [SerializeField] Transform _rightHand = null;
    [SerializeField] Transform _leftHand = null;
    [SerializeField] float _grabRadius_Weapon = 2f;
    [SerializeField] Script_Weapon _weapon = null;

    [Header("Sound")]
    [SerializeField] AudioSource _audioSource = null;
    [SerializeField] AudioSource _audioSource_hit = null;
    [SerializeField] List<AudioClip> _footStepSE = null;
    [SerializeField] List<AudioClip> _hitSE = null;

    void Start()
    {
        Init();
    }

    void Update()
    {
        Move();
        MouseButton();
        Weapon_Reload();
    }

    void Init()
    {
        if (_rigidBody == null)
            _rigidBody = GetComponent<Rigidbody>();

        if (_animator == null)
            _animator = GetComponent<Animator>();
    }

    void Move()
    {
        mouseX += Input.GetAxis("Mouse X") * _cameraSpeed;
        mouseY += Input.GetAxis("Mouse Y") * _cameraSpeed;
        mouseY = Mathf.Clamp(mouseY, -50f, 30f);

        _eulerBuffer.y = mouseX;
        _eulerBuffer.x = -mouseY;

        _isGround = Physics.CheckSphere(this.transform.position, _landCheckRadius, Script_ReferenceHub.instacne.GetLayer(Script_ReferenceHub.Layer.staticObject).layerShift);

        if (_isGround)
        {
            _axis.x = Input.GetAxis("Horizontal");
            _axis.y = Input.GetAxis("Vertical");

            _velocityBuffer = _axis.y * _cameraTransform.forward + _axis.x * _cameraTransform.right;
            _velocityBuffer.y = 0f;
            _velocityBuffer.Normalize();

            if (_axis != Vector2.zero)
            {
                _velocity = Vector3.ClampMagnitude(_velocity + _moveSpeed * _velocityBuffer, _maxSpeed);

                _animator.SetBool("IsMove", true);
                _animator.SetFloat("JumpState", 0.33f);
                _animator.SetFloat("ReloadState", Mathf.Lerp(_animator.GetFloat("ReloadState"), 0.33f, 5f * Time.deltaTime));

                FootStepSE();
            }
            else
            {
                _velocity.x = 0f;
                _velocity.z = 0f;

                _animator.SetBool("IsMove", false);
                _animator.SetFloat("JumpState", 0f);
                _animator.SetFloat("ReloadState", Mathf.Lerp(_animator.GetFloat("ReloadState"), 0f, 5f * Time.deltaTime));
            }

            CoolDown_Jump();

            _velocity.y = 0f;
        }
        else
        {
            _velocity += Physics.gravity * Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Space) && _jumpCoolDownCount >= _jumpCoolDown)
        {
            this.transform.position += Vector3.up * _landCheckRadius;
            _velocity.y = _jumpSpeed;
            _jumpCoolDownCount = 0f;

            _animator.SetTrigger("Jump");
        }

        Vector3 XZ = _velocity;
        XZ.y = 0f;

        if (Physics.SphereCast(_center.position, _landCheckRadius, XZ.normalized, out RaycastHit hit,
            1f, Script_ReferenceHub.instacne.GetLayer(Script_ReferenceHub.Layer.staticObject).layerShift))
        {
            _velocity = hit.normal;
        }

        this.transform.localEulerAngles = _eulerBuffer;
        transform.position += Time.deltaTime * _velocity;
    }

    void CoolDown_Jump()
    {
        if (_jumpCoolDownCount < _jumpCoolDown)
        {
            _jumpCoolDownCount += Time.deltaTime;
        }
    }

    void MouseButton()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (_weapon == null)
                Weapon_Get();
            else
            {
                _cameraController.Zoom(true);
                Weapon_Use();
            }
        }
        else if (Input.GetButton("Fire1"))
        {
            if (_weapon != null)
                Weapon_Use();
        }

        if (Input.GetButtonUp("Fire1"))
        {
            _cameraController.Zoom(false);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Weapon_Discard();
        }
    }

    void Weapon_Get()
    {
        Collider[] weapons = Physics.OverlapSphere(RightHand().position, _grabRadius_Weapon, Script_ReferenceHub.instacne.GetLayer(Script_ReferenceHub.Layer.weapon).layerShift);

        foreach (Collider w in weapons)
        {
            Script_Weapon SC = w.GetComponent<Script_Weapon>();

            if (SC != null)
            {
                _weapon = SC;
                SC.Get();

                StateUpdate(state.run);
            }
        }
    }

    void Weapon_Discard()
    {
        if (_weapon == null) return;

        _weapon.Discard();
        _weapon = null;

        Script_ReferenceHub.instacne.GetUI().SetWeaponSprite(Script_UIManager.WeaponType.none, Script_UIManager.WeaponImage.black);

        StateUpdate(state.normal);
    }

    void Weapon_Use()
    {
        StateUpdate(state.fire);

        _weapon.Use();
    }

    void Weapon_Reload()
    {
        if (Input.GetKeyDown(KeyCode.R) && _weapon != null && _weapon.CanReload())
        {
            StateUpdate(state.reload);
            _weapon.Reload();
        }
    }

    void StateUpdate(state newState)
    {
        switch (newState)
        {
            case state.normal:
                {
                    CorutineForStateChange(0.15f, 0f, 0f);
                    break;
                }
            case state.run:
                {
                    CorutineForStateChange(1f, 0.33f, 0f);
                    break;
                }
            case state.fire:
                {
                    _animator.SetFloat("IdleState", 0.66f);
                    _animator.SetFloat("MoveState", 0.66f);
                    CorutineForStateChange(1f, 0.33f, 0.15f);
                    break;
                }
            case state.reload:
                {
                    if (_animator.GetBool("IsMove"))
                    {
                        _animator.SetFloat("ReloadState", 0.66f);
                        CorutineForStateChange(1f, 0.33f, 1f);
                    }
                    else
                    {
                        _animator.SetFloat("ReloadState", 0.33f);
                        CorutineForStateChange(1f, 0f, 1f);
                    }

                    _animator.SetTrigger("Reload");

                    break;
                }
        }

        _state = newState;
    }

    void CorutineForStateChange(float time, float stateGoal, float delay)
    {
        if (_stateChangeCo != null) StopCoroutine(_stateChangeCo);
        _stateChangeCo = StartCoroutine(GoToIdle(time, stateGoal, delay));
    }

    IEnumerator GoToIdle(float time, float stateGoal, float delay)
    {
        float timeCount = delay;

        while (timeCount > 0)
        {
            yield return null;
            timeCount += -Time.deltaTime;
        }

        timeCount = 0f;

        while (timeCount < time)
        {
            float timeRatio = timeCount / time;

            _animator.SetFloat("IdleState", Mathf.Lerp(_animator.GetFloat("IdleState"), stateGoal, timeRatio));
            _animator.SetFloat("MoveState", Mathf.Lerp(_animator.GetFloat("MoveState"), stateGoal, timeRatio));

            yield return null;

            timeCount += Time.deltaTime;
        }

        _animator.SetFloat("IdleState", stateGoal);
        _animator.SetFloat("MoveState", stateGoal);
    }

    public void Hit()
    {
        _audioSource_hit.clip = _hitSE[UnityEngine.Random.Range(0, _hitSE.Count)];
        _audioSource_hit.Play();
    }

    public Transform LeftHand()
    {
        return _leftHand;
    }

    public Transform RightHand()
    {
        return _rightHand;
    }

    void FootStepSE()
    {
        if (_audioSource.isPlaying) return;

        _audioSource.clip = _footStepSE[UnityEngine.Random.Range(0, _footStepSE.Count)];
        _audioSource.Play();
    }
}
