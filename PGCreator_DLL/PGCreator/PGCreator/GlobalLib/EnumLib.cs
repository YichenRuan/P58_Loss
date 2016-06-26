namespace P58_Loss.GlobalLib
{
    public enum PGComponents : byte
    {
        BracedFrame,            //B1033
        SteelBCJoint,           //B1035
        BeamColumnJoint,        //B1041
        LinkBeam,               //B1042
        ShearWall,              //B1044
        FlatSlab,               //B1049
        MasonryWall,            //B1051, B1052
        //CFSteelWall           //B1061
        //WoodWall              //B1071
        //ExternalWall          //B2011
        CurtainWall,            //B2022
        Storefront,             //B2023
        Roof,                   //B3011
        //Chimney               //B3031
        //Parapet               //B3041
        GypWall,                //C1011 
        Stair,                  //C2011
        WallFinish,             //C3011
        //FloorFinish           //C3021       
        //RAFloor               //C3027
        Ceiling,                //C3032
        CeilingLighting,        //C3033, C3034
        //Elevator              //D1014
        Pipe,                   //D2021, D2022, D2031, D2051, D2061, D4011
        Chiller,                //D3031
        CoolingTower,           //D3031
        Compressor,             //D3032
        HVACFan_InLine,         //D3041
        Duct,                   //D3041
        VAV,                    //D3041
        HVACFan,                //D3041
        Diffuser,               //D3041
        AHU,                    //D3052
        ControlPanel,           //D3067
        FireSprinkler,          //D4011
        Transformer,            //D5011
        MCC,                    //D5012
        LVS,                    //D5012
        DistPanel,              //D5012
        BatteryRack,            //D5092
        BatteryCharger,         //D5092
        DieselGen               //D5092
        //Furnishings           //E2022, F1012
    }

    public enum MEPComponents : byte
    {
        Chiller,
        CoolingTower,
        Compressor,
        HVACFan_InLine,   
        VAV,
        HVACFan,
        Diffuser,
        AHU,
        ControlPanel,
        FireSprinkler,
        Transformer,
        MCC,
        LVS,
        DistPanel,
        BatteryRack,
        BatteryCharger,
        DieselGen
    }

    public enum PGMaterialType : byte
    {
        Concrete,
        PrestressConcrete,
        Steel,
        WeldedSteel,
        ThreadedSteel,
        VitaulicSteel,
        CastIron_FC,
        CastIron_BSC,
        Masonry,
        Wood,
        Glass,
        Gypsum,
        WallPaper,
        Ceramic,
        Marble,
        ConcreteTile,
        ClayTile
    }

    public enum DefaultSet : byte
    {
        BracedFrame_FrameType,                  //B1033
        BracedFrame_CrossSection,
        BracedFrame_Brace,
        BracedFrame_PLF,
        SteelBCJoint_ConnectionType,            //B1035
        LinkBeam_Rein,                          //B1042
        ShearWall_BoundaryCondition,            //B1044
        ShearWall_Curtain,
        FlatSlab_ShearRein,                     //B1049
        FlatSlab_VgVo,
        FlatSlab_ContiRein,
        MasonryWall_Grout,                      //B1051, B1052
        MasonryWall_Mechanics,
        Roof_Secure,                            //B3011
        GypWall_Stud,                           //C1011
        GypWall_BoundaryCondition,
        Stair_Matl,                             //C2011
        Stair_Joint,
        Ceiling_LateralSupport,                 //C3032
        Ceiling_Ip,
        Lighting_Support,                       //C3033, C3034
        Lighting_SeisRated,
        Pipe_FragilityType,                     //D2021, D2022, D2031, D2051, D2061, D4011
        Chiller_Install,                        //D3031
        Chiller_Capacity,
        Chiller_DamageMode,
        CoolingTower_Install,                   //D3031
        CoolingTower_Capacity,
        CoolingTower_DamageMode,
        Compressor_Install,                     //D3032
        Compressor_Capacity,
        Compressor_DamageMode,
        HVACFan_InLine_Install,                 //D3041
        Duct_Material,                          //D3041     
        HVACFan_Install,                        //D3041
        HVACFan_DamageMode,
        Diffuser_Ceiling,                       //D3041
        AHU_Install,                            //D3052
        AHU_Capacity,
        AHU_DamageMode,
        ControlPanel_Install,                   //D3067
        ControlPanel_DamageMode,
        FireSprinkler_Bracing,                  //D4011
        FireSprinkler_Ceilling,
        Transformer_Install,                    //D5011
        Transformer_Capacity,
        Transformer_DamageMode,
        MCC_Install,                            //D5012
        MCC_DamageMode,
        LVS_Install,                            //D5012
        LVS_Capacity,
        LVS_DamageMode,
        DistPanel_Install,                      //D5012
        DistPanel_Capacity,
        DistPanel_DamageMode,
        BatteryRack_Install,                    //D5092
        BatteryRack_DamageMode,
        BatteryCharger_Install,                 //D5092
        BatteryCharger_DamageMode,
        DieselGen_Install,                      //D5092
        DieselGen_Capacity,
        DieselGen_DamageMode
    }

    public enum Direction : byte
    {
        X = 0,
        Y,
        Undefined
    }

    public enum AbandonmentTable : int
    {
        LevelNotFound = 10,
        LevelOutOfRoof,   
        SkewBeam,
        SkewWall,

        Joint_TooManyBeamsAtOneJoint = 100,
        Joint_ColumnTooShort,
        Wall_WallTooShort,
        StruWall_MatlOOR,

        BracedFrame_TypeCrossConflict = 1010,
        BracedFrame_BRBwXBrace,
        SteelJoint_BeamDepthOOR = 1020,
        SteelJoint_BeamNotSteel,
        RCJoint_BeamNotConcrete = 1030,
        LinkBeam_BeamTooHi = 1040,
        LinkBeam_AspectRatioOOR,
        ShearWall_RectangularWallTooThick = 1050,
        ShearWall_ThickWallHasSingleCurtain,
        ShearWall_ThinWallHasDoubleCurtain,
        ShearWall_LowRiseWallTooThick,
        ShearWall_UnSlenderWallTooHigh,
        ShearWall_SlenderWallTooHigh,
        ShearWall_SlenderWallTooThick,
        ShearWall_SlenderWallTooLong,
        FlatSlab_ReinVgVoConflict = 1060,
        MasonryWall_ThicknessOOR = 1070,

        CurtainWall_PanelNotGlazed = 2010,
        Roof_MatlOOR = 2020,
        Roof_TooManyRoofs,
        GypWall_BoundConflictHi = 2030,
        Stair_MatlJointConflict = 2040,
        TooManyFinishes = 2050,
        Ceiling_SDCConflictLatSupport = 2060,
        Ceiling_SDCConflictIp,

        Pipe_TypeUnknown = 3010,
        Pipe_MatlOOR,
        Pipe_NonCircular,
        Pipe_TypeMateConflict,
        Pipe_DiameterMateConflict,
        Pipe_MateFragConflict,
        Pipe_DiameterOOR,
        Duct_CrossSectionOOR = 3020,

        Chiller_InsDMConflict = 4011,
        CoolingTower_InsDMConflict,
        Compressor_InsDMConflict,
        VAV_SDCNonABC,
        HVACFan_InsDMConflict,
        Diffuser_InsSDCConflict,
        AHU_InsDMConflict,
        ControlPanel_InsDMConflict,
        FireSprinkler_Conflict,
        Transformer_InsDMConflict,
        MCC_InsDMConflict,
        LVS_InsDMConflict,
        DistPanel_InsDMConflict,
        BatteryRack_InsDMConflict,
        BatteryCharger_InsDMConflict,
        DieselGen_InsDMConflict
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
