using System;

namespace IDB.Presentation.MVC4.Areas.Indicators.Enums
{
    [Flags]
    public enum LanguageTypeEnum
    {
        None = 0x0,
        LevelLanguagueEnRequired = 0x1,
        LevelLanguagueEsRequired = 0x2,
        LevelLanguagueFrRequired = 0X4,
        LevelLanguaguePtRequired = 0x8,

        CategoryLanguagueEnRequired = 0x16,
        CategoryLanguagueEsRequired = 0x32,
        CategoryLanguagueFrRequired = 0x64,
        CategoryLanguaguePtRequired = 0x128
    }
}