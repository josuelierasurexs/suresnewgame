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
        [Header("Typography")]
        [SerializeField] private Font titleFont;
        [SerializeField] private Font bodyFont;
        [SerializeField] private Font hitPhraseFont;
        [Header("Input Sources")]
        [SerializeField] private RhythmInputSourceType player1InputSource = RhythmInputSourceType.Gamepad;
        [SerializeField] private RhythmInputSourceType player2InputSource = RhythmInputSourceType.Gamepad;
        [SerializeField, Min(0)] private int player1GamepadIndex;
        [SerializeField, Min(0)] private int player2GamepadIndex = 1;
        [SerializeField, Range(0.1f, 0.95f)] private float player1GamepadAxisThreshold = 0.5f;
        [SerializeField, Range(0.1f, 0.95f)] private float player2GamepadAxisThreshold = 0.5f;
        [SerializeField] private string player1GamepadDevice = "XInputControllerWindows";
        [SerializeField] private string player2GamepadDevice = "XInputControllerWindows1";
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
        [SerializeField] private Texture leftControlTexture;
        [SerializeField] private Texture centerControlTexture;
        [SerializeField] private Texture rightControlTexture;
        [SerializeField] private Sprite surexsLogo;
        [Header("Versus Gameplay Graphics")]
        [SerializeField] private Texture background2Player;
        [SerializeField] private Texture player2Texture;
        [SerializeField] private Texture brokerHeroLogoTexture;
        [Header("Results Graphics")]
        [SerializeField] private Texture resultsCharacterTexture;
        [SerializeField] private Texture resultsContainerTexture;
        [SerializeField] private Texture resultsRetryButtonTexture;
        [SerializeField] private Texture resultsMenuButtonTexture;
        [SerializeField] private Texture resultsSurexsLogoTexture;
        [SerializeField] private Texture resultsQrTexture;
        [SerializeField, Min(0.05f)] private float poseDuration = 0.35f;
        [SerializeField] private PoseData[] poseDefinitions =
        {
            new PoseData("neutral", "Neutral", new Color(.22f,.67f,.92f), 15f,-15f),
            new PoseData("phone", "Phone", new Color(.26f,.80f,.65f), 65f,-10f),
            new PoseData("laptop", "Laptop", new Color(.48f,.58f,.95f),-55f,55f),
            new PoseData("tablet", "Tablet", new Color(.95f,.48f,.38f),125f,-25f),
            new PoseData("combo", "Combo", new Color(.98f,.78f,.20f),145f,-145f),
            new PoseData("miss", "Miss", new Color(1f,.25f,.38f),15f,-15f)
        };

        private RawImage soloPlayerArtwork;
        private RawImage versusPlayer1Artwork;
        private RawImage versusPlayer2Artwork;
        private RawImage versusPlayer1Glow;
        private RawImage versusPlayer2Glow;

        private void Awake()
        {
            SurexsVisualTheme.ConfigureFonts(titleFont,bodyFont);
            if (!gameplayConfig.IsValid(out var error)) { Debug.LogError(error, this); return; }
            CreateCamera();
            CreateEventSystem();
            var canvas = CreateCanvas();
#if UNITY_EDITOR
            if (ChartRecorderAccess.ConsumeLaunchRequest())
            {
                CreateSoloBackdrop(canvas);
                CreateChartRecorder(canvas);
                return;
            }
#endif
            if (gameMode == GameMode.Solo) CreateSoloBackdrop(canvas);
            else CreateVersusBackdrop(canvas);
            var shared = Shared();
            PlayerSession[] sessions;

            if (gameMode == GameMode.LocalVersus)
            {
                var p1TileRoot=MaskedTileRoot("Player 1 Runtime Tiles",canvas,new Vector2(-550f,-155f),new Vector2(630f,520f));
                var p2TileRoot=MaskedTileRoot("Player 2 Runtime Tiles",canvas,new Vector2(550f,-155f),new Vector2(630f,520f));
                var p1 = Player(shared, p1TileRoot, canvas, "PLAYER 1", -550, new[] {-760f,-550f,-340f}, Key.A, Key.W, Key.S, Key.D, new Color(.22f,.67f,.92f),player1InputSource,player1GamepadIndex,player1GamepadAxisThreshold,player1GamepadDevice,player1Joystick,PlayerVisualLayout.VersusLeft);
                var p2 = Player(shared, p2TileRoot, canvas, "PLAYER 2",  550, new[] { 340f, 550f, 760f}, Key.LeftArrow, Key.UpArrow, Key.DownArrow, Key.RightArrow, new Color(.95f,.45f,.55f),player2InputSource,player2GamepadIndex,player2GamepadAxisThreshold,player2GamepadDevice,player2Joystick,PlayerVisualLayout.VersusRight);
                shared.Controller.Configure(shared.Audio, shared.Chart, new[] {p1.Tiles,p2.Tiles}, new[] {p1.Judge,p2.Judge}, gameplayConfig);
                shared.Root.AddComponent<LocalVersusMatchState>().Configure(p1.Score, p2.Score);
                sessions = new[] { p1.Session, p2.Session };
            }
            else
            {
                var tileRoot=MaskedTileRoot("Runtime Tiles",canvas,new Vector2(-380f,0f),new Vector2(900f,720f));
                var p1 = Player(shared, tileRoot, canvas, "PLAYER 1", 520, new[] {-670f,-380f,-90f}, Key.A, Key.W, Key.S, Key.D, new Color(.22f,.67f,.92f),player1InputSource,player1GamepadIndex,player1GamepadAxisThreshold,player1GamepadDevice,player1Joystick,PlayerVisualLayout.Solo);
                shared.Controller.Configure(shared.Audio, shared.Chart, p1.Tiles, p1.Judge, gameplayConfig);
                sessions = new[] { p1.Session };
            }
            var results = CreateResultsView(canvas);
            var flow=shared.Root.AddComponent<GameFlowController>();
            flow.Configure(gameMode,shared.Controller,sessions,results,false);
            var navigation=shared.Root.AddComponent<GameplayNavigationController>();
            navigation.Configure(flow,shared.Controller,results);
            CreatePauseMenu(canvas,flow,shared.Controller,navigation);
            if (song == null) Debug.LogWarning("[Bootstrap] No hay canción asignada.", this);
            if (chart == null) Debug.LogWarning("[Bootstrap] No hay chart asignado.", this);
            if (leftControlTexture == null) Debug.LogWarning("[Bootstrap] Falta left_control.png.",this);
            if (centerControlTexture == null) Debug.LogWarning("[Bootstrap] Falta center_control.png.",this);
            if (rightControlTexture == null) Debug.LogWarning("[Bootstrap] Falta right_control.png.",this);
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

            soloPlayerArtwork=Raw("Solo Player Artwork",canvas,new Vector2(470,-35),new Vector2(620,615),player1Texture);
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
            versusPlayer1Glow=CharacterGlow("Player 1 Character Glow",canvas,new Vector2(-550,245),new Vector2(358,353),new Color(.05f,.66f,1f,.72f));
            versusPlayer2Glow=CharacterGlow("Player 2 Character Glow",canvas,new Vector2(550,245),new Vector2(358,353),new Color(1f,.16f,.34f,.72f));
            for (var i=0;i<3;i++)
            {
                NeonBackdrop("Player 1 Tile Glow "+i,canvas,new Vector2(p1Lanes[i],-155f),new Vector2(202,532),new Color(.05f,.66f,1f,1f));
                NeonBackdrop("Player 2 Tile Glow "+i,canvas,new Vector2(p2Lanes[i],-155f),new Vector2(202,532),new Color(1f,.16f,.34f,1f));
            }
            for (var i=0;i<3;i++)
            {
                Raw("Player 1 Tile Container "+i,canvas,new Vector2(p1Lanes[i],-155f),new Vector2(190,520),tileContainerTexture);
                Raw("Player 2 Tile Container "+i,canvas,new Vector2(p2Lanes[i],-155f),new Vector2(190,520),tileContainerTexture);
            }

            versusPlayer1Artwork=Raw("Player 1 Artwork",canvas,new Vector2(-550,245),new Vector2(340,335),player1Texture);
            versusPlayer2Artwork=Raw("Player 2 Artwork",canvas,new Vector2(550,245),new Vector2(340,335),player2Texture);

            var brandPanel=Image("Central Brand Backdrop",canvas,new Vector2(0,95),new Vector2(310,180),new Color(.01f,.06f,.14f,.95f));
            SurexsVisualTheme.ApplyRounded(brandPanel);
            Raw("Broker Hero Logo",brandPanel.transform,Vector2.zero,new Vector2(290,145),brokerHeroLogoTexture);

            if (resultsSurexsLogoTexture != null)
            {
                var aspect=resultsSurexsLogoTexture.height>0
                    ? (float)resultsSurexsLogoTexture.width/resultsSurexsLogoTexture.height
                    : 3.7f;
                var logo=Raw("Versus Surexs Logo",canvas,new Vector2(0,-480),new Vector2(230,230/aspect),resultsSurexsLogoTexture);
                var fitter=logo.gameObject.AddComponent<AspectRatioFitter>();
                fitter.aspectMode=AspectRatioFitter.AspectMode.WidthControlsHeight;
                fitter.aspectRatio=aspect;
            }
            else
            {
                Debug.LogWarning("[Bootstrap] Falta surexs-logo.png para el logo inferior de Versus.",this);
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
            WarnMissingVersusAsset(resultsSurexsLogoTexture,nameof(resultsSurexsLogoTexture));
        }

        private void WarnMissingVersusAsset(Object asset,string field)
        {
            if (asset == null) Debug.LogWarning($"[Bootstrap] Falta el asset visual Versus '{field}'.",this);
        }

        private void WarnMissingSoloAsset(Object asset,string field)
        {
            if (asset == null) Debug.LogWarning($"[Bootstrap] Falta el asset visual Solo '{field}'.",this);
        }

#if UNITY_EDITOR
        private void CreateChartRecorder(Transform canvas)
        {
            const float tileRootX = -380f;
            const float hitZoneY = -285f;
            var lanes = new[] {-670f, -380f, -90f};
            var localLanes = new[] {lanes[0] - tileRootX, lanes[1] - tileRootX, lanes[2] - tileRootX};
            var labels = new[] {"LEFT ←", "CENTER ●", "RIGHT →"};
            var keys = new[] {"A", "W / S", "D"};
            var hitZones = new Image[3];
            var tileRoot = MaskedTileRoot("Chart Recorder Note Preview", canvas,
                new Vector2(tileRootX, 0f), new Vector2(900f, 720f));

            for (var index = 0; index < lanes.Length; index++)
            {
                hitZones[index] = Image("Recorder Hit " + index, canvas,
                    new Vector2(lanes[index], hitZoneY), new Vector2(230, 72),
                    new Color(.12f, .86f, .48f, .72f));
                SurexsVisualTheme.ApplyRounded(hitZones[index]);
                var hitOutline = hitZones[index].gameObject.AddComponent<Outline>();
                hitOutline.effectColor = new Color(.45f, 1f, .72f, .9f);
                hitOutline.effectDistance = new Vector2(3f, -3f);

                var laneLabel = Text("Recorder Lane Label " + index, canvas,
                    new Vector2(lanes[index], 350f), new Vector2(210, 34), labels[index], 22);
                laneLabel.font = SurexsVisualTheme.TitleFont;

                var keyPanel = Image("Recorder Key " + index, canvas,
                    new Vector2(lanes[index], -420f), new Vector2(112, 46),
                    new Color(.02f, .18f, .34f, .94f));
                SurexsVisualTheme.ApplyRounded(keyPanel);
                var keyOutline = keyPanel.gameObject.AddComponent<Outline>();
                keyOutline.effectColor = new Color(.22f, .67f, .92f, .95f);
                keyOutline.effectDistance = new Vector2(2f, -2f);
                var keyLabel = Text("Recorder Key Label " + index, keyPanel.transform,
                    Vector2.zero, new Vector2(112, 46), keys[index], 20);
                keyLabel.font = SurexsVisualTheme.BodyFont;
            }

            var panel = Image("Chart Recorder Panel", canvas, new Vector2(520, 0),
                new Vector2(650, 890), new Color(.01f, .08f, .18f, .96f));
            SurexsVisualTheme.ApplyRounded(panel);
            var panelOutline = panel.gameObject.AddComponent<Outline>();
            panelOutline.effectColor = new Color(.08f, .70f, 1f, .9f);
            panelOutline.effectDistance = new Vector2(4f, -4f);

            var title = Text("Recorder Title", panel.transform, new Vector2(0, 395),
                new Vector2(570, 54), "EDITOR DE CHART", 34);
            title.font = SurexsVisualTheme.TitleFont;
            var recorderInputLabel = InputLabel("P1", player1InputSource, player1GamepadIndex, player1Joystick);
            var instructions = Text("Recorder Instructions", panel.transform, new Vector2(0, 350),
                new Vector2(570, 42), recorderInputLabel, 18);
            instructions.font = SurexsVisualTheme.BodyFont;
            instructions.color = new Color(.36f, .78f, 1f);

            var time = Text("Recorder Time", panel.transform, new Vector2(0, 292),
                new Vector2(570, 50), "SONG TIME  00:00.000", 29);
            time.font = SurexsVisualTheme.BodyFont;
            var count = Text("Recorder Count", panel.transform, new Vector2(0, 246),
                new Vector2(570, 38), "NOTAS REGISTRADAS  0", 20);
            count.font = SurexsVisualTheme.BodyFont;

            var poseButton = Button("Recorder Pose", panel.transform, new Vector2(0, 188),
                new Vector2(360, 48), "POSE: PHONE", new Color(.18f, .46f, .78f));
            var poseText = poseButton.GetComponentInChildren<Text>();
            poseText.font = SurexsVisualTheme.BodyFont;

            var defaultChartName = song != null ? song.name + "_recorded" : "surexs_recorded_chart";
            var fileName = Input("Recorder File Name", panel.transform, new Vector2(0, 126),
                new Vector2(500, 48), defaultChartName, "Nombre del JSON");

            var playPause = Button("Recorder Play Pause", panel.transform, new Vector2(-142, 58),
                new Vector2(260, 50), "PAUSA", SurexsVisualTheme.Success);
            var restart = Button("Recorder Restart", panel.transform, new Vector2(142, 58),
                new Vector2(260, 50), "REINICIAR AUDIO", SurexsVisualTheme.Primary);
            var undo = Button("Recorder Undo", panel.transform, new Vector2(-190, -8),
                new Vector2(170, 48), "DESHACER", new Color(.22f, .32f, .50f));
            var nudgeBack = Button("Recorder Nudge Back", panel.transform, new Vector2(0, -8),
                new Vector2(170, 48), "ÚLTIMA -10ms", new Color(.22f, .32f, .50f));
            var nudgeForward = Button("Recorder Nudge Forward", panel.transform, new Vector2(190, -8),
                new Vector2(170, 48), "ÚLTIMA +10ms", new Color(.22f, .32f, .50f));

            var offset = Text("Recorder Offset", panel.transform, new Vector2(0, -65),
                new Vector2(320, 36), "OFFSET GLOBAL  0 ms", 18);
            offset.font = SurexsVisualTheme.BodyFont;
            var offsetBack = Button("Recorder Offset Back", panel.transform, new Vector2(-115, -112),
                new Vector2(190, 44), "OFFSET -10ms", new Color(.18f, .36f, .58f));
            var offsetForward = Button("Recorder Offset Forward", panel.transform, new Vector2(115, -112),
                new Vector2(190, 44), "OFFSET +10ms", new Color(.18f, .36f, .58f));

            var recent = Text("Recorder Recent Notes", panel.transform, new Vector2(0, -212),
                new Vector2(560, 135), "AÚN NO HAY NOTAS", 17);
            recent.font = SurexsVisualTheme.BodyFont;
            recent.alignment = TextAnchor.UpperLeft;
            recent.horizontalOverflow = HorizontalWrapMode.Overflow;

            var save = Button("Recorder Save", panel.transform, new Vector2(0, -322),
                new Vector2(430, 60), "GUARDAR JSON NUEVO", SurexsVisualTheme.Accent);
            var status = Text("Recorder Status", panel.transform, new Vector2(0, -392),
                new Vector2(570, 64), "Preparando grabación...", 16);
            status.font = SurexsVisualTheme.BodyFont;

            var recorderRoot = new GameObject("Chart Recorder Systems");
            var source = recorderRoot.AddComponent<AudioSource>();
            source.clip = song;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            var audio = recorderRoot.AddComponent<AudioManager>();
            audio.Configure(source);
            var input = CreateInputSource(recorderRoot, player1InputSource, player1GamepadIndex,
                player1GamepadAxisThreshold, player1GamepadDevice, player1Joystick,
                Key.A, Key.W, Key.S, Key.D);
            recorderRoot.AddComponent<ControlInputFeedbackView>()
                .Configure(input, hitZones[0], hitZones[1], hitZones[2]);
            var recorder = recorderRoot.AddComponent<ChartRecorderController>();
            recorder.Configure(audio, source, input, tileRoot, localLanes, hitZoneY,
                time, count, poseText, offset, recent, status, fileName,
                playPause.GetComponentInChildren<Text>());

            poseButton.onClick.AddListener(recorder.SelectNextPose);
            playPause.onClick.AddListener(recorder.TogglePlayback);
            restart.onClick.AddListener(recorder.RestartPlayback);
            undo.onClick.AddListener(recorder.UndoLastNote);
            nudgeBack.onClick.AddListener(() => recorder.NudgeLastNote(-10));
            nudgeForward.onClick.AddListener(() => recorder.NudgeLastNote(10));
            offsetBack.onClick.AddListener(() => recorder.AdjustGlobalOffset(-10));
            offsetForward.onClick.AddListener(() => recorder.AdjustGlobalOffset(10));
            save.onClick.AddListener(recorder.SaveChart);

            if (song == null)
                Debug.LogWarning("[ChartRecorder] No hay AudioClip asignado en Phase One Bootstrap.", this);
        }
#endif

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
            string gamepadDevice, JoystickInputConfig joystickConfig, PlayerVisualLayout layout)
        {
            var soloRedesign=layout==PlayerVisualLayout.Solo;
            var labels = new[] {"LEFT ←","CENTER ●","RIGHT →"};
            var hitZones = new Image[3];
            var hitY=soloRedesign ? -285f : -355f;
            for (var i=0;i<3;i++)
            {
                var hitSize=soloRedesign ? new Vector2(230,72) : new Vector2(170,60);
                hitZones[i]=Image(name+" Hit "+i, canvas, new Vector2(lanes[i],hitY), hitSize, new Color(.12f,.86f,.48f,.72f));
                SurexsVisualTheme.ApplyRounded(hitZones[i]);
                var hitOutline=hitZones[i].gameObject.AddComponent<Outline>(); hitOutline.effectColor=new Color(.45f,1f,.72f,.9f); hitOutline.effectDistance=new Vector2(3,-3);
                var laneLabel=Text(name+" Label "+i, canvas, new Vector2(lanes[i],soloRedesign ? 350f : 125f), new Vector2(210,34), labels[i], soloRedesign ? 22 : 16);
                laneLabel.font=SurexsVisualTheme.TitleFont;
                var keyY=soloRedesign ? -420f : -450f;
                var controlTexture=i==0 ? leftControlTexture : i==1 ? centerControlTexture : rightControlTexture;
                var iconHeight=soloRedesign ? 109f : 89f;
                var iconAspect=controlTexture != null && controlTexture.height>0
                    ? (float)controlTexture.width/controlTexture.height
                    : 720f/353f;
                var iconSize=new Vector2(iconHeight*iconAspect,iconHeight);
                var controlIcon=Raw(name+" Control Icon "+i,canvas,new Vector2(lanes[i],keyY),iconSize,controlTexture);
                controlIcon.raycastTarget=false;
                var aspectFitter=controlIcon.gameObject.AddComponent<AspectRatioFitter>();
                aspectFitter.aspectMode=AspectRatioFitter.AspectMode.HeightControlsWidth;
                aspectFitter.aspectRatio=iconAspect;
                var iconGlow=controlIcon.gameObject.AddComponent<Outline>();
                iconGlow.effectColor=new Color(accent.r,accent.g,accent.b,.82f);
                iconGlow.effectDistance=new Vector2(2,-2);
            }
            RawImage judgmentImage=null; Text points=null;
            Text scoreLabel=null; Text comboLabel=null; Text songLabel=null; Text statsLabel=null; Image progressFill=null;
            JudgmentStarBurstView starBurst=null;
            HitPhraseFeedbackView hitPhrase=null;
            var milestone=Text(name+" Milestone",canvas,new Vector2(x,soloRedesign ? 300f : 145f),new Vector2(500,50),"",soloRedesign ? 27 : 22);
            VisualSet visual;
            if (soloRedesign)
            {
                scoreLabel=Text("Solo Score",canvas,new Vector2(750,330),new Vector2(300,170),"",27);
                comboLabel=Text("Solo Combo",canvas,new Vector2(750,155),new Vector2(340,220),"",30);
                scoreLabel.font=SurexsVisualTheme.BodyFont;
                comboLabel.font=SurexsVisualTheme.BodyFont;
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
                var phraseLabel=Text(name+" Hit Phrase",canvas,new Vector2(470,225),new Vector2(650,72),"",32);
                phraseLabel.font=hitPhraseFont!=null ? hitPhraseFont : SurexsVisualTheme.BodyFont;
                hitPhrase=phraseLabel.gameObject.AddComponent<HitPhraseFeedbackView>();
                hitPhrase.Configure(phraseLabel,false);
                visual=VisualSolo(canvas,name);
            }
            else
            {
                var card=Image(name+" Card",canvas,new Vector2(x,458),new Vector2(630,125),new Color(.01f,.13f,.28f,.95f));
                SurexsVisualTheme.ApplyRounded(card);
                scoreLabel=Text(name+" Score",card.transform,new Vector2(-75,-17),new Vector2(230,90),"",19);
                comboLabel=Text(name+" Combo",card.transform,new Vector2(185,-17),new Vector2(220,100),"",20);
                scoreLabel.font=SurexsVisualTheme.BodyFont;
                comboLabel.font=SurexsVisualTheme.BodyFont;
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
                var phraseY=layout==PlayerVisualLayout.VersusLeft ? -95f : -178f;
                var phraseLabel=Text(name+" Hit Phrase",canvas,new Vector2(0,phraseY),new Vector2(410,50),"",19);
                phraseLabel.font=hitPhraseFont!=null ? hitPhraseFont : SurexsVisualTheme.BodyFont;
                hitPhrase=phraseLabel.gameObject.AddComponent<HitPhraseFeedbackView>();
                hitPhrase.Configure(phraseLabel,true);
            }
            var go=new GameObject(name+" Systems");
            var input=CreateInputSource(go,inputSourceType,gamepadIndex,gamepadAxisThreshold,gamepadDevice,joystickConfig,left,centerPrimary,centerSecondary,right);
            go.AddComponent<ControlInputFeedbackView>().Configure(input,hitZones[0],hitZones[1],hitZones[2]);
            var judge=go.AddComponent<RhythmJudge>(); judge.Configure(shared.Audio,input,gameplayConfig);
            var combo=go.AddComponent<ComboManager>(); combo.Configure(gameplayConfig);
            var score=go.AddComponent<ScoreManager>(); score.Configure(gameplayConfig,combo);
            var feedback=go.AddComponent<GameplayFeedbackView>();
            feedback.Configure(judgmentImage,points,milestone,perfectTexture,greatTexture,goodTexture,missTexture,starBurst,hitPhrase);
            go.AddComponent<JudgmentProcessor>().Configure(judge,score,combo,feedback);
            var poses=go.AddComponent<PoseController>(); poses.Configure(poseDefinitions,visual.Body,visual.LeftArm,visual.RightArm,visual.BodyImage,visual.PoseLabel);
            SoloCharacterAnimationView characterAnimation=null;
            RawImage characterArtwork=null;
            RawImage characterGlow=null;
            var animationResources="Animations/Player1";
            if (soloRedesign) characterArtwork=soloPlayerArtwork;
            else if (layout==PlayerVisualLayout.VersusLeft)
            {
                characterArtwork=versusPlayer1Artwork;
                characterGlow=versusPlayer1Glow;
            }
            else if (layout==PlayerVisualLayout.VersusRight)
            {
                characterArtwork=versusPlayer2Artwork;
                characterGlow=versusPlayer2Glow;
                animationResources="Animations/Player2";
            }
            if (characterArtwork != null)
            {
                characterAnimation=go.AddComponent<SoloCharacterAnimationView>();
                characterAnimation.Configure(characterArtwork,animationResources,characterGlow);
            }
            var player=go.AddComponent<PlayerController>(); player.Configure(judge,poses,poseDuration,combo,characterAnimation);
            var tileRootOffset=tileRoot.anchoredPosition;
            var tiles=go.AddComponent<TileSpawner>(); tiles.Configure(shared.Audio,shared.Chart,tileRoot,judge,
                lanes[0]-tileRootOffset.x,lanes[1]-tileRootOffset.x,lanes[2]-tileRootOffset.x,hitY-tileRootOffset.y);
            var statusView=go.AddComponent<PrototypeStatusView>();
            if (soloRedesign) statusView.ConfigureSolo(shared.Audio,shared.Chart,tiles,score,combo,scoreLabel,comboLabel,songLabel,statsLabel,progressFill);
            else statusView.ConfigureVersus(shared.Audio,shared.Chart,tiles,score,combo,scoreLabel,comboLabel,statsLabel);
            return new PlayerSet(judge,tiles,score,new PlayerSession(judge,tiles,score,combo,feedback,player));
        }

        private static IRhythmInputSource CreateInputSource(GameObject owner, RhythmInputSourceType sourceType,
            int gamepadIndex, float gamepadAxisThreshold, string gamepadDevice, JoystickInputConfig joystickConfig,
            Key left, Key centerPrimary, Key centerSecondary, Key right)
        {
            if (sourceType == RhythmInputSourceType.Gamepad)
            {
                var gamepad=owner.AddComponent<GamepadInputReader>();
                gamepad.Configure(gamepadIndex,gamepadAxisThreshold,gamepadDevice);
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

        private ResultsView CreateResultsView(Transform canvas)
        {
            var panel=Raw("Results Panel",canvas,Vector2.zero,new Vector2(1920,1080),background2Player);
            Stretch(panel.rectTransform);
            var summaryPage=Rect("Results Summary Page",panel.transform,Vector2.zero,new Vector2(1920,1080));
            Stretch(summaryPage);
            Raw("Results Broker Hero Logo",summaryPage,new Vector2(-745,405),new Vector2(390,195),brokerHeroLogoTexture);
            Raw("Results Character",summaryPage,new Vector2(-720,-120),new Vector2(400,695),resultsCharacterTexture);
            Raw("Results Surexs Logo",summaryPage,new Vector2(785,-475),new Vector2(250,57),resultsSurexsLogoTexture);

            var p1Card=Raw("P1 Results Card",summaryPage,new Vector2(125,35),new Vector2(650,725),resultsContainerTexture);
            var p2Card=Raw("P2 Results Card",summaryPage,new Vector2(420,35),new Vector2(560,625),resultsContainerTexture);
            var title=Text("Results Title",summaryPage,new Vector2(125,325),new Vector2(520,82),"RESULTADOS",48);
            var p1=Text("P1 Results",p1Card.transform,new Vector2(0,-85),new Vector2(540,520),"",30);
            var p2=Text("P2 Results",p2Card.transform,new Vector2(0,-58),new Vector2(470,450),"",26);
            var p1Header=Text("P1 Results Header",p1Card.transform,new Vector2(0,220),new Vector2(460,48),"PLAYER 1",30);
            var p2Header=Text("P2 Results Header",p2Card.transform,new Vector2(0,255),new Vector2(470,42),"PLAYER 2",23);
            var next=TextureButton("Next",summaryPage,new Vector2(125,-435),new Vector2(430,120),"SIGUIENTE",resultsRetryButtonTexture);
            next.navigation=new Navigation
            {
                mode=Navigation.Mode.Explicit,
                selectOnLeft=next,
                selectOnRight=next,
                selectOnUp=next,
                selectOnDown=next
            };

            var socialPage=Rect("Results Social Page",panel.transform,Vector2.zero,new Vector2(1920,1080));
            Stretch(socialPage);
            var socialCard=Raw("Social Results Card",socialPage,new Vector2(0,45),new Vector2(900,900),resultsContainerTexture);
            var socialTitle=Text("Social Results Title",socialPage,new Vector2(0,420),new Vector2(820,86),"SÍGUENOS EN REDES",46);
            var qr=Raw("Social QR",socialCard.transform,new Vector2(0,-25),new Vector2(680,601),resultsQrTexture);
            qr.raycastTarget=false;
            var retry=TextureButton("Rematch",socialPage,new Vector2(-215,-445),new Vector2(390,120),"REVANCHA",resultsRetryButtonTexture);
            var menu=TextureButton("Menu",socialPage,new Vector2(215,-445),new Vector2(390,120),"MENU",resultsMenuButtonTexture);
            retry.navigation=new Navigation
            {
                mode=Navigation.Mode.Explicit,
                selectOnLeft=menu,
                selectOnRight=menu,
                selectOnUp=menu,
                selectOnDown=menu
            };
            menu.navigation=new Navigation
            {
                mode=Navigation.Mode.Explicit,
                selectOnLeft=retry,
                selectOnRight=retry,
                selectOnUp=retry,
                selectOnDown=retry
            };
            title.font=SurexsVisualTheme.TitleFont;
            p1.font=SurexsVisualTheme.BodyFont;
            p2.font=SurexsVisualTheme.BodyFont;
            p1Header.font=SurexsVisualTheme.TitleFont;
            p2Header.font=SurexsVisualTheme.TitleFont;
            socialTitle.font=SurexsVisualTheme.TitleFont;
            next.GetComponentInChildren<Text>().font=SurexsVisualTheme.BodyFont;
            retry.GetComponentInChildren<Text>().font=SurexsVisualTheme.BodyFont;
            menu.GetComponentInChildren<Text>().font=SurexsVisualTheme.BodyFont;
            socialPage.gameObject.SetActive(false);
            var view=panel.gameObject.AddComponent<ResultsView>();
            view.Configure(panel.gameObject,title,p1,p2,p1Header,p2Header,p1Card.gameObject,p2Card.gameObject,
                summaryPage.gameObject,socialPage.gameObject,next,retry,menu);
            WarnMissingResultsAsset(background2Player,nameof(background2Player));
            WarnMissingResultsAsset(brokerHeroLogoTexture,nameof(brokerHeroLogoTexture));
            WarnMissingResultsAsset(resultsCharacterTexture,nameof(resultsCharacterTexture));
            WarnMissingResultsAsset(resultsContainerTexture,nameof(resultsContainerTexture));
            WarnMissingResultsAsset(resultsRetryButtonTexture,nameof(resultsRetryButtonTexture));
            WarnMissingResultsAsset(resultsMenuButtonTexture,nameof(resultsMenuButtonTexture));
            WarnMissingResultsAsset(resultsSurexsLogoTexture,nameof(resultsSurexsLogoTexture));
            WarnMissingResultsAsset(resultsQrTexture,nameof(resultsQrTexture));
            return view;
        }

        private void CreatePauseMenu(Transform canvas,GameFlowController flow,RhythmPrototypeController gameplay,
            GameplayNavigationController navigation)
        {
            var overlay=Image("Pause Overlay",canvas,Vector2.zero,new Vector2(1920,1080),new Color(0f,.015f,.055f,.78f));
            Stretch(overlay.rectTransform);
            overlay.raycastTarget=true;
            var card=Raw("Pause Container",overlay.transform,Vector2.zero,new Vector2(720,780),resultsContainerTexture);
            var title=Text("Pause Title",overlay.transform,new Vector2(0,315),new Vector2(600,90),"PAUSA",52);
            title.font=SurexsVisualTheme.TitleFont;

            var resume=TextureButton("Pause Resume",card.transform,new Vector2(0,105),new Vector2(430,112),
                "REANUDAR",resultsRetryButtonTexture);
            var restart=TextureButton("Pause Restart",card.transform,new Vector2(0,-40),new Vector2(430,112),
                "REINICIAR",resultsMenuButtonTexture);
            var exit=TextureButton("Pause Exit",card.transform,new Vector2(0,-185),new Vector2(430,112),
                "SALIR",resultsMenuButtonTexture);
            resume.navigation=new Navigation
            {
                mode=Navigation.Mode.Explicit,selectOnUp=exit,selectOnDown=restart,
                selectOnLeft=resume,selectOnRight=resume
            };
            restart.navigation=new Navigation
            {
                mode=Navigation.Mode.Explicit,selectOnUp=resume,selectOnDown=exit,
                selectOnLeft=restart,selectOnRight=restart
            };
            exit.navigation=new Navigation
            {
                mode=Navigation.Mode.Explicit,selectOnUp=restart,selectOnDown=resume,
                selectOnLeft=exit,selectOnRight=exit
            };
            var sequenceOverlay=Image("Gameplay Start Overlay",canvas,Vector2.zero,new Vector2(1920,1080),
                new Color(0f,.015f,.055f,.78f));
            Stretch(sequenceOverlay.rectTransform);
            sequenceOverlay.raycastTarget=true;
            var brandContent=Rect("Gameplay Start Branding",sequenceOverlay.transform,Vector2.zero,new Vector2(1100,420));
            var brandGroup=brandContent.gameObject.AddComponent<CanvasGroup>();
            var logoAspect=resultsSurexsLogoTexture!=null && resultsSurexsLogoTexture.height>0
                ? (float)resultsSurexsLogoTexture.width/resultsSurexsLogoTexture.height
                : 3.7f;
            var introLogo=Raw("Gameplay Start Surexs Logo",brandContent,new Vector2(0,65),
                new Vector2(570,570/logoAspect),resultsSurexsLogoTexture);
            var logoFitter=introLogo.gameObject.AddComponent<AspectRatioFitter>();
            logoFitter.aspectMode=AspectRatioFitter.AspectMode.WidthControlsHeight;
            logoFitter.aspectRatio=logoAspect;
            var slogan=Text("Gameplay Start Slogan",brandContent,new Vector2(0,-95),new Vector2(1000,70),
                "Broker de Seguros para Empresas",38);
            slogan.font=SurexsVisualTheme.BodyFont;
            slogan.fontStyle=FontStyle.Normal;
            var countdown=Text("Gameplay Countdown",sequenceOverlay.transform,Vector2.zero,new Vector2(500,300),"",180);
            countdown.font=SurexsVisualTheme.TitleFont;
            countdown.fontStyle=FontStyle.Normal;

            gameplay.gameObject.AddComponent<PauseMenuController>().Configure(overlay.gameObject,resume,restart,exit,
                sequenceOverlay.gameObject,brandGroup,countdown,flow,gameplay,navigation);
        }

        private void WarnMissingResultsAsset(Object asset,string field)
        {
            if (asset == null) Debug.LogWarning($"[Bootstrap] Falta el asset visual de resultados '{field}'.",this);
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
            var canvas=go.GetComponent<Canvas>();
            canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect=true;
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
        { var go=new GameObject(name,typeof(RectTransform),typeof(Text)); var r=go.GetComponent<RectTransform>(); r.SetParent(parent,false); r.anchoredPosition=pos; r.sizeDelta=size; var t=go.GetComponent<Text>(); t.font=SurexsVisualTheme.BodyFont; t.fontSize=fontSize; t.fontStyle=FontStyle.Normal; t.alignment=TextAnchor.MiddleCenter; t.color=SurexsVisualTheme.TextPrimary; t.text=value; t.raycastTarget=false; return t; }
        private static Button Button(string name,Transform parent,Vector2 pos,Vector2 size,string label,Color color)
        { var image=Image(name,parent,pos,size,color); image.raycastTarget=true; var outline=image.gameObject.AddComponent<Outline>(); outline.effectColor=new Color(1,1,1,.25f); outline.effectDistance=new Vector2(2,-2); var button=image.gameObject.AddComponent<Button>(); button.targetGraphic=image; button.transition=Selectable.Transition.ColorTint; SurexsVisualTheme.StyleButton(button,color); Text("Label",image.transform,Vector2.zero,size,label,20); return button; }
        private static Button TextureButton(string name,Transform parent,Vector2 pos,Vector2 size,string label,Texture texture)
        { var image=Raw(name,parent,pos,size,texture); image.raycastTarget=true; var button=image.gameObject.AddComponent<Button>(); button.targetGraphic=image; button.transition=Selectable.Transition.ColorTint; SurexsVisualTheme.StyleButton(button,Color.white); var text=Text("Label",image.transform,Vector2.zero,new Vector2(size.x*.76f,size.y*.6f),label,34); text.font=SurexsVisualTheme.BodyFont; return button; }

#if UNITY_EDITOR
        private static InputField Input(string name,Transform parent,Vector2 pos,Vector2 size,string value,string hint)
        {
            var background=Image(name,parent,pos,size,new Color(.025f,.12f,.24f,.98f));
            background.raycastTarget=true;
            SurexsVisualTheme.ApplyRounded(background);
            var input=background.gameObject.AddComponent<InputField>();
            input.targetGraphic=background;

            var text=Text("Text",background.transform,Vector2.zero,new Vector2(size.x-34f,size.y-8f),value,18);
            text.font=SurexsVisualTheme.BodyFont;
            text.alignment=TextAnchor.MiddleLeft;
            text.raycastTarget=true;
            input.textComponent=text;

            var placeholder=Text("Placeholder",background.transform,Vector2.zero,new Vector2(size.x-34f,size.y-8f),hint,18);
            placeholder.font=SurexsVisualTheme.BodyFont;
            placeholder.fontStyle=FontStyle.Italic;
            placeholder.alignment=TextAnchor.MiddleLeft;
            placeholder.color=new Color(1f,1f,1f,.42f);
            input.placeholder=placeholder;
            input.text=value;
            input.characterLimit=80;
            return input;
        }
#endif

        private static void NeonBackdrop(string name,Transform parent,Vector2 pos,Vector2 size,Color neon)
        {
            var scales=new[] {1.10f,1.06f,1.025f};
            var alphas=new[] {.035f,.065f,.12f};
            for (var i=0;i<scales.Length;i++)
            {
                var layer=Image(name+" "+i,parent,pos,size*scales[i],new Color(neon.r,neon.g,neon.b,alphas[i]));
                SurexsVisualTheme.ApplyRounded(layer);
                layer.raycastTarget=false;
            }
        }

        private static RawImage CharacterGlow(string name,Transform parent,Vector2 pos,Vector2 size,Color color)
        {
            var glow=Raw(name,parent,pos,size,null);
            glow.color=color;
            glow.raycastTarget=false;
            var outline=glow.gameObject.AddComponent<Outline>();
            outline.effectColor=new Color(color.r,color.g,color.b,.42f);
            outline.effectDistance=new Vector2(7f,-7f);
            return glow;
        }

        private readonly struct SharedSet { public SharedSet(GameObject r,AudioManager a,ChartManager c,RhythmPrototypeController p){Root=r;Audio=a;Chart=c;Controller=p;} public GameObject Root{get;} public AudioManager Audio{get;} public ChartManager Chart{get;} public RhythmPrototypeController Controller{get;} }
        private readonly struct PlayerSet { public PlayerSet(RhythmJudge j,TileSpawner t,ScoreManager s,PlayerSession session){Judge=j;Tiles=t;Score=s;Session=session;} public RhythmJudge Judge{get;} public TileSpawner Tiles{get;} public ScoreManager Score{get;} public PlayerSession Session{get;} }
        private readonly struct VisualSet { public VisualSet(RectTransform b,RectTransform l,RectTransform r,Image i,Text p){Body=b;LeftArm=l;RightArm=r;BodyImage=i;PoseLabel=p;} public RectTransform Body{get;} public RectTransform LeftArm{get;} public RectTransform RightArm{get;} public Image BodyImage{get;} public Text PoseLabel{get;} }
        private enum PlayerVisualLayout { Solo, VersusLeft, VersusRight }
    }
}
