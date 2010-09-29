using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using org.flixel;

namespace Mode
{
    public class Bullet : FlxSprite
    {
        //[Embed(source="../../../data/bullet.png")] private var ImgBullet:Class;
        //[Embed(source="../../../data/jump.mp3")] private var SndHit:Class;
        //[Embed(source="../../../data/shoot.mp3")] private var SndShoot:Class;
        private Texture2D ImgBullet;
        private const string SndHit = "Mode/jump";
        private const string SndShoot = "Mode/shoot";

		public Bullet()
		{
            ImgBullet= FlxG.Content.Load<Texture2D>("Mode/bullet");

			loadGraphic(ImgBullet,true);
			width = 6;
			height = 6;
			offset.X = 1;
			offset.Y = 1;
			exists = false;
			
			addAnimation("up", new int[] {0});
			addAnimation("down", new int[] {1});
			addAnimation("left",new int[] {2});
			addAnimation("right",new int[] {3});
			addAnimation("poof", new int[] {4, 5, 6, 7}, 50, false);
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
			FlxG.play(SndShoot);
			base.reset(X,Y);
			solid = true;
			velocity.X = VelocityX;
			velocity.Y = VelocityY;
			if(velocity.Y < 0)
				play("up");
			else if(velocity.Y > 0)
				play("down");
			else if(velocity.X < 0)
				play("left");
			else if(velocity.X > 0)
				play("right");
		}

    }
}
