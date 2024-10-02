using LiL.TimeTracking.Auth;
using LiL.TimeTracking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//adding a service that is available now to that infrastructure and it's a singleton - we have one instance of our emaildomainhandler that is going to get created to handle authorization requests.
builder.Services.AddSingleton<IAuthorizationHandler, EmailDomainHandler>();

// we have added a policy. we can add any number of policies and each policy will have a requirement that goes along with it. so in order for the policy to succeed, the requirements also need to succeed.
// not only do you need to authenticate the user but you also need to apply this particular policy made up of requirements to validate the user. (add policy in controller)
builder.Services.AddAuthorization(options => options.AddPolicy("EmailDomain", policy => policy.AddRequirements(new EmailDomainRequirement("hplusport.com"))));
builder.Services.AddAuthentication().AddScheme<APIKeyOptions, APIKeyAuthHandler>("APIKEY", o => o.DisplayMessage = "API Key Authenticator");
builder.Services.AddDbContext<TimeTrackingDbContext>(options => 
options.UseSqlite(builder.Configuration.GetConnectionString("TrackingDbContext")));
// below line of code sets up the identity endpoints and ensures that the identity data is stored in the specified database context.
builder.Services.AddIdentityApiEndpoints<IdentityUser>().AddEntityFrameworkStores<TimeTrackingDbContext>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

//below statement configures an Identity API group with routes related to Identity, prefixed by /Identity. It would allow operations like user registration, authentication, password reset, etc., using IdentityUser as the user model.
//e.g. /identity/login
app.MapGroup("Identity").MapIdentityApi<IdentityUser>();
app.Run();
