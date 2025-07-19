namespace YqlossClientHarmony;

public static class Adofai
{
    public static scrController Controller => scrController.instance;

    public static scnEditor Editor => scnEditor.instance;

    public static scnGame Game => scnGame.instance;

    public static scrConductor Conductor => scrConductor.instance;

    public static int CurrentFloorId => Controller.currFloor.seqID;

    public static int TotalFloorCount =>
        Game.levelData.isOldLevel ? Game.levelData.pathData.Length : Game.levelData.angleData.Count;
}