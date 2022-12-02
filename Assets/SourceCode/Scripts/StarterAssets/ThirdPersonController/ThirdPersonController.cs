/**
	第三人称角色控制器
	控制人物在正常和瞄准状态下的移动，旋转，相机控制
	必要功能，不可拆分
*/

using UnityEngine;
using Sirenix.OdinInspector;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class ThirdPersonController : MonoBehaviour
	{
#region "Public Variables"
		/// <summary>
		/// 控制鼠标旋转的灵敏度
		/// </summary>
		/// <value></value>
		public float Sensitivity 
		{
			get => _sensitivity;
			set => _sensitivity = value;
		}

		public float RotationVelocity
		{
			get => _rotationVelocity;
			set => _rotationVelocity = value;
		}

		public float TimeScale
		{
			get => _timeScale;
			set => _timeScale = value;
		}

		public bool IsInBulletTime
		{
			get => _isInBulletTime;
			set => _isInBulletTime = value;
		}

		/// <summary>
		/// 虚拟摄像机跟随的目标
		/// </summary>
		/// <value></value>
		public Transform VCameraFollowTarget
		{
			get => _lookAtTarget;
		}

		public Vector3 DesiredLookAtTargetPosition { get => _desiredLookAtTargetPosition; }

		public Quaternion DesiredCameraRotation { get => _desiredCameraRotation; }

		[Header("Player")]
		[ReadOnly, Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 2.0f;
		public float NormalMoveSpeed = 2.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 5.335f;
		[Tooltip("How fast the character turns to face movement direction")]
		[Range(0.0f, 0.3f)]
		public float RotationSmoothTime = 0.12f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.50f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.28f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 70.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -30.0f;
		[Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
		public float CameraAngleOverride = 0.0f;
		[Tooltip("For locking the camera position on all axis")]
		public bool LockCameraPosition = false;
#endregion

#region "Private Variables"
		private float _sensitivity = 0.5f;
		private float _timeScale = 1f;
		// cinemachine
		private float _cinemachineTargetYaw;
		private float _cinemachineTargetPitch;
		private Quaternion _desiredCameraRotation;

		// player
		private float _speed;
		private float _animationBlend;
		private float _targetRotation = 0.0f;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;

		// timeout deltatime
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;

		// animation IDs
		private int _animIDSpeed;
		private int _animIDGrounded;
		private int _animIDJump;
		private int _animIDFreeFall;
		private int _animIDMotionSpeed;

		private Animator _animator;
		private CharacterController _characterController;
		private StarterAssetsInputs _input;
		private GameObject _mainCamera;
		private ShootController _shootController;
		private const float _threshold = 0.01f;

		private bool _hasAnimator;

		/// <summary>
		/// 虚拟摄像机跟随的目标
		/// </summary>
		private Transform _lookAtTarget;
		private Vector3 _desiredLookAtTargetPosition;

		// 子弹时间
		private bool _isInBulletTime;

#endregion
		private void Awake()
		{
			Debug.Assert(_animator = GetComponentInChildren<Animator>()				);
			Debug.Assert(_characterController = GetComponent<CharacterController>()	);
			Debug.Assert(_input = GetComponent<StarterAssetsInputs>()				);
			Debug.Assert(_lookAtTarget = GameObject.Find("PlayerCameraRoot").transform	);

			if(!(_shootController = GetComponent<ShootController>()))
			{
				Debug.LogWarning("ShootController Closed");
			}
			
		}

		private void Start()
		{
			// 必须等待相机被生成后才能获取引用
			// get a reference to our main camera
			Debug.Assert(_mainCamera = GameObject.FindGameObjectWithTag("MainCamera"));

			UpdateAnimatorState();

			AssignAnimationIDs();

			// reset our timeouts on start
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;
		}

		private void Update()
		{
			CalculateDesiredCameraRotationAndLookAtPosition();
			UpdateAnimatorState();
			
			JumpAndGravity();
			GroundedCheck();

			Move();
		}

		private void LateUpdate()
		{
			CameraRotation();
		}
		private void UpdateAnimatorState()
		{
			_hasAnimator = _animator != null;
		}

		private void AssignAnimationIDs()
		{
			_animIDSpeed = Animator.StringToHash("Speed");
			_animIDGrounded = Animator.StringToHash("Grounded");
			_animIDJump = Animator.StringToHash("Jump");
			_animIDFreeFall = Animator.StringToHash("FreeFall");
			_animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
		}

		private void GroundedCheck()
		{
			// set sphere position, with offset
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

			// update animator if using character
			if (_hasAnimator)
			{
				_animator.SetBool(_animIDGrounded, Grounded);
			}
		}

		private void CameraRotation()
		{
			// Cinemachine will follow this target
			CinemachineCameraTarget.transform.rotation = _desiredCameraRotation;
		}

		/// <summary>
		/// must be invoked before function: CameraRotation()
		/// </summary>
		private void CalculateDesiredCameraRotationAndLookAtPosition()
		{
			// if there is an input and camera position is not fixed
			if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
			{
				_cinemachineTargetYaw += _input.look.x * Time.deltaTime * _sensitivity;
				_cinemachineTargetPitch += _input.look.y * Time.deltaTime * _sensitivity;
			}

			// clamp our rotations so our values are limited 360 degrees
			_cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
			_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

			Vector3 desiredCameraAngle = new Vector3(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
			_desiredCameraRotation = Quaternion.Euler(desiredCameraAngle);

			// set look at target position
			var curCameraRotation = _mainCamera.transform.rotation;
			_mainCamera.transform.rotation = _desiredCameraRotation;
			_desiredLookAtTargetPosition = _lookAtTarget.position;
			_mainCamera.transform.rotation = curCameraRotation;

			// weapon look at target
			// _weaponIK.LookAtTarget(_input.look.y);
			// if(_input.aim) LookAt(1f);
		}

		private void Move()
		{
			MoveSpeed = _shootController && _input.aim ? _shootController.AimMoveSpeed : NormalMoveSpeed;
			// set target speed based on move speed, sprint speed and if sprint is pressed
			float targetSpeed = !_input.aim && _input.sprint ? SprintSpeed : MoveSpeed;

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (_input.move == Vector2.zero) targetSpeed = 0.0f;

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = new Vector3(_characterController.velocity.x, 0.0f, _characterController.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped, so we don't need to clamp our speed
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

				// round speed to 3 decimal places
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}
			_animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);

			// normalise input direction
			Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			bool rotateWithCamera = _input.aim || (!_input.aim && _input.move != Vector2.zero);
			if ( !_input.aim && _input.move != Vector2.zero )
			{
				// Vector3 cameraEulerAngles = _mainCamera.transform.eulerAngles;
				Vector3 cameraEulerAngles = _desiredCameraRotation.eulerAngles;
				_targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + cameraEulerAngles.y;
				float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

				// rotate to face input direction relative to camera position
				transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
			}
			else if(_input.aim)
			{
				if(_input.move != Vector2.zero)
				{
					// Vector3 cameraEulerAngles = _mainCamera.transform.eulerAngles;
					Vector3 cameraEulerAngles = _desiredCameraRotation.eulerAngles;
					_targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + cameraEulerAngles.y;
				}

				float cameraEulerY = _desiredCameraRotation.eulerAngles.y;
				transform.rotation = Quaternion.Euler(0, cameraEulerY, 0);
			}


			Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

			// move the player
			_characterController.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

			// transform.Translate(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

			// update animator if using character
			if (_hasAnimator)
			{
				_animator.SetFloat(_animIDSpeed, _animationBlend);
				_animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
			}
		}

		private void JumpAndGravity()
		{
			if (Grounded)
			{
				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;

				// update animator if using character
				if (_hasAnimator)
				{
					_animator.SetBool(_animIDJump, false);
					_animator.SetBool(_animIDFreeFall, false);
				}

				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}

				// Jump
				if (!_input.aim && _input.jump && _jumpTimeoutDelta <= 0.0f)
				{
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

					// update animator if using character
					if (_hasAnimator)
					{
						_animator.SetBool(_animIDJump, true);
					}
				}

				if(_input.aim)
				{
					_input.jump = false;
				}

				// jump timeout
				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				// reset the jump timeout timer
				_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}
				else
				{
					// update animator if using character
					if (_hasAnimator)
					{
						_animator.SetBool(_animIDFreeFall, true);
					}
				}

				// if we are not grounded, do not jump
				_input.jump = false;
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}

		public static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;
			
			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}
	}
}