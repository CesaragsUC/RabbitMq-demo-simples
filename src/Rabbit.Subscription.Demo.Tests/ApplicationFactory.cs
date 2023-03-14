using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitAppContext;

namespace Rabbit.Subscription.Demo.Tests
{
    public class ApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class 
    {

        //Configurado para usar banco InMemory
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services => {

                services.AddDbContext<RabbitContext>(options => options.UseInMemoryDatabase("InMemoryDbForTesting"));

                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    using (var appContext =  scope.ServiceProvider.GetRequiredService<RabbitContext>())
                    {
                        try
                        {
                            appContext.Database.EnsureCreated();
                        }
                        catch (Exception ex)
                        {

                            throw ex;
                        }
                    }
                }
            });

            //se fosse o caso de usar um banco especifico somente para teste
            //podemos criar uma appsetting.Testing.json onde teria as info do banco de teste.
            builder.UseEnvironment("Testing");

            base.ConfigureWebHost(builder); 
        }
    }

}
