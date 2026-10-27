using System;
using UnityEngine;

namespace GTWM.FpsLimiter
{
    [ModInfo("gtwm.fpslimiter", "FPS Limiter", "1.0.0",
        Author = "GTWM",
        Description = "Cap the frame rate with hotkeys: presets 240, 60 and 45 plus three custom slots, all rebindable. Optional FPS counter. F11 menu.")]
    public sealed class FpsLimiterMod : Mod
    {
        private sealed class LimitSlot
        {
            public string Id;
            public string Name;
            public int Fps;
            public bool Custom;
            public KeyBind Key;
        }

        private const int MinFps = 10;
        private const int MaxFps = 1000;

        private LimitSlot[] _slots;
        private KeyBind _offKey;
        private KeyBind _menuKey;
        private bool _open;
        private Rect _window = new Rect(-1f, -1f, 500f, 640f);
        private Vector2 _scroll;
        private readonly string[] _customText = new string[3];

        private int _active; // 0 = off (game default)
        private bool _saved;
        private int _savedVsync;
        private int _savedTarget;

        private bool _showCounter;
        private bool _pressAgainToClear;
        private float _smoothedDelta = 1f / 60f;
        private string _toast;
        private float _toastUntil;

        public override void OnLoad()
        {
            _slots = new[]
            {
                new LimitSlot { Id = "preset1", Name = "Preset", Fps = 240 },
                new LimitSlot { Id = "preset2", Name = "Preset", Fps = 60 },
                new LimitSlot { Id = "preset3", Name = "Preset", Fps = 45 },
                new LimitSlot { Id = "custom1", Name = "Custom 1", Fps = 144, Custom = true },
                new LimitSlot { Id = "custom2", Name = "Custom 2", Fps = 120, Custom = true },
                new LimitSlot { Id = "custom3", Name = "Custom 3", Fps = 30, Custom = true },
            };
            for (int i = 0; i < _slots.Length; i++)
            {
                LimitSlot s = _slots[i];
                s.Key = Config.GetKeyBind(s.Id + "_key", new KeyBind(KeyCode.Alpha1 + i, ctrl: true));
                if (s.Custom)
                {
                    s.Fps = Mathf.Clamp(Config.Get(s.Id + "_fps", s.Fps), MinFps, MaxFps);
                    _customText[i - 3] = s.Fps.ToString();
                }
            }
            _offKey = Config.GetKeyBind("off_key", new KeyBind(KeyCode.Alpha0, ctrl: true));
            _menuKey = Config.GetKeyBind("menu_key", new KeyBind(KeyCode.F11));
            _showCounter = Config.Get("show_counter", false);
            _pressAgainToClear = Config.Get("press_again_to_clear", true);

            int remembered = Config.Get("active_fps", 0);
            if (remembered > 0)
            {
                Apply(remembered, announce: false);
            }
        }

        public override void OnUnload() => Clear(announce: false, remember: false);

        public override void OnUpdate()
        {
            _smoothedDelta = Mathf.Lerp(_smoothedDelta, Time.unscaledDeltaTime, 0.05f);
            if (_menuKey.Down())
            {
                _open = !_open;
                ModUI.SetMenuOpen(this, _open);
            }
            foreach (LimitSlot s in _slots)
            {
                if (s.Key.Down())
                {
                    if (_pressAgainToClear && _active == s.Fps)
                    {
                        Clear(announce: true, remember: true);
                    }
                    else
                    {
                        Apply(s.Fps, announce: true);
                    }
                }
            }
            if (_offKey.Down())
            {
                Clear(announce: true, remember: true);
            }

            // The game sets its own target (and the video menu toggles vsync, which overrides any
            // target); keep ours in force while a limit is on.
            if (_active > 0 && (Application.targetFrameRate != _active || QualitySettings.vSyncCount != 0))
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = _active;
            }
        }

        public override void OnGUI()
        {
            if (_showCounter)
            {
                DrawCounter();
            }
            if (_toast != null && Time.unscaledTime < _toastUntil)
            {
                DrawToast();
            }
            if (_open && ModUI.Panel("gtwm.fpslimiter", ref _window, "FPS Limiter", _active > 0 ? $"Limited to {_active} FPS" : "Off · game default", DrawContent))
            {
                _open = false;
                ModUI.SetMenuOpen(this, false);
            }
        }

        // ───────────────────────────── limiting ─────────────────────────────

        private void Apply(int fps, bool announce)
        {
            fps = Mathf.Clamp(fps, MinFps, MaxFps);
            if (!_saved)
            {
                _savedVsync = QualitySettings.vSyncCount;
                _savedTarget = Application.targetFrameRate;
                _saved = true;
            }
            _active = fps;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = fps;
            Config.Set("active_fps", fps);
            if (announce)
            {
                Toast($"{fps} FPS limit");
            }
        }

        private void Clear(bool announce, bool remember)
        {
            if (_saved)
            {
                QualitySettings.vSyncCount = _savedVsync;
                Application.targetFrameRate = _savedTarget;
                _saved = false;
            }
            _active = 0;
            if (remember)
            {
                Config.Set("active_fps", 0);
            }
            if (announce)
            {
                Toast("FPS limit off");
            }
        }

        private void Toast(string text)
        {
            _toast = text;
            _toastUntil = Time.unscaledTime + 1.6f;
        }

        // ───────────────────────────── UI ─────────────────────────────

        private void DrawContent()
        {
            _scroll = ModUI.BeginScroll(_scroll);

            ModUI.BeginGroup("Now");
            GUILayout.BeginHorizontal(GUILayout.Height(52f));
            GUILayout.BeginVertical();
            GUILayout.Label(_active > 0 ? $"{_active} FPS" : "No limit", ModUI.Title);
            GUILayout.Label($"Running at {1f / Mathf.Max(_smoothedDelta, 1e-4f):0} FPS", ModUI.Caption);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            if (_active > 0 && ModUI.Button("Remove limit", ButtonKind.Secondary, GUILayout.Width(130f)))
            {
                Clear(announce: false, remember: true);
            }
            GUILayout.EndHorizontal();
            ModUI.EndGroup();

            ModUI.BeginGroup("Presets");
            for (int i = 0; i < 3; i++)
            {
                if (i > 0)
                {
                    ModUI.Separator();
                }
                SlotRow(_slots[i], i);
            }
            ModUI.EndGroup();

            ModUI.BeginGroup("Custom");
            for (int i = 3; i < 6; i++)
            {
                if (i > 3)
                {
                    ModUI.Separator();
                }
                SlotRow(_slots[i], i);
            }
            ModUI.EndGroup($"Any value from {MinFps} to {MaxFps}.");

            ModUI.BeginGroup("Keys");
            if (ModUI.KeybindField("fps.off", "Remove limit", ref _offKey))
            {
                Config.SetKeyBind("off_key", _offKey);
            }
            ModUI.Separator();
            if (ModUI.KeybindField("fps.menu", "Open this menu", ref _menuKey))
            {
                Config.SetKeyBind("menu_key", _menuKey);
            }
            ModUI.Separator();
            bool again = ModUI.Switch("Press again to remove", _pressAgainToClear, "Pressing the active limit's key turns the limit off.");
            if (again != _pressAgainToClear)
            {
                _pressAgainToClear = again;
                Config.Set("press_again_to_clear", again);
            }
            ModUI.EndGroup("Click a key to change it, then press the new combination. Esc cancels, Backspace clears.");

            ModUI.BeginGroup("Display");
            bool counter = ModUI.Switch("FPS counter", _showCounter);
            if (counter != _showCounter)
            {
                _showCounter = counter;
                Config.Set("show_counter", counter);
            }
            ModUI.EndGroup("While a limit is on, VSync is turned off so the limit can apply. It's restored when you remove the limit.");

            ModUI.EndScroll();
        }

        private void SlotRow(LimitSlot s, int index)
        {
            GUILayout.BeginHorizontal(GUILayout.Height(44f));
            if (s.Custom)
            {
                GUILayout.Label(s.Name, ModUI.Body, GUILayout.Width(80f), GUILayout.Height(36f));
                string text = ModUI.Field(_customText[index - 3], 4, "FPS", "gtwm.fps." + s.Id, GUILayout.Width(70f));
                if (text != _customText[index - 3])
                {
                    _customText[index - 3] = new string(Array.FindAll(text.ToCharArray(), char.IsDigit));
                    if (int.TryParse(_customText[index - 3], out int fps) && fps >= MinFps && fps <= MaxFps && fps != s.Fps)
                    {
                        bool wasActive = _active == s.Fps;
                        s.Fps = fps;
                        Config.Set(s.Id + "_fps", fps);
                        if (wasActive)
                        {
                            Apply(fps, announce: false);
                        }
                    }
                }
                GUILayout.Label("FPS", ModUI.Caption, GUILayout.Width(30f), GUILayout.Height(36f));
            }
            else
            {
                GUILayout.Label($"{s.Fps} FPS", ModUI.Headline, GUILayout.Width(180f), GUILayout.Height(36f));
            }
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical(GUILayout.Width(140f));
            if (ModUI.KeybindField("fps." + s.Id, "", ref s.Key))
            {
                Config.SetKeyBind(s.Id + "_key", s.Key);
            }
            GUILayout.EndVertical();
            GUILayout.Space(8f);
            if (_active == s.Fps && _active > 0)
            {
                GUILayout.BeginVertical(GUILayout.Width(70f));
                GUILayout.Space(10f);
                ModUI.Badge("Active", ModUI.Good);
                GUILayout.EndVertical();
            }
            else if (ModUI.Button("Use", ButtonKind.Primary, GUILayout.Width(70f)))
            {
                Apply(s.Fps, announce: false);
            }
            GUILayout.EndHorizontal();
        }

        private void DrawCounter()
        {
            float fps = 1f / Mathf.Max(_smoothedDelta, 1e-4f);
            string text = _active > 0 ? $"{fps:0} FPS  ·  limit {_active}" : $"{fps:0} FPS";
            var style = new GUIStyle(ModUI.HudText) { fontSize = 13 };
            float w = style.CalcSize(new GUIContent(text)).x + 24f;
            var r = new Rect(16f, ModUI.ScreenHeight - 44f, w, 28f);
            ModUI.HudCard(r, 14f);
            ModUI.ShadowLabel(r, text, Color.white, new GUIStyle(style) { alignment = TextAnchor.MiddleCenter });
        }

        private void DrawToast()
        {
            float alpha = Mathf.Clamp01((_toastUntil - Time.unscaledTime) * 3f);
            var style = new GUIStyle(ModUI.HudText) { fontSize = 15, alignment = TextAnchor.MiddleCenter };
            float w = style.CalcSize(new GUIContent(_toast)).x + 36f;
            var r = new Rect((ModUI.ScreenWidth - w) * 0.5f, 64f, w, 36f);
            ModUI.HudCard(r, 18f, alpha);
            ModUI.ShadowLabel(r, _toast, new Color(1f, 1f, 1f, alpha), style);
        }
    }
}
