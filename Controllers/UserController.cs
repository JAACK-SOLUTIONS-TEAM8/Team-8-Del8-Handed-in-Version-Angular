﻿using Louman.Models.DTOs.User;
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
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }


        [HttpPost("Add")]
        public async Task<IActionResult> AddNewUserType(UserTypeDto userType)
        {
            var newUserType = await _userRepository.AddUserType(userType);
            if (newUserType != null)
                return Ok(new { UserType = newUserType, StatusCode = StatusCodes.Status200OK });
            return NotFound(new { UserType = newUserType, StatusCode = StatusCodes.Status404NotFound });

        }

        [HttpGet("UserTypes/All")]
        public async Task<IActionResult> GetAllUserTypes()
        {
            var userTypes = await _userRepository.GetAllUserTypes();
            if (userTypes != null)
                return Ok(new { UserTypes = userTypes, StatusCode = StatusCodes.Status200OK });
            return NotFound(new { UserTypes = userTypes, StatusCode = StatusCodes.Status404NotFound });

        }

        [HttpGet("UserTypes/{id}")]
        public async Task<IActionResult> GetProdutTypeById([FromRoute] int id)
        {
            var userType = await _userRepository.GetUserTypeById(id);
            if (userType != null)
                return Ok(new { UserType = userType, StatusCode = StatusCodes.Status200OK });
            return NotFound(new { UserType = userType, StatusCode = StatusCodes.Status404NotFound });

        }

        [HttpGet("Delete")]
        public async Task<IActionResult> DeleteUserType([FromRoute] int id)
        {
            var response = await _userRepository.DeleteUserType(id);
            if (response != true)
                return Ok(new { Response = true, StatusCode = StatusCodes.Status200OK });
            return NotFound(new { Response = false, StatusCode = StatusCodes.Status404NotFound });

        }
        //add audit method
        [HttpGet("Audit")]
        public async Task<IActionResult> GetAudits()
        {
            var audits = await _userRepository.GetAuditDetail();
            if (audits != null)
                return Ok(new { Audits = audits, StatusCode = StatusCodes.Status200OK });
            return NotFound(new { Audits = audits, StatusCode = StatusCodes.Status404NotFound });

        }

        [HttpGet("Audit/Search")]
        public async Task<IActionResult> SearchAudit([FromQuery] string name)
        {
            var audits = await _userRepository.SearchAuditByUserName(name);
            if (audits != null)
                return Ok(new { Audits = audits, StatusCode = StatusCodes.Status200OK });
            return NotFound(new { Audits = audits, StatusCode = StatusCodes.Status404NotFound });

        }

        [HttpGet("Roles/{id}")]
        public async Task<IActionResult> GetUserRoles([FromRoute] int id)
        {
            var reponse = await _userRepository.GetUserRole(id);
            if (reponse != null)
                return Ok(new { Roles = reponse, statusCode = StatusCodes.Status200OK });
            else
                return Ok(new { Roles = reponse, statusCode = StatusCodes.Status400BadRequest });

        }

        [HttpPost("Roles/Add")]
        public async Task<IActionResult> AddUserRoles([FromBody] AddRoleDto roleData)
        {
            var reponse = await _userRepository.AddUserRole(roleData);
            if (reponse == true)
                return Ok(new { Roles = reponse, statusCode = StatusCodes.Status200OK });
            else
                return Ok(new { Roles = reponse, statusCode = StatusCodes.Status400BadRequest });

        }
        
        [HttpGet("Features/{id}")]
        public async Task<IActionResult> GetRoleFeatures([FromRoute] int id)
        {
            var reponse = await _userRepository.GetRoleFeatures(id);
            if (reponse != null)
                return Ok(new { Features = reponse, statusCode = StatusCodes.Status200OK });
            else
                return Ok(new { Features = reponse, statusCode = StatusCodes.Status400BadRequest });

        }

        [HttpPost("Features/Add")]
        public async Task<IActionResult> AddRoleFeatures([FromBody] AddFeatureDto featureData)
        {
            var reponse = await _userRepository.AddRoleFeature(featureData);
            if (reponse == true)
                return Ok(new { Features = reponse, statusCode = StatusCodes.Status200OK });
            else
                return Ok(new { Features = reponse, statusCode = StatusCodes.Status400BadRequest });

        }


    }
}
