using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Engine;
using VoxelGame.Util.Math;
using VoxelGame.World;

namespace VoxelGame.Client
{
	public class Camera
	{
		public Vector3 position;
		public Vector3 offset;
		public Vector3 rotation;

		public float Yaw
		{
			get
			{
				return rotation.Y;
			}
			set
			{
				rotation.Y = value < 0 ? MathHelper.TwoPi - (value % MathHelper.TwoPi) : value % MathHelper.TwoPi;
			}
		}

		public float Pitch
		{
			get
			{
				return rotation.X;
			}
			set { rotation = new Vector3(value, rotation.Y, rotation.Z); }
		}

		public float Roll
		{
			get
			{
				return rotation.Z;
			}
			set
			{
				rotation = new Vector3(rotation.X, rotation.Y, value);
			}
		}

		public Vector3 LookAtVector = Vector3.Zero;

		public BoundingFrustum ViewFrustum
		{
			get
			{
				return new BoundingFrustum(ViewMatrix * ProjectionMatrix);
			}
		}

		public Matrix ViewMatrix
		{
			get
			{
				/*
				Vector3 originalTarget = new Vector3(0, 0, -1);
				Vector3 rotatedTarget = Vector3.Transform(originalTarget, Rotation);

				Vector3 originalUpVector = new Vector3(0, 1, 0);
				Vector3 rotatedUpVector = Vector3.Transform(originalUpVector, Rotation);
				*/

				return Matrix.CreateTranslation(offset) * Matrix.CreateLookAt(position, position + Vector3.Transform(Vector3.Forward, Rotation), Vector3.Up);
			}
		}

		public Matrix SkyViewMatrix
		{
			get
			{
				return Matrix.CreateTranslation(offset) * Matrix.CreateLookAt(Vector3.Zero, Vector3.Transform(Vector3.Forward, Rotation), Vector3.Up);
			}
		}

		public Matrix SkyHeightViewMatrix
		{
			get
			{
				return Matrix.CreateTranslation(offset) * Matrix.CreateLookAt(Vector3.Zero, Vector3.Transform(Vector3.Forward, RotationHeight), Vector3.Up);
			}
		}

		public Matrix ProjectionMatrix
		{
			get
			{
				float nearClipPlane = 0.01f; //Min Render Distance
				float farClipPlane = Chunk.Size * (GameSettings.RenderDistance + 10); //Max Render Distance
				float aspectRatio = VoxelClient.Graphics.GraphicsDevice.Viewport.Width / (float)VoxelClient.Graphics.GraphicsDevice.Viewport.Height;

				return Matrix.CreatePerspectiveFieldOfView(GameSettings.FOV, aspectRatio, nearClipPlane, farClipPlane);
			}
		}

		public Matrix OrtoProjectionMatrix
		{
			get
			{
				return Matrix.CreateOrthographic(64, 64, 0.01f, 128f);
			}
		}

		public Matrix WorldMatrix
		{
			get
			{
				return Rotation * Matrix.CreateTranslation(position);
			}
		}

		public Matrix Rotation
		{
			get
			{
				return Matrix.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
			}
		}

		public Matrix RotationHeight
		{
			get
			{
				return Matrix.CreateFromYawPitchRoll(0, rotation.X, 0);
			}
		}

		public Camera()
		{
			position = Vector3.Zero;
			rotation = Vector3.Zero;
		}

		public Camera(Vector3 position)
		{
			this.position = position;
			this.rotation = Vector3.Zero;
		}

		public Camera(Vector3 position, Vector3 rotation)
		{
			this.position = position;
			this.rotation = rotation;

			//Check angles
			float x = rotation.X % 360;
			float y = rotation.Y % 360;
			float z = rotation.Z % 360;
		}

		public void Rotate(Vector3 addedRotation)
		{
			float x = rotation.X, y = rotation.Y, z = rotation.Z;

			x += addedRotation.X;
			y += addedRotation.Y;
			z += addedRotation.Z;

			//Check angles
		}

		public void Rotate(float xAxis, float yAxis)
		{
			if (!float.IsNaN(xAxis))
			{
				Yaw += xAxis;
			}

			if (!float.IsNaN(Yaw))
			{
				Pitch = FastMath.FastClamp(Pitch + yAxis, -1.547501f, 1.532499f);
			}

			//Check angles
			/*float x = rotation.X % 360;
			float y = rotation.Y % 360;
			float z = rotation.Z % 360;*/
		}

		public void SetRotation(Vector3 rotation)
		{
			if (!float.IsNaN(rotation.X))
			{
				Yaw = rotation.X;
			}

			if (!float.IsNaN(rotation.Y))
			{
				Pitch = rotation.Y;
			}

			//Check angles
			/*float x = rotation.X % 360;
			float y = rotation.Y % 360;
			float z = rotation.Z % 360;*/
		}

		public void SetRotation(float xAxis, float yAxis)
		{
			if (!float.IsNaN(xAxis))
			{
				Yaw = xAxis;
			}

			if (!float.IsNaN(Yaw))
			{
				Pitch = yAxis;
			}

			//Check angles
			/*float x = rotation.X % 360;
			float y = rotation.Y % 360;
			float z = rotation.Z % 360;*/
		}
	}
}
