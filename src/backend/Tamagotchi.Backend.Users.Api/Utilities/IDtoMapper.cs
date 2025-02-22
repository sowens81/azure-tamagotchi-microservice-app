using Microsoft.Azure.Cosmos;
using Tamagotchi.Backend.Users.Api.Dtos;
using Tamagotchi.Backend.Users.Api.Entities;

namespace Tamagotchi.Backend.Users.Api.Utilities;

public interface IDtoMapper
{
    UserEntity MapUserRegistrationRequestToUserEntity(UserRegistrationRequestDto request);

    UserRegistrationResponseDto MapUserEntityToUserRegistrationResponse(UserEntity entity);

    UserEntity CombineUserUpdateRequsestWithUserEntity(UserUpdateRequestDto request, UserEntity entity);

    UserUpdateResponseDto MapUserEntityToUserUpdateResonse(UserEntity entity);

    UserResponseDto MapUserEntityToUserResponse(UserEntity entity);

    List<UserResponseDto> MapUserEntitiesToListUserResponse(IEnumerable<UserEntity> entities);

}
