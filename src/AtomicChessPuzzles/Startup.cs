using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.DependencyInjection;
using AtomicChessPuzzles.DbRepositories;
using AtomicChessPuzzles.MemoryRepositories;

namespace AtomicChessPuzzles
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSingleton<IUserRepository, UserRepository>();
            services.AddSingleton<IPuzzlesBeingEditedRepository, PuzzlesBeingEditedRepository>();
            services.AddSingleton<IPuzzleRepository, PuzzleRepository>();
            services.AddSingleton<IPuzzlesTrainingRepository, PuzzlesTrainingRepository>();
            services.AddSingleton<ICommentRepository, CommentRepository>();
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
