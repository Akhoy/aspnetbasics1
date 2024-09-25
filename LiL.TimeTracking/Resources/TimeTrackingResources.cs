namespace LiL.TimeTracking.Resources;

// for rest api
//gonna create a few records - representations for my resources as they go out to the service. this separates those representations from the database.
//init only properties
//defining records in the constructor model means they are immutable and cannot be changed once created. cuz I'm using this syntax where I'm providing this constructor type definition where I'm saying here are all the properties you need to define an instance of this and you can only set them at the time you create it.
public record Employee (int Id, string Name, DateOnly StartDate);

public record Project(int Id, string Name, DateTime StartDate, DateTime ?EndDate);

//definining time entry liek this because when we go to store this tiem entry, we dont have to load employee, project to get the id. to store in db, we only need employeeid and projectid. those are going to be my reference properties to get into that db
public record TimeEntry(Guid Id, int EmployeeId, int ProjectId, DateOnly DateWorked, decimal HoursWorked);

//this one is where you'll see why mapping from dbs to these resources makes sense because i'm going to have a project assignment resource type
//when you think about creating in db, a notion of connection between employee and project. i want to add emp to project, remove emp from proj. in resource based api we are building, we don't have operations like add emp to project or remove emp from project. if we did, we will be building an RPC type interface. we do not. we're using REST. project assignment is a resource that allows us to represent the conenction between employee and project. the ids are what we are going to be care about in db level but when we pull them out from db, it will be nice to see the employee name and the project name. so made them nullable meaning we wont require when we create it but we can populate them when we pull back from those project assignments.
public record ProjectAssignment(int EmployeeId, int ProjectId, string? EmployeeName, string? ProjectName);

// so now we've got resources that we can use in the API - that's what will be represented when we return these and what will be represented when requests come in. but we're going to have different models at db level cuz they need to track 2 specific tables in the backend that we don't want to force our front end to confirm to.

public record Resource(string Name, string Url);

/*Linked Resource Class:
    The idea is to create a class that wraps your data (like an employee or project) and includes links to related resources.
    This helps clients navigate from one resource to related items easily.
    Class Structure:

    Data Property: This holds the actual data (e.g., an employee or project).
    Links Property: This is a list of links to related resources.
    */

public record LinkedResource<T>{
    public LinkedResource(T resource)
    {
        Data = resource;
        Links = new List<Resource>();
    }
    public T Data{get;set;}
    public List<Resources.Resource> Links{get;set;}
}