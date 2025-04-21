using AutoMapper;
using kriefTrackAiApi.Common.Dto;
using kriefTrackAiApi.Core.Models;
using kriefTrackAiApi.Core.Interfaces;
using kriefTrackAiApi.UseCases.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace kriefTrackAiApi.UseCases.Services;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _repository;
    private readonly IUserRepository _userRepository;

    private readonly IMapper _mapper;

    public CompanyService(ICompanyRepository repository, IUserRepository userRepository, IMapper mapper)
    {
        _repository = repository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<List<CompanyDto>> GetAllAsync()
    {
        var companies = await _repository.GetAllAsync();
        return _mapper.Map<List<CompanyDto>>(companies);
    }

    public async Task<CompanyDto> GetByIdAsync(Guid id)
    {
        var company = await _repository.GetByIdAsync(id);
        return _mapper.Map<CompanyDto>(company);
    }

    public async Task<CompanyDto> AddAsync(CompanyDto companyDto)
    {
        var existingCompany = await _repository.GetByCustomerNumberAsync(companyDto.CustomerNumber);
        if (existingCompany != null)
        {
            throw new InvalidOperationException($"Company with CustomerNumber {companyDto.CustomerNumber} already exists.");
        }

        var company = _mapper.Map<Company>(companyDto);
        await _repository.AddAsync(company);
        return _mapper.Map<CompanyDto>(company);
    }

    public async Task<CompanyDto> UpdateAsync(CompanyDto companyDto)
    {
        var company = _mapper.Map<Company>(companyDto);
        var updatedCompany = await _repository.UpdateAsync(company.Id, company);
        return _mapper.Map<CompanyDto>(updatedCompany);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _repository.DeleteAsync(id);
    }
    public async Task<List<CompanyWithUsersDto>> GetCompaniesWithUsersAsync()
    {
        var companies = await _repository.GetAllAsync();
        var users = await _userRepository.GetAllAsync();

        var result = companies.Select(company =>
        {
            var mappedCompany = _mapper.Map<CompanyWithUsersDto>(company);

            var relevantUsers = users
                .Where(u => u.CompanyIds.Contains(company.Id))
                .ToList();

            mappedCompany.Users = _mapper.Map<List<UserDto>>(relevantUsers);

            return mappedCompany;
        }).ToList();

        return result;
    }
}
