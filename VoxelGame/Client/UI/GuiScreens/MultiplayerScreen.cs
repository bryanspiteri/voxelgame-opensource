using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Networking;
using VoxelGame.Util.Managers;

namespace VoxelGame.Client.UI
{
	public class MultiplayerScreen : GuiScreen
	{
		public GuiScreen previousScreen;

		private UiImage background;
		public UiTextbox serverIP, serverPort;
		public UiButton cancelBtn, joinBtn;
		public UiLangText serverJoinHeader;

		public MultiplayerScreen() : base()
		{
			background = new UiImage(this, Point.Zero, Point.Zero, "textures/ui/ui_background");
			AddComponent(background);

			serverIP = new UiTextbox(this, Point.Zero, new Point(264, 20), "multiplayer.add.ip");
			serverIP.maxCharacters = 256;
			serverIP.isLanguageText = true;
			AddComponent(serverIP);

			serverPort = new UiTextbox(this, Point.Zero, new Point(164, 20), "multiplayer.server.port");
			serverPort.maxCharacters = 5;
			serverPort.isLanguageText = true;
			AddComponent(serverPort);

			cancelBtn = new UiButton(this, "multiplayer.add.cancel", Point.Zero, new Point(164, 20));
			AddComponent(cancelBtn);
			cancelBtn.OnClicked += OnCancelClicked;

			joinBtn = new UiButton(this, "multiplayer.add.join", Point.Zero, new Point(164, 20));
			AddComponent(joinBtn);
			joinBtn.OnClicked += OnJoinClicked;

			serverJoinHeader = new UiLangText(this, Point.Zero, "multiplayer.add.title");
			AddComponent(serverJoinHeader);

			visible = false;
			Reset();
		}

		public void Reset()
		{

		}

		public override void Update()
		{
			if (visible)
			{
				#region Centering
				background.size = new Point(GameClient.ViewWidth, GameClient.ViewHeight);

				serverJoinHeader.location = new Point(GameClient.ViewWidth / 2 - serverJoinHeader.size.X / 2, GameClient.ViewHeight / 2 - GameSettings.UIScale * 80);

				serverIP.size = new Point(serverIP.rawSize.X * GameSettings.UIScale, serverIP.rawSize.Y * GameSettings.UIScale);
				serverPort.size = new Point(serverPort.rawSize.X * GameSettings.UIScale, serverPort.rawSize.Y * GameSettings.UIScale);

				serverIP.location = new Point(GameClient.ViewWidth / 2 - (serverIP.size.X + serverPort.size.X + 5 * GameSettings.UIScale) / 2, GameClient.ViewHeight / 2 - serverIP.size.Y / 2);
				serverPort.location = new Point(serverIP.location.X + serverIP.size.X + 5 * GameSettings.UIScale, serverIP.location.Y);

				cancelBtn.size = new Point(cancelBtn.rawSize.X * GameSettings.UIScale, cancelBtn.rawSize.Y * GameSettings.UIScale);
				joinBtn.size = new Point(joinBtn.rawSize.X * GameSettings.UIScale, joinBtn.rawSize.Y * GameSettings.UIScale);

				cancelBtn.location = new Point(GameClient.ViewWidth / 2 + 5 * GameSettings.UIScale, GameClient.ViewHeight - cancelBtn.size.Y - 50 * GameSettings.UIScale);
				joinBtn.location = new Point(GameClient.ViewWidth / 2 - 5 * GameSettings.UIScale - joinBtn.size.X, GameClient.ViewHeight - joinBtn.size.Y - 50 * GameSettings.UIScale);

				#endregion
			}

			base.Update();
		}

		public override void HandleEscape()
		{
			OnCancelClicked(null, null);
		}

		#region Button Handling

		public void OnCancelClicked(object sender, EventArgs e)
		{
			visible = false;
			previousScreen.visible = true;
			GameClient.canPause = true;
		}

		public void OnJoinClicked(object sender, EventArgs e)
		{
			//Hide all UI apart from debug and loading screen
			UiRegisterer.loadingUI.visible = true;
			UiRegisterer.optionsUI.visible = false;
			visible = false;

			//Set the world renderer.
			//TODO: Multiplayer Socket Open, todo more
			GameClient.World.End();
			GameClient.theWorld = new World.World(new NetworkingManager() { packetTransfer = new PacketTransfererWAN() });
			GameClient.Instance.worldRenderer = new WorldRenderer();

			int port = 11043;
			if (!int.TryParse(serverPort.text, out port))
			{
				port = 11043;
			}

			GameClient.theWorld.networkingManager.Connect(serverIP.text, port, "dummy-access-token", new PacketTransfererWAN());

			//unpause the client
			GameClient.isPaused = false;
			GameClient.canPause = true;

			//Hide the loading screen
			GameClient.DiscordManager.SetState("Making something work", "In Debug World");

			//Just for good measure
			UiRegisterer.optionsUI.visible = false;
			UiRegisterer.optionsUI.Reset();
			visible = false;
		}

		#endregion
	}
}
