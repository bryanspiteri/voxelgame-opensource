using Microsoft.Xna.Framework;
using VoxelGame.Engine;
using System;
using Microsoft.Xna.Framework.Graphics;

namespace VoxelGame.Client
{
	public class SkyboxRenderer
	{
		public void Render(Skybox sky)
		{
			if (sky != null)
			{
				//detect type of skybox
				if (sky is Skybox)
				{
					//PROCEDURAL SKYBOX

					//Draw the skybox

					//TODO: Draw the transition based on the rotation
					//TODO: Draw the stars based on the rotation
					sky.Draw();

				}
				else if (sky is TexturedSkybox)
				{
					//TEXTURED CUBE SKYBOX
					//Get the camera orientation

					//Draw the visible faces
					sky.Draw();
				}
				else if (sky is ColorSkybox)
				{
					//COLORED CUBE SKYBOX
					//Draw the skybox
					VoxelClient.Instance.GraphicsDevice.Clear(((ColorSkybox)sky).skycolor);
				}
				else
				{
					//Default to a black skybox
					VoxelClient.Instance.GraphicsDevice.Clear(Color.Black);
				}
			}
		}
	}
}