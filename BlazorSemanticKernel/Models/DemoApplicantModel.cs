using System;
using System.ComponentModel;
using System.Text.Json;
using Microsoft.SemanticKernel;

namespace BlazorSemanticKernel.Models;


public class ApplicantManagementPlugin
{
    private List<ApplicantModel> _applicants {get; set;}

    public ApplicantManagementPlugin()
    {
        _applicants = new List<ApplicantModel>();
    }

    [KernelFunction, Description("Add a new applicant to the system")]
    public string AddApplicant(
        [Description("The applicant's full name")] string name,
        [Description("The applicant's email address")] string email,
        [Description("The position being applied for")] string position)
    {
        var applicant = new ApplicantModel
        {
            Name = name,
            Email = email,
            Position = position,
            ApplicationDate = DateTime.Now
        };
        
        _applicants.Add(applicant);
        return $"Added applicant {name} for position {position}";
    }

    [KernelFunction, Description("Get information about a specific applicant by name")]
    public string GetApplicantByName([Description("The full name of the applicant to find")] string name)
    {
        var applicant = _applicants.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return applicant != null 
            ? JsonSerializer.Serialize(applicant, new JsonSerializerOptions { WriteIndented = true })
            : "Applicant not found";
    }

    [KernelFunction, Description("Get all applicants for a specific position")]
    public string GetApplicantsByPosition([Description("The position to search for")] string position)
    {
        var applicants = _applicants.Where(a => a.Position.Equals(position, StringComparison.OrdinalIgnoreCase)).ToList();
        return applicants.Any()
            ? JsonSerializer.Serialize(applicants, new JsonSerializerOptions { WriteIndented = true })
            : "No applicants found for this position";
    }

/// <summary>
    //Pretend this is a database/api call that actually returns the applicants
/// </summary>
/// <returns></returns>
    [KernelFunction("get_all_applicants"), Description("Get an updated list of applicant models containing information about all applicants")]
    public List<ApplicantModel> GetAllApplicants()
    {
        _applicants = new List<ApplicantModel>
        {
            new() {
                Name = "John Smith",
                Email = "john.smith@email.com",
                Position = "Software Engineer",
                ApplicationDate = DateTime.Now.AddDays(-30),
                AdditionalData = new() {
                    { "YearsExperience", 5 },
                    { "ProgrammingLanguages", new[] { "C#", "Python", "JavaScript" } },
                    { "Location", "Remote" },
                    { "SalaryExpectation", 120000 },
                    { "InterviewStage", "Technical Interview" }
                }
            },
            new() {
                Name = "Sarah Johnson",
                Email = "sarah.j@email.com",
                Position = "UX Designer",
                ApplicationDate = DateTime.Now.AddDays(-15),
                AdditionalData = new() {
                    { "YearsExperience", 3 },
                    { "Skills", new[] { "Figma", "Adobe XD", "User Research" } },
                    { "Location", "New York" },
                    { "SalaryExpectation", 95000 },
                    { "InterviewStage", "Portfolio Review" }
                }
            },
            new() {
                Name = "Michael Chen",
                Email = "m.chen@email.com",
                Position = "Software Engineer",
                ApplicationDate = DateTime.Now.AddDays(-45),
                AdditionalData = new() {
                    { "YearsExperience", 8 },
                    { "ProgrammingLanguages", new[] { "Java", "Kotlin", "C#" } },
                    { "Location", "San Francisco" },
                    { "SalaryExpectation", 150000 },
                    { "InterviewStage", "Final Round" }
                }
            },
            new() {
                Name = "Emily Brown",
                Email = "emily.b@email.com",
                Position = "Product Manager",
                ApplicationDate = DateTime.Now.AddDays(-7),
                AdditionalData = new() {
                    { "YearsExperience", 6 },
                    { "Skills", new[] { "Agile", "Roadmapping", "Stakeholder Management" } },
                    { "Location", "Chicago" },
                    { "SalaryExpectation", 130000 },
                    { "InterviewStage", "Initial Screening" }
                }
            }
        };
        return _applicants;
    }
}

public class ApplicantModel
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Position { get; set; } = "";
    public DateTime ApplicationDate { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}
