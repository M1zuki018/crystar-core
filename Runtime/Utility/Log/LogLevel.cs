namespace CryStar.Utility.Enum
{
    /// <summary>
    /// ログレベル定義
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// 最も詳細なログ。変数の値、細かい処理の流れなど開発時のデバッグ用
        /// </summary>
        Verbose = 0,
        
        /// <summary>
        /// デバッグ情報
        /// </summary>
        Debug = 1,
        
        /// <summary>
        /// 一般的な情報。ゲーム状態の変化、重要なイベントなど運用でも有用な情報
        /// </summary>
        Info = 2,
        
        /// <summary>
        /// 警告。問題の可能性があるが動作に支障がない状況
        /// </summary>
        Warning = 3,
        
        /// <summary>
        /// エラー。機能に影響するが致命的ではない問題
        /// </summary>
        Error = 4,
        
        /// <summary>
        /// 致命的エラー。アプリケーションの継続が困難な重大な問題
        /// </summary>
        Fatal = 5
    }

}
