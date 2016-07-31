using ChessDotNet;
using ChessDotNet.Pieces;
using ChessDotNet.Variants.Atomic;
using System;

namespace AtomicChessPuzzles.Models
{
    public class TimedTrainingSession
    {
        public string SessionID { get; private set; }
        public DateTime StartedAt { get; private set; }
        public DateTime EndsAt { get; private set; }
        public string CurrentFen { get; set; }
        public AtomicChessGame AssociatedGame { get; set; }
        TrainingPosition currentPosition = null;
        public bool Ended
        { 
            get
            {
                return DateTime.UtcNow >= EndsAt;
            }
        }
        public bool RecordedInDb { get; set; }
        public TimedTrainingScore Score { get; set; }
        public bool AutoAcknowledegable
        {
            get
            {
                return DateTime.UtcNow >= EndsAt + new TimeSpan(0, 1, 0);
            }
        }


        public TimedTrainingSession(string sessionId, DateTime startedAt, DateTime endsAt, string owner, string type)
        {
            SessionID = sessionId;
            StartedAt = startedAt;
            EndsAt = endsAt;
            RecordedInDb = false;
            Score = new TimedTrainingScore(0, type, owner);
        }

        public bool VerifyMove(string origin, string destination, string promotion)
        {
            bool correctMove = false;
            Piece promotionPiece = null;
            if (promotion != null)
            {
                promotionPiece = Utilities.GetPromotionPieceFromName(promotion, AssociatedGame.WhoseTurn);
            }
            MoveType moveType = AssociatedGame.ApplyMove(new Move(origin, destination, AssociatedGame.WhoseTurn, promotionPiece), false);
            if (moveType != MoveType.Invalid)
            {
                correctMove = AssociatedGame.IsCheckmated(AssociatedGame.WhoseTurn) || AssociatedGame.KingIsGone(AssociatedGame.WhoseTurn);
            }
            else
            {
                correctMove = false;
            }
            if (correctMove)
            {
                Score.Score++;
            }
            return correctMove;
        }

        public void SetPosition(TrainingPosition position)
        {
            currentPosition = position;
            AssociatedGame = new AtomicChessGame(position.FEN);
            CurrentFen = position.FEN;
        }

        public void RetryCurrentPosition()
        {
            SetPosition(currentPosition);
        }
    }
}