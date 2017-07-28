using ACT.MPTimer.Properties;

namespace ACT.MPTimer
{
    public static class Constants
    {
        /// <summary>
        /// MP回復周期
        /// エオルゼアタイムの1分とは無関係に実時間3秒周期である
        /// </summary>
        public static readonly double MPRecoverySpan = Settings.Default.MPRecoveryInterval;

        /// <summary>
        /// MP回復割合
        /// </summary>
        public static class MPRecoveryRate
        {
            /// <summary>
            /// 戦闘中
            /// </summary>
            public const double InCombat = 0.02d;

            /// <summary>
            /// 通常(非戦闘時)
            /// </summary>
            public const double Normal = 0.06d;

            /// <summary>
            /// アンブラルブリザード1による増量
            /// </summary>
            public const double UmbralIce1 = 0.30d;

            /// <summary>
            /// アンブラルブリザード2による増量
            /// </summary>
            public const double UmbralIce2 = 0.45d;

            /// <summary>
            /// アンブラルブリザード3による増量
            /// </summary>
            public const double UmbralIce3 = 0.60d;
        }
    }
}
