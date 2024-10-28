public class CombatStateMachine
{
    public enum CurrentState
    {
        Cutscene,
        PlayerTurn,
        EnemyTurn,
        PlayerMovement,
        EnemyMovement,
        PlayerChosen,
    }

    public enum Command
    {
        Begin,
        End,
        Pause,
        Resume,
        Exit
    }
}