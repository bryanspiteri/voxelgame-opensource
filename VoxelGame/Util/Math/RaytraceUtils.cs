using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VoxelGame.Client;
using VoxelGame.Client.Renderers;
using VoxelGame.World;

namespace VoxelGame.Util.Math
{
	public class RaytraceUtils
	{
		private static readonly float MAX_RAY_DISTANCE = GameSettings.Reach;
		public const float Reach = 5f;

		public static Ray GetRay(Vector2 mouseLocation, Matrix view, Matrix projection, Viewport viewport)
		{
			Vector3 nearPoint = viewport.Unproject(new Vector3(mouseLocation.X,
						mouseLocation.Y, 0.0f),
						projection,
						view,
						Matrix.Identity);

			Vector3 farPoint = viewport.Unproject(new Vector3(mouseLocation.X,
					mouseLocation.Y, 1.0f),
					projection,
					view,
					Matrix.Identity);

			Vector3 direction = farPoint - nearPoint;
			direction.Normalize();

			//Console.WriteLine("ray at start: " + nearPoint + "; facing: " + direction);
			//return new Ray(nearPoint + new Vector3(0.5f, 0.5f, 0.5f), direction);
			return new Ray(nearPoint, direction);
		}

		public static RayTraceResult RayTraceBlocks(Ray ray)
		{
			return RayTraceBlocks(ray.Position, ray.Direction);
		}

		public static RayTraceResult RayTraceBlocks(Vector3 start, Vector3 direction)
		{
			//GameClient.LOGGER.info("ray at start: " + start + "; facing: " + direction);

			//RenderUtils.RenderRay(new Ray(start, direction), Color.Blue, GameSettings.BlockOutlineWidth);

			Vector3Int Selected;
			Vector3Int PreviousSelected = -Vector3Int.One;

			Side face;

			foreach (Vector3Int coord in GetCellsOnRay(new Ray(start + new Vector3(0.5f, 0.5f, 0.5f), direction), (int)MAX_RAY_DISTANCE))
			{
				uint id = GameClient.theWorld.GetBlockIdAt(coord);
				Vector3 renderPos = coord.ToVector3();

				float i = Vector3.DistanceSquared(GameClient.World.player.camera.position, renderPos);
				if (i > Reach * Reach) continue; //Dont let us choose blocks which are too far away
				if (BlockManager.GetBlock(id).Selectable) //If we can pick the block
				{
					if (GameSettings.Debug.RenderRaycastLocations)
					{
						Vector3 min = new Vector3(renderPos.X - 0.5f, renderPos.Y - 0.5f, renderPos.Z - 0.5f);
						Vector3 max = new Vector3(renderPos.X + 0.5f, renderPos.Y + 0.5f, renderPos.Z + 0.5f);

						RenderUtils.RenderBox(new BoundingBox(min, max),
							Color.White, GameSettings.BlockOutlineWidth);
					}

					//TODO: Ray test

					Selected = coord;
					//Determine the facing face
					face = GetFace(Selected, PreviousSelected);
					return new RayTraceResult(renderPos, face, coord);
				}
				else
				{
					PreviousSelected = coord;
					if (GameSettings.Debug.RenderRaycastLocations)
					{
						Vector3 min = new Vector3(renderPos.X - 0.5f, renderPos.Y - 0.5f, renderPos.Z - 0.5f);
						Vector3 max = new Vector3(renderPos.X + 0.5f, renderPos.Y + 0.5f, renderPos.Z + 0.5f);

						RenderUtils.RenderBox(new BoundingBox(min, max),
							Color.Yellow, GameSettings.BlockOutlineWidth);
					}
				}
			}

			return null;

			//return Cast(new Ray(start, direction), (int)MAX_RAY_DISTANCE);
		}

		private static int FastFloor(double x)
		{
			return x > 0 ? (int)x : (int)x - 1;
		}

		//TODO: Rewrite Raytracer once mobs are implemented

		/// <summary>
		/// Gets the cells which intersect with the specified <see cref="Ray"/>.
		/// </summary>
		/// <param name="ray">The cell-relative <see cref="Ray"/>.</param>
		/// <param name="maxDepth">The maximum search depth, which equals the maximum number of
		/// returned points.</param>
		/// <returns>An enumerable list of points, starting with the cell closest to the starting
		/// point of the ray.</returns>
		/// <remarks>
		/// <para>The position and direction of the <paramref name="ray"/> must be in the cell
		/// coordinate system.</para>
		/// <para>The first cell which is returned refers to the cell in which the ray starts.</para>
		/// </remarks>
		public static IEnumerable<Vector3Int> GetCellsOnRay(Ray ray, int maxDepth)
		{
			// Implementation is based on:
			// "A Fast Voxel Traversal Algorithm for Ray Tracing"
			// John Amanatides, Andrew Woo
			// http://www.cse.yorku.ca/~amana/research/grid.pdf <- use this lol
			// http://www.devmaster.net/articles/raytracing_series/A%20faster%20voxel%20traversal%20algorithm%20for%20ray%20tracing.pdf

			// NOTES:
			// * This code assumes that the ray's position and direction are in 'cell coordinates', which means
			//   that one unit equals one cell in all directions.
			// * When the ray doesn't start within the voxel grid, calculate the first position at which the
			//   ray could enter the grid. If it never enters the grid, there is nothing more to do here.
			// * Also, it is important to test when the ray exits the voxel grid when the grid isn't infinite.
			// * The Point3D structure is a simple structure having three integer fields (X, Y and Z).

			if (float.IsNaN(ray.Position.X) || float.IsNaN(ray.Position.Y) || float.IsNaN(ray.Position.Z)) yield break;

			int x = FastFloor(ray.Position.X);
			int y = FastFloor(ray.Position.Y);
			int z = FastFloor(ray.Position.Z);

			// Determine which way we go.
			int stepX = System.Math.Sign(ray.Direction.X);
			int stepY = System.Math.Sign(ray.Direction.Y);
			int stepZ = System.Math.Sign(ray.Direction.Z);

			// Calculate cell boundaries. When the step (i.e. direction sign) is positive,
			// the next boundary is AFTER our current position, meaning that we have to add 1.
			// Otherwise, it is BEFORE our current position, in which case we add nothing.
			//Point3D cellBoundary = new Point3D(
			Vector3Int cellBoundary = new Vector3Int(
				x + (stepX > 0 ? 1 : 0),
				y + (stepY > 0 ? 1 : 0),
				z + (stepZ > 0 ? 1 : 0));

			// NOTE: For the following calculations, the result will be Single.PositiveInfinity
			// when ray.Direction.X, Y or Z equals zero, which is OK. However, when the left-hand
			// value of the division also equals zero, the result is Single.NaN, which is not OK.

			// Determine how far we can travel along the ray before we hit a voxel boundary.
			Vector3 tMax = new Vector3(
				(cellBoundary.X - ray.Position.X) / ray.Direction.X,    // Boundary is a plane on the YZ axis.
				(cellBoundary.Y - ray.Position.Y) / ray.Direction.Y,    // Boundary is a plane on the XZ axis.
				(cellBoundary.Z - ray.Position.Z) / ray.Direction.Z);    // Boundary is a plane on the XY axis.
			if (Single.IsNaN(tMax.X)) tMax.X = Single.PositiveInfinity;
			if (Single.IsNaN(tMax.Y)) tMax.Y = Single.PositiveInfinity;
			if (Single.IsNaN(tMax.Z)) tMax.Z = Single.PositiveInfinity;

			// Determine how far we must travel along the ray before we have crossed a gridcell.
			Vector3 tDelta = new Vector3(
				stepX / ray.Direction.X,                    // Crossing the width of a cell.
				stepY / ray.Direction.Y,                    // Crossing the height of a cell.
				stepZ / ray.Direction.Z);                    // Crossing the depth of a cell.
			if (Single.IsNaN(tDelta.X)) tDelta.X = Single.PositiveInfinity;
			if (Single.IsNaN(tDelta.Y)) tDelta.Y = Single.PositiveInfinity;
			if (Single.IsNaN(tDelta.Z)) tDelta.Z = Single.PositiveInfinity;

			// For each step, determine which distance to the next voxel boundary is lowest (i.e.
			// which voxel boundary is nearest) and walk that way.
			for (int i = 0; i < maxDepth; i++)
			{
				// Return it.
				yield return new Vector3Int(x, y, z);

				// Do the next step.
				if (tMax.X < tMax.Y && tMax.X < tMax.Z)
				{
					// tMax.X is the lowest, an YZ cell boundary plane is nearest.
					x += stepX;
					tMax.X += tDelta.X;
				}
				else if (tMax.Y < tMax.Z)
				{
					// tMax.Y is the lowest, an XZ cell boundary plane is nearest.
					y += stepY;
					tMax.Y += tDelta.Y;
				}
				else
				{
					// tMax.Z is the lowest, an XY cell boundary plane is nearest.
					z += stepZ;
					tMax.Z += tDelta.Z;
				}
			}
		}

		public static Side GetFace(Vector3Int block, Vector3Int rayPosition)
		{
			if (rayPosition.X > block.X)
			{
				return Side.NORTH;
			}
			if (rayPosition.X < block.X)
			{
				return Side.SOUTH;
			}

			if (rayPosition.Y > block.Y)
			{
				return Side.UP;
			}
			if (rayPosition.Y < block.Y)
			{
				return Side.DOWN;
			}

			if (rayPosition.Z > block.Z)
			{
				return Side.EAST;
			}
			if (rayPosition.Z < block.Z)
			{
				return Side.WEST;
			}

			//Default
			return Side.DOWN;
		}

		public static Vector3Int GetPlaceBlock(RayTraceResult rayHit)
		{
			return GetBreakBlock(rayHit) + rayHit.sideHit.GetOffset();
		}

		public static Vector3Int GetBreakBlock(RayTraceResult rayHit)
		{
			return rayHit.blockPos;
		}

		/// <summary>
		/// Returns the distance along the ray where it intersects the specified bounding box, if it intersects at all.
		/// </summary>
		public static double? Intersects(Ray ray, BoundingBox box, out Side face)
		{
			face = Side.UP;
			//first test if start in box
			if (ray.Position.X >= box.Min.X
					&& ray.Position.X <= box.Max.X
					&& ray.Position.Y >= box.Min.Y
					&& ray.Position.Y <= box.Max.Y
					&& ray.Position.Z >= box.Min.Z
					&& ray.Position.Z <= box.Max.Z)
				return 0.0f;// here we concidere cube is full and origine is in cube so intersect at origine

			//Second we check each face
			Vector3 maxT = new Vector3(-1.0f);
			//Vector3 minT = new Vector3(-1.0f);
			//calcul intersection with each faces
			if (ray.Direction.X != 0.0f)
			{
				if (ray.Position.X < box.Min.X)
					maxT.X = (box.Min.X - ray.Position.X) / ray.Direction.X;
				else if (ray.Position.X > box.Max.X)
					maxT.X = (box.Max.X - ray.Position.X) / ray.Direction.X;
			}

			if (ray.Direction.Y != 0.0f)
			{
				if (ray.Position.Y < box.Min.Y)
					maxT.Y = (box.Min.Y - ray.Position.Y) / ray.Direction.Y;
				else if (ray.Position.Y > box.Max.Y)
					maxT.Y = (box.Max.Y - ray.Position.Y) / ray.Direction.Y;
			}

			if (ray.Direction.Z != 0.0f)
			{
				if (ray.Position.Z < box.Min.Z)
					maxT.Z = (box.Min.Z - ray.Position.Z) / ray.Direction.Z;
				else if (ray.Position.Z > box.Max.Z)
					maxT.Z = (box.Max.Z - ray.Position.Z) / ray.Direction.Z;
			}

			//get the maximum maxT
			if (maxT.X > maxT.Y && maxT.X > maxT.Z)
			{
				if (maxT.X < 0.0f)
					return null;// ray go on opposite of face
				//coordonate of hit point of face of cube
				double coord = ray.Position.Z + maxT.X * ray.Direction.Z;
				// if hit point coord ( intersect face with ray) is out of other plane coord it miss
				if (coord < box.Min.Z || coord > box.Max.Z)
					return null;
				coord = ray.Position.Y + maxT.X * ray.Direction.Y;
				if (coord < box.Min.Y || coord > box.Max.Y)
					return null;

				if (ray.Position.X < box.Min.X)
					face = Side.SOUTH;
				else if (ray.Position.X > box.Max.X)
					face = Side.NORTH;

				return maxT.X;
			}
			if (maxT.Y > maxT.X && maxT.Y > maxT.Z)
			{
				if (maxT.Y < 0.0f)
					return null;// ray go on opposite of face
				//coordonate of hit point of face of cube
				double coord = ray.Position.Z + maxT.Y * ray.Direction.Z;
				// if hit point coord ( intersect face with ray) is out of other plane coord it miss
				if (coord < box.Min.Z || coord > box.Max.Z)
					return null;
				coord = ray.Position.X + maxT.Y * ray.Direction.X;
				if (coord < box.Min.X || coord > box.Max.X)
					return null;

				if (ray.Position.Y < box.Min.Y)
					face = Side.DOWN;
				else if (ray.Position.Y > box.Max.Y)
					face = Side.UP;

				return maxT.Y;
			}
			else //Z
			{
				if (maxT.Z < 0.0f)
					return null;// ray go on opposite of face
				//coordonate of hit point of face of cube
				double coord = ray.Position.X + maxT.Z * ray.Direction.X;
				// if hit point coord ( intersect face with ray) is out of other plane coord it miss
				if (coord < box.Min.X || coord > box.Max.X)
					return null;
				coord = ray.Position.Y + maxT.Z * ray.Direction.Y;
				if (coord < box.Min.Y || coord > box.Max.Y)
					return null;

				if (ray.Position.Z < box.Min.Z)
					face = Side.WEST;
				else if (ray.Position.Z > box.Max.Z)
					face = Side.EAST;

				return maxT.Z;
			}
		}
	}

	public class RayTraceResult
	{
		public Vector3Int blockPos;

		/// <summary>
		/// The type of hit that occured, see RayTraceResult.Type for possibilities.
		/// </summary>
		public Type typeOfHit;
		public Side sideHit;

		/// <summary>
		/// The vector position of the hit
		/// </summary>
		public Vector3 hitVec;

		/// <summary>
		/// The hit entity
		/// </summary>
		public Entity entityHit;

		public RayTraceResult(Vector3 hitVecIn, Side sideHitIn, Vector3Int blockPosIn)
		{
			this.typeOfHit = Type.BLOCK;
			this.blockPos = blockPosIn;
			this.sideHit = sideHitIn;
			this.hitVec = hitVecIn;
		}

		public RayTraceResult(Vector3 hitVecIn, Side sideHitIn)
		{
			this.typeOfHit = Type.BLOCK;
			this.blockPos = Vector3Int.Zero;
			this.sideHit = sideHitIn;
			this.hitVec = hitVecIn;
		}

		public RayTraceResult(Entity entityIn)
		{
			this.typeOfHit = Type.ENTITY;
			this.entityHit = entityIn;
			this.hitVec = new Vector3(entityIn.X, entityIn.Y, entityIn.Z);
		}

		public RayTraceResult(Type typeIn, Vector3 hitVecIn, Side sideHitIn, Vector3Int blockPosIn)
		{
			this.typeOfHit = typeIn;
			this.blockPos = blockPosIn;
			this.sideHit = sideHitIn;
			this.hitVec = hitVecIn;
		}

		public RayTraceResult(Entity entityHitIn, Vector3 hitVecIn)
		{
			this.typeOfHit = Type.ENTITY;
			this.entityHit = entityHitIn;
			this.hitVec = hitVecIn;
		}

		public override string ToString()
		{
			return "HitResult { Type = " + this.typeOfHit + ", BlockPos = " + this.blockPos + ", f = " + this.sideHit + ", pos = " + this.hitVec + ", entity = " + this.entityHit + '}';
		}

		public enum Type
		{
			MISS,
			BLOCK,
			ENTITY,
		}
	}
}
