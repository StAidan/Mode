using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using org.flixel;

namespace Mode
{
    public class VictoryState : FlxState
    {
		private Texture2D ImgGibs;
		private const string SndMenu = "Mode/menu_hit_2";
		
		private float _timer;
		private bool _fading;

		override public void create()
		{
            base.create();

            ImgGibs=FlxG.Content.Load<Texture2D>("Mode/spawner_gibs");

			_timer = 0;
			_fading = false;
			FlxG.flash.start(new Color(0xd8, 0xeb, 0xa2));
			
			//Gibs emitted upon death
			FlxEmitter gibs = new FlxEmitter(0,-50);
			gibs.setSize(FlxG.width,0);
			gibs.setXSpeed();
			gibs.setYSpeed(0,100);
			gibs.setRotation(-360,360);
			gibs.gravity = 80;
			gibs.createSprites(ImgGibs,800);
			add(gibs);
			gibs.start(false,0.005f);
			
			add((new FlxText(0,FlxG.height/3,FlxG.width,"VICTORY\nSCORE: "+FlxG.score)).setFormat(null,3,new Color(0xd8, 0xeb, 0xa2), FlxJustification.Center, Color.Black));
		}

		override public void update()
		{
			base.update();
			if(!_fading)
			{
				_timer += FlxG.elapsed;
				if((_timer > 0.35) && ((_timer > 10) || FlxG.keys.justPressed(Keys.X) || FlxG.keys.justPressed(Keys.C) || FlxG.gamepads.isNewButtonPress(Buttons.Start) || FlxG.gamepads.isNewButtonPress(Buttons.A)))
				{
					_fading = true;
					FlxG.play(SndMenu);
					FlxG.fade.start(new Color(0x13, 0x1c, 0x1b),2,onPlay,false);
				}
			}
		}

        private void onPlay(object Sender, FlxEffectCompletedEvent e) 
		{
			FlxG.state = new PlayState();
		}
    }
}
