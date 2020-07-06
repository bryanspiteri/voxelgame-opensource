using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using VoxelGame.Blocks;
using VoxelGame.Client.Renderers;
using VoxelGame.Client.UI;
using VoxelGame.Util;
using VoxelGame.Util.Math;

namespace VoxelGame.Client
{
	public class Player : Entity
	{
		public Camera camera;
		public Vector2 headRotation = Vector2.Zero;
		public Vector2 headBob = Vector2.Zero;

		public Vector3 previousPosition = Vector3.Zero;

		public bool noClip = false;
		public float LookVelocity = 0.0025f; //change to mouseSensitivity
		float lx = MathHelper.PiOver2;
		float ly = -MathHelper.Pi / 10.0f;

		public bool Ortho = false;

		public RayTraceResult rayHit = null;
		public Vector3Int SelectedBlock = Vector3Int.Down;
		public uint HeldBlock = 3;

		public readonly float BlockCooldown = 0.16f;
		public float BlockCooldownTimer = 0f;

		public Inventory inventory;
		public int HeldSlot = 0;

		public Player(ulong id) : base(id, Vector3.Zero)
		{
			base.eyePosition = new Vector3(0, 1.45f, 0);
			_coll = BoundingBoxExtensions.FromSize(.6f, 1.8f, 0.6f);
			inventory = new Inventory(9 * 4 + 4 + 1); //Inventory storage + armor + offhand

			//init inventory
			for (int i = 0; i < inventory.items.Length; i++)
			{
				inventory.items[i].ItemCount = 1;
				inventory.items[i].ItemID = (ushort) FastMath.FastClamp(i + 1, 0, 7);
			}
		}

		/// <summary>
		/// Whether to reset the velocity after the frame
		/// </summary>
		public override bool ResetVelocityOnFrame
		{
			get
			{
				return true;
			}
		}

		protected const float JumpVelocity = 1.00f;
		protected const float JumpForce = 0.33f;
		protected const float MoveVelocity = 0.072f;
		protected const float SprintSpeed = 0.12f;
		protected const float SneakSpeed = 0.042f;
		public float MoveSpeed = MoveVelocity;

		public override void Initialise()
		{
			camera = new Camera();
			camera.position = position + eyePosition;
			base.Initialise();
			EntityCollider = BoundingBoxExtensions.FromSize(.6f, 1.8f, 0.6f);
			DoCalculateRay = true;
		}

		public override void Update()
		{
			//Sync the noclip var with the physics engine
			EnablePhysics = !noClip;

			var lookUnits = LookVelocity;// * Time.DeltaTime;
			var moveUnits = MoveSpeed;// * Time.DeltaTime;
			if (GameClient.isPaused == false)
			{
				if (InputManager.IsPressed("move.sprint"))
				{
					MoveSpeed = SprintSpeed;
				}
				else
				{
					MoveSpeed = MoveVelocity;
				}
			}

			// get our movement basis vectors
			Vector3 forward, up, right;
			TransformBasisVector(Vector3.Forward, out forward);
			TransformBasisVector(Vector3.Right, out right);
			if (camera.rotation.X >= 0.0f)
			{
				TransformBasisVector(Vector3.Down, out up);
			}
			else
			{
				TransformBasisVector(Vector3.Up, out up);
			}

			if (GameClient.isPaused == false)
			{
				#region Input
				// check keyboard for which direction to move
				Vector3 move = Vector3.Zero;
				Vector3 moveVector = Vector3.Zero;
				if (InputManager.IsPressed("move.forward"))
				{
					moveVector += Vector3.Forward;

					move += forward;
					move += up;
				}
				if (InputManager.IsPressed("move.back"))
				{
					moveVector += Vector3.Backward;

					move -= forward;
					move -= up;
				}
				if (InputManager.IsPressed("move.right"))
				{
					moveVector += Vector3.Left;

					move += right;
				}
				if (InputManager.IsPressed("move.left"))
				{
					moveVector += Vector3.Right;

					move -= right;
				}
				//jumping / flying
				if (InputManager.IsPressed("move.jump"))
				{
					if (noClip)
					{
						moveVector += Vector3.Up;

						move += Vector3.Up;
					}
					else if (IsGrounded)
					{
						//handle jump
						moveVector += Vector3.Up;

						move += Vector3.Up * JumpForce * 0.3f / MoveSpeed;
					}
				}
				//sneaking / flying down
				if (InputManager.IsPressed("move.sneak"))
				{
					if (noClip)
					{
						moveVector -= Vector3.Up;

						move -= Vector3.Up;
					}
					else
					{
						//handle sneak
						moveVector -= Vector3.Up;

						move -= Vector3.Up;

						MoveSpeed = SneakSpeed;
					}
				}

				#region Hotbar Slot Handling
				//Change the slot
				//Controller support
				if (InputManager.Released("hotbar.right") || InputManager.MouseScrollWheel < 0)
				{
					HeldSlot += 1;
					HeldSlot = FastMath.Normalise(HeldSlot, 0, 9);
				}
				if (InputManager.Released("hotbar.left") || InputManager.MouseScrollWheel > 0)
				{
					HeldSlot -= 1;
					HeldSlot = FastMath.Normalise(HeldSlot, 0, 9);
				}

				//Slot based on keypress
				if (InputManager.Released("hotbar.one"))
				{
					HeldSlot = 0;
				}
				if (InputManager.Released("hotbar.two"))
				{
					HeldSlot = 1;
				}
				if (InputManager.Released("hotbar.three"))
				{
					HeldSlot = 2;
				}
				if (InputManager.Released("hotbar.four"))
				{
					HeldSlot = 3;
				}
				if (InputManager.Released("hotbar.five"))
				{
					HeldSlot = 4;
				}
				if (InputManager.Released("hotbar.six"))
				{
					HeldSlot = 5;
				}
				if (InputManager.Released("hotbar.seven"))
				{
					HeldSlot = 6;
				}
				if (InputManager.Released("hotbar.eight"))
				{
					HeldSlot = 7;
				}
				if (InputManager.Released("hotbar.nine"))
				{
					HeldSlot = 8;
				}
				#endregion

				// check mouse for rotating
				float lx = InputManager.MouseDeltaX * LookVelocity * GameSettings.MouseSensitivity;
				float ly = InputManager.MouseDeltaY * LookVelocity * GameSettings.MouseSensitivity;

				if (GameSettings.HeadBobbing)
				{
					headBob.X += move.X * Time.DeltaTime;
					headBob.Y += move.Z * Time.DeltaTime;
					camera.offset = new Vector3(0, (float)Math.Sin(headBob.X * 5f) * 0.0625f, (float)Math.Sin(headBob.Y * 5f) * 0.125f);
				}

				// move and rotate the camera
				//move.Normalize();
				ApplyForce(move * moveUnits);
				camera.Rotate(lx, ly);
				camera.position = position + eyePosition;

				rotation = camera.rotation;
				headRotation = new Vector2(camera.Yaw, camera.Pitch);

				#endregion

				#region Block Handling
				//Handle the block selection
				Vector2 mouseloc = new Vector2(GameClient.ViewWidth / 2 + lx, GameClient.ViewHeight / 2 + ly);

				LookingDir = RaytraceUtils.GetRay(mouseloc, camera.ViewMatrix, camera.ProjectionMatrix, GameClient.Viewport);

				rayHit = RaytraceUtils.RayTraceBlocks(LookingDir);
				
				if (rayHit != null && rayHit.typeOfHit == RayTraceResult.Type.BLOCK)
				{
					SelectedBlock = rayHit.hitVec.ToVector3Int();
				}

				//Block placing / breaking
				if (BlockCooldownTimer <= 0f)
				{
					if (InputManager.IsPressed("item.break") && rayHit != null && GameClient.theWorld.GetBlockIdAt(RaytraceUtils.GetBreakBlock(rayHit)) != 0)
					{
						//TODO: breaking logic
						GameClient.theWorld.SetBlockAt(RaytraceUtils.GetBreakBlock(rayHit), 0, (uint)BlockBreakReason.PLAYER_CREATIVE);
						BlockCooldownTimer = BlockCooldown;
						SelectedBlock = RaytraceUtils.GetBreakBlock(rayHit);
					}

					//Block placement
					if (InputManager.IsPressed("item.use") && rayHit != null && GameClient.theWorld.GetBlockIdAt(RaytraceUtils.GetPlaceBlock(rayHit)) == BlockManager.AIR.Id && !Intersects(position, HeldBlock, RaytraceUtils.GetPlaceBlock(rayHit)))
					{
						//GameClient.theWorld.SetBlockAt(RaytraceUtils.GetPlaceBlock(rayHit), HeldBlock, (byte)BlockPlaceReason.PLAYER);
						BlockCooldownTimer = BlockCooldown;
						SelectedBlock = RaytraceUtils.GetPlaceBlock(rayHit);
						ItemManager.GetItem(inventory.items[HeldSlot].ItemID).OnUse(this, RaytraceUtils.GetBreakBlock(rayHit), SelectedBlock, BlockPlaceReason.PLAYER);
					}
				}
				else
				{
					BlockCooldownTimer -= GameClient.RawDeltaTime;

					if (!InputManager.IsPressed("item.break") && !InputManager.IsPressed("item.use"))
					{
						//none of the buttons are held
						//reset the timer
						BlockCooldownTimer = 0f;
					}
				}

				//Orthographic camera mode
				if(InputManager.IsPressed("debug.hotkey") && InputManager.Released("debug.orthographic"))
				{
					Ortho = !Ortho;
				}

				//Highlight the block after placing
				if (rayHit != null && rayHit.typeOfHit == RayTraceResult.Type.BLOCK)
				{
					Block id = BlockManager.GetBlock(GameClient.theWorld.GetBlockIdAt(SelectedBlock));

					RenderUtils.RenderBox(id.GetBoundingBox(SelectedBlock).Offset(new Vector3(-0.5f, -0.5f, -0.5f)), GameSettings.BlockOutlineColor, GameSettings.BlockOutlineWidth);
				}
				#endregion

				// update previous states
				InputManager.CenterMouse();

				//Snooping
				Snooper.Snoop("move.totaldistance", Vector3.Distance(previousPosition, position));
			}

			previousPosition = position;
			base.Update();
		}

		private void TransformBasisVector(Vector3 basis, out Vector3 transformed)
		{
			Matrix rotation = camera.Rotation;
			Vector3.Transform(ref basis, ref rotation, out transformed);
			transformed.Y = 0.0f;
		}
	}
}
