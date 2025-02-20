using ControllRR.Application.Dto;

namespace ControllRR.Application.Interfaces;

public interface IUserService
{
    Task<List<ApplicationUserDto>> FindAllAsync();
    Task<ApplicationUserDto> FindByIdAsync(int id);
    Task InsertAsync(ApplicationUserDto user);
    Task RemoveAsync(int id);
    Task UpdateAsync(ApplicationUserDto userDto);


}