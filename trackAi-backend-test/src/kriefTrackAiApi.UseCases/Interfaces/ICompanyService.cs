using kriefTrackAiApi.Common.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace kriefTrackAiApi.UseCases.Interfaces;

public interface ICompanyService
{
    Task<List<CompanyDto>> GetAllAsync();
    Task<CompanyDto> GetByIdAsync(Guid id);
    Task<CompanyDto> AddAsync(CompanyDto companyDto);
    Task<CompanyDto> UpdateAsync(CompanyDto companyDto);
    Task<bool> DeleteAsync(Guid id);
    Task<List<CompanyWithUsersDto>> GetCompaniesWithUsersAsync();

}
