using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#if !WINDOWS_PHONE
using Microsoft.Xna.Framework.Storage;
#endif
using org.flixel;

namespace Mode
{
    public class MenuState : FlxState
    {
        private static bool _alreadySaved = false;

        private Texture2D ImgGibs;
        private Texture2D ImgCursor;
        private const string SndHit = "Mode/menu_hit";
        private const string SndHit2 = "Mode/menu_hit_2";

		private FlxEmitter _gibs;
		private FlxButton _b;
		private FlxText _t1;
		private FlxText _t2;
		private bool _ok;
		private bool _ok2;


#if !WINDOWS_PHONE
        FlxSave save;
        private bool hasCheckedSaveFile = false;
#endif

		override public void create()
		{
            base.create();

            ImgGibs = FlxG.Content.Load<Texture2D>("Mode/spawner_gibs");
            ImgCursor = FlxG.Content.Load<Texture2D>("Mode/cursor");

			_gibs = new FlxEmitter(FlxG.width/2-50,FlxG.height/2-10);
			_gibs.setSize(100,30);
			_gibs.setYSpeed(-200,-20);
			_gibs.setRotation(-720,720);
			_gibs.gravity = 100;
			_gibs.createSprites(ImgGibs,1000);
			add(_gibs);
				
			_t1 = new FlxText(FlxG.width,FlxG.height/3-10,80,"mo");
			_t1.scale = 4; // size = 32
			_t1.color = new Color(0x3a, 0x5c, 0x39);
			_t1.antialiasing = true;
			add(_t1);

			_t2 = new FlxText(-60,FlxG.height/3-10,80,"de");
			_t2.scale = _t1.scale;
			_t2.color = _t1.color;
			_t2.antialiasing = _t1.antialiasing;
			add(_t2);
			
			_ok = false;
			_ok2 = false;
			
			FlxG.mouse.show(ImgCursor);

 #if !WINDOWS_PHONE
           if (!_alreadySaved)
            {
                _alreadySaved = true;
                //Simple use of flixel save game object
                save = new FlxSave();
                //In X-flixel, we have to wait until the user selects a storage device.
                //We'll check the X-flixel-only members "waitingOnDeviceSelector" and "canSave"
                //during the update() call.
                hasCheckedSaveFile = false;
            }
            else
            {
                hasCheckedSaveFile = true;
            }
#endif
		}

		override public void update()
		{
            PlayerIndex pi;

#if !WINDOWS_PHONE
            if (!hasCheckedSaveFile)
            {
                if (save.waitingOnDeviceSelector)
                {
                    return;
                }
                else if (save.canSave)
                {
                    hasCheckedSaveFile = true;
                    if (save.bind("Mode"))
                    {
                        if (save.data["plays"] == null)
                            save.data["plays"] = "0";
                        else
                            save.data["plays"] = (int.Parse(save.data["plays"]) + 1).ToString();
                        FlxG.log("Number of plays: " + save.data["plays"]);
                        save.forceSave(0);
                    }
                }
            }
#endif

			//Slides the text onto the screen
			int t1m = FlxG.width/2-64;
			if(_t1.x > t1m)
			{
				_t1.x -= FlxG.elapsed*FlxG.width;
				if(_t1.x < t1m) _t1.x = t1m;
			}
			int t2m = FlxG.width/2+16;
			if(_t2.x < t2m)
			{
				_t2.x += FlxG.elapsed*FlxG.width;
				if(_t2.x > t2m) _t2.x = t2m;
			}
			
			//Check to see if the text is in position
			if(!_ok && ((_t1.x == t1m) || (_t2.x == t2m)))
			{
				//explosion
				_ok = true;
                FlxG.play(SndHit);
                FlxG.flash.start(new Color(0xd8, 0xeb, 0xa2), 0.5f, null, false);
				FlxG.quake.start(0.035f,0.5f);
				_t1.color = new Color(0xd8, 0xeb, 0xa2);
                _t2.color = new Color(0xd8, 0xeb, 0xa2);
				_gibs.start(true,5);
				_t1.angle = FlxU.random()*40-20;
				_t2.angle = FlxU.random()*40-20;

				FlxText t1;
				FlxText t2;
				FlxButton b;

                t1 = new FlxText(t1m, FlxG.height / 3 + 39, 110, "by Adam Atomic");
				t1.alignment = FlxJustification.Center;
				t1.color = new Color(0x3a, 0x5c, 0x39);
				add(t1);
				
				//flixel button
				this.add((new FlxSprite(t1m+1,FlxG.height/3+53)).createGraphic(106,19, new Color(0x13, 0x1c, 0x1b)));
				b = new FlxButton(t1m+2,FlxG.height/3+54,onFlixel);
				b.loadGraphic((new FlxSprite()).createGraphic(104,15,new Color(0x3a, 0x5c, 0x39)),(new FlxSprite()).createGraphic(104,15, new Color(0x72, 0x99, 0x54)));
				t1 = new FlxText(2,1,100,"www.flixel.org");
				t1.color = new Color(0x72, 0x99, 0x54);
				t2 = new FlxText(t1.x,t1.y,t1.width,t1.text);
				t2.color = new Color(0xd8, 0xeb, 0xa2);
                b.loadText(t1, t2);
				add(b);
				
				//danny B button
                this.add((new FlxSprite(t1m + 1, FlxG.height / 3 + 75)).createGraphic(106, 19, new Color(0x13, 0x1c, 0x1b)));
				b = new FlxButton(t1m+2,FlxG.height/3+76,onDanny);
                b.loadGraphic((new FlxSprite()).createGraphic(104, 15, new Color(0x3a, 0x5c, 0x39)), (new FlxSprite()).createGraphic(104, 15, new Color(0x72, 0x99, 0x54)));
                t1 = new FlxText(2, 1, 100, "music: danny B");
				t1.color = new Color(0x72, 0x99, 0x54);
                t2 = new FlxText(t1.x, t1.y, t1.width, t1.text);
				t2.color = new Color(0xd8, 0xeb, 0xa2);
                b.loadText(t1, t2);
				add(b);
				
				//play button
                this.add((new FlxSprite(t1m + 1, FlxG.height / 3 + 137)).createGraphic(106, 19, new Color(0x13, 0x1c, 0x1b)));
#if WINDOWS
				t1 = new FlxText(t1m,FlxG.height/3+139,110,"PRESS X+C TO PLAY.");
#endif
#if XBOX360
				t1 = new FlxText(t1m,FlxG.height/3+139,110,"PRESS START TO PLAY.");
#endif
#if WINDOWS_PHONE
				t1 = new FlxText(t1m,FlxG.height/3+139,110,"TAP TO PLAY.");
#endif
                t1.color = new Color(0x72, 0x99, 0x54);
				t1.alignment = FlxJustification.Center;
				add(t1);

#if WINDOWS
				_b = new FlxButton((FlxG.width - 154) / 2,FlxG.height/3+138,onButton);
                _b.loadGraphic((new FlxSprite()).createGraphic(134, 15, new Color(0x3a, 0x5c, 0x39)), (new FlxSprite()).createGraphic(134, 15, new Color(0x72, 0x99, 0x54)));
				t1 = new FlxText(2,1,130,"CLICK HERE");
                t1.color = new Color(0x72, 0x99, 0x54);
				t2 = new FlxText(t1.x,t1.y,t1.width,t1.text);
				t2.color = new Color(0xd8, 0xeb, 0xa2);
                _b.loadText(t1, t2);
				add(_b);
#endif
			}
			
			//X + C were pressed, fade out and change to play state
			if(_ok && !_ok2 &&
                ((FlxG.keys.isKeyDown(Keys.X, FlxG.controllingPlayer, out pi) && FlxG.keys.isKeyDown(Keys.C, FlxG.controllingPlayer, out pi))
                || (FlxG.gamepads.isNewButtonPress(Buttons.Start, FlxG.controllingPlayer, out pi))))
			{
				_ok2 = true;
                FlxG.play(SndHit2);
				FlxG.flash.start(new Color(0xd8, 0xeb, 0xa2),0.5f, null, false);
				FlxG.fade.start(new Color(0x13, 0x1c, 0x1b),1f,onFade, false);
			}
            else if (FlxG.gamepads.isNewButtonPress(Buttons.Back, FlxG.controllingPlayer, out pi))
            {
                FlxG.Game.Exit();
            }

			base.update();
		}

		private void onFlixel()
		{
			FlxU.openURL("http://flixel.org");
		}
		
		private void onDanny()
		{
			FlxU.openURL("http://dbsoundworks.com");
		}
		
		private void onButton()
		{
			_b.visible = false;
			_b.active = false;
            FlxG.play(SndHit2);
		}

        private void onFade(object sender, FlxEffectCompletedEvent e)
		{
			FlxG.state = new PlayState();
			//FlxG.state = new PlayStateTiles();
		}

    }
}
