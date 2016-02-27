namespace P58_Loss.GlobalLib
{
    public enum ElementName : byte
    {
        Beam = 0,
        StruColumn,
        BearingWall,
        ShearWall,
        CombinedWall,
        Slab,
        NonBearingWall = 100,
        Level = 200
    }

    public enum PGComponents : byte
    {
        BeamColumnJoint = 0,
        ShearWall
    }

    public enum Direction : byte
    {
        X = 0,
        Y,
        Undefined
    }

    public enum AbandonmentTable : int
    {
        SkewBeam,                           //B1041
        BeamFloorNotFound,
        TooManyBeamsAtOneJoint,
        ColumnTooShort,
        ShearWallNonConcrete,               //B1044
        RectangularWallTooThick,
        ThickWallHasSingleCurtain,
        ThinWallHasDoubleCurtain,
        LowRiseWallTooThick,
        UnSlenderWallTooHigh,
        SlenderWallTooHigh,
        SlenderWallTooThick,
        SlenderWallTooLong,
        WallTooShort
    }

    public enum MomentFrameType : byte
    {
        SMF,
        confirmedMF,
        IMF,
        OMF,
        unconfirmedMF
    }

    public enum SDC : byte
    {
        A,
        B,
        C,
        D,
        E,
        F
    }

}
