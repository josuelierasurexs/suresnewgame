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
            Image("Background", canvas, Vector2.zero, new Vector2(1920,1080), SurexsVisualTheme.Background);
            Image("Header Glow",canvas,new Vector2(0,485),new Vector2(1920,110),SurexsVisualTheme.BackgroundGlow);
            Image("Header Accent",canvas,new Vector2(0,438),new Vector2(680,5),SurexsVisualTheme.Accent);
            Text("Title", canvas, new Vector2(0,505), new Vector2(1500,55),
                gameMode == GameMode.LocalVersus ? "SUREXS DANCE OFF  •  1 VS 1 LOCAL" : "SUREXS DANCE OFF  •  SOLO", 34);
            Text("Controls", canvas, new Vector2(0,460), new Vector2(1500,38),
                ControlsText(), 20);
            var tileRoot = Rect("Runtime Tiles", canvas, Vector2.zero, new Vector2(1920,1080));
            var shared = Shared();
            PlayerSession[] sessions;

            if (gameMode == GameMode.LocalVersus)
            {
                Image("Divider", canvas, Vector2.zero, new Vector2(4,880), new Color(.35f,.45f,.65f,.38f));
                var p1 = Player(shared, tileRoot, canvas, "PLAYER 1", -500, new[] {-750f,-500f,-250f}, Key.A, Key.W, Key.S, Key.D, new Color(.22f,.67f,.92f),player1InputSource,player1GamepadIndex,player1GamepadAxisThreshold,player1Joystick);
                var p2 = Player(shared, tileRoot, canvas, "PLAYER 2",  500, new[] { 250f, 500f, 750f}, Key.LeftArrow, Key.UpArrow, Key.DownArrow, Key.RightArrow, new Color(.95f,.45f,.55f),player2InputSource,player2GamepadIndex,player2GamepadAxisThreshold,player2Joystick);
                shared.Controller.Configure(shared.Audio, shared.Chart, new[] {p1.Tiles,p2.Tiles}, new[] {p1.Judge,p2.Judge}, gameplayConfig);
                shared.Root.AddComponent<LocalVersusMatchState>().Configure(p1.Score, p2.Score);
                sessions = new[] { p1.Session, p2.Session };
            }
            else
            {
                var p1 = Player(shared, tileRoot, canvas, "PLAYER 1", 700, new[] {-350f,0f,350f}, Key.A, Key.W, Key.S, Key.D, new Color(.22f,.67f,.92f),player1InputSource,player1GamepadIndex,player1GamepadAxisThreshold,player1Joystick);
                shared.Controller.Configure(shared.Audio, shared.Chart, p1.Tiles, p1.Judge, gameplayConfig);
                sessions = new[] { p1.Session };
            }
            var results = CreateResultsView(canvas);
            var flow=shared.Root.AddComponent<GameFlowController>(); flow.Configure(gameMode, shared.Controller, sessions, results);
            shared.Root.AddComponent<GameplayNavigationController>().Configure(flow,shared.Controller,results);
            if (song == null) Debug.LogWarning("[Bootstrap] No hay canción asignada.", this);
            if (chart == null) Debug.LogWarning("[Bootstrap] No hay chart asignado.", this);
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
            JoystickInputConfig joystickConfig)
        {
            var labels = new[] {"LEFT ←","CENTER ●","RIGHT →"};
            var hitZones = new Image[3];
            for (var i=0;i<3;i++)
            {
                var lane=Image(name+" Lane "+i, canvas, new Vector2(lanes[i],35), new Vector2(190,620), new Color(accent.r*.20f,accent.g*.20f,accent.b*.20f,.82f));
                SurexsVisualTheme.ApplyRounded(lane);
                hitZones[i]=Image(name+" Hit "+i, canvas, new Vector2(lanes[i],-175), new Vector2(180,80), new Color(.12f,.86f,.48f,.48f));
                SurexsVisualTheme.ApplyRounded(hitZones[i]);
                var hitOutline=hitZones[i].gameObject.AddComponent<Outline>(); hitOutline.effectColor=new Color(.45f,1f,.72f,.9f); hitOutline.effectDistance=new Vector2(3,-3);
                Text(name+" Label "+i, canvas, new Vector2(lanes[i],375), new Vector2(205,38), labels[i], 19);
            }
            var status=Text(name+" Status",canvas,new Vector2(x,420),new Vector2(880,80),"",17);
            var judgment=Text(name+" Judgment",canvas,new Vector2(x,-85),new Vector2(380,110),"",38);
            var milestone=Text(name+" Milestone",canvas,new Vector2(x,320),new Vector2(500,50),"",27);
            var visual=Visual(canvas,name,x,accent);
            var go=new GameObject(name+" Systems");
            var input=CreateInputSource(go,inputSourceType,gamepadIndex,gamepadAxisThreshold,joystickConfig,left,centerPrimary,centerSecondary,right);
            go.AddComponent<ControlInputFeedbackView>().Configure(input,hitZones[0],hitZones[1],hitZones[2]);
            var judge=go.AddComponent<RhythmJudge>(); judge.Configure(shared.Audio,input,gameplayConfig);
            var combo=go.AddComponent<ComboManager>(); combo.Configure(gameplayConfig);
            var score=go.AddComponent<ScoreManager>(); score.Configure(gameplayConfig,combo);
            var feedback=go.AddComponent<GameplayFeedbackView>(); feedback.Configure(judgment,milestone);
            go.AddComponent<JudgmentProcessor>().Configure(judge,score,combo,feedback);
            var poses=go.AddComponent<PoseController>(); poses.Configure(poseDefinitions,visual.Body,visual.LeftArm,visual.RightArm,visual.BodyImage,visual.PoseLabel);
            var player=go.AddComponent<PlayerController>(); player.Configure(judge,poses,poseDuration);
            var tiles=go.AddComponent<TileSpawner>(); tiles.Configure(shared.Audio,shared.Chart,tileRoot,judge,lanes[0],lanes[1],lanes[2],-175);
            go.AddComponent<PrototypeStatusView>().Configure(shared.Audio,shared.Chart,tiles,score,combo,status);
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
            return go.transform;
        }
        private static RectTransform Rect(string name,Transform parent,Vector2 pos,Vector2 size)
        { var go=new GameObject(name,typeof(RectTransform)); var r=go.GetComponent<RectTransform>(); r.SetParent(parent,false); r.anchoredPosition=pos; r.sizeDelta=size; return r; }
        private static Image Image(string name,Transform parent,Vector2 pos,Vector2 size,Color color)
        { var go=new GameObject(name,typeof(RectTransform),typeof(Image)); var r=go.GetComponent<RectTransform>(); r.SetParent(parent,false); r.anchoredPosition=pos; r.sizeDelta=size; var i=go.GetComponent<Image>(); i.color=color; i.raycastTarget=false; return i; }
        private static Text Text(string name,Transform parent,Vector2 pos,Vector2 size,string value,int fontSize)
        { var go=new GameObject(name,typeof(RectTransform),typeof(Text)); var r=go.GetComponent<RectTransform>(); r.SetParent(parent,false); r.anchoredPosition=pos; r.sizeDelta=size; var t=go.GetComponent<Text>(); t.font=SurexsVisualTheme.Font; t.fontSize=fontSize; t.fontStyle=FontStyle.Bold; t.alignment=TextAnchor.MiddleCenter; t.color=SurexsVisualTheme.TextPrimary; t.text=value; t.raycastTarget=false; return t; }
        private static Button Button(string name,Transform parent,Vector2 pos,Vector2 size,string label,Color color)
        { var image=Image(name,parent,pos,size,color); image.raycastTarget=true; var outline=image.gameObject.AddComponent<Outline>(); outline.effectColor=new Color(1,1,1,.25f); outline.effectDistance=new Vector2(2,-2); var button=image.gameObject.AddComponent<Button>(); button.targetGraphic=image; button.transition=Selectable.Transition.ColorTint; SurexsVisualTheme.StyleButton(button,color); Text("Label",image.transform,Vector2.zero,size,label,20); return button; }

        private readonly struct SharedSet { public SharedSet(GameObject r,AudioManager a,ChartManager c,RhythmPrototypeController p){Root=r;Audio=a;Chart=c;Controller=p;} public GameObject Root{get;} public AudioManager Audio{get;} public ChartManager Chart{get;} public RhythmPrototypeController Controller{get;} }
        private readonly struct PlayerSet { public PlayerSet(RhythmJudge j,TileSpawner t,ScoreManager s,PlayerSession session){Judge=j;Tiles=t;Score=s;Session=session;} public RhythmJudge Judge{get;} public TileSpawner Tiles{get;} public ScoreManager Score{get;} public PlayerSession Session{get;} }
        private readonly struct VisualSet { public VisualSet(RectTransform b,RectTransform l,RectTransform r,Image i,Text p){Body=b;LeftArm=l;RightArm=r;BodyImage=i;PoseLabel=p;} public RectTransform Body{get;} public RectTransform LeftArm{get;} public RectTransform RightArm{get;} public Image BodyImage{get;} public Text PoseLabel{get;} }
    }
}
