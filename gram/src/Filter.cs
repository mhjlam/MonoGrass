using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace gram
{
	public class Filter
	{
		protected GraphicsDevice device;
		protected SpriteBatch spritebatch;
		protected Effect effect;

		public Filter(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, Effect postProcessEffect)
		{
			device = graphicsDevice ?? throw new ArgumentNullException("graphicsDevice");
			spritebatch = spriteBatch ?? throw new ArgumentNullException("spriteBatch");
			effect = postProcessEffect ?? throw new ArgumentNullException("postProcessEffect");
		}
		
		public virtual void Draw(RenderTarget2D renderTarget)
		{
			if (renderTarget == null) return;
			
			device.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);

			// Setup bitmap renderer
			spritebatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearClamp, null, null, effect);

			// Draw bitmap to screen: use input texture, fill entire screen, no tint
			spritebatch.Draw(renderTarget, Vector2.Zero, Color.White);

			// Flush bitmap renderer
			spritebatch.End();
		}
	}

	public class GaussianBlur : Filter
	{
		private float sigma;									// Blur coefficient (sigma / σ)
		private float[] horizontalWeights, verticalWeights;		// Horizontal and vertical weights for the sampler
		private Vector2[] horizontalOffsets, verticalOffsets;   // Horizontal and vertical offsets for the sampler
		private RenderTarget2D capture;                         // Render target for first pass


		public GaussianBlur(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, Effect postProcessEffect, float blurCoefficient = 2.0f) : base(graphicsDevice, spriteBatch, postProcessEffect)
		{
			sigma = blurCoefficient;

			float centerX = 1f / (float)device.Viewport.Width;
			float centerY = 1f / (float)device.Viewport.Height;

			// Computer weights and offsets for both passes
			CalculateParameters(centerX, 0, out horizontalWeights, out horizontalOffsets);
			CalculateParameters(0, centerY, out verticalWeights, out verticalOffsets);

			capture = new RenderTarget2D(device, device.Viewport.Width, device.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24);
		}
		
		public override void Draw(RenderTarget2D renderTarget)
		{
			// Parameters for horizontal pass
			effect.Parameters["Offsets"].SetValue(horizontalOffsets);
			effect.Parameters["Weights"].SetValue(horizontalWeights);

			// Render horizontal pass
			device.SetRenderTarget(capture);
			base.Draw(renderTarget);
			device.SetRenderTarget(null);

			// Parameters for vertical pass
			effect.Parameters["Offsets"].SetValue(verticalOffsets);
			effect.Parameters["Weights"].SetValue(verticalWeights);

			// Render vertical pass
			base.Draw(capture);
		}

		// Calculate the weights and offsets for the samples in the shader.
		private void CalculateParameters(float w, float h, out float[] weights, out Vector2[] offsets)
		{
			// When computing a discrete approximation of the Gaussian function, 
			// pixels at a distance of more than 3σ can be ignored (second paragraph: http://en.wikipedia.org/wiki/Gaussian_blur#Mechanics)
			int limit = (int)(3 * sigma); // The shader can support a blur coefficient up to 5.0f, since it always uses 15 samples

			// Prevent any out of range errors that may occur when calculating the parameters
			if (limit % 2 == 0) limit++; // Increment limit if number is even

			// Number of samples for the shader is determined by the limit
			weights = new float[limit];
			offsets = new Vector2[limit];

			// Center texel yields no blur effect on its own
			weights[0] = GaussianFunction(0);
			offsets[0] = new Vector2(0, 0);

			// Keep track of the total weight for normalization
			float totalWeight = weights[0];

			for (int i = 0; i < limit / 2; ++i)
			{
				// Sample weights are based on the Gaussian function to give precedence to texels that are close to the center texel
				float weight = GaussianFunction(i + 1);
				totalWeight += weight * 2;

				// Samples are offset by 1.5 pixels to make use of filtering halfway between pixels
				Vector2 offset = new Vector2(w, h) * (i * 2 + 1.5f);

				// Save parameters
				weights[i * 2 + 1] = weight;
				weights[i * 2 + 2] = weight;

				offsets[i * 2 + 1] = offset;
				offsets[i * 2 + 2] = -offset;
			}

			// Normalize all weights so the total sum will be unity, resolving decoloration issues due to discretization (http://en.wikipedia.org/wiki/Gaussian_blur#Implementation)
			for (int i = 0; i < weights.Length; i++)
				weights[i] /= totalWeight;
		}

		// Equation for Gaussian function in one dimension (http://en.wikipedia.org/wiki/Gaussian_blur#Mechanics)
		private float GaussianFunction(float x) // x is the distance from the center pixel
		{
			float sigma2 = sigma * sigma;
			return (float)((1.0f / Math.Sqrt(2 * Math.PI * sigma2)) * Math.Exp(-(x * x) / (2 * sigma2)));
		}
	}
}
