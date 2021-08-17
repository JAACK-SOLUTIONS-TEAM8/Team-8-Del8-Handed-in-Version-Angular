﻿using Louman.Models.DTOs.Team;
using Louman.Repositories.Abstraction;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Louman.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamController : ControllerBase
    {
        private readonly ITeamRepository _teamRepository;

        public TeamController(ITeamRepository teamRepository)
        {
           _teamRepository = teamRepository;
        }


        [HttpPost("Add")]
        public async Task<IActionResult> AddNewTeam(TeamDto team)
        {
            var newTeam = await _teamRepository.AddAsync(team);
            if (newTeam != null)
                return Ok(new { Team = newTeam, StatusCode = StatusCodes.Status200OK });
            return NotFound(new { Team = newTeam, StatusCode = StatusCodes.Status404NotFound });

        }

        [HttpGet("WeekDays")]
        public async Task<IActionResult> GetWeekDays()
        {
            var days = await _teamRepository.GetWeekDays();
            if (days != null)
                return Ok(new { Days = days, StatusCode = StatusCodes.Status200OK });
            return NotFound(new { Days = days, StatusCode = StatusCodes.Status404NotFound });

        }


    }
}
