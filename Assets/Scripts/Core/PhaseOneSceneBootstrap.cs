using Surexs.DanceOff.Data;
using Surexs.DanceOff.Gameplay;
using Surexs.DanceOff.Input;
using Surexs.DanceOff.Player;
using Surexs.DanceOff.Rhythm;
using Surexs.DanceOff.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Surexs.DanceOff.Core
{
    public sealed class PhaseOneSceneBootstrap : MonoBehaviour
    {
        [SerializeField] private AudioClip song;
        [SerializeField] private TextAsset chart;
        private GameMode gameMode => GameSession.SelectedMode;
        [SerializeField] private RhythmGameplayConfig gameplayConfig = new RhythmGameplayConfig();
        [Header("Input Sources")]
        [SerializeField] private RhythmInputSourceType player1InputSource = RhythmInputSourceType.Keyboard;
        [SerializeField] private RhythmInputSourceType player2InputSource = RhythmInputSourceType.Keyboard;
        [SerializeField, Min(0)] private int player1GamepadIndex;
        [SerializeField, Min(0)] private int player2GamepadIndex = 1;
        [SerializeField, Range(0.1f, 0.95f)] private float player1GamepadAxisThreshold = 0.5f;
        [SerializeField, Range(0.1f, 0.95f)] private float player2GamepadAxisThreshold = 0.5f;
        [SerializeField] private JoystickInputConfig player1Joystick = new JoystickInputConfig();
        [SerializeField] private JoystickInputConfig player2Joystick = new JoystickInputConfig { joystickIndex = 1 };
        [Header("Solo Gameplay Graphics")]
        [SerializeField] private Texture background1Player;
        [SerializeField] private Texture tileContainerTexture;
        [SerializeField] private Texture player1Texture;
        [SerializeField] private Texture perfectTexture;
        [SerializeField] private Texture greatTexture;
        [SerializeField] private Texture goodTexture;
        [SerializeField] private Texture missTexture;
        [SerializeField] private Texture blueStarTexture;
        [SerializeField] private Texture yellowStarTexture;
        [SerializeField] private Sprite surexsLogo;
        [Header("Versus Gameplay Graphics")]
        [SerializeField] private Texture background2Player;
        [SerializeField] private Texture player2Texture;
        [SerializeField] private Texture brokerHeroLogoTexture;
        [SerializeField, Min(0.05f)] private float poseDuration = 0.35f;
        [SerializeField] private PoseData[] poseDefinitions =
        {
            new PoseData("neutral", "Neutral", new Color(.22f,.67f,.92f), 15f,-15f),
            new PoseData("phone", "Phone", new Color(.26f,.80f,.65f), 65f,-10f),
            new PoseData("typing", "Typing", new Color(.48f,.58f,.95f),-55f,55f),
            new PoseData("presentation", "Presentation", new Color(.95f,.48f,.38f),125f,-25f),
            new PoseData("coffee", "Coffee", new Color(.62f,.42f,.25f),25f,-75f),
            new PoseData("documents", "Documents", new Color(.92f,.72f,.25f),-20f,70f),
            new PoseData("document", "Documents", new Color(.92f,.72f,.25f),-20f,70f),
            new PoseData("meeting", "Meeting", new Color(.75f,.42f,.90f),115f,-115f),
            new PoseData("thinking", "Thinking", new Color(.38f,.72f,.86f),80f,-5f),
            new PoseData("celebration", "Celebration", new Color(.98f,.55f,.72f),145f,-145f),
            new PoseData("mouse", "Mouse", new Color(.42f,.82f,.48f),10f,-50f)
        };

        private void Awake()
        {
            if (!gameplayConfig.IsValid(out var error)) { Debug.LogError(error, this); return; }
            CreateCamera();
            CreateEventSystem();
            var canvas = CreateCanvas();
            if (gameMode == GameMode.Solo) CreateSoloBackdrop(canvas);
            else CreateVersusBackdrop(canvas);
            var shared = Shared();
            PlayerSession[] sessions;

            if (gameMode == GameMode.LocalVersus)
            {
                var p1TileRoot=MaskedTileRoot("Player 1 Runtime Tiles",canvas,new Vector2(-550f,-155f),new Vector2(630f,520f));
                var p2TileRoot=MaskedTileRoot("Player 2 Runtime Tiles",canvas,new Vector2(550f,-155f),new Vector2(630f,520f));
                var p1 = Player(shared, p1TileRoot, canvas, "PLAYER 1", -550, new[] {-760f,-550f,-340f}, Key.A, Key.W, Key.S, Key.D, new Color(.22f,.67f,.92f),player1InputSource,player1GamepadIndex,player1GamepadAxisThreshold,player1Joystick,PlayerVisualLayout.VersusLeft);
                var p2 = Player(shared, p2TileRoot, canvas, "PLAYER 2",  550, new[] { 340f, 550f, 760f}, Key.LeftArrow, Key.UpArrow, Key.DownArrow, Key.RightArrow, new Color(.95f,.45f,.55f),player2InputSource,player2GamepadIndex,player2GamepadAxisThreshold,player2Joystick,PlayerVisualLayout.VersusRight);
                shared.Controller.Configure(shared.Audio, shared.Chart, new[] {p1.Tiles,p2.Tiles}, new[] {p1.Judge,p2.Judge}, gameplayConfig);
                shared.Root.AddComponent<LocalVersusMatchState>().Configure(p1.Score, p2.Score);
                sessions = new[] { p1.Session, p2.Session };
            }
            else
            {
                var tileRoot=MaskedTileRoot("Runtime Tiles",canvas,new Vector2(-380f,0f),new Vector2(900f,720f));
                var p1 = Player(shared, tileRoot, canvas, "PLAYER 1", 520, new[] {-670f,-380f,-90f}, Key.A, Key.W, Key.S, Key.D, new Color(.22f,.67f,.92f),player1InputSource,player1GamepadIndex,player1GamepadAxisThreshold,player1Joystick,PlayerVisualLayout.Solo);
                shared.Controller.Configure(shared.Audio, shared.Chart, p1.Tiles, p1.Judge, gameplayConfig);
                sessions = new[] { p1.Session };
            }
            var results = CreateResultsView(canvas);
            var flow=shared.Root.AddComponent<GameFlowController>(); flow.Configure(gameMode, shared.Controller, sessions, results);
            shared.Root.AddComponent<GameplayNavigationController>().Configure(flow,shared.Controller,results);
            if (song == null) Debug.LogWarning("[Bootstrap] No hay canción asignada.", this);
            if (chart == null) Debug.LogWarning("[Bootstrap] No hay chart asignado.", this);
        }

        private void CreateSoloBackdrop(Transform canvas)
        {
            if (background1Player != null)
            {
                var background=Raw("Solo Background",canvas,Vector2.zero,new Vector2(1920,1080),background1Player);
                Stretch(background.rectTransform);
            }
            else Image("Solo Background Fallback",canvas,Vector2.zero,new Vector2(1920,1080),SurexsVisualTheme.Background);

            var lanes=new[] {-670f,-380f,-90f};
            for (var i=0;i<lanes.Length;i++)
                Raw("Solo Tile Container "+i,canvas,new Vector2(lanes[i],0),new Vector2(320,720),tileContainerTexture);

            Raw("Solo Player Artwork",canvas,new Vector2(470,-35),new Vector2(620,615),player1Texture);
            Raw("Blue Star",canvas,new Vector2(205,235),new Vector2(76,76),blueStarTexture);
            Raw("Yellow Star",canvas,new Vector2(775,-25),new Vector2(64,64),yellowStarTexture);

            if (surexsLogo != null)
            {
                var logo=Image("Surexs Logo",canvas,new Vector2(-770,-465),new Vector2(260,75),Color.white);
                logo.sprite=surexsLogo; logo.preserveAspect=true;
            }

            WarnMissingSoloAsset(background1Player,nameof(background1Player));
            WarnMissingSoloAsset(tileContainerTexture,nameof(tileContainerTexture));
            WarnMissingSoloAsset(player1Texture,nameof(player1Texture));
            WarnMissingSoloAsset(perfectTexture,nameof(perfectTexture));
            WarnMissingSoloAsset(greatTexture,nameof(greatTexture));
            WarnMissingSoloAsset(goodTexture,nameof(goodTexture));
            WarnMissingSoloAsset(missTexture,nameof(missTexture));
            if (surexsLogo == null) Debug.LogWarning("[Bootstrap] Falta el asset surexsLogo para el layout Solo.",this);
        }

        private void CreateVersusBackdrop(Transform canvas)
        {
            if (background2Player != null)
            {
                var background=Raw("Versus Background",canvas,Vector2.zero,new Vector2(1920,1080),background2Player);
                Stretch(background.rectTransform);
            }
            else Image("Versus Background Fallback",canvas,Vector2.zero,new Vector2(1920,1080),SurexsVisualTheme.Background);

            var p1Lanes=new[] {-760f,-550f,-340f};
            var p2Lanes=new[] {340f,550f,760f};
            for (var i=0;i<3;i++)
            {
                Raw("Player 1 Tile Container "+i,canvas,new Vector2(p1Lanes[i],-155f),new Vector2(190,520),tileContainerTexture);
                Raw("Player 2 Tile Container "+i,canvas,new Vector2(p2Lanes[i],-155f),new Vector2(190,520),tileContainerTexture);
            }

            Raw("Player 1 Artwork",canvas,new Vector2(-550,245),new Vector2(340,335),player1Texture);
            var p2Artwork=Raw("Player 2 Artwork",canvas,new Vector2(550,245),new Vector2(340,335),player2Texture != null ? player2Texture : player1Texture);
            if (player2Texture == null) p2Artwork.color=new Color(1f,.64f,.78f,1f);

            var brandPanel=Image("Central Brand Backdrop",canvas,new Vector2(0,95),new Vector2(310,180),new Color(.01f,.06f,.14f,.95f));
            SurexsVisualTheme.ApplyRounded(brandPanel);
            Raw("Broker Hero Logo",brandPanel.transform,Vector2.zero,new Vector2(290,145),brokerHeroLogoTexture);

            if (surexsLogo != null)
            {
                var logo=Image("Versus Surexs Logo",canvas,new Vector2(0,-480),new Vector2(230,62),Color.white);
                logo.sprite=surexsLogo; logo.preserveAspect=true;
            }
            else
            {
                var logoFallback=Text("Versus Surexs Logo Fallback",canvas,new Vector2(0,-480),new Vector2(230,62),"Surexs",34);
                logoFallback.fontStyle=FontStyle.BoldAndItalic;
            }

            Raw("Versus Blue Star",canvas,new Vector2(-175,285),new Vector2(54,54),blueStarTexture);
            Raw("Versus Yellow Star",canvas,new Vector2(175,285),new Vector2(48,48),yellowStarTexture);
            WarnMissingVersusAsset(background2Player,nameof(background2Player));
            WarnMissingVersusAsset(brokerHeroLogoTexture,nameof(brokerHeroLogoTexture));
            WarnMissingVersusAsset(tileContainerTexture,nameof(tileContainerTexture));
            WarnMissingVersusAsset(player1Texture,nameof(player1Texture));
            WarnMissingVersusAsset(perfectTexture,nameof(perfectTexture));
            WarnMissingVersusAsset(greatTexture,nameof(greatTexture));
            WarnMissingVersusAsset(goodTexture,nameof(goodTexture));
            WarnMissingVersusAsset(missTexture,nameof(missTexture));
            WarnMissingVersusAsset(blueStarTexture,nameof(blueStarTexture));
            WarnMissingVersusAsset(yellowStarTexture,nameof(yellowStarTexture));
            if (player2Texture == null) Debug.LogWarning("[Bootstrap] No existe player2.png; se utiliza player1.png tintado como fallback visual para PLAYER 2.",this);
        }

        private void WarnMissingVersusAsset(Object asset,string field)
        {
            if (asset == null) Debug.LogWarning($"[Bootstrap] Falta el asset visual Versus '{field}'.",this);
        }

        private void WarnMissingSoloAsset(Object asset,string field)
        {
            if (asset == null) Debug.LogWarning($"[Bootstrap] Falta el asset visual Solo '{field}'.",this);
        }

        private SharedSet Shared()
        {
            var root = new GameObject("Shared Rhythm Systems");
            var source = root.AddComponent<AudioSource>(); source.clip = song; source.playOnAwake = false; source.spatialBlend = 0;
            var audio = root.AddComponent<AudioManager>(); audio.Configure(source);
            var charts = root.AddComponent<ChartManager>(); charts.Configure(chart);
            return new SharedSet(root, audio, charts, root.AddComponent<RhythmPrototypeController>());
        }

        private PlayerSet Player(SharedSet shared, RectTransform tileRoot, Transform canvas, string name, float x,
            float[] lanes, Key left, Key centerPrimary, Key centerSecondary, Key right, Color accent,
            RhythmInputSourceType inputSourceType, int gamepadIndex, float gamepadAxisThreshold,
            JoystickInputConfig joystickConfig, PlayerVisualLayout layout)
        {
            var soloRedesign=layout==PlayerVisualLayout.Solo;
            var playerTwo=layout==PlayerVisualLayout.VersusRight;
            var labels = new[] {"LEFT ←","CENTER ●","RIGHT →"};
            var hitZones = new Image[3];
            var hitY=soloRedesign ? -285f : -355f;
            for (var i=0;i<3;i++)
            {
                var hitSize=soloRedesign ? new Vector2(230,72) : new Vector2(170,60);
                hitZones[i]=Image(name+" Hit "+i, canvas, new Vector2(lanes[i],hitY), hitSize, new Color(.12f,.86f,.48f,.72f));
                SurexsVisualTheme.ApplyRounded(hitZones[i]);
                var hitOutline=hitZones[i].gameObject.AddComponent<Outline>(); hitOutline.effectColor=new Color(.45f,1f,.72f,.9f); hitOutline.effectDistance=new Vector2(3,-3);
                Text(name+" Label "+i, canvas, new Vector2(lanes[i],soloRedesign ? 350f : 125f), new Vector2(210,34), labels[i], soloRedesign ? 22 : 16);
                var key=playerTwo ? (i==0 ? "←" : i==1 ? "↑ / ↓" : "→") : (i==0 ? "A" : i==1 ? "W / S" : "D");
                var keyY=soloRedesign ? -420f : -450f;
                var keyWidth=soloRedesign ? 112f : 105f;
                var keyPanel=Image(name+" Key "+i,canvas,new Vector2(lanes[i],keyY),new Vector2(keyWidth,46),new Color(.02f,.18f,.34f,.94f));
                SurexsVisualTheme.ApplyRounded(keyPanel);
                var keyOutline=keyPanel.gameObject.AddComponent<Outline>(); keyOutline.effectColor=new Color(accent.r,accent.g,accent.b,.95f); keyOutline.effectDistance=new Vector2(2,-2);
                Text(name+" Key Label "+i,keyPanel.transform,Vector2.zero,new Vector2(keyWidth,46),key,soloRedesign ? 20 : 18);
            }
            RawImage judgmentImage=null; Text points=null;
            Text scoreLabel=null; Text comboLabel=null; Text songLabel=null; Text statsLabel=null; Image progressFill=null;
            JudgmentStarBurstView starBurst=null;
            var milestone=Text(name+" Milestone",canvas,new Vector2(x,soloRedesign ? 300f : 145f),new Vector2(500,50),"",soloRedesign ? 27 : 22);
            VisualSet visual;
            if (soloRedesign)
            {
                scoreLabel=Text("Solo Score",canvas,new Vector2(750,330),new Vector2(300,170),"",27);
                comboLabel=Text("Solo Combo",canvas,new Vector2(750,155),new Vector2(340,220),"",30);
                songLabel=Text("Solo Song",canvas,new Vector2(720,475),new Vector2(360,90),"",17);
                songLabel.alignment=TextAnchor.MiddleLeft;
                var statsPanel=Image("Solo Stats Panel",canvas,new Vector2(175,-505),new Vector2(760,50),new Color(.01f,.13f,.28f,.92f));
                SurexsVisualTheme.ApplyRounded(statsPanel);
                statsLabel=Text("Solo Stats",statsPanel.transform,Vector2.zero,new Vector2(730,44),"",17);
                var starRoot=Rect("Judgment Star Burst",canvas,Vector2.zero,new Vector2(1920,1080));
                starBurst=starRoot.gameObject.AddComponent<JudgmentStarBurstView>();
                starBurst.Configure(starRoot,blueStarTexture,yellowStarTexture,
                    new Vector2(lanes[0],-285f),new Vector2(lanes[1],-285f),new Vector2(lanes[2],-285f));
                judgmentImage=Raw("Judgment Image",canvas,new Vector2(520,-285),new Vector2(650,215),null);
                points=Text("Judgment Points",canvas,new Vector2(520,-395),new Vector2(240,50),"",30);
                visual=VisualSolo(canvas,name);
            }
            else
            {
                var card=Image(name+" Card",canvas,new Vector2(x,458),new Vector2(630,125),new Color(.01f,.13f,.28f,.95f));
                SurexsVisualTheme.ApplyRounded(card);
                scoreLabel=Text(name+" Score",card.transform,new Vector2(-75,-17),new Vector2(230,90),"",19);
                comboLabel=Text(name+" Combo",card.transform,new Vector2(185,-17),new Vector2(220,100),"",20);
                visual=VisualVersusCard(card.transform,name,accent);

                var statsPanel=Image(name+" Stats Panel",canvas,new Vector2(x,-510),new Vector2(680,42),new Color(.01f,.13f,.28f,.92f));
                SurexsVisualTheme.ApplyRounded(statsPanel);
                statsLabel=Text(name+" Stats",statsPanel.transform,Vector2.zero,new Vector2(650,38),"",14);

                var starRoot=Rect(name+" Judgment Star Burst",canvas,Vector2.zero,new Vector2(1920,1080));
                starBurst=starRoot.gameObject.AddComponent<JudgmentStarBurstView>();
                starBurst.Configure(starRoot,blueStarTexture,yellowStarTexture,
                    new Vector2(lanes[0],hitY),new Vector2(lanes[1],hitY),new Vector2(lanes[2],hitY));
                judgmentImage=Raw(name+" Judgment Image",canvas,new Vector2(x,62),new Vector2(390,125),null);
                points=Text(name+" Judgment Points",canvas,new Vector2(x,-5),new Vector2(220,42),"",24);
            }
            var go=new GameObject(name+" Systems");
            var input=CreateInputSource(go,inputSourceType,gamepadIndex,gamepadAxisThreshold,joystickConfig,left,centerPrimary,centerSecondary,right);
            go.AddComponent<ControlInputFeedbackView>().Configure(input,hitZones[0],hitZones[1],hitZones[2]);
            var judge=go.AddComponent<RhythmJudge>(); judge.Configure(shared.Audio,input,gameplayConfig);
            var combo=go.AddComponent<ComboManager>(); combo.Configure(gameplayConfig);
            var score=go.AddComponent<ScoreManager>(); score.Configure(gameplayConfig,combo);
            var feedback=go.AddComponent<GameplayFeedbackView>();
            feedback.Configure(judgmentImage,points,milestone,perfectTexture,greatTexture,goodTexture,missTexture,starBurst);
            go.AddComponent<JudgmentProcessor>().Configure(judge,score,combo,feedback);
            var poses=go.AddComponent<PoseController>(); poses.Configure(poseDefinitions,visual.Body,visual.LeftArm,visual.RightArm,visual.BodyImage,visual.PoseLabel);
            var player=go.AddComponent<PlayerController>(); player.Configure(judge,poses,poseDuration);
            var tileRootOffset=tileRoot.anchoredPosition;
            var tiles=go.AddComponent<TileSpawner>(); tiles.Configure(shared.Audio,shared.Chart,tileRoot,judge,
                lanes[0]-tileRootOffset.x,lanes[1]-tileRootOffset.x,lanes[2]-tileRootOffset.x,hitY-tileRootOffset.y);
            var statusView=go.AddComponent<PrototypeStatusView>();
            if (soloRedesign) statusView.ConfigureSolo(shared.Audio,shared.Chart,tiles,score,combo,scoreLabel,comboLabel,songLabel,statsLabel,progressFill);
            else statusView.ConfigureVersus(shared.Audio,shared.Chart,tiles,score,combo,scoreLabel,comboLabel,statsLabel);
            return new PlayerSet(judge,tiles,score,new PlayerSession(judge,tiles,score,combo,feedback,player));
        }

        private static IRhythmInputSource CreateInputSource(GameObject owner, RhythmInputSourceType sourceType,
            int gamepadIndex, float gamepadAxisThreshold, JoystickInputConfig joystickConfig,
            Key left, Key centerPrimary, Key centerSecondary, Key right)
        {
            if (sourceType == RhythmInputSourceType.Gamepad)
            {
                var gamepad=owner.AddComponent<GamepadInputReader>();
                gamepad.Configure(gamepadIndex,gamepadAxisThreshold);
                return gamepad;
            }

            if (sourceType == RhythmInputSourceType.Joystick)
            {
                var joystick=owner.AddComponent<JoystickInputReader>();
                joystick.Configure(joystickConfig);
                return joystick;
            }

            var keyboard=owner.AddComponent<KeyboardInputReader>();
            keyboard.Configure(left,centerPrimary,centerSecondary,right);
            return keyboard;
        }

        private string ControlsText()
        {
            var player1=InputLabel("P1",player1InputSource,player1GamepadIndex,player1Joystick);
            if (gameMode != GameMode.LocalVersus) return player1;
            var player2=InputLabel("P2",player2InputSource,player2GamepadIndex,player2Joystick);
            return $"{player1}     •     {player2}";
        }

        private static string InputLabel(string player, RhythmInputSourceType sourceType, int gamepadIndex,
            JoystickInputConfig joystickConfig)
        {
            if (sourceType == RhythmInputSourceType.Gamepad) return $"{player}: GAMEPAD {gamepadIndex + 1}  X / A / B + D-PAD/STICK";
            if (sourceType == RhythmInputSourceType.Joystick) return $"{player}: JOYSTICK {(joystickConfig?.joystickIndex ?? 0) + 1}  STICK / HAT / BUTTONS";
            return player == "P1" ? "P1: A / W-S / D" : "P2: ← / ↑-↓ / →";
        }

        private static ResultsView CreateResultsView(Transform canvas)
        {
            var panel=Image("Results Panel",canvas,Vector2.zero,new Vector2(1920,1080),SurexsVisualTheme.Background);
            Image("Results Glow",panel.transform,new Vector2(0,470),new Vector2(1920,140),SurexsVisualTheme.BackgroundGlow);
            Image("Results Accent",panel.transform,new Vector2(0,418),new Vector2(560,6),SurexsVisualTheme.Accent);
            var p1Card=Image("P1 Results Card",panel.transform,new Vector2(-430,45),new Vector2(650,650),SurexsVisualTheme.SurfaceRaised);
            var p2Card=Image("P2 Results Card",panel.transform,new Vector2(430,45),new Vector2(650,650),new Color(.15f,.07f,.18f,.98f));
            SurexsVisualTheme.ApplyRounded(p1Card); SurexsVisualTheme.ApplyRounded(p2Card);
            var title=Text("Results Title",panel.transform,new Vector2(0,400),new Vector2(1200,90),"RESULTADOS",48);
            var p1=Text("P1 Results",panel.transform,new Vector2(-430,60),new Vector2(620,650),"",28);
            var p2=Text("P2 Results",panel.transform,new Vector2(430,60),new Vector2(620,650),"",28);
            var retry=Button("Rematch",panel.transform,new Vector2(-180,-420),new Vector2(300,75),"REVANCHA",SurexsVisualTheme.Success);
            var menu=Button("Menu",panel.transform,new Vector2(180,-420),new Vector2(300,75),"MENU",SurexsVisualTheme.Primary);
            var view=panel.gameObject.AddComponent<ResultsView>(); view.Configure(panel.gameObject,title,p1,p2,p1Card.gameObject,p2Card.gameObject,retry,menu);
            return view;
        }

        private static void CreateEventSystem()
        {
            if (EventSystem.current != null) return;
            var go=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
            go.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        private static VisualSet Visual(Transform parent,string name,float x,Color accent)
        {
            var stage=Image(name+" Placeholder",parent,new Vector2(x,-425),new Vector2(250,230),SurexsVisualTheme.Surface);
            SurexsVisualTheme.ApplyRounded(stage);
            Text(name+" Name",stage.transform,new Vector2(0,91),new Vector2(230,32),name,19);
            var body=Image("Body",stage.transform,new Vector2(0,-8),new Vector2(75,90),accent);
            Image("Head",stage.transform,new Vector2(0,56),new Vector2(55,55),new Color(1,.78f,.58f));
            var la=Image("Left Arm",stage.transform,new Vector2(-54,-5),new Vector2(24,75),accent).rectTransform;
            var ra=Image("Right Arm",stage.transform,new Vector2(54,-5),new Vector2(24,75),accent).rectTransform;
            var pose=Text("Current Pose",stage.transform,new Vector2(0,-91),new Vector2(230,32),"POSE: NEUTRAL",16);
            return new VisualSet(body.rectTransform,la,ra,body,pose);
        }

        private static VisualSet VisualSolo(Transform parent,string name)
        {
            var panel=Image(name+" Status Panel",parent,new Vector2(755,-455),new Vector2(360,105),new Color(.01f,.13f,.28f,.94f));
            SurexsVisualTheme.ApplyRounded(panel);
            Text(name+" Name",panel.transform,new Vector2(55,20),new Vector2(220,34),name,21);
            var pose=Text("Current Pose",panel.transform,new Vector2(55,-20),new Vector2(220,30),"POSE: NEUTRAL",16);
            pose.color=new Color(.36f,.78f,1f);
            var avatar=Image("Avatar",panel.transform,new Vector2(-125,0),new Vector2(68,68),new Color(.18f,.68f,1f,.9f));
            SurexsVisualTheme.ApplyRounded(avatar);
            var hiddenBody=Image("Pose Driver Body",panel.transform,Vector2.zero,Vector2.zero,Color.clear);
            var hiddenLeft=Rect("Pose Driver Left Arm",hiddenBody.transform,Vector2.zero,Vector2.zero);
            var hiddenRight=Rect("Pose Driver Right Arm",hiddenBody.transform,Vector2.zero,Vector2.zero);
            return new VisualSet(hiddenBody.rectTransform,hiddenLeft,hiddenRight,hiddenBody,pose);
        }

        private static VisualSet VisualVersusCard(Transform card,string name,Color accent)
        {
            var avatar=Image(name+" Avatar",card,new Vector2(-275,15),new Vector2(66,66),accent);
            SurexsVisualTheme.ApplyRounded(avatar);
            var playerName=Text(name+" Name",card,new Vector2(-178,30),new Vector2(210,34),name,23);
            playerName.alignment=TextAnchor.MiddleLeft;
            var pose=Text(name+" Current Pose",card,new Vector2(-178,0),new Vector2(210,26),"POSE: NEUTRAL",13);
            pose.alignment=TextAnchor.MiddleLeft;
            pose.color=new Color(accent.r,accent.g,accent.b,1f);
            var hiddenBody=Image(name+" Pose Driver Body",card,Vector2.zero,Vector2.zero,Color.clear);
            var hiddenLeft=Rect(name+" Pose Driver Left Arm",hiddenBody.transform,Vector2.zero,Vector2.zero);
            var hiddenRight=Rect(name+" Pose Driver Right Arm",hiddenBody.transform,Vector2.zero,Vector2.zero);
            return new VisualSet(hiddenBody.rectTransform,hiddenLeft,hiddenRight,hiddenBody,pose);
        }

        private static void CreateCamera()
        {
            if (Camera.main) return;
            var go=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener)); go.tag="MainCamera"; go.transform.position=new Vector3(0,0,-10);
            var camera=go.GetComponent<Camera>(); camera.orthographic=true; camera.orthographicSize=5; camera.backgroundColor=SurexsVisualTheme.Background;
        }
        private static Transform CreateCanvas()
        {
            var go=new GameObject("Prototype Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            go.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=go.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1920,1080);
            scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight; scaler.matchWidthOrHeight=.5f;
            return go.transform;
        }
        private static RectTransform Rect(string name,Transform parent,Vector2 pos,Vector2 size)
        { var go=new GameObject(name,typeof(RectTransform)); var r=go.GetComponent<RectTransform>(); r.SetParent(parent,false); r.anchoredPosition=pos; r.sizeDelta=size; return r; }
        private static RectTransform MaskedTileRoot(string name,Transform parent,Vector2 pos,Vector2 size)
        { var root=Rect(name,parent,pos,size); root.gameObject.AddComponent<RectMask2D>(); return root; }
        private static Image Image(string name,Transform parent,Vector2 pos,Vector2 size,Color color)
        { var go=new GameObject(name,typeof(RectTransform),typeof(Image)); var r=go.GetComponent<RectTransform>(); r.SetParent(parent,false); r.anchoredPosition=pos; r.sizeDelta=size; var i=go.GetComponent<Image>(); i.color=color; i.raycastTarget=false; return i; }
        private static RawImage Raw(string name,Transform parent,Vector2 pos,Vector2 size,Texture texture)
        { var go=new GameObject(name,typeof(RectTransform),typeof(RawImage)); var r=go.GetComponent<RectTransform>(); r.SetParent(parent,false); r.anchoredPosition=pos; r.sizeDelta=size; var image=go.GetComponent<RawImage>(); image.texture=texture; image.color=Color.white; image.raycastTarget=false; return image; }
        private static void Stretch(RectTransform rect)
        { rect.anchorMin=Vector2.zero; rect.anchorMax=Vector2.one; rect.offsetMin=Vector2.zero; rect.offsetMax=Vector2.zero; }
        private static Text Text(string name,Transform parent,Vector2 pos,Vector2 size,string value,int fontSize)
        { var go=new GameObject(name,typeof(RectTransform),typeof(Text)); var r=go.GetComponent<RectTransform>(); r.SetParent(parent,false); r.anchoredPosition=pos; r.sizeDelta=size; var t=go.GetComponent<Text>(); t.font=SurexsVisualTheme.Font; t.fontSize=fontSize; t.fontStyle=FontStyle.Bold; t.alignment=TextAnchor.MiddleCenter; t.color=SurexsVisualTheme.TextPrimary; t.text=value; t.raycastTarget=false; return t; }
        private static Button Button(string name,Transform parent,Vector2 pos,Vector2 size,string label,Color color)
        { var image=Image(name,parent,pos,size,color); image.raycastTarget=true; var outline=image.gameObject.AddComponent<Outline>(); outline.effectColor=new Color(1,1,1,.25f); outline.effectDistance=new Vector2(2,-2); var button=image.gameObject.AddComponent<Button>(); button.targetGraphic=image; button.transition=Selectable.Transition.ColorTint; SurexsVisualTheme.StyleButton(button,color); Text("Label",image.transform,Vector2.zero,size,label,20); return button; }

        private readonly struct SharedSet { public SharedSet(GameObject r,AudioManager a,ChartManager c,RhythmPrototypeController p){Root=r;Audio=a;Chart=c;Controller=p;} public GameObject Root{get;} public AudioManager Audio{get;} public ChartManager Chart{get;} public RhythmPrototypeController Controller{get;} }
        private readonly struct PlayerSet { public PlayerSet(RhythmJudge j,TileSpawner t,ScoreManager s,PlayerSession session){Judge=j;Tiles=t;Score=s;Session=session;} public RhythmJudge Judge{get;} public TileSpawner Tiles{get;} public ScoreManager Score{get;} public PlayerSession Session{get;} }
        private readonly struct VisualSet { public VisualSet(RectTransform b,RectTransform l,RectTransform r,Image i,Text p){Body=b;LeftArm=l;RightArm=r;BodyImage=i;PoseLabel=p;} public RectTransform Body{get;} public RectTransform LeftArm{get;} public RectTransform RightArm{get;} public Image BodyImage{get;} public Text PoseLabel{get;} }
        private enum PlayerVisualLayout { Solo, VersusLeft, VersusRight }
    }
}
