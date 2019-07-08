using System.Collections.Generic;

namespace ChessVariantsTraining.Models
{
    public static class UserRole
    {
        public const string NONE = "None";

        public const string GENERATOR = "Generator";
        public const string BETA_GENERATOR = "BetaGenerator";

        public const string PUZZLE_TAGGER = "PuzzleTagger";
        public const string PUZZLE_REVIEWER = "PuzzleReviewer";
        public const string PUZZLE_EDITOR = "PuzzleEditor";

        public const string COMMENT_MODERATOR = "CommentModerator";
        public const string USER_MODERATOR = "UserModerator";

        public const string ADMIN = "Admin";

        public static string UserRolesToString(List<string> roles)
        {
            return string.Join(",", roles);
        }

        public static bool HasAtLeastThePrivilegesOf(IEnumerable<string> actualPrivileges, string privilegeToCheckAgainst)
        {
            foreach (string actualPrivilege in actualPrivileges)
            {
                if (HasAtLeastThePrivilegesOf(actualPrivilege, privilegeToCheckAgainst))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool HasAtLeastThePrivilegesOf(string actualPrivilege, string privilegeToCheckAgainst)
        {
            if (actualPrivilege == ADMIN || privilegeToCheckAgainst == NONE) return true;

            switch (privilegeToCheckAgainst)
            {
                case ADMIN:
                    return actualPrivilege == ADMIN;
                case USER_MODERATOR:
                    return actualPrivilege == USER_MODERATOR;
                case COMMENT_MODERATOR:
                    return actualPrivilege == COMMENT_MODERATOR || actualPrivilege == USER_MODERATOR;
                case PUZZLE_EDITOR:
                    return actualPrivilege == PUZZLE_EDITOR;
                case PUZZLE_REVIEWER:
                    return actualPrivilege == PUZZLE_REVIEWER || actualPrivilege == PUZZLE_EDITOR;
                case PUZZLE_TAGGER:
                    return actualPrivilege == PUZZLE_TAGGER || actualPrivilege == PUZZLE_REVIEWER || actualPrivilege == PUZZLE_EDITOR;
                case BETA_GENERATOR:
                    return actualPrivilege == BETA_GENERATOR;
                case GENERATOR:
                    return actualPrivilege == GENERATOR;
                default:
                    return false;
            }
        }

        public static bool HasAtLeastThePrivilegesOf(IEnumerable<string> actualPrivileges, IEnumerable<string> privilegesToCheckAgainst)
        {
            foreach (string pr in privilegesToCheckAgainst)
            {
                if (HasAtLeastThePrivilegesOf(actualPrivileges, pr))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
