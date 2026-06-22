using System;
using System.Collections.Generic;

namespace SportAcademy.Application.DTOs.TraineeDtos;

public class TraineeExportDto
{
    // ── Personal ─────────────────────────────────────────────────────────
    public int Id { get; set; }
    public string TraineeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string SSN { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? SecondPhoneNumber { get; set; }
    public DateOnly BirthDate { get; set; }
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public string? GuardianName { get; set; }
    public string? ParentNumber { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }

    // ── System ───────────────────────────────────────────────────────────
    public DateTime JoinDate { get; set; }
    public string? BranchName { get; set; }
    public int FamilyId { get; set; }
    public int FamilyCode { get; set; }
    public string? NationalityCategoryName { get; set; }
    public bool IsSubscribed { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }

    // ── Academic – Sports (pipe-delimited for CSV) ───────────────────────
    public string SportNames { get; set; } = string.Empty;
    public string SkillLevels { get; set; } = string.Empty;

    // ── Academic – Latest enrollment ─────────────────────────────────────
    public DateOnly? LatestEnrollmentDate { get; set; }
    public DateOnly? LatestExpiryDate { get; set; }
    public int? LatestSessionAllowed { get; set; }
    public int? LatestSessionRemaining { get; set; }
    public string? LatestGroupName { get; set; }

    // ── Academic – Latest subscription ───────────────────────────────────
    public DateOnly? LatestSubscriptionStartDate { get; set; }
    public DateOnly? LatestSubscriptionEndDate { get; set; }
    public string? LatestSubscriptionType { get; set; }
    public string? LatestSubscriptionStatus { get; set; }
}
