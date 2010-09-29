using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using org.flixel;

namespace Mode
{
    public class Bot : FlxSprite
    {
		protected Texture2D ImgBot;
		protected Texture2D ImgJet;
		protected const string SndExplode = "Mode/asplode";
		protected const string SndHit = "Mode/hit";
		protected const string SndJet = "Mode/jet";
		
		protected FlxEmitter _gibs;
		protected FlxEmitter _jets;
		protected Player _player;
		protected float _timer;
        protected List<FlxObject> _b;
		static protected int _cb = 0;
		protected float _shotClock;

        public Bot(int xPos, int yPos, List<FlxObject> Bullets, FlxEmitter Gibs, Player ThePlayer)
            : base(xPos,yPos)
		{
            ImgBot = FlxG.Content.Load<Texture2D>("Mode/bot");
            ImgJet = FlxG.Content.Load<Texture2D>("Mode/jetsmoke");

			loadGraphic(ImgBot,false);
			_player = ThePlayer;
			_b = Bullets;
			_gibs = Gibs;
			
			width = 12;
			height = 12;
			offset.X = 2;
			offset.Y = 2;
			maxAngular = 120;
			angularDrag = 400;
			maxThrust = 100;
			drag.X = 80;
			drag.Y = 80;
			
			//Jet effect that shoots out from behind the bot
			_jets = new FlxEmitter();
			_jets.setRotation();
			_jets.gravity = 0;
			_jets.createSprites(ImgJet,15,false);

			reset(x,y);
		}
		
		override public void update()
		{			
			float ot = _timer;
			if((_timer == 0) && onScreen()) FlxG.play(SndJet);
			_timer += FlxG.elapsed;
			if((ot < 8) && (_timer >= 8))
				_jets.stop(0.1f);

			//Aiming
			float dx = x-_player.x;
			float dy = y-_player.y;
			float da = FlxU.getAngle(dx,dy);
			if(da < 0)
				da += 360;
			float ac = angle;
			if(ac < 0)
				ac += 360;
			if(da < angle)
				angularAcceleration = -angularDrag;
			else if(da > angle)
				angularAcceleration = angularDrag;
			else
				angularAcceleration = 0;

			//Jets
			thrust = 0;
			if(_timer > 9)
				_timer = 0;
			else if(_timer < 8)
			{
				thrust = 40;
				Vector2 v = FlxU.rotatePoint(thrust,0,0,0,angle);
				_jets.at(this);
				_jets.setXSpeed(v.X-30,v.X+30);
				_jets.setYSpeed(v.Y-30,v.Y+30);
				if(!_jets.on)
					_jets.start(false,0.01f,0);
			}

			//Shooting
			if(onScreen())
			{
				float os = _shotClock;
				_shotClock += FlxG.elapsed;
				if((os < 4.0) && (_shotClock >= 4.0))
				{
					_shotClock = 0;
					shoot();
				}
				else if((os < 3.5) && (_shotClock >= 3.5))
					shoot();
				else if((os < 3.0) && (_shotClock >= 3.0))
					shoot();
			}
			
			_jets.update();
			base.update();
		}
		
		override public void render(SpriteBatch spriteBatch)
		{
			_jets.render(spriteBatch);
			base.render(spriteBatch);
		}
		
		override public void hurt(float Damage)
		{
			FlxG.play(SndHit);
			flicker(0.2f);
			FlxG.score += 10;
			base.hurt(Damage);
		}
		
		override public void kill()
		{
			if(dead)
				return;
			FlxG.play(SndExplode);
			base.kill();
			flicker(-1);
			_jets.kill();
			_gibs.at(this);
			_gibs.start(true,0,20);
			FlxG.score += 200;
		}
		
		override public void reset(float X, float Y)
		{
			base.reset(X,Y);
			thrust = 0;
			velocity.X = 0;
			velocity.Y = 0;
			angle = FlxU.random()*360 - 180;
			health = 2;
			_timer = 0;
			_shotClock = 0;
		}
		
		protected void shoot()
		{
			Vector2 ba = FlxU.rotatePoint(-120,0,0,0,angle);
            ((BotBullet)_b[_cb]).shoot((int)(x + width / 2 - 2), (int)(y + height / 2 - 2), (int)ba.X, (int)ba.Y);
			if(++_cb >= _b.Count) _cb = 0;
		}
    }
}
