using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#if !WINDOWS_PHONE
using Microsoft.Xna.Framework.Storage;
#endif
using System.IO;

using org.flixel;

namespace Mode
{
    public class PlayStateTiles : FlxState
    {
		private const string SndMode = "Mode/mode";
        private const string TxtMap = "Mode/map.txt";
        private const string TxtMap2 = "Mode/map2.txt";
		private Texture2D ImgTiles;
		
		//major game objects
		private FlxTilemap _tilemap;
		private FlxGroup _bullets;
		private Player _player;
		
		override public void create()
		{
            ImgTiles = FlxG.Content.Load<Texture2D>("Mode/tiles_all");

            //load map from file
            string sMap;
            using (Stream file = TitleContainer.OpenStream("Content/" + TxtMap))
            {
                StreamReader sr = new StreamReader(file);
                sMap = sr.ReadToEnd().Replace("\r", "");
                sr.Close();
            }
            //create tilemap
			_tilemap = new FlxTilemap();
			_tilemap.collideIndex = 3;
            _tilemap.loadMap(sMap, ImgTiles, 8, 8);
			//_tilemap.loadMap(new TxtMap2,ImgTiles,8); //This is an alternate tiny map
			
			//create player and bullets
			_bullets = new FlxGroup();
			_player = new Player((int)(_tilemap.width/2-4),(int)_tilemap.height/2-4,_bullets.members,null);
			for(int i = 0; i < 8; i++)
				_bullets.add(new Bullet());
			add(_bullets);
			
			//add player and set up camera
			add(_player);
			FlxG.follow(_player,2.5f);
			FlxG.followAdjust(0.5f,0.0f);
			_tilemap.follow();	//Set the followBounds to the map dimensions
			
			//Uncomment these lines if you want to center TxtMap2
			//var fx:uint = _tilemap.width/2 - FlxG.width/2;
			//var fy:uint = _tilemap.height/2 - FlxG.height/2;
			//FlxG.followBounds(fx,fy,fx,fy);
			
			//add tilemap last so it is in front, looks neat
			add(_tilemap);
			
			//fade in
			FlxG.flash.start(new Color(0x13, 0x1c, 0x1b), 1f);
			
			//The music in this mode is positional - it fades out toward the edges of the level
			FlxSound s = FlxG.play(SndMode,1,true);
			s.proximity(320,320,_player,160);
		}

		override public void update()
		{
			base.update();
			_tilemap.collide(_player);
			_tilemap.collide(_bullets);
			
			//Toggle the bounding box visibility
			if(FlxG.keys.justPressed(Keys.B))
				FlxG.showBounds = !FlxG.showBounds;
		}
    }
}
