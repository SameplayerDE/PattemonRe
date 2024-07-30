namespace PatteLib.Gameplay.BattlePlay;

public enum BattleState
{
    Setup,
    PickOption, //Fight, Items, Run, Swap
    #region Fight
    PickAttack,
    PickTarget,
    #endregion
    #region Item
    #endregion
    #region Swap
    #endregion
}