using Surexs.DanceOff.Input;
using Surexs.DanceOff.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Surexs.DanceOff.Core
{
    public sealed class MainMenuSceneBootstrap : MonoBehaviour
    {
        [Header("Typography")]
        [SerializeField] private Font titleFont;
        [SerializeField] private Font bodyFont;
        [Header("Main Menu Graphics")]
        [SerializeField] private Texture brokerHeroLogoTexture;
        [SerializeField] private Texture surexsLogoTexture;
        [SerializeField] private Texture blueButtonTexture;
        [SerializeField] private Texture greenButtonTexture;
        [SerializeField] private VideoClip gameplayDemoClip;
        [Header("Instructions")]
        [SerializeField] private Texture instructionsTexture;
        [SerializeField, Range(0.25f, 2f)] private float readyHoldDuration = 0.8f;
        [Header("Ready Input Sources")]
        [SerializeField] private RhythmInputSourceType player1InputSource = RhythmInputSourceType.Gamepad;
        [SerializeField] private RhythmInputSourceType player2InputSource = RhythmInputSourceType.Gamepad;
        [SerializeField, Min(0)] private int player1GamepadIndex;
        [SerializeField, Min(0)] private int player2GamepadIndex = 1;
        [SerializeField] private string player1GamepadDevice = "XInputControllerWindows";
        [SerializeField] private string player2GamepadDevice = "XInputControllerWindows1";
        [SerializeField] private JoystickInputConfig player1Joystick = new JoystickInputConfig();
        [SerializeField] private JoystickInputConfig player2Joystick = new JoystickInputConfig { joystickIndex = 1 };

        private void Awake()
        {
            SurexsVisualTheme.ConfigureFonts(titleFont,bodyFont);
            CreateEventSystem();
            var canvas=CreateCanvas();
            var main=Rect("Main Menu",canvas,Vector2.zero,new Vector2(1920,1080));
            Stretch(main);
            CreateMainBackground(main);
            CreateVideoPreview(main);
            CreateHeader(main);

            var controller=gameObject.AddComponent<MainMenuController>();
            var play=TextureButton("Play",main,new Vector2(0,-315),new Vector2(460,118),"JUGAR",greenButtonTexture,39);
            var solo=TextureButton("Solo",main,new Vector2(-235,-445),new Vector2(420,94),"1 JUGADOR",blueButtonTexture,28);
            var versus=TextureButton("Versus",main,new Vector2(235,-445),new Vector2(420,94),"1 VS 1",blueButtonTexture,28);
            var leaderboardButton=TextureButton("Leaderboard",main,new Vector2(0,-445),new Vector2(460,94),
                "LEADERBOARD",blueButtonTexture,28);
            ConfigureMenuNavigation(play,solo,versus,leaderboardButton);

            var leaderboardPanel=CreateLeaderboard(canvas,out var leaderboardView,out var leaderboardBack);

            var instructions=CreateInstructions(canvas);
            var p1Ready=CreateReadyPrompt(instructions.transform,"PLAYER 1",new Vector2(-420,-390),
                new Color(.10f,.70f,1f),blueButtonTexture);
            var p2Ready=CreateReadyPrompt(instructions.transform,"PLAYER 2",new Vector2(420,-390),
                new Color(.10f,.70f,1f),blueButtonTexture);
            var instructionsBack=TextureButton("Instructions Back",instructions.transform,new Vector2(0,-390),
                new Vector2(300,112),"B  VOLVER",greenButtonTexture,27);
            var continueLabel=Text("Ready Continue",instructions.transform,new Vector2(0,-495),
                new Vector2(1200,72),"",34);
            continueLabel.font=SurexsVisualTheme.TitleFont;

            var p1Input=gameObject.AddComponent<PlayerReadyInputMonitor>();
            p1Input.Configure(player1InputSource,player1GamepadIndex,player1GamepadDevice,player1Joystick,
                Key.A,Key.W,Key.S,Key.D);
            var p2Input=gameObject.AddComponent<PlayerReadyInputMonitor>();
            p2Input.Configure(player2InputSource,player2GamepadIndex,player2GamepadDevice,player2Joystick,
                Key.LeftArrow,Key.UpArrow,Key.DownArrow,Key.RightArrow);

            controller.Configure(main.gameObject,instructions,leaderboardPanel,play,solo,versus,leaderboardButton,
                leaderboardBack,leaderboardView,instructionsBack,p1Input,p2Input,p1Ready,p2Ready,continueLabel,
                readyHoldDuration);
            play.onClick.AddListener(controller.FocusModeSelection);
            solo.onClick.AddListener(controller.StartSolo);
            versus.onClick.AddListener(controller.StartVersus);
            leaderboardButton.onClick.AddListener(controller.ShowLeaderboard);
            leaderboardBack.onClick.AddListener(controller.ShowMain);
            instructionsBack.onClick.AddListener(controller.FocusModeSelection);

            WarnMissingAsset(brokerHeroLogoTexture,nameof(brokerHeroLogoTexture));
            WarnMissingAsset(surexsLogoTexture,nameof(surexsLogoTexture));
            WarnMissingAsset(blueButtonTexture,nameof(blueButtonTexture));
            WarnMissingAsset(greenButtonTexture,nameof(greenButtonTexture));
            WarnMissingAsset(gameplayDemoClip,nameof(gameplayDemoClip));
            WarnMissingAsset(instructionsTexture,nameof(instructionsTexture));
        }

        private void CreateMainBackground(Transform parent)
        {
            var background=Image("Background",parent,Vector2.zero,new Vector2(1920,1080),new Color(.006f,.025f,.072f,1f));
            Stretch(background.rectTransform);
            background.raycastTarget=false;
            for (var index=0;index<18;index++)
            {
                var normalized=index/17f;
                var thickness=index%4==0 ? 3f : 1.5f;
                var line=Image("Menu Blue Line "+index,parent,
                    new Vector2(0,Mathf.Lerp(-520f,520f,normalized)),new Vector2(1920,thickness),
                    new Color(.02f,.35f,.78f,index%4==0 ? .2f : .085f));
                line.rectTransform.anchorMin=new Vector2(0f,.5f);
                line.rectTransform.anchorMax=new Vector2(1f,.5f);
                line.rectTransform.sizeDelta=new Vector2(0f,thickness);
                line.raycastTarget=false;
            }
        }

        private void CreateHeader(Transform parent)
        {
            var bar=Image("Header Bar",parent,Vector2.zero,new Vector2(1920,180),new Color(.015f,.13f,.28f,.98f));
            bar.rectTransform.anchorMin=new Vector2(0f,1f);
            bar.rectTransform.anchorMax=new Vector2(1f,1f);
            bar.rectTransform.pivot=new Vector2(.5f,1f);
            bar.rectTransform.anchoredPosition=Vector2.zero;
            bar.rectTransform.sizeDelta=new Vector2(0f,180f);
            bar.raycastTarget=false;
            var accent=Image("Header Accent",bar.transform,new Vector2(0,-177),new Vector2(1920,3),new Color(.02f,.68f,1f,.9f));
            accent.rectTransform.anchorMin=new Vector2(0f,1f);
            accent.rectTransform.anchorMax=new Vector2(1f,1f);
            accent.rectTransform.sizeDelta=new Vector2(0f,3f);
            accent.raycastTarget=false;

            var gameLogo=Raw("Broker Hero Logo",bar.transform,new Vector2(0,-105),new Vector2(500,250),brokerHeroLogoTexture);
            gameLogo.raycastTarget=false;
            gameLogo.gameObject.AddComponent<MenuLogoPulse>();
            var companyLogo=Raw("Surexs Logo",bar.transform,new Vector2(835,-52),new Vector2(140,32),surexsLogoTexture);
            companyLogo.raycastTarget=false;
        }

        private void CreateVideoPreview(Transform parent)
        {
            var frame=Image("Gameplay Demo Frame",parent,new Vector2(0,55),new Vector2(1090,610),new Color(.005f,.055f,.12f,.98f));
            SurexsVisualTheme.ApplyRounded(frame);
            frame.raycastTarget=false;
            var outline=frame.gameObject.AddComponent<Outline>();
            outline.effectColor=new Color(.05f,.72f,1f,.95f);
            outline.effectDistance=new Vector2(5f,-5f);
            var shadow=frame.gameObject.AddComponent<Shadow>();
            shadow.effectColor=new Color(0,0,0,.72f);
            shadow.effectDistance=new Vector2(0,-12f);

            var viewport=Rect("Gameplay Demo Viewport",frame.transform,Vector2.zero,new Vector2(1050,570));
            viewport.gameObject.AddComponent<RectMask2D>();
            var videoImage=Raw("Gameplay Demo",viewport,Vector2.zero,new Vector2(1050,570),null);
            videoImage.raycastTarget=false;
            var aspect=videoImage.gameObject.AddComponent<AspectRatioFitter>();
            aspect.aspectMode=AspectRatioFitter.AspectMode.FitInParent;
            aspect.aspectRatio=16f/9f;
            videoImage.gameObject.AddComponent<MenuVideoPreview>().Configure(videoImage,gameplayDemoClip,aspect);
        }

        private GameObject CreateInstructions(Transform canvas)
        {
            var instructions=Image("Instructions",canvas,Vector2.zero,new Vector2(1920,1080),new Color(.008f,.035f,.10f,1f));
            Stretch(instructions.rectTransform);
            instructions.raycastTarget=true;
            for (var lineIndex=0;lineIndex<22;lineIndex++)
            {
                var normalized=lineIndex/21f;
                var line=Image("Instruction Blue Line "+lineIndex,instructions.transform,
                    new Vector2(0,Mathf.Lerp(-520f,520f,normalized)),new Vector2(1920,2f),
                    new Color(.03f,.48f,1f,Mathf.Lerp(.08f,.24f,1f-Mathf.Abs(normalized-.5f)*2f)));
                line.rectTransform.anchorMin=new Vector2(0f,.5f);
                line.rectTransform.anchorMax=new Vector2(1f,.5f);
                line.rectTransform.sizeDelta=new Vector2(0f,2f);
                line.raycastTarget=false;
            }
            var artwork=Raw("Instruction Artwork",instructions.transform,Vector2.zero,new Vector2(1672,941),instructionsTexture);
            artwork.raycastTarget=false;
            var aspect=artwork.gameObject.AddComponent<AspectRatioFitter>();
            aspect.aspectMode=AspectRatioFitter.AspectMode.FitInParent;
            aspect.aspectRatio=1672f/941f;
            return instructions.gameObject;
        }

        private PlayerReadyPromptView CreateReadyPrompt(Transform parent,string playerName,Vector2 position,
            Color accent,Texture buttonTexture)
        {
            var panel=Raw(playerName+" Ready",parent,position,new Vector2(430,128),buttonTexture);
            panel.color=Color.white;
            panel.raycastTarget=false;
            var outline=panel.gameObject.AddComponent<Outline>();
            outline.effectColor=new Color(accent.r,accent.g,accent.b,.95f);
            outline.effectDistance=new Vector2(4,-4);
            var fill=Image(playerName+" Ready Progress",panel.transform,new Vector2(0,-44),new Vector2(344,11),accent);
            fill.type=UnityEngine.UI.Image.Type.Filled;
            fill.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;
            fill.fillOrigin=(int)UnityEngine.UI.Image.OriginHorizontal.Left;
            fill.fillAmount=0f;
            fill.raycastTarget=false;
            var label=Text(playerName+" Ready Label",panel.transform,new Vector2(0,5),new Vector2(390,88),"",28);
            label.font=SurexsVisualTheme.TitleFont;
            label.lineSpacing=1.05f;
            var view=panel.gameObject.AddComponent<PlayerReadyPromptView>();
            view.Configure(playerName,label,fill,outline,accent);
            return view;
        }

        private GameObject CreateLeaderboard(Transform canvas,out LeaderboardView view,out Button back)
        {
            var page=Rect("Menu Leaderboard",canvas,Vector2.zero,new Vector2(1920,1080));
            Stretch(page);
            CreateMainBackground(page);
            CreateHeader(page);
            var title=Text("Menu Leaderboard Title",page,new Vector2(0,375),new Vector2(1000,90),"LEADERBOARD",52);
            title.font=SurexsVisualTheme.TitleFont;
            var board=Image("Menu Leaderboard Card",page,new Vector2(0,5),new Vector2(980,650),new Color(.01f,.10f,.22f,.97f));
            SurexsVisualTheme.ApplyRounded(board);
            var boardTitle=Text("Menu Leaderboard Card Title",board.transform,new Vector2(0,260),new Vector2(820,60),"MEJORES PUNTAJES",32);
            boardTitle.color=SurexsVisualTheme.Primary;
            var entries=Text("Menu Leaderboard Entries",board.transform,new Vector2(0,-15),new Vector2(820,485),"",27);
            entries.alignment=TextAnchor.UpperLeft;
            entries.lineSpacing=1.15f;
            back=TextureButton("Leaderboard Back",page,new Vector2(0,-425),new Vector2(390,105),"VOLVER",
                greenButtonTexture,30);
            back.navigation=new Navigation
            {
                mode=Navigation.Mode.Explicit,selectOnUp=back,selectOnDown=back,selectOnLeft=back,selectOnRight=back
            };
            view=page.gameObject.AddComponent<LeaderboardView>();
            view.Configure(page.gameObject,entries,back);
            return page.gameObject;
        }

        private static void ConfigureMenuNavigation(Button play,Button solo,Button versus,Button leaderboard)
        {
            play.navigation=ExplicitNavigation(leaderboard,leaderboard,leaderboard,leaderboard);
            solo.navigation=ExplicitNavigation(play,play,versus,play);
            versus.navigation=ExplicitNavigation(play,solo,solo,play);
            leaderboard.navigation=ExplicitNavigation(play,leaderboard,leaderboard,play);
        }

        private static Navigation ExplicitNavigation(Selectable up,Selectable left,Selectable right,Selectable down)
        {
            return new Navigation
            {
                mode=Navigation.Mode.Explicit,
                selectOnUp=up,
                selectOnLeft=left,
                selectOnRight=right,
                selectOnDown=down
            };
        }

        private void WarnMissingAsset(Object asset,string field)
        {
            if (asset == null) Debug.LogWarning($"[MainMenu] Falta el asset '{field}'.",this);
        }

        private static void CreateEventSystem()
        {
            if (EventSystem.current != null) return;
            var go=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
            go.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        private static Transform CreateCanvas()
        {
            var go=new GameObject("Menu Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            var canvas=go.GetComponent<Canvas>();
            canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect=true;
            var scaler=go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight=.5f;
            return go.transform;
        }

        private static RectTransform Rect(string name,Transform parent,Vector2 pos,Vector2 size)
        {
            var go=new GameObject(name,typeof(RectTransform));
            var rect=go.GetComponent<RectTransform>();
            rect.SetParent(parent,false);
            rect.anchoredPosition=pos;
            rect.sizeDelta=size;
            return rect;
        }

        private static Image Image(string name,Transform parent,Vector2 pos,Vector2 size,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));
            var rect=go.GetComponent<RectTransform>();
            rect.SetParent(parent,false);
            rect.anchoredPosition=pos;
            rect.sizeDelta=size;
            var image=go.GetComponent<Image>();
            image.color=color;
            return image;
        }

        private static RawImage Raw(string name,Transform parent,Vector2 pos,Vector2 size,Texture texture)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(RawImage));
            var rect=go.GetComponent<RectTransform>();
            rect.SetParent(parent,false);
            rect.anchoredPosition=pos;
            rect.sizeDelta=size;
            var image=go.GetComponent<RawImage>();
            image.texture=texture;
            image.color=Color.white;
            return image;
        }

        private static Button TextureButton(string name,Transform parent,Vector2 pos,Vector2 size,string label,
            Texture texture,int fontSize)
        {
            var image=Raw(name,parent,pos,size,texture);
            image.raycastTarget=true;
            var button=image.gameObject.AddComponent<Button>();
            button.targetGraphic=image;
            button.transition=Selectable.Transition.ColorTint;
            SurexsVisualTheme.StyleButton(button,Color.white);
            var text=Text("Label",image.transform,Vector2.zero,new Vector2(size.x*.78f,size.y*.58f),label,fontSize);
            text.font=SurexsVisualTheme.BodyFont;
            return button;
        }

        private static Text Text(string name,Transform parent,Vector2 pos,Vector2 size,string value,int fontSize)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Text));
            var rect=go.GetComponent<RectTransform>();
            rect.SetParent(parent,false);
            rect.anchoredPosition=pos;
            rect.sizeDelta=size;
            var text=go.GetComponent<Text>();
            text.font=SurexsVisualTheme.BodyFont;
            text.text=value;
            text.fontSize=fontSize;
            text.fontStyle=FontStyle.Normal;
            text.alignment=TextAnchor.MiddleCenter;
            text.color=SurexsVisualTheme.TextPrimary;
            text.raycastTarget=false;
            return text;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin=Vector2.zero;
            rect.anchorMax=Vector2.one;
            rect.offsetMin=Vector2.zero;
            rect.offsetMax=Vector2.zero;
        }
    }
}
