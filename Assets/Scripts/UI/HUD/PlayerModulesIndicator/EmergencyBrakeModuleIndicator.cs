public class EmergencyBrakeModuleIndicator : BaseChargeIndicator
{
    EnumStandardGameState _currentState;

    [Zenject.Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    protected override void Construct(GlobalEventsManager globalEventsManager)
    {
        base.Construct(globalEventsManager);
    }

    protected override void FastTravelStart()
    {
        ChangeState(EnumStandardGameState.Warp);
        ActiveSetting(MaxCharges);
    }

    protected override void RoundStart()
    {
        ChangeState(EnumStandardGameState.Gameplay);
        ActiveSetting(MaxCharges);
    }

    protected override void RoundEnd()
    {
        ChangeState(EnumStandardGameState.Lobby);
        ActiveSetting(MaxCharges);
    }

    private void ChangeState(EnumStandardGameState newState)
    {
        _currentState = newState;
    }

    protected override bool ActiveSetting(int maxCharges)
    {
        var activeFlag = base.ActiveSetting(maxCharges);

        if (!activeFlag) return activeFlag;

        switch (_currentState)
        {
            case EnumStandardGameState.Warp:
                {
                    gameObject.SetActive(true);
                    activeFlag = true;

                    break;
                }
            case EnumStandardGameState.Gameplay:
                {
                    gameObject.SetActive(true);
                    activeFlag = true;

                    break;
                }
            case EnumStandardGameState.Lobby:
                {
                    gameObject.SetActive(false);
                    activeFlag = false;

                    break;
                }
        }

        return activeFlag;
    }
}
