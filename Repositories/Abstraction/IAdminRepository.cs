﻿using Louman.Models.DTOs;
using Louman.Models.DTOs.Timer;
using Louman.Models.DTOs.User;
using Louman.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Louman.Repositories.Abstraction
{
    public interface IAdminRepository
    {
        
        AdminDto Add(UpsertAdminDto admin);
        bool Delete(int Id);
        List<AdminDto> GetAll();
        AdminDto GetById(int id);
        List<AdminDto> SearchByName(string name);
        TimerConfigEntity GetTimerCongif();

        bool SetTimerConfig(TimerConfigDto config);
        Task<List<RoleDto>> GetAllRoles();
        Task<RoleDto> AddRole(RoleDto role);
        Task<RoleDto> GetRoleById(int id);

        Task<List<FeatureDto>> GetAllFeatures();
        Task<FeatureDto> AddFeature(FeatureDto feature);
        Task<FeatureDto> GetFeatureById(int id);

    }
}
