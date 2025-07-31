using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using CryStar.Utility.Enum;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CryStar.Utility
{
    /// <summary>
    /// ログUtility テスト運用中
    /// </summary>
    public class LogUtility : MonoBehaviour
    {
        #region Settings
        
        /// <summary>
        /// 出力する最小ログレベル。これより低いレベルのログは出力されない
        /// </summary>
        public static LogLevel MinLogLevel { get; set; } = LogLevel.Debug;
        
        /// <summary>
        /// ファイルへのログ出力を有効にするか
        /// 有効時はpersistentDataPath/Logsフォルダに日付別でログファイルを保存
        /// </summary>
        public static bool IsFileLoggingEnabled { get; set; }
        
        /// <summary>
        /// スタックトレース情報の出力を有効にするか
        /// 有効時はWarning以上のログでコールスタックを表示
        /// </summary>
        public static bool IsStackTraceEnabled { get; set; }
        
        /// <summary>
        /// ログにタイムスタンプを付与するか
        /// </summary>
        public static bool IsTimestampEnabled { get; set; } = true;
        
        /// <summary>
        /// カスタムカラー設定を使用するか（falseの場合はデフォルトカラーを使用）
        /// </summary>
        public static bool UseCustomColors { get; set; } = true;
        
        /// <summary>
        /// パフォーマンス測定ログの出力を有効にするか
        /// </summary>
        public static bool IsPerformanceLoggingEnabled { get; set; } = true;
        
        private static readonly Dictionary<LogCategory, bool> CategoryEnabled = new()
        {
            { LogCategory.General, true },
            { LogCategory.System, true },
            { LogCategory.Gameplay, true },
            { LogCategory.UI, true },
            { LogCategory.Audio, true },
            { LogCategory.Network, true },
            { LogCategory.Performance, true },
            { LogCategory.Test, true },
            { LogCategory.Debug, true }
        };
        
        /// <summary>
        /// ログファイルを出力する場所のパス
        /// </summary>
        private static string LogFilePath => Path.Combine(Application.persistentDataPath, "Logs", $"game_log_{DateTime.Now:yyyy-MM-dd}.txt");
        #endregion
        
        #region Color Constants
        private const string COLOR_ERROR = "red";
        private const string COLOR_WARNING = "yellow";
        private const string COLOR_INFO = "cyan";
        private const string COLOR_DEBUG = "white";
        private const string COLOR_VERBOSE = "gray";
        private const string COLOR_PERFORMANCE = "green";
        private const string COLOR_NETWORK = "orange";
        #endregion

        #region Public API Methods
        
        /// <summary>
        /// 詳細ログ出力
        /// </summary>
        public static void Verbose(object message, LogCategory category = LogCategory.General, Object context = null)
        {
            Log(LogLevel.Verbose, category, message, context);
        }

        /// <summary>
        /// デバッグログ出力
        /// </summary>
        public static void Debug(object message, LogCategory category = LogCategory.Debug, Object context = null)
        {
            Log(LogLevel.Debug, category, message, context);
        }

        /// <summary>
        /// 情報ログ出力
        /// </summary>
        public static void Info(object message, LogCategory category = LogCategory.General, Object context = null)
        {
            Log(LogLevel.Info, category, message, context);
        }

        /// <summary>
        /// 警告ログ出力
        /// </summary>
        public static void Warning(object message, LogCategory category = LogCategory.General, Object context = null)
        {
            Log(LogLevel.Warning, category, message, context);
        }

        /// <summary>
        /// エラーログ出力
        /// </summary>
        public static void Error(object message, LogCategory category = LogCategory.General, Object context = null)
        {
            Log(LogLevel.Error, category, message, context);
        }

        /// <summary>
        /// 致命的エラーログ出力
        /// </summary>
        public static void Fatal(object message, LogCategory category = LogCategory.General, Object context = null)
        {
            Log(LogLevel.Fatal, category, message, context);
        }

        /// <summary>
        /// クラス名つきでログを出力
        /// </summary>
        public static void DebugLog<T>(object message, LogLevel level = LogLevel.Debug, Object context = null)
        {
            Log(level, LogCategory.Debug, $"[{typeof(T).Name}] {message}", context);
        }

        /// <summary>
        /// パフォーマンス測定ログ
        /// </summary>
        public static void LogPerformance(string operationName, float timeMs)
        {
            if (!IsPerformanceLoggingEnabled) return;
            
            string message = $"⏱️ {operationName}: {timeMs:F2}ms";
            Log(LogLevel.Info, LogCategory.Performance, message);
        }

        /// <summary>
        /// フレームレート警告
        /// </summary>
        public static void LogFrameRateWarning(float currentFPS, float targetFPS)
        {
            if (currentFPS < targetFPS * 0.8f) // 80%を下回ったら警告
            {
                Warning($"🚨 Frame rate drop detected! Current: {currentFPS:F1} FPS, Target: {targetFPS:F1} FPS", LogCategory.Performance);
            }
        }

        // NOTE: 今回のプロジェクトでは使用しないかも
        // /// <summary>
        // /// ネットワーク関連ログ
        // /// </summary>
        // public static void LogNetwork(string operation, bool success, string details = "")
        // {
        //     string status = success ? "✅ SUCCESS" : "❌ FAILED";
        //     string message = $"🌐 {operation}: {status}";
        //     if (!string.IsNullOrEmpty(details))
        //     {
        //         message += $" - {details}";
        //     }
        //     
        //     LogLevel level = success ? LogLevel.Info : LogLevel.Warning;
        //     Log(level, LogCategory.Network, message);
        // }

        /// <summary>
        /// ゲームプレイイベントログ
        /// </summary>
        public static void LogGameplayEvent(string eventName, params object[] parameters)
        {
            var sb = new StringBuilder($"🎮 {eventName}");
            if (parameters?.Length > 0)
            {
                sb.Append(" | Params: ");
                sb.Append(string.Join(", ", parameters));
            }
            
            Log(LogLevel.Info, LogCategory.Gameplay, sb.ToString());
        }

        /// <summary>
        /// UI操作ログ
        /// </summary>
        public static void LogUIAction(string action, string elementName, object additionalData = null)
        {
            string message = $"🖱️ UI Action: {action} on {elementName}";
            if (additionalData != null)
            {
                message += $" | Data: {additionalData}";
            }
            
            Log(LogLevel.Debug, LogCategory.UI, message);
        }
        #endregion

        #region Configuration Methods
        /// <summary>
        /// カテゴリ有効/無効設定
        /// </summary>
        public static void SetCategoryEnabled(LogCategory category, bool enabled)
        {
            CategoryEnabled[category] = enabled;
        }

        /// <summary>
        /// 全カテゴリ有効/無効設定
        /// </summary>
        public static void SetAllCategoriesEnabled(bool enabled)
        {
            var keys = new List<LogCategory>(CategoryEnabled.Keys);
            foreach (var key in keys)
            {
                CategoryEnabled[key] = enabled;
            }
        }

        /// <summary>
        /// リリースビルド用設定
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void ConfigureForDevelopment()
        {
            MinLogLevel = LogLevel.Verbose;
            IsFileLoggingEnabled = true;
            IsStackTraceEnabled = true;
            IsPerformanceLoggingEnabled = true;
        }

        /// <summary>
        /// リリースビルド用設定
        /// </summary>
        public static void ConfigureForRelease()
        {
            MinLogLevel = LogLevel.Warning;
            IsFileLoggingEnabled = false;
            IsStackTraceEnabled = false;
            SetCategoryEnabled(LogCategory.Debug, false);
            SetCategoryEnabled(LogCategory.Test, false);
        }
        #endregion

        #region File Logging
        /// <summary>
        /// ファイルログ出力
        /// </summary>
        private static void WriteToFile(string message)
        {
            try
            {
                string logDir = Path.GetDirectoryName(LogFilePath);
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }

                string plainMessage = Regex.Replace(message, "<.*?>", "");
                File.AppendAllText(LogFilePath, $"{plainMessage}\n");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to write log to file: {ex.Message}");
            }
        }

        /// <summary>
        /// ログファイルクリア
        /// </summary>
        public static void ClearLogFile()
        {
            try
            {
                if (File.Exists(LogFilePath))
                {
                    File.Delete(LogFilePath);
                }
            }
            catch (Exception ex)
            {
                Error($"Failed to clear log file: {ex.Message}");
            }
        }
        #endregion

        #region Performance Helpers
        /// <summary>
        /// 処理時間測定用のDisposableクラス
        /// </summary>
        public class PerformanceScope : IDisposable
        {
            private readonly string _operationName;
            private readonly float _startTime;

            public PerformanceScope(string operationName)
            {
                _operationName = operationName;
                _startTime = Time.realtimeSinceStartup * 1000f;
            }

            public void Dispose()
            {
                float endTime = Time.realtimeSinceStartup * 1000f;
                LogPerformance(_operationName, endTime - _startTime);
            }
        }

        /// <summary>
        /// パフォーマンス測定スコープ作成
        /// 使用例: using (LogUtility.MeasurePerformance("Heavy Operation")) { /* 処理 */ }
        /// </summary>
        public static PerformanceScope MeasurePerformance(string operationName)
        {
            return new PerformanceScope(operationName);
        }
        #endregion

        #region Unity Lifecycle Integration
        /// <summary>
        /// Unity起動時の自動設定
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
#if UNITY_EDITOR
            ConfigureForDevelopment();
#else
            ConfigureForRelease();
#endif
            Info("LogUtility initialized", LogCategory.System);
        }
        #endregion
        
        #region ログ出力のコアメソッド
        /// <summary>
        /// 基本ログ出力メソッド
        /// </summary>
        private static void Log(LogLevel level, LogCategory category, object message, Object context = null)
        {
            if (!ShouldLog(level, category)) return;

            // フォーマットに合わせてメッセージを整形
            string formattedMessage = FormatMessage(level, category, message);

            switch (level)
            {
                case LogLevel.Error:
                case LogLevel.Fatal:
                    UnityEngine.Debug.LogError(formattedMessage, context);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(formattedMessage, context);
                    break;
                default:
                    UnityEngine.Debug.Log(formattedMessage, context);
                    break;
            }

            if (IsFileLoggingEnabled)
            {
                WriteToFile(formattedMessage);
            }
        }
        
        /// <summary>
        /// ログ出力判定
        /// </summary>
        private static bool ShouldLog(LogLevel level, LogCategory category)
        {
            return level >= MinLogLevel && CategoryEnabled.GetValueOrDefault(category, true);
        }
        
        /// <summary>
        /// メッセージフォーマット
        /// </summary>
        private static string FormatMessage(LogLevel level, LogCategory category, object message)
        {
            var sb = new StringBuilder();
            
            if (IsTimestampEnabled)
            {
                sb.Append($"[{DateTime.Now:HH:mm:ss}] ");
            }
            
            string color = GetColorForLevel(level, category);
            string levelStr = level.ToString().ToUpper();
            string categoryStr = category.ToString().ToUpper();
            
            sb.Append($"<color={color}><b>[{levelStr}][{categoryStr}]</b></color> ");
            sb.Append(message);
            
            if (IsStackTraceEnabled && level >= LogLevel.Warning)
            {
                var stackTrace = new StackTrace(3, true);
                sb.AppendLine($"\nStack Trace: {stackTrace}");
            }
            
            return sb.ToString();
        }
        
        /// <summary>
        /// ログレベル・カテゴリに応じた色を取得
        /// </summary>
        private static string GetColorForLevel(LogLevel level, LogCategory category)
        {
            // カスタムカラー設定が有効な場合
            if (UseCustomColors)
            {
                // まずログレベルの色を優先
                if (LogColorSettings.LevelColors.ContainsKey(level))
                {
                    return LogColorSettings.GetLevelColorHtml(level);
                }
        
                // ログレベルの色が設定されていない場合はカテゴリの色を使用
                if (LogColorSettings.CategoryColors.ContainsKey(category))
                {
                    return LogColorSettings.GetCategoryColorHtml(category);
                }
            }
    
            // フォールバック: デフォルトの色設定を使用
            return level switch
            {
                LogLevel.Fatal or LogLevel.Error => COLOR_ERROR,
                LogLevel.Warning => COLOR_WARNING,
                LogLevel.Info => COLOR_INFO,
                LogLevel.Debug => COLOR_DEBUG,
                LogLevel.Verbose => COLOR_VERBOSE,
                _ => category switch
                {
                    LogCategory.Performance => COLOR_PERFORMANCE,
                    LogCategory.Network => COLOR_NETWORK,
                    _ => COLOR_DEBUG
                }
            };
        }
        #endregion
    }
}