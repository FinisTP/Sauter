using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System.Collections;
using UnityEngine.UI;

public class CharacterController2D : MonoBehaviour
{
	// ========= PLAYER STATE ========== //
	[Header("Player State")]
	public float MaxHealth;
	public float DamagedCooldown;
	private float _currentHealth;
	private bool _canBeAttacked = true;
	public Slider HPSlider;
	public Image HPColor;

	// ========= MOVEMENT ========== //
	[Header("Movement Settings")]
	private float _move = 0;
	[SerializeField] [Range(0, 3)] private float MoveSpeed = 5f;                              // Player default speed
	[Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;  // How much to smooth out the movement

	// ========= GROUND & AIR STATE ========== //
	[Header("Ground & Air check")]
	[SerializeField] private bool m_AirControl = true;                         // Whether or not a player can steer while jumping;
	[SerializeField] private LayerMask m_WhatIsGround;                          // A mask determining what is ground to the character
	[SerializeField] private Transform m_GroundCheck;                           // A position marking where to check if the player is grounded.
	[SerializeField] private Transform m_CeilingCheck;                          // A position marking where to check for ceilings
	public float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
	public float k_WallCheckRadius = .2f;
	[SerializeField] private bool m_Grounded;            // Whether or not the player is grounded.
	[SerializeField] private bool _hitCeiling;
	private Rigidbody2D _rb;
	private bool m_FacingRight = true;  // For determining which way the player is currently facing.
	private Vector3 m_Velocity = Vector3.zero;

	// ========= JUMPING ========== //
	[Header("Jumping")]
	[SerializeField] private float JumpForce = 20f;   // Amount of force added when the player jumps.
	[SerializeField] private float _jumpTime = 1f;		// Jump smoothing
	private float _jumpTimeCounter = 0f;
	[SerializeField] private float _maxJumpHeight = 3f;
	private float _oldJumpHeight;
	[SerializeField] private bool _jump = false;

	// ========= WALL CLIMBING ========== //
	[Header("Wall climbing")]
	private bool _isTouchingFront;
	public Transform FrontCheck;
	[SerializeField]  private bool _wallSliding;
	public float WallSlidingSpeed;
	[SerializeField] private LayerMask m_WhatIsWall;                          // A mask determining what is wall to the character
	[SerializeField]  private bool _wallJumping;
	public float WallForceX;
	public float WallForceY;
	public float WallJumpTime;
	private bool _canSlide = true;

	// ========= ATTACKING ========== //
	[Header("Attacking")]
	public Scyther Weapon;
	public float AttackRange;
	public float AttackPower;

	// ========= SWIMMING ========== //
	[Header("Swimming")]
	public float SwimSpeed;
	public LayerMask WaterMask;
	[SerializeField] private bool _isUnderwater;

	// ========= DASHING ========== //
	[Header("Dashing")]
	public float DashPower;
	public float DashDelay;
	public GameObject DashEffect;
	private bool _canDash = true;

	// ========= AIMING ========== //
	[Header("Aiming")]
	public float AimRadius;
	public GameObject AimTarget;
	public LayerMask m_EnemyLayer;
	private bool _isAiming = false;
	public GameObject Crosshair;

	// ========= LADDER CLIMBING ========== //
	[Header("Ladder Climbing")]
	public float ClimbingSpeed;
	public LayerMask LadderLayer;
	[SerializeField] private bool _isClimbing;
	private float _moveLadder;

	// ========= PARTICLE EFFECT ========== //
	[Header("Particle Effect")]
	public ParticleSystem MoveParticleFeet;
	public ParticleSystem MoveParticleHand;

	[Header("Events")]
	[Space]
	public UnityEvent OnLandEvent; // For adding landing particle effect

	private void Awake()
	{
		_currentHealth = MaxHealth;
		_rb = GetComponent<Rigidbody2D>();

		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();
	}

	public void UpdateHealth()
    {
		HPSlider.value = (int)_currentHealth / MaxHealth;
		if (_currentHealth / MaxHealth >= 0.5f)
			HPColor.color = new Color((1 - _currentHealth / MaxHealth) * 2, 1, 0, 1);
		else HPColor.color = new Color(1, _currentHealth / MaxHealth * 2, 0, 1);
	}


    private void Update()
    {
		//Debug.DrawRay(transform.position, AttackRange * Vector2.right * transform.localScale.x);
		UpdateHealth();

		if (m_Grounded) _oldJumpHeight = transform.position.y;

			_move = Input.GetAxisRaw("Horizontal") * MoveSpeed;

		if ((m_Grounded || _wallSliding || _isUnderwater) && Input.GetKeyDown(KeyCode.Space))
		{
			if (_wallSliding)
            {
				StartCoroutine(WallJump());
            }
			_jump = true;
			_jumpTimeCounter = _jumpTime;
			if (_isUnderwater) Jump(SwimSpeed);
			else Jump(JumpForce);
		}

		if (Input.GetKey(KeyCode.Space) && _jump)
		{
			if (_isUnderwater || (_jumpTimeCounter > 0 && transform.position.y - _oldJumpHeight <= _maxJumpHeight))
			{
				if (_isUnderwater) Jump(SwimSpeed);
				else Jump(JumpForce);
				_jumpTimeCounter -= Time.deltaTime;
			}
			else
			{
				_jump = false;
			}
		}

		if (Input.GetKeyUp(KeyCode.Space) || _hitCeiling) _jump = false;

		_isTouchingFront = Physics2D.OverlapCircle(FrontCheck.position, k_WallCheckRadius, m_WhatIsWall);
		if (_isTouchingFront && !m_Grounded && _move != 0 && _canSlide && !_isClimbing && !_isUnderwater)
		{
			_wallSliding = true;
		}
		else _wallSliding = false;

		if (_wallSliding)
		{
			_rb.velocity = new Vector2(_rb.velocity.x, Mathf.Clamp(_rb.velocity.y, -WallSlidingSpeed, float.MaxValue));
		}


		if (Input.GetKeyDown(KeyCode.X))
        {
			if (Weapon.CanBeThrown)
			{
				
				int wallDir = _wallSliding ? -1 : 1;
				// RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right * transform.localScale.x * wallDir, AttackRange, m_WhatIsGround);
				Vector2 target;
				if (_isAiming && AimTarget != null) target = AimTarget.transform.position;
				else target = new Vector2(transform.position.x + AttackRange * transform.localScale.x * wallDir, transform.position.y);
				Weapon.GoToTarget(target);
			}
			else if (Weapon.CanBeRetrieved) Weapon.GoBackToOwner();
        }

		if (Input.GetKeyDown(KeyCode.Z) && (m_Grounded || (m_AirControl && !_wallSliding)) && _canDash && !_isClimbing)
        {
			StartCoroutine(Dash());
        }

		if (Input.GetKey(KeyCode.LeftShift)) _isAiming = true;
		else _isAiming = false;

	}

	public IEnumerator TakeDamage(float damage)
	{
		_canBeAttacked = false;
		_currentHealth -= damage;
		if (_currentHealth <= 0)
		{
			_currentHealth = 0;
			GameManager_.Instance.GameOver = true;
		}

		GetComponent<Animator>().enabled = true;
		GetComponent<Animator>().Play("PlayerAttacked");
		yield return new WaitForSeconds(DamagedCooldown);
		GetComponent<Animator>().enabled = false;
		_canBeAttacked = true;
	}

	IEnumerator WallJump ()
    {
		_canSlide = false;
		MoveParticleHand.Play();
		if ((_move > 0 && m_FacingRight) || (_move < 0 && !m_FacingRight))
			_rb.velocity = new Vector2(WallForceX * -_move, WallForceY);
		yield return new WaitForSeconds(WallJumpTime);
		_canSlide = true;
    }

	IEnumerator Dash()
    {
		// _canBeAttacked = false;
		DashEffect.SetActive(true);
		_canDash = false;
		_rb.velocity = new Vector2(DashPower * transform.localScale.x, _rb.velocity.y);
		MoveParticleFeet.Play();
		yield return new WaitForSeconds(0.1f);
		DashEffect.SetActive(false);
		// _canBeAttacked = true;
		
		yield return new WaitForSeconds(DashDelay);
		_canDash = true;
    }

	IEnumerator TransitionToGroundless()
    {
		yield return new WaitForSeconds(0.1f);
		m_Grounded = false;
    }

    private void FixedUpdate()
	{
		bool wasGrounded = m_Grounded;
		
		bool groundCheck = Physics2D.OverlapCircle(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
		if (groundCheck)
		{
			m_Grounded = true;
			if (!wasGrounded)
			MoveParticleFeet.Play();
		}
		else StartCoroutine(TransitionToGroundless());
		Move(_move);

		_hitCeiling = Physics2D.OverlapCircle(m_CeilingCheck.position, k_GroundedRadius, m_WhatIsGround);
		
		if (_isUnderwater) _rb.gravityScale = 0.3f;
		else _rb.gravityScale = 3f;

		if (_isAiming)
		{
			Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, AimRadius, m_EnemyLayer);
			if (targets.Length > 0) AimTarget = targets
				.OrderBy(c => Vector2.Distance(c.transform.position, transform.position))
				.First().gameObject;
			if (AimTarget != null)
            {
				Crosshair.SetActive(true);
				Crosshair.transform.position = AimTarget.transform.position;
			}
			
		}
		else
        {
			Crosshair.SetActive(false);
			AimTarget = null;
		}

		RaycastHit2D hitInfo = Physics2D.Raycast(m_GroundCheck.position, Vector2.down, 0.5f, LadderLayer);
		if (hitInfo.collider != null)
        {
			_canSlide = false;
			_wallSliding = false;
			// print("Not null");
			if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow))
            {
				_isClimbing = true;
            }
        } else { _isClimbing = false; _canSlide = true; }

		if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.3f) _isClimbing = false;

		if (_isClimbing)
        {
			
			_moveLadder = Input.GetAxisRaw("Vertical");
			_rb.velocity = new Vector2(_rb.velocity.x, _moveLadder * ClimbingSpeed);
			_rb.gravityScale = 0;
        } else if (!_isUnderwater)
        {
			_rb.gravityScale = 3f;
        }
	}

	public void Move(float move)
	{
		if (m_Grounded || (m_AirControl && !_wallSliding))
		{
			Vector3 targetVelocity = new Vector2(move * 10f, _rb.velocity.y);
			_rb.velocity = Vector3.SmoothDamp(_rb.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

			if ((move > 0 && !m_FacingRight) || (move < 0 && m_FacingRight))
			{
				MoveParticleFeet.Play();
				m_FacingRight = !m_FacingRight;

				Vector3 characterScale = transform.localScale;
				characterScale.x = m_FacingRight ? 1 : -1;
				transform.localScale = characterScale;
			}
		}
		

	}
	public void Jump(float force)
    {
		MoveParticleFeet.Play();
		_rb.velocity = new Vector2(_rb.velocity.x, force);
	}

	public void Recover(float amount)
    {
		_currentHealth += amount;
		if (_currentHealth > MaxHealth) _currentHealth = MaxHealth;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
		if (collision.gameObject.CompareTag("Enemy") && _canBeAttacked)
		{
			StartCoroutine(TakeDamage(collision.gameObject.GetComponent<Enemy>().EnemyDamage));
		}
		else if (collision.gameObject.CompareTag("EnemyBullet") && _canBeAttacked)
		{
			StartCoroutine(TakeDamage(0.5f));
		} 
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
		if (collision.gameObject.CompareTag("Water")) _isUnderwater = true;
	}

    private void OnCollisionStay2D(Collision2D collision)
    {
		if (collision.gameObject.CompareTag("Spike") && _canBeAttacked)
		{
			print("Spikiee");
			StartCoroutine(TakeDamage(1f));
		}
	}

    private void OnTriggerExit2D(Collider2D collision)
    {
		if (collision.gameObject.CompareTag("Water")) _isUnderwater = false;
	}


}