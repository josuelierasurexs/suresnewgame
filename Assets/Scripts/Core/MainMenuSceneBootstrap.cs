using Surexs.DanceOff.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Surexs.DanceOff.Core
{
    public sealed class MainMenuSceneBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            CreateEventSystem();
            var canvas=CreateCanvas();
            Image("Background",canvas,Vector2.zero,new Vector2(1920,1080),new Color(.035f,.047f,.082f));
            Text("Title",canvas,new Vector2(0,360),new Vector2(1200,150),"SUREXS\nDANCE OFF",64);
            Text("Subtitle",canvas,new Vector2(0,260),new Vector2(900,45),"RHYTHM • POSES • OFFICE CHAOS",20);
            var controller=gameObject.AddComponent<MainMenuController>();
            var main=Panel("Main",canvas); var modes=Panel("Modes",canvas); var options=Panel("Options",canvas);
            var play=Button("Play",main.transform,new Vector2(0,80),"JUGAR");
            var opts=Button("Options",main.transform,new Vector2(0,-20),"OPCIONES");
            var quit=Button("Quit",main.transform,new Vector2(0,-120),"SALIR");
            Text("Mode Title",modes.transform,new Vector2(0,170),new Vector2(900,80),"SELECCIONA MODO",42);
            var solo=Button("Solo",modes.transform,new Vector2(0,60),"SOLO");
            var versus=Button("Versus",modes.transform,new Vector2(0,-40),"1 VS 1");
            var modeBack=Button("Back",modes.transform,new Vector2(0,-140),"VOLVER");
            Text("Options Title",options.transform,new Vector2(0,120),new Vector2(900,80),"OPCIONES",42);
            Text("Options Placeholder",options.transform,new Vector2(0,20),new Vector2(900,60),"PRÓXIMAMENTE",28);
            var optionsBack=Button("Options Back",options.transform,new Vector2(0,-110),"VOLVER");
            controller.Configure(main,modes,options,play,solo,optionsBack);
            play.onClick.AddListener(controller.ShowModes); opts.onClick.AddListener(controller.ShowOptions); quit.onClick.AddListener(controller.Quit);
            solo.onClick.AddListener(controller.StartSolo); versus.onClick.AddListener(controller.StartVersus);
            modeBack.onClick.AddListener(controller.ShowMain); optionsBack.onClick.AddListener(controller.ShowMain);
        }

        private static void CreateEventSystem()
        {
            if (EventSystem.current != null) return;
            var go=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
            go.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }
        private static Transform CreateCanvas()
        { var go=new GameObject("Menu Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)); var c=go.GetComponent<Canvas>(); c.renderMode=RenderMode.ScreenSpaceOverlay; var s=go.GetComponent<CanvasScaler>(); s.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; s.referenceResolution=new Vector2(1920,1080); return go.transform; }
        private static GameObject Panel(string name,Transform parent)
        { var go=new GameObject(name,typeof(RectTransform),typeof(Image)); var r=go.GetComponent<RectTransform>(); r.SetParent(parent,false); r.sizeDelta=new Vector2(760,570); r.anchoredPosition=new Vector2(0,-100); go.GetComponent<Image>().color=new Color(.055f,.075f,.13f,.92f); return go; }
        private static Button Button(string name,Transform parent,Vector2 pos,string label)
        { var image=Image(name,parent,pos,new Vector2(420,78),new Color(.12f,.38f,.65f)); var outline=image.gameObject.AddComponent<Outline>(); outline.effectColor=new Color(.35f,.75f,1f,.85f); outline.effectDistance=new Vector2(2,-2); var b=image.gameObject.AddComponent<Button>(); b.targetGraphic=image; b.transition=Selectable.Transition.ColorTint; b.colors=Colors(image.color); Text("Label",image.transform,Vector2.zero,new Vector2(420,78),label,26); return b; }
        private static Image Image(string name,Transform parent,Vector2 pos,Vector2 size,Color color)
        { var go=new GameObject(name,typeof(RectTransform),typeof(Image)); var r=go.GetComponent<RectTransform>(); r.SetParent(parent,false); r.anchoredPosition=pos; r.sizeDelta=size; var i=go.GetComponent<Image>(); i.color=color; return i; }
        private static Text Text(string name,Transform parent,Vector2 pos,Vector2 size,string value,int fontSize)
        { var go=new GameObject(name,typeof(RectTransform),typeof(Text)); var r=go.GetComponent<RectTransform>(); r.SetParent(parent,false); r.anchoredPosition=pos; r.sizeDelta=size; var t=go.GetComponent<Text>(); t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); t.text=value; t.fontSize=fontSize; t.fontStyle=FontStyle.Bold; t.alignment=TextAnchor.MiddleCenter; t.color=Color.white; return t; }
        private static ColorBlock Colors(Color normal)
        { var c=ColorBlock.defaultColorBlock; c.normalColor=normal; c.highlightedColor=new Color(.22f,.62f,.92f); c.selectedColor=new Color(.25f,.72f,1f); c.pressedColor=new Color(.08f,.28f,.48f); c.disabledColor=new Color(.16f,.18f,.22f,.65f); c.colorMultiplier=1f; c.fadeDuration=.08f; return c; }
    }
}
