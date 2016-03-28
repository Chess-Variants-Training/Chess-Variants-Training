using System;

namespace AtomicChessPuzzles.Models
{
    [Flags]
    public enum UserRole
    {
        None = 0,
        PuzzleReviewer = 1,
        PuzzleEditor = 2,
        UserModerator = 4,
        Admin = 8
    }
}
