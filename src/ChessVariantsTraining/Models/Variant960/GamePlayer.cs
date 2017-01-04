using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessVariantsTraining.Models.Variant960
{
    public class GamePlayer : IEquatable<GamePlayer>
    {
        public bool Registered
        {
            get;
            private set;
        }

        int _userId;
        public int UserId
        {
            get
            {
                if (Registered)
                {
                    return _userId;
                }
                else
                {
                    throw new InvalidOperationException("Player.UserId cannot be fetched if !Registered.");
                }
            }
        }

        string _identifier;
        public string AnonymousIdentifier
        {
            get
            {
                if (Registered)
                {
                    throw new InvalidOperationException("Player.AnonymousIdentifier cannot be fetched if Registered.");
                }
                else
                {
                    return _identifier;
                }
            }
        }

        public GamePlayer(int user)
        {
            Registered = true;
            _userId = user;
        }

        public GamePlayer(string anonIdentifier)
        {
            Registered = false;
            _identifier = anonIdentifier;
        }

        public override bool Equals(object obj)
        {
            GamePlayer other = obj as GamePlayer;
            if (other == null)
            {
                return false;
            }

            return Equals(other);
        }

        public bool Equals(GamePlayer other)
        {
            if (Registered != other.Registered)
            {
                return false;
            }

            if (Registered)
            {
                return UserId == other.UserId;
            }
            else
            {
                return AnonymousIdentifier == other.AnonymousIdentifier;
            }
        }

        public override int GetHashCode()
        {
            return new { Registered = Registered, UserId = _userId, AnonymousIdentifier = _identifier }.GetHashCode();
        }

        public static bool operator==(GamePlayer player1, GamePlayer player2)
        {
            if (player1 == null && player2 == null) return true;
            else if (player1 == null || player2 == null) return false;
            return player1.Equals(player2);
        }

        public static bool operator!=(GamePlayer player1, GamePlayer player2)
        {
            if (player1 == null && player2 == null) return false;
            else if (player1 == null || player2 == null) return true;
            return !player1.Equals(player2);
        }
    }
}
