using AtomicChessPuzzles.MemoryRepositories;
using AtomicChessPuzzles.Models;
using AtomicChessPuzzles.Services;
using ChessDotNet;
using ChessDotNet.Pieces;
using ChessDotNet.Variants.Atomic;
using Microsoft.AspNet.Mvc;
using System;
using System.Dynamic;

namespace AtomicChessPuzzles.Controllers
{
    public class EndgamesController : Controller
    {
        IBoardGenerator boardGenerator;
        IEndgameTrainingSessionRepository endgameTrainingSessionRepository;
        IMoveCollectionTransformer moveCollectionTransformer;

        public EndgamesController(IBoardGenerator _boardGenerator, IEndgameTrainingSessionRepository _endgameTrainingSessionRepository, IMoveCollectionTransformer _moveCollectionTransformer)
        {
            boardGenerator = _boardGenerator;
            endgameTrainingSessionRepository = _endgameTrainingSessionRepository;
            moveCollectionTransformer = _moveCollectionTransformer;
        }

        [Route("/Endgames")]
        public ActionResult Index()
        {
            return View();
        }

        IActionResult StartNewSession(Piece[] required, bool adjacentKings)
        {
            GameCreationData gcd = new GameCreationData();
            gcd.Board = boardGenerator.Generate(required, adjacentKings);
            gcd.EnPassant = null;
            gcd.HalfMoveClock = 0;
            gcd.FullMoveNumber = 1;
            gcd.WhoseTurn = Player.White;
            gcd.CanBlackCastleKingSide = gcd.CanBlackCastleQueenSide = gcd.CanWhiteCastleKingSide = gcd.CanWhiteCastleQueenSide = false;

            AtomicChessGame game = new AtomicChessGame(gcd);


            string sessionId;
            do
            {
                sessionId = Guid.NewGuid().ToString();
            } while (endgameTrainingSessionRepository.Exists(sessionId));

            EndgameTrainingSession session = new EndgameTrainingSession(sessionId, game);
            endgameTrainingSessionRepository.Add(session);
            string fen = session.InitialFEN;
            return View("Train", new Tuple<string, string>(fen, sessionId));
        }

        [Route("/Endgames/KRR-K-Adjacent-Kings", Name = "KRRvsKWithAdjacentKings")]
        public IActionResult KRRvsKWithAdjacentKings()
        {
            return StartNewSession(new Piece[] { new King(Player.White), new King(Player.Black), new Rook(Player.White), new Rook(Player.White) }, true);
        }

        [Route("/Endgames/KQQ-K-Adjacent-Kings", Name = "KQQvsKWithAdjacentKings")]
        public IActionResult KQQvsKWithAdjacentKings()
        {
            return StartNewSession(new Piece[] { new King(Player.White), new King(Player.Black), new Queen(Player.White), new Queen(Player.White) }, true);
        }

        [Route("/Endgames/GetValidMoves/{trainingSessionId}")]
        public IActionResult GetValidMoves(string trainingSessionId)
        {
            EndgameTrainingSession session = endgameTrainingSessionRepository.Get(trainingSessionId);
            if (session == null)
            {
                return Json(new { success = false, error = "Training session ID not found." });
            }

            return Json(new { success = true, dests = moveCollectionTransformer.GetChessgroundDestsForMoveCollection(session.Game.GetValidMoves(session.Game.WhoseTurn)) });
        }

        [HttpPost]
        [Route("/Endgames/SubmitMove")]
        public IActionResult SubmitMove(string trainingSessionId, string origin, string destination, string promotion = null)
        {
            EndgameTrainingSession session = endgameTrainingSessionRepository.Get(trainingSessionId);
            if (session == null)
            {
                return Json(new { success = false, error = "Training session ID not found." });
            }

            Piece promotionPiece = null;
            if (promotion != null)
            {
                promotionPiece = Utilities.GetPromotionPieceFromName(promotion, session.Game.WhoseTurn);
            }
            Move move = new Move(origin, destination, session.Game.WhoseTurn, promotionPiece);
            SubmittedMoveResponse response = session.SubmitMove(move);
            dynamic jsonResp = new ExpandoObject();
            jsonResp.success = response.Success;
            jsonResp.correct = response.Correct;
            jsonResp.check = response.Check;
            if (response.Error != null) jsonResp.error = response.Error;
            if (response.FEN != null) jsonResp.fen = response.FEN;
            if (response.FenAfterPlay != null)
            {
                jsonResp.fenAfterPlay = response.FenAfterPlay;
                jsonResp.checkAfterAutoMove = response.CheckAfterAutoMove;
            }
            if (response.Moves != null) jsonResp.dests = moveCollectionTransformer.GetChessgroundDestsForMoveCollection(response.Moves);
            if (response.LastMove != null) jsonResp.lastMove = response.LastMove;
            jsonResp.drawAfterAutoMove = response.DrawAfterAutoMove;
            return Json(jsonResp);
        }
    }
}
