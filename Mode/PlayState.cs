using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using org.flixel;

namespace Mode
{
    public class PlayState : FlxState
    {
        protected Texture2D ImgTech;
        protected Texture2D ImgDirtTop;
        protected Texture2D ImgDirt;
        protected Texture2D ImgNotch;
        protected const string SndHit2 = "Mode/menu_hit_2";
        protected const string SndMode = "Mode/mode";
        protected const string SndCount = "Mode/countdown";
        private Texture2D ImgGibs;
        private Texture2D ImgSpawnerGibs;
		
		//major game objects
		protected FlxGroup _blocks;
		protected FlxGroup _decorations;
		protected FlxGroup _bullets;
		protected Player _player;
		protected FlxGroup _bots;
		protected FlxGroup _spawners;
		protected FlxGroup _botBullets;
		protected FlxEmitter _littleGibs;
		protected FlxEmitter _bigGibs;
		
		//meta groups, to help speed up collisions
		protected FlxGroup _objects;
		protected FlxGroup _enemies;
		
		//HUD
		protected FlxText _score;
		protected FlxText _score2;
		protected float _scoreTimer;
		protected float _jamTimer;
		protected FlxSprite _jamBar;
		protected FlxText _jamText;
		protected List<FlxSprite> _notches = new List<FlxSprite>();
		
		//just to prevent weirdness during level transition
		protected bool _fading;
		
		//used to safely reload the playstate after dying
		public bool reload;
		
		override public void create()
		{
            base.create();

            ImgTech=FlxG.Content.Load<Texture2D>("Mode/tech_tiles");
            ImgDirtTop=FlxG.Content.Load<Texture2D>("Mode/dirt_top");
            ImgDirt=FlxG.Content.Load<Texture2D>("Mode/dirt");
            ImgNotch=FlxG.Content.Load<Texture2D>("Mode/notch");
            ImgGibs=FlxG.Content.Load<Texture2D>("Mode/gibs");
            ImgSpawnerGibs = FlxG.Content.Load<Texture2D>("Mode/spawner_gibs");

			FlxG.mouse.hide();
			reload = false;
			
			//get the gibs set up and out of the way
			_littleGibs = new FlxEmitter();
			_littleGibs.delay = 3;
			_littleGibs.setXSpeed(-150,150);
			_littleGibs.setYSpeed(-200,0);
			_littleGibs.setRotation(-720,-720);
			_littleGibs.createSprites(ImgGibs,100,true,0.5f,0.65f);
			_bigGibs = new FlxEmitter();
			_bigGibs.setXSpeed(-200,200);
			_bigGibs.setYSpeed(-300,0);
			_bigGibs.setRotation(-720,-720);
			_bigGibs.createSprites(ImgSpawnerGibs,50,true,0.5f,0.35f);
			
			//level generation needs to know about the spawners (and thusly the bots, players, etc)
			_blocks = new FlxGroup();
			_decorations = new FlxGroup();
			_bullets = new FlxGroup();
			_player = new Player(316,300,_bullets.members,_littleGibs);
			_bots = new FlxGroup();
			_botBullets = new FlxGroup();
			_spawners = new FlxGroup();
			
			//simple procedural level generation
			int i;
			int r = 160;
			FlxTileblock b;
			
			b = new FlxTileblock(0,0,640,16);
			b.loadGraphic(ImgTech);
			_blocks.add(b);
			
			b = new FlxTileblock(0,16,16,640-16);
			b.loadGraphic(ImgTech);
			_blocks.add(b);
			
			b = new FlxTileblock(640-16,16,16,640-16);
			b.loadGraphic(ImgTech);
			_blocks.add(b);
			
			b = new FlxTileblock(16,640-24,640-32,8);
			b.loadGraphic(ImgDirtTop);
			_blocks.add(b);
			
			b = new FlxTileblock(16,640-16,640-32,16);
			b.loadGraphic(ImgDirt);
			_blocks.add(b);

            buildRoom(r * 0, r * 0, true);
			buildRoom(r*1,r*0);
			buildRoom(r*2,r*0);
            buildRoom(r * 3, r * 0, true);
            buildRoom(r * 0, r * 1, true);
			buildRoom(r*1,r*1);
			buildRoom(r*2,r*1);
            buildRoom(r * 3, r * 1, true);
			buildRoom(r*0,r*2);
			buildRoom(r*1,r*2);
			buildRoom(r*2,r*2);
			buildRoom(r*3,r*2);
			buildRoom(r*0,r*3,true);
			buildRoom(r*1,r*3);
			buildRoom(r*2,r*3);
			buildRoom(r*3,r*3,true);
			
			//Add bots and spawners after we add blocks to the state,
			// so that they're drawn on top of the level, and so that
			// the bots are drawn on top of both the blocks + the spawners.
			add(_spawners);
			add(_littleGibs);
			add(_bigGibs);
			add(_blocks);
			add(_decorations);
			add(_bots);
			
			//actually create the bullets now
			for(i = 0; i < 50; i++)
				_botBullets.add(new BotBullet());
			for(i = 0; i < 8; i++)
				_bullets.add(new Bullet());

			//add player and set up scrolling camera
			add(_player);
			FlxG.follow(_player,2.5f);
			FlxG.followAdjust(0.5f,0.0f);
			FlxG.followBounds(0,0,640,640);
			
			//add gibs + bullets to scene here, so they're drawn on top of pretty much everything
			add(_botBullets);
			add(_bullets);
			
			//finally we are going to sort things into a couple of helper groups.
			//we don't add these to the state, we just use them for collisions later!
			_enemies = new FlxGroup();
			_enemies.add(_botBullets);
			_enemies.add(_spawners);
			_enemies.add(_bots);
			_objects = new FlxGroup();
			_objects.add(_botBullets);
			_objects.add(_bullets);
			_objects.add(_bots);
			_objects.add(_player);
			_objects.add(_littleGibs);
			_objects.add(_bigGibs);
			
			//HUD - score
			Vector2 ssf = new Vector2(0,0);
			_score = new FlxText(0,0,FlxG.width);
			_score.color = new Color (0xd8, 0xeb, 0xa2);
			_score.scale = 2;
			_score.alignment = FlxJustification.Center;
			_score.scrollFactor = ssf;
			_score.shadow = new Color(0x13, 0x1c, 0x1b);
			add(_score);
            if (FlxG.scores.Count < 2)
            {
                FlxG.scores.Add(0);
                FlxG.scores.Add(0);
            }
			
			//HUD - highest and last scores
			_score2 = new FlxText(FlxG.width/2,0,FlxG.width/2);
			_score2.color = new Color(0xd8, 0xeb, 0xa2);
			_score2.alignment = FlxJustification.Right;
			_score2.scrollFactor = ssf;
			_score2.shadow = _score.shadow;
			add(_score2);
            if (FlxG.score > FlxG.scores[0])
                FlxG.scores[0] = FlxG.score;
            if (FlxG.scores[0] != 0)
                _score2.text = "HIGHEST: " + FlxG.scores[0] + "\nLAST: " + FlxG.score;
			FlxG.score = 0;
			_scoreTimer = 0;
			
			//HUD - the "number of spawns left" icons
            _notches = new List<FlxSprite>();
            FlxSprite tmp;
			for(i = 0; i < 6; i++)
			{
				tmp = new FlxSprite(4+i*10,4);
				tmp.loadGraphic(ImgNotch,true);
				tmp.scrollFactor.X = tmp.scrollFactor.Y = 0;
				tmp.addAnimation("on", new int[] {0});
				tmp.addAnimation("off",new int[] {1});
				tmp.moves = false;
				tmp.solid = false;
				tmp.play("on");
				_notches.Add((FlxSprite)this.add(tmp));
			}
			
			//HUD - the "gun jammed" notification
			_jamBar = this.add((new FlxSprite(0,FlxG.height-22)).createGraphic(FlxG.width,24, new Color(0x13, 0x1c, 0x1b))) as FlxSprite;
			_jamBar.scrollFactor.X = _jamBar.scrollFactor.Y = 0;
			_jamBar.visible = false;
			_jamText = new FlxText(0,FlxG.height-22,FlxG.width,"GUN IS JAMMED");
			_jamText.color = new Color(0xd8, 0xeb, 0xa2);
			_jamText.scale = 2;
			_jamText.alignment = FlxJustification.Center;
			_jamText.scrollFactor = ssf;
			_jamText.visible = false;
			add(_jamText);
			
			FlxG.playMusic(SndMode);
			FlxG.flash.start(new Color(0x13, 0x1c, 0x1b), 0.5f, null, false);
			_fading = false;
		}

		override public void update()
		{
            PlayerIndex pi;

			int os = FlxG.score;
			
			base.update();
			
			//collisions with environment
			FlxU.collide(_blocks,_objects);
			FlxU.overlap(_enemies,_player,overlapped);
			FlxU.overlap(_bullets,_enemies,overlapped);
			
			//Jammed message
			if(FlxG.keys.justPressed(Keys.C) && _player.flickering())
			{
				_jamTimer = 1;
				_jamBar.visible = true;
				_jamText.visible = true;
			}
			if(_jamTimer > 0)
			{
				if(!_player.flickering()) _jamTimer = 0;
				_jamTimer -= FlxG.elapsed;
				if(_jamTimer < 0)
				{
					_jamBar.visible = false;
					_jamText.visible = false;
				}
			}

			if(!_fading)
			{
				//Score + countdown stuffs
				if(os != FlxG.score) _scoreTimer = 2;
				_scoreTimer -= FlxG.elapsed;
				if(_scoreTimer < 0)
				{
					if(FlxG.score > 0) 
					{
						FlxG.play(SndCount);
						if(FlxG.score > 100) FlxG.score -= 100;
						else { FlxG.score = 0; _player.kill(); }
						_scoreTimer = 1;
						if(FlxG.score < 600)
							FlxG.play(SndCount);
						if(FlxG.score < 500)
							FlxG.play(SndCount);
						if(FlxG.score < 400)
							FlxG.play(SndCount);
						if(FlxG.score < 300)
							FlxG.play(SndCount);
						if(FlxG.score < 200)
							FlxG.play(SndCount);
					}
				}
			
				//Fade out to victory screen stuffs
				int spawnerCount = _spawners.countLiving();
				if(spawnerCount <= 0)
				{
					_fading = true;
					FlxG.fade.start(new Color(0xd8, 0xeb, 0xa2),3,onVictory,false);
				}
				else
				{
					int l = _notches.Count;
					for(int i = 0; i < l; i++)
					{
						if(i < spawnerCount)
							_notches[i].play("on");
						else
							_notches[i].play("off");
					}
				}
			}
			
			//actually update score text if it changed
			if(os != FlxG.score)
			{
				if(_player.dead) FlxG.score = 0;
				_score.text = FlxG.score.ToString();
			}
			
			if(reload)
				FlxG.state = new PlayState();
			
			//Toggle the bounding box visibility
			if(FlxG.keys.justPressed(Microsoft.Xna.Framework.Input.Keys.B))
				FlxG.showBounds = !FlxG.showBounds;

            if (FlxG.gamepads.isNewButtonPress(Buttons.Back, FlxG.controllingPlayer, out pi))
            {
                _fading = true;
                FlxG.play(SndHit2);
                FlxG.flash.start(new Color(0xd8, 0xeb, 0xa2), 0.5f, null, false);
                FlxG.fade.start(new Color(0x13, 0x1c, 0x1b), 1f, onFade, false);
            }
		}

        protected bool overlapped(object Sender, FlxSpriteCollisionEvent e)
		{
			if((e.Object1 is BotBullet) || (e.Object1 is Bullet))
				e.Object1.kill();
			e.Object2.hurt(1);
            return true;
		}
		
		protected void onVictory(object Sender, FlxEffectCompletedEvent e)
		{
			FlxG.music.stop();
			FlxG.state = new VictoryState();
		}
		
		//Just plops down a spawner and some blocks - haphazard and crappy atm but functional!
        protected void buildRoom(int RX, int RY)
        {
            buildRoom(RX, RY, false);
        }
		protected void buildRoom(int RX, int RY, bool Spawners)
		{
			//first place the spawn point (if necessary)
			int rw = 20;
			int sx = 0;
			int sy = 0;
			if(Spawners)
			{
				sx = 2+(int)(FlxU.random()*(rw-7));
				sy = 2+(int)(FlxU.random()*(rw-7));
			}
			
			//then place a bunch of blocks
			int numBlocks = 5+(int)(FlxU.random()*4);
			if(!Spawners) numBlocks++;
			int maxW = 10;
			int minW = 2;
			int maxH = 6;
			int minH = 1;
			int bx;
			int by;
			int bw;
			int bh;
			bool check;
			for(int i = 0; i < numBlocks; i++)
			{
				check = false;
				do
				{
					//keep generating different specs if they overlap the spawner
					bw = minW + (int)(FlxU.random()*(maxW-minW));
                    bh = minH + (int)(FlxU.random() * (maxH - minH));
                    bx = -1 + (int)(FlxU.random() * (rw + 1 - bw));
                    by = -1 + (int)(FlxU.random() * (rw + 1 - bh));
					if(Spawners)
						check = ((sx>bx+bw) || (sx+3<bx) || (sy>by+bh) || (sy+3<by));
					else
						check = true;
				} while(!check);
				
				FlxTileblock b;
				
				b = new FlxTileblock(RX+bx*8,RY+by*8,bw*8,bh*8);
				b.loadTiles(ImgTech);
				_blocks.add(b);
				
				//If the block has room, add some non-colliding "dirt" graphics for variety
				if((bw >= 4) && (bh >= 5))
				{
					b = new FlxTileblock(RX+bx*8+8,RY+by*8,bw*8-16,8);
					b.loadTiles(ImgDirtTop);
					_decorations.add(b);
					
					b = new FlxTileblock(RX+bx*8+8,RY+by*8+8,bw*8-16,bh*8-24);
					b.loadTiles(ImgDirt);
					_decorations.add(b);
				}
			}
			
			//Finally actually add the spawner
			if(Spawners)
				_spawners.add(new Spawner(RX+sx*8,RY+sy*8,_bigGibs,_bots,_botBullets.members,_littleGibs,_player));
		}

        private void onFade(object sender, FlxEffectCompletedEvent e)
        {
            FlxG.music.stop();
            FlxG.state = new MenuState();
        }

    }
}
