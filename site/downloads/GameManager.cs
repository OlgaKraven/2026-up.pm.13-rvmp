using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public bool IsPlaying { get; private set; }
    public Vector2 MobileDirection { get; private set; }
    public PlayerController player;
    public HUDView hud;
    public ScreenMockupController screens;
    public VisualTheme theme;
    public AudioFeedback audioFeedback;
    public Collectible[] items;
    public TextAsset profiles;
    [Range(0, 30)] public int variantNumber;
    public SpriteRenderer[] gateIndicators;
    public int Progress => progress;
    public int Cargo => cargo;
    public int Lives => lives;
    public bool Won { get; private set; }
    public Profile Current => profile;

    [Serializable] public class Profile
    {
        public string title, goal, progressLabel;
        public int mode, target, capacity;
        public bool resource;
    }
    [Serializable] private class ProfileList { public Profile[] variants; }
    private Profile profile;
    private int progress, cargo, lives;
    private float immunity, remaining;
    private readonly bool[] flags = new bool[6];

    private void Awake() { Instance = this; }
    private void Start() { SelectVariant(variantNumber); screens.ShowMainMenu(); }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPlaying) PauseGame(); else if (screens.pausePanel.activeSelf) ResumeGame();
        }
        if (IsPlaying && profile.resource)
        {
            remaining -= Time.deltaTime;
            Refresh();
            if (remaining <= 0f) Finish(false);
        }
    }

    public void SelectVariant(int number)
    {
        variantNumber = number;
        profile = number == 0
            ? new Profile { title = "Кристальный маршрут", goal = "Соберите 5 кристаллов. Избегайте знака !. Войдите в выход.", progressLabel = "Кристаллы", mode = 0, target = 5, capacity = 0 }
            : JsonUtility.FromJson<ProfileList>(profiles.text).variants[number - 1];
        for (int i = 0; i < items.Length; i++)
        {
            bool warning = profile.mode == 0 && i >= profile.target;
            items[i].transform.Find("Visual").GetComponent<ThemeRole>().role = warning ? ThemeRole.Role.Danger : ThemeRole.Role.Target;
            items[i].transform.Find("Number").GetComponent<TextMesh>().text = warning ? "!" : ((profile.mode == 4 || profile.mode == 5) ? i % profile.target + 1 : i + 1).ToString();
        }
        theme.Apply();
        hud.SetGoal(profile.title, profile.goal);
        ResetState();
    }

    private void ResetState()
    {
        progress = cargo = 0; lives = 3; remaining = 90f; immunity = 0f;
        Won = false; IsPlaying = false; MobileDirection = Vector2.zero;
        Array.Clear(flags, 0, flags.Length);
        foreach (Collectible item in items) item.gameObject.SetActive(true);
        player.ReturnToStart();
        hud.ShowHint(profile.goal, false);
        Refresh();
    }

    public void StartGame() { ResetState(); ResumeGame(); }
    public void RestartGame() { StartGame(); }
    public void ResumeGame() { IsPlaying = true; screens.ShowGame(); }
    public void PauseGame() { IsPlaying = false; MobileDirection = Vector2.zero; screens.ShowPause(); }
    public void GoToMenu() { ResetState(); screens.ShowMainMenu(); }
    public void SetDirection(Vector2 direction) { MobileDirection = IsPlaying ? direction : Vector2.zero; }

    public void Collect(Collectible item)
    {
        if (!IsPlaying || !item.gameObject.activeSelf) return;
        int type = item.index % profile.target;
        bool wrong = profile.mode == 0 && item.index >= profile.target
            || profile.mode == 1 && item.index != progress
            || (profile.mode == 2 || profile.mode == 3) && cargo >= profile.capacity
            || (profile.mode == 4 || profile.mode == 5) && flags[type];
        if (wrong) { Warn("Действие не принято: проверьте порядок, тип или вместимость."); return; }
        if (profile.mode == 2 || profile.mode == 3) cargo++;
        else { flags[type] = true; progress++; }
        item.gameObject.SetActive(false);
        audioFeedback.PlayCollect();
        hud.ShowHint("Принято. " + Status(), false);
        Refresh();
    }

    public void TryFinishLevel()
    {
        if (!IsPlaying) return;
        if (profile.mode == 2 || profile.mode == 3)
        {
            if (cargo == 0) { Warn("Сначала возьмите груз."); return; }
            progress += cargo; cargo = 0;
            hud.ShowHint("Груз доставлен. " + Status(), false);
            Refresh();
            if (progress < profile.target) { audioFeedback.PlayCollect(); return; }
        }
        if (progress >= profile.target) Finish(true);
        else Warn("Цель ещё не достигнута. " + Status());
    }

    public void HitHazard()
    {
        if (!IsPlaying || Time.time < immunity) return;
        lives--; immunity = Time.time + 1f;
        player.ReturnToStart();
        if (lives == 0) Finish(false);
        else { Warn("Повреждение: потеряна жизнь."); Refresh(); }
    }

    private void Warn(string message) { audioFeedback.PlayHazard(); hud.ShowHint(message, true); }
    private string Status()
    {
        string text = profile.progressLabel + ": " + progress + "/" + profile.target;
        if (profile.mode == 1) text += " · следующий №" + Mathf.Min(progress + 1, profile.target);
        if (profile.mode == 2 || profile.mode == 3) text += " · груз " + cargo + "/" + profile.capacity;
        if (profile.mode == 4 || profile.mode == 5)
        {
            text += " · ";
            for (int i = 0; i < profile.target; i++) text += (flags[i] ? "[✓" : "[ ") + (i + 1) + "] ";
        }
        if (profile.resource) text += " · ресурс " + Mathf.CeilToInt(remaining);
        return text;
    }

    private void Refresh()
    {
        hud.ShowProgress(profile.title, Status(), lives);
        for (int i = 0; i < gateIndicators.Length; i++)
        {
            gateIndicators[i].gameObject.SetActive(profile.mode == 5 && i < profile.target);
            gateIndicators[i].color = flags[i] ? theme.success : theme.danger;
        }
    }

    private void Finish(bool won)
    {
        Won = won; IsPlaying = false; MobileDirection = Vector2.zero;
        if (won) audioFeedback.PlaySuccess(); else audioFeedback.PlayFailure();
        hud.ShowResult(won); screens.ShowResult();
    }
}
