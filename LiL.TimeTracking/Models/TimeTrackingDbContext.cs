using Microsoft.EntityFrameworkCore;

namespace LiL.TimeTracking.Models;

// allow us to connect to db and store and retrieve our entities
public class TimeTrackingDbContext:DbContext{
    //allow us to navigate from this context and use context.Employees to get all employees from database

    //gives me constructors for this to be able to be initialized in my code and initialized by tooling code where i create migrations and scripts to create my db
    public TimeTrackingDbContext()
    {        
    }

    public TimeTrackingDbContext(DbContextOptions options) : base(options)
    {    
    }
    public DbSet<Employee> Employees{get;set;}
    public DbSet<Project> Projects{get;set;}
    public DbSet<TimeEntry> TimeEntries {get;set;}
}