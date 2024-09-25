using System.Collections.ObjectModel;

namespace LiL.TimeTracking.Models;

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateOnly StartDate { get; set; }
    //Virtual members define related entities which Entity Framework can use to create database table keys and relationships.
    public virtual ICollection<Project> Projects { get; set; }
}