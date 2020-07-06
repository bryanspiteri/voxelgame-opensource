using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelGame
{
	public static class RasterizerStateExtensions
	{
		private static RasterizerState Copy(RasterizerState state)
		{
			return new RasterizerState()
			{
				CullMode = state.CullMode,
				DepthBias = state.DepthBias,
				FillMode = state.FillMode,
				MultiSampleAntiAlias = state.MultiSampleAntiAlias,
				Name = state.Name,
				ScissorTestEnable = state.ScissorTestEnable,
				SlopeScaleDepthBias = state.SlopeScaleDepthBias,
				Tag = state.Tag,
			};
		}

		public static RasterizerState WithDepthBias(this RasterizerState state, float depthBias)
		{
			RasterizerState copy = Copy(state);
			copy.DepthBias = depthBias;
			return copy;
		}
	}
}
