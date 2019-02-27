﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelBot.Dialogs.Shared
{
    public enum InterruptionStatus
    {
        /// <summary>
        /// Indicates that the active dialog was interrupted and needs to resume.
        /// </summary>
        Interrupted,

        /// <summary>
        /// Indicates that there is a new dialog waiting and the active dialog needs to be shelved.
        /// </summary>
        Waiting,

        /// <summary>
        /// Indicates that no interruption action is required.
        /// </summary>
        NoAction,
    }
}
