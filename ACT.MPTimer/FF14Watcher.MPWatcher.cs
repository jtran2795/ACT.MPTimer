namespace ACT.MPTimer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using ACT.MPTimer.Properties;

    /// <summary>
    /// FF14を監視する MPウォッチャー
    /// </summary>
    public partial class FF14Watcher
    {
        /// <summary>
        /// 最後にログを出力した日時
        /// </summary>
        private DateTime lastLoggingDateTime;

        /// <summary>
        /// MP回復量を記録した辞書
        /// </summary>
        private Dictionary<int, int[]> MPRecoveryAmounts = new Dictionary<int, int[]>();

        /// <summary>
        /// ジョブフィルタによる対象ジョブID
        /// </summary>
        private int targetJobId = -1;

        /// <summary>
        /// ジョブフィルタによるMPタイマー及びエノキアンタイマーの有効性
        /// </summary>
        public bool EnabledByJobFilter
        {
            get
            {
                if (Settings.Default.TargetJobId == 0)
                {
                    return true;
                }

                if (this.LastPlayerInfo == null)
                {
#if !DEBUG
                    return false;
#else
                    return true;
#endif
                }

                if (this.targetJobId < 0)
                {
                    this.UpdateTargetJobId();
                }

                return this.LastPlayerInfo.Job == this.targetJobId;
            }
        }

        /// <summary>
        /// 最後にMPが満タンになった日時
        /// </summary>
        public DateTime LastMPFullDateTime { get; private set; }

        /// <summary>
        /// 最後に取得したプレイヤー情報
        /// </summary>
        public Combatant LastPlayerInfo { get; private set; }

        /// <summary>
        /// 最後に回復した日時
        /// </summary>
        public DateTime LastRecoveryDateTime { get; private set; }

        /// <summary>
        /// 次に回復するであろう日時
        /// </summary>
        public DateTime NextRecoveryDateTime { get; private set; }

        /// <summary>
        /// 直前のMP
        /// </summary>
        private int PreviousMP { get; set; }

        /// <summary>
        /// 対象ジョブIDを更新する
        /// </summary>
        public void UpdateTargetJobId()
        {
            this.targetJobId = Settings.Default.TargetJobId;
        }

        /// <summary>
        /// MP回復スパンを監視する
        /// </summary>
        public void WacthMPRecovery()
        {
            var vm = MPTimerWindow.Default.ViewModel;

            var player = FF14PluginHelper.GetCombatantPlayer();
            if (player == null)
            {
                vm.Visible = false;
                return;
            }
            else
            {
                // プレイヤー情報を保存する
                this.LastPlayerInfo = player;
            }

            // MPTimerが無効？
            if (!Settings.Default.EnabledMPTimer)
            {
                vm.Visible = false;
                return;
            }

            // ジョブ指定を設定する
            vm.Visible = this.EnabledByJobFilter;
            if (!vm.Visible)
            {
                return;
            }

            // 戦闘中のみ稼働させる？
            if (Settings.Default.CountInCombat)
            {
                // MPが満タンになった？
                if (player.CurrentMP > this.PreviousMP &&
                    player.CurrentMP >= player.MaxMP)
                {
                    this.LastMPFullDateTime = DateTime.Now;
                }

                // 現在がMP満タン状態？
                if (player.CurrentMP >= player.MaxMP ||
                    this.PreviousMP < 0)
                {
                    // 前回の満タンからn秒以上経過した？
                    if ((DateTime.Now - this.LastMPFullDateTime).TotalSeconds >=
                        Settings.Default.CountInCombatSpan)
                    {
                        vm.InCombat = false;
                    }
                }
                else
                {
                    vm.InCombat = true;
                }
            }

            // 自然回復による回復量を求める
            if (!this.MPRecoveryAmounts.ContainsKey(player.MaxMP))
            {
                var mpRecoveryValueNorml = (int)Math.Floor(player.MaxMP * Constants.MPRecoveryRate.Normal);
                var mpRecoveryValueInCombat = (int)Math.Floor(player.MaxMP * Constants.MPRecoveryRate.InCombat);
                var mpRecoveryValueUI1 = (int)Math.Floor(player.MaxMP * Constants.MPRecoveryRate.UmbralIce1);
                var mpRecoveryValueUI2 = (int)Math.Floor(player.MaxMP * Constants.MPRecoveryRate.UmbralIce2);
                var mpRecoveryValueUI3 = (int)Math.Floor(player.MaxMP * Constants.MPRecoveryRate.UmbralIce3);

                this.MPRecoveryAmounts[player.MaxMP] = new int[]
                {
                    mpRecoveryValueNorml,
                    mpRecoveryValueNorml + mpRecoveryValueUI1,
                    mpRecoveryValueNorml + mpRecoveryValueUI2,
                    mpRecoveryValueNorml + mpRecoveryValueUI3,
                    mpRecoveryValueInCombat,
                    mpRecoveryValueInCombat + mpRecoveryValueUI1,
                    mpRecoveryValueInCombat + mpRecoveryValueUI2,
                    mpRecoveryValueInCombat + mpRecoveryValueUI3,
                };
            }

            var mpRecoveryAmounts = this.MPRecoveryAmounts[player.MaxMP];

            var now = DateTime.Now;

            // MPが回復している？
            if (this.PreviousMP > -1 &&
                player.CurrentMP > this.PreviousMP)
            {
                // 今回の回復量を算出する
                var mpRecoveryValue = player.CurrentMP - this.PreviousMP;

                // 算出した回復量と一致する？
                if (mpRecoveryAmounts.Any(x => x == mpRecoveryValue))
                {
#if DEBUG
                    var interval = (now - this.LastRecoveryDateTime).TotalMilliseconds;
#endif
                    this.LastRecoveryDateTime = now;
                    this.NextRecoveryDateTime = this.LastRecoveryDateTime.AddSeconds(Constants.MPRecoverySpan);

#if DEBUG
                    Trace.WriteLine(string.Format("MPRecovery matched. MP={0:N0}, Interval={1:N0}", mpRecoveryValue, interval));
#endif
                }

                #region Logger

                // ログを出力する
                if ((now - this.lastLoggingDateTime).TotalMinutes >= 30.0d)
                {
                    var message = string.Empty;

                    message += "MaxMP " + player.MaxMP.ToString("N0") + Environment.NewLine;
                    message += "通常回復       : " + mpRecoveryAmounts[0].ToString("N0") + Environment.NewLine;
                    message += "通常回復+UB1   : " + mpRecoveryAmounts[1].ToString("N0") + Environment.NewLine;
                    message += "通常回復+UB2   : " + mpRecoveryAmounts[2].ToString("N0") + Environment.NewLine;
                    message += "通常回復+UB3   : " + mpRecoveryAmounts[3].ToString("N0") + Environment.NewLine;
                    message += "戦闘時回復     : " + mpRecoveryAmounts[4].ToString("N0") + Environment.NewLine;
                    message += "戦闘時回復+UB1 : " + mpRecoveryAmounts[5].ToString("N0") + Environment.NewLine;
                    message += "戦闘時回復+UB2 : " + mpRecoveryAmounts[6].ToString("N0") + Environment.NewLine;
                    message += "戦闘時回復+UB3 : " + mpRecoveryAmounts[7].ToString("N0") + Environment.NewLine;
                    message += "今回の回復量   : " + (player.CurrentMP - this.PreviousMP).ToString("N0");

                    Trace.WriteLine(message);
                    this.lastLoggingDateTime = now;
                }

                #endregion Logger
            }

            if (this.NextRecoveryDateTime <= DateTime.MinValue)
            {
                this.NextRecoveryDateTime = now.AddSeconds(Constants.MPRecoverySpan);
            }

            // 回復までの残り時間を算出する
            var remain = (this.NextRecoveryDateTime - now).TotalMilliseconds;

            // 回復までの時間が過ぎている？
            if (remain <= 0.0d)
            {
                this.LastRecoveryDateTime = now.AddMilliseconds(remain);
                this.NextRecoveryDateTime = this.LastRecoveryDateTime.AddSeconds(Constants.MPRecoverySpan);

#if DEBUG
                Trace.WriteLine(string.Format("MPRecovery was over. {0:N0}", remain));
#endif
            }

            if (remain < 0d)
            {
                remain = 0d;
            }

            // ViewModelにセットする
            vm.TimeToRecovery = remain;

            // 現在のMPを保存する
            this.PreviousMP = player.CurrentMP;
        }
    }
}
