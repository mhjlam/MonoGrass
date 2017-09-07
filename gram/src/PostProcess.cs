using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace gram3
{
	public class PostProcess
	{
		protected GraphicsDevice device;
		protected SpriteBatch spritebatch;
		protected Effect effect;

		// A post-processor uses an effect to render over a previously rendered texture.
		public PostProcess(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, Effect postProcessEffect)
		{
			device = graphicsDevice ?? throw new ArgumentNullException("graphicsDevice");
			spritebatch = spriteBatch ?? throw new ArgumentNullException("spriteBatch");
			effect = postProcessEffect ?? throw new ArgumentNullException("postProcessEffect");
		}

		// Draw the scene with the bitmap renderer using a given texture that has been rendered beforehand.
		public virtual void Draw(RenderTarget2D renderTarget)
		{
			// prevent run-time errors by making sure required data is set
			if (renderTarget == null) return;
			
			device.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);

			// setup bitmap renderer
			//spritebatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearClamp, null, null, effect);
			spritebatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearClamp, null, null, effect);

			// draw the bitmap to the screen: use input texture, fill entire screen, no tint
			spritebatch.Draw(renderTarget, Vector2.Zero, Color.White);

			// flush the bitmap renderer
			spritebatch.End();
		}
	}

	public class GaussianBlur : PostProcess
	{
		// blur coefficient (sigma / σ)
		private float sigma;

		// horizontal and vertical weights for the sampler
		private float[] horizontalWeights, verticalWeights;

		// horizontal and vertical offsets for the sampler
		private Vector2[] horizontalOffsets, verticalOffsets;

		// texture to render first pass to
		private RenderTarget2D firstPassCapture;


		// The Guassian blur post-processor is based on the default post-processor with added calculations for required shader parameters.
		public GaussianBlur(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, Effect postProcessEffect, float blurCoefficient = 2.0f) : base(graphicsDevice, spriteBatch, postProcessEffect)
		{
			sigma = blurCoefficient;

			float centerX = 1f / (float)device.Viewport.Width;
			float centerY = 1f / (float)device.Viewport.Height;

			// weights and offsets for horizontal pass
			CalculateParameters(centerX, 0, out horizontalWeights, out horizontalOffsets);

			// weights and offsets for vertical pass
			CalculateParameters(0, centerY, out verticalWeights, out verticalOffsets);

			// intialize texture
			firstPassCapture = new RenderTarget2D(device, device.Viewport.Width, device.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24);
		}

		// Apply the Gaussian blur operation in two passes: one horizontally over the texture, another vertically over the result of the first pass.
		public override void Draw(RenderTarget2D renderTarget)
		{
			// Gaussian blur can be applied to a two-dimensional image as two independent one-dimensional calculations:

			// parameters for horizontal pass
			effect.Parameters["Offsets"].SetValue(horizontalOffsets);
			effect.Parameters["Weights"].SetValue(horizontalWeights);

			// render first pass
			device.SetRenderTarget(firstPassCapture);
			base.Draw(renderTarget);
			device.SetRenderTarget(null);

			// parameters for vertical pass
			effect.Parameters["Offsets"].SetValue(verticalOffsets);
			effect.Parameters["Weights"].SetValue(verticalWeights);

			// render second pass using the result of the first pass
			base.Draw(firstPassCapture);
		}

		// Calculate the weights and offsets for the samples in the shader.
		private void CalculateParameters(float w, float h, out float[] weights, out Vector2[] offsets)
		{
			// when computing a discrete approximation of the Gaussian function, 
			// pixels at a distance of more than 3σ can be ignored (second paragraph: http://en.wikipedia.org/wiki/Gaussian_blur#Mechanics)
			int limit = (int)(3 * sigma); // the shader can support a blur coefficient up to 5.0f, since it always uses 15 samples

			// prevent any out of range errors that may occur when calculating the parameters
			if (limit % 2 == 0) limit++; // increment limit if number is even

			// number of samples for the shader is determined by the limit
			weights = new float[limit];
			offsets = new Vector2[limit];

			// calulate values for center texel (which yield no blur effect on their own)
			weights[0] = GaussianFunction(0);
			offsets[0] = new Vector2(0, 0);

			// keep track of the total weight for normalization
			float totalWeight = weights[0];

			for (int i = 0; i < limit / 2; ++i)
			{
				// the sample weights are based on the Gaussian function to give precedence to texels that are close to the center texel
				float weight = GaussianFunction(i + 1);
				totalWeight += weight * 2;

				// samples are offset by 1.5 pixels to make use of filtering halfway between pixels
				Vector2 offset = new Vector2(w, h) * (i * 2 + 1.5f);

				// store parameters
				weights[i * 2 + 1] = weight;
				weights[i * 2 + 2] = weight;

				offsets[i * 2 + 1] = offset;
				offsets[i * 2 + 2] = -offset;
			}

			// normalize all weights so the total sum will be 1 
			// solves decoloration issues that occurred during conversion from continuous values to discrete values (http://en.wikipedia.org/wiki/Gaussian_blur#Implementation)
			for (int i = 0; i < weights.Length; i++) weights[i] /= totalWeight;
		}

		// Equation for Gaussian function in one dimension (http://en.wikipedia.org/wiki/Gaussian_blur#Mechanics)
		private float GaussianFunction(float x) // x is the distance from the center pixel
		{
			float sigma2 = sigma * sigma;
			return (float)((1.0f / Math.Sqrt(2 * Math.PI * sigma2)) * Math.Exp(-(x * x) / (2 * sigma2)));
		}
	}
}
