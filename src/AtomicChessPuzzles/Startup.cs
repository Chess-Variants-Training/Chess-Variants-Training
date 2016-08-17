using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.DependencyInjection;
using AtomicChessPuzzles.Configuration;
using AtomicChessPuzzles.DbRepositories;
using AtomicChessPuzzles.MemoryRepositories;
using AtomicChessPuzzles.Services;

namespace AtomicChessPuzzles
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            // Configuration
            services.AddSingleton<ISettings, Settings>();

            // Database repositories
            services.AddSingleton<IAttemptRepository, AttemptRepository>();
            services.AddSingleton<ICommentRepository, CommentRepository>();
            services.AddSingleton<ICommentVoteRepository, CommentVoteRepository>();
            services.AddSingleton<IPositionRepository, PositionRepository>();
            services.AddSingleton<IPuzzleRepository, PuzzleRepository>();
            services.AddSingleton<IRatingRepository, RatingRepository>();
            services.AddSingleton<IReportRepository, ReportRepository>();
            services.AddSingleton<ITimedTrainingScoreRepository, TimedTrainingScoreRepository>();
            services.AddSingleton<IUserRepository, UserRepository>();

            // Memory repositories
            services.AddSingleton<IEndgameTrainingSessionRepository, EndgameTrainingSessionRepository>();
            services.AddSingleton<IPuzzlesBeingEditedRepository, PuzzlesBeingEditedRepository>();
            services.AddSingleton<IPuzzleTrainingSessionRepository, PuzzleTrainingSessionRepository>();
            services.AddSingleton<ITimedTrainingSessionRepository, TimedTrainingSessionRepository>();

            // Miscellaneous services
            services.AddSingleton<IMoveCollectionTransformer, MoveCollectionTransformer>();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddSingleton<IRatingUpdater, RatingUpdater>();
            services.AddSingleton<IValidator, Validator>();

            services.AddCaching();
            services.AddSession();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseIISPlatformHandler();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSession();
            app.UseStaticFiles();
            app.UseMvc();
        }

        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
