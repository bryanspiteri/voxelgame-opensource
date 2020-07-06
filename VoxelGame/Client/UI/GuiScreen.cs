using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Client;

namespace VoxelGame.Client
{
	public class GuiScreen
	{
		public List<Component> components = new List<Component>();

		public Component FocusedComponent;

		private bool _visible = true;
		public bool visible
		{
			get { return _visible; }
			set
			{
				if (_visible != value)
				{
					_visible = value;
					for (int i = 0; i < components.Count; i++)
					{
						components[i].screenVisible = value;
					}
					Mouse.SetCursor(MouseCursor.Arrow);
				}
			}
		}

		public GuiScreen()
		{

		}

		public void AddComponent(Component toAdd)
		{
			components.Add(toAdd);
		}

		public virtual void Update()
		{
			#region Handle Escape
			if (visible)
			{
				if (InputManager.Released("misc.pause"))
				{
					//If it isnt a textbox
					if (FocusedComponent != null)
					{
						if (FocusedComponent.GetType() == typeof(UiTextbox))
						{
							if (!((UiTextbox)FocusedComponent).isEditing)
							{
								HandleEscape();
							}
							else
							{
								if (!((UiTextbox)FocusedComponent).isMouseHovering())
								{
									Mouse.SetCursor(MouseCursor.Arrow);
								}
							}
						}
						else
						{
							HandleEscape();
						}
					}
					else
					{
						HandleEscape();
					}

				}
			}
			#endregion

			//Reset focus to null
			//FocusedComponent = null;

			foreach (Component component in components)
			{
				component.onUpdate();
			}
		}

		public virtual void HandleEscape()
		{

		}

		public virtual void Render()
		{
			int componentsDone = 0;
			for (int currZ = -1024; currZ < 1024; currZ++)
			{
				foreach (Component component in components)
				{
					if (currZ == component.zIndex)
					{
						if (component.render) {
							component.Draw();
						}
						componentsDone++;
						if (componentsDone == components.Count)
						{
							return;
						}
					}
				}
			}
		}
	}
}
