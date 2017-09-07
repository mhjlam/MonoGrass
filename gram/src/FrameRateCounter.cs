using Microsoft.Xna.Framework;

namespace gram3
{
	class FrameRateCounter : DrawableGameComponent
	{
		private int frameRate;
		private int frameCounter;
		private int secondsPassed;

		public int FrameRate => frameRate;

		public FrameRateCounter(Application game) : base(game)
		{
			frameRate = 0;
			frameCounter = 0;
			secondsPassed = 0;
		}

		public override void Update(GameTime gameTime)
		{
			if (secondsPassed != gameTime.TotalGameTime.Seconds)
			{
				frameRate = frameCounter;
				secondsPassed = gameTime.TotalGameTime.Seconds;
				frameCounter = 0;
			}

			frameCounter++;
		}
	}
}
