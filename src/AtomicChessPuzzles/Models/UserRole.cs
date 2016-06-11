using System;

namespace AtomicChessPuzzles.Models
{
    [Flags]
    public enum UserRole
    {
        None = 0,
        PuzzleReviewer = 1,
        PuzzleEditor = 2,
        CommentModerator = 4,
        UserModerator = 8,
        Admin = 16
    }
}
