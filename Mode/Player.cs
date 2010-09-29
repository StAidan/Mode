using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using org.flixel;

namespace Mode
{
    public class Player : FlxSprite
    {
		private Texture2D ImgSpaceman;
		private const string SndJump = "Mode/jump";
		private const string SndLand = "Mode/land";
		private const string SndExplode = "Mode/asplode";
		private const string SndExplode2 = "Mode/menu_hit_2";
		private const string SndHurt = "Mode/hurt";
		private const string SndJam = "Mode/jam";
		
		private int _jumpPower;
        private List<FlxObject> _bullets;
		private int _curBullet;
		private int _bulletVel;
		private bool _up;
		private bool _down;
		private float _restart;
		private FlxEmitter _gibs;
		
		public Player(int X, int Y, List<FlxObject> Bullets, FlxEmitter Gibs)
            : base(X, Y)
		{
            ImgSpaceman = FlxG.Content.Load<Texture2D>("Mode/spaceman");

			loadGraphic(ImgSpaceman,true,true,8);
			_restart = 0;
			
			//bounding box tweaks
			width = 6;
			height = 7;
			offset.X = 1;
			offset.Y = 1;
			
			//basic player physics
			int runSpeed = 80;
			drag.X = runSpeed*8;
			acceleration.Y = 420;
			_jumpPower = 205;
			maxVelocity.X = runSpeed;
			maxVelocity.Y = _jumpPower;
			
			//animations
			addAnimation("idle", new int[] {0});
			addAnimation("run", new int [] {1, 2, 3, 0}, 12);
			addAnimation("jump", new int[] {4});
			addAnimation("idle_up", new int[] {5});
			addAnimation("run_up", new int[] {6, 7, 8, 5}, 12);
			addAnimation("jump_up", new int[] {9});
			addAnimation("jump_down", new int[] {10});
			
			//bullet stuff
			_bullets = Bullets;
			_curBullet = 0;
			_bulletVel = 360;
			
			//Gibs emitted upon death
			_gibs = Gibs;
		}
		
		override public void update()
		{
            PlayerIndex pi;

			//game restart timer
			if(dead)
			{
				_restart += FlxG.elapsed;
				if(_restart > 2)
					(FlxG.state as PlayState).reload = true;
				return;
			}
			
			//MOVEMENT
			acceleration.X = 0;
			if(FlxG.keys.LEFT || FlxG.gamepads.isButtonDown(Buttons.LeftThumbstickLeft, FlxG.controllingPlayer, out pi))
			{
				facing = Flx2DFacing.Left;
				acceleration.X -= drag.X;
			}
            else if (FlxG.keys.RIGHT || FlxG.gamepads.isButtonDown(Buttons.LeftThumbstickRight, FlxG.controllingPlayer, out pi))
			{
				facing = Flx2DFacing.Right;
				acceleration.X += drag.X;
			}
			if((FlxG.keys.justPressed(Keys.X) || FlxG.gamepads.isNewButtonPress(Buttons.A, FlxG.controllingPlayer, out pi))
                && velocity.Y == 0)
			{
				velocity.Y = -_jumpPower;
				FlxG.play(SndJump);
			}
			
			//AIMING
			_up = false;
			_down = false;
			if(FlxG.keys.UP || FlxG.gamepads.isButtonDown(Buttons.LeftThumbstickUp, FlxG.controllingPlayer, out pi)) _up = true;
			else if((FlxG.keys.DOWN  || FlxG.gamepads.isButtonDown(Buttons.LeftThumbstickDown, FlxG.controllingPlayer, out pi)) && velocity.Y != 0) _down = true;
			
			//ANIMATION
			if(velocity.Y != 0)
			{
				if(_up) play("jump_up");
				else if(_down) play("jump_down");
				else play("jump");
			}
			else if(velocity.X == 0)
			{
				if(_up) play("idle_up");
				else play("idle");
			}
			else
			{
				if(_up) play("run_up");
				else play("run");
			}
			
			//SHOOTING
            if (!flickering() && (FlxG.keys.justPressed(Keys.C) ||
                    FlxG.gamepads.isNewButtonPress(Buttons.X, FlxG.controllingPlayer, out pi)))
			{
				int bXVel = 0;
				int bYVel = 0;
				int bX = (int)x;
				int bY = (int)y;
				if(_up)
				{
					bY -= (int)_bullets[_curBullet].height - 4;
					bYVel = -_bulletVel;
				}
				else if(_down)
				{
					bY += (int)height - 4;
					bYVel = _bulletVel;
					velocity.Y -= 36;
				}
				else if(facing == Flx2DFacing.Right)
				{
					bX += (int)width - 4;
					bXVel = _bulletVel;
				}
				else
				{
					bX -= (int)_bullets[_curBullet].width - 4;
					bXVel = -_bulletVel;
				}
				((Bullet)(_bullets[_curBullet])).shoot(bX,bY,bXVel,bYVel);
				if(++_curBullet >= _bullets.Count)
					_curBullet = 0;
			}
				
			//UPDATE POSITION AND ANIMATION
			base.update();

			//Jammed, can't fire!
			if(flickering())
			{
				if(FlxG.keys.justPressed(Keys.C) ||
                    FlxG.gamepads.isNewButtonPress(Buttons.X, FlxG.controllingPlayer, out pi))
					FlxG.play(SndJam);
			}
		}
		
		override public void hitBottom(FlxObject Contact, float Velocity)
		{
			if(velocity.Y > 50)
				FlxG.play(SndLand);
			onFloor = true;
			base.hitBottom(Contact,Velocity);
		}
		
		override public void hurt(float Damage)
		{
			Damage = 0;
			if(flickering())
				return;
			FlxG.play(SndHurt);
			flicker(1.3f);
			if(FlxG.score > 1000) FlxG.score -= 1000;
			if(velocity.X > 0)
				velocity.X = -maxVelocity.X;
			else
				velocity.X = maxVelocity.X;
			base.hurt(Damage);
		}

        override public void kill()
		{
			if(dead)
				return;
			solid = false;
			FlxG.play(SndExplode);
			FlxG.play(SndExplode2);
			base.kill();
			flicker(-1);
			exists = true;
			visible = false;
			FlxG.quake.start(0.005f,0.35f);
			FlxG.flash.start(new Color(0xd8, 0xeb, 0xa2),0.35f, null, false);
			if(_gibs != null)
			{
				_gibs.at(this);
				_gibs.start(true,0,50);
			}
		}
    }
}
