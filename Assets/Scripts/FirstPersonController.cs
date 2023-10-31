using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class FirstPersonController : MonoBehaviour
	{
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 6.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 6.0f;
		[Tooltip("Crouch speed of the character in m/s")]
		public float CrouchSpeed = 3.0f;
		[Tooltip("Rotation speed of the character")]
		public float RotationSpeed = 1.0f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.1f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;

		[Header("Weapon")]
		[Tooltip("Weapon Transform Reference")]
		public Transform wrench;
		[Tooltip("Resting position/rotation for weapon")]
		public Transform startT;
		[Tooltip("Position/rotation to set to for animation")]
		public Transform endT;
		public LayerMask raycastLayers;
		private bool shmacked = false;
		private float shmAnimTimer;
		private float shmAnimLength = 1f;

		//that game jam type of implementation
		[Header("gjpump")]
		public Light pumpLight;
		private bool pumpActive = true;
		private float pumpActiveTimer = 15f;

		private AudioSource mp3Player;
		public AudioClip tHit;
		public AudioClip hFix;
		public AudioClip pFix;
		public AudioClip pBreak;

		// cinemachine
		private float _cinemachineTargetPitch;

		// player
		private float _speed;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;

		// timeout deltatime
		//private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;

	
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
		private PlayerInput _playerInput;
#endif
		private CharacterController _controller;
		private StarterAssetsInputs _input;
		private GameObject _mainCamera;

		private const float _threshold = 0.01f;

		private bool IsCurrentDeviceMouse
		{
			get
			{
				#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
				return _playerInput.currentControlScheme == "KeyboardMouse";
				#else
				return false;
				#endif
			}
		}

		private void Awake()
		{
			// get a reference to our main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
		}

		private void Start()
		{
			_controller = GetComponent<CharacterController>();
			_input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
			_playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

			// reset our timeouts on start
			//_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;

			shmAnimTimer = shmAnimLength;
			mp3Player = GetComponent<AudioSource>();
		}
		private void Update()
		{
			JumpAndGravity();
			GroundedCheck();
			Move();
            if (_input.shmack)
            {
                if (!shmacked)
                {
					wrench.localPosition = endT.localPosition;
					wrench.localRotation = endT.localRotation;
					shmAnimTimer = shmAnimLength;
					shmacked = true;
				}
            }
            if (pumpActive)
            {
				pumpLight.color = Color.green;
				pumpActiveTimer -= Time.deltaTime;
				bigLogicScript.floodProgress -= 2f * Time.deltaTime;
				if(bigLogicScript.floodProgress < 0f)
                {
					bigLogicScript.floodProgress = 0f;
                }
				if(pumpActiveTimer <= 0f)
                {
					pumpActive = false;
					pumpLight.color = Color.red;
					AudioSource.PlayClipAtPoint(pBreak, new Vector3(-6.682f, 8.844f, 2.515f), 1f);
                }
			}
            if (_input.exit)
            {
				Application.Quit();
            }
		}
        private void FixedUpdate()
        {
			if (shmacked)
			{
				if(shmAnimTimer == shmAnimLength)
                {
					RaycastHit hit;
					//Debug.DrawRay(_mainCamera.transform.position, _mainCamera.transform.forward*3.5f, Color.red, 3f);
					if(Physics.Raycast(_mainCamera.transform.position, _mainCamera.transform.forward, out hit, 3.5f, raycastLayers))
                    {
						if(hit.collider.gameObject.layer == 11)
                        {
							hit.collider.GetComponentInParent<animSwapT>().slapped = true;
							if (!hit.collider.GetComponentInParent<animSwapT>().tentacleModel.activeInHierarchy)
							{
								hit.collider.transform.parent.gameObject.SetActive(false);
								mp3Player.PlayOneShot(hFix, 0.4f);
							}
                            else
                            {
								mp3Player.PlayOneShot(tHit, 0.4f);
                            }
						}
                        else
                        {
							pumpActiveTimer = 15f;
							pumpActive = true;
							mp3Player.PlayOneShot(pFix, 0.4f);
                        }
                    }
                }
				shmAnimTimer -= Time.fixedDeltaTime;
				wrench.localPosition = Vector3.Lerp(endT.localPosition, startT.localPosition, 1 - shmAnimTimer / shmAnimLength);
				wrench.localRotation = Quaternion.Lerp(endT.localRotation, startT.localRotation, 1 - shmAnimTimer / shmAnimLength);
				if(shmAnimTimer <= 0f)
                {
					wrench.localPosition = startT.localPosition;
					wrench.localRotation = startT.localRotation;
					shmacked = false;
                }
			}
		}

        private void LateUpdate()
		{
			CameraRotation();
		}

		private void GroundedCheck()
		{
			// set sphere position, with offset
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
		}

		private void CameraRotation()
		{
			// if there is an input
			if (_input.look.sqrMagnitude >= _threshold)
			{
				//Don't multiply mouse input by Time.deltaTime
				float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
				
				_cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
				_rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

				// clamp our pitch rotation
				_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

				// Update Cinemachine camera target pitch
				CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

				// rotate the player left and right
				transform.Rotate(Vector3.up * _rotationVelocity);
			}
		}

		private void Move()
		{
			// set target speed based on move speed, sprint speed and if sprint is pressed
			float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
            if (Grounded)
            {
				targetSpeed = _input.crouch ? CrouchSpeed : targetSpeed;
			}
            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon
            if (_input.crouch)
            {
				_controller.height = Mathf.Lerp(_controller.height, 1, Time.deltaTime * 5);
				//CinemachineCameraTarget.transform.localPosition = Vector3.Lerp(CinemachineCameraTarget.transform.localPosition, new Vector3(0f, 0.6875f, 0f), Time.deltaTime*5);
            }
            else
            {
				_controller.height = Mathf.Lerp(_controller.height, 2, Time.deltaTime * 5);
				//CinemachineCameraTarget.transform.localPosition = Vector3.Lerp(CinemachineCameraTarget.transform.localPosition, new Vector3(0f, 1.375f, 0f), Time.deltaTime*5);
			}
			// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (_input.move == Vector2.zero) targetSpeed = 0.0f;

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

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

			// normalise input direction
			Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			if (_input.move != Vector2.zero)
			{
				// move
				inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
			}

			// move the player
			_controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
		}

		private void JumpAndGravity()
		{
			//Weird spot to put the reset code but it's all set up for me so w.e.
			if (_input.jump && bigLogicScript.youLose)
            {
				bigLogicScript bls = FindObjectOfType<bigLogicScript>();
				bigLogicScript.youLose = false;
				bls.spawnTimer = 5f;
				for (int i = 0; i < 4; i++)
				{
					bls.resetWarning(i);
				}
                foreach (GameObject g in bls.tentaclees)
                {
					g.SetActive(false);
                }
				pumpActive = true;
				pumpActiveTimer = 15f;
				bigLogicScript.speedrunTimer.Restart();
				bls.finalTime.text = "The submarine sinks into the abyss...\nPress Space or A to try again.\nEscape or the Start button to quit.\n\nYou survived for: ";
				bigLogicScript.floodProgress = 0f;
			}
			if (Grounded)
			{
				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;

				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}

				// Jump
				//if (_input.jump)// && _jumpTimeoutDelta <= 0.0f)
				//{
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					//_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
				//}

				// jump timeout
				//if (_jumpTimeoutDelta >= 0.0f)
				//{
				//	_jumpTimeoutDelta -= Time.deltaTime;
				//}
			}
			else
			{
				// reset the jump timeout timer
				//_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
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

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
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

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if(hit.gameObject.layer == 10)
            {
				if(_input.move.y > 0)
                {
					_verticalVelocity = 3;
                }
            }
        }
    }
}