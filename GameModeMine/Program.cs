﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using ManicDigger.Network;
using ManicDigger.Renderers;

namespace ManicDigger
{
    public class ManicDiggerProgram2 : IInternetGameFactory, ICurrentShadows
    {
        public string GameUrl = null;
        public string User = null;
        public string Password = null;
        ManicDiggerGameWindow w;
        AudioOpenAl audio;
        public void Start()
        {
            w = new ManicDiggerGameWindow();
            audio = new AudioOpenAl();
            w.audio = audio;
            MakeGame(true);
            w.GameUrl = GameUrl;
            if (User != null)
            {
                w.username = User;
            }
            if (Password != null)
            {
                w.mppassword = Password;
            }
            w.Run();
        }
        GameMinecraft clientgame;
        private void MakeGame(bool singleplayer)
        {
            var gamedata = new GameDataTilesMinecraft();

            INetworkClient network;
            if (singleplayer)
            {
                network = new NetworkClientDummy();
            }
            else
            {
                network = new NetworkClientMinecraft();
            }
            clientgame = new GameMinecraft();
            /* fix for crash */
            //w.fpshistorygraphrenderer = new FpsHistoryGraphRenderer() { draw = w, viewportsize = w };
            var mapstorage = clientgame;
            var getfile = new GetFilePath(new[] { "mine", "minecraft" });
            var config3d = new Config3d();
            var mapManipulator = new MapManipulator();
            var terrainDrawer = new TerrainRenderer();
            var the3d = w;
            var exit = w;
            var localplayerposition = w;
            var worldfeatures = new WorldFeaturesDrawer();
            var physics = new CharacterPhysics();
            var mapgenerator = new MapGeneratorPlain();
            var internetgamefactory = this;
            if (singleplayer)
            {
                var n = (NetworkClientDummy)network;
                n.player = localplayerposition;
                n.Gui = w;
                n.Map1 = w;
                n.Map = mapstorage;
                n.Data = gamedata;
                n.Gen = new fCraft.MapGenerator();
                n.Gen.data = gamedata;
                n.Gen.log = new fCraft.FLogDummy();
                n.Gen.map = new MyFCraftMap() { data = gamedata, map = mapstorage, mapManipulator = mapManipulator };
                n.Gen.rand = new GetRandomDummy();
                n.DEFAULTMAP = "mountains";
            }
            else
            {
                var n = (NetworkClientMinecraft)network;
                n.Map = w;
                n.Clients = clientgame;
                n.Chatlines = w;
                n.Position = localplayerposition;
                n.gameworld = new GameWorldTodoDummy();
            }
            terrainDrawer.the3d = the3d;
            terrainDrawer.getfile = getfile;
            terrainDrawer.config3d = config3d;
            terrainDrawer.mapstorage = clientgame;
            terrainDrawer.data = gamedata;
            terrainDrawer.exit = exit;
            terrainDrawer.localplayerposition = localplayerposition;
            terrainDrawer.worldfeatures = worldfeatures;
            terrainDrawer.OnCrash += (a, b) => { CrashReporter.Crash(b.exception); };
            var terrainChunkDrawer = new TerrainChunkRenderer();
            terrainChunkDrawer.config3d = config3d;
            terrainChunkDrawer.data = gamedata;
            terrainChunkDrawer.mapstorage = clientgame;
            terrainDrawer.terrainchunkdrawer = terrainChunkDrawer;
            terrainChunkDrawer.terrainrenderer = terrainDrawer;
            worldfeatures.getfile = getfile;
            worldfeatures.localplayerposition = localplayerposition;
            worldfeatures.mapstorage = mapstorage;
            worldfeatures.the3d = the3d;
            mapManipulator.getfile = getfile;
            mapManipulator.mapgenerator = mapgenerator;
            w.map = clientgame;
            w.physics = physics;
            w.clients = clientgame;
            w.network = network;
            w.data = gamedata;
            w.getfile = getfile;
            w.config3d = config3d;
            w.mapManipulator = mapManipulator;
            w.terrain = terrainDrawer;
            bool IsMono = Type.GetType("Mono.Runtime") != null;
            terrainDrawer.textureatlasconverter = new TextureAtlasConverter();
            if (IsMono)
            {
                terrainDrawer.textureatlasconverter.fastbitmapfactory = () => { return new FastBitmapDummy(); };
            }
            else
            {
                terrainDrawer.textureatlasconverter.fastbitmapfactory = () => { return new FastBitmap(); };
            }
            weapon = new WeaponBlockInfo() { data = gamedata, terrain = terrainDrawer, viewport = w, map = clientgame, shadows = shadowssimple };
            w.weapon = new WeaponRenderer() { info = weapon, playerpos = w }; //no torch in mine mode
            var playerdrawer = new CharacterRendererMonsterCode();
            playerdrawer.Load(new List<string>(File.ReadAllLines(getfile.GetFile("player.mdc"))));
            w.characterdrawer = playerdrawer;
            w.particleEffectBlockBreak = new ParticleEffectBlockBreak() {  d_Data = gamedata, d_Map = clientgame, d_Terrain = terrainDrawer, d_Shadows = shadowsfull};
            clientgame.terrain = terrainDrawer;
            clientgame.network = network;

            clientgame.viewport = w;
            clientgame.data = gamedata;
            w.game = clientgame;
            w.login = new LoginClientMinecraft();
            w.internetgamefactory = internetgamefactory;
            PlayerSkinDownloader playerskindownloader = new PlayerSkinDownloader();
            playerskindownloader.exit = w;
            playerskindownloader.the3d = the3d;
            playerskindownloader.skinserver = "http://minecraft.net/skin/";
            w.playerskindownloader = playerskindownloader;
            physics.map = clientgame;
            physics.data = gamedata;
            mapgenerator.data = gamedata;
            audio.getfile = getfile;
            audio.gameexit = w;
            shadowsfull = new Shadows()
            {
                data = gamedata,
                map = clientgame,
                terrain = terrainDrawer,
                localplayerposition = localplayerposition,
                config3d = config3d
            };
            shadowssimple = new ShadowsSimple() { data = gamedata, map = clientgame};
            UseShadowsSimple();
            w.currentshadows = this;
            
            var frustumculling = new FrustumCulling() { the3d = the3d };
            terrainDrawer.frustumculling = frustumculling;
            terrainDrawer.batcher = new MeshBatcher() { frustumculling = frustumculling };
            terrainDrawer.frustumculling = frustumculling;
            w.RenderFrame += (a, b) => { frustumculling.CalcFrustumEquations(); };
            var dirtychunks = new DirtyChunks() { mapstorage = clientgame };
            terrainDrawer.ischunkready = dirtychunks;
            terrainDrawer.ischunkready.frustum = frustumculling;
            terrainDrawer.ischunkready.Start();
            //clientgame.ischunkready = dirtychunks;
            

            if (Debugger.IsAttached)
            {
                new DependencyChecker(typeof(InjectAttribute)).CheckDependencies(
                    w, audio, gamedata, clientgame, network, mapstorage, getfile,
                    config3d, mapManipulator, terrainDrawer, the3d, exit,
                    localplayerposition, worldfeatures, physics, mapgenerator,
                    internetgamefactory, playerdrawer, mapManipulator,
                    w.login, shadowsfull, shadowssimple, terrainChunkDrawer);
            }
        }
        ShadowsSimple shadowssimple;
        Shadows shadowsfull;
        WeaponBlockInfo weapon;
        #region IInternetGameFactory Members
        public void NewInternetGame()
        {
            MakeGame(false);
        }
        #endregion
        void UseShadowsSimple()
        {
            if (clientgame.shadows != null)
            {
                shadowssimple.sunlight = clientgame.shadows.sunlight;
            }
            clientgame.shadows = shadowssimple;
            weapon.shadows = clientgame.shadows;
        }
        void UseShadowsFull()
        {
            if (clientgame.shadows != null)
            {
                shadowsfull.sunlight = clientgame.shadows.sunlight;
            }
            clientgame.shadows = shadowsfull;
            weapon.shadows = clientgame.shadows;
        }
        bool fullshadows = false;
        #region ICurrentShadows Members
        public bool ShadowsFull
        {
            get
            {
                return fullshadows;
            }
            set
            {
                if (value && !fullshadows)
                {
                    UseShadowsFull();
                }
                if (!value && fullshadows)
                {
                    UseShadowsSimple();
                }
                fullshadows = value;
            }
        }
        #endregion
    }
    public class ManicDiggerProgram
    {
        [STAThread]
        public static void Main(string[] args)
        {
            new CrashReporter().Start(Start, args);
        }
        private static void Start(string[] args)
        {
            if (!Debugger.IsAttached)
            {
                string appPath = Path.GetDirectoryName(Application.ExecutablePath);
                System.Environment.CurrentDirectory = appPath;
            }
            var p = new ManicDiggerProgram2();
            if (args.Length > 0)
            {
                if (args[0].EndsWith(".mdlink", StringComparison.InvariantCultureIgnoreCase))
                {
                    XmlDocument d = new XmlDocument();
                    d.Load(args[0]);
                    string mode = XmlTool.XmlVal(d, "/ManicDiggerLink/GameMode");
                    if (mode != "Mine")
                    {
                        throw new Exception("Invalid game mode: " + mode);
                    }
                    p.GameUrl = XmlTool.XmlVal(d, "/ManicDiggerLink/Ip");
                    int port = int.Parse(XmlTool.XmlVal(d, "/ManicDiggerLink/Port"));
                    p.GameUrl += ":" + port;
                    p.User = XmlTool.XmlVal(d, "/ManicDiggerLink/User");
                    p.Password = XmlTool.XmlVal(d, "/ManicDiggerLink/Password");
                }
            }
            p.Start();
        }
    }
}