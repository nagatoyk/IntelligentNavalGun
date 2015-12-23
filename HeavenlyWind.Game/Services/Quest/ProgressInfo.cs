﻿using Sakuno.KanColle.Amatsukaze.Game.Models;
using System;

namespace Sakuno.KanColle.Amatsukaze.Game.Services.Quest
{
    public class ProgressInfo : ModelBase
    {
        public QuestInfo Quest { get; }
        public QuestType ResetType { get; }

        QuestState r_State;
        public QuestState State
        {
            get { return r_State; }
            set
            {
                if (r_State != value)
                {
                    r_State = value;
                    UpdateTime = DateTimeOffset.Now;

                    RecordService.Instance.QuestProgress.UpdateState(this);
                }
            }
        }

        int r_Progress;
        public int Progress
        {
            get { return r_Progress; }
            internal set
            {
                var rProgress = Math.Min(value, Quest.Total - Quest.StartFrom);
                if (r_Progress != rProgress)
                {
                    r_Progress = rProgress;
                    UpdateTime = DateTimeOffset.Now;
                    OnPropertyChanged(nameof(Progress));
                    OnPropertyChanged(nameof(DisplayProgress));

                    RecordService.Instance.QuestProgress.UpdateProgress(this);
                }
            }
        }
        public int DisplayProgress => Progress - Quest.StartFrom;

        public DateTimeOffset UpdateTime { get; internal set; }

        internal ProgressInfo(int rpID, QuestType rpResetType, QuestState rpState, int rpProgress) : this(rpID, rpResetType, rpState, rpProgress, DateTimeOffset.Now) { }
        internal ProgressInfo(int rpID, QuestType rpResetType, QuestState rpState, int rpProgress, DateTimeOffset rpUpdateTime)
        {
            Quest = QuestProgressService.Instance.Infos[rpID];
            ResetType = rpResetType;

            r_State = rpState;
            r_Progress = rpProgress;
            UpdateTime = rpUpdateTime;
        }
    }
}
