using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool aim;
		public bool shoot;

		[Header("Movement Settings")]
		public bool analogMovement;

#if !UNITY_IOS || !UNITY_ANDROID
		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;
#endif

		// 响应信号，为1屏蔽，为0打开
		private uint response = 0;
		const uint REP_MOVE = 0x1;
		const uint REP_LOOK = 0x2;
		const uint REP_JUMP = 0x4;
		const uint REP_SPRINT = 0x8;
		const uint REP_AIM = 0x10;
		const uint REP_SHOOT = 0x20;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

		public void OnAim(InputValue value)
		{
			AimInput(value.isPressed);
		}

		public void OnShoot(InputValue value)
		{
			ShootInput(value.isPressed);
		}
#else
	// old input sys if we do decide to have it (most likely wont)...
#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			if((response & REP_MOVE) == 0)
				move = newMoveDirection;
			else
				move = Vector2.zero;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			if((response & REP_LOOK) == 0)
				look = newLookDirection;
			else
				look = Vector2.zero;
		}

		public void JumpInput(bool newJumpState)
		{
			if((response & REP_JUMP) == 0)
				jump = newJumpState;
			else
				jump = false;
		}

		public void SprintInput(bool newSprintState)
		{
			if((response & REP_SPRINT) == 0)
				sprint = newSprintState;
			else
				sprint = false;
		}

		public void AimInput(bool newAimState)
		{
			if((response & REP_AIM) == 0)
				aim = newAimState;
			else
				aim = false;
		}

		public void ShootInput(bool newShootState)
		{
			if((response & REP_SHOOT) == 0)
				shoot = newShootState;
			else
				shoot = false;
		}

#if !UNITY_IOS || !UNITY_ANDROID

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}

#endif

		// 屏蔽/打开某个输入
		public void ChangeInputResponse(string inputName, bool turnOn)
		{
			switch(inputName)
			{
				case "Move":
					if(turnOn)
						response &= ~REP_MOVE;
					else
						response |= REP_MOVE;
				break;

				default:
				break;
			}
		}
	}
}