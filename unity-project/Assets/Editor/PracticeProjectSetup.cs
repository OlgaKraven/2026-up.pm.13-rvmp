using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public static class PracticeProjectSetup
{
    private static Font font;
    private static Sprite circle, diamond, cross, square;
    [MenuItem("Practice/Rebuild teaching scenes (replaces scenes)")]
    public static void BuildAll()
    {
        Directory.CreateDirectory("Assets/Art"); Directory.CreateDirectory("Assets/Prefabs");
        Directory.CreateDirectory("Assets/Scenes");
        circle = Shape("Circle",0); diamond = Shape("Diamond",1); cross = Shape("Cross",2); square = Shape("Square",3);
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        BuildScene(false,"Assets/Scenes/Main.unity");
        BuildScene(true,"Assets/Scenes/Demonstration.unity");
        EditorBuildSettings.scenes = new[] {new EditorBuildSettingsScene("Assets/Scenes/Main.unity",true)};
        EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
        AssetDatabase.SaveAssets();
        Debug.Log("TEACHING_SCENES_SAVED");
    }
    private static Sprite Shape(string name,int mode)
    {
        var texture = new Texture2D(64,64);
        for(int y=0;y<64;y++) for(int x=0;x<64;x++)
        {
            float a=(x+0.5f)/32f-1f,b=(y+0.5f)/32f-1f;
            bool inside=mode==0?a*a+b*b<0.90f:mode==1?Mathf.Abs(a)+Mathf.Abs(b)<0.94f:mode==2?Mathf.Abs(a-b)<0.24f||Mathf.Abs(a+b)<0.24f:true;
            texture.SetPixel(x,y,inside?Color.white:Color.clear);
        }
        texture.Apply(); string path="Assets/Art/"+name+".png";
        File.WriteAllBytes(path,texture.EncodeToPNG()); Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=TextureImporterType.Sprite; importer.spritePixelsPerUnit=64;
        importer.SaveAndReimport(); return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    private static GameObject SpriteObject(string name,Vector2 pos,Vector2 scale,Sprite sprite,ThemeRole.Role role)
    {
        var g=new GameObject(name);g.transform.position=pos;g.transform.localScale=scale;
        g.AddComponent<SpriteRenderer>().sprite=sprite;g.AddComponent<ThemeRole>().role=role;
        return g;
    }
    private static GameObject UI(string name,Transform parent,Vector2 min,Vector2 max,Vector2 offsetMin,Vector2 offsetMax)
    {
        var g=new GameObject(name,typeof(RectTransform));g.transform.SetParent(parent,false);
        var r=g.GetComponent<RectTransform>();r.anchorMin=min;r.anchorMax=max;r.offsetMin=offsetMin;r.offsetMax=offsetMax;
        return g;
    }
    private static GameObject Panel(string name,Transform parent)
    {
        var g=UI(name,parent,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
        g.AddComponent<Image>().color=new Color(.03f,.05f,.10f,.97f);
        g.AddComponent<ThemeRole>().role=ThemeRole.Role.Surface; return g;
    }
    private static Text Text(string name,Transform parent,string text,Vector2 min,Vector2 max,int size=24)
    {
        var g=UI(name,parent,min,max,new Vector2(12,8),new Vector2(-12,-8));
        var t=g.AddComponent<Text>();t.font=font;t.text=text;t.fontSize=size;t.color=Color.white;
        t.alignment=TextAnchor.MiddleCenter;t.horizontalOverflow=HorizontalWrapMode.Wrap;
        g.AddComponent<ThemeRole>().role=ThemeRole.Role.Text;return t;
    }
    private static Button Button(string name,Transform parent,string text,Vector2 min,Vector2 max,UnityEngine.Events.UnityAction action)
    {
        var g=UI(name,parent,min,max,new Vector2(8,8),new Vector2(-8,-8));g.AddComponent<Image>();
        var b=g.AddComponent<Button>();g.AddComponent<ThemeRole>().role=ThemeRole.Role.Surface;
        Text("Label",g.transform,text,Vector2.zero,Vector2.one,25);
        if(action!=null)UnityEventTools.AddPersistentListener(b.onClick,action);return b;
    }
    private static void BuildScene(bool finished,string path)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var camera=new GameObject("Main Camera");camera.tag="MainCamera";
        var c=camera.AddComponent<Camera>();c.clearFlags=CameraClearFlags.SolidColor;c.orthographic=true;c.orthographicSize=5.3f;
        camera.transform.position=new Vector3(0,0,-10);camera.AddComponent<AudioListener>();camera.AddComponent<CameraFit>();
        var root=new GameObject("GameManager");var gm=root.AddComponent<GameManager>();
        gm.audioFeedback=root.AddComponent<AudioFeedback>();gm.theme=root.AddComponent<VisualTheme>();
        if(!finished)
        {
            gm.theme.background=new Color(.18f,.18f,.18f);gm.theme.surface=new Color(.25f,.25f,.25f);
            gm.theme.player=new Color(.75f,.75f,.75f);gm.theme.target=new Color(.65f,.65f,.65f);
            gm.theme.danger=new Color(.55f,.55f,.55f);gm.theme.success=new Color(.85f,.85f,.85f);
        }
        gm.profiles=AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/profiles.json");
        var player=SpriteObject("Player",new Vector2(-6.5f,-2.2f),Vector2.one*.65f,circle,ThemeRole.Role.Player);
        player.AddComponent<CircleCollider2D>();player.AddComponent<Rigidbody2D>();
        gm.player=player.AddComponent<PlayerController>();
        gm.items=new Collectible[6];
        for(int i=0;i<6;i++)
        {
            var item=new GameObject("Item-"+(i+1));item.transform.position=new Vector2(-5.5f+i*2.1f,1f);
            item.AddComponent<CircleCollider2D>().radius=.42f;gm.items[i]=item.AddComponent<Collectible>();gm.items[i].index=i;
            var visual=SpriteObject("Visual",Vector2.zero,Vector2.one*.7f,diamond,ThemeRole.Role.Target);
            visual.transform.SetParent(item.transform,false);
            var animation=item.AddComponent<CollectibleVisual>();
            var so=new SerializedObject(animation);so.FindProperty("visual").objectReferenceValue=visual.transform;so.ApplyModifiedProperties();
            var label=new GameObject("Number");label.transform.SetParent(item.transform,false);label.transform.localPosition=new Vector3(0,.7f,0);
            var tm=label.AddComponent<TextMesh>();tm.text=(i+1).ToString();tm.characterSize=.22f;tm.fontSize=36;tm.anchor=TextAnchor.MiddleCenter;
        }
        for(int i=0;i<3;i++)
        {
            var hazard=SpriteObject("Hazard-"+(i+1),new Vector2(-3f+i*3f,-.9f),Vector2.one*.8f,cross,ThemeRole.Role.Danger);
            hazard.AddComponent<CircleCollider2D>();hazard.AddComponent<Hazard>();
        }
        var exit=SpriteObject("Exit",new Vector2(6.7f,2.3f),Vector2.one*.95f,square,ThemeRole.Role.Success);
        exit.AddComponent<BoxCollider2D>();exit.AddComponent<ExitZone>();
        var exitLabel=new GameObject("ExitLabel");exitLabel.transform.position=new Vector3(6.7f,2.95f,0);
        var et=exitLabel.AddComponent<TextMesh>();et.text="ВЫХОД";et.fontSize=28;et.characterSize=.15f;et.anchor=TextAnchor.MiddleCenter;
        gm.gateIndicators=new SpriteRenderer[3];
        for(int i=0;i<3;i++)gm.gateIndicators[i]=SpriteObject("GateIndicator-"+(i+1),new Vector2(5.6f+i*.45f,1.3f),new Vector2(.28f,.8f),square,ThemeRole.Role.Danger).GetComponent<SpriteRenderer>();
        for(int i=0;i<4;i++)
        {
            Vector2 pos=i<2?new Vector2(0,i==0?4.5f:-4.5f):new Vector2(i==2?-8.5f:8.5f,0);
            Vector2 scale=i<2?new Vector2(17,.25f):new Vector2(.25f,9);
            var wall=SpriteObject("Wall-"+i,pos,scale,square,ThemeRole.Role.Surface);wall.AddComponent<BoxCollider2D>();
        }
        var canvas=new GameObject("Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1280,720);scaler.matchWidthOrHeight=0;
        new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
        gm.screens=canvas.AddComponent<ScreenMockupController>();gm.hud=canvas.AddComponent<HUDView>();
        var menu=Panel("MainMenuPanel",canvas.transform);var game=UI("GamePanel",canvas.transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
        var pause=Panel("PausePanel",canvas.transform);var result=Panel("ResultPanel",canvas.transform);
        gm.screens.mainMenuPanel=menu;gm.screens.gamePanel=game;gm.screens.pausePanel=pause;gm.screens.resultPanel=result;
        gm.hud.menuTitle=Text("MenuTitle",menu.transform,"Кристальный маршрут",new Vector2(.05f,.68f),new Vector2(.95f,.87f),42);
        gm.hud.menuGoal=Text("MenuGoal",menu.transform,"Цель",new Vector2(.1f,.38f),new Vector2(.9f,.67f),28);
        Button("PlayButton",menu.transform,"ИГРАТЬ",new Vector2(.3f,.16f),new Vector2(.7f,.30f),gm.StartGame);
        var hud=UI("HUDPanel",game.transform,new Vector2(0,1),Vector2.one,new Vector2(16,-142),new Vector2(-16,-10));hud.AddComponent<Image>();hud.AddComponent<ThemeRole>();
        gm.hud.titleText=Text("TitleText",hud.transform,"Название",new Vector2(0,.56f),new Vector2(.76f,1),28);
        gm.hud.progressText=Text("ProgressText",hud.transform,"Прогресс",new Vector2(0,0),new Vector2(1,.56f),23);
        gm.hud.livesText=Text("LivesText",hud.transform,"Жизни: 3",new Vector2(.76f,.56f),new Vector2(1,1),24);
        gm.hud.hintText=Text("HintText",game.transform,"Подсказка",new Vector2(.26f,0),new Vector2(.98f,.15f),22);
        Button("PauseButton",game.transform,"ПАУЗА",new Vector2(.8f,.18f),new Vector2(.98f,.29f),gm.PauseGame);
        Vector2[] dirs={Vector2.up,Vector2.down,Vector2.left,Vector2.right};string[] labels={"↑","↓","←","→"};
        for(int i=0;i<4;i++)
        {
            Vector2 pos=new Vector2(i==2?16:i==3?176:96,i==0?96:16);
            var b=Button("Direction-"+i,game.transform,labels[i],Vector2.zero,Vector2.zero,null);
            var rect=b.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=Vector2.zero;rect.pivot=Vector2.zero;rect.anchoredPosition=pos;rect.sizeDelta=new Vector2(72,72);
            b.gameObject.AddComponent<DirectionalButton>().direction=dirs[i];
        }
        Text("PauseTitle",pause.transform,"ПАУЗА",new Vector2(.1f,.65f),new Vector2(.9f,.85f),44);
        Button("ResumeButton",pause.transform,"ПРОДОЛЖИТЬ",new Vector2(.25f,.38f),new Vector2(.75f,.53f),gm.ResumeGame);
        Button("MenuButton",pause.transform,"В МЕНЮ",new Vector2(.25f,.18f),new Vector2(.75f,.33f),gm.GoToMenu);
        gm.hud.resultTitle=Text("ResultTitle",result.transform,"Результат",new Vector2(.1f,.65f),new Vector2(.9f,.85f),42);
        gm.hud.resultMessage=Text("ResultMessage",result.transform,"Пояснение",new Vector2(.1f,.38f),new Vector2(.9f,.63f),28);
        Button("RestartButton",result.transform,"ЗАНОВО",new Vector2(.25f,.16f),new Vector2(.75f,.31f),gm.RestartGame);
        game.SetActive(false);pause.SetActive(false);result.SetActive(false);
        gm.theme.Apply();
        PrefabUtility.SaveAsPrefabAsset(player,"Assets/Prefabs/Player.prefab");
        PrefabUtility.SaveAsPrefabAsset(gm.items[0].gameObject,"Assets/Prefabs/Item.prefab");
        PrefabUtility.SaveAsPrefabAsset(GameObject.Find("Hazard-1"),"Assets/Prefabs/Hazard.prefab");
        PrefabUtility.SaveAsPrefabAsset(menu.transform.Find("PlayButton").gameObject,"Assets/Prefabs/Button.prefab");
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),path);
    }
}
