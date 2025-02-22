using Azure.Core;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tamagotchi.Backend.SharedLibrary.Security;
using Tamagotchi.Backend.SharedLibrary.Utilities;
using Tamagotchi.Backend.Users.Api.Dtos;
using Tamagotchi.Backend.Users.Api.Entities;

namespace Tamagotchi.Backend.Users.Api.Utilities;

public class DtoMapper : IDtoMapper
{
    private readonly IPasswordHasher _passwordHasher;

    public DtoMapper(IPasswordHasher passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    public UserEntity MapUserRegistrationRequestToUserEntity(UserRegistrationRequestDto request)
    {
        return new UserEntity
        {
            UserId = IdGenerator.GenerateShortId(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        };
    }

    public UserRegistrationResponseDto MapUserEntityToUserRegistrationResponse(UserEntity entity)
    {
        return new UserRegistrationResponseDto()
        {
            UserId = entity.UserId,
            Username = entity.Username,
            CreatedAt = DateTime.Now,
        };
    }

    public UserEntity CombineUserUpdateRequsestWithUserEntity(UserUpdateRequestDto request, UserEntity entity)
    {
        entity.Email = request.Email;
        entity.FirstName = request.FirstName;
        entity.LastName = request.LastName;
        entity.UpdatedAt = DateTime.Now;
        return entity;

    }

    public UserUpdateResponseDto MapUserEntityToUserUpdateResonse(UserEntity entity)
    {
        return new UserUpdateResponseDto()
        {
            Email = entity.Email,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            UpdatedAt = entity.UpdatedAt,
        };
    }

    public UserResponseDto MapUserEntityToUserResponse(UserEntity entity)
    {
        return new UserResponseDto()
        {
            UserId = entity.UserId,
            Username = entity.Username,
            Email = entity.Email,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            UpdatedAt = entity.UpdatedAt,
            CreatedAt = entity.CreatedAt,
        };
    }

    public List<UserResponseDto> MapUserEntitiesToListUserResponse(IEnumerable<UserEntity> entities)
    {
        return entities.Select(MapUserEntityToUserResponse).ToList();
    }
}
