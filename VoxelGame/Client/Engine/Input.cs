using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockyWorld.Client
{
    public class Input
    {
        private KeyboardState oldKeys;
        private KeyboardState newKeys;

        private static Input instance;

        public static Input Instance
        {
            get
            {
                if(instance == null){
                    instance = new Input();
                }
                return instance;
            }
            set { }
        }

        /// <summary>
        /// Centers the mouse on this game's window.
        /// </summary>
        public static void CenterMouse()
        {
            Mouse.SetPosition(Game.Instance._pp.BackBufferWidth / 2, Game.Instance._pp.BackBufferHeight / 2);
        }

        /// <summary>
        /// Checks to see if the given key was pressed.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static bool WasKeyPressed(Keys key)
        {
            return Instance.oldKeys.IsKeyDown(key) && Instance.newKeys.IsKeyUp(key);
        }

        public void update()
        {
            // if escape is being pressed, exit
            Instance.newKeys = Keyboard.GetState();
        }

        public void finalUpdate()
        {
            // update old key state and update base game
            Instance.oldKeys = Instance.newKeys;
        }
    }
}
