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
        ShearWall,
        GypWall,
        CurtainWall,
        Storefront,
        Ceiling,
        CeilingLighting,
        MasonryWall,
        WallFinish,
        Duct
    }

    public enum PGMaterialType : byte
    {
        Concrete,
        Metal,
        Masonry,
        Wood,
        Glass,
        Gypsum,
        WallPaper,
        Ceramic,
        Marble
    }

    public enum DefaultSet : byte
    {
        ShearWall_BoundaryCondition,
        ShearWall_Curtain,
        GypWall_Stud,
        GypWall_BoundaryCondition,
        Ceiling_LateralSupport,
        Ceiling_Ip,
        Lighting_Support,
        Lighting_SeisRated,
        MasonryWall_Grout,
        MasonryWall_Mechanics,
        Duct_Material
    }

    public enum Direction : byte
    {
        X = 0,
        Y,
        Undefined
    }

    public enum AbandonmentTable : int
    {
        BeamNotConcrete,                    //B1041
        SkewBeam,                           
        BeamFloorNotFound,
        TooManyBeamsAtOneJoint,
        ColumnTooShort,
        StruWallMaterialOOR,                //B1044
        RectangularWallTooThick,
        ThickWallHasSingleCurtain,
        ThinWallHasDoubleCurtain,
        LowRiseWallTooThick,
        UnSlenderWallTooHigh,
        SlenderWallTooHigh,
        SlenderWallTooThick,
        SlenderWallTooLong,
        WallTooShort,
        SkewWall,
        WallBeyondRoof,
        NonBearingWallNonGyp,               //C1011
        WallBottomError,
        GypWallBoundConflictHi,
        CurtainWallPanelNotGlazed,          //B2022, B2023
        CeilingLevelError,                  //C3032
        CeilingSDCConflictLatSupport,
        CeilingSDCConflictIp,
        MasonryWallThicknessOOR,            //B1015
        TooManyFinishes,
        DuctLevelError                      //D3041
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
        F,
        OSHPD
    }

}
