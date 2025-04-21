using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kriefTrackAiApi.Core.Models;
using kriefTrackAiApi.Common.Dto;

namespace kriefTrackAiApi.Core.Interfaces;

public interface ICompanyRepository
{
  Task<List<Company>> GetAllAsync();
  Task<Company?> GetByIdAsync(Guid id);
  Task<Company> AddAsync(Company company);
  Task<Company?> UpdateAsync(Guid id, Company company);
  Task<bool> DeleteAsync(Guid id);
  Task<List<CustomerDto>> GetActiveCustomersByIdsAsync(List<Guid> companyIds);
  Task<Company?> GetByCustomerNumberAsync(int customerNumber);
  Task<List<Company>> GetCompaniesByIdsAsync(List<Guid> companyIds);

}
