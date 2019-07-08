using ChessVariantsTraining.Configuration;
using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.DbRepositories.Variant960;
using ChessVariantsTraining.Extensions;
using ChessVariantsTraining.MemoryRepositories;
using ChessVariantsTraining.MemoryRepositories.Variant960;
using ChessVariantsTraining.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChessVariantsTraining
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("config.json")
                .AddJsonFile("config-secret.json");

            Configuration = builder.Build();
        }

        IConfigurationRoot Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            // Configuration
            services.AddOptions();
            services.Configure<Settings>(Configuration);
            int assetVersion = Configuration.GetValue<int>("AssetVersion");
            if (!UrlHelperExtensions.SetAssetVersionIfUnset(assetVersion))
            {
                throw new System.Exception("Asset value already set, which must not happen.");
            }

            // Database repositories
            services.AddSingleton<IAttemptRepository, AttemptRepository>();
            services.AddSingleton<ICommentRepository, CommentRepository>();
            services.AddSingleton<ICommentVoteRepository, CommentVoteRepository>();
            services.AddSingleton<ICounterRepository, CounterRepository>();
            services.AddSingleton<IGameRepository, GameRepository>();
            services.AddSingleton<INotificationRepository, NotificationRepository>();
            services.AddSingleton<IPositionRepository, PositionRepository>();
            services.AddSingleton<IPuzzleRepository, PuzzleRepository>();
            services.AddSingleton<IRatingRepository, RatingRepository>();
            services.AddSingleton<IReportRepository, ReportRepository>();
            services.AddSingleton<ISavedLoginRepository, SavedLoginRepository>();
            services.AddSingleton<ITimedTrainingScoreRepository, TimedTrainingScoreRepository>();
            services.AddSingleton<IUserRepository, UserRepository>();
            services.AddSingleton<ITagRepository, TagRepository>();

            // Memory repositories
            services.AddSingleton<IEndgameTrainingSessionRepository, EndgameTrainingSessionRepository>();
            services.AddSingleton<IGameRepoForSocketHandlers, GameRepoForSocketHandlers>();
            services.AddSingleton<IGameSocketHandlerRepository, GameSocketHandlerRepository>();
            services.AddSingleton<ILobbySeekRepository, LobbySeekRepository>();
            services.AddSingleton<ILobbySocketHandlerRepository, LobbySocketHandlerRepository>();
            services.AddSingleton<IPuzzlesBeingEditedRepository, PuzzlesBeingEditedRepository>();
            services.AddSingleton<IPuzzleTrainingSessionRepository, PuzzleTrainingSessionRepository>();
            services.AddSingleton<ITimedTrainingSessionRepository, TimedTrainingSessionRepository>();

            // Miscellaneous services
            services.AddSingleton<IEmailSender, EmailSender>();
            services.AddSingleton<IGameConstructor, GameConstructor>();
            services.AddSingleton<IMoveCollectionTransformer, MoveCollectionTransformer>();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddSingleton<IPersistentLoginHandler, PersistentLoginHandler>();
            services.AddSingleton<IRandomProvider, RandomProvider>();
            services.AddSingleton<IRatingUpdater, RatingUpdater>();
            services.AddSingleton<IUserVerifier, UserVerifier>();
            services.AddSingleton<IValidator, Validator>();

            services.Configure<RouteOptions>(options =>
            {
                options.ConstraintMap.Add("supportedVariantOrMixed", typeof(SupportedVariantOrMixedRouteConstraint));
                options.ConstraintMap.Add("supportedVariant", typeof(SupportedVariantRouteConstraint));
            });
            services.AddSession();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSession();
            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = (context) =>
                {
                    context.Context.Response.Headers["Cache-Control"] = "max-age=86400";
                }
            });
            app.UseWebSockets();
            app.UseMvc();
        }
    }
}
