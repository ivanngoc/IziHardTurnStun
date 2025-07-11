﻿using IziHardGames.STUN;
using System;
using static IziHardGames.STUN.StunHeader;

namespace IziHardGames
{
    /// <summary>
    /// Bit Positioning: <cref="https://datatracker.ietf.org/doc/html/rfc5389#section-6"/><br/>
    /// What indication is: <cref="https://datatracker.ietf.org/doc/html/rfc5389#section-5"/><br/>
    /// </summary>
    public enum EStunClass : UInt16
    {
        Request = ConstantsForStun.CLASS_REQUEST,
        Indication = ConstantsForStun.CLASS_INDICATION,             //A STUN message that does not receive a response
        ResponseSuccess = ConstantsForStun.CLASS_SUCCESS_RESPONSE,
        ResponseError = ConstantsForStun.CLASS_ERROR_RESPONSE,
    }
}
