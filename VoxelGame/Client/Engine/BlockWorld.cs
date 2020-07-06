using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VoxelGame.Assets;
using VoxelGame.Client;
using VoxelGame.Renderers;
using VoxelGame.Util;

namespace VoxelGame.Engine
{
	public class BlockWorld
	{
		//public List<Voxel> voxels = new List<Voxel>();
		public List<Entity> entities = new List<Entity>();
		public Player player = new Player(0);
		public Skybox sky;
		public TexturedSkybox skyTex;
		//List<GuiScreen> GUIs = new List<GuiScreen>();
		public EntityManager entityManager;

		public bool Paused = false;

		public float TimeActive;
		public float RawTimeActive;
		public bool Focused { get; private set; }

		public int ElapsedTicks = 0;
		public const int TickSpeed = 20; //Ticks per second
		public bool doClock = true;

		public Vector2 cameraangle;
		public Vector2 CameraAngle { get { return cameraangle; } set { cameraangle = value; } }

		SkyboxRenderer skyboxRenderer;

		public BlockWorld()
		{
			/*
			voxels.Add(new Voxel(new Vector3Int(0, 0, 0), new Vector3Int(1, 1, 1), new BlockTextures(TextureManager.uvMap["grass_side"], TextureManager.uvMap["grass_side"], TextureManager.uvMap["grass_side"], TextureManager.uvMap["grass_side"], TextureManager.uvMap["grass"], TextureManager.uvMap["dirt"])));
			voxels.Add(new Voxel(new Vector3Int(0, 2, 0), new Vector3Int(1, 1, 1), new BlockTextures(TextureManager.uvMap["northTest"])));
			voxels.Add(new Voxel(new Vector3Int(0, 0, 2), new Vector3Int(1, 1, 1), new BlockTextures(TextureManager.uvMap["stone"])));
			voxels.Add(new Voxel(new Vector3Int(2, 0, 0), new Vector3Int(1, 1, 1), new BlockTextures(TextureManager.uvMap["stone"])));
			voxels.Add(new Voxel(new Vector3Int(3, 0, 0), new Vector3Int(1, 1, 1), new BlockTextures(TextureManager.uvMap["stone"])));
			voxels.Add(new Voxel(new Vector3Int(2, 1, 0), new Vector3Int(1, 1, 1), new BlockTextures(TextureManager.uvMap["dirt"])));
			voxels.Add(new Voxel(new Vector3Int(3, 1, 0), new Vector3Int(1, 1, 1), new BlockTextures(TextureManager.uvMap["dirt"])));
			*/
			 this.sky = new EarthSky();
			//this.skyTex = new TexturedSkybox();
			skyboxRenderer = new SkyboxRenderer();
			//GUIs.Add(new DebugMenu());

		}

		public void AddEntity(Entity toAdd)
		{
			//entities.Add(toAdd);
			toAdd.Initialise();
			entityManager.SpawnEntity(toAdd);
		}

		public Entity[] GetEntitiesAt(Vector3Int BlockPosition, float Radius)
		{
			return entityManager.GetEntitiesAt(BlockPosition, Radius);
		}

		public Entity[] GetEntitiesAt(Vector3 BlockPosition, float Radius)
		{
			return entityManager.GetEntitiesAt(new Vector3Int(BlockPosition), Radius);
		}

		public virtual void Begin()
		{
			//init the entity manager
			entityManager = new EntityManager();
			entityManager.Init();

			player.Initialise();
			this.Focused = true;
			this.Paused = false;
			this.doClock = true;
		}

		public virtual void End()
		{
			this.Focused = false;
			this.Paused = true;
			this.doClock = false;
		}

		public virtual void BeforeUpdate()
		{
			if (!this.Paused)
			{
				this.TimeActive += VoxelClient.DeltaTime;
				if (GameClient.World.GetType() == typeof(World.World))
				{
					player.BeforeUpdate();
				}
				this.RawTimeActive += VoxelClient.RawDeltaTime;
				if (sky != null)
				{
					sky.BeforeUpdate();
				}
				/*
				if (voxels != null)
				{
					foreach (Voxel voxel in voxels)
					{
						if (voxel != null)
						{
							voxel.BeforeUpdate();
						}
					}
				}
				*/
			}
		}

		public virtual void Update(GameTime gameTime)
		{
			if (!this.Paused)
			{
				if (sky != null)
				{
					sky.Update();
				}
				/*if (voxels != null)
				{
					foreach (Voxel voxel in voxels)
					{
						if (voxel != null)
						{
							voxel.Update();
						}
					}
				}*/
				if (GameClient.World.GetType() == typeof(World.World))
				{
					entityManager.Update(player);
				}
			}
		}

		public virtual void Draw(GameTime gameTime)
		{
			//Draw the skybox
			skyboxRenderer.Render(sky);
			skyboxRenderer.Render(skyTex);
			/*foreach (Voxel voxel in voxels)
			{
				VoxelRenderer.RenderVoxel(voxel);
			}
			*/

			if (GameClient.World.GetType() == typeof(World.World))
			{
				entityManager.Draw(player);
			}
		}

		public void TickHandler()
		{
			while (doClock == true)
			{
				if (!Paused)
				{
					OnGameTick();
					if (GameClient.World.GetType() == typeof(World.World))
					{
						entityManager.Tick();
					}
					Thread.Sleep(20 / 1000);
				}
			}
		}

		public virtual void OnGameTick()
		{

		}
	}
}
