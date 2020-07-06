using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Client.Renderers;
using VoxelGame.Util;
using VoxelGame.Util.Math;
using VoxelGame.World;

namespace VoxelGame.Client
{
	public class Entity
	{
		public ulong EntityID = 0x0;
		public Ray LookingDir;
		public bool DoCalculateRay = false;

		#region HP
		public int Health = 0;
		public int MaxHealth = 20;
		public bool IsAlive = true;
		public bool Invincible = false;
		public bool Attackable = false;
		#endregion

		#region Position
		public Vector3 position;
		/// <summary>
		/// The current rotation, in radians
		/// </summary>
		public Vector3 rotation;
		private Vector3 _prevRot;
		public Vector3 translation;

		public Vector3Int blockPosition
		{
			get
			{
				return new Vector3Int((int)Math.Round(position.X, MidpointRounding.AwayFromZero),
					(int)Math.Round(position.Y, MidpointRounding.AwayFromZero),
					(int)Math.Round(position.Z, MidpointRounding.AwayFromZero));
			}
		}

		public Vector3Int ChunkPosition
		{
			get
			{
				int chunkX = blockPosition.X / Chunk.CHUNK_SIZE;
				int chunkY = blockPosition.Y / Chunk.CHUNK_SIZE;
				int chunkZ = blockPosition.Z / Chunk.CHUNK_SIZE;

				//Negative fix
				if (blockPosition.X < 0)
				{
					chunkX--;
				}
				if (blockPosition.Y < 0)
				{
					chunkY--;
				}
				if (blockPosition.Z < 0)
				{
					chunkZ--;
				}

				return new Vector3Int(chunkX, chunkY, chunkZ);
			}
		}

		public float X
		{
			get
			{
				return position.X;
			}
			set
			{
				position.X = value;
			}
		}

		public float Y
		{
			get
			{
				return position.Y;
			}
			set
			{
				position.Y = value;
			}
		}

		public float Z
		{
			get
			{
				return position.Z;
			}
			set
			{
				position.Z = value;
			}
		}

		public virtual void Move(float x, float y, float z)
		{
			if (!float.IsNaN(x))
			{
				translation.X += x;
			}
			if (!float.IsNaN(y))
			{
				translation.Y += y;
			}
			if (!float.IsNaN(z))
			{
				translation.Z += z;
			}
		}

		public Vector3 eyePosition = Vector3.Zero;
		#endregion

		#region Colliders

		/// <summary>
		/// Whether the entity can collide with other entities
		/// </summary>
		public bool Collidable = true;

		/// <summary>
		/// The actual collision shape
		/// </summary>
		public BoundingBox _coll = BoundingBoxExtensions.FromSize(.6f, 1.8f, 0.6f);

		/// <summary>
		/// The entity's bounding box
		/// </summary>
		public virtual BoundingBox EntityCollider
		{
			get
			{
				return _coll;
			}
			set
			{
				_coll = value;
			}
		}

		/// <summary>
		/// Returns the entity collider in the world
		/// </summary>
		public BoundingBox Collider
		{
			get
			{
				return EntityCollider.Offset(position);
			}
		}

		#endregion

		#region Physics Variables
		public Vector3 Velocity = Vector3.Zero;
		/// <summary>
		/// Whether the entity is on a block
		/// </summary>
		public bool IsGrounded = false;

		/// <summary>
		/// A toggle for special case scenarios like when the player is climbing a ladder or in a fluid.
		/// </summary>
		public bool ApplyGravity = true;

		/// <summary>
		/// Whether this entity obeys the laws of physics.
		/// </summary>
		public bool EnablePhysics = true;

		public float Gravity = 10f;

		/// <summary>
		/// The speed of the entity
		/// </summary>
		public virtual float Speed
		{
			get
			{
				return 50f;
			}
		}

		/// <summary>
		/// Whether to reset the velocity after the frame
		/// </summary>
		public virtual bool ResetVelocityOnFrame
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// The maximum velocity in any axis
		/// </summary>
		public readonly static float TerminalVelocity = 256f;
		/// <summary>
		/// The terminal velocity along the Z (height) axis. Different because of gravity and the world heights
		/// </summary>
		public readonly static float TerminalVelocityY = 512f;

		#endregion

		/// <summary>
		/// Spawns the entity into the world at the given coordinates
		/// </summary>
		/// <param name="id">The entity id. No two entites can have the same id. ID 0 is reserved for the player.</param>
		/// <param name="Position"></param>
		public Entity(ulong id, Vector3 Position)
		{
			//TODO: ID checking
			EntityID = id;
			this.position = Position;
		}

		//Used to move simulate camera movement
		//without actually moving the camera
		//Good for checking collision before allowing player to move
		public Vector3 PreviewMove(Vector3 amount)
		{
			Matrix rotate = Matrix.CreateRotationY(rotation.Y);
			Vector3 movement = new Vector3(amount.X, amount.Y, amount.Z);
			movement = Vector3.Transform(movement, rotate);
			return position + movement;
		}

		//Actually moves the camera by the scale factor passed in
		public void Move(Vector3 scale)
		{
			//MoveTo(PreviewMove(scale), Rotation);
			ApplyForce(PreviewMove(scale) - position);
		}

		public virtual void Initialise()
		{

		}

		public virtual void BeforeUpdate()
		{

		}

		public virtual void Update()
		{
			if (DoCalculateRay)
			{
				if (_prevRot != rotation)
				{
					CalculateRay();
				}
			}
			_prevRot = rotation;
		}

		public virtual void OnTick()
		{
			//Handle AI
		}

		public virtual void Draw()
		{
			//Draw the hitbox
			if (GameSettings.DrawHitboxes)
			{
				RenderUtils.DebugRenderBox(_coll.Offset(position), GameSettings.EntityHitboxColor, GameSettings.BlockOutlineWidth);
			}
		}

		/// <summary>
		/// Renders a block's bounds at the the given position
		/// </summary>
		/// <param name="blockPos"></param>
		/// <param name="color"></param>
		public void RenderBlockBounds(Vector3Int blockPos, Color color)
		{
			Vector3 renderPos = blockPos.ToVector3();
			Vector3 min = new Vector3(renderPos.X - 0.5f, renderPos.Y - 0.5f, renderPos.Z - 0.5f);
			Vector3 max = new Vector3(renderPos.X + 0.5f, renderPos.Y + 0.5f, renderPos.Z + 0.5f);

			RenderUtils.RenderBox(new BoundingBox(min, max), color, GameSettings.BlockOutlineWidth);
		}

		#region Physics
		public virtual void PhysicsUpdate()
		{
			if (!GameClient.isPaused)
			{
				//Apply terminal velocity and speed
				Velocity = new Vector3(MathHelper.Clamp(Velocity.X * Speed, -TerminalVelocity, TerminalVelocity), MathHelper.Clamp(Velocity.Y * Speed, -TerminalVelocityY, TerminalVelocityY), MathHelper.Clamp(Velocity.Z * Speed, -TerminalVelocity, TerminalVelocity));

				//Apply gravity
				if (ApplyGravity && EnablePhysics)
				{
					Velocity.Y -= Gravity * Time.PhysicsDeltaTime;
				}

				Vector3 previousPositon = position;

				//Apply velocity
				Vector3 newPosition = new Vector3(position.X + (Velocity.X * Time.PhysicsDeltaTime), position.Y + (Velocity.Y * Time.PhysicsDeltaTime), position.Z + (Velocity.Z * Time.PhysicsDeltaTime));

				//Entity Collision
				Entity[] neighbouringEntities = GameClient.theWorld.GetEntitiesAt(newPosition, 1f);

				//Actually move
				PhysicsMove(new Vector3(position.X, newPosition.Y, position.Z), Time.PhysicsDeltaTime, Color.LimeGreen, true, false, true, neighbouringEntities);
				PhysicsMove(new Vector3(newPosition.X, position.Y, position.Z), Time.PhysicsDeltaTime, Color.Red, false, true, false, neighbouringEntities);
				PhysicsMove(new Vector3(position.X, position.Y, newPosition.Z), Time.PhysicsDeltaTime, Color.Blue, false, false, false, neighbouringEntities);

				//Resolve velocity
				//Disable gravity if we are on the ground
				if (IsGrounded && EnablePhysics && false == true)
				{
					Velocity.Y = 0;
				}

				//Fix banging head on ceiling makes you freeze for a while
				//Check if velocity isnt 0 and the position didnt change
				if ((EnablePhysics && Velocity.Y != 0 && position.Y == previousPositon.Y) || IsGrounded)
				{
					//if the velocity is negative (falling) then ground the player
					if (Velocity.Y < 0)
					{
						IsGrounded = true;
					}
					//otherwise we are rising in the air (jumping probably)
					else
					{
						IsGrounded = false;
					}
					//No change in height. Reset velocity on Y
					Velocity.Y = 0;
				}

				if (ResetVelocityOnFrame)
				{
					Velocity.X = 0;
					Velocity.Z = 0;
					Velocity.Y = EnablePhysics && ApplyGravity ? Velocity.Y / Speed : 0;
				}
			}
		}

		/// <summary>
		/// Moves the entity to the specified location in the time given.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="time"></param>
		public void PhysicsMove(Vector3 destination, float time, Color debugColor, bool checkGroundedness, bool Xmove, bool Ymove, Entity[] neighbouringEntities)
		{
			if (time == Time.PhysicsDeltaTime)
			{
				//Get the colliders
				bool Intersected = false, Grounded = false;
				BoundingBox newCollider = _coll.Offset(destination);

				Vector3Int MinBounds = Vector3Int.Floor(newCollider.Min);
				Vector3Int MaxBounds = Vector3Int.Ceiling(newCollider.Max);

				float axisPoint = 0;
				float axis = 0; //TEMP VAR

				for (int x = MinBounds.X; x <= MaxBounds.X; x++)
				{
					for (int y = MinBounds.Y; y <= MaxBounds.Y; y++)
					{
						for (int z = MinBounds.Z; z <= MaxBounds.Z; z++)
						{
							//Intersection test
							if (Xmove)
							{
								Intersected = Intersected || TestIntersectionX(new Vector3Int(x, y, z), newCollider, out axis);
								axisPoint = FastMath.ClosestToZero(axis, axisPoint);

							}
							else
							{
								if (Ymove)
								{
									if (checkGroundedness)
									{
										Grounded = Grounded || TestIntersectionY(new Vector3Int(x, y, z), newCollider, out axis);
									}
									Intersected = Intersected || TestIntersectionY(new Vector3Int(x, y, z), newCollider, out axis);
									axisPoint = FastMath.ClosestToZero(axis, axisPoint);
								}
								else
								{
									Intersected = Intersected || TestIntersectionZ(new Vector3Int(x, y, z), newCollider, out axis);
									axisPoint = FastMath.ClosestToZero(axis, axisPoint);
								}
							}

							//Exit the loop if we collided, these are all extra iterations
							if (Intersected)
							{
								break;
							}
						}

						//Exit the loop if we collided, these are all extra iterations
						if (Intersected)
						{
							break;
						}
					}

					//Exit the loop if we collided, these are all extra iterations
					if (Intersected)
					{
						break;
					}
				}

				//TODO: Entity intersections

				if (GameSettings.Debug.RenderPhysicsTestLocations)
				{
					//Entity Collider
					RenderUtils.DebugRenderBox(newCollider, debugColor, GameSettings.BlockOutlineWidth);
				}

				//GameClient.LOGGER.debug(-1, "=======================================");

				//Check for intersecting bounding boxes
				if (EnablePhysics)
				{
					if (Intersected == false)
					{
						//GameClient.LOGGER.debug(-1, "intersected: " + Intersected);
						//No intersection. Move the entity
						position = destination;
					}
					else
					{
						if (!IsGrounded)
						{
							//Intersection. Add axis point to the corresponding axis

							//Convert axisPoint to a Vector3
							if (Xmove)
							{
								position.X += axisPoint;
							}
							else
							{
								if (Ymove)
								{
									position.Y += axisPoint;
								}
								else
								{
									position.Z += axisPoint;
								}
							}
						}
					}
				}
				else if (!EnablePhysics)
				{
					//Move the entity
					position = destination;
				}

				if (checkGroundedness && Ymove)
				{
					//Check for y
					IsGrounded = Grounded;
				}
			}
		}

		/// <summary>
		/// Returns whether the entity intersects at the given destination, assuming the specified block is placed at the specified block position
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="BlockID"></param>
		/// <param name="blockpos"></param>
		/// <returns></returns>
		public bool Intersects(Vector3 destination, uint BlockID, Vector3Int blockpos)
		{
			//Get the colliders
			bool Intersected = false;
			BoundingBox newCollider = _coll.Offset(destination);

			Vector3Int MinBounds = Vector3Int.Floor(newCollider.Min);
			Vector3Int MaxBounds = Vector3Int.Ceiling(newCollider.Max);

			//loop between all blocks the collider is in
			for (int x = MinBounds.X; x <= MaxBounds.X; x++)
			{
				for (int y = MinBounds.Y; y <= MaxBounds.Y; y++)
				{
					for (int z = MinBounds.Z; z <= MaxBounds.Z; z++)
					{
						Intersected = Intersected || TestIntersection(new Vector3Int(x, y, z), newCollider, BlockID, blockpos);

						//Exit the loop if we collided, these are all extra iterations
						if (Intersected)
						{
							return Intersected;
						}
					}

					//Exit the loop if we collided, these are all extra iterations
					if (Intersected)
					{
						return Intersected;
					}
				}

				//Exit the loop if we collided, these are all extra iterations
				if (Intersected)
				{
					return Intersected;
				}
			}

			return Intersected;
		}

		public bool Intersects(Vector3 destination)
		{
			//Get the colliders
			bool Intersected = false;
			BoundingBox newCollider = _coll.Offset(destination);

			Vector3Int MinBounds = Vector3Int.Floor(newCollider.Min);
			Vector3Int MaxBounds = Vector3Int.Ceiling(newCollider.Max);

			//loop between all blocks the collider is in
			for (int x = MinBounds.X; x <= MaxBounds.X; x++)
			{
				for (int y = MinBounds.Y; y <= MaxBounds.Y; y++)
				{
					for (int z = MinBounds.Z; z <= MaxBounds.Z; z++)
					{
						Intersected = Intersected || TestIntersection(new Vector3Int(x, y, z), newCollider);

						//Exit the loop if we collided, these are all extra iterations
						if (Intersected)
						{
							return Intersected;
						}
					}

					//Exit the loop if we collided, these are all extra iterations
					if (Intersected)
					{
						return Intersected;
					}
				}

				//Exit the loop if we collided, these are all extra iterations
				if (Intersected)
				{
					return Intersected;
				}
			}

			return Intersected;
		}

		/// <summary>
		/// Returns a block's collider
		/// </summary>
		/// <param name="blockPos"></param>
		/// <returns></returns>
		public BoundingBox GetBoxCollider(Vector3Int blockPos)
		{
			return GameClient.theWorld.GetBlockAt(blockPos).GetBoundingBox(blockPos);
		}

		private bool TestIntersection(Vector3Int block, BoundingBox collider)
		{
			Vector3 Offset = new Vector3(-0.5f, -0.5f, -0.5f);
			BoundingBox blockCollider = GetBoxCollider(block).Offset(Offset);
			if (GameSettings.Debug.RenderPhysicsTestLocations)
			{
				RenderUtils.DebugRenderBox(blockCollider, Color.Purple, GameSettings.BlockOutlineWidth);
			}
			return collider.Intersects(blockCollider);
		}

		private bool TestIntersectionX(Vector3Int block, BoundingBox collider, out float dist)
		{
			Vector3 Offset = new Vector3(-0.5f, -0.5f, -0.5f);
			BoundingBox blockCollider = GetBoxCollider(block).Offset(Offset);
			if (GameSettings.Debug.RenderPhysicsTestLocations)
			{
				RenderUtils.DebugRenderBox(blockCollider, Color.Purple, GameSettings.BlockOutlineWidth);
			}
			return collider.IntersectsX(blockCollider, out dist);
		}

		private bool TestIntersectionY(Vector3Int block, BoundingBox collider, out float dist)
		{
			Vector3 Offset = new Vector3(-0.5f, -0.5f, -0.5f);
			BoundingBox blockCollider = GetBoxCollider(block).Offset(Offset);
			if (GameSettings.Debug.RenderPhysicsTestLocations)
			{
				RenderUtils.DebugRenderBox(blockCollider, Color.Purple, GameSettings.BlockOutlineWidth);
			}
			return collider.IntersectsY(blockCollider, out dist);
		}

		private bool TestIntersectionZ(Vector3Int block, BoundingBox collider, out float dist)
		{
			Vector3 Offset = new Vector3(-0.5f, -0.5f, -0.5f);
			BoundingBox blockCollider = GetBoxCollider(block).Offset(Offset);
			if (GameSettings.Debug.RenderPhysicsTestLocations)
			{
				RenderUtils.DebugRenderBox(blockCollider, Color.Purple, GameSettings.BlockOutlineWidth);
			}
			return collider.IntersectsZ(blockCollider, out dist);
		}

		private bool TestIntersection(Vector3Int block, BoundingBox collider, uint newBlockID, Vector3Int newBlockPos)
		{
			Vector3 Offset = new Vector3(-0.5f, -0.5f, -0.5f);

			//Get the block of newBlock if its the new block
			BoundingBox blockCollider;
			if (block == newBlockPos)
			{
				blockCollider = BlockManager.GetBlock(newBlockID).GetBoundingBox(newBlockPos).Offset(Offset);
			}
			else
			{
				blockCollider = GetBoxCollider(block).Offset(Offset);
			}

			if (GameSettings.Debug.RenderPhysicsTestLocations)
			{
				RenderUtils.DebugRenderBox(blockCollider, Color.Purple, GameSettings.BlockOutlineWidth);
			}

			return collider.Intersects(blockCollider);
		}

		/// <summary>
		/// Applies a force to the body
		/// </summary>
		/// <param name="force"></param>
		public void ApplyForce(Vector3 force)
		{
			Velocity += force;
		}
		#endregion

		#region Ray Calculation
		public void CalculateRay()
		{
			Vector3 rayPos = position + eyePosition;
			Matrix headRotation = Matrix.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z) * Matrix.CreateTranslation(rayPos + Vector3.Forward);
			Vector3 dir = rayPos - Matrix.Invert(headRotation).Translation;
			dir.Normalize();

			LookingDir = new Ray(rayPos, dir);
		}
		#endregion
	}
}
