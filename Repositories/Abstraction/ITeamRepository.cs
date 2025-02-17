﻿using Louman.Models.DTOs.Employee;
using Louman.Models.DTOs.Team;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Louman.Repositories.Abstraction
{
    public interface ITeamRepository
    {
        Task<TeamDto> AddAsync(TeamDto team);
        Task<List<TeamDto>> GetAllAsync();
        bool CanMarkAttendance(int teamId);
        bool CheckTeamValidity(CheckTeamDto team);
        Task<List<AttendanceDto>> GetAttendanceData(int teamId);
        Task<bool> MarkAttendance(List<AttendanceDto> attendances);
        Task<TeamDto> GetByIdAsync(int teamId);
        Task<List<EmployeeDto>> GetTeamEmployees(int teamId);
        Task<bool> RemoveAsync(int teamId);
        Task<List<TeamDto>> SearchByName(string name);
        Task<List<DayDto>> GetWeekDays();
        Task<bool> RemoveEmployeeFromTeam(int teamId, int employeeId);
        Task<List<TeamEmployeeDto>> AddTeamEmployee(TeamEmployeeDto employee);
        Task<List<AttendanceDto>> GetAttendanceDataForReport(int teamId, string date);






    }
}
