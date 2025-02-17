﻿using Louman.AppDbContexts;
using Louman.Models.DTOs;
using Louman.Models.DTOs.Admin;
using Louman.Models.DTOs.User;
using Louman.Models.Entities;
using Louman.Repositories.Abstraction;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Louman.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _dbContext;

        public UserRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<UserTypeDto> AddUserType(UserTypeDto userType)
        {
            if (userType.UserTypeId == 0)
            {
                var newUserType = new UserTypeEntity
                {
                   UserTypeDescription=userType.UserTypeDescription,
                    isDeleted = false
                };
                _dbContext.UserTypes.Add(newUserType);
                await _dbContext.SaveChangesAsync();


                return await Task.FromResult(new UserTypeDto
                {
                    UserTypeId=newUserType.UserTypeId,
                    UserTypeDescription=userType.UserTypeDescription
                });

            }
            else
            {

                var existingUserType = await(from ut in _dbContext.UserTypes where userType.UserTypeId == userType.UserTypeId && ut.isDeleted == false select ut).SingleOrDefaultAsync();
                if (existingUserType != null)
                {
                    existingUserType.UserTypeDescription = userType.UserTypeDescription;
                    _dbContext.Update(existingUserType);
                    await _dbContext.SaveChangesAsync();

                    return await Task.FromResult(new UserTypeDto
                    {
                        UserTypeId = existingUserType.UserTypeId,
                        UserTypeDescription = userType.UserTypeDescription
                    });
                }
            }
            return new UserTypeDto();

        }

        public async Task<List<UserTypeDto>> GetAllUserTypes()
        {
            return await (from ut in _dbContext.UserTypes
                          where ut.isDeleted == false
                          orderby ut.UserTypeDescription
                          select new UserTypeDto
                          {
                              UserTypeId = ut.UserTypeId,
                              UserTypeDescription = ut.UserTypeDescription
                          }).ToListAsync();
        }

        public async Task<UserTypeDto> GetUserTypeById(int userTypeId)
        {
            return await (from ut in _dbContext.UserTypes
                          where ut.isDeleted == false && ut.UserTypeId == userTypeId
                          orderby ut.UserTypeDescription
                          select new UserTypeDto
                          {
                              UserTypeId = ut.UserTypeId,
                              UserTypeDescription = ut.UserTypeDescription
                          }).SingleOrDefaultAsync();
        }

        public async Task<bool> DeleteUserType(int userTypeId)
        {
            var userType = _dbContext.UserTypes.Find(userTypeId);
            if (userType != null)
            {
                userType.isDeleted = true;
                _dbContext.UserTypes.Update(userType);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<List<AuditDto>> GetAuditDetail()
        {
            return await (from at in _dbContext.Audits
                          join u in _dbContext.Users on at.UserId equals u.UserId
                          orderby at.Date descending
                          select new AuditDto
                          {
                              AuditId = at.AuditId,
                              Date = at.Date,
                              Operation = at.Operation,
                              UserId = at.UserId,
                              UserName = $"{u.Name} {u.Surname}",
                          }).ToListAsync();
        }

        public async Task<List<AuditDto>> SearchAuditByUserName(string name)
        {
            return await(from at in _dbContext.Audits
                         join u in _dbContext.Users on at.UserId equals u.UserId
                         where u.Name.StartsWith(name) ||u.Surname.StartsWith(name)|| u.Name.Contains(name) || u.Surname.Contains(name)
                         orderby at.Date descending
                         select new AuditDto
                         {
                             AuditId = at.AuditId,
                             Date = at.Date,
                             Operation = at.Operation,
                             UserId = at.UserId,
                             UserName = $"{u.Name} {u.Surname}",
                         }).ToListAsync();
        }

        public async Task<List<UserRoleDto>> GetUserRole(int userId)
        {
            return await (from ur in _dbContext.UserRoles
                          where ur.UserId == userId
                          select new UserRoleDto
                          {
                              UserRoleId = ur.UserRoleId,
                              UserId = ur.UserId,
                              RoleId = ur.RoleId,
                              isActive=ur.isActive
                          }
                ).ToListAsync();
        }

        public async Task<bool> AddUserRole(AddRoleDto roleData)
        {
            var userRoleEntity =await (from ur in _dbContext.UserRoles
                                 where ur.UserId == roleData.UserId && ur.RoleId == roleData.RoleId
                                 select ur).SingleOrDefaultAsync();
            if (userRoleEntity != null)
            {
                userRoleEntity.isActive = roleData.isActive;
                _dbContext.UserRoles.Update(userRoleEntity);
               await _dbContext.SaveChangesAsync();
                return true;
            }
            else
            {
                var newUserRole = new UserRoleEntity
                {
                    RoleId = roleData.RoleId,
                    UserId = roleData.UserId,
                    isActive = roleData.isActive
                };

                _dbContext.UserRoles.Add(newUserRole);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }
        public async Task<List<RoleFeatureDto>> GetRoleFeatures(int roleId)
        {
            return await (from rf in _dbContext.RoleFeatures
                          where rf.RoleId == roleId
                          select new RoleFeatureDto
                          {
                              RoleFeatureId=rf.RoleFeatureId,
                              FeatureId=rf.FeatureId,
                              RoleId = rf.RoleId,
                              isActive=rf.isActive
                          }
                ).ToListAsync();
        }

        public async Task<bool> AddRoleFeature(AddFeatureDto featureData)
        {
            var roleFeatureEntity =await (from rf in _dbContext.RoleFeatures
                                 where rf.RoleId == featureData.RoleId && rf.FeatureId == featureData.FeatureId
                                 select rf).SingleOrDefaultAsync();
            if (roleFeatureEntity != null)
            {
                roleFeatureEntity.isActive = featureData.isActive;
                _dbContext.RoleFeatures.Update(roleFeatureEntity);
               await _dbContext.SaveChangesAsync();
                return true;
            }
            else
            {
                var newRoleFeature = new RoleFeatureEntity
                {
                    RoleId = featureData.RoleId,
                    FeatureId = featureData.FeatureId,
                    isActive = featureData.isActive
                };

                _dbContext.RoleFeatures.Add(newRoleFeature);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
