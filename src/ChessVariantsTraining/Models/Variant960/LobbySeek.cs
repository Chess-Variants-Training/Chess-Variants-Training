using ChessVariantsTraining.DbRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessVariantsTraining.Models.Variant960
{
    public class LobbySeek
    {
        public enum Position
        {
            Random,
            FromNumbers
        }

        public string ID { get; set; }
        public TimeControl TimeControl { get; set; }
        public string Variant { get; private set; }
        public string FullVariantName
        {
            get
            {
                if (Variant == "Atomar")
                {
                    return Variant;
                }

                string beautifiedVariantName = Variant;
                if (beautifiedVariantName == "ThreeCheck")
                {
                    beautifiedVariantName = "Three-check";
                }
                else if (beautifiedVariantName == "RacingKings")
                {
                    beautifiedVariantName = "Racing Kings";
                }
                else if (beautifiedVariantName == "KingOfTheHill")
                {
                    beautifiedVariantName = "King of the Hill";
                }
                return string.Format("{0} {1}{2}", beautifiedVariantName, Variant != "RacingKings" ? 960 : 1440, Variant != "Horde" ? string.Format(" ({0}, {1})", Symmetrical ? "symmetrical" : "asymmetrical", ChosenPosition == Position.Random ? "random" : WhitePosition + "-" + BlackPosition) : (ChosenPosition == Position.Random ? " (random)" : " (" + BlackPosition + ")"));
            }
        }
        public Position ChosenPosition { get; private set; }
        public bool Symmetrical { get; private set; }
        public int WhitePosition { get; private set; }
        public int BlackPosition { get; private set; }
        public GamePlayer Owner { get; private set; }
        public DateTime LatestBump { get; set; }

        public LobbySeek(TimeControl timeControl, string variant, Position position, bool symmetrical, int whitePosition, int blackPosition, GamePlayer owner)
        {
            TimeControl = timeControl;
            Variant = variant;
            ChosenPosition = position;
            Symmetrical = position == Position.Random ? symmetrical : whitePosition == blackPosition;
            WhitePosition = whitePosition;
            BlackPosition = blackPosition;
            Owner = owner;
        }

        public static bool TryParse(string encoded, GamePlayer owner, out LobbySeek seek)
        {
            string[] parts = encoded.Split(';');
            if (parts.Length != 7)
            {
                seek = null;
                return false;
            }

            if (!int.TryParse(parts[0], out int secondsInitial))
            {
                seek = null;
                return false;
            }
            if ((secondsInitial != 30 && secondsInitial != 45 && secondsInitial != 90) && (secondsInitial % 60 != 0 || secondsInitial > 30 * 60 || secondsInitial < 0))
            {
                seek = null;
                return false;
            }

            if (!int.TryParse(parts[1], out int secondsIncrement))
            {
                seek = null;
                return false;
            }

            if (secondsIncrement < 0 || secondsIncrement > 30 || (secondsIncrement == 0 && secondsInitial == 0))
            {
                seek = null;
                return false;
            }

            string[] allowedVariants = new string[] { "Antichess", "Atomic", "Crazyhouse", "Horde", "KingOfTheHill", "RacingKings", "ThreeCheck", "Atomar", "Atomar960" };
            string variant = parts[2];
            if (!allowedVariants.Contains(parts[2]))
            {
                seek = null;
                return false;
            }

            if (parts[3] != "random" && parts[3] != "number")
            {
                seek = null;
                return false;
            }
            Position position = parts[3] == "random" ? Position.Random : Position.FromNumbers;

            if (parts[4] != "true" && parts[4] != "false")
            {
                seek = null;
                return false;
            }
            bool symmetrical = parts[4] == "true";

            if (!int.TryParse(parts[5], out int whitePosition) || whitePosition < 0 || whitePosition > (variant != "RacingKings" ? 959 : 1439))
            {
                seek = null;
                return false;
            }

            if (!int.TryParse(parts[6], out int blackPosition) || blackPosition < 0 || blackPosition > (variant != "RacingKings" ? 959 : 1439))
            {
                seek = null;
                return false;
            }

            if (whitePosition == 518 && blackPosition == 518 && variant != "RacingKings")
            {
                seek = null;
                return false;
            }

            seek = new LobbySeek(new TimeControl(secondsInitial, secondsIncrement), variant, position, symmetrical, whitePosition, blackPosition, owner);
            return true;
        }

        public async Task<Dictionary<string, string>> SeekJson(IUserRepository userRepository)
        {
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "l", FullVariantName }
            };
            if (Owner is RegisteredPlayer)
            {
                data.Add("o", (await userRepository.FindByIdAsync((Owner as RegisteredPlayer).UserId)).Username);
            }
            else
            {
                data.Add("o", "(Anonymous)");
            }
            data.Add("c", TimeControl.ToString());
            data.Add("i", ID);
            return data;
        }
    }
}
