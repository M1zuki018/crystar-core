#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Collections.Generic;
using System.Reflection;
using CryStar.Utility;
using CryStar.Utility.Enum;

namespace CryStar.Editor
{
    /// <summary>
    /// LogUtilityの設定を管理するエディター拡張ウィンドウ
    /// </summary>
    public class LogUtilitySettingsWindow : EditorWindow
    {
        #region Private Fields
        private Vector2 _scrollPosition;
        private bool _showAdvancedSettings = false;
        private bool _showCategorySettings = true;
        private bool _showFileSettings = false;
        private bool _showTestSection = false;
        private bool _showColorSettings = false;
        
        // テスト用の一時的な値
        private string _testMessage = "Test log message";
        private LogLevel _testLogLevel = LogLevel.Info;
        private LogCategory _testCategory = LogCategory.Debug;
        
        // カラー設定用
        private ColorTheme _selectedTheme = ColorTheme.Default;
        
        // UI設定
        private const float LABEL_WIDTH = 180f;
        private const float BUTTON_HEIGHT = 25f;
        private const float SECTION_SPACING = 10f;
        private const float COLOR_FIELD_WIDTH = 60f;
        #endregion

        #region Unity Menu Integration
        [MenuItem("CryStar/Utility/Log Utility Settings")]
        public static void ShowWindow()
        {
            var window = GetWindow<LogUtilitySettingsWindow>("Log Settings");
            window.minSize = new Vector2(400f, 500f);
            window.Show();
        }
        #endregion

        #region Unity Lifecycle
        private void OnEnable()
        {
            // ウィンドウが開かれた時の初期化
            titleContent = new GUIContent("Log Settings", EditorGUIUtility.IconContent("console.infoicon").image);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(5f);
            
            // ヘッダー
            DrawHeader();
            
            // スクロール開始
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            // 基本設定セクション
            DrawBasicSettings();
            
            // カテゴリ設定セクション
            DrawCategorySettings();
            
            // 色設定セクション
            DrawColorSettings();
            
            // ファイル設定セクション
            DrawFileSettings();
            
            // 高度な設定セクション
            DrawAdvancedSettings();
            
            // テストセクション
            DrawTestSection();
            
            // プリセット設定セクション
            DrawPresetSettings();
            
            EditorGUILayout.EndScrollView();
            
            // フッター
            DrawFooter();
        }
        #endregion

        #region UI Drawing Methods
        private void DrawHeader()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            
            // タイトル
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("🛠️ LogUtility Settings", titleStyle);
            
            EditorGUILayout.Space(3f);
            
            // 現在の状態表示
            GUIStyle statusStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Application.isPlaying ? Color.green : Color.gray }
            };
            string status = Application.isPlaying ? "● Runtime Active" : "○ Editor Only";
            EditorGUILayout.LabelField(status, statusStyle);
            
            GUILayout.EndVertical();
            EditorGUILayout.Space(SECTION_SPACING);
        }

        private void DrawBasicSettings()
        {
            DrawSectionHeader("🔧 基本設定", ref _showAdvancedSettings);
            
            if (_showAdvancedSettings)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                
                // 最小ログレベル
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("出力する最小のログレベル", GUILayout.Width(LABEL_WIDTH));
                LogUtility.MinLogLevel = (LogLevel)EditorGUILayout.EnumPopup(LogUtility.MinLogLevel);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(3f);
                
                // タイムスタンプ
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("タイムスタンプを有効にする", GUILayout.Width(LABEL_WIDTH));
                LogUtility.IsTimestampEnabled = EditorGUILayout.Toggle(LogUtility.IsTimestampEnabled);
                EditorGUILayout.EndHorizontal();
                
                // スタックトレース
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("スタックトレースを有効にする", GUILayout.Width(LABEL_WIDTH));
                LogUtility.IsStackTraceEnabled = EditorGUILayout.Toggle(LogUtility.IsStackTraceEnabled);
                EditorGUILayout.EndHorizontal();
                
                // パフォーマンスログ
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("パフォーマンスログを出力する", GUILayout.Width(LABEL_WIDTH));
                LogUtility.IsPerformanceLoggingEnabled = EditorGUILayout.Toggle(LogUtility.IsPerformanceLoggingEnabled);
                EditorGUILayout.EndHorizontal();
                
                GUILayout.EndVertical();
            }
            
            EditorGUILayout.Space(SECTION_SPACING);
        }

        private void DrawCategorySettings()
        {
            DrawSectionHeader("📂 カテゴリごとの設定", ref _showCategorySettings);
            
            if (_showCategorySettings)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                
                // 全カテゴリ一括設定
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Enable All", GUILayout.Height(BUTTON_HEIGHT)))
                {
                    LogUtility.SetAllCategoriesEnabled(true);
                }
                if (GUILayout.Button("Disable All", GUILayout.Height(BUTTON_HEIGHT)))
                {
                    LogUtility.SetAllCategoriesEnabled(false);
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(5f);
                
                // 各カテゴリの設定
                var categories = Enum.GetValues(typeof(LogCategory));
                foreach (LogCategory category in categories)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    // カテゴリアイコン
                    string icon = GetCategoryIcon(category);
                    EditorGUILayout.LabelField($"{icon} {category}", GUILayout.Width(LABEL_WIDTH));
                    
                    // 現在の状態を取得（リフレクションを使用）
                    bool currentState = GetCategoryEnabled(category);
                    bool newState = EditorGUILayout.Toggle(currentState);
                    
                    if (newState != currentState)
                    {
                        LogUtility.SetCategoryEnabled(category, newState);
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                GUILayout.EndVertical();
            }
            
            EditorGUILayout.Space(SECTION_SPACING);
        }
        
        private void DrawColorSettings()
        {
            DrawSectionHeader("🎨 色の設定", ref _showColorSettings);
            
            if (_showColorSettings)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                
                // カラーテーマ選択
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Color Theme:", GUILayout.Width(LABEL_WIDTH));
                ColorTheme newTheme = (ColorTheme)EditorGUILayout.EnumPopup(_selectedTheme);
                if (newTheme != _selectedTheme)
                {
                    _selectedTheme = newTheme;
                    LogColorSettings.ApplyColorTheme(_selectedTheme);
                    ShowNotification(new GUIContent($"Applied {_selectedTheme} theme!"));
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(5f);
                
                // リセットボタン
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("🔄 デフォルトにリセット", GUILayout.Height(BUTTON_HEIGHT)))
                {
                    LogColorSettings.ResetToDefaults();
                    _selectedTheme = ColorTheme.Default;
                    ShowNotification(new GUIContent("色の設定をデフォルトにリセットしました！"));
                }
                if (GUILayout.Button("🎨 カスタムカラーを適用", GUILayout.Height(BUTTON_HEIGHT)))
                {
                    ShowNotification(new GUIContent("カスタムカラーを適用しました！"));
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(8f);
                
                // ログレベル色設定
                EditorGUILayout.LabelField("📊 Log Level Colors:", EditorStyles.boldLabel);
                
                foreach (LogLevel level in Enum.GetValues(typeof(LogLevel)))
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    // ログレベル名
                    string levelIcon = LogColorSettings.LogLevelIcon(level);
                    EditorGUILayout.LabelField($"{levelIcon} {level}", GUILayout.Width(LABEL_WIDTH));
                    
                    // 現在の色表示
                    Color currentColor = LogColorSettings.LevelColors[level];
                    Color newColor = EditorGUILayout.ColorField(GUIContent.none, currentColor, 
                        false, false, false, GUILayout.Width(COLOR_FIELD_WIDTH));
                    
                    if (newColor != currentColor)
                    {
                        LogColorSettings.SetLevelColor(level, newColor);
                    }
                    
                    // プレビュー
                    GUIStyle previewStyle = new GUIStyle(EditorStyles.label);
                    previewStyle.normal.textColor = currentColor;
                    previewStyle.fontStyle = FontStyle.Bold;
                    EditorGUILayout.LabelField($"Sample {level} text", previewStyle, GUILayout.ExpandWidth(true));
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.Space(8f);
                
                // ログカテゴリ色設定
                EditorGUILayout.LabelField("📂 Log Category Colors:", EditorStyles.boldLabel);
                
                foreach (LogCategory category in Enum.GetValues(typeof(LogCategory)))
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    // カテゴリ名
                    string categoryIcon = GetCategoryIcon(category);
                    EditorGUILayout.LabelField($"{categoryIcon} {category}", GUILayout.Width(LABEL_WIDTH));
                    
                    // 現在の色表示
                    Color currentColor = LogColorSettings.CategoryColors[category];
                    Color newColor = EditorGUILayout.ColorField(GUIContent.none, currentColor, 
                        false, false, false, GUILayout.Width(COLOR_FIELD_WIDTH));
                    
                    if (newColor != currentColor)
                    {
                        LogColorSettings.SetCategoryColor(category, newColor);
                    }
                    
                    // プレビュー
                    GUIStyle previewStyle = new GUIStyle(EditorStyles.label);
                    previewStyle.normal.textColor = currentColor;
                    previewStyle.fontStyle = FontStyle.Bold;
                    EditorGUILayout.LabelField($"Sample {category} text", previewStyle, GUILayout.ExpandWidth(true));
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.Space(5f);
                
                // カラープレビューセクション
                EditorGUILayout.LabelField("🎭 Color Preview:", EditorStyles.boldLabel);
                GUILayout.BeginVertical(EditorStyles.textArea);
                
                // 各レベルでのプレビュー表示
                foreach (LogLevel level in Enum.GetValues(typeof(LogLevel)))
                {
                    GUIStyle previewStyle = new GUIStyle(EditorStyles.label);
                    previewStyle.normal.textColor = LogColorSettings.LevelColors[level];
                    previewStyle.fontStyle = FontStyle.Bold;
                    
                    string previewText = $"[{DateTime.Now:HH:mm:ss}] [{level.ToString().ToUpper()}][{_testCategory.ToString().ToUpper()}] Sample {level} log message";
                    EditorGUILayout.LabelField(previewText, previewStyle);
                }
                
                GUILayout.EndVertical();
                
                GUILayout.EndVertical();
            }
            
            EditorGUILayout.Space(SECTION_SPACING);
        }


        private void DrawFileSettings()
        {
            DrawSectionHeader("💾 ファイルログの設定", ref _showFileSettings);
            
            if (_showFileSettings)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                
                // ファイルログ有効/無効
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("ファイルログを有効にする", GUILayout.Width(LABEL_WIDTH));
                LogUtility.IsFileLoggingEnabled = EditorGUILayout.Toggle(LogUtility.IsFileLoggingEnabled);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(5f);
                
                // ファイルパス表示
                string logPath = Path.Combine(Application.persistentDataPath, "Logs");
                EditorGUILayout.LabelField("Log Directory:", EditorStyles.boldLabel);
                EditorGUILayout.SelectableLabel(logPath, EditorStyles.textField, GUILayout.Height(18f));
                
                EditorGUILayout.Space(3f);
                
                // ファイル操作ボタン
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Open Log Folder", GUILayout.Height(BUTTON_HEIGHT)))
                {
                    OpenLogFolder();
                }
                if (GUILayout.Button("Clear Log File", GUILayout.Height(BUTTON_HEIGHT)))
                {
                    if (EditorUtility.DisplayDialog("Clear Log File", "Are you sure you want to clear the current log file?", "Clear", "Cancel"))
                    {
                        LogUtility.ClearLogFile();
                        ShowNotification(new GUIContent("Log file cleared!"));
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                GUILayout.EndVertical();
            }
            
            EditorGUILayout.Space(SECTION_SPACING);
        }

        private void DrawAdvancedSettings()
        {
            // 高度な設定は基本設定に統合済み
        }

        private void DrawTestSection()
        {
            DrawSectionHeader("🧪 テスト用ログを出力する", ref _showTestSection);
            
            if (_showTestSection)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                
                // テストメッセージ
                EditorGUILayout.LabelField("Test Message:", EditorStyles.boldLabel);
                _testMessage = EditorGUILayout.TextField(_testMessage);
                
                EditorGUILayout.Space(3f);
                
                // ログレベル選択
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Log Level:", GUILayout.Width(80f));
                _testLogLevel = (LogLevel)EditorGUILayout.EnumPopup(_testLogLevel);
                EditorGUILayout.EndHorizontal();
                
                // カテゴリ選択
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Category:", GUILayout.Width(80f));
                _testCategory = (LogCategory)EditorGUILayout.EnumPopup(_testCategory);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(5f);
                
                // テスト実行ボタン
                if (GUILayout.Button("🚀 ログを送信", GUILayout.Height(BUTTON_HEIGHT * 1.2f)))
                {
                    SendTestLog();
                }
                
                EditorGUILayout.Space(3f);
                
                // 各レベルのクイックテスト
                EditorGUILayout.LabelField("Quick Tests:", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Verbose", GUILayout.Height(BUTTON_HEIGHT)))
                    LogUtility.Verbose("Test verbose message", _testCategory);
                if (GUILayout.Button("Debug", GUILayout.Height(BUTTON_HEIGHT)))
                    LogUtility.Debug("Test debug message", _testCategory);
                if (GUILayout.Button("Info", GUILayout.Height(BUTTON_HEIGHT)))
                    LogUtility.Info("Test info message", _testCategory);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Warning", GUILayout.Height(BUTTON_HEIGHT)))
                    LogUtility.Warning("Test warning message", _testCategory);
                if (GUILayout.Button("Error", GUILayout.Height(BUTTON_HEIGHT)))
                    LogUtility.Error("Test error message", _testCategory);
                if (GUILayout.Button("Fatal", GUILayout.Height(BUTTON_HEIGHT)))
                    LogUtility.Fatal("Test fatal message", _testCategory);
                EditorGUILayout.EndHorizontal();
                
                GUILayout.EndVertical();
            }
            
            EditorGUILayout.Space(SECTION_SPACING);
        }

        private void DrawPresetSettings()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("⚙️ 設定プリセット", EditorStyles.boldLabel);
            EditorGUILayout.Space(3f);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🔧 開発用", GUILayout.Height(BUTTON_HEIGHT)))
            {
                ApplyDevelopmentConfig();
                ShowNotification(new GUIContent("開発用のプリセットを適用しました！"));
            }
            if (GUILayout.Button("🚀 リリース用", GUILayout.Height(BUTTON_HEIGHT)))
            {
                if (EditorUtility.DisplayDialog("リリース用のプリセットを適用しました！", "これにより、デバッグログの記録が無効化されます。続行しますか？", "Apply", "Cancel"))
                {
                    LogUtility.ConfigureForRelease();
                    ShowNotification(new GUIContent("リリース構成が適用されました！"));
                }
            }
            EditorGUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
        }

        private void DrawFooter()
        {
            EditorGUILayout.Space(SECTION_SPACING);
            
            GUILayout.BeginVertical(EditorStyles.helpBox);
            
            GUIStyle footerStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.gray }
            };
            
            EditorGUILayout.LabelField("LogUtility Settings v1.0 | Settings are applied immediately", footerStyle);
            
            GUILayout.EndVertical();
        }

        private void DrawSectionHeader(string title, ref bool isExpanded)
        {
            GUIStyle headerStyle = new GUIStyle(EditorStyles.foldout)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };
            
            isExpanded = EditorGUILayout.Foldout(isExpanded, title, headerStyle);
        }
        #endregion

        #region Helper Methods
        private string GetCategoryIcon(LogCategory category)
        {
            return category switch
            {
                LogCategory.General => "📝",
                LogCategory.System => "⚙️",
                LogCategory.Gameplay => "🎮",
                LogCategory.UI => "🖼️",
                LogCategory.Audio => "🔊",
                LogCategory.Network => "🌐",
                LogCategory.Performance => "⚡",
                LogCategory.Test => "🧪",
                LogCategory.Debug => "🐛",
                _ => "📄"
            };
        }

        private bool GetCategoryEnabled(LogCategory category)
        {
            // リフレクションを使ってCategoryEnabledフィールドにアクセス
            var field = typeof(LogUtility).GetField("CategoryEnabled", 
                BindingFlags.NonPublic | BindingFlags.Static);
            
            if (field?.GetValue(null) is Dictionary<LogCategory, bool> dict)
            {
                return dict.GetValueOrDefault(category, true);
            }
            
            return true; // デフォルト値
        }

        private void SendTestLog()
        {
            string message = $"{_testMessage} (Test from Editor)";
            
            switch (_testLogLevel)
            {
                case LogLevel.Verbose:
                    LogUtility.Verbose(message, _testCategory);
                    break;
                case LogLevel.Debug:
                    LogUtility.Debug(message, _testCategory);
                    break;
                case LogLevel.Info:
                    LogUtility.Info(message, _testCategory);
                    break;
                case LogLevel.Warning:
                    LogUtility.Warning(message, _testCategory);
                    break;
                case LogLevel.Error:
                    LogUtility.Error(message, _testCategory);
                    break;
                case LogLevel.Fatal:
                    LogUtility.Fatal(message, _testCategory);
                    break;
            }
            
            ShowNotification(new GUIContent($"Sent {_testLogLevel} log!"));
        }

        private void ApplyDevelopmentConfig()
        {
            LogUtility.MinLogLevel = LogLevel.Verbose;
            LogUtility.IsFileLoggingEnabled = true;
            LogUtility.IsStackTraceEnabled = true;
            LogUtility.IsPerformanceLoggingEnabled = true;
            LogUtility.SetAllCategoriesEnabled(true);
        }

        private void OpenLogFolder()
        {
            string logPath = Path.Combine(Application.persistentDataPath, "Logs");
            
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
            
            EditorUtility.RevealInFinder(logPath);
        }
        #endregion
    }
}
#endif
