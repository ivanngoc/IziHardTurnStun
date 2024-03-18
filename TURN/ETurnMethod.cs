using static IziHardGames.STUN.StunHeader;

namespace IziHardGames.TURN
{
    public class LoggerForTurn
    {

    }

    public enum ETurnMethod
    {
        #region TURN EXTENDED
        /// <summary>
        /// <see cref="EStunClass.Request"/> & <see cref="EStunMethod.Allocate"/>
        /// </summary>
        Allocate = ConstantsForTurn.METHOD_ALLOCATE,
        Refresh = ConstantsForTurn.METHOD_REFRASH,
        Send = ConstantsForTurn.METHOD_SEND,
        Data = ConstantsForTurn.METHOD_DATA,
        CreatePermission = ConstantsForTurn.METHOD_CREATE_PERMISSION,
        ChannelBind = ConstantsForTurn.METHOD_CHANNEL_BIND,
        #endregion
    }
}
