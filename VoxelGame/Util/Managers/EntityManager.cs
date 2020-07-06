using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Client;
using VoxelGame.Util.Math;

namespace VoxelGame
{
	public class EntityManager
	{
		public float Timer = 0f;
		public readonly float TimerDelay = 60f;

		public List<Entity> Entities = new List<Entity>();

		public void Init()
		{
			if (Entities != null && Entities.Count > 0)
			{
				Entities.Clear();
			}
			Entities = new List<Entity>();
		}

		public void SpawnEntity(Entity toSpawn)
		{
			//check for dupes (entities with the same id)
			for (int i = 0; i < Entities.Count; i++)
			{
				if (Entities[i].EntityID == toSpawn.EntityID)
				{
					return;
				}
			}
			//spawn the mob
			Entities.Add(toSpawn);
		}

		/// <summary>
		/// Updates all entities
		/// </summary>
		public void Update(Entity player)
		{
			//TicksPerSecond
			Timer += (float)Time.GameTime.ElapsedGameTime.TotalSeconds;

			bool ToUpdate = false;

			while (Timer > 1 / TimerDelay)
			{
				Timer -= (1 / TimerDelay) * FastMath.Floor(Timer / (1 / TimerDelay));
				ToUpdate = true;
			}

			//Update loop
			for (int i = 0; i < Entities.Count; i++)
			{
				Entities[i].Update();

				//Physics Update
				if (ToUpdate)
				{
					//We must run the physics method. Call the physics update event
					Entities[i].PhysicsUpdate();
				}
			}

			//Update the player
			player.Update();

			//Physics Update
			if (ToUpdate)
			{
				//We must run the physics method. Call the physics update event
				player.PhysicsUpdate();
			}
		}

		public void Draw(Entity player)
		{
			//TicksPerSecond
			Timer += (float)Time.GameTime.ElapsedGameTime.TotalSeconds;

			bool ToUpdate = false;

			while (Timer > 1 / TimerDelay)
			{
				Timer -= (1 / TimerDelay) * FastMath.Floor(Timer / (1 / TimerDelay));
				ToUpdate = true;
			}

			for (int i = 0; i < Entities.Count; i++)
			{
				//Physics Update
				if (ToUpdate)
				{
					//We must run the physics method. Call the physics update event
					Entities[i].PhysicsUpdate();
				}

				//Draw the entity after applying physics
				Entities[i].Draw();
			}

			player.Draw();
		}

		public void Tick()
		{
			//TicksPerSecond
			Timer += (float)Time.GameTime.ElapsedGameTime.TotalSeconds;

			bool ToUpdate = false;

			while (Timer > 1 / TimerDelay)
			{
				Timer -= (1 / TimerDelay) * FastMath.Floor(Timer / (1 / TimerDelay));
				ToUpdate = true;
			}

			for (int i = 0; i < Entities.Count; i++)
			{
				//Physics Update
				if (ToUpdate)
				{
					//We must run the physics method. Call the physics update event
					Entities[i].PhysicsUpdate();
				}

				//Draw the entity after applying physics
				Entities[i].OnTick();
			}
		}

		public Entity[] GetEntitiesAt(Vector3Int Blockpos, float Radius)
		{
			List<Entity> toReturn = new List<Entity>();
			for (int i = 0; i < Entities.Count; i++)
			{
				if (Vector3Int.DistanceSquared(Entities[i].blockPosition, Blockpos) <= Radius * Radius)
				{
					toReturn.Add(Entities[i]);
				}
			}
			return toReturn.ToArray();
		}
	}
}
