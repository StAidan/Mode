using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using org.flixel;

namespace Mode
{
    public class BotBullet : FlxSprite
    {
		private Texture2D ImgBullet;
		private const string SndHit = "Mode/jump";
		private const string SndShoot = "Mode/enemy";
		
		public BotBullet()
		{
            ImgBullet = FlxG.Content.Load<Texture2D>("Mode/bot_bullet");

			loadGraphic(ImgBullet,true);
			addAnimation("idle", new int[] {0, 1}, 50);
			addAnimation("poof", new int[] {2, 3, 4}, 50, false);
			exists = false;
		}
		
		override public void update()
		{
			if(dead && finished) exists = false;
			else base.update();
		}

		override public void hitSide(FlxObject Contact, float Velocity) { kill(); }
		override public void hitBottom(FlxObject Contact, float Velocity) { kill(); }
		override public void hitTop(FlxObject Contact, float Velocity) { kill(); }
		override public void kill()
		{
			if(dead) return;
			velocity.X = 0;
			velocity.Y = 0;
			if(onScreen()) FlxG.play(SndHit);
			dead = true;
			solid = false;
			play("poof");
		}
		
		public void shoot(int X, int Y, int VelocityX, int VelocityY)
		{
			FlxG.play(SndShoot,0.5f);
			base.reset(X,Y);
			solid = true;
			velocity.X = VelocityX;
			velocity.Y = VelocityY;
			play("idle");
		}
    }
}
