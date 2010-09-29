using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using org.flixel;

namespace Mode
{
    public class Spawner : FlxSprite
    {
		private Texture2D ImgSpawner;
		private const string SndExplode = "Mode/asplode";
		private const string SndExplode2 = "Mode/menu_hit_2";
		private const string SndHit = "Mode/hit";
		
		private float _timer;
		private FlxGroup _bots;
		private List<FlxObject> _botBullets;
		private FlxEmitter _botGibs;
		private FlxEmitter _gibs;
		private Player _player;
		private bool _open;
		
		public Spawner(int X, int Y, FlxEmitter Gibs, FlxGroup Bots, List<FlxObject> BotBullets, FlxEmitter BotGibs, Player ThePlayer)
            : base (X, Y)
		{
            ImgSpawner = FlxG.Content.Load<Texture2D>("Mode/spawner");

			loadGraphic(ImgSpawner,true);
			_gibs = Gibs;
			_bots = Bots;
			_botBullets = BotBullets;
			_botGibs = BotGibs;
			_player = ThePlayer;
			_timer = FlxU.random()*20;
			_open = false;
			health = 8;

			addAnimation("open", new int[] {1, 2, 3, 4, 5}, 40, false);
			addAnimation("close", new int[] {4, 3, 2, 1, 0}, 40, false);
			addAnimation("dead", new int[] {6});
		}
		
		override public void update()
		{
			_timer += FlxG.elapsed;
			int limit = 20;
			if(onScreen())
				limit = 4;
			if(_timer > limit)
			{
				_timer = 0;
				makeBot();
			}
			else if(_timer > limit - 0.35)
			{
				if(!_open)
				{
					_open = true;
					play("open");
				}
			}
			else if(_timer > 1)
			{
				if(_open)
				{
					play("close");
					_open = false;
				}
			}
				
			base.update();
		}
		
		override public void hurt(float Damage)
		{
			FlxG.play(SndHit);
			flicker(0.2f);
			FlxG.score += 50;
			base.hurt(Damage);
		}
		
		override public void kill()
		{
			if(dead)
				return;
			FlxG.play(SndExplode);
			FlxG.play(SndExplode2);
			base.kill();
			active = false;
			exists = true;
			solid = false;
			flicker(-1);
			play("dead");
			FlxG.quake.start(0.005f,0.35f);
			FlxG.flash.start(new Color(0xd8, 0xeb, 0xa2),0.35f, null, false);
			makeBot();
			_gibs.at(this);
			_gibs.start(true,3,0);
			FlxG.score += 1000;
		}
		
		protected void makeBot()
		{
			//Try to recycle a dead bot
			if(_bots.resetFirstAvail((int)(x + width/2 - 6), (int)(y + height/2 - 6)))
				return;
			
			//If there weren't any non-existent ones to respawn, just add a new one instead
            Bot bot = new Bot((int)(x + width / 2), (int)(y + height / 2), _botBullets, _botGibs, _player);
			bot.x -= bot.width/2;
			bot.y -= bot.height/2;
			_bots.add(bot);
		}
    }
}
