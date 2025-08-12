using System.Text;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using boat_share.Services;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using boat_share.Api.Core.BackgroundReservation;
using NLog;
using NLog.Web;
using boat_share.UseCases;


namespace BoatShare
{
	public class Program
    {
        public static void Main(string[] args)
        {
            var logFactory = LogManager.Setup().LoadConfigurationFromAppSettings().LogFactory;
            var logger = logFactory.GetCurrentClassLogger();
            try
            {
                logger.Info("Starting application...");
                CreateHostBuilder(args);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Application stopped due to an exception.");
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        public static void CreateHostBuilder(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Register S3Service
            builder.Services.AddSingleton<S3Service>(sp =>
            {
                var s3Client = sp.GetRequiredService<IAmazonS3>();
                var bucketName = builder.Configuration["AWS:S3:BucketName"];
                return new S3Service(s3Client, bucketName);
            });

            // Register services here
            ConfigureServices(builder.Services, builder.Configuration);

			// Configure middleware pipeline and build the app
			var app = builder.Build();

            // Add Swagger to the middleware pipeline
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Boat Share API v1");
            });

            app.UseRouting();

            // Enable CORS
            app.UseCors("AllowAll");

            // Enable Authentication and Authorization
            app.UseAuthentication();  // Make sure to add authentication middleware here
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.Run();
        }
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });

			services.AddControllers();

            // Register services here
            services.AddSingleton<BoatService>();
            services.AddSingleton<UserService>();
            services.AddSingleton<ReservationService>();
			services.AddSingleton<AuthService>();
            services.AddSingleton<ReservationDataService>();
            services.AddSingleton<NextRunService>();
            services.AddHostedService<ReservationBackgroundService>();

            services.AddSingleton<DeleteAllPastReservationsUseCase>();
            services.AddSingleton<DeleteReservationUseCase>();


            /*
            var awsOptions = configuration.GetAWSOptions();
            services.AddDefaultAWSOptions(awsOptions);

            // Register DynamoDB and S3 clients
            services.AddSingleton<IDynamoDBContext, DynamoDBContext>(serviceProvider =>
            {
                var amazonDynamoDBClient = serviceProvider.GetRequiredService<IAmazonDynamoDB>();
                var dynamoDBContextConfig = new DynamoDBContextConfig();
                return new DynamoDBContext(amazonDynamoDBClient, dynamoDBContextConfig);
            });
            services.AddAWSService<IAmazonDynamoDB>();
            */

            // Register DynamoDB and S3 clients
            services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();
            services.AddSingleton<IDynamoDBContext, DynamoDBContext>();
            services.AddSingleton<IAmazonS3, AmazonS3Client>();
            services.AddAWSService<IAmazonS3>();

            // JWT Authentication Configuration
            var key = Encoding.ASCII.GetBytes(configuration["Jwt:Key"]);
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            // Add Swagger and include JWT in Swagger UI
            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

			services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);
		}

    }
}
