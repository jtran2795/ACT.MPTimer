namespace ACT.MPTimer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    using ACT.MPTimer.Properties;
    using Advanced_Combat_Tracker;

    /// <summary>
    /// FF14を監視する エノキアンタイマ
    /// </summary>
    public partial class FF14Watcher
    {
        /// <summary>
        /// エノキアンの効果期間
        /// </summary>
        public const double EnochianDuration = 30.0d;

        /// <summary>
        /// エノキアンの延長時の効果期間の劣化量
        /// </summary>
        public const double EnochianDegradationSecondsExtending = 5.0d;

        /// <summary>
        /// エノキアンOFF後にエノキアンの更新を受付ける猶予期間（ms）
        /// </summary>
        public const int GraceToUpdateEnochian = 1700;

        /// <summary>
        /// エノキアン効果中か？
        /// </summary>
        private bool inEnochian;

        /// <summary>
        /// アンブラルアイス中か？
        /// </summary>
        private bool inUmbralIce;

        /// <summary>
        /// エノキアンの更新回数
        /// </summary>
        private long updateEnchianCount;

        /// <summary>
        /// ログキュー
        /// </summary>
        private Queue<string> logQueue = new Queue<string>();

        /// <summary>
        /// エノキアンタイマータスク
        /// </summary>
        private Task enochianTimerTask;

        /// <summary>
        /// エノキアンタイマー停止フラグ
        /// </summary>
        private bool enochianTimerStop;

        /// <summary>
        /// プレイヤーの名前
        /// </summary>
        private string playerName;

        /// <summary>
        /// エノキアンの更新猶予期間
        /// </summary>
        private bool inGraceToUpdate;

        /// <summary>
        /// 猶予期間中に更新されたか？
        /// </summary>
        private bool updatedDuringGrace;

        /// <summary>
        /// 最後のエノキアンの残り時間イベント
        /// </summary>
        private string lastRemainingTimeOfEnochian;

        /// <summary>
        /// エノキアンタイマーを開始する
        /// </summary>
        private void StartEnochianTimer()
        {
            ActGlobals.oFormActMain.OnLogLineRead += this.OnLoglineRead;
            this.playerName = string.Empty;
            this.lastRemainingTimeOfEnochian = string.Empty;
            this.logQueue.Clear();
            this.enochianTimerStop = false;
            this.inGraceToUpdate = false;
            this.updatedDuringGrace = false;
            this.enochianTimerTask = new Task(this.AnalyseLogLinesToEnochian);
            this.enochianTimerTask.Start();
        }

        /// <summary>
        /// エノキアンタイマーを終了する
        /// </summary>
        private void EndEnochianTimer()
        {
            ActGlobals.oFormActMain.OnLogLineRead -= this.OnLoglineRead;

            if (this.enochianTimerTask != null)
            {
                this.enochianTimerStop = true;
                this.enochianTimerTask.Wait();
                this.enochianTimerTask.Dispose();
                this.enochianTimerTask = null;
            }
        }

        /// <summary>
        ///  Logline Read
        /// </summary>
        /// <param name="isImport">インポートログか？</param>
        /// <param name="logInfo">発生したログ情報</param>
        private void OnLoglineRead(
            bool isImport,
            LogLineEventArgs logInfo)
        {
            if (isImport)
            {
                return;
            }

            // エノキアンタイマーが有効ならば・・・
            if (Settings.Default.EnabledEnochianTimer &&
                this.EnabledByJobFilter)
            {
                // ログをキューに貯める
                lock (this.logQueue)
                {
                    this.logQueue.Enqueue(logInfo.logLine);
                }
            }
        }

        /// <summary>
        /// エノキアンタイマー向けにログを分析する
        /// </summary>
        private void AnalyseLogLinesToEnochian()
        {
            while (true)
            {
                try
                {
                    if (this.enochianTimerStop)
                    {
                        break;
                    }

                    // エノキアンタイマーViewModelを参照する
                    var vm = EnochianTimerWindow.Default.ViewModel;

                    // プレイヤー名を保存する
                    if (string.IsNullOrWhiteSpace(this.playerName))
                    {
                        if (this.LastPlayerInfo != null)
                        {
                            this.playerName = this.LastPlayerInfo.Name;
                            Trace.WriteLine("Player name is " + this.playerName);
                        }
                    }

                    // エノキアンタイマーが無効？
                    if (!Settings.Default.EnabledEnochianTimer)
                    {
                        vm.Visible = false;
                        Thread.Sleep(Settings.Default.ParameterRefreshRate);
                        continue;
                    }

                    // ジョブフィルタを設定する
                    if (!this.EnabledByJobFilter)
                    {
                        vm.Visible = false;
                        Thread.Sleep(Settings.Default.ParameterRefreshRate);
                        continue;
                    }

                    // ログを解析する
                    if (!string.IsNullOrWhiteSpace(this.playerName))
                    {
                        var log = string.Empty;

                        while (true)
                        {
                            lock (this.logQueue)
                            {
                                if (this.logQueue.Count > 0)
                                {
                                    log = this.logQueue.Dequeue();
                                }
                                else
                                {
                                    break;
                                }
                            }

                            this.AnalyzeLogLineToEnochian(log);
                            Thread.Sleep(1);
                        }
                    }

                    // エノキアンの残り秒数をログとして発生させる
                    if (vm.EndScheduledDateTime >= DateTime.MinValue)
                    {
                        var remainSeconds = (vm.EndScheduledDateTime - DateTime.Now).TotalSeconds;
                        if (remainSeconds >= 0)
                        {
                            var notice = "Remaining time of Enochian. " + remainSeconds.ToString("N0");
                            if (this.lastRemainingTimeOfEnochian != notice)
                            {
                                this.lastRemainingTimeOfEnochian = notice;
                                ActGlobals.oFormActMain.ParseRawLogLine(false, DateTime.Now, notice);
                            }
                        }
                    }

                    Thread.Sleep(Settings.Default.ParameterRefreshRate / 2);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(
                        "Enochian Timer Error!" + Environment.NewLine +
                        ex.ToString());

                    Thread.Sleep(5 * 1000);
                }
            }
        }

        /// <summary>
        /// エノキアンタイマー向けにログを分析する
        /// </summary>
        /// <param name="log">ログ</param>
        private void AnalyzeLogLineToEnochian(
            string log)
        {
            if (string.IsNullOrWhiteSpace(log))
            {
                return;
            }

            if (log.Contains("Welcome to") ||
                log.Contains("Willkommen auf"))
            {
                // プレイヤ情報を取得する
                var player = FF14PluginHelper.GetCombatantPlayer();
                if (player != null)
                {
                    this.playerName = player.Name;
                    Trace.WriteLine("Player name is " + this.playerName);
                }
            }

            if (string.IsNullOrWhiteSpace(this.playerName))
            {
                Trace.WriteLine("Player name is empty.");
                return;
            }

            // イニシャル版のプレイヤー名を生成する
            var names = this.playerName.Split(' ');
            var firstName = names.Length > 0 ? names[0].Trim() : string.Empty;
            var familyName = names.Length > 1 ? names[1].Trim() : string.Empty;

            var playerNameSaL = 
                firstName.Substring(0, 1) + "." + 
                " " + 
                familyName;

            var playerNameLaS = 
                firstName + 
                " " + 
                familyName.Substring(0, 1) + ".";

            var playerNameSaS = 
                firstName.Substring(0, 1) + "." + 
                " " + 
                familyName.Substring(0, 1) + ".";

            // 各種マッチング用の文字列を生成する
            var machingTextToEnochianOn = new string[]
            {
                this.playerName + "の「エノキアン」",
                playerNameSaL + "の「エノキアン」",
                playerNameLaS + "の「エノキアン」",
                playerNameSaS + "の「エノキアン」",
                "You use Enochian.",
                "Vous utilisez Énochien.",
                "Du setzt Henochisch ein.",
            };

            var machingTextToEnochianOff = new string[]
            {
                this.playerName + "の「エノキアン」が切れた。",
                playerNameSaL + "の「エノキアン」が切れた。",
                playerNameLaS + "の「エノキアン」が切れた。",
                playerNameSaS + "の「エノキアン」が切れた。",
                "You lose the effect of Enochian.",
                "Vous perdez l'effet Énochien.",
                "Du verlierst den Effekt von Henochisch.",
            };

            var machingTextToUmbralIceOn = new string[]
            {
                this.playerName + "に「アンブラルブリザード」の効果。",
                this.playerName + "に「アンブラルブリザードII」の効果。",
                this.playerName + "に「アンブラルブリザードIII」の効果。",
                playerNameSaL + "に「アンブラルブリザード」の効果。",
                playerNameSaL + "に「アンブラルブリザードII」の効果。",
                playerNameSaL + "に「アンブラルブリザードIII」の効果。",
                playerNameLaS + "に「アンブラルブリザード」の効果。",
                playerNameLaS + "に「アンブラルブリザードII」の効果。",
                playerNameLaS + "に「アンブラルブリザードIII」の効果。",
                playerNameSaS + "に「アンブラルブリザード」の効果。",
                playerNameSaS + "に「アンブラルブリザードII」の効果。",
                playerNameSaS + "に「アンブラルブリザードIII」の効果。",
                "You gain the effect of Umbral Ice.",
                "You gain the effect of Umbral Ice II.",
                "You gain the effect of Umbral Ice III.",
                "Vous bénéficiez de l'effet Glace ombrale.",
                "Vous bénéficiez de l'effet Glace ombrale II.",
                "Vous bénéficiez de l'effet Glace ombrale III.",
                "Du erhältst den Effekt von Schatteneis.",
                "Du erhältst den Effekt von Schatteneis II.",
                "Du erhältst den Effekt von Schatteneis III.",
            };

            var machingTextToUmbralIceOff = new string[]
            {
                this.playerName + "の「アンブラルブリザード」が切れた。",
                this.playerName + "の「アンブラルブリザードII」が切れた。",
                this.playerName + "の「アンブラルブリザードIII」が切れた。",
                playerNameSaL + "の「アンブラルブリザード」が切れた。",
                playerNameSaL + "の「アンブラルブリザードII」が切れた。",
                playerNameSaL + "の「アンブラルブリザードIII」が切れた。",
                playerNameLaS + "の「アンブラルブリザード」が切れた。",
                playerNameLaS + "の「アンブラルブリザードII」が切れた。",
                playerNameLaS + "の「アンブラルブリザードIII」が切れた。",
                playerNameSaS + "の「アンブラルブリザード」が切れた。",
                playerNameSaS + "の「アンブラルブリザードII」が切れた。",
                playerNameSaS + "の「アンブラルブリザードIII」が切れた。",
                "You lose the effect of Umbral Ice.",
                "You lose the effect of Umbral Ice II.",
                "You lose the effect of Umbral Ice III.",
                "Vous perdez l'effet Glace ombrale.",
                "Vous perdez l'effet Glace ombrale II.",
                "Vous perdez l'effet Glace ombrale III.",
                "Du verlierst den Effekt von Schatteneis.",
                "Du verlierst den Effekt von Schatteneis II.",
                "Du verlierst den Effekt von Schatteneis III.",
            };

            var machingTextToBlizzard4 = new string[]
            {
                this.playerName + "の「ブリザジャ」",
                playerNameSaL + "の「ブリザジャ」",
                playerNameLaS + "の「ブリザジャ」",
                playerNameSaS + "の「ブリザジャ」",
                "You cast Blizzard IV.",
                "Vous lancez Giga Glace.",
                "Du wirkst Eiska.",
            };

            // エノキアンON？
            foreach (var text in machingTextToEnochianOn)
            {
                if (log.EndsWith(text))
                {
                    this.inEnochian = true;
                    this.updateEnchianCount = 0;
                    this.UpdateEnochian(log);
                    this.lastRemainingTimeOfEnochian = string.Empty;

                    Trace.WriteLine("Enochian On. -> " + log);
                    return;
                }
            }

            // エノキアンOFF？
            foreach (var text in machingTextToEnochianOff)
            {
                if (log.Contains(text))
                {
                    // エノキアンの更新猶予期間をセットする
                    this.inGraceToUpdate = true;
                    this.updatedDuringGrace = false;

                    Task.Run(() =>
                    {
                        Thread.Sleep(GraceToUpdateEnochian + Settings.Default.ParameterRefreshRate);

                        // 更新猶予期間中？
                        if (this.inGraceToUpdate)
                        {
                            // 期間中に更新されていない？
                            if (!this.updatedDuringGrace)
                            {
                                this.inEnochian = false;
                                this.updateEnchianCount = 0;
                                Trace.WriteLine("Enochian Off. -> " + log);
                            }

                            this.inGraceToUpdate = false;
                            return;
                        }

                        this.inEnochian = false;
                        this.updateEnchianCount = 0;
                        Trace.WriteLine("Enochian Off. -> " + log);
                    });

                    return;
                }
            }

            // アンブラルアイスON？
            foreach (var text in machingTextToUmbralIceOn)
            {
                if (log.Contains(text))
                {
                    this.inUmbralIce = true;

                    Trace.WriteLine("Umbral Ice On. -> " + log);
                    return;
                }
            }

            // アンブラルアイスOFF？
            foreach (var text in machingTextToUmbralIceOff)
            {
                if (log.Contains(text))
                {
                    Task.Run(() =>
                    {
                        Thread.Sleep(GraceToUpdateEnochian + Settings.Default.ParameterRefreshRate);

                        this.inUmbralIce = false;
                        Trace.WriteLine("Umbral Ice Off. -> " + log);
                    });

                    return;
                }
            }

            // ブリザジャ？
            foreach (var text in machingTextToBlizzard4)
            {
                if (log.EndsWith(text))
                {
                    if (this.inEnochian &&
                        this.inUmbralIce)
                    {
                        // 猶予期間中？
                        if (this.inGraceToUpdate)
                        {
                            this.updatedDuringGrace = true;
                        }

                        this.updateEnchianCount++;
                        this.UpdateEnochian(log);
                    }

                    return;
                }
            }
        }

        /// <summary>
        /// エノキアンの効果時間を更新する
        /// </summary>
        private void UpdateEnochian(
            string log)
        {
            var vm = EnochianTimerWindow.Default.ViewModel;

            vm.StartDateTime = DateTime.Now;

            var duration = EnochianDuration - (EnochianDegradationSecondsExtending * this.updateEnchianCount);
            vm.EndScheduledDateTime = vm.StartDateTime.AddSeconds(duration);

            // ACTにログを発生させる
            ActGlobals.oFormActMain.ParseRawLogLine(false, DateTime.Now, "Updated Enochian.");

            Trace.WriteLine("Enochian Update, +" + duration.ToString() + "s. -> " + log);
        }
    }
}
