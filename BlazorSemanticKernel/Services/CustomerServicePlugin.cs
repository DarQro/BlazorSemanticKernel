using System.ComponentModel;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;
public class CustomerServicePlugin
{
    // Simulated user data
    private static Dictionary<string, CustomerInfo> Customers = new()
    {
        {"12345", new CustomerInfo
        {
            PolicyNumber = "12345",
            Name = "John Doe",
            Address = "123 Old Street, Old City, OC 12345",
            PolicyType = "Life Insurance",
            CoverageAmount = 500000,
            Beneficiary = "Jane Doe",
            LastInteraction = DateTime.Now.AddMonths(-6),
            PendingClaims = new List<string> { "Claim123: Hospitalization" }
        }}
    };

    [KernelFunction, Description("Get customer information")]
    public string GetCustomerInfo(string policyNumber)
    {
        if (Customers.TryGetValue(policyNumber, out var customer))
        {
            return JsonConvert.SerializeObject(customer);
        }
        return "Customer not found";
    }

    [KernelFunction, Description("Update customer address")]
    public string UpdateAddress(string policyNumber, string newAddress)
    {
        if (Customers.TryGetValue(policyNumber, out var customer))
        {
            customer.Address = newAddress;
            return $"Address updated successfully for policy {policyNumber}";
        }
        return "Customer not found";
    }

    [KernelFunction, Description("Get policy details")]
    public string GetPolicyDetails(string policyNumber)
    {
        if (Customers.TryGetValue(policyNumber, out var customer))
        {
            return $"Policy Type: {customer.PolicyType}, Coverage: ${customer.CoverageAmount}";
        }
        return "Policy not found";
    }

    [KernelFunction, Description("Check claim status")]
    public string CheckClaimStatus(string policyNumber)
    {
        if (Customers.TryGetValue(policyNumber, out var customer))
        {
            return customer.PendingClaims.Count > 0 
                ? $"Pending claims: {string.Join(", ", customer.PendingClaims)}" 
                : "No pending claims";
        }
        return "Policy not found";
    }

    [KernelFunction, Description("Suggest policy review")]
    public string SuggestPolicyReview(string policyNumber)
    {
        if (Customers.TryGetValue(policyNumber, out var customer))
        {
            return DateTime.Now - customer.LastInteraction > TimeSpan.FromDays(365)
                ? "It's been over a year since your last policy review. Would you like to schedule one?"
                : "Your policy is up to date.";
        }
        return "Policy not found";
    }
}

public class CustomerInfo
{
    public string PolicyNumber { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string PolicyType { get; set; }
    public int CoverageAmount { get; set; }
    public string Beneficiary { get; set; }
    public DateTime LastInteraction { get; set; }
    public List<string> PendingClaims { get; set; }
}