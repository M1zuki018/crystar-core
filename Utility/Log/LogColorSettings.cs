using System;
using System.Collections.Generic;
using System.Linq;
using CryStar.Utility;
using CryStar.Utility.Enum;
using UnityEngine;

namespace CryStar.Utility
{
    /// <summary>
    /// ログ色設定管理クラス
    /// </summary>
    public static class LogColorSettings
    {
        // デフォルトカラー設定
        private static readonly Dictionary<LogLevel, Color> DefaultLevelColors = new()
        {
            { LogLevel.Verbose, new Color(0.7f, 0.7f, 0.7f, 1f) }, // Gray
            { LogLevel.Debug, new Color(1f, 1f, 1f, 1f) }, // White
            { LogLevel.Info, new Color(0f, 1f, 1f, 1f) }, // Cyan
            { LogLevel.Warning, new Color(1f, 1f, 0f, 1f) }, // Yellow
            { LogLevel.Error, new Color(1f, 0f, 0f, 1f) }, // Red
            { LogLevel.Fatal, new Color(1f, 0f, 1f, 1f) } // Magenta
        };
            
        private static string GetLogLevelIcon(LogLevel level)
        {
            return level switch
            {
                LogLevel.Verbose => "💬",
                LogLevel.Debug => "🐛",
                LogLevel.Info => "ℹ️",
                LogLevel.Warning => "⚠️",
                LogLevel.Error => "❌",
                LogLevel.Fatal => "💀",
                _ => "📝"
            };
        }
        public static string LogLevelIcon(LogLevel level) => GetLogLevelIcon(level);

        private static readonly Dictionary<LogCategory, Color> DefaultCategoryColors = new()
        {
            { LogCategory.General, new Color(1f, 1f, 1f, 1f) },         // White
            { LogCategory.System, new Color(0f, 1f, 0f, 1f) },          // Green
            { LogCategory.Gameplay, new Color(0f, 0.5f, 1f, 1f) },      // Blue
            { LogCategory.UI, new Color(1f, 0.5f, 0f, 1f) },            // Orange
            { LogCategory.Audio, new Color(1f, 0f, 1f, 1f) },           // Magenta
            { LogCategory.Network, new Color(1f, 0.5f, 0f, 1f) },       // Orange
            { LogCategory.Performance, new Color(0f, 1f, 0f, 1f) },     // Green
            { LogCategory.Test, new Color(1f, 1f, 0f, 1f) },            // Yellow
            { LogCategory.Debug, new Color(0.5f, 0.5f, 1f, 1f) }        // Light Blue
        };

        // 現在の色設定（実行時に変更可能）
        private static Dictionary<LogLevel, Color> _currentLevelColors;
        private static Dictionary<LogCategory, Color> _currentCategoryColors;

        // プロパティ
        public static Dictionary<LogLevel, Color> LevelColors
        {
            get
            {
                _currentLevelColors ??= new Dictionary<LogLevel, Color>(DefaultLevelColors);
                return _currentLevelColors;
            }
        }

        public static Dictionary<LogCategory, Color> CategoryColors
        {
            get
            {
                _currentCategoryColors ??= new Dictionary<LogCategory, Color>(DefaultCategoryColors);
                return _currentCategoryColors;
            }
        }

        /// <summary>
        /// ログレベルの色を設定
        /// </summary>
        public static void SetLevelColor(LogLevel level, Color color)
        {
            LevelColors[level] = color;
        }

        /// <summary>
        /// ログカテゴリの色を設定
        /// </summary>
        public static void SetCategoryColor(LogCategory category, Color color)
        {
            CategoryColors[category] = color;
        }

        /// <summary>
        /// 色をHTML色文字列に変換
        /// </summary>
        public static string ColorToHtml(Color color)
        {
            return $"#{ColorUtility.ToHtmlStringRGB(color)}";
        }

        /// <summary>
        /// ログレベルの色をHTML文字列で取得
        /// </summary>
        public static string GetLevelColorHtml(LogLevel level)
        {
            return ColorToHtml(LevelColors.GetValueOrDefault(level, Color.white));
        }

        /// <summary>
        /// ログカテゴリの色をHTML文字列で取得
        /// </summary>
        public static string GetCategoryColorHtml(LogCategory category)
        {
            return ColorToHtml(CategoryColors.GetValueOrDefault(category, Color.white));
        }

        /// <summary>
        /// デフォルト色にリセット
        /// </summary>
        public static void ResetToDefaults()
        {
            _currentLevelColors = new Dictionary<LogLevel, Color>(DefaultLevelColors);
            _currentCategoryColors = new Dictionary<LogCategory, Color>(DefaultCategoryColors);
        }

        /// <summary>
        /// プリセットカラーテーマを適用
        /// </summary>
        public static void ApplyColorTheme(ColorTheme theme)
        {
            switch (theme)
            {
                case ColorTheme.Dark:
                    ApplyDarkTheme();
                    break;
                case ColorTheme.Light:
                    ApplyLightTheme();
                    break;
                case ColorTheme.Colorful:
                    ApplyColorfulTheme();
                    break;
                case ColorTheme.Minimal:
                    ApplyMinimalTheme();
                    break;
                default:
                    ResetToDefaults();
                    break;
            }
        }

        private static void ApplyDarkTheme()
        {
            LevelColors[LogLevel.Verbose] = new Color(0.5f, 0.5f, 0.5f, 1f);
            LevelColors[LogLevel.Debug] = new Color(0.8f, 0.8f, 0.8f, 1f);
            LevelColors[LogLevel.Info] = new Color(0.4f, 0.8f, 1f, 1f);
            LevelColors[LogLevel.Warning] = new Color(1f, 0.8f, 0.2f, 1f);
            LevelColors[LogLevel.Error] = new Color(1f, 0.3f, 0.3f, 1f);
            LevelColors[LogLevel.Fatal] = new Color(1f, 0.2f, 0.8f, 1f);
        }

        private static void ApplyLightTheme()
        {
            LevelColors[LogLevel.Verbose] = new Color(0.6f, 0.6f, 0.6f, 1f);
            LevelColors[LogLevel.Debug] = new Color(0.2f, 0.2f, 0.2f, 1f);
            LevelColors[LogLevel.Info] = new Color(0.2f, 0.4f, 0.8f, 1f);
            LevelColors[LogLevel.Warning] = new Color(0.8f, 0.6f, 0f, 1f);
            LevelColors[LogLevel.Error] = new Color(0.8f, 0.2f, 0.2f, 1f);
            LevelColors[LogLevel.Fatal] = new Color(0.6f, 0f, 0.4f, 1f);
        }

        private static void ApplyColorfulTheme()
        {
            LevelColors[LogLevel.Verbose] = new Color(0.7f, 0.5f, 1f, 1f);      // Purple
            LevelColors[LogLevel.Debug] = new Color(0.5f, 1f, 0.5f, 1f);        // Light Green
            LevelColors[LogLevel.Info] = new Color(0.5f, 0.8f, 1f, 1f);         // Sky Blue
            LevelColors[LogLevel.Warning] = new Color(1f, 0.7f, 0.3f, 1f);      // Orange
            LevelColors[LogLevel.Error] = new Color(1f, 0.4f, 0.4f, 1f);        // Light Red
            LevelColors[LogLevel.Fatal] = new Color(1f, 0.2f, 0.6f, 1f);        // Pink
        }

        private static void ApplyMinimalTheme()
        {
            Color baseColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            foreach (var level in System.Enum.GetValues(typeof(LogLevel)).Cast<LogLevel>())
            {
                LevelColors[level] = baseColor;
            }
        }
    }
}