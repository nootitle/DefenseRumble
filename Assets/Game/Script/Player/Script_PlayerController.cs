using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Script_PlayerController : MonoBehaviour
{
    [Header("General")]
    [SerializeField] Rigidbody _rigidBody = null;
    [SerializeField] Animator _animator = null;

    [Header("Movement")]
    [SerializeField] float _landCheckRadius = 0.25f;
    [SerializeField] float _moveSpeed = 1f;
    [SerializeField] float _maxSpeed = 2f;
    [SerializeField] float _jumpSpeed = 5f;
    [SerializeField] float _jumpCoolDown = 0.25f;
    [SerializeField] float _rotateSpeed = 1f;

    Vector3 _velocity = Vector3.zero;
    Vector3 _velocityBuffer = Vector3.zero;
    Vector3 _eulerBuffer= Vector3.zero;
    Vector2 _axis = Vector2.zero;

    float _jumpCoolDownCount = 0f;

    bool _isGround = false;

    [Header("Weapon")]
    [SerializeField] Transform _rightHand = null;
    [SerializeField] Transform _leftHand = null;

    [Header("Sound")]
    [SerializeField] AudioSource _audioSource = null;
    [SerializeField] List<AudioClip> _footStepSE = null;

    int _staticLayer = 1 << 10;

    void Start()
    {
        Init();
    }

    void Update()
    {
        Move();
    }

    void Init()
    {
        if(_rigidBody ==  null)
            _rigidBody = GetComponent<Rigidbody>();

        if (_animator == null)
            _animator = GetComponent<Animator>();
    }

    void Move()
    {
        _isGround = Physics.CheckSphere(this.transform.position, _landCheckRadius, _staticLayer);

        if (_isGround)
        {
            _velocityBuffer = this.transform.forward;
            _velocityBuffer.y = 0f;
            _velocityBuffer.Normalize();

            _axis.x = Input.GetAxis("Horizontal");
            _axis.y = Input.GetAxis("Vertical");

            if (_axis != Vector2.zero)
            {
                _velocity = Vector3.ClampMagnitude(_velocity + _axis.y * _moveSpeed * _velocityBuffer, _maxSpeed);
                _eulerBuffer.y = _axis.x * Mathf.Rad2Deg * _rotateSpeed;

                _animator.SetBool("IsMove", true);
                _animator.SetFloat("JumpState", 0.33f);

                FootStepSE();
            }
            else
            {
                _velocity.x = 0f;
                _velocity.z = 0f;
                _eulerBuffer.y = 0f;

                _animator.SetBool("IsMove", false);
                _animator.SetFloat("JumpState", 0f);
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

        transform.Rotate(_eulerBuffer);
        transform.position += Time.deltaTime * _velocity;
    }

    void CoolDown_Jump()
    {
        if(_jumpCoolDownCount < _jumpCoolDown)
        {
            _jumpCoolDownCount += Time.deltaTime;
        }
    }

    void Weapon_Get()
    {

    }

    void Weapon_Discard()
    {

    }

    void Weapon_Use()
    {

    }

    void FootStepSE()
    {
        if (_audioSource.isPlaying) return;

        _audioSource.clip = _footStepSE[UnityEngine.Random.Range(0, _footStepSE.Count)];
        _audioSource.Play();
    }
}
