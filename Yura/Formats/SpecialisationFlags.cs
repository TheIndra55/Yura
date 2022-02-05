using System;

namespace Yura.Formats
{
    [Flags]
    enum SpecialisationFlags : uint
    {
        English = 1 << 0,
        French = 1 << 1,
        German = 1 << 2,
        Italian = 1 << 3,
        Spanish = 1 << 4,
        Japanese = 1 << 5,
        Portugese = 1 << 6,
        Polish = 1 << 7,
        British = 1 << 8,
        Russian = 1 << 9,

        GfxNextGen = 0x80000000
    }
}
