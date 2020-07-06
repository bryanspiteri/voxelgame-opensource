using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VoxelGame.Client
{
    public sealed class GraphicsState
    {
        private static Stack<GraphicsState> _states;
        private BlendState _blendState;
        private DepthStencilState _depthStencilState;
        private SamplerState _samplerState;
        
        public BlendState BlendState
        {
            get
            {
                return _blendState;
            }
            private set
            {
                if (value != null)
                {
                    _blendState = value;
                }
            }
        }
        
        public DepthStencilState DepthStencilState
        {
            get
            {
                return _depthStencilState;
            }
            private set
            {
                if (value != null)
                {
                    _depthStencilState = value;
                }
            }
        }
        
        public SamplerState SamplerState
        {
            get
            {
                return _samplerState;
            }
            private set
            {
                if (value != null)
                {
                    _samplerState = value;
                }
            }
        }

        static GraphicsState()
        {
            _states = new Stack<GraphicsState>();
        }
        
        public static void Push()
        {
            lock (_states)
            {
                _states.Push(new GraphicsState(GameClient.Instance.GraphicsDevice));
            }
        }
        
        public static void Pop()
        {
            lock (_states)
            {
                if (_states.Count > 0)
                {
                    var state = _states.Pop();
                    state.Restore();
                }
            }
        }
        
        public static GraphicsState Peek()
        {
            return _states.Peek();
        }
        
        public GraphicsState(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("graphicsDevice");
            }
            BlendState = graphicsDevice.BlendState ?? BlendState.Opaque;
            DepthStencilState = graphicsDevice.DepthStencilState ?? DepthStencilState.Default;
            SamplerState = graphicsDevice.SamplerStates[0] ?? SamplerState.PointClamp;
        }
        
        private void Restore()
        {
            var device = GameClient.Instance.GraphicsDevice;
            device.BlendState = BlendState;
            device.DepthStencilState = DepthStencilState;
            device.SamplerStates[0] = SamplerState;
        }
    }
}
